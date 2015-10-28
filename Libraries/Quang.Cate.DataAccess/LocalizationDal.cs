using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Quang.Cate.Entities;

namespace Quang.Cate.DataAccess
{
    public static class LocalizationDal
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static async Task<int> Delete(List<long> ids)
        {
            int results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangCateConn))
            {
                const string sql = @"delete from LocaleStringResources WHERE Id in @ids";
                results = await conn.ExecuteAsync(sql, new { ids = ids.ToArray() });
            }

            return results;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="localeStringResourceId"></param>
        /// <returns></returns>
        public static async Task<LocaleStringResource> GetLocaleStringResourceById(long localeStringResourceId)
        {
            LocaleStringResource results;
            const string commandText = "SELECT * FROM LocaleStringResources WHERE Id=@id";
            var parameters = new Dictionary<string, object> { { "@id", localeStringResourceId } };

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangCateConn))
            {

                var data = await conn.QueryAsync<LocaleStringResource>(commandText, parameters);
                results = data.FirstOrDefault();
            }


            return results;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="languageId"></param>
        /// <returns></returns>
        public static async Task< LocaleStringResource> GetLocaleStringResourceByName(string resourceName, long languageId)
        {
            LocaleStringResource results;
            const string commandText = "SELECT * FROM LocaleStringResources WHERE ResourceName=@resourceName AND LanguageId=@languageId ORDER BY ResourceName";
            var parameters = new Dictionary<string, object> { { "@resourceName", resourceName }, { "@languageId",languageId } };
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangCateConn))
            {

                var data = await conn.QueryAsync<LocaleStringResource>(commandText, parameters);
                results = data.FirstOrDefault();
            }
            return results;
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
            const string commandText = "SELECT * FROM LocaleStringResources WHERE ResourceName like @resourceName AND LanguageId=@languageId ORDER BY ResourceName LIMIT @rowNumber,@pageSize";
            IEnumerable<LocaleStringResource> results;
            var parameters = new Dictionary<string, object>();
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangCateConn))
            {
                parameters.Add("@resourceName", "%" + Utils.EncodeForLike(resourceName) + "%");
                parameters.Add("@languageId", languageId);
                parameters.Add("@rowNumber", pageSize * pageNumber);
                parameters.Add("@pageSize", pageSize);


                var data = await conn.QueryAsync<LocaleStringResource>(commandText, parameters);
                results = data.ToList();
            }

            return results;
        }
        /// <summary>
        /// Get total record locale string resources
        /// </summary>
        /// <param name="languageId"></param>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        public static async Task<long> GetTotal(long languageId, string resourceName)
        {
            long results;
            var parameters = new Dictionary<string, object>();
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangCateConn))
            {
                var param = "%" + Utils.EncodeForLike(resourceName) + "%";
                const string sql = "select count(id) from LocaleStringResources where ResourceName LIKE @param AND LanguageId=@languageId";
                parameters.Add("@resourceName", "%" + Utils.EncodeForLike(resourceName) + "%");
                parameters.Add("@languageId", languageId);
                var data = await conn.QueryAsync<long>(sql, parameters);
                results = data.FirstOrDefault();
            }

            return results;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static async Task<int> CountAll()
        {
            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangCateConn))
            {
                const string sql = "select count(*) from LocaleStringResources";
                var data = await conn.QueryAsync<long>(sql);
                results = data.FirstOrDefault();
            }

            return (int)results;
        }
    }
}
