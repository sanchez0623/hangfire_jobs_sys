using System;

namespace HangfireJobsSys.Domain.Services
{
    /// <summary>
    /// 缓存键工厂，用于统一管理缓存键的生成
    /// </summary>
    public static class CacheKeyFactory
    {
        private const string JobPrefix = "job:";
        private const string JobListPrefix = "jobs:";
        private const string SchedulePrefix = "schedule:";
        private const string ScheduleListPrefix = "schedules:";
        private const string JobTypePrefix = "jobtype:";
        private const string StatsPrefix = "stats:";

        /// <summary>
        /// 获取任务详情缓存键
        /// </summary>
        /// <param name="jobId">任务ID</param>
        /// <returns>缓存键</returns>
        public static string GetJobKey(Guid jobId) => $"{JobPrefix}{jobId}";

        /// <summary>
        /// 获取任务列表缓存键（带过滤条件）
        /// </summary>
        /// <param name="status">状态过滤</param>
        /// <param name="jobTypeName">任务类型过滤</param>
        /// <param name="page">页码</param>
        /// <param name="pageSize">每页大小</param>
        /// <returns>缓存键</returns>
        public static string GetJobListKey(string status = null, string jobTypeName = null, int page = 1, int pageSize = 20)
            => $"{JobListPrefix}status:{status ?? "all"}:type:{jobTypeName ?? "all"}:page:{page}:size:{pageSize}";

        /// <summary>
        /// 获取任务执行日志缓存键
        /// </summary>
        /// <param name="jobId">任务ID</param>
        /// <param name="page">页码</param>
        /// <param name="pageSize">每页大小</param>
        /// <returns>缓存键</returns>
        public static string GetJobLogsKey(Guid jobId, int page = 1, int pageSize = 20)
            => $"{JobPrefix}{jobId}:logs:page:{page}:size:{pageSize}";

        /// <summary>
        /// 获取调度计划缓存键
        /// </summary>
        /// <param name="scheduleId">调度计划ID</param>
        /// <returns>缓存键</returns>
        public static string GetScheduleKey(Guid scheduleId) => $"{SchedulePrefix}{scheduleId}";

        /// <summary>
        /// 获取任务的调度计划列表缓存键
        /// </summary>
        /// <param name="jobId">任务ID</param>
        /// <returns>缓存键</returns>
        public static string GetSchedulesByJobKey(Guid jobId) => $"{ScheduleListPrefix}job:{jobId}";

        /// <summary>
        /// 获取任务类型缓存键
        /// </summary>
        /// <param name="jobTypeName">任务类型名称</param>
        /// <returns>缓存键</returns>
        public static string GetJobTypeKey(string jobTypeName) => $"{JobTypePrefix}{jobTypeName}";

        /// <summary>
        /// 获取所有任务类型列表缓存键
        /// </summary>
        /// <returns>缓存键</returns>
        public static string GetAllJobTypesKey() => $"{JobTypePrefix}all";

        /// <summary>
        /// 获取系统统计信息缓存键
        /// </summary>
        /// <param name="statType">统计类型</param>
        /// <returns>缓存键</returns>
        public static string GetStatsKey(string statType) => $"{StatsPrefix}{statType}";

        /// <summary>
        /// 获取任务优先级队列配置缓存键
        /// </summary>
        /// <returns>缓存键</returns>
        public static string GetPriorityQueuesConfigKey() => "config:priority_queues";

        /// <summary>
        /// 获取任务类型模式匹配的缓存键前缀
        /// </summary>
        /// <param name="pattern">匹配模式</param>
        /// <returns>缓存键前缀</returns>
        public static string GetPatternKey(string pattern) => pattern;
    }
}