using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace HangfireJobsSys.Infrastructure.Data
{
    /// <summary>
    /// EF Core设计时工厂类，用于在执行迁移命令时创建DbContext实例
    /// </summary>
    public class HangfireJobsSysDbContextFactory : IDesignTimeDbContextFactory<HangfireJobsSysDbContext>
    {
        /// <summary>
        /// 创建DbContext实例
        /// </summary>
        public HangfireJobsSysDbContext CreateDbContext(string[] args)
        {
            // 加载配置文件
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();
            
            var optionsBuilder = new DbContextOptionsBuilder<HangfireJobsSysDbContext>();
            
            // 使用DatabaseProviderFactory配置数据库上下文
            DatabaseProviderFactory.ConfigureDbContext(optionsBuilder, configuration);

            return new HangfireJobsSysDbContext(optionsBuilder.Options);
        }
    }
}