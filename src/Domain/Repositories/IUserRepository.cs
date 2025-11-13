namespace HangfireJobsSys.Domain.Repositories
{
    using HangfireJobsSys.Domain.Entities;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// 用户仓库接口
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// 根据用户名获取用户
        /// </summary>
        Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);

        /// <summary>
        /// 根据邮箱获取用户
        /// </summary>
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// 根据ID获取用户
        /// </summary>
        Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// 添加用户
        /// </summary>
        Task AddAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// 更新用户
        /// </summary>
        Task UpdateAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// 检查用户名是否已存在
        /// </summary>
        Task<bool> UserNameExistsAsync(string userName, CancellationToken cancellationToken = default);

        /// <summary>
        /// 检查邮箱是否已存在
        /// </summary>
        Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    }
}