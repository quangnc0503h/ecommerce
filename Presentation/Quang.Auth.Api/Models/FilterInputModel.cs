namespace Quang.Auth.Api.Models
{
    public class FilterInputModel
    {
        public string Keyword { get; set; }

        public int PageSize { get; set; }

        public int PageNumber { get; set; }
    }

    public class GetByIdInputModel
    {
        public long Id { get; set; }
    }

    public class ResultUpdateOutputModel
    {
        public int Status { get; set; }
    }
}