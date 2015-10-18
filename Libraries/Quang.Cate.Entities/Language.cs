namespace Quang.Cate.Entities
{
    public class Language
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
