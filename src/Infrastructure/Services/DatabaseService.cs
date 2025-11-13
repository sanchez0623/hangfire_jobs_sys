using HangfireJobsSys.Domain.Services;
using HangfireJobsSys.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace HangfireJobsSys.Infrastructure.Services
{
    /// <summary>
    /// 数据库服务实现
    /// </summary>
    public class DatabaseService : IDatabaseService
    {
        private readonly HangfireJobsSysDbContext _dbContext;
        private readonly IConfiguration _configuration;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dbContext">数据库上下文</param>
        /// <param name="configuration">配置信息</param>
        public DatabaseService(HangfireJobsSysDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }
        
        /// <summary>
        /// 初始化数据库
        /// </summary>
        public async Task InitializeDatabaseAsync()
        {
            // 应用数据库迁移
            await _dbContext.Database.MigrateAsync();
        }
        
        /// <summary>
        /// 获取数据库连接状态
        /// </summary>
        /// <returns>是否连接正常</returns>
        public async Task<bool> GetConnectionStatusAsync()
        {
            try
            {
                // 测试数据库连接
                return await _dbContext.Database.CanConnectAsync();
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// 获取当前使用的数据库提供商名称
        /// </summary>
        /// <returns>数据库提供商名称</returns>
        public string GetDatabaseProviderName()
        {
            var provider = DatabaseProviderFactory.GetProvider(_configuration);
            return provider.ToString();
        }
    }
}