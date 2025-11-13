namespace HangfireJobsSys.Infrastructure.Data.Repositories
{
    using HangfireJobsSys.Domain.Entities;
    using HangfireJobsSys.Domain.Repositories;
    using Microsoft.EntityFrameworkCore;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// 用户仓库实现
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly HangfireJobsSysDbContext _dbContext;

        /// <summary>
        /// 构造函数
        /// </summary>
        public UserRepository(HangfireJobsSysDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        /// <summary>
        /// 根据用户名获取用户
        /// </summary>
        public async Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users
                .Where(u => u.UserName == userName)
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// 根据邮箱获取用户
        /// </summary>
        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users
                .Where(u => u.Email == email)
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// 根据ID获取用户
        /// </summary>
        public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users.FindAsync([id], cancellationToken);
        }

        /// <summary>
        /// 添加用户
        /// </summary>
        public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            await _dbContext.Users.AddAsync(user, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// 更新用户
        /// </summary>
        public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// 检查用户名是否已存在
        /// </summary>
        public async Task<bool> UserNameExistsAsync(string userName, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users
                .AnyAsync(u => u.UserName == userName, cancellationToken);
        }

        /// <summary>
        /// 检查邮箱是否已存在
        /// </summary>
        public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users
                .AnyAsync(u => u.Email == email, cancellationToken);
        }
    }
}