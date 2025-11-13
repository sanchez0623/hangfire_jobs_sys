namespace HangfireJobsSys.Infrastructure.Services
{
    using HangfireJobsSys.Domain.Services;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// 密码哈希服务实现
    /// </summary>
    public class PasswordHasherService : IPasswordHasherService
    {
        /// <summary>
        /// 对密码进行哈希处理
        /// </summary>
        public string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("密码不能为空", nameof(password));

            // 简单的SHA256哈希实现（实际生产环境应使用更安全的算法）
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        /// <summary>
        /// 验证密码是否匹配
        /// </summary>
        public bool VerifyPassword(string password, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("密码不能为空", nameof(password));
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("密码哈希不能为空", nameof(passwordHash));

            // 计算输入密码的哈希并与存储的哈希比较
            var inputHash = HashPassword(password);
            return inputHash == passwordHash;
        }
    }
}