using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace HangfireJobsSys.Infrastructure.Data
{
    /// <summary>
    /// 数据库提供程序类型枚举
    /// </summary>
    public enum DatabaseProvider
    {
        /// <summary>
        /// SQL Server数据库
        /// </summary>
        SqlServer
        // MySQL支持将在后续版本中添加
    }

    /// <summary>
    /// 数据库提供程序工厂类
    /// </summary>
    public static class DatabaseProviderFactory
    {
        /// <summary>
        /// 获取数据库提供程序类型
        /// </summary>
        /// <param name="configuration">配置信息</param>
        /// <returns>数据库提供程序类型</returns>
        public static DatabaseProvider GetProvider(IConfiguration configuration)
        {
            var providerName = configuration.GetSection("Database:Provider").Value;
            if (string.IsNullOrEmpty(providerName))
            {
                // 默认使用SQL Server
                return DatabaseProvider.SqlServer;
            }
            return Enum.TryParse<DatabaseProvider>(providerName, true, out var provider)
                ? provider
                : DatabaseProvider.SqlServer;
        }

        /// <summary>
        /// 配置数据库上下文，包含性能优化设置
        /// </summary>
        /// <param name="optionsBuilder">选项构建器</param>
        /// <param name="configuration">配置信息</param>
        public static void ConfigureDbContext(DbContextOptionsBuilder optionsBuilder, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
            
            // SQL Server配置优化
            optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(DatabaseProviderFactory).Assembly.FullName)
                    // 添加重试策略
                    .EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null)
                    // 设置命令超时时间
                    .CommandTimeout(60);
            })
            // 启用敏感数据日志记录（开发环境）
            .EnableSensitiveDataLogging(false)
            // 启用详细错误信息
            .EnableDetailedErrors(false);
            
            // 添加性能监控钩子
            optionsBuilder.LogTo(
                message => Console.WriteLine(message),
                new[] { Microsoft.EntityFrameworkCore.DbLoggerCategory.Database.Command.Name },
                Microsoft.Extensions.Logging.LogLevel.Information);
        }

        /// <summary>
        /// 获取数据库连接字符串
        /// </summary>
        /// <param name="configuration">配置信息</param>
        /// <returns>连接字符串</returns>
        public static string GetConnectionString(IConfiguration configuration)
        {
            return configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        }
    }
}