using System;

namespace HangfireJobsSys.Application.Queries.Logs
{
    /// <summary>
    /// 任务执行日志数据传输对象
    /// </summary>
    public class JobExecutionLogDto
    {
        /// <summary>
        /// 执行日志ID
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
        /// 执行ID（Hangfire Job Id）
        /// </summary>
        public string ExecutionId { get; set; }

        /// <summary>
        /// 执行状态
        /// </summary>
        public string ExecutionStatus { get; set; }

        /// <summary>
        /// 开始执行时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 结束执行时间
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 执行时长（毫秒）
        /// </summary>
        public long ExecutionDuration { get; set; }

        /// <summary>
        /// 执行结果
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 重试次数
        /// </summary>
        public int RetryCount { get; set; }

        /// <summary>
        /// 队列名称
        /// </summary>
        public string QueueName { get; set; }

        /// <summary>
        /// 执行服务器名称
        /// </summary>
        public string ServerName { get; set; }
    }
}