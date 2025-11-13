using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace HangfireJobsSys.Domain.Entities
{
    /// <summary>
    /// 调度计划实体
    /// </summary>
    [Table("Schedules")]
    public class Schedule
    {
        /// <summary>
        /// 调度计划ID
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; private set; }

        /// <summary>
        /// 任务ID
        /// </summary>
        [Required]
        public Guid JobId { get; private set; }

        /// <summary>
        /// 关联的任务
        /// </summary>
        [ForeignKey("JobId")]
        public Job Job { get; private set; }

        /// <summary>
        /// 调度类型
        /// </summary>
        public ScheduleType Type { get; private set; }

        /// <summary>
        /// Cron表达式（用于定时任务）
        /// </summary>
        [MaxLength(50)]
        public string CronExpression { get; private set; }

        /// <summary>
        /// 执行间隔（秒）（用于周期性任务）
        /// </summary>
        public int? IntervalSeconds { get; private set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime { get; private set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; private set; }

        /// <summary>
        /// 调度计划状态
        /// </summary>
        public ScheduleStatus Status { get; private set; }

        /// <summary>
        /// Hangfire任务ID
        /// </summary>
        [MaxLength(100)]
        public string HangfireJobId { get; private set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime UpdatedAt { get; private set; }

        /// <summary>
        /// 创建人ID
        /// </summary>
        [Required]
        public Guid CreatedBy { get; private set; }

        /// <summary>
        /// 最后修改人ID
        /// </summary>
        [Required]
        public Guid UpdatedBy { get; private set; }

        /// <summary>
        /// 私有构造函数，防止直接实例化
        /// </summary>
        protected Schedule() { }

        /// <summary>
        /// 创建Cron类型的调度计划
        /// </summary>
        public static Schedule CreateCronSchedule(Guid jobId, string cronExpression, DateTime? startTime, DateTime? endTime, Guid createdBy)
        {
            if (string.IsNullOrEmpty(cronExpression))
                throw new ArgumentException("Cron表达式不能为空", nameof(cronExpression));

            var now = DateTime.UtcNow;
            return new Schedule
            {
                Id = Guid.NewGuid(),
                JobId = jobId,
                Type = ScheduleType.Cron,
                CronExpression = cronExpression,
                StartTime = startTime,
                EndTime = endTime,
                Status = ScheduleStatus.Inactive,
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = createdBy,
                UpdatedBy = createdBy
            };
        }

        /// <summary>
        /// 创建间隔类型的调度计划
        /// </summary>
        public static Schedule CreateIntervalSchedule(Guid jobId, int intervalSeconds, DateTime? startTime, DateTime? endTime, Guid createdBy)
        {
            if (intervalSeconds <= 0)
                throw new ArgumentException("执行间隔必须大于0", nameof(intervalSeconds));

            var now = DateTime.UtcNow;
            return new Schedule
            {
                Id = Guid.NewGuid(),
                JobId = jobId,
                Type = ScheduleType.Interval,
                IntervalSeconds = intervalSeconds,
                StartTime = startTime,
                EndTime = endTime,
                Status = ScheduleStatus.Inactive,
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = createdBy,
                UpdatedBy = createdBy
            };
        }

        /// <summary>
        /// 关联Hangfire任务ID
        /// </summary>
        public void AssociateHangfireJobId(string hangfireJobId)
        {
            HangfireJobId = hangfireJobId;
        }

        /// <summary>
        /// 清除Hangfire任务ID
        /// </summary>
        public void ClearHangfireJobId()
        {
            HangfireJobId = null;
        }

        /// <summary>
        /// 激活调度计划
        /// </summary>
        public void Activate(Guid updatedBy)
        {
            if (Status == ScheduleStatus.Deleted)
                throw new InvalidOperationException("已删除的调度计划不能激活");
            if (Status == ScheduleStatus.Active)
                throw new InvalidOperationException("调度计划已经是激活状态");

            Status = ScheduleStatus.Active;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        /// <summary>
        /// 暂停调度计划
        /// </summary>
        public void Pause(Guid updatedBy)
        {
            if (Status != ScheduleStatus.Active)
                throw new InvalidOperationException("只有激活状态的调度计划才能暂停");

            Status = ScheduleStatus.Paused;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        /// <summary>
        /// 删除调度计划
        /// </summary>
        public void Delete(Guid updatedBy)
        {
            if (Status == ScheduleStatus.Deleted)
                throw new InvalidOperationException("调度计划已经被删除");

            Status = ScheduleStatus.Deleted;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }
    }

    /// <summary>
    /// 调度类型枚举
    /// </summary>
    public enum ScheduleType
    {
        /// <summary>
        /// Cron表达式调度
        /// </summary>
        Cron = 0,

        /// <summary>
        /// 间隔调度
        /// </summary>
        Interval = 1
    }

    /// <summary>
    /// 调度状态枚举
    /// </summary>
    public enum ScheduleStatus
    {
        /// <summary>
        /// 未激活
        /// </summary>
        Inactive = 0,

        /// <summary>
        /// 激活
        /// </summary>
        Active = 1,

        /// <summary>
        /// 暂停
        /// </summary>
        Paused = 2,

        /// <summary>
        /// 删除
        /// </summary>
        Deleted = 3
    }
}