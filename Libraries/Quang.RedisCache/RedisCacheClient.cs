using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;
using System.Net;

namespace Quang.RedisCache
{
    public class RedisCacheClient : ICacheClient
    {
        private static readonly Encoding encoding = Encoding.UTF8;
        private readonly ConnectionMultiplexer connectionMultiplexer;
        private readonly IDatabase db;
        private readonly ISerializer serializer;

        public IDatabase Database
        {
            get
            {
                return db;
            }
        }

        public ISerializer Serializer
        {
            get
            {
                return serializer;
            }
        }

        public RedisCacheClient(string connectionString, ISerializer serializer = null, int database = 0)
        {
            if (serializer == null)
                serializer = new JilSerializer();
            this.serializer = serializer;
            connectionMultiplexer = ConnectionMultiplexer.Connect(connectionString);
            db = connectionMultiplexer.GetDatabase(database);
        }

        public RedisCacheClient(ISerializer serializer, string connectionString, int database = 0)
        {
            if (serializer == null)
                throw new ArgumentNullException("serializer");
            this.serializer = serializer;
            connectionMultiplexer = ConnectionMultiplexer.Connect(connectionString);
            db = connectionMultiplexer.GetDatabase(database);
        }

        public RedisCacheClient(ConnectionMultiplexer connectionMultiplexer, ISerializer serializer, int database = 0)
        {
            if (connectionMultiplexer == null)
                throw new ArgumentNullException("connectionMultiplexer");
            if (serializer == null)
                throw new ArgumentNullException("serializer");
            this.serializer = serializer;
            this.connectionMultiplexer = connectionMultiplexer;
            db = connectionMultiplexer.GetDatabase(database);
        }

        public void Dispose()
        {
            connectionMultiplexer.Dispose();
        }

        public bool Exists(string key)
        {
            return db.KeyExists(key);
        }

        public Task<bool> ExistsAsync(string key)
        {
            return db.KeyExistsAsync(key);
        }

        public bool Remove(string key)
        {
            return db.KeyDelete(key);
        }

        public Task<bool> RemoveAsync(string key)
        {
            return db.KeyDeleteAsync(key);
        }

        public void RemoveAll(IEnumerable<string> keys)
        {
            keys.ForEach(x => Remove(x));
        }

        public Task RemoveAllAsync(IEnumerable<string> keys)
        {
            return keys.ForEachAsync(RemoveAsync);
        }

        public T Get<T>(string key) where T : class
        {
            var redisValue = db.StringGet(key);
            return !redisValue.HasValue ? default(T) : serializer.Deserialize<T>(redisValue);
        }

        public async Task<T> GetAsync<T>(string key) where T : class
        {
            RedisValue valueBytes = await db.StringGetAsync(key);
            T obj;
            if (!valueBytes.HasValue)
                obj = default(T);
            else
                obj = await serializer.DeserializeAsync<T>(valueBytes);
            return obj;
        }

        public bool Add<T>(string key, T value) where T : class
        {
            byte[] numArray = serializer.Serialize(value);
            return db.StringSet(key, numArray, new TimeSpan?(), When.Always, CommandFlags.FireAndForget);
        }

        public async Task<bool> AddAsync<T>(string key, T value) where T : class
        {
            byte[] entryBytes = await serializer.SerializeAsync(value);
            return await db.StringSetAsync(key, entryBytes, new TimeSpan?(), When.Always, CommandFlags.FireAndForget);
        }

        public bool Replace<T>(string key, T value) where T : class
        {
            return Add(key, value);
        }

        public Task<bool> ReplaceAsync<T>(string key, T value) where T : class
        {
            return AddAsync(key, value);
        }

        public bool Add<T>(string key, T value, DateTimeOffset expiresAt) where T : class
        {
            byte[] numArray = serializer.Serialize(value);
            TimeSpan timeSpan = expiresAt.Subtract(DateTimeOffset.Now);
            return db.StringSet(key, numArray, timeSpan, When.Always, CommandFlags.FireAndForget);
        }

        public async Task<bool> AddAsync<T>(string key, T value, DateTimeOffset expiresAt) where T : class
        {
            byte[] entryBytes = await serializer.SerializeAsync(value);
            TimeSpan expiration = expiresAt.Subtract(DateTimeOffset.Now);
            return await db.StringSetAsync(key, entryBytes, expiration, When.Always, CommandFlags.FireAndForget);
        }

