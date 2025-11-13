using System;
using System.Collections.Generic;

namespace HangfireJobsSys.Application.Queries.Jobs
{
    /// <summary>
    /// 任务详情数据传输对象
    /// </summary>
    public class JobDetailDto : JobDto
    {
        /// <summary>
        /// 关联的调度计划列表
        /// </summary>
        public List<ScheduleDto> Schedules { get; set; } = new List<ScheduleDto>();

        /// <summary>
        /// 执行历史记录（最近10条）
        /// </summary>
        public List<ExecutionHistoryDto> RecentExecutionHistory { get; set; } = new List<ExecutionHistoryDto>();

        /// <summary>
        /// 统计信息
        /// </summary>
        public JobStatisticsDto Statistics { get; set; } = new JobStatisticsDto();
    }

    /// <summary>
    /// 任务统计信息
    /// </summary>
    public class JobStatisticsDto
    {
        /// <summary>
        /// 总执行次数
        /// </summary>
        public int TotalExecutions { get; set; }

        /// <summary>
        /// 成功执行次数
        /// </summary>
        public int SuccessfulExecutions { get; set; }

        /// <summary>
        /// 失败执行次数
        /// </summary>
        public int FailedExecutions { get; set; }

        /// <summary>
        /// 平均执行时间（毫秒）
        /// </summary>
        public double AverageExecutionTime { get; set; }

        /// <summary>
        /// 最长执行时间（毫秒）
        /// </summary>
        public long MaxExecutionTime { get; set; }

        /// <summary>
        /// 最短执行时间（毫秒）
        /// </summary>
        public long MinExecutionTime { get; set; }
    }

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
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// 执行历史记录数据传输对象
    /// </summary>
    public class ExecutionHistoryDto
    {
        /// <summary>
        /// 执行ID
        /// </summary>
        public string ExecutionId { get; set; }

        /// <summary>
        /// 执行时间
        /// </summary>
        public DateTime ExecutionTime { get; set; }

        /// <summary>
        /// 执行状态
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 执行时间（毫秒）
        /// </summary>
        public long ExecutionDuration { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}