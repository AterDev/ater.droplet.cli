﻿namespace Droplet.CommandLine;

public class CommandRunner
{
    public CommandRunner()
    {
    }

    /// <summary>
    /// angular 代码生成
    /// </summary>
    /// <param name="url">swagger json地址</param>
    /// <param name="output">ng前端根目录</param>
    /// <returns></returns>
    public async Task GenerateNgAsync(string url = "", string output = "")
    {
        try
        {
            Console.WriteLine("🔵 Generating ts models and ng services...");
            var cmd = new NgCommand(url, output);
            await cmd.RunAsync();
        }
        catch (WebException webExp)
        {
            Console.WriteLine(webExp.Message);
            Console.WriteLine("Ensure you had input correct url!");
        }
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
            Console.WriteLine(exp.StackTrace);
        }
    }

    /// <summary>
    /// dto生成或更新
    /// </summary>
    /// <param name="entityPath"></param>
    public async Task GenerateDtoAsync(string entityPath, string output, bool force)
    {
        Console.WriteLine("🔵 Generating Dtos...");
        var cmd = new DtoCommand(entityPath, output);
        await cmd.RunAsync(force);
    }

    /// <summary>
    /// api项目代码生成
    /// </summary>
    /// <param name="path">实体文件路径</param>
    /// <param name="servicePath">service目录</param>
    /// <param name="apiPath">网站目录</param>
    /// <param name="dbContext"></param>
    public async Task GenerateApi(string path, string dtoPath = "",
            string servicePath = "", string apiPath = "", string dbContext = "")
    {
        Console.WriteLine("🔵 Generate dtos");
        var dtoCmd = new DtoCommand(path, dtoPath);
        await dtoCmd.RunAsync();
        Console.WriteLine("🔵 Generate store");
        var storeCmd = new StoreCommand(path, dtoPath, servicePath, dbContext);
        await storeCmd.RunAsync();

        Console.WriteLine("🔵 Generate rest api");
        var apiCmd = new ApiCommand(path,dtoPath, servicePath, apiPath,dbContext);
        await apiCmd.RunAsync();
    }

    /// <summary>
    /// 根据已生成的dto生成相应的前端表单页面
    /// </summary>
    /// <param name="dtoPath">service根目录</param>
    /// <param name="name">实体类名称</param>
    /// <param name="output">前端根目录</param>
    public void GenerateNgPages(string name, string dtoPath, string output = "")
    {
        var pageGen = new NgPageGenerate(name, dtoPath, output);
        pageGen.Build();
        Console.WriteLine("前端页面生成完成");
    }

    /// <summary>
    /// 全部生成
    /// </summary>
    /// <param name="entityFile"></param>
    /// <param name="servicePath"></param>
    /// <param name="webPath"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    public async Task GenerateAsync(string entityFile, string servicePath, string share, string webPath, string output)
    {
        Console.WriteLine("生成后台Api代码");
        GenerateApi(entityFile, servicePath, webPath);

        Console.WriteLine("请输入swagger json在地址,按回车确认");
        var url = Console.ReadLine();
        Console.WriteLine("生成angular客户端请求服务");
        await GenerateNgAsync(url, output);
        Console.WriteLine("生成angular在基础表单页面");
        var fileName = System.IO.Path.GetFileNameWithoutExtension(entityFile);
        GenerateNgPages(fileName, share, output);
        Console.WriteLine("全部执行完成，请在web项目中注入仓储服务 services.AddRepositories();");
    }
}


