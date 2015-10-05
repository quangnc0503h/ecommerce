using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quang.Auth.Api.Models
{
    public class DataSourceResultModel
    {
        public object ExtraData { get; set; }

        public IEnumerable Data { get; set; }

        public object Errors { get; set; }

        public long Total { get; set; }
    }
}