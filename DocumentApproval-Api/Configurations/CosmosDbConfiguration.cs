using DocumentApproval_Api.Entities;
using DocumentApproval_DataSource.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocumentApproval_Api.Configurations
{
    public static class CosmosDbConfiguration
    {
        public static void AddCosmosDbConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<CosmosDbSetting>(configuration.GetSection("CosmosSettings"));
        }

        public static void AddStorageAccountConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<StorageDbSetting>(configuration.GetSection("StorageAccountSettings"));
        }
    }
}
