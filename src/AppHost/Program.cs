using Azure.Provisioning.Storage;
using Projects;

namespace TDS.AspireFunction.AppHost;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);

        builder.AddAzureContainerAppEnvironment("env");

        var storage = builder.AddAzureStorage("storage").RunAsEmulator()
                             .ConfigureInfrastructure(infrastructure =>
                             {
                                 var storageAccount = infrastructure.GetProvisionableResources().OfType<StorageAccount>().FirstOrDefault(r => r.BicepIdentifier == "storage")
                                                      ?? throw new InvalidOperationException("Could not find configured storage account with name 'storage'");

                                 // Ensure that public access to blobs is disabled
                                 storageAccount.AllowBlobPublicAccess = false;
                             });
        var blobs = storage.AddBlobs("blobs");
        var queues = storage.AddQueues("queues");

        builder.AddAzureFunctionsProject<Function>("functions")
               .WithReference(queues)
               .WithReference(blobs)
               .WaitFor(storage)
               .WithRoleAssignments(storage,
                   // Storage Account Contributor and Storage Blob Data Owner roles are required by the Azure Functions host
                   StorageBuiltInRole.StorageAccountContributor, StorageBuiltInRole.StorageBlobDataOwner,
                   // Queue Data Contributor role is required to send messages to the queue
                   StorageBuiltInRole.StorageQueueDataContributor)
               .WithHostStorage(storage);

        builder.Build().Run();
    }
}
