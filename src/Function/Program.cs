using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;
using TDS.AspireFunction.ServiceDefaults;

namespace TDS.AspireFunction.Function;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = FunctionsApplication.CreateBuilder(args);

        builder.AddServiceDefaults();

        builder.AddAzureQueueServiceClient("queues");
        builder.AddAzureBlobServiceClient("blobs");

        builder.ConfigureFunctionsWebApplication();

        await builder.Build()
                     .RunAsync()
                     .ConfigureAwait(false);
    }
}
