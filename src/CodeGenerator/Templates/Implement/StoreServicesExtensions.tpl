using ${Namespace}.DataStore;
using ${Namespace}.Manager;

namespace ${Namespace}.Implement;

public static class StoreServicesExtensions
{
    public static void AddDataStore(this IServiceCollection services)
    {
        services.AddTransient<IUserContext, UserContext>();
        services.AddScoped(typeof(DataStoreContext));
${StoreServices}
    }

    public static void AddManager(this IServiceCollection services)
    {
${ManagerServices}
    }
}
