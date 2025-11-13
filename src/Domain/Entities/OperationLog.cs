using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace HangfireJobsSys.Domain.Entities
{
    /// <summary>
    /// 操作日志实体
    /// </summary>
    [Table("OperationLogs")]
    public class OperationLog
    {
        /// <summary>
        /// 日志ID
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; private set; }

        /// <summary>
        /// 操作人ID
        /// </summary>
        public Guid? OperatorId { get; private set; }

        /// <summary>
        /// 操作人名称
        /// </summary>
        [MaxLength(100)]
        public string OperatorName { get; private set; }

        /// <summary>
        /// 操作类型
        /// </summary>
        public OperationType Type { get; private set; }

        /// <summary>
        /// 操作模块
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Module { get; private set; }

        /// <summary>
        /// 操作内容描述
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Description { get; private set; }

        /// <summary>
        /// 关联实体ID
        /// </summary>
        public Guid? EntityId { get; private set; }

        /// <summary>
        /// 操作参数（JSON格式）
        /// </summary>
        public string Parameters { get; private set; }

        /// <summary>
        /// 客户端IP
        /// </summary>
        [MaxLength(50)]
        public string ClientIp { get; private set; }

        /// <summary>
        /// 客户端浏览器
        /// </summary>
        [MaxLength(200)]
        public string ClientBrowser { get; private set; }

        /// <summary>
        /// 操作状态
        /// </summary>
        public OperationStatus Status { get; private set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// 私有构造函数，防止直接实例化
        /// </summary>
        protected OperationLog() { }

        /// <summary>
        /// 创建操作日志
        /// </summary>
        public static OperationLog Create(
            Guid? operatorId,
            string operatorName,
            OperationType type,
            string module,
            string description,
            Guid? entityId = null,
            string parameters = null,
            string clientIp = null,
            string clientBrowser = null)
        {
            var now = DateTime.UtcNow;
            return new OperationLog
            {
                Id = Guid.NewGuid(),
                OperatorId = operatorId,
                OperatorName = operatorName,
                Type = type,
                Module = module,
                Description = description,
                EntityId = entityId,
                Parameters = parameters,
                ClientIp = clientIp,
                ClientBrowser = clientBrowser,
                Status = OperationStatus.Succeeded,
                CreatedAt = now
            };
        }

        /// <summary>
        /// 标记操作失败
        /// </summary>
        public void Fail(string errorMessage)
        {
            Status = OperationStatus.Failed;
            ErrorMessage = errorMessage;
        }
    }

    /// <summary>
    /// 操作类型枚举
    /// </summary>
    public enum OperationType
    {
        /// <summary>
        /// 创建
        /// </summary>
        Create = 0,

        /// <summary>
        /// 更新
        /// </summary>
        Update = 1,

        /// <summary>
        /// 删除
        /// </summary>
        Delete = 2,

        /// <summary>
        /// 查询
        /// </summary>
        Query = 3,

        /// <summary>
        /// 执行
        /// </summary>
        Execute = 4,

        /// <summary>
        /// 启用
        /// </summary>
        Activate = 5,

        /// <summary>
        /// 停用
        /// </summary>
        Deactivate = 6,

        /// <summary>
        /// 登录
        /// <summary>
        Login = 7,

        /// <summary>
        /// 登出
        /// </summary>
        Logout = 8,

        /// <summary>
        /// 其他
        /// </summary>
        Other = 9
    }

    /// <summary>
    /// 操作状态枚举
    /// </summary>
    public enum OperationStatus
    {
        /// <summary>
        /// 成功
        /// </summary>
        Succeeded = 0,

        /// <summary>
        /// 失败
        /// </summary>
        Failed = 1
    }
}