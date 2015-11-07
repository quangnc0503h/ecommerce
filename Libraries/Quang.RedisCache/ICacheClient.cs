using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Quang.RedisCache
{
    public interface ICacheClient : IDisposable
    {
        IDatabase Database { get; }

        ISerializer Serializer { get; }

        bool Exists(string key);

        Task<bool> ExistsAsync(string key);

        bool Remove(string key);

        Task<bool> RemoveAsync(string key);

        void RemoveAll(IEnumerable<string> keys);

        Task RemoveAllAsync(IEnumerable<string> keys);

        T Get<T>(string key) where T : class;

        Task<T> GetAsync<T>(string key) where T : class;

        bool Add<T>(string key, T value) where T : class;

        Task<bool> AddAsync<T>(string key, T value) where T : class;

        bool Replace<T>(string key, T value) where T : class;

        Task<bool> ReplaceAsync<T>(string key, T value) where T : class;

        bool Add<T>(string key, T value, DateTimeOffset expiresAt) where T : class;

        Task<bool> AddAsync<T>(string key, T value, DateTimeOffset expiresAt) where T : class;

        bool Replace<T>(string key, T value, DateTimeOffset expiresAt) where T : class;

        Task<bool> ReplaceAsync<T>(string key, T value, DateTimeOffset expiresAt) where T : class;

        bool Add<T>(string key, T value, TimeSpan expiresIn) where T : class;

        Task<bool> AddAsync<T>(string key, T value, TimeSpan expiresIn) where T : class;

        bool Replace<T>(string key, T value, TimeSpan expiresIn) where T : class;

        Task<bool> ReplaceAsync<T>(string key, T value, TimeSpan expiresIn) where T : class;

        IDictionary<string, T> GetAll<T>(IEnumerable<string> keys) where T : class;

        Task<List<T>> GetAllAsync<T>(List<string> keys) where T : class;

        bool AddAll<T>(IList<Tuple<string, T>> items) where T : class;

        Task<bool> AddAllAsync<T>(IList<Tuple<string, T>> items) where T : class;

        bool SetAdd(string memberName, string key);

        Task<bool> SetAddAsync(string memberName, string key);

        string[] SetMember(string memberName);

        Task<string[]> SetMemberAsync(string memberName);

        IEnumerable<string> SearchKeys(string pattern);

        Task<IEnumerable<string>> SearchKeysAsync(string pattern);

        Task<long> ListRightPushAsync<T>(string key, T value) where T : class;

        void FlushDb();

        Task FlushDbAsync();

        void Save(SaveType saveType);

        void SaveAsync(SaveType saveType);

        Dictionary<string, string> GetInfo();

        Task<Dictionary<string, string>> GetInfoAsync();

        Task<T> ListLeftPopAsync<T>(string key) where T : class;

        long ListLength(string listBookingCodeKey);
    }
}
