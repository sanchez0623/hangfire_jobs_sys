namespace HangfireJobsSys.Application.Services
{
    using HangfireJobsSys.Domain.Repositories;
    using HangfireJobsSys.Domain.Services;
    using HangfireJobsSys.Domain.Entities;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;
    using System.Threading;
    using System;
    using Microsoft.IdentityModel.Tokens;
    using System.IdentityModel.Tokens.Jwt;

    /// <summary>
    /// 认证服务实现
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasherService _passwordHasherService;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;
        private readonly string _jwtKey;
        private readonly int _jwtExpirationMinutes;

        /// <summary>
        /// 构造函数
        /// </summary>
        public AuthService(
            IUserRepository userRepository,
            IPasswordHasherService passwordHasherService,
            string jwtIssuer,
            string jwtAudience,
            string jwtKey,
            int jwtExpirationMinutes = 60)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _passwordHasherService = passwordHasherService ?? throw new ArgumentNullException(nameof(passwordHasherService));
            _jwtIssuer = jwtIssuer ?? throw new ArgumentNullException(nameof(jwtIssuer));
            _jwtAudience = jwtAudience ?? throw new ArgumentNullException(nameof(jwtAudience));
            _jwtKey = jwtKey ?? throw new ArgumentNullException(nameof(jwtKey));
            _jwtExpirationMinutes = jwtExpirationMinutes;
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        public async Task<string> LoginAsync(string userName, string password, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("用户名不能为空", nameof(userName));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("密码不能为空", nameof(password));

            // 根据用户名查找用户
            var user = await _userRepository.GetByUserNameAsync(userName, cancellationToken);
            if (user == null)
                throw new InvalidOperationException("用户名或密码错误");

            // 验证密码
            if (!VerifyPassword(password, user.PasswordHash))
                throw new InvalidOperationException("用户名或密码错误");

            // 生成JWT token
            return GenerateJwtToken(user.UserName, user.Id.ToString());
        }

        /// <summary>
        /// 用户注册
        /// </summary>
        public async Task<bool> RegisterAsync(string userName, string password, string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("用户名不能为空", nameof(userName));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("密码不能为空", nameof(password));
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("邮箱不能为空", nameof(email));

            // 检查用户名是否已存在
            if (await _userRepository.UserNameExistsAsync(userName, cancellationToken))
                throw new InvalidOperationException("用户名已存在");

            // 检查邮箱是否已存在
            if (await _userRepository.EmailExistsAsync(email, cancellationToken))
                throw new InvalidOperationException("邮箱已存在");

            // 对密码进行哈希处理
            var passwordHash = _passwordHasherService.HashPassword(password);

            // 创建用户
            var user = User.Create(userName, passwordHash, email);

            // 保存用户
            await _userRepository.AddAsync(user, cancellationToken);

            return true;
        }

        /// <summary>
        /// 验证密码
        /// </summary>
        public bool VerifyPassword(string password, string passwordHash)
        {
            return _passwordHasherService.VerifyPassword(password, passwordHash);
        }

        /// <summary>
        /// 生成JWT token
        /// </summary>
        public string GenerateJwtToken(string userName, string userId)
        {
            // 创建token内容
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // 创建密钥
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 创建token
            var token = new JwtSecurityToken(
                issuer: _jwtIssuer,
                audience: _jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtExpirationMinutes),
                signingCredentials: credentials
            );

            // 生成token字符串
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}