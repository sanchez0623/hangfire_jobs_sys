using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace HangfireJobsSys.Domain.Entities
{
    /// <summary>
    /// 任务执行日志实体
    /// </summary>
    [Table("JobExecutionLogs")]
    public class JobExecutionLog
    {
        /// <summary>
        /// 日志ID
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
        /// Hangfire执行ID
        /// </summary>
        [MaxLength(100)]
        public string HangfireExecutionId { get; private set; }

        /// <summary>
        /// 执行状态
        /// </summary>
        public ExecutionStatus Status { get; private set; }

        /// <summary>
        /// 开始执行时间
        /// </summary>
        public DateTime StartedAt { get; private set; }

        /// <summary>
        /// 结束执行时间
        /// </summary>
        public DateTime? EndedAt { get; private set; }

        /// <summary>
        /// 执行时长（毫秒）
        /// </summary>
        public long? DurationMs { get; private set; }

        /// <summary>
        /// 执行结果（JSON格式）
        /// </summary>
        public string Result { get; private set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// 错误堆栈
        /// </summary>
        public string ErrorStack { get; private set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// 私有构造函数，防止直接实例化
        /// </summary>
        protected JobExecutionLog() { }

        /// <summary>
        /// 创建任务执行日志
        /// </summary>
        public static JobExecutionLog Create(Guid jobId, string hangfireExecutionId)
        {
            var now = DateTime.UtcNow;
            return new JobExecutionLog
            {
                Id = Guid.NewGuid(),
                JobId = jobId,
                HangfireExecutionId = hangfireExecutionId,
                Status = ExecutionStatus.Running,
                StartedAt = now,
                CreatedAt = now
            };
        }

        /// <summary>
        /// 完成任务执行
        /// </summary>
        public void Complete(string result = null)
        {
            if (Status != ExecutionStatus.Running)
                throw new InvalidOperationException("任务执行状态不正确，无法标记为完成");

            var endTime = DateTime.UtcNow;
            Status = ExecutionStatus.Succeeded;
            EndedAt = endTime;
            DurationMs = (long)(endTime - StartedAt).TotalMilliseconds;
            Result = result;
        }

        /// <summary>
        /// 标记任务执行失败
        /// </summary>
        public void Fail(string errorMessage, string errorStack = null)
        {
            if (Status != ExecutionStatus.Running)
                throw new InvalidOperationException("任务执行状态不正确，无法标记为失败");

            var endTime = DateTime.UtcNow;
            Status = ExecutionStatus.Failed;
            EndedAt = endTime;
            DurationMs = (long)(endTime - StartedAt).TotalMilliseconds;
            ErrorMessage = errorMessage;
            ErrorStack = errorStack;
        }

        /// <summary>
        /// 标记任务执行被取消
        /// </summary>
        public void Cancel()
        {
            if (Status != ExecutionStatus.Running)
                throw new InvalidOperationException("任务执行状态不正确，无法标记为取消");

            var endTime = DateTime.UtcNow;
            Status = ExecutionStatus.Canceled;
            EndedAt = endTime;
            DurationMs = (long)(endTime - StartedAt).TotalMilliseconds;
        }
    }

    /// <summary>
    /// 执行状态枚举
    /// </summary>
    public enum ExecutionStatus
    {
        /// <summary>
        /// 运行中
        /// </summary>
        Running = 0,

        /// <summary>
        /// 成功
        /// </summary>
        Succeeded = 1,

        /// <summary>
        /// 失败
        /// </summary>
        Failed = 2,

        /// <summary>
        /// 取消
        /// </summary>
        Canceled = 3
    }
}