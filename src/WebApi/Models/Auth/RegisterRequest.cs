using System.ComponentModel.DataAnnotations;

namespace HangfireJobsSys.WebApi.Models.Auth
{
    /// <summary>
    /// 注册请求模型
    /// </summary>
    public class RegisterRequest
    {
        /// <summary>
        /// 用户名
        /// </summary>
        [Required(ErrorMessage = "用户名不能为空")]
        [StringLength(50, ErrorMessage = "用户名长度不能超过50个字符")]
        public string UserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [Required(ErrorMessage = "密码不能为空")]
        [DataType(DataType.Password)]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "密码长度必须在6到50个字符之间")]
        public string Password { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        [Required(ErrorMessage = "邮箱不能为空")]
        [EmailAddress(ErrorMessage = "请输入有效的邮箱地址")]
        public string Email { get; set; }
    }
}