        public bool Replace<T>(string key, T value, DateTimeOffset expiresAt) where T : class
        {
            return Add(key, value, expiresAt);
        }

        public Task<bool> ReplaceAsync<T>(string key, T value, DateTimeOffset expiresAt) where T : class
        {
            return AddAsync(key, value, expiresAt);
        }

        public bool Add<T>(string key, T value, TimeSpan expiresIn) where T : class
        {
            byte[] numArray = serializer.Serialize(value);
            return db.StringSet(key, numArray, expiresIn, When.Always, CommandFlags.FireAndForget);
        }

        public async Task<bool> AddAsync<T>(string key, T value, TimeSpan expiresIn) where T : class
        {
            byte[] entryBytes = await serializer.SerializeAsync(value);
            return await db.StringSetAsync(key, entryBytes, expiresIn, When.Always, CommandFlags.FireAndForget);
        }

        public bool Replace<T>(string key, T value, TimeSpan expiresIn) where T : class
        {
            return Add(key, value, expiresIn);
        }

        public Task<bool> ReplaceAsync<T>(string key, T value, TimeSpan expiresIn) where T : class
        {
            return AddAsync(key, value, expiresIn);
        }

        public IDictionary<string, T> GetAll<T>(IEnumerable<string> keys) where T : class
        {
            var keysList = keys.ToList();
            var redisKeyArray = new RedisKey[keysList.Count];
            var redisResultArray = (RedisResult[])db.ScriptEvaluate(CreateLuaScriptForMget(redisKeyArray, keysList), redisKeyArray);
            var dictionary = new Dictionary<string, T>();
            for (int index = 0; index < redisResultArray.Count(); ++index)
            {
                T obj = default(T);
                if (!redisResultArray[index].IsNull)
                    obj = serializer.Deserialize<T>(encoding.GetBytes(redisResultArray[index].ToString()));
                dictionary.Add(keysList[index], obj);
            }
            return dictionary;
        }

