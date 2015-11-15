using AspNet.Identity.MySQL;
using Quang.Auth.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Quang.Auth.Api.DataAccess
{
    public class TermTable : ITermTable
    {
        private readonly MySQLDatabase _database;

        public TermTable(MySQLDatabase database)
        {
            _database = database;
        }

        public int Delete(int termId)
        {
            return _database.Execute("Delete from Terms where Id = @id", new Dictionary<string, object>()
      {
        {
          "@id",
          termId
        }
      });
        }

        public int Delete(IEnumerable<int> Ids)
        {
            int num = _database.Execute("Delete from Terms where Id in (" + string.Join(",", Ids.ToArray()) + ")", new Dictionary<string, object>());
            _database.Execute("Delete from UserTerms where TermId in (" + string.Join(",", Ids.ToArray()) + ")", new Dictionary<string, object>());
            _database.Execute("Delete from GroupTerms where TermId in (" + string.Join(",", Ids.ToArray()) + ")", new Dictionary<string, object>());
            return num;
        }

        public int Insert(Term term)
        {
            const string commandText = "Insert into Terms (Id, RoleKey, Name, Description) values (@id, @roleKey, @name, @description)";
            var parameters = new Dictionary<string, object>();
            if (term.Id > 0)
                parameters.Add("@id", term.Id);
            else
                parameters.Add("@id", null);
            parameters.Add("@name", term.Name);
            parameters.Add("@roleKey", term.RoleKey);
            parameters.Add("@description", term.Description);
            return _database.Execute(commandText, parameters);
        }

        public int Update(Term term)
        {
            return _database.Execute("Update Terms set RoleKey = @roleKey, Name = @name, Description = @description  where Id = @id", new Dictionary<string, object>
                                                                                                                                      {
        {
          "@id",
          term.Id
        },
        {
          "@roleKey",
          term.RoleKey
        },
        {
          "@name",
          term.Name
        },
        {
          "@description",
          term.Description
        }
      });
        }

        public IEnumerable<Term> GetAllTerms()
        {
            IList<Term> list = _database.Query("Select * from Terms order by RoleKey").Select(dictionary => new Term
                                                                                                                 {
                                                                                                                     Id = int.Parse(dictionary["Id"]), Name = dictionary["Name"], RoleKey = dictionary["RoleKey"], Description = dictionary["Description"]
                                                                                                                 }).ToList();
            return list;
        }

        public Term GetOneTerm(int termId)
        {
            Term term = null;
            List<Dictionary<string, string>> list = _database.Query("Select * from Terms where Id = @id", new Dictionary<string, object>
                                                                                                          {
        {
          "@id",
          termId
        }
      });
            if (list != null && list.Count == 1)
            {
                Dictionary<string, string> dictionary = list[0];
                term = new Term
                       {
                           Id = int.Parse(dictionary["Id"]),
                           RoleKey = dictionary["RoleKey"],
                           Name = string.IsNullOrEmpty(dictionary["Name"]) ? null : dictionary["Name"],
                           Description =
                               string.IsNullOrEmpty(dictionary["Description"])
                                   ? null
                                   : dictionary["Description"]
                       };
            }
            return term;
        }

        public int GetTotal(string keyword)
        {
            return int.Parse(_database.QueryValue("select count(*) from Terms where Name LIKE @param", new Dictionary<string, object>
                                                                                                       {
        {
          "@param",
          "%" + Utils.EncodeForLike(keyword) + "%"
        }
      }).ToString());
        }

        public IEnumerable<Term> GetPaging(int pageSize, int pageNumber, string keyword)
        {
            var parameters = new Dictionary<string, object>();
            const string str = "select * from Terms where Name LIKE @param";
            parameters.Add("@param", "%" + Utils.EncodeForLike(keyword) + "%");
            const string commandText = str + " order by RoleKey limit @rowNumber, @pageSize";
            parameters.Add("@rowNumber", pageSize * pageNumber);
            parameters.Add("@pageSize", pageSize);
            return _database.Query(commandText, parameters).Select(dictionary => new Term
                                                                                 {
                                                                                          Id = int.Parse(dictionary["Id"]), RoleKey = dictionary["RoleKey"], Name = dictionary["Name"], Description = dictionary["Description"]
                                                                                      }).ToList();
        }

        public IEnumerable<Term> GetTermsByGroup(int groupId)
        {
            var parameters = new Dictionary<string, object>();
            const string commandText = "" + "select t.*" + "from GroupTerms ut " + "inner join Terms t on t.Id = ut.TermId " + "where ut.GroupId = @groupId " + "order by RoleKey";
            parameters.Add("@groupId", groupId);
            return _database.Query(commandText, parameters).Select(dictionary => new Term
                                                                                 {
                                                                                          Id = int.Parse(dictionary["Id"]), RoleKey = dictionary["RoleKey"], Name = dictionary["Name"], Description = dictionary["Description"]
                                                                                      }).ToList();
        }

        public IDictionary<Term, bool> GetTermsByUser(int userId)
        {
            var parameters = new Dictionary<string, object>();
            const string commandText = "" + "select t.*,ut.IsAccess " + "from UserTerms ut " + "inner join Terms t on t.Id = ut.TermId " + "where ut.UserId = @userId " + "order by RoleKey";
            parameters.Add("@userId", userId);
            var dictionary1 = (IDictionary<Term, bool>)new Dictionary<Term, bool>();
            foreach (Dictionary<string, string> dictionary2 in _database.Query(commandText, parameters))
            {
                var key = new Term
                           {
                               Id = int.Parse(dictionary2["Id"]),
                               RoleKey = dictionary2["RoleKey"],
                               Name = dictionary2["Name"],
                               Description = dictionary2["Description"]
                           };
                bool flag = false;
                if (!string.IsNullOrEmpty(dictionary2["IsAccess"]))
                    flag = bool.Parse(dictionary2["IsAccess"]);
                dictionary1.Add(key, flag);
            }
            return dictionary1;
        }

        public IDictionary<string, bool> GetUsersByTerm(int termId)
        {
            var parameters = new Dictionary<string, object>();
            const string commandText = "" + "select u.UserName,ut.IsAccess " + "from UserTerms ut " + "inner join Users u on u.Id = ut.UserId " + "where ut.TermId = @termId ";
            parameters.Add("@termId", termId);
            var dictionary1 = (IDictionary<string, bool>)new Dictionary<string, bool>();
            foreach (var dictionary2 in _database.Query(commandText, parameters))
            {
                bool flag = false;
                if (!string.IsNullOrEmpty(dictionary2["IsAccess"]))
                    flag = bool.Parse(dictionary2["IsAccess"]);
                dictionary1.Add(dictionary2["UserName"], flag);
            }
            return dictionary1;
        }

        public IEnumerable<Term> GetGroupTermsBelongToUser(int userId)
        {
            var parameters = new Dictionary<string, object>();
            var list = (IList<Term>)new List<Term>();
            const string commandText = "" + "select t.* " + "from Terms t " + "inner join (" + "    select distinct gt.TermId " + "    from GroupUsers as gu " + "    inner join GroupTerms as gt on gt.GroupId = gu.GroupId " + "    where gu.UserId = @userId " + ") as t1 on t1.TermId = t.Id";
            parameters.Add("@userId", userId);
            foreach (var dictionary in _database.Query(commandText, parameters))
                list.Add(new Term
                         {
                    Id = int.Parse(dictionary["Id"]),
                    Name = dictionary["Name"],
                    RoleKey = dictionary["RoleKey"],
                    Description = dictionary["Description"]
                });
            return list;
        }

        public IDictionary<Term, bool> GetUserTermsWithGroupAccess(int userId)
        {
            var parameters = new Dictionary<string, object>();
            var dictionary1 = (IDictionary<Term, bool>)new Dictionary<Term, bool>();
            const string commandText = "" + "select t.*, " + "case when t1.TermId is not null then 1 else 0 end as IsAccess " + "from Terms t " + "left join (" + "    select distinct gt.TermId " + "    from GroupUsers as gu " + "    inner join GroupTerms as gt on gt.GroupId = gu.GroupId " + "    where gu.UserId = @userId " + ") as t1 on t1.TermId = t.Id " + "order by t.RoleKey";
            parameters.Add("@userId", userId);
            foreach (Dictionary<string, string> dictionary2 in _database.Query(commandText, parameters))
            {
                var key = new Term
                          {
                              Id = int.Parse(dictionary2["Id"]),
                              Name = dictionary2["Name"],
                              RoleKey = dictionary2["RoleKey"],
                              Description = dictionary2["Description"]
                          };
                bool flag = false;
                if (!string.IsNullOrEmpty(dictionary2["IsAccess"]))
                    flag = int.Parse(dictionary2["IsAccess"]) != 0;
                dictionary1.Add(key, flag);
            }
            return dictionary1;
        }

        public int addTermToUser(int userId, int termId, bool isAccess)
        {
            return _database.Execute("Insert into UserTerms (UserId, TermId, IsAccess) values (@userId, @termId, @isAccess)", new Dictionary<string, object>
                                                                                                                              {
        {
          "@userId",
          userId
        },
        {
          "@termId",
          termId
        },
        {
          "@isAccess",
          isAccess ? 1 : 0
        }
      });
        }

        public int removeTermFromUser(int userId, int termId)
        {
            return _database.Execute("Delete from UserTerms where UserId = @userId and TermId = @termId", new Dictionary<string, object>
                                                                                                          {
        {
          "@userId",
          userId
        },
        {
          "@termId",
          termId
        }
      });
        }

        public int addTermToGroup(int groupId, int termId)
        {
            return _database.Execute("Insert into GroupTerms (GroupId, TermId) values (@groupId, @termId)", new Dictionary<string, object>
                                                                                                            {
        {
          "@groupId",
          groupId
        },
        {
          "@termId",
          termId
        }
      });
        }

        public int removeTermFromGroup(int groupId, int termId)
        {
            return _database.Execute("Delete from GroupTerms where GroupId = @groupId and TermId = @termId", new Dictionary<string, object>
                                                                                                             {
        {
          "@groupId",
          groupId
        },
        {
          "@termId",
          termId
        }
      });
        }

        public IEnumerable<string> GetGroupUsersBelongToTerm(int termId)
        {
            var parameters = new Dictionary<string, object>();
            const string commandText = "" + "select u.UserName " + "from Users u " + "inner join ( " + "    select distinct gu.UserId" + "    FROM GroupTerms gt" + "    inner join GroupUsers gu on gu.GroupId = gt.GroupId" + "    where gt.TermId = @termId" + ") as t on t.UserId = u.Id";
            parameters.Add("@termId", termId);
            var list = (IList<string>)new List<string>();
            foreach (Dictionary<string, string> dictionary in _database.Query(commandText, parameters))
                list.Add(dictionary["UserName"]);
            return list;
        }
    }
}