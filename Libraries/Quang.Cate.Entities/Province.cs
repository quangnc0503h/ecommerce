using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quang.Cate.Entities
{
    public class Province
    {
        /// <summary>
        /// 
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long CountryId { get; set; }
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
        public bool Published { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int DisplayOrder { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool Deleted { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? CreatedOn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? UpdatedOn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UserName { get; set; }
    }
}
