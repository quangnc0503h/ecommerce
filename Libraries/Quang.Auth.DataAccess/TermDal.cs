
using Quang.Auth.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace Quang.Auth.DataAccess
{
    /// <summary>
    /// 
    /// </summary>
    public static class TermDal
    {   
        /// <summary>
        /// 
        /// </summary>
        /// <param name="termId"></param>
        /// <returns></returns>
                     
        public async static Task<long> Delete(long termId)
        {
            string commandText = "Delete from Terms where Id = @id";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@id", termId);
            long results;
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {
                
                var data = await conn.ExecuteAsync(commandText, parameters);
                results = data;
            }
            return results;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Ids"></param>
        /// <returns></returns>
        public async static Task<long> Delete(IEnumerable<long> Ids)
        {
            string commandText = "Delete from Terms where Id in (" + string.Join(",", Ids.ToArray()) + ")";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            long results;
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var data = await conn.ExecuteAsync(commandText, parameters);
                results = data;
                commandText = "Delete from UserTerms where TermId in (" + string.Join(",", Ids.ToArray()) + ")";
                await conn.ExecuteAsync(commandText, new Dictionary<string, object>());

                // Remove terms from GroupTerms
                commandText = "Delete from GroupTerms where TermId in (" + string.Join(",", Ids.ToArray()) + ")";
                await conn.ExecuteAsync(commandText, new Dictionary<string, object>());

            }
            return results;
            
        }

        public async static Task<long> Insert(Term term)
        {
            string commandText = "Insert into Terms (Id, RoleKey, Name, Description) values (@id, @roleKey, @name, @description)";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            if (term.Id > 0)
            {
                parameters.Add("@id", term.Id);
            } else
            {
                parameters.Add("@id", null);
            }
            parameters.Add("@name", term.Name);
            parameters.Add("@roleKey", term.RoleKey);
            parameters.Add("@description", term.Description);

            long results;
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var data = await conn.ExecuteAsync(commandText, parameters);
                results = data;
            }
            return results;
        }

        public async static Task<long> Update(Term term)
        {
            string commandText = "Update Terms set RoleKey = @roleKey, Name = @name, Description = @description  where Id = @id";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@id", term.Id);
            parameters.Add("@roleKey", term.RoleKey);
            parameters.Add("@name", term.Name);
            parameters.Add("@description", term.Description);

            long results;
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var data = await conn.ExecuteAsync(commandText, parameters);
                results = data;
            }
            return results;
        }

        public async static Task<IEnumerable<Term>> GetAllTerms()
        {
            IList<Term> terms = new List<Term>();
            string commandText = "Select * from Terms order by RoleKey";

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {
                var data = await conn.QueryAsync<Term>(commandText, new { });
                terms = data.ToList();
            }            
            return terms;
        }

        public async static Task<Term> GetOneTerm(long termId)
        {
            Term term = null;
            string commandText = "Select * from Terms where Id = @id";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@id", termId } };
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {
                var data = await conn.QueryAsync<Term>(commandText, parameters);
                term = data.First();
            }            
            return term;
        }

        public async static Task<long> GetTotal(string keyword)
        {
            var sql = "select count(*) from Terms where Name LIKE @param";

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@param", "%" + Utils.EncodeForLike(keyword) + "%");

            long results;
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var data = await conn.QueryAsync<long>(sql, parameters);
                results = data.FirstOrDefault();
            }
            return results;
        }

        public async static Task<IEnumerable<Term>> GetPaging(int pageSize, int pageNumber, string keyword)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            var sql = "select * from Terms where Name LIKE @param";

            parameters.Add("@param", "%" + Utils.EncodeForLike(keyword) + "%");

            sql += " order by RoleKey limit @rowNumber, @pageSize";
            parameters.Add("@rowNumber", pageSize * pageNumber);
            parameters.Add("@pageSize", pageSize);

            List<Term> terms = new List<Term>();
            
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var data = await conn.QueryAsync<Term>(sql, parameters);
                terms = data.ToList();
            }
            
            return terms;
        }

        public async static Task<IEnumerable<Term>> GetTermsByGroup(long groupId)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            var sql = "";
            sql += "select t.*";
            sql += "from GroupTerms ut ";
            sql += "inner join Terms t on t.Id = ut.TermId ";
            sql += "where ut.GroupId = @groupId ";
            sql += "order by RoleKey";

            parameters.Add("@groupId", groupId);

            List<Term> terms = new List<Term>();

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var data = await conn.QueryAsync<Term>(sql, parameters);
                terms = data.ToList();
            }

            return terms;
        }

        public async static Task<IDictionary<Term, bool>> GetTermsByUser(long userId)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            var sql = "";
            sql += "select t.*,ut.IsAccess ";
            sql += "from UserTerms ut ";
            sql += "inner join Terms t on t.Id = ut.TermId ";
            sql += "where ut.UserId = @userId ";
            sql += "order by RoleKey";

            parameters.Add("@userId", userId);

            IDictionary<Term, bool> items = new Dictionary<Term, bool>();
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var rows = await conn.QueryAsync(sql, parameters);
                foreach (var row in rows)
                {
                    var term = new Term();
                    term.Id = Int32.Parse(row["Id"]);
                    term.RoleKey = row["RoleKey"];
                    term.Name = row["Name"];
                    term.Description = row["Description"];

                    var isAccess = false;
                    if (!string.IsNullOrEmpty(row["IsAccess"]))
                    {
                        //isAccess = Int32.Parse(row["IsAccess"]) != 0 ? true : false;
                        isAccess = bool.Parse(row["IsAccess"]);
                    }

                    items.Add(term, isAccess);
                }
            }

            
            

            return items;
        }
        
        public async static Task<IDictionary<string, bool>> GetUsersByTerm(long termId)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            var sql = "";
            sql += "select u.UserName,ut.IsAccess ";
            sql += "from UserTerms ut ";
            sql += "inner join Users u on u.Id = ut.UserId ";
            sql += "where ut.TermId = @termId ";

            parameters.Add("@termId", termId);

            IDictionary<string, bool> items = new Dictionary<string, bool>();

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var rows = await conn.QueryAsync(sql, parameters);
                foreach (var row in rows)
                {
                    var isAccess = false;
                    if (!string.IsNullOrEmpty(row["IsAccess"]))
                    {
                        isAccess = bool.Parse(row["IsAccess"]);
                    }

                    items.Add(row["UserName"], isAccess);
                }

            }


            
            return items;
        }

        public async static Task<IEnumerable<Term>> GetGroupTermsBelongToUser(long userId)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            IList<Term> groupTerms = new List<Term>();

            string sql = "";
            sql += "select t.* ";
            sql += "from Terms t ";
            sql += "inner join (";
            sql += "    select distinct gt.TermId ";
            sql += "    from GroupUsers as gu ";
            sql += "    inner join GroupTerms as gt on gt.GroupId = gu.GroupId ";
            sql += "    where gu.UserId = @userId ";
            sql += ") as t1 on t1.TermId = t.Id";

            parameters.Add("@userId", userId);

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var rows = await conn.QueryAsync(sql, parameters);
                foreach (var row in rows)
                {
                    var term = new Term();
                    term.Id = Int32.Parse(row["Id"]);
                    term.Name = row["Name"];
                    term.RoleKey = row["RoleKey"];
                    term.Description = row["Description"];

                    groupTerms.Add(term);
                }


            }



            return groupTerms;
        }

        public async static Task<IDictionary<Term, bool>> GetUserTermsWithGroupAccess(long userId)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            IDictionary<Term, bool> userTerms = new Dictionary<Term, bool>();
            string sql = "";
            sql += "select t.*, ";
            sql += "case when t1.TermId is not null then 1 else 0 end as IsAccess ";
            sql += "from Terms t ";
            sql += "left join (";
            sql += "    select distinct gt.TermId ";
            sql += "    from GroupUsers as gu ";
            sql += "    inner join GroupTerms as gt on gt.GroupId = gu.GroupId ";
            sql += "    where gu.UserId = @userId ";
            sql += ") as t1 on t1.TermId = t.Id ";
            sql += "order by t.RoleKey";

            parameters.Add("@userId", userId);

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var rows = await conn.QueryAsync(sql, parameters);
                foreach (var row in rows)
                {
                    var term = new Term();
                    term.Id = Int32.Parse(row["Id"]);
                    term.Name = row["Name"];
                    term.RoleKey = row["RoleKey"];
                    term.Description = row["Description"];

                    var isAccess = false;
                    if (!string.IsNullOrEmpty(row["IsAccess"]))
                    {
                        isAccess = Int32.Parse(row["IsAccess"]) != 0 ? true : false;
                    }

                    userTerms.Add(term, isAccess);
                }
            }

            return userTerms;
        }

        public async static Task<long> AddTermToUser(long userId, long termId, bool isAccess)
        {
            string commandText = "Insert into UserTerms (UserId, TermId, IsAccess) values (@userId, @termId, @isAccess)";
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            
            parameters.Add("@userId", userId);
            parameters.Add("@termId", termId);
            parameters.Add("@isAccess", isAccess ? 1 : 0);

            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var id = await conn.ExecuteAsync(commandText, parameters);
                results = (long)id;
            }

            return results;
        }

        public async static Task<long> RemoveTermFromUser(long userId, long termId)
        {
            string commandText = "Delete from UserTerms where UserId = @userId and TermId = @termId";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@userId", userId);
            parameters.Add("@termId", termId);

            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var id = await conn.ExecuteAsync(commandText, parameters);
                results = id;
            }

            return results;
        }

        public async static Task<long> AddTermToGroup(long groupId, long termId)
        {
            string commandText = "Insert into GroupTerms (GroupId, TermId) values (@groupId, @termId)";
            Dictionary<string, object> parameters = new Dictionary<string, object>();


            parameters.Add("@groupId", groupId);
            parameters.Add("@termId", termId);

            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var id = await conn.ExecuteAsync(commandText, parameters);
                results = (long)id;
            }

            return results;
        }

        public async static Task<long> RemoveTermFromGroup(long groupId, long termId)
        {
            string commandText = "Delete from GroupTerms where GroupId = @groupId and TermId = @termId";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@groupId", groupId);
            parameters.Add("@termId", termId);

            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var id = await conn.ExecuteAsync(commandText, parameters);
                results = (long)id;
            }

            return results;
        }

        public async static Task<IEnumerable<string>> GetGroupUsersBelongToTerm(long termId)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            var sql = "";
            sql += "select u.UserName ";
            sql += "from Users u ";
            sql += "inner join ( ";
            sql += "    select distinct gu.UserId";
            sql += "    FROM GroupTerms gt";
            sql += "    inner join GroupUsers gu on gu.GroupId = gt.GroupId";
            sql += "    where gt.TermId = @termId";
            sql += ") as t on t.UserId = u.Id";

            parameters.Add("@termId", termId);

            IList<string> users = new List<string>();
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {
                var rows = await conn.QueryAsync(sql, parameters);
                foreach (var row in rows)
                {
                    users.Add(row["UserName"]);
                }
            }
            
            

            return users;
        }
    }
}