using AspNet.Identity.MySQL;
using Quang.Auth.Entities;
using System;
using System.Collections.Generic;

namespace Quang.Auth.Api.DataAccess
{
    public class RefreshTokenTable : IRefreshTokenTable
    {
        private readonly MySQLDatabase _database;

        public RefreshTokenTable(MySQLDatabase database)
        {
            _database = database;
        }

        public Client GetOneClient(string clientId)
        {
            var client = (Client)null;
            List<Dictionary<string, string>> list = _database.Query("Select * from Clients where Id = @id", new Dictionary<string, object>()
      {
        {
          "@id",
          clientId
        }
      });
            if (list != null && list.Count == 1)
            {
                Dictionary<string, string> dictionary = list[0];
                client = new Client {Id = dictionary["Id"], Secret = dictionary["Secret"], Name = dictionary["Name"]};
                if (int.Parse(dictionary["ApplicationType"]) == 1)
                    client.ApplicationType = ApplicationTypes.NativeConfidential;
                else if (int.Parse(dictionary["ApplicationType"]) == 0)
                    client.ApplicationType = ApplicationTypes.JavaScript;
                client.Active = int.Parse(dictionary["Active"]) == 1;
                client.RefreshTokenLifeTime = int.Parse(dictionary["RefreshTokenLifeTime"]);
                client.AllowedOrigin = dictionary["AllowedOrigin"];
            }
            return client;
        }

        private RefreshToken ParseRefreshTokenFromRow(IDictionary<string, string> row)
        {
            return new RefreshToken
                   {
                       Id = string.IsNullOrEmpty(row["Id"]) ? null : row["Id"],
                       Subject = string.IsNullOrEmpty(row["Subject"]) ? null : row["Subject"],
                       ClientId = string.IsNullOrEmpty(row["ClientId"]) ? null : row["ClientId"],
                       IssuedUtc =
                           string.IsNullOrEmpty(row["IssuedUtc"]) ? new DateTime() : DateTime.Parse(row["IssuedUtc"]),
                       ExpiresUtc =
                           string.IsNullOrEmpty(row["ExpiresUtc"]) ? new DateTime() : DateTime.Parse(row["ExpiresUtc"]),
                       ProtectedTicket = string.IsNullOrEmpty(row["ProtectedTicket"]) ? null : row["ProtectedTicket"]
                   };
        }

        public IEnumerable<RefreshToken> GetAllRefreshTokens()
        {
            var list1 = (IList<RefreshToken>)new List<RefreshToken>();
            List<Dictionary<string, string>> list2 = _database.Query("Select * from RefreshTokens");
            if (list2 != null)
            {
                foreach (Dictionary<string, string> dictionary in list2)
                    list1.Add(ParseRefreshTokenFromRow(dictionary));
            }
            return list1;
        }

        public RefreshToken GetOneRefreshToken(string refreshTokenId)
        {
            RefreshToken refreshToken = null;
            List<Dictionary<string, string>> list = _database.Query("Select * from RefreshTokens where Id = @Id", new Dictionary<string, object>()
      {
        {
          "@Id",
          refreshTokenId
        }
      });
            if (list != null && list.Count == 1)
                refreshToken = ParseRefreshTokenFromRow(list[0]);
            return refreshToken;
        }

        public RefreshToken FindRefreshToken(string clientId, string subject)
        {
            RefreshToken refreshToken = null;
            List<Dictionary<string, string>> list = _database.Query("Select * from RefreshTokens where ClientId = @ClientId and Subject = @Subject", new Dictionary<string, object>()
      {
        {
          "@ClientId",
          clientId
        },
        {
          "@Subject",
          subject
        }
      });
            if (list != null && list.Count == 1)
                refreshToken = ParseRefreshTokenFromRow(list[0]);
            return refreshToken;
        }

        public int InsertRefreshToken(RefreshToken token)
        {
            return _database.Execute("INSERT INTO `Security`.`RefreshTokens`(\n                                        `Id`,\n                                        `Subject`,\n                                        `ClientId`,\n                                        `IssuedUtc`,\n                                        `ExpiresUtc`,\n                                        `ProtectedTicket`\n                                    ) VALUES (\n                                        @Id,\n                                        @Subject,\n                                        @ClientId,\n                                        @IssuedUtc,\n                                        @ExpiresUtc,\n                                        @ProtectedTicket)", new Dictionary<string, object>()
      {
        {
          "@Id",
          token.Id
        },
        {
          "@Subject",
          token.Subject
        },
        {
          "@ClientId",
          token.ClientId
        },
        {
          "@IssuedUtc",
          token.IssuedUtc
        },
        {
          "@ExpiresUtc",
          token.ExpiresUtc
        },
        {
          "@ProtectedTicket",
          token.ProtectedTicket
        }
      });
        }

        public int AddRefreshToken(RefreshToken token)
        {
            RefreshToken refreshToken = FindRefreshToken(token.ClientId, token.Subject);
            if (refreshToken != null)
                RemoveRefreshToken(refreshToken.Id);
            return InsertRefreshToken(token);
        }

        public int RemoveRefreshToken(string refreshTokenId)
        {
            return _database.Execute("Delete from RefreshTokens where Id = @refreshTokenId", new Dictionary<string, object>()
      {
        {
          "@refreshTokenId",
          refreshTokenId
        }
      });
        }
    }
}