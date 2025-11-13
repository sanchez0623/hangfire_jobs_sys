using Hangfire;
using Hangfire.Dashboard;
using HangfireJobsSys.Infrastructure.Data;
using HangfireJobsSys.WebApi.Configurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Text;

// 顶级语句必须在类型定义之前
await MainAsync(args);

async Task MainAsync(string[] args)
{
    var builder = WebApplication.CreateBuilder(args);

    // 配置日志服务
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
    builder.Logging.AddDebug();
    builder.Logging.AddEventSourceLogger();
    
    // 配置日志过滤级别
    // 根据环境设置不同的最小日志级别
    if (builder.Environment.IsDevelopment())
    {
        builder.Logging.SetMinimumLevel(LogLevel.Debug);
    }
    else if (builder.Environment.IsStaging())
    {
        builder.Logging.SetMinimumLevel(LogLevel.Information);
    }
    else
    {
        builder.Logging.SetMinimumLevel(LogLevel.Warning);
    }
    
    // 设置不同组件的日志级别
    // ASP.NET Core框架日志
    builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
    builder.Logging.AddFilter("Microsoft.AspNetCore.Hosting", LogLevel.Information);
    builder.Logging.AddFilter("Microsoft.AspNetCore.Routing.EndpointMiddleware", LogLevel.Information);
    
    // Entity Framework日志
    builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
    builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information);
    
    // Hangfire日志
    builder.Logging.AddFilter("Hangfire", LogLevel.Information);
    builder.Logging.AddFilter("Hangfire.BackgroundJobServer", LogLevel.Debug);
    builder.Logging.AddFilter("Hangfire.Server.BackgroundServerProcess", LogLevel.Information);
    
    // 应用程序日志
    builder.Logging.AddFilter("HangfireJobsSys", LogLevel.Debug);
    builder.Logging.AddFilter("HangfireJobsSys.Application", LogLevel.Information);
    builder.Logging.AddFilter("HangfireJobsSys.Infrastructure", LogLevel.Information);
    builder.Logging.AddFilter("HangfireJobsSys.WebApi", LogLevel.Information);

    // Add services to the container.
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        // 配置Swagger支持JWT认证
        options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "JWT授权" + "\r\n" + "请输入：Bearer {token}"
        });
        
        options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });
    
    // 配置自定义服务
    builder.Services.ConfigureServices(builder.Configuration);

    var app = builder.Build();

    // 初始化数据库
    await app.Services.InitializeDatabaseAsync();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    
    // 添加认证和授权中间件
    app.UseAuthentication();
    app.UseAuthorization();
    
    // 配置Hangfire Dashboard的身份验证
    if (app.Environment.IsDevelopment())
    {
        // 开发环境：仍允许无身份验证访问，但记录警告
        app.Logger.LogWarning("开发环境中，Hangfire Dashboard未配置身份验证。生产环境必须配置身份验证。");
        app.UseHangfireDashboard("/hangfire");
    }
    else
    {
        // 生产环境：配置身份验证过滤器
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = new[] { new HangfireAuthorizationFilter() }
        });
    }
    
    // 配置路由
    app.MapControllers();

    app.Run();
}

/// <summary>
/// Hangfire Dashboard的授权过滤器
/// </summary>
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    /// <summary>
    /// 检查用户是否有权限访问Dashboard
    /// </summary>
    public bool Authorize(DashboardContext context)
    {
        // 获取HTTP上下文
        var httpContext = context.GetHttpContext();
        
        // 添加空值检查
        if (httpContext == null || httpContext.User == null || httpContext.User.Identity == null)
        {
            return false;
        }
        
        // 检查用户是否已认证
        if (!httpContext.User.Identity.IsAuthenticated)
        {
            return false;
        }
        
        // 这里可以添加更多的权限检查，比如角色检查
        // 例如：return httpContext.User.IsInRole("Administrator");
        
        return true; // 暂时允许所有已认证用户访问
    }
}
