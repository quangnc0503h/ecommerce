using Dapper;
using Quang.Auth.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quang.Auth.Entities
{
    public static class RefreshTokenDal
    {
        public static async Task< Client> GetOneClient(string clientId)
        {
            Client client = (Client)null;
            const string commandText = "Select * from Clients where Id = @id";
            var parameters = new DynamicParameters();
            parameters.Add("@id", clientId);
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {
                var data = await conn.QueryAsync<Client>(commandText, parameters);
                client = data.First();
            }         
            return client;
        }

        private static RefreshToken ParseRefreshTokenFromRow(IDictionary<string, string> row)
        {
            return new RefreshToken()
            {
                Id = string.IsNullOrEmpty(row["Id"]) ? (string)null : row["Id"],
                Subject = string.IsNullOrEmpty(row["Subject"]) ? (string)null : row["Subject"],
                ClientId = string.IsNullOrEmpty(row["ClientId"]) ? (string)null : row["ClientId"],
                IssuedUtc = string.IsNullOrEmpty(row["IssuedUtc"]) ? new DateTime() : DateTime.Parse(row["IssuedUtc"]),
                ExpiresUtc = string.IsNullOrEmpty(row["ExpiresUtc"]) ? new DateTime() : DateTime.Parse(row["ExpiresUtc"]),
                ProtectedTicket = string.IsNullOrEmpty(row["ProtectedTicket"]) ? (string)null : row["ProtectedTicket"]
            };
        }

        public static async Task< IEnumerable<RefreshToken>> GetAllRefreshTokens()
        {
            

            var commandText ="Select * from RefreshTokens";
            var parameters = new Dictionary<string, object>() { };
            List<RefreshToken> refreshTokens;
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {
                var data = await conn.QueryAsync<RefreshToken>(commandText, parameters);
                refreshTokens = data.ToList();
            }
         
            return refreshTokens;
        }

        public static async Task<RefreshToken> GetOneRefreshToken(string refreshTokenId)
        {
            RefreshToken refreshToken = (RefreshToken)null;
            const string commandText = "Select * from RefreshTokens where Id = @id";
            var parameters = new DynamicParameters();
            parameters.Add("@id", refreshTokenId);
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {
                var data = await conn.QueryAsync<RefreshToken>(commandText, parameters);
                refreshToken = data.First();
            }
            
            return refreshToken;
        }

        public static async Task< RefreshToken> FindRefreshToken(string clientId, string subject)
        {
            RefreshToken refreshToken = (RefreshToken)null;
            const string commandText = "Select * from RefreshTokens where ClientId = @clientId and Subject = @subject";
            var parameters = new DynamicParameters();
            parameters.Add("@clientId", clientId);
            parameters.Add("@subject", subject);
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {
                var data = await conn.QueryAsync<RefreshToken>(commandText, parameters);
                refreshToken = data.First();
            }           
            return refreshToken;
        }

        public static async Task<long> InsertRefreshToken(RefreshToken token)

        {
            const string commandText = @" INSERT INTO RefreshTokens(Id, Subject, ClientId, IssuedUtc, ExpiresUtc, ProtectedTicket) VALUES (@Id, @Subject, @ClientId, @IssuedUtc, @ExpiresUtc, @ProtectedTicket)";
            var parameters = new DynamicParameters();
            parameters.Add("@Id", token.Id);
            parameters.Add("@Subject", token.Subject);
            parameters.Add("@ClientId", token.ClientId);
            parameters.Add("@IssuedUtc", token.IssuedUtc);
            parameters.Add("@ExpiresUtc", token.ExpiresUtc);
            parameters.Add("@ProtectedTicket", token.ProtectedTicket);

            long results;
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var data = await conn.QueryAsync<long>(commandText, parameters);
                results = data.FirstOrDefault();
            }
            return results;
            
        }

        public static async Task<long> AddRefreshToken(RefreshToken token)
        {
            RefreshToken refreshToken = await FindRefreshToken(token.ClientId, token.Subject);
            if (refreshToken != null)
                 await RemoveRefreshToken(refreshToken.Id);
            return await InsertRefreshToken(token);
        }

        public static async Task<long> RemoveRefreshToken(string refreshTokenId)
        {
            const string commandText = "Delete from RefreshTokens where Id = @refreshTokenId";
            var parameters = new DynamicParameters();
            parameters.Add("refreshTokenId", refreshTokenId);
            long results;
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var data = await conn.QueryAsync<long>(commandText, parameters);
                results = data.FirstOrDefault();
            }
            return results;           
        }
    }
}
