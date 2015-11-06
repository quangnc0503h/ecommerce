using AspNet.Identity.MySQL;
using Quang.Auth.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quang.Auth.Api.DataAccess
{
    public class RefreshTokenTable : IRefreshTokenTable
    {
        private MySQLDatabase _database;

        public RefreshTokenTable(MySQLDatabase database)
        {
            this._database = database;
        }

        public Client GetOneClient(string clientId)
        {
            Client client = (Client)null;
            List<Dictionary<string, string>> list = this._database.Query("Select * from Clients where Id = @id", new Dictionary<string, object>()
      {
        {
          "@id",
          (object) clientId
        }
      });
            if (list != null && list.Count == 1)
            {
                Dictionary<string, string> dictionary = list[0];
                client = new Client();
                client.Id = dictionary["Id"];
                client.Secret = dictionary["Secret"];
                client.Name = dictionary["Name"];
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

        public IEnumerable<RefreshToken> GetAllRefreshTokens()
        {
            IList<RefreshToken> list1 = (IList<RefreshToken>)new List<RefreshToken>();
            List<Dictionary<string, string>> list2 = this._database.Query("Select * from RefreshTokens");
            if (list2 != null)
            {
                foreach (Dictionary<string, string> dictionary in list2)
                    list1.Add(this.ParseRefreshTokenFromRow((IDictionary<string, string>)dictionary));
            }
            return (IEnumerable<RefreshToken>)list1;
        }

        public RefreshToken GetOneRefreshToken(string refreshTokenId)
        {
            RefreshToken refreshToken = (RefreshToken)null;
            List<Dictionary<string, string>> list = this._database.Query("Select * from RefreshTokens where Id = @Id", new Dictionary<string, object>()
      {
        {
          "@Id",
          (object) refreshTokenId
        }
      });
            if (list != null && list.Count == 1)
                refreshToken = this.ParseRefreshTokenFromRow((IDictionary<string, string>)list[0]);
            return refreshToken;
        }

        public RefreshToken FindRefreshToken(string clientId, string subject)
        {
            RefreshToken refreshToken = (RefreshToken)null;
            List<Dictionary<string, string>> list = this._database.Query("Select * from RefreshTokens where ClientId = @ClientId and Subject = @Subject", new Dictionary<string, object>()
      {
        {
          "@ClientId",
          (object) clientId
        },
        {
          "@Subject",
          (object) subject
        }
      });
            if (list != null && list.Count == 1)
                refreshToken = this.ParseRefreshTokenFromRow((IDictionary<string, string>)list[0]);
            return refreshToken;
        }

        public int InsertRefreshToken(RefreshToken token)
        {
            return this._database.Execute("INSERT INTO `Security`.`RefreshTokens`(\n                                        `Id`,\n                                        `Subject`,\n                                        `ClientId`,\n                                        `IssuedUtc`,\n                                        `ExpiresUtc`,\n                                        `ProtectedTicket`\n                                    ) VALUES (\n                                        @Id,\n                                        @Subject,\n                                        @ClientId,\n                                        @IssuedUtc,\n                                        @ExpiresUtc,\n                                        @ProtectedTicket)", new Dictionary<string, object>()
      {
        {
          "@Id",
          (object) token.Id
        },
        {
          "@Subject",
          (object) token.Subject
        },
        {
          "@ClientId",
          (object) token.ClientId
        },
        {
          "@IssuedUtc",
          (object) token.IssuedUtc
        },
        {
          "@ExpiresUtc",
          (object) token.ExpiresUtc
        },
        {
          "@ProtectedTicket",
          (object) token.ProtectedTicket
        }
      });
        }

        public int AddRefreshToken(RefreshToken token)
        {
            RefreshToken refreshToken = this.FindRefreshToken(token.ClientId, token.Subject);
            if (refreshToken != null)
                this.RemoveRefreshToken(refreshToken.Id);
            return this.InsertRefreshToken(token);
        }

        public int RemoveRefreshToken(string refreshTokenId)
        {
            return this._database.Execute("Delete from RefreshTokens where Id = @refreshTokenId", new Dictionary<string, object>()
      {
        {
          "@refreshTokenId",
          (object) refreshTokenId
        }
      });
        }
    }
}