using HangfireJobsSys.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace HangfireJobsSys.Infrastructure.Data
{
    /// <summary>
    /// 数据库上下文类
    /// </summary>
    public class HangfireJobsSysDbContext : DbContext
    {
        /// <summary>
        /// 任务表
        /// </summary>
        public DbSet<Job> Jobs { get; set; }

        /// <summary>
        /// 调度计划表
        /// </summary>
        public DbSet<Schedule> Schedules { get; set; }

        /// <summary>
        /// 任务执行日志表
        /// </summary>
        public DbSet<JobExecutionLog> JobExecutionLogs { get; set; }

        /// <summary>
        /// 操作日志表
        /// </summary>
        public DbSet<OperationLog> OperationLogs { get; set; }

        /// <summary>
        /// 用户表
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// 审计日志表
        /// </summary>
        public DbSet<AuditLog> AuditLogs { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public HangfireJobsSysDbContext(DbContextOptions<HangfireJobsSysDbContext> options) : base(options)
        {}

        /// <summary>
        /// 配置实体映射
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 配置实体关系
            modelBuilder.Entity<Job>()
                .HasMany(j => j.Schedules)
                .WithOne(s => s.Job)
                .HasForeignKey(s => s.JobId);

            modelBuilder.Entity<Job>()
                .HasMany(j => j.ExecutionLogs)
                .WithOne(log => log.Job)
                .HasForeignKey(log => log.JobId);

            // 配置表名
            modelBuilder.Entity<Job>().ToTable("Jobs");
            modelBuilder.Entity<Schedule>().ToTable("Schedules");
            modelBuilder.Entity<JobExecutionLog>().ToTable("JobExecutionLogs");
            modelBuilder.Entity<OperationLog>().ToTable("OperationLogs");
            modelBuilder.Entity<User>().ToTable("Users");

            // 配置用户表索引
            modelBuilder.Entity<User>()
                .HasIndex(u => u.UserName)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // 配置审计日志表
            modelBuilder.Entity<AuditLog>().ToTable("AuditLogs");

            // 配置种子数据（可选）
            // modelBuilder.Entity<Job>().HasData(...);
        }
    }
}