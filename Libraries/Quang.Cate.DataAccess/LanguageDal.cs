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
                const string sql = "SELECT * FROM Language WHERE Name LIKE @param";
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
                const string sql = "SELECT * FROM Language";
                var data = await conn.QueryAsync<Language>(sql);
                results = data.ToList();
            }

            return results;
        }


        public static async Task<Language> GetLanguageById(long id)
        {
            Language results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangCateConn))
            {
                const string sql = "SELECT * FROM Country WHERE Id=@id";
                var data = await conn.QueryAsync<Language>(sql, new { id });
                results = data.ToList().First();
            }

            return results;
        }


        public static async Task<long> Create(Language item)
        {
            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangCateConn))
            {
                const string sql = @"INSERT INTO Country(Name,ShortName,Published,DisplayOrder, Description) values(@Name,@ShortName,@Published,@DisplayOrder,@Description); 
                               SELECT CONVERT(LAST_INSERT_ID() , UNSIGNED INTEGER) AS id;";
                var id = await conn.QueryAsync<ulong>(sql, item);
                results = (long)id.Single();
            }

            return results;
        }


        public static async Task<int> Update(Language item)
        {
            int results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangCateConn))
            {
                string sql = @"update  Country set Name=@Name,
                                                ShortName=@ShortName,                                               
                                                Published=@Published,
                                                DisplayOrder=@DisplayOrder,
                                                Description = @Description 
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
                const string sql = "select count(id) from Language where Name LIKE @param";
                var data = await conn.QueryAsync<long>(sql, new { param });
                results = data.FirstOrDefault();
            }

            return results;
        }

        public static async Task<IEnumerable<Language>> GetAll(int pageSize, int pageNumber, string orderBy, string keyword)
        {
            IEnumerable<Language> results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangCateConn))
            {
                var param = "%" + Utils.EncodeForLike(keyword) + "%";
                orderBy = string.IsNullOrEmpty(orderBy) ? "id" : orderBy;
                var rowNumber = pageSize * pageNumber;
                const string sql = @"SELECT * FROM Language WHERE Name LIKE @param 
                                                      ORDER BY @OrderBy 
                                                      LIMIT @RowNumber,@PageSize ";
                var data = await conn.QueryAsync<Language>(sql, new { param, orderBy, rowNumber, pageSize });
                results = data.ToList();
            }

            return results;
        }

        public static async Task<int> Delete(List<long> ids)
        {
            int results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangCateConn))
            {
                const string sql = @"delete from Language WHERE Id in @ids";
                results = await conn.ExecuteAsync(sql, new { ids = ids.ToArray() });
            }

            return results;
        }

        public static async Task<int> CountAll()
        {
            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangCateConn))
            {
                const string sql = "select count(*) from Language";
                var data = await conn.QueryAsync<long>(sql);
                results = data.FirstOrDefault();
            }

            return (int)results;
        }
    }
}
