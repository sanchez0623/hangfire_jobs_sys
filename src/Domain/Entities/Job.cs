using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace HangfireJobsSys.Domain.Entities
{
    /// <summary>
    /// 任务实体 - 领域核心对象
    /// </summary>
    [Table("Jobs")]
    public class Job
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; private set; }

        /// <summary>
        /// 任务名称
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; private set; }

        /// <summary>
        /// 任务描述
        /// </summary>
        [MaxLength(1000)]
        public string Description { get; private set; }

        /// <summary>
        /// 任务类型完整名称（包括命名空间）
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string JobTypeName { get; private set; }

        /// <summary>
        /// 任务参数（JSON格式）
        /// </summary>
        public string Parameters { get; private set; }

        /// <summary>
    /// 任务状态
    /// </summary>
    public JobStatus Status { get; private set; }

    /// <summary>
    /// 任务优先级
    /// </summary>
    public JobPriority Priority { get; private set; }

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
        public Guid CreatedBy { get; private set; }

        /// <summary>
        /// 最后修改人ID
        /// </summary>
        public Guid UpdatedBy { get; private set; }

        /// <summary>
        /// 调度计划集合
        /// </summary>
        public ICollection<Schedule> Schedules { get; private set; }

        /// <summary>
        /// 任务执行日志集合
        /// </summary>
        public ICollection<JobExecutionLog> ExecutionLogs { get; private set; }

        /// <summary>
        /// 私有构造函数，防止直接实例化
        /// </summary>
        protected Job() { }

        /// <summary>
        /// 创建新任务
        /// </summary>
        public static Job Create(string name, string description, string jobTypeName, string parameters, Guid createdBy, JobPriority priority = JobPriority.Default)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("任务名称不能为空", nameof(name));
            if (string.IsNullOrEmpty(jobTypeName))
                throw new ArgumentException("任务类型名称不能为空", nameof(jobTypeName));

            var now = DateTime.UtcNow;
            return new Job
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description,
                JobTypeName = jobTypeName,
                Parameters = parameters ?? "{}",
                Status = JobStatus.Draft,
                Priority = priority,
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = createdBy,
                UpdatedBy = createdBy,
                Schedules = new List<Schedule>(),
                ExecutionLogs = new List<JobExecutionLog>()
            };
        }

        /// <summary>
        /// 更新任务信息
        /// </summary>
        public void Update(string name, string description, string jobTypeName, string parameters, Guid updatedBy, JobPriority? priority = null)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("任务名称不能为空", nameof(name));
            if (string.IsNullOrEmpty(jobTypeName))
                throw new ArgumentException("任务类型名称不能为空", nameof(jobTypeName));

            Name = name;
            Description = description;
            JobTypeName = jobTypeName;
            Parameters = parameters ?? "{}";
            if (priority.HasValue)
            {
                Priority = priority.Value;
            }
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        /// <summary>
        /// 激活任务
        /// </summary>
        public void Activate(Guid updatedBy)
        {
            if (Status == JobStatus.Deleted)
                throw new InvalidOperationException("已删除的任务不能激活");

            Status = JobStatus.Active;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        /// <summary>
        /// 暂停任务
        /// </summary>
        public void Pause(Guid updatedBy)
        {
            if (Status != JobStatus.Active)
                throw new InvalidOperationException("只有激活状态的任务才能暂停");

            Status = JobStatus.Paused;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        /// <summary>
        /// 删除任务
        /// </summary>
        public void Delete(Guid updatedBy)
        {
            if (Status == JobStatus.Deleted)
                throw new InvalidOperationException("任务已经被删除");

            Status = JobStatus.Deleted;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }
    }

    /// <summary>
    /// 任务状态枚举
    /// </summary>
    public enum JobStatus
    {
        /// <summary>
        /// 草稿
        /// </summary>
        Draft = 0,

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

    /// <summary>
    /// 任务优先级枚举
    /// </summary>
    public enum JobPriority
    {
        /// <summary>
        /// 低优先级
        /// </summary>
        Low = 0,

        /// <summary>
        /// 默认优先级
        /// </summary>
        Default = 1,

        /// <summary>
        /// 高优先级
        /// </summary>
        High = 2,

        /// <summary>
        /// 关键优先级
        /// </summary>
        Critical = 3
    }
}