namespace HangfireJobsSys.Domain.Enums
{
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
        /// 已取消
        /// </summary>
        Canceled = 3
    }
}