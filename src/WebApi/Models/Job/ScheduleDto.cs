namespace HangfireJobsSys.WebApi.Models.Job
{
    /// <summary>
    /// 调度DTO
    /// </summary>
    public class ScheduleDto
    {
        /// <summary>
        /// 调度ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 任务ID
        /// </summary>
        public Guid JobId { get; set; }

        /// <summary>
        /// Cron表达式
        /// </summary>
        public string CronExpression { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 上次执行时间
        /// </summary>
        public DateTime? LastExecutionTime { get; set; }

        /// <summary>
        /// 下次执行时间
        /// </summary>
        public DateTime? NextExecutionTime { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}