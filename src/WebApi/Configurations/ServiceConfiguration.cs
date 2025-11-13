using HangfireJobsSys.Application.Services;
using HangfireJobsSys.Domain.Repositories;
using HangfireJobsSys.Domain.Services;
using HangfireJobsSys.Infrastructure.Data;
using HangfireJobsSys.Infrastructure.Data.Repositories;
using HangfireJobsSys.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace HangfireJobsSys.WebApi.Configurations
{
    /// <summary>
    /// 服务配置
    /// </summary>
    public static class ServiceConfiguration
    {
        /// <summary>
        /// 配置服务依赖注入
        /// </summary>
        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // 注意：数据库上下文已在Infrastructure.DependencyInjection中配置

            // 注册仓储
            // 修复命名空间问题
            services.AddScoped<IJobRepository, Infrastructure.Repositories.JobRepository>();
            services.AddScoped<IScheduleRepository, Infrastructure.Repositories.ScheduleRepository>();
            services.AddScoped<IUserRepository, Infrastructure.Data.Repositories.UserRepository>();

            // 注册服务
            services.AddScoped<IPasswordHasherService, PasswordHasherService>();
            services.AddScoped<IAuthService, AuthService>(provider =>
            {
                // 从配置中获取JWT设置
                var jwtSettings = configuration.GetSection("Jwt");
                var jwtIssuer = jwtSettings["Issuer"] ?? "HangfireJobsSys";
                var jwtAudience = jwtSettings["Audience"] ?? "HangfireJobsSys";
                var jwtKey = jwtSettings["Key"] ?? "your-secret-key-change-in-production";
                var jwtExpirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");

                return new AuthService(
                    provider.GetRequiredService<IUserRepository>(),
                    provider.GetRequiredService<IPasswordHasherService>(),
                    jwtIssuer,
                    jwtAudience,
                    jwtKey,
                    jwtExpirationMinutes);
            });
            
            // 数据库初始化应该直接调用，而不是通过依赖注入
            // 移除静态类型的依赖注入注册

            // 配置JWT认证
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var jwtSettings = configuration.GetSection("Jwt");
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? ""))
                };
            });
            
            // 添加授权服务
            services.AddAuthorization();

            return services;
        }


    }
}