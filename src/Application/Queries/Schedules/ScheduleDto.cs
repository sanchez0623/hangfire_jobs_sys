using System;

namespace HangfireJobsSys.Application.Queries.Schedules
{
    /// <summary>
    /// 调度计划数据传输对象
    /// </summary>
    public class ScheduleDto
    {
        /// <summary>
        /// 调度计划ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 任务ID
        /// </summary>
        public Guid JobId { get; set; }

        /// <summary>
        /// 任务名称
        /// </summary>
        public string JobName { get; set; }

        /// <summary>
        /// 调度类型（Cron/Interval）
        /// </summary>
        public string ScheduleType { get; set; }

        /// <summary>
        /// Cron表达式
        /// </summary>
        public string CronExpression { get; set; }

        /// <summary>
        /// 执行间隔（秒）
        /// </summary>
        public int? IntervalSeconds { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 下次执行时间
        /// </summary>
        public DateTime? NextExecutionTime { get; set; }

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
    }
}