using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HangfireJobsSys.Domain.Entities;

namespace HangfireJobsSys.Domain.Services
{
    /// <summary>
    /// 任务领域服务接口
    /// </summary>
    public interface IJobService
    {
        /// <summary>
        /// 创建任务
        /// </summary>
        Task<Job> CreateJobAsync(string name, string description, string jobTypeName, string parameters, Guid createdBy);

        /// <summary>
        /// 更新任务
        /// </summary>
        Task<Job> UpdateJobAsync(Guid jobId, string name, string description, string jobTypeName, string parameters, Guid updatedBy);

        /// <summary>
        /// 获取任务详情
        /// </summary>
        Task<Job> GetJobByIdAsync(Guid jobId);

        /// <summary>
        /// 获取任务列表
        /// </summary>
        Task<IEnumerable<Job>> GetJobsAsync(JobStatus? status = null, int page = 1, int pageSize = 20);

        /// <summary>
        /// 激活任务
        /// </summary>
        Task<Job> ActivateJobAsync(Guid jobId, Guid updatedBy);

        /// <summary>
        /// 暂停任务
        /// </summary>
        Task<Job> PauseJobAsync(Guid jobId, Guid updatedBy);

        /// <summary>
        /// 删除任务
        /// </summary>
        Task DeleteJobAsync(Guid jobId, Guid updatedBy);

        /// <summary>
        /// 立即执行任务
        /// </summary>
        Task<string> ExecuteJobImmediatelyAsync(Guid jobId, Guid executedBy);

        /// <summary>
        /// 创建Cron调度计划
        /// </summary>
        Task<Schedule> CreateCronScheduleAsync(Guid jobId, string cronExpression, DateTime? startTime, DateTime? endTime, Guid createdBy);

        /// <summary>
        /// 创建间隔调度计划
        /// </summary>
        Task<Schedule> CreateIntervalScheduleAsync(Guid jobId, int intervalSeconds, DateTime? startTime, DateTime? endTime, Guid createdBy);

        /// <summary>
        /// 激活调度计划
        /// </summary>
        Task<Schedule> ActivateScheduleAsync(Guid scheduleId, Guid updatedBy);

        /// <summary>
        /// 暂停调度计划
        /// </summary>
        Task<Schedule> PauseScheduleAsync(Guid scheduleId, Guid updatedBy);

        /// <summary>
        /// 删除调度计划
        /// </summary>
        Task DeleteScheduleAsync(Guid scheduleId, Guid updatedBy);

        /// <summary>
        /// 获取任务执行日志
        /// </summary>
        Task<IEnumerable<JobExecutionLog>> GetJobExecutionLogsAsync(Guid jobId, int page = 1, int pageSize = 20);

        /// <summary>
        /// 记录任务执行开始
        /// </summary>
        Task<JobExecutionLog> LogJobExecutionStartAsync(Guid jobId, string hangfireExecutionId);

        /// <summary>
        /// 记录任务执行完成
        /// </summary>
        Task LogJobExecutionCompleteAsync(Guid logId, string result = null);

        /// <summary>
        /// 记录任务执行失败
        /// </summary>
        Task LogJobExecutionFailureAsync(Guid logId, string errorMessage, string errorStack = null);

        /// <summary>
        /// 记录任务执行取消
        /// </summary>
        Task LogJobExecutionCancelAsync(Guid logId);

        /// <summary>
        /// 调度任务
        /// </summary>
        Task<string> ScheduleJobAsync(Guid jobId, string cronExpression, DateTime? startTime, DateTime? endTime);

        /// <summary>
        /// 取消已调度的任务
        /// </summary>
        Task CancelScheduledJobAsync(string hangfireJobId);
    }
}