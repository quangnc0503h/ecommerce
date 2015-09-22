using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quang.Auth.Entities;
namespace Quang.Auth.Api.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class FilterGroupInputModel : FilterInputModel
    {
        public long? ParentId { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class GetListGrouputputModel
    {
        public IEnumerable<Group> Groups { get; set; }

        public long TotalCount { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class GetOneGroupOutputModel
    {
        public Group Group { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class GroupInputModel
    {
        public long Id { get; set; }

        public long? ParentId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class DeleteGroupInputModel
    {
        public List<long> Ids { get; set; }
    }

    public class GroupOutputModel
    {
        public int Status { get; set; }
        public string Messages { get; set; }
    }
}