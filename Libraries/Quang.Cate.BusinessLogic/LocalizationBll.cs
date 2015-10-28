using Quang.Cate.DataAccess;
using Quang.Cate.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quang.Cate.BusinessLogic
{
    public static class LocalizationBll
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static async Task<int> Delete(List<long> ids)
        {
            return await LocalizationDal.Delete(ids);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="localeStringResourceId"></param>
        /// <returns></returns>
        public static async Task<LocaleStringResource> GetLocaleStringResourceById(long localeStringResourceId)
        {
            return await LocalizationDal.GetLocaleStringResourceById(localeStringResourceId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="languageId"></param>
        /// <returns></returns>
        public static async Task<LocaleStringResource> GetLocaleStringResourceByName(string resourceName, long languageId)
        {
            return await LocalizationDal.GetLocaleStringResourceByName(resourceName, languageId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageNumber"></param>
        /// <param name="languageId"></param>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<LocaleStringResource>> GetPaging(int pageSize, int pageNumber,
            long languageId, string resourceName)
        {
            return await LocalizationDal.GetPaging(pageSize, pageNumber, languageId, resourceName);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="languageId"></param>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        public static async Task<long> GetTotal(long languageId, string resourceName)
        {
            return await LocalizationDal.GetTotal(languageId, resourceName);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static async Task<int> CountAll()
        {
            return await LocalizationDal.CountAll();
        }
    }
}
