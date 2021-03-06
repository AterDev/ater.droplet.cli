using System.CommandLine;
namespace Droplet.CommandLine;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        await ConfigCommand.InitConfigFileAsync();
        var root = new CommandBuilder().Build();
        return await root.InvokeAsync(args);
    }
}