        public async Task<List<T>> GetAllAsync<T>(List<string> keys) where T : class
        {
            var result = new List<T>();
            List<T> list;
            try
            {
                var inputKeys = keys.Where(t => !string.IsNullOrEmpty(t)).Select(t => (RedisKey)t).ToList();
                if (inputKeys.Count == 0)
                {
                    list = result;
                }
                else
                {
                    RedisValue[] data = await db.StringGetAsync(inputKeys.ToArray());
                    result.AddRange(data.Where(item => !item.IsNull).Select(redisValue => serializer.Deserialize<T>(encoding.GetBytes(redisValue.ToString()))));
                    list = result;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return list;
        }

        public bool AddAll<T>(IList<Tuple<string, T>> items) where T : class
        {
            return db.StringSet(items.Select(item => new KeyValuePair<RedisKey, RedisValue>(item.Item1, serializer.Serialize(item.Item2))).ToArray(), When.Always, CommandFlags.FireAndForget);
        }

        public async Task<bool> AddAllAsync<T>(IList<Tuple<string, T>> items) where T : class
        {
            KeyValuePair<RedisKey, RedisValue>[] redisInput = items.Select(item => new KeyValuePair<RedisKey, RedisValue>(item.Item1, serializer.Serialize(item.Item2))).ToArray();
            bool result = await db.StringSetAsync(redisInput, When.Always, CommandFlags.FireAndForget);
            return result;
        }

        public bool SetAdd(string memberName, string key)
        {
            return db.SetAdd(memberName, key);
        }

        public Task<bool> SetAddAsync(string memberName, string key)
        {
            return db.SetAddAsync(memberName, key);
        }

        public string[] SetMember(string memberName)
        {
            return db.SetMembers(memberName).Select(x => x.ToString()).ToArray();
        }

        public async Task<string[]> SetMemberAsync(string memberName)
        {
            return (await db.SetMembersAsync(memberName)).Select(x => x.ToString()).ToArray();
        }

        public IEnumerable<string> SearchKeys(string pattern)
        {
            var hashSet = new HashSet<RedisKey>();
            foreach (EndPoint endpoint in db.Multiplexer.GetEndPoints())
            {
                foreach (RedisKey redisKey in db.Multiplexer.GetServer(endpoint).Keys(0, pattern))
                {
                    if (!hashSet.Contains(redisKey))
                        hashSet.Add(redisKey);
                }
            }
            return hashSet.Select(x => (string)x);
        }

        public Task<IEnumerable<string>> SearchKeysAsync(string pattern)
        {
            return Task.Factory.StartNew(() => SearchKeys(pattern));
        }

        public async Task<long> ListRightPushAsync<T>(string key, T value) where T : class
        {
            byte[] entryBytes = await serializer.SerializeAsync(value);
            return await db.ListRightPushAsync(key, entryBytes, When.Always, CommandFlags.FireAndForget);
        }

        public void FlushDb()
        {
            foreach (EndPoint endpoint in db.Multiplexer.GetEndPoints())
            {
                IServer server = db.Multiplexer.GetServer(endpoint);
                if (!server.IsSlave && server.IsConnected)
                    server.FlushAllDatabases();
            }
        }

        public async Task FlushDbAsync()
        {
            EndPoint[] endPoints = db.Multiplexer.GetEndPoints();
            foreach (EndPoint endpoint in endPoints)
                await db.Multiplexer.GetServer(endpoint).FlushDatabaseAsync();
            //bool flag;
            //int num = flag ? 1 : 0;
        }

        public void Save(SaveType saveType)
        {
            foreach (EndPoint endpoint in db.Multiplexer.GetEndPoints())
                db.Multiplexer.GetServer(endpoint).Save(saveType);
        }

        public async void SaveAsync(SaveType saveType)
        {
            EndPoint[] endPoints = db.Multiplexer.GetEndPoints();
            foreach (EndPoint endpoint in endPoints)
                await db.Multiplexer.GetServer(endpoint).SaveAsync(saveType);
           // bool flag;
            //int num = flag ? 1 : 0;
        }

        public Dictionary<string, string> GetInfo()
        {
            return ParseInfo(db.ScriptEvaluate("return redis.call('INFO')").ToString());
        }

        public async Task<Dictionary<string, string>> GetInfoAsync()
        {
            string info = (await db.ScriptEvaluateAsync("return redis.call('INFO')")).ToString();
            return ParseInfo(info);
        }

        public async Task<T> ListLeftPopAsync<T>(string key) where T : class
        {
            T result = default(T);
            RedisValue valueBytes = await db.ListLeftPopAsync(key);
            if (valueBytes.HasValue)
                result = await serializer.DeserializeAsync<T>(valueBytes);
            return result;
        }

        public long ListLength(string key)
        {
            return db.ListLength(key);
        }

        private string CreateLuaScriptForMset<T>(RedisKey[] redisKeys, RedisValue[] redisValues, IList<Tuple<string, T>> objects)
        {
            var stringBuilder = new StringBuilder("return redis.call('mset',");
            for (int index = 0; index < objects.Count; ++index)
            {
                redisKeys[index] = objects[index].Item1;
                redisValues[index] = serializer.Serialize(objects[index].Item2);
                stringBuilder.AppendFormat("KEYS[{0}],ARGV[{0}]", index + 1);
                if (index < objects.Count - 1)
                    stringBuilder.Append(",");
            }
            stringBuilder.Append(")");
            return stringBuilder.ToString();
        }

        private string CreateLuaScriptForMget(RedisKey[] redisKeys, List<string> keysList)
        {
            var stringBuilder = new StringBuilder("return redis.call('mget',");
            for (int index = 0; index < keysList.Count; ++index)
            {
                redisKeys[index] = keysList[index];
                stringBuilder.AppendFormat("KEYS[{0}]", index + 1);
                if (index < keysList.Count - 1)
                    stringBuilder.Append(",");
            }
            stringBuilder.Append(")");
            return stringBuilder.ToString();
        }

        private Dictionary<string, string> ParseInfo(string info)
        {
            string[] strArray = info.Split(new[]{"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
            var dictionary = new Dictionary<string, string>();
            foreach (string str1 in strArray)
            {
                if (!string.IsNullOrEmpty(str1) && str1[0] != 35)
                {
                    int length = str1.IndexOf(':');
                    if (length > 0)
                    {
                        string key = str1.Substring(0, length);
                        string str2 = str1.Substring(length + 1).Trim();
                        dictionary.Add(key, str2);
                    }
                }
            }
            return dictionary;
        }
    }
}
