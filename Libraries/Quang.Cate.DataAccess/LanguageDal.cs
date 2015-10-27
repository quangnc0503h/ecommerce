using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Quang.Cate.Entities;

namespace Quang.Cate.DataAccess
{
    public static class LanguageDal
    {
        public static async Task<IEnumerable<Language>> GetAll(string keyword)
        {
            IEnumerable<Language> results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangCateConn))
            {
                var param = "%" + Utils.EncodeForLike(keyword) + "%";
                const string sql = "SELECT * FROM languages WHERE Name LIKE @param";
                var data = await conn.QueryAsync<Language>(sql, new { param });
                results = data.ToList();
            }

            return results;
        }

        public static async Task<IEnumerable<Language>> GetAll()
        {
            IEnumerable<Language> results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangCateConn))
            {
                const string sql = "SELECT * FROM languages";
                var data = await conn.QueryAsync<Language>(sql);
                results = data.ToList();
            }

            return results;
        }


        public static async Task<Language> GetLanguageById(long id)
        {
            Language results;
            const string commandText = "SELECT * FROM languages WHERE Id=@id";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@id", id } };

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangCateConn))
            {

                var data = await conn.QueryAsync<Language>(commandText, parameters);
                results = data.FirstOrDefault();
            }

         
            return results;
        }


        public static async Task<long> Create(Language item)
        {
            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangCateConn))
            {
                const string sql = @"INSERT INTO languages(Name,LanguageCulture,Published,DisplayOrder, UniqueSeoCode) values(@Name,@LanguageCulture,@Published,@DisplayOrder,@UniqueSeoCode); 
                               SELECT CONVERT(LAST_INSERT_ID() , UNSIGNED INTEGER) AS id;";
                results = await conn.ExecuteAsync(sql, item);
                 
            }

            return results;
        }


        public static async Task<int> Update(Language item)
        {
            int results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangCateConn))
            {
                string sql = @"update  languages set Name=@Name,
                                                LanguageCulture=@LanguageCulture,
                                                UniqueSeoCode = @UniqueSeoCode ,                                               
                                                Published=@Published,
                                                DisplayOrder=@DisplayOrder
                                where Id=@Id";
                results = await conn.ExecuteAsync(sql, item);

            }

            return results;
        }

        public static async Task<long> GetTotal(string keyword)
        {
            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangCateConn))
            {
                var param = "%" + Utils.EncodeForLike(keyword) + "%";
                const string sql = "select count(id) from languages where Name LIKE @param";
                var data = await conn.QueryAsync<long>(sql, new { param });
                results = data.FirstOrDefault();
            }

            return results;
        }

        public static async Task<IEnumerable<Language>> GetPaging(int pageSize, int pageNumber, string orderBy, string keyword)
        {
            IEnumerable<Language> results;
            var parameters = new Dictionary<string, object>();
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangCateConn))
            {
                parameters.Add("@param", "%" + Utils.EncodeForLike(keyword) + "%");
                parameters.Add("@rowNumber", pageSize * pageNumber);
                parameters.Add("@pageSize", pageSize);
               // orderBy = string.IsNullOrEmpty(orderBy) ? "id" : orderBy;
                var rowNumber = pageSize * pageNumber;
                const string sql = @"SELECT * FROM languages WHERE Name LIKE @param 
                                                      ORDER BY DisplayOrder 
                                                      LIMIT @rowNumber,@pageSize ";
                var data = await conn.QueryAsync<Language>(sql, parameters);
                results = data.ToList();
            }

            return results;
        }

        public static async Task<int> Delete(List<long> ids)
        {
            int results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangCateConn))
            {
                const string sql = @"delete from languages WHERE Id in @ids";
                results = await conn.ExecuteAsync(sql, new { ids = ids.ToArray() });
            }

            return results;
        }

        public static async Task<int> CountAll()
        {
            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangCateConn))
            {
                const string sql = "select count(*) from languages";
                var data = await conn.QueryAsync<long>(sql);
                results = data.FirstOrDefault();
            }

            return (int)results;
        }
    }
}
