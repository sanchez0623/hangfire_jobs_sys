namespace HangfireJobsSys.Domain.Services
{
    using HangfireJobsSys.Domain.Entities;
using System.Threading.Tasks;
using System.Threading;

    /// <summary>
    /// 认证服务接口
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// 用户登录
        /// </summary>
        Task<string> LoginAsync(string userName, string password, CancellationToken cancellationToken = default);

        /// <summary>
        /// 用户注册
        /// </summary>
        Task<bool> RegisterAsync(string userName, string password, string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// 验证密码
        /// </summary>
        bool VerifyPassword(string password, string passwordHash);

        /// <summary>
        /// 生成JWT token
        /// </summary>
        string GenerateJwtToken(string userName, string userId);
    }
}