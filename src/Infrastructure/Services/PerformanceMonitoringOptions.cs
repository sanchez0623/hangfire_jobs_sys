using System;

namespace HangfireJobsSys.Infrastructure.Services
{
    /// <summary>
    /// 性能监控服务配置选项
    /// </summary>
    public class PerformanceMonitoringOptions
    {
        /// <summary>
        /// 性能数据保留天数
        /// </summary>
        public int DataRetentionDays { get; set; } = 90;
        
        /// <summary>
        /// 历史数据迁移阈值天数
        /// </summary>
        public int HistoryMigrationThresholdDays { get; set; } = 30;
        
        /// <summary>
        /// 每次迁移的批量大小
        /// </summary>
        public int BatchSize { get; set; } = 1000;
        
        /// <summary>
        /// 清理任务的Cron表达式
        /// </summary>
        public string CleanupCronExpression { get; set; } = "0 2 * * *"; // 每天凌晨2点
        
        /// <summary>
        /// 性能监控告警阈值配置
        /// </summary>
        public AlertThresholds AlertThresholds { get; set; } = new AlertThresholds();
    }
    
    /// <summary>
    /// 告警阈值配置
    /// </summary>
    public class AlertThresholds
    {
        /// <summary>
        /// 最大执行时间（毫秒）
        /// </summary>
        public double MaxExecutionTimeMs { get; set; } = 30000;
        
        /// <summary>
        /// 失败率阈值百分比
        /// </summary>
        public double FailureRatePercentage { get; set; } = 10.0;
        
        /// <summary>
        /// 超时百分比
        /// </summary>
        public double TimeoutPercentage { get; set; } = 5.0;
    }
}