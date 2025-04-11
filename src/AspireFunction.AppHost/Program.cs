
namespace TDS.AzureFunction.AspireFunction.AppHost;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);

        builder.Build().Run();
    }
}
