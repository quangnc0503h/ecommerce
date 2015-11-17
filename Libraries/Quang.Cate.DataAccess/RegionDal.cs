using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Quang.Cate.Entities;

namespace Quang.Cate.DataAccess
{
    public static class RegionDal
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="countryId"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<Region>> GetAll(string keyword, long? countryId)
        {
            IEnumerable<Region> results;
            var parametters = new DynamicParameters();
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangCateConn))
            {
                var param = "%" + Utils.EncodeForLike(keyword) + "%";
                string sql = "SELECT * FROM Regions WHERE Name LIKE @param";
                parametters.Add("param", param);
                if (countryId.HasValue)
                {
                    sql += " AND CountryID=@countryId";
                    parametters.Add("countryId", countryId.Value);
                }
                var data = await conn.QueryAsync<Region>(sql, parametters);
                results = data.ToList();
            }

            return results;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static async Task<IEnumerable<Region>> GetAll()
        {
            IEnumerable<Region> results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangCateConn))
            {
                const string sql = "SELECT * FROM Regions";
                var data = await conn.QueryAsync<Region>(sql);
                results = data.ToList();
            }

            return results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<Region> GetRegionById(long id)
        {
            Region results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangCateConn))
            {
                const string sql = "SELECT * FROM Regions WHERE Id=@id";
                var data = await conn.QueryAsync<Region>(sql, new { id });
                results = data.ToList().First();
            }

            return results;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static async Task<long> Create(Region item)
        {
            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangCateConn))
            {
                const string sql = @"INSERT INTO Regions(Name,ShortName,Published,DisplayOrder, Description) values(@Name,@ShortName,@Published,@DisplayOrder,@Description); 
                               SELECT CONVERT(LAST_INSERT_ID() , UNSIGNED INTEGER) AS id;";
                var id = await conn.QueryAsync<ulong>(sql, item);
                results = (long)id.Single();
            }

            return results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static async Task<int> Update(Region item)
        {
            int results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangCateConn))
            {
                string sql = @"update  Regions set Name=@Name,
                                                ShortName=@ShortName,                                               
                                                Published=@Published,
                                                DisplayOrder=@DisplayOrder,
                                                Description = @Description 
                                where Id=@Id";
                results = await conn.ExecuteAsync(sql, item);

            }

            return results;
        }

        public static async Task<long> GetTotal(string keyword, long? countryId)
        {
            var parametters = new DynamicParameters();
            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangCateConn))
            {
                var param = "%" + Utils.EncodeForLike(keyword) + "%";
                 string sql = "select count(id) from Regions where Name LIKE @param";
                parametters.Add("param", param);
                if (countryId.HasValue)
                {
                    sql += " AND CountryID=@countryId";
                    parametters.Add("countryId", countryId.Value);
                }
                var data = await conn.QueryAsync<long>(sql, parametters);
                results = data.FirstOrDefault();
            }

            return results;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageNumber"></param>
        /// <param name="orderBy"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<Region>> GetAll(int pageSize, int pageNumber, string orderBy, string keyword)
        {
            IEnumerable<Region> results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangCateConn))
            {
                var param = "%" + Utils.EncodeForLike(keyword) + "%";
                orderBy = string.IsNullOrEmpty(orderBy) ? "id" : orderBy;
                var rowNumber = pageSize * pageNumber;
                const string sql = @"SELECT * FROM Regions WHERE Name LIKE @param 
                                                      ORDER BY @OrderBy 
                                                      LIMIT @RowNumber,@PageSize ";
                var data = await conn.QueryAsync<Region>(sql, new { param, orderBy, rowNumber, pageSize });
                results = data.ToList();
            }

            return results;
        }

        public static async Task<int> Delete(List<long> ids)
        {
            int results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangCateConn))
            {
                const string sql = @"delete from Regions WHERE Id in @ids";
                results = await conn.ExecuteAsync(sql, new { ids = ids.ToArray() });
            }

            return results;
        }

        public static async Task<int> CountAll()
        {
            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangCateConn))
            {
                const string sql = "select count(*) from Regions";
                var data = await conn.QueryAsync<long>(sql);
                results = data.FirstOrDefault();
            }

            return (int)results;
        }
    }
}
