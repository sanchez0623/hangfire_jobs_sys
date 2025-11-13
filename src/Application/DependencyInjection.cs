using HangfireJobsSys.Application.Handlers.Jobs;
using HangfireJobsSys.Application.Handlers.Schedules;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace HangfireJobsSys.Application
{
    /// <summary>
    /// 应用层依赖注入配置
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// 注册应用层服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // 注册MediatR
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            });

            // 注册命令处理程序
            RegisterCommandHandlers(services);

            return services;
        }

        /// <summary>
        /// 注册命令处理程序
        /// </summary>
        /// <param name="services">服务集合</param>
        private static void RegisterCommandHandlers(IServiceCollection services)
        {
            // 任务相关命令处理程序
            services.AddScoped<CreateJobCommandHandler>();
            services.AddScoped<UpdateJobCommandHandler>();
            services.AddScoped<DeleteJobCommandHandler>();
            services.AddScoped<UpdateJobStatusCommandHandler>();
            services.AddScoped<ExecuteJobImmediatelyCommandHandler>();

            // 调度计划相关命令处理程序
            services.AddScoped<CreateCronScheduleCommandHandler>();
            services.AddScoped<CreateIntervalScheduleCommandHandler>();
            services.AddScoped<CreateOneTimeScheduleCommandHandler>();
            services.AddScoped<DeleteScheduleCommandHandler>();
            services.AddScoped<UpdateScheduleStatusCommandHandler>();
        }
    }
}