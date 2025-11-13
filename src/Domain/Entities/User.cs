namespace HangfireJobsSys.Domain.Entities
{
    /// <summary>
    /// 用户实体类
    /// </summary>
    public class User
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; private set; }

        /// <summary>
        /// 密码哈希
        /// </summary>
        public string PasswordHash { get; private set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; private set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdatedAt { get; private set; }

        /// <summary>
        /// 创建用户
        /// </summary>
        public static User Create(string userName, string passwordHash, string email)
        {
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("用户名不能为空", nameof(userName));
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("密码哈希不能为空", nameof(passwordHash));
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("邮箱不能为空", nameof(email));

            return new User
            {
                Id = Guid.NewGuid(),
                UserName = userName,
                PasswordHash = passwordHash,
                Email = email,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// 更新密码哈希
        /// </summary>
        public void UpdatePasswordHash(string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("密码哈希不能为空", nameof(passwordHash));
            
            PasswordHash = passwordHash;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// 更新邮箱
        /// </summary>
        public void UpdateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("邮箱不能为空", nameof(email));
            
            Email = email;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}