namespace quanlykhoupdate.common
{
    [Serializable]
    public partial class PageList<T> : List<T>, IPageList<T>
    {
        public PageList()
        {

        }
        private void Initialize(IEnumerable<T> source, int pageIndexs, int pageSizes, int? totalCount = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (pageSizes <= 0)
                pageSizes = 1;

            totalCounts = totalCount ?? source.Count();

            if (pageSizes > 0)
            {
                totalPages = totalCounts / pageSizes;
                if (totalCounts % pageSizes > 0)
                    totalPages++;
            }

            pageSize = pageSizes;
            pageIndex = pageIndexs;
            source = totalCount == null ? source.Skip(pageIndexs * pageSizes).Take(pageSizes) : source;
            AddRange(source);


        }

        private Task InitializeAsync(IQueryable<T> source, int pageIndexs, int pageSizes, int? totalCount = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (pageSizes <= 0)
                pageSizes = 1;
            pageIndexs = pageIndexs > 0 ? pageIndexs - 1 : 0;

            totalCounts = totalCount ?? source.Count();

            if (totalCount == null)
            {
                source = source.Skip(pageIndexs * pageSizes).Take(pageSizes);
            }
            AddRange(source);

            if (pageSizes > 0)
            {
                totalPages = totalCounts / pageSizes;
                if (totalCounts % pageSizes > 0)
                    totalPages++;
            }

            pageIndex = pageIndexs;
            pageSize = pageSizes;
            return Task.CompletedTask;
        }

        public PageList(IEnumerable<T> source, int pageIndexs, int pageSizes)
        {
            Initialize(source, pageIndexs, pageSizes);
        }

        public PageList(IEnumerable<T> source, int pageIndexs, int pagerSizes, int totalCount)
        {
            Initialize(source, pageIndexs, pagerSizes, totalCount);
        }

        public static async Task<PageList<T>> Create(IQueryable<T> source, int pageIndexs, int pageSizes)
        {
            var pageLists = new PageList<T>();
            await pageLists.InitializeAsync(source, pageIndexs, pageSizes);
            return pageLists;
        }

        public static async Task<PageList<T>> PandingList(IQueryable<T> source, int pageIndexs, int pageSizes)
        {
            var pageLists = new PageList<T>();
            await pageLists.InitializeAsync(source, pageIndexs, pageSizes);
            return new PageList<T>
            {
                pageIndex = pageLists.pageIndex,
                pageSize = pageLists.pageSize,
                totalCounts = pageLists.totalCounts,
                totalPages = pageLists.totalPages,
                Data = pageLists
            };
        }
        public List<T> Data { get; protected set; }
        public int pageIndex { get; protected set; }
        public int pageSize { get; protected set; }
        public int totalCounts { get; protected set; }
        public int totalPages { get; protected set; }
        public bool HasPreviousPage
        {
            get { return (pageIndex > 0); }
        }
        public bool HasNextPage
        {
            get { return (pageIndex + 1 < totalPages); }
        }
    }
}
