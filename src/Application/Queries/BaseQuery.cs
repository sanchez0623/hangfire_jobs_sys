namespace HangfireJobsSys.Application.Queries
{
    /// <summary>
    /// 基础分页查询
    /// </summary>
    public class BaseQuery
    {
        /// <summary>
        /// 当前页码
        /// </summary>
        public int PageIndex { get; set; } = 1;

        /// <summary>
        /// 每页大小
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// 排序字段
        /// </summary>
        public string SortField { get; set; } = "CreatedAt";

        /// <summary>
        /// 排序方向（asc/desc）
        /// </summary>
        public string SortDirection { get; set; } = "desc";
    }
}