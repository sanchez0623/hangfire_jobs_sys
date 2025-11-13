namespace HangfireJobsSys.WebApi.Models.Auth
{
    /// <summary>
    /// 登录响应模型
    /// </summary>
    public class LoginResponse
    {
        /// <summary>
        /// JWT令牌
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// 令牌过期时间
        /// </summary>
        public DateTime Expiration { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public Guid UserId { get; set; }
    }
}