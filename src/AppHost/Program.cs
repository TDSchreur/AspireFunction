using Aspire.Hosting.Azure;
using Azure.Provisioning;
using Azure.Provisioning.Storage;
using Projects;

namespace TDS.AspireFunction.AppHost;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);

        var storage = builder.AddAzureStorage("storage").RunAsEmulator()
                             .ConfigureInfrastructure(infrastructure =>
                             {
                                 var storageAccount = infrastructure.GetProvisionableResources().OfType<StorageAccount>().FirstOrDefault(r => r.BicepIdentifier == "storage")
                                                      ?? throw new InvalidOperationException("Could not find configured storage account with name 'storage'");

                                 // Storage Account Contributor and Storage Blob Data Owner roles are required by the Azure Functions host
                                 var principalTypeParameter = new ProvisioningParameter(AzureBicepResource.KnownParameters.PrincipalType, typeof(string));
                                 var principalIdParameter = new ProvisioningParameter(AzureBicepResource.KnownParameters.PrincipalId, typeof(string));
                                 infrastructure.Add(storageAccount.CreateRoleAssignment(StorageBuiltInRole.StorageAccountContributor, principalTypeParameter, principalIdParameter));
                                 infrastructure.Add(storageAccount.CreateRoleAssignment(StorageBuiltInRole.StorageBlobDataOwner, principalTypeParameter, principalIdParameter));

                                 // Ensure that public access to blobs is disabled
                                 storageAccount.AllowBlobPublicAccess = false;
                             });
        var blobs = storage.AddBlobs("blobs");
        var queues = storage.AddQueues("queues");

        builder.AddAzureFunctionsProject<Function>("functions")
               .WithReference(queues)
               .WithReference(blobs)
               .WaitFor(storage)
               .WithHostStorage(storage);

        builder.Build().Run();
    }
}
