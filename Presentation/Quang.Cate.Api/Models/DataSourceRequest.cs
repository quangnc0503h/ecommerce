namespace Quang.Cate.Api.Models
{
    public class DataSourceRequest
    {
        public string Keyword { get; set; }
        public int Page { get; set; }

        public int PageSize { get; set; }

        public DataSourceRequest()
        {
            Page = 1;
            PageSize = 10;
        }
    }
    public class GetOneInputModel
    {
        public long Id { get; set; }
    }
}