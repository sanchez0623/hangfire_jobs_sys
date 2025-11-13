using System.Threading.Tasks;

namespace HangfireJobsSys.Domain.Services
{
    /// <summary>
    /// 数据库服务接口
    /// </summary>
    public interface IDatabaseService
    {
        /// <summary>
        /// 初始化数据库
        /// </summary>
        Task InitializeDatabaseAsync();
        
        /// <summary>
        /// 获取数据库连接状态
        /// </summary>
        /// <returns>是否连接正常</returns>
        Task<bool> GetConnectionStatusAsync();
        
        /// <summary>
        /// 获取当前使用的数据库提供商名称
        /// </summary>
        /// <returns>数据库提供商名称</returns>
        string GetDatabaseProviderName();
    }
}