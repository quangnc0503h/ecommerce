using System.Collections.Generic;
using Quang.Auth.Entities;

namespace Quang.Auth.Api.DataAccess
{
    public interface IRefreshTokenTable
    {
        Client GetOneClient(string clientId);

        IEnumerable<RefreshToken> GetAllRefreshTokens();

        RefreshToken GetOneRefreshToken(string refreshTokenId);

        RefreshToken FindRefreshToken(string clientId, string subject);

        int AddRefreshToken(RefreshToken token);

        int RemoveRefreshToken(string refreshTokenId);

        int InsertRefreshToken(RefreshToken token);
    }
}