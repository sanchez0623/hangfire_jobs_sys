using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using HangfireJobsSys.Domain.Enums;

namespace HangfireJobsSys.Domain.Entities
{
    /// <summary>
    /// 任务执行日志历史实体（历史表）
    /// </summary>
    [Table("JobExecutionLogs_History")]
    public class JobExecutionLogHistory
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// 任务ID
        /// </summary>
        [Required]
        public Guid JobId { get; set; }

        /// <summary>
        /// 执行时间
        /// </summary>
        [Required]
        public DateTime ExecutionTime { get; set; }

        /// <summary>
        /// 执行状态
        /// </summary>
        [Required]
        public ExecutionStatus Status { get; set; }

        /// <summary>
        /// 结果消息
        /// </summary>
        [MaxLength(500)]
        public string ResultMessage { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        [MaxLength(1000)]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 错误详情
        /// </summary>
        public string ErrorDetails { get; set; }

        /// <summary>
        /// 执行时长（毫秒）
        /// </summary>
        public long ExecutionDurationMs { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 迁移到历史表的时间
        /// </summary>
        [Required]
        public DateTime MigratedAt { get; set; }
    }
}