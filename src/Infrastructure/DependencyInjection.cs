using Hangfire;
using Hangfire.SqlServer;
using HangfireJobsSys.Domain.Repositories;
using HangfireJobsSys.Domain.Services;
using HangfireJobsSys.Infrastructure.Data;
using HangfireJobsSys.Infrastructure.Repositories;
using HangfireJobsSys.Infrastructure.Services;
using HangfireJobsSys.Infrastructure.Services.JobExecutors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace HangfireJobsSys.Infrastructure
{
    /// <summary>
    /// 依赖注入配置
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// 注册Infrastructure层的服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="configuration">配置信息</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // 注册数据库上下文
            services.AddDbContext<HangfireJobsSysDbContext>(options =>
                DatabaseProviderFactory.ConfigureDbContext(options, configuration));
                
            // 获取数据库提供程序类型
            var databaseProvider = DatabaseProviderFactory.GetProvider(configuration);

            // 注册仓储服务 - 使用作用域生命周期
        services.AddScoped<IJobRepository, JobRepository>();
        services.AddScoped<IScheduleRepository, ScheduleRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        
        // 注册任务服务
        services.AddScoped<IJobService, HangfireJobService>();
        
        // 注册任务执行器（瞬态生命周期，避免长时间持有资源）
        services.AddTransient<JobExecutor>();
            
            // 注册数据库服务
            services.AddScoped<IDatabaseService, DatabaseService>();
            
            // 注册性能监控和增强日志服务
            services.AddSingleton<IPerformanceMonitoringService, PerformanceMonitoringService>();
            services.AddScoped<IEnhancedLoggingService, EnhancedLoggingService>();
            
            // 获取连接字符串
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            
            // 配置Hangfire存储（使用SQL Server），优化性能设置
            services.AddHangfire(config => 
            {
                config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
                    {
                        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                        // 减少锁定时间以提高并发性能
                        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(2),
                        // 增加轮询间隔以减少CPU使用率
                        QueuePollInterval = TimeSpan.FromMilliseconds(500),
                        UseRecommendedIsolationLevel = true,
                        DisableGlobalLocks = true,
                        PrepareSchemaIfNecessary = true,
                        SchemaName = "Hangfire",
                        // 启用作业过期和自动清理
                        JobExpirationCheckInterval = TimeSpan.FromHours(1),
                        CountersAggregateInterval = TimeSpan.FromMinutes(5)
                    });
            });
            
            // 添加Hangfire服务器，优化配置
            services.AddHangfireServer(options =>
            {
                // 根据CPU核心数配置工作线程
                options.WorkerCount = Environment.ProcessorCount * 5;
                // 设置队列优先级
                options.Queues = new[] { "critical", "high", "default", "low" };
                // 设置服务器名称，便于监控
                options.ServerName = $"HangfireServer-{Environment.MachineName}-{Guid.NewGuid().ToString().Substring(0, 8)}";
                // 优化心跳间隔
                options.HeartbeatInterval = TimeSpan.FromSeconds(30);
                // 设置取消超时时间
                options.ShutdownTimeout = TimeSpan.FromMinutes(1);
            });
            
            return services;
        }
    }
}