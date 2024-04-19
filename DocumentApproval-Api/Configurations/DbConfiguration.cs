namespace DocumentApproval_Api.Configurations
{
    public static class DbConfiguration
    {
        public static void AddDbConfig(this IServiceCollection services, IConfiguration configuration)
        {
            //services.AddDbContext<ApplicationDbContext>(options =>
            // options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        }
    }
}
