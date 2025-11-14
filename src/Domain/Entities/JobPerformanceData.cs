using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using HangfireJobsSys.Domain.Enums;

namespace HangfireJobsSys.Domain.Entities
{
    /// <summary>
    /// 任务性能数据实体
    /// 用于存储任务执行性能相关的指标
    /// </summary>
    [Table("JobPerformanceData")]
    public class JobPerformanceData
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
        /// 任务名称
        /// </summary>
        [MaxLength(255)]
        public string JobName { get; set; }

        /// <summary>
        /// 任务类型
        /// </summary>
        [MaxLength(255)]
        public string JobType { get; set; }

        /// <summary>
        /// 执行时间（毫秒）
        /// </summary>
        [Required]
        public double ExecutionTimeMs { get; set; }

        /// <summary>
        /// 执行状态
        /// </summary>
        [Required]
        public ExecutionStatus Status { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        [Required]
        // 注意：索引将在DbContext中配置
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// 自定义指标（JSON格式）
        /// </summary>
        public string CustomMetrics { get; set; }
    }
}