using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HangfireJobsSys.Domain.Entities;
using HangfireJobsSys.Domain.Enums;
using HangfireJobsSys.Domain.Repositories;
using HangfireJobsSys.Infrastructure.Data;
using HangfireJobsSys.Infrastructure.Services.JobExecutors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Infrastructure.Tests
{
    public class DataMigrationJobTests
    {
        private readonly HangfireJobsSysDbContext _dbContext;
        private readonly DataMigrationJob _dataMigrationJob;

        public DataMigrationJobTests()
        {
            // 设置内存数据库
            var options = new DbContextOptionsBuilder<HangfireJobsSysDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
            
            _dbContext = new HangfireJobsSysDbContext(options);
            
            // 直接创建服务提供者，而不是使用Moq
            var services = new ServiceCollection();
            services.AddSingleton(_dbContext);
            var loggerMock = Mock.Of<ILogger<DataMigrationJob>>();
            services.AddSingleton(loggerMock);
            
            // 添加DataMigrationJob需要的其他依赖
            var jobRepositoryMock = Mock.Of<IJobRepository>();
            services.AddSingleton(jobRepositoryMock);
            
            services.AddSingleton<IServiceScopeFactory>(new ServiceScopeFactoryMock(_dbContext));
            var serviceProvider = services.BuildServiceProvider();
            
            // 创建DataMigrationJob实例
            _dataMigrationJob = new DataMigrationJob(serviceProvider, loggerMock);
        }
        
        // 简单的ServiceScopeFactory mock实现
        private class ServiceScopeFactoryMock : IServiceScopeFactory
        {
            private readonly HangfireJobsSysDbContext _dbContext;
            
            public ServiceScopeFactoryMock(HangfireJobsSysDbContext dbContext)
            {
                _dbContext = dbContext;
            }
            
            public IServiceScope CreateScope()
            {
                return new ServiceScopeMock(_dbContext);
            }
        }
        
        private class ServiceScopeMock : IServiceScope
        {
            private readonly HangfireJobsSysDbContext _dbContext;
            
            public ServiceScopeMock(HangfireJobsSysDbContext dbContext)
            {
                _dbContext = dbContext;
            }
            
            public void Dispose() { }
            
            public IServiceProvider ServiceProvider => new ServiceProviderMock(_dbContext);
        }
        
        private class ServiceProviderMock : IServiceProvider
        {
            private readonly HangfireJobsSysDbContext _dbContext;
            
            public ServiceProviderMock(HangfireJobsSysDbContext dbContext)
            {
                _dbContext = dbContext;
            }
            
            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(HangfireJobsSysDbContext))
                {
                    return _dbContext;
                }
                return null;
            }
        }

        [Fact]
        public async Task ExecuteAsync_ShouldRunWithoutExceptionWithValidParameters()
        {
            // Arrange
            var parameters = @"{""BatchSize"":1000,""RetentionDays"":30}";
            
            // Act & Assert - 确保方法能正常执行不抛出异常
            await _dataMigrationJob.ExecuteAsync(parameters);
            // 如果执行到这里没有抛出异常，测试就通过了
        }

        [Fact]
        public async Task ExecuteAsync_ShouldRunWithEmptyParameters()
        {
            // Arrange & Act & Assert
            // 确保当参数为空时不会抛出异常
            await _dataMigrationJob.ExecuteAsync(null);
        }

        // 移除对私有方法的测试，专注于测试公共方法ExecuteAsync
    }
}