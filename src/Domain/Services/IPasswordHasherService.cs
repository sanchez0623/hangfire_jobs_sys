namespace HangfireJobsSys.Domain.Services
{
    /// <summary>
    /// 密码哈希服务接口
    /// </summary>
    public interface IPasswordHasherService
    {
        /// <summary>
        /// 对密码进行哈希处理
        /// </summary>
        string HashPassword(string password);

        /// <summary>
        /// 验证密码是否匹配
        /// </summary>
        bool VerifyPassword(string password, string passwordHash);
    }
}