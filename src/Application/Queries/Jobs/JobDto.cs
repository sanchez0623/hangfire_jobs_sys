using System;
using System.Collections.Generic;

namespace HangfireJobsSys.Application.Queries.Jobs
{
    /// <summary>
    /// 任务数据传输对象
    /// </summary>
    public class JobDto
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 任务名称
        /// </summary>
        public string JobName { get; set; }

        /// <summary>
        /// 任务描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 任务类型
        /// </summary>
        public string JobType { get; set; }

        /// <summary>
        /// 执行参数（JSON格式）
        /// </summary>
        public string Parameters { get; set; }

        /// <summary>
        /// 任务状态
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 最大重试次数
        /// </summary>
        public int MaxRetryCount { get; set; }

        /// <summary>
        /// 队列名称
        /// </summary>
        public string QueueName { get; set; }

        /// <summary>
        /// 创建人ID
        /// </summary>
        public Guid CreatedBy { get; set; }

        /// <summary>
        /// 创建人名称
        /// </summary>
        public string CreatedByName { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新人ID
        /// </summary>
        public Guid UpdatedBy { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// 是否有活跃的调度计划
        /// </summary>
        public bool HasActiveSchedule { get; set; }

        /// <summary>
        /// 最近执行时间
        /// </summary>
        public DateTime? LastExecutionTime { get; set; }

        /// <summary>
        /// 最近执行状态
        /// </summary>
        public string LastExecutionStatus { get; set; }
    }
}