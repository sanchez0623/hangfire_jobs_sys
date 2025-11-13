using HangfireJobsSys.Domain.Entities;
using HangfireJobsSys.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace HangfireJobsSys.Infrastructure.Data
{
    /// <summary>
    /// 数据库初始化扩展类
    /// </summary>
    public static class DatabaseInitialization
    {
        /// <summary>
        /// 初始化数据库
        /// </summary>
        /// <param name="serviceProvider">服务提供者</param>
        public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<HangfireJobsSysDbContext>();

            // 执行数据库迁移
            await dbContext.Database.MigrateAsync();

            // 如果需要添加种子数据，可以在这里实现
            var passwordHasherService = scope.ServiceProvider.GetRequiredService<IPasswordHasherService>();
            await SeedDataAsync(dbContext, passwordHasherService);
        }

        /// <summary>
        /// 添加种子数据
        /// </summary>
        /// <param name="dbContext">数据库上下文</param>
        /// <param name="passwordHasherService">密码哈希服务</param>
        private static async Task SeedDataAsync(HangfireJobsSysDbContext dbContext, IPasswordHasherService passwordHasherService)
        {
            // 初始化管理员用户
            var adminUserId = await InitializeAdminUserAsync(dbContext, passwordHasherService);
            
            // 检查是否已有任务数据
            if (await dbContext.Jobs.AnyAsync())
            {
                // 数据库已有任务数据，不需要再添加种子数据
                return;
            }

            // 添加示例任务数据
            
            var sampleJob = Job.Create(
                name: "示例定时任务",
                description: "这是一个示例定时任务，用于测试系统功能",
                jobTypeName: "HangfireJobsSys.Tasks.SampleTask",
                parameters: "{\"Message\":\"Hello World\"}",
                createdBy: adminUserId);
            
            await dbContext.Jobs.AddAsync(sampleJob);
            await dbContext.SaveChangesAsync();

            // 添加示例调度计划
            var sampleSchedule = Schedule.CreateCronSchedule(
                jobId: sampleJob.Id,
                cronExpression: "0 * * * *", // 每小时执行一次
                startTime: DateTime.UtcNow,
                endTime: null,
                createdBy: adminUserId);
            
            await dbContext.Schedules.AddAsync(sampleSchedule);
            await dbContext.SaveChangesAsync();
        }
        
        /// <summary>
        /// 初始化管理员用户
        /// </summary>
        /// <param name="dbContext">数据库上下文</param>
        /// <param name="passwordHasherService">密码哈希服务</param>
        private static async Task<Guid> InitializeAdminUserAsync(HangfireJobsSysDbContext dbContext, IPasswordHasherService passwordHasherService)
        {
            // 检查是否已有用户数据
            var existingUser = await dbContext.Users.FirstOrDefaultAsync();
            if (existingUser != null)
            {
                return existingUser.Id;
            }

            // 创建默认管理员用户
            var adminPasswordHash = passwordHasherService.HashPassword("Admin123456");
            var adminUser = User.Create(
                userName: "admin",
                passwordHash: adminPasswordHash,
                email: "admin@example.com"
            );

            await dbContext.Users.AddAsync(adminUser);
            await dbContext.SaveChangesAsync();
            return adminUser.Id;
        }
    }
}