using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quang.Auth.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class Group
    {
        /// <summary>
        /// 
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long? ParentId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ParentName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int TotalMembers { get; set; }
    }
}