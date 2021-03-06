namespace CodeGenerator.Generate;

public class RestApiGenerate : GenerateBase
{
    /// <summary>
    /// 实体文件路径
    /// </summary>
    public string EntityPath { get; }
    /// <summary>
    /// DTO 所有项目目录路径
    /// </summary>
    public string SharePath { get; set; }
    /// <summary>
    /// service项目目录路径
    /// </summary>
    public string StorePath { get; }
    /// <summary>
    /// api项目目录路径
    /// </summary>
    public string ApiPath { get; }

    public string? ContextName { get; set; }
    public string? EntityNamespace { get; set; }
    /// <summary>
    /// DataStore 项目的命名空间
    /// </summary>
    public string? ShareNamespace { get; set; }
    public string? ServiceNamespace { get; set; }
    public string? ApiNamespace { get; set; }
    public readonly EntityInfo EntityInfo;

    public RestApiGenerate(string entityPath, string dtoPath, string servicePath, string apiPath, string? contextName = null)
    {
        EntityPath = entityPath;
        SharePath = dtoPath;
        StorePath = servicePath;
        ApiPath = apiPath;
        ContextName = contextName;
        var entityDir =  new FileInfo(entityPath).Directory!;
        var entityProjectFile = AssemblyHelper.FindProjectFile(entityDir, entityDir.Root);
        if (entityProjectFile == null) throw new FileNotFoundException("project file not found!");

        EntityNamespace = AssemblyHelper.GetNamespaceName(entityProjectFile.Directory!);
        ShareNamespace = AssemblyHelper.GetNamespaceName(new DirectoryInfo(SharePath));
        ServiceNamespace = AssemblyHelper.GetNamespaceName(new DirectoryInfo(StorePath));
        ApiNamespace = AssemblyHelper.GetNamespaceName(new DirectoryInfo(ApiPath));

        var entityHelper = new EntityParseHelper(entityPath);
        EntityInfo = entityHelper.GetEntity();
    }

    public string GetRestApiInterface()
    {
        var content = GetTplContent("Interface.IRestApiBase.tpl");
        content = content.Replace(TplConst.NAMESPACE, ApiNamespace);
        return content;
    }

    /// <summary>
    /// Rest Api 实现类
    /// </summary>
    /// <param name="dbContextName">DbContext 名称</param>
    /// <returns></returns>
    public string GetRestApiBase()
    {
        var dbContextName = GetContextName();
        var content = GetTplContent("Implement.RestApiBase.tpl");
        content = content.Replace(TplConst.NAMESPACE, ApiNamespace)
            .Replace(TplConst.DBCONTEXT_NAME, dbContextName);
        return content;
    }

    public List<string> GetGlobalUsings()
    {
        return new List<string>
        {
            "global using Microsoft.Extensions.DependencyInjection;",
            "global using Microsoft.AspNetCore.Mvc;",
            "global using Microsoft.AspNetCore.Authorization;",
            "global using System.Text.Json.Serialization;",
            "global using Microsoft.EntityFrameworkCore;",
            $"global using {EntityNamespace}.Utils;",
            $"global using {EntityNamespace}.Models;",
            $"// global using {EntityNamespace}.Identity;",
            $"global using {ShareNamespace};",
            $"global using {ShareNamespace}.Models;",
            $"global using {ApiNamespace}.Controllers;",
            $"global using {ServiceNamespace};",
            $"global using {ServiceNamespace}.Interface;",
            $"global using {ServiceNamespace}.DataStore;"
        };
    }

    /// <summary>
    /// 生成控制器
    /// </summary>
    public string GetRestApiContent()
    {
        var entityName = Path.GetFileNameWithoutExtension(EntityPath);
        var tplContent = GetTplContent("Implement.RestApi.tpl");

        //var actionContent = GetAddApiContent();
        //actionContent += GetUpdateApiContent();
        var actionContent = "";

        tplContent = tplContent.Replace(TplConst.NAMESPACE, ApiNamespace)
            .Replace(TplConst.SHARE_NAMESPACE, ShareNamespace)
            .Replace(TplConst.ENTITY_NAME, entityName)
            .Replace(TplConst.COMMENT, EntityInfo?.Comment ?? "")
            .Replace(TplConst.ADDITION_ACTION, actionContent ?? "")
            .Replace(TplConst.ID_TYPE, Config.IdType);
        return tplContent;
    }

