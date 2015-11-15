using System.Collections.Generic;
using Quang.Auth.Entities;

namespace Quang.Auth.Api.DataAccess
{
    public interface ITermTable
    {
        Term GetOneTerm(int termId);

        int Delete(int termId);

        int Delete(IEnumerable<int> Ids);

        int Insert(Term term);

        int Update(Term term);

        IEnumerable<Term> GetAllTerms();

        int GetTotal(string keyword);

        IEnumerable<Term> GetPaging(int pageSize, int pageNumber, string keyword);

        IEnumerable<Term> GetTermsByGroup(int groupId);

        IDictionary<Term, bool> GetTermsByUser(int userId);

        IDictionary<string, bool> GetUsersByTerm(int termId);

        IEnumerable<Term> GetGroupTermsBelongToUser(int userId);

        IEnumerable<string> GetGroupUsersBelongToTerm(int termId);

        IDictionary<Term, bool> GetUserTermsWithGroupAccess(int userId);

        int addTermToUser(int userId, int termId, bool isAccess);

        int removeTermFromUser(int userId, int termId);

        int addTermToGroup(int groupId, int termId);

        int removeTermFromGroup(int groupId, int termId);
    }
}