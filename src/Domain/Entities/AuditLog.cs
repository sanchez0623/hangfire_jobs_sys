using System;
using System.Collections.Generic;

namespace HangfireJobsSys.Domain.Entities
{
    /// <summary>
    /// 审计日志实体
    /// </summary>
    public class AuditLog
    {
        /// <summary>
        /// ID
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// 操作
        /// </summary>
        public string Action { get; private set; }

        /// <summary>
        /// 实体类型
        /// </summary>
        public string EntityType { get; private set; }

        /// <summary>
        /// 实体ID
        /// </summary>
        public Guid EntityId { get; private set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public string UserId { get; private set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// 详情数据
        /// </summary>
        public string DetailsJson { get; private set; }

        /// <summary>
        /// 私有构造函数（用于EF Core）
        /// </summary>
        private AuditLog() { }

        /// <summary>
        /// 创建审计日志
        /// </summary>
        public static AuditLog Create(string action, string entityType, Guid entityId, string userId = null)
        {
            return new AuditLog
            {
                Id = Guid.NewGuid(),
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                UserId = userId ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                DetailsJson = "{}"
            };
        }

        /// <summary>
        /// 添加详情
        /// </summary>
        public void AddDetail(string key, object value)
        {
            // 这里简化实现，实际项目中可能需要使用JSON序列化
            // 或者维护一个详情字典并在保存时序列化
        }
    }
}