    /// <summary>
    /// 生成关联添加
    /// </summary>
    /// <returns></returns>
    public string? GetAddApiContent()
    {
        var entityName = EntityInfo.Name;
        var navigationProp = EntityInfo.GetNavigation();
        if (navigationProp == null) return null;
        var content = $@"
    /// <summary>
    /// 关联添加
    /// </summary>
    /// <param name=""id"">所属对象id</param>
    /// <param name=""list"">数组</param>
    /// <param name=""dependStore""></param>
    /// <returns></returns>
    [HttpPost(""{{id}}"")]
    public async Task<ActionResult<int>> AddInAsync([FromRoute] ${{IdType}} id, List<{entityName}UpdateDto> list, [FromServices] {navigationProp.Type}DataStore dependStore)
    {{
        var depend = await dependStore.FindAsync(id);
        if (depend == null) return NotFound(""depend not exist"");
        var newList = new List<{entityName}>();
        list.ForEach(item =>
        {{
            var newItem = new {entityName}()
            {{
                {navigationProp.Name} = depend
            }};
            newList.Add(newItem.Merge(item));
        }});
        return await _store.BatchAddAsync(newList);
    }}";

        return content;
    }
    // TODO:update api 
    public static string? GetUpdateApiContent()
    {
        return default;
    }

    /// <summary>
    /// 生成仓储的注入服务
    /// </summary>
    public void GenerateRepositoryServicesDI()
    {
        // 获取services中所有Repository仓储类
        var dir = new DirectoryInfo(Path.Combine(StorePath, "Repositories"));
        Console.WriteLine("搜索目录:" + dir.FullName);
        var files = dir.GetFiles("*Repository.cs", SearchOption.TopDirectoryOnly);
        var classes = files.Where(f => f.Name != "Repository.cs").ToList();
        Console.WriteLine("共找到" + classes.Count + "个仓储");
        var content = string.Join(string.Empty, classes.Select(c => "            services.AddScoped(typeof(" + Path.GetFileNameWithoutExtension(c.FullName) + "));\r\n").ToArray());
        // 替换模板文件并写入
        var tplContent = GetTplContent("RepositoryServiceExtensions.tpl");
        string replaceSign = "// {$TobeAddRepository}";
        tplContent = tplContent.Replace(replaceSign, content);
        File.WriteAllText(Path.Combine(ApiPath, "RepositoryServiceExtensions.cs"), tplContent);
        Console.WriteLine("create file:" + Path.Combine(ApiPath, "RepositoryServiceExtensions.cs") + "\r\n" + "写入仓储注册服务完成");
    }


    /// <summary>
    /// get user DbContext name
    /// </summary>
    /// <param name="contextName"></param>
    /// <returns></returns>
    public string GetContextName(string? contextName = null)
    {
        var name = "ContextBase";
        var assemblyName = AssemblyHelper.GetAssemblyName(new DirectoryInfo(StorePath));
        var cpl = new CompilationHelper(StorePath, assemblyName);
        var classes = cpl.GetAllClasses();
        if (classes != null)
        {
            // 获取所有继承 dbcontext的上下文
            var allDbContexts = CompilationHelper.GetClassNameByBaseType(classes, "IdentityDbContext");
            if (!allDbContexts.Any())
                allDbContexts = CompilationHelper.GetClassNameByBaseType(classes, "DbContext");

            if (allDbContexts.Any())
            {
                if (contextName == null)
                {
                    name = allDbContexts.FirstOrDefault()!.Name;
                }
                else if (allDbContexts.Any(c => c.Name.Equals(contextName)))
                {
                    name = contextName;
                }
            }
        }
        return name;
    }
}
