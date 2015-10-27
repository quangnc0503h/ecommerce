using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quang.Cate.Entities
{
    public class District
    {
        /// <summary>
        /// 
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ShortName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long ProvinceId { get; set; }
    }
}
