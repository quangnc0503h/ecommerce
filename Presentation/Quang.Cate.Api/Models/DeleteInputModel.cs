using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quang.Cate.Api.Models
{
    public class DeleteInputModel
    {
        public List<long> Ids { get; set; }
    }

    public class SelectListItemModel
    {
        public string Value { get; set; }
        public string Text { get; set; }
    }
}