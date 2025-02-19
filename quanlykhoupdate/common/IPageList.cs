namespace quanlykhoupdate.common
{
    public interface IPageList<T> : IList<T>
    {
        int pageIndex { get; }
        int pageSize { get; }
        int totalCounts { get; }
        int totalPages { get; }
        bool HasPreviousPage { get; }
        bool HasNextPage { get; }
    }
}
