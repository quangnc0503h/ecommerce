using AspNet.Identity.MySQL;
using Quang.Auth.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quang.Auth.Api.DataAccess
{
    public class TermTable : ITermTable
    {
        private MySQLDatabase _database;

        public TermTable(MySQLDatabase database)
        {
            this._database = database;
        }

        public int Delete(int termId)
        {
            return this._database.Execute("Delete from Terms where Id = @id", new Dictionary<string, object>()
      {
        {
          "@id",
          (object) termId
        }
      });
        }

        public int Delete(IEnumerable<int> Ids)
        {
            int num = this._database.Execute("Delete from Terms where Id in (" + string.Join<int>(",", (IEnumerable<int>)Enumerable.ToArray<int>(Ids)) + ")", new Dictionary<string, object>());
            this._database.Execute("Delete from UserTerms where TermId in (" + string.Join<int>(",", (IEnumerable<int>)Enumerable.ToArray<int>(Ids)) + ")", new Dictionary<string, object>());
            this._database.Execute("Delete from GroupTerms where TermId in (" + string.Join<int>(",", (IEnumerable<int>)Enumerable.ToArray<int>(Ids)) + ")", new Dictionary<string, object>());
            return num;
        }

        public int Insert(Term term)
        {
            string commandText = "Insert into Terms (Id, RoleKey, Name, Description) values (@id, @roleKey, @name, @description)";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            if (term.Id > 0)
                parameters.Add("@id", (object)term.Id);
            else
                parameters.Add("@id", (object)null);
            parameters.Add("@name", (object)term.Name);
            parameters.Add("@roleKey", (object)term.RoleKey);
            parameters.Add("@description", (object)term.Description);
            return this._database.Execute(commandText, parameters);
        }

        public int Update(Term term)
        {
            return this._database.Execute("Update Terms set RoleKey = @roleKey, Name = @name, Description = @description  where Id = @id", new Dictionary<string, object>()
      {
        {
          "@id",
          (object) term.Id
        },
        {
          "@roleKey",
          (object) term.RoleKey
        },
        {
          "@name",
          (object) term.Name
        },
        {
          "@description",
          (object) term.Description
        }
      });
        }

        public IEnumerable<Term> GetAllTerms()
        {
            IList<Term> list = (IList<Term>)new List<Term>();
            foreach (Dictionary<string, string> dictionary in this._database.Query("Select * from Terms order by RoleKey"))
                list.Add(new Term()
                {
                    Id = int.Parse(dictionary["Id"]),
                    Name = dictionary["Name"],
                    RoleKey = dictionary["RoleKey"],
                    Description = dictionary["Description"]
                });
            return (IEnumerable<Term>)list;
        }

        public Term GetOneTerm(int termId)
        {
            Term term = (Term)null;
            List<Dictionary<string, string>> list = this._database.Query("Select * from Terms where Id = @id", new Dictionary<string, object>()
      {
        {
          "@id",
          (object) termId
        }
      });
            if (list != null && list.Count == 1)
            {
                Dictionary<string, string> dictionary = list[0];
                term = new Term();
                term.Id = int.Parse(dictionary["Id"]);
                term.RoleKey = dictionary["RoleKey"];
                term.Name = string.IsNullOrEmpty(dictionary["Name"]) ? (string)null : dictionary["Name"];
                term.Description = string.IsNullOrEmpty(dictionary["Description"]) ? (string)null : dictionary["Description"];
            }
            return term;
        }

        public int GetTotal(string keyword)
        {
            return int.Parse(this._database.QueryValue("select count(*) from Terms where Name LIKE @param", new Dictionary<string, object>()
      {
        {
          "@param",
          (object) ("%" + Utils.EncodeForLike(keyword) + "%")
        }
      }).ToString());
        }

        public IEnumerable<Term> GetPaging(int pageSize, int pageNumber, string keyword)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string str = "select * from Terms where Name LIKE @param";
            parameters.Add("@param", (object)("%" + Utils.EncodeForLike(keyword) + "%"));
            string commandText = str + " order by RoleKey limit @rowNumber, @pageSize";
            parameters.Add("@rowNumber", (object)(pageSize * pageNumber));
            parameters.Add("@pageSize", (object)pageSize);
            List<Term> list = new List<Term>();
            foreach (Dictionary<string, string> dictionary in this._database.Query(commandText, parameters))
                list.Add(new Term()
                {
                    Id = int.Parse(dictionary["Id"]),
                    RoleKey = dictionary["RoleKey"],
                    Name = dictionary["Name"],
                    Description = dictionary["Description"]
                });
            return (IEnumerable<Term>)list;
        }

        public IEnumerable<Term> GetTermsByGroup(int groupId)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string commandText = "" + "select t.*" + "from GroupTerms ut " + "inner join Terms t on t.Id = ut.TermId " + "where ut.GroupId = @groupId " + "order by RoleKey";
            parameters.Add("@groupId", (object)groupId);
            List<Term> list = new List<Term>();
            foreach (Dictionary<string, string> dictionary in this._database.Query(commandText, parameters))
                list.Add(new Term()
                {
                    Id = int.Parse(dictionary["Id"]),
                    RoleKey = dictionary["RoleKey"],
                    Name = dictionary["Name"],
                    Description = dictionary["Description"]
                });
            return (IEnumerable<Term>)list;
        }

        public IDictionary<Term, bool> GetTermsByUser(int userId)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string commandText = "" + "select t.*,ut.IsAccess " + "from UserTerms ut " + "inner join Terms t on t.Id = ut.TermId " + "where ut.UserId = @userId " + "order by RoleKey";
            parameters.Add("@userId", (object)userId);
            IDictionary<Term, bool> dictionary1 = (IDictionary<Term, bool>)new Dictionary<Term, bool>();
            foreach (Dictionary<string, string> dictionary2 in this._database.Query(commandText, parameters))
            {
                Term key = new Term();
                key.Id = int.Parse(dictionary2["Id"]);
                key.RoleKey = dictionary2["RoleKey"];
                key.Name = dictionary2["Name"];
                key.Description = dictionary2["Description"];
                bool flag = false;
                if (!string.IsNullOrEmpty(dictionary2["IsAccess"]))
                    flag = bool.Parse(dictionary2["IsAccess"]);
                dictionary1.Add(key, flag);
            }
            return dictionary1;
        }

        public IDictionary<string, bool> GetUsersByTerm(int termId)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string commandText = "" + "select u.UserName,ut.IsAccess " + "from UserTerms ut " + "inner join Users u on u.Id = ut.UserId " + "where ut.TermId = @termId ";
            parameters.Add("@termId", (object)termId);
            IDictionary<string, bool> dictionary1 = (IDictionary<string, bool>)new Dictionary<string, bool>();
            foreach (Dictionary<string, string> dictionary2 in this._database.Query(commandText, parameters))
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
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            IList<Term> list = (IList<Term>)new List<Term>();
            string commandText = "" + "select t.* " + "from Terms t " + "inner join (" + "    select distinct gt.TermId " + "    from GroupUsers as gu " + "    inner join GroupTerms as gt on gt.GroupId = gu.GroupId " + "    where gu.UserId = @userId " + ") as t1 on t1.TermId = t.Id";
            parameters.Add("@userId", (object)userId);
            foreach (Dictionary<string, string> dictionary in this._database.Query(commandText, parameters))
                list.Add(new Term()
                {
                    Id = int.Parse(dictionary["Id"]),
                    Name = dictionary["Name"],
                    RoleKey = dictionary["RoleKey"],
                    Description = dictionary["Description"]
                });
            return (IEnumerable<Term>)list;
        }

        public IDictionary<Term, bool> GetUserTermsWithGroupAccess(int userId)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            IDictionary<Term, bool> dictionary1 = (IDictionary<Term, bool>)new Dictionary<Term, bool>();
            string commandText = "" + "select t.*, " + "case when t1.TermId is not null then 1 else 0 end as IsAccess " + "from Terms t " + "left join (" + "    select distinct gt.TermId " + "    from GroupUsers as gu " + "    inner join GroupTerms as gt on gt.GroupId = gu.GroupId " + "    where gu.UserId = @userId " + ") as t1 on t1.TermId = t.Id " + "order by t.RoleKey";
            parameters.Add("@userId", (object)userId);
            foreach (Dictionary<string, string> dictionary2 in this._database.Query(commandText, parameters))
            {
                Term key = new Term();
                key.Id = int.Parse(dictionary2["Id"]);
                key.Name = dictionary2["Name"];
                key.RoleKey = dictionary2["RoleKey"];
                key.Description = dictionary2["Description"];
                bool flag = false;
                if (!string.IsNullOrEmpty(dictionary2["IsAccess"]))
                    flag = int.Parse(dictionary2["IsAccess"]) != 0;
                dictionary1.Add(key, flag);
            }
            return dictionary1;
        }

        public int addTermToUser(int userId, int termId, bool isAccess)
        {
            return this._database.Execute("Insert into UserTerms (UserId, TermId, IsAccess) values (@userId, @termId, @isAccess)", new Dictionary<string, object>()
      {
        {
          "@userId",
          (object) userId
        },
        {
          "@termId",
          (object) termId
        },
        {
          "@isAccess",
          (object) (isAccess ? 1 : 0)
        }
      });
        }

        public int removeTermFromUser(int userId, int termId)
        {
            return this._database.Execute("Delete from UserTerms where UserId = @userId and TermId = @termId", new Dictionary<string, object>()
      {
        {
          "@userId",
          (object) userId
        },
        {
          "@termId",
          (object) termId
        }
      });
        }

        public int addTermToGroup(int groupId, int termId)
        {
            return this._database.Execute("Insert into GroupTerms (GroupId, TermId) values (@groupId, @termId)", new Dictionary<string, object>()
      {
        {
          "@groupId",
          (object) groupId
        },
        {
          "@termId",
          (object) termId
        }
      });
        }

        public int removeTermFromGroup(int groupId, int termId)
        {
            return this._database.Execute("Delete from GroupTerms where GroupId = @groupId and TermId = @termId", new Dictionary<string, object>()
      {
        {
          "@groupId",
          (object) groupId
        },
        {
          "@termId",
          (object) termId
        }
      });
        }

        public IEnumerable<string> GetGroupUsersBelongToTerm(int termId)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string commandText = "" + "select u.UserName " + "from Users u " + "inner join ( " + "    select distinct gu.UserId" + "    FROM GroupTerms gt" + "    inner join GroupUsers gu on gu.GroupId = gt.GroupId" + "    where gt.TermId = @termId" + ") as t on t.UserId = u.Id";
            parameters.Add("@termId", (object)termId);
            IList<string> list = (IList<string>)new List<string>();
            foreach (Dictionary<string, string> dictionary in this._database.Query(commandText, parameters))
                list.Add(dictionary["UserName"]);
            return (IEnumerable<string>)list;
        }
    }
}