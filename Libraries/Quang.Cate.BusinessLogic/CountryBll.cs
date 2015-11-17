using Quang.Cate.DataAccess;
using Quang.Cate.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quang.Cate.BusinessLogic
{
    /// <summary>
    /// 
    /// </summary>
    public static class CountryBll
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<Country>> GetAll(string keyword)
        {
            return await CountryDal.GetAll(keyword);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static async Task<IEnumerable<Country>> GetAll()
        {
            return await CountryDal.GetAll();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<Country> GetCountryById(long id)
        {
            return await CountryDal.GetCountryById(id);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static async Task<long> Create(Country item)
        {
            return await CountryDal.Create(item);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static async Task<int> Update(Country item)
        {
            return await CountryDal.Update(item);
        }
        public static async Task<long> GetTotal(string keyword)
        {
            return await CountryDal.GetTotal(keyword);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageNumber"></param>
        /// <param name="orderBy"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<Country>> GetAll(int pageSize, int pageNumber, string orderBy, string keyword)
        {
            return await CountryDal.GetAll(pageSize, pageNumber, orderBy, keyword);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static async Task<int> Delete(List<long> ids)
        {
            return await CountryDal.Delete(ids);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static async Task<int> CountAll()
        {
            return await CountryDal.CountAll();
        }
    }
}
