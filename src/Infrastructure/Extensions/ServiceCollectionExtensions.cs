using HangfireJobsSys.Domain.Repositories;
using HangfireJobsSys.Domain.Services;
using HangfireJobsSys.Infrastructure.Data;
using HangfireJobsSys.Infrastructure.Repositories;
using HangfireJobsSys.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace HangfireJobsSys.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // 注册数据库上下文
            services.AddDbContext<HangfireJobsSysDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            
            // 注册仓储
            services.AddScoped<IJobRepository, JobRepository>();
            services.AddScoped<IScheduleRepository, ScheduleRepository>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            
            // 注册服务
            services.AddScoped<IJobService, HangfireJobService>();
            services.AddScoped<IEnhancedLoggingService, EnhancedLoggingService>();
            services.AddScoped<IPerformanceMonitoringService, PerformanceMonitoringService>();
            
            // 配置性能监控选项
            services.Configure<PerformanceMonitoringOptions>(configuration.GetSection("PerformanceMonitoring"));
            
            return services;
        }
    }
}