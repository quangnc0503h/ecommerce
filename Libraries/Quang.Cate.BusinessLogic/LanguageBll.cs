using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quang.Cate.DataAccess;
using Quang.Cate.Entities;

namespace Quang.Cate.BusinessLogic
{
    public static class LanguageBll
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="languageId"></param>
        /// <returns></returns>
        public async static Task<long> Delete(long languageId)
        {
            return await LanguageDal.Delete(new List<long>{ languageId});
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="languageIds"></param>
        /// <returns></returns>
        public async static Task<long> Delete(List<long> languageIds)
        {
            return await LanguageDal.Delete(languageIds);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="group"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public async static Task<long> Insert(Language language)
        {
            return await LanguageDal.Create(language);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="group"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public async static Task<long> Update(Language language)
        {
            return await LanguageDal.Update(language);
        }

        public async static Task<long> GetTotal( string keyword)
        {
            return await LanguageDal.GetTotal( keyword);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageNumber"></param>
        /// <param name="orderby"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public async static Task<IEnumerable<Language>> GetPaging(int pageSize, int pageNumber, string orderby,  string keyword)
        {
            return await LanguageDal.GetPaging(pageSize, pageNumber, orderby,  keyword);
        }
        public async static Task<Language> GetOneLanguage(long languageId)
        {
            return await LanguageDal.GetLanguageById(languageId);
        }
    }
}
