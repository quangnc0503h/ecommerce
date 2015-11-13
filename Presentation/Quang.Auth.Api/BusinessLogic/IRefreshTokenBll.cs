using System.Collections.Generic;
using System.Threading.Tasks;
using Quang.Auth.Entities;

namespace Quang.Auth.Api.BusinessLogic
{
    public interface IRefreshTokenBll
    {
        Task<Client> GetOneClient(string clientId);

        Task<IEnumerable<RefreshToken>> GetAllRefreshTokens();

        Task<RefreshToken> GetOneRefreshToken(string refreshTokenId);

        Task<RefreshToken> FindRefreshToken(string clientId, string subject);

        Task<int> AddRefreshToken(RefreshToken token);

        Task<int> RemoveRefreshToken(string refreshTokenId);

        Task<int> InsertRefreshToken(RefreshToken token);
    }
}