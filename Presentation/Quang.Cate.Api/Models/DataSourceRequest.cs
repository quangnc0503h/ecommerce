namespace Quang.Cate.Api.Models
{
    public class DataSourceRequest
    {
        public string Keyword { get; set; }
        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public DataSourceRequest()
        {
            PageNumber = 1;
            PageSize = 10;
        }
    }
    public class GetOneInputModel
    {
        public long Id { get; set; }
    }
}