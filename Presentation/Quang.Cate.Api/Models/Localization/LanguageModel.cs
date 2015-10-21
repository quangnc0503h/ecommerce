using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quang.Cate.Api.Models.Localization
{
    public class LanguageModel
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
        public string LanguageCulture { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UniqueSeoCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool Published { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int DisplayOrder { get; set; }
    }
}