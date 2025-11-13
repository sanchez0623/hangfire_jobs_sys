using HangfireJobsSys.Domain.Services;
using HangfireJobsSys.WebApi.Models.Auth;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace HangfireJobsSys.WebApi.Controllers
{
    /// <summary>
    /// 认证控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        /// <summary>
        /// 构造函数
        /// </summary>
        public AuthController(IAuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var token = await _authService.LoginAsync(request.UserName, request.Password);
                if (string.IsNullOrEmpty(token))
                {
                    return Unauthorized(new { message = "用户名或密码错误" });
                }

                // 从token中提取用户信息或直接从数据库获取
                // 这里简化处理，直接设置过期时间为配置的1小时
                var expiration = DateTime.UtcNow.AddMinutes(60);
                
                // 为了演示，这里创建一个JwtSecurityTokenHandler来解析token获取用户ID
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                
                Guid userId;
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out userId))
                {
                    return Ok(new LoginResponse
                    {
                        Token = token,
                        Expiration = expiration,
                        UserName = request.UserName,
                        UserId = userId
                    });
                }
                
                // 如果无法从token中解析用户ID，返回错误
                return Unauthorized(new { message = "无效的令牌" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "登录失败：" + ex.Message });
            }
        }

        /// <summary>
        /// 用户注册
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _authService.RegisterAsync(
                    request.UserName,
                    request.Password,
                    request.Email
                );

                if (!result)
                {
                    return BadRequest(new { message = "用户名或邮箱已存在" });
                }

                return Ok(new { message = "注册成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "注册失败：" + ex.Message });
            }
        }
    }
}