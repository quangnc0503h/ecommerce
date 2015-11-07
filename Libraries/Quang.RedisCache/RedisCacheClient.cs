using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StackExchange.Redis;
using System.Net;
using System.IO;

namespace Quang.RedisCache
{
    public class RedisCacheClient : ICacheClient, IDisposable
    {
        private static readonly Encoding encoding = Encoding.UTF8;
        private readonly ConnectionMultiplexer connectionMultiplexer;
        private readonly IDatabase db;
        private readonly ISerializer serializer;

        public IDatabase Database
        {
            get
            {
                return this.db;
            }
        }

        public ISerializer Serializer
        {
            get
            {
                return this.serializer;
            }
        }

        public RedisCacheClient(string connectionString, ISerializer serializer = null, int database = 0)
        {
            if (serializer == null)
                serializer = (ISerializer)new JilSerializer();
            this.serializer = serializer;
            this.connectionMultiplexer = ConnectionMultiplexer.Connect(connectionString, (TextWriter)null);
            this.db = this.connectionMultiplexer.GetDatabase(database, (object)null);
        }

        public RedisCacheClient(ISerializer serializer, string connectionString, int database = 0)
        {
            if (serializer == null)
                throw new ArgumentNullException("serializer");
            this.serializer = serializer;
            this.connectionMultiplexer = ConnectionMultiplexer.Connect(connectionString, (TextWriter)null);
            this.db = this.connectionMultiplexer.GetDatabase(database, (object)null);
        }

        public RedisCacheClient(ConnectionMultiplexer connectionMultiplexer, ISerializer serializer, int database = 0)
        {
            if (connectionMultiplexer == null)
                throw new ArgumentNullException("connectionMultiplexer");
            if (serializer == null)
                throw new ArgumentNullException("serializer");
            this.serializer = serializer;
            this.connectionMultiplexer = connectionMultiplexer;
            this.db = connectionMultiplexer.GetDatabase(database, (object)null);
        }

        public void Dispose()
        {
            this.connectionMultiplexer.Dispose();
        }

        public bool Exists(string key)
        {
            return this.db.KeyExists((RedisKey)key, CommandFlags.None);
        }

        public Task<bool> ExistsAsync(string key)
        {
            return this.db.KeyExistsAsync((RedisKey)key, CommandFlags.None);
        }

        public bool Remove(string key)
        {
            return this.db.KeyDelete((RedisKey)key, CommandFlags.None);
        }

        public Task<bool> RemoveAsync(string key)
        {
            return this.db.KeyDeleteAsync((RedisKey)key, CommandFlags.None);
        }

        public void RemoveAll(IEnumerable<string> keys)
        {
            LinqExtensions.ForEach<string>(keys, (Action<string>)(x => this.Remove(x)));
        }

        public Task RemoveAllAsync(IEnumerable<string> keys)
        {
            return LinqExtensions.ForEachAsync<string>(keys, new Func<string, Task>(this.RemoveAsync));
        }

        public T Get<T>(string key) where T : class
        {
            RedisValue redisValue = this.db.StringGet((RedisKey)key, CommandFlags.None);
            if (!redisValue.HasValue)
                return default(T);
            return this.serializer.Deserialize<T>((byte[])redisValue);
        }

        public async Task<T> GetAsync<T>(string key) where T : class
        {
            RedisValue valueBytes = await this.db.StringGetAsync((RedisKey)key, CommandFlags.None);
            T obj;
            if (!valueBytes.HasValue)
                obj = default(T);
            else
                obj = await this.serializer.DeserializeAsync<T>((byte[])valueBytes);
            return obj;
        }

        public bool Add<T>(string key, T value) where T : class
        {
            byte[] numArray = this.serializer.Serialize((object)value);
            return this.db.StringSet((RedisKey)key, (RedisValue)numArray, new TimeSpan?(), When.Always, CommandFlags.FireAndForget);
        }

        public async Task<bool> AddAsync<T>(string key, T value) where T : class
        {
            byte[] entryBytes = await this.serializer.SerializeAsync((object)(T)value);
            return await this.db.StringSetAsync((RedisKey)key, (RedisValue)entryBytes, new TimeSpan?(), When.Always, CommandFlags.FireAndForget);
        }

        public bool Replace<T>(string key, T value) where T : class
        {
            return this.Add<T>(key, value);
        }

        public Task<bool> ReplaceAsync<T>(string key, T value) where T : class
        {
            return this.AddAsync<T>(key, value);
        }

        public bool Add<T>(string key, T value, DateTimeOffset expiresAt) where T : class
        {
            byte[] numArray = this.serializer.Serialize((object)value);
            TimeSpan timeSpan = expiresAt.Subtract(DateTimeOffset.Now);
            return this.db.StringSet((RedisKey)key, (RedisValue)numArray, new TimeSpan?(timeSpan), When.Always, CommandFlags.FireAndForget);
        }

        public async Task<bool> AddAsync<T>(string key, T value, DateTimeOffset expiresAt) where T : class
        {
            byte[] entryBytes = await this.serializer.SerializeAsync((object)(T)value);
            TimeSpan expiration = expiresAt.Subtract(DateTimeOffset.Now);
            return await this.db.StringSetAsync((RedisKey)key, (RedisValue)entryBytes, new TimeSpan?(expiration), When.Always, CommandFlags.FireAndForget);
        }

        public bool Replace<T>(string key, T value, DateTimeOffset expiresAt) where T : class
        {
            return this.Add<T>(key, value, expiresAt);
        }

        public Task<bool> ReplaceAsync<T>(string key, T value, DateTimeOffset expiresAt) where T : class
        {
            return this.AddAsync<T>(key, value, expiresAt);
        }

        public bool Add<T>(string key, T value, TimeSpan expiresIn) where T : class
        {
            byte[] numArray = this.serializer.Serialize((object)value);
            return this.db.StringSet((RedisKey)key, (RedisValue)numArray, new TimeSpan?(expiresIn), When.Always, CommandFlags.FireAndForget);
        }

        public async Task<bool> AddAsync<T>(string key, T value, TimeSpan expiresIn) where T : class
        {
            byte[] entryBytes = await this.serializer.SerializeAsync((object)(T)value);
            return await this.db.StringSetAsync((RedisKey)key, (RedisValue)entryBytes, new TimeSpan?(expiresIn), When.Always, CommandFlags.FireAndForget);
        }

        public bool Replace<T>(string key, T value, TimeSpan expiresIn) where T : class
        {
            return this.Add<T>(key, value, expiresIn);
        }

        public Task<bool> ReplaceAsync<T>(string key, T value, TimeSpan expiresIn) where T : class
        {
            return this.AddAsync<T>(key, value, expiresIn);
        }

        public IDictionary<string, T> GetAll<T>(IEnumerable<string> keys) where T : class
        {
            List<string> keysList = Enumerable.ToList<string>(keys);
            RedisKey[] redisKeyArray = new RedisKey[keysList.Count];
            RedisResult[] redisResultArray = (RedisResult[])this.db.ScriptEvaluate(this.CreateLuaScriptForMget(redisKeyArray, keysList), redisKeyArray, (RedisValue[])null, CommandFlags.None);
            Dictionary<string, T> dictionary = new Dictionary<string, T>();
            for (int index = 0; index < Enumerable.Count<RedisResult>((IEnumerable<RedisResult>)redisResultArray); ++index)
            {
                T obj = default(T);
                if (!redisResultArray[index].IsNull)
                    obj = this.serializer.Deserialize<T>(RedisCacheClient.encoding.GetBytes(redisResultArray[index].ToString()));
                dictionary.Add(keysList[index], obj);
            }
            return (IDictionary<string, T>)dictionary;
        }

        public async Task<List<T>> GetAllAsync<T>(List<string> keys) where T : class
        {
            List<T> result = new List<T>();
            List<T> list;
            try
            {
                List<RedisKey> inputKeys = Enumerable.ToList<RedisKey>(Enumerable.Select<string, RedisKey>(Enumerable.Where<string>((IEnumerable<string>)keys, (Func<string, bool>)(t => !string.IsNullOrEmpty(t))), (Func<string, RedisKey>)(t => (RedisKey)t)));
                if (inputKeys.Count == 0)
                {
                    list = result;
                }
                else
                {
                    RedisValue[] data = await this.db.StringGetAsync(inputKeys.ToArray(), CommandFlags.None);
                    result.AddRange(Enumerable.Select<RedisValue, T>(Enumerable.Where<RedisValue>((IEnumerable<RedisValue>)data, (Func<RedisValue, bool>)(item => !item.IsNull)), (Func<RedisValue, T>)(redisValue => this.serializer.Deserialize<T>(RedisCacheClient.encoding.GetBytes(redisValue.ToString())))));
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
            return this.db.StringSet(Enumerable.ToArray<KeyValuePair<RedisKey, RedisValue>>(Enumerable.Select<Tuple<string, T>, KeyValuePair<RedisKey, RedisValue>>((IEnumerable<Tuple<string, T>>)items, (Func<Tuple<string, T>, KeyValuePair<RedisKey, RedisValue>>)(item => new KeyValuePair<RedisKey, RedisValue>((RedisKey)item.Item1, (RedisValue)this.serializer.Serialize((object)item.Item2))))), When.Always, CommandFlags.FireAndForget);
        }

        public async Task<bool> AddAllAsync<T>(IList<Tuple<string, T>> items) where T : class
        {
            KeyValuePair<RedisKey, RedisValue>[] redisInput = Enumerable.ToArray<KeyValuePair<RedisKey, RedisValue>>(Enumerable.Select<Tuple<string, T>, KeyValuePair<RedisKey, RedisValue>>((IEnumerable<Tuple<string, T>>)items, (Func<Tuple<string, T>, KeyValuePair<RedisKey, RedisValue>>)(item => new KeyValuePair<RedisKey, RedisValue>((RedisKey)item.Item1, (RedisValue)this.serializer.Serialize((object)item.Item2)))));
            bool result = await this.db.StringSetAsync(redisInput, When.Always, CommandFlags.FireAndForget);
            return result;
        }

        public bool SetAdd(string memberName, string key)
        {
            return this.db.SetAdd((RedisKey)memberName, (RedisValue)key, CommandFlags.None);
        }

        public Task<bool> SetAddAsync(string memberName, string key)
        {
            return this.db.SetAddAsync((RedisKey)memberName, (RedisValue)key, CommandFlags.None);
        }

        public string[] SetMember(string memberName)
        {
            return Enumerable.ToArray<string>(Enumerable.Select<RedisValue, string>((IEnumerable<RedisValue>)this.db.SetMembers((RedisKey)memberName, CommandFlags.None), (Func<RedisValue, string>)(x => x.ToString())));
        }

        public async Task<string[]> SetMemberAsync(string memberName)
        {
            return Enumerable.ToArray<string>(Enumerable.Select<RedisValue, string>((IEnumerable<RedisValue>)await this.db.SetMembersAsync((RedisKey)memberName, CommandFlags.None), (Func<RedisValue, string>)(x => x.ToString())));
        }

        public IEnumerable<string> SearchKeys(string pattern)
        {
            HashSet<RedisKey> hashSet = new HashSet<RedisKey>();
            foreach (EndPoint endpoint in this.db.Multiplexer.GetEndPoints(false))
            {
                foreach (RedisKey redisKey in this.db.Multiplexer.GetServer(endpoint, (object)null).Keys(0, (RedisValue)pattern, 10, 0L, 0, CommandFlags.None))
                {
                    if (!hashSet.Contains(redisKey))
                        hashSet.Add(redisKey);
                }
            }
            return Enumerable.Select<RedisKey, string>((IEnumerable<RedisKey>)hashSet, (Func<RedisKey, string>)(x => (string)x));
        }

        public Task<IEnumerable<string>> SearchKeysAsync(string pattern)
        {
            return Task.Factory.StartNew<IEnumerable<string>>((Func<IEnumerable<string>>)(() => this.SearchKeys(pattern)));
        }

        public async Task<long> ListRightPushAsync<T>(string key, T value) where T : class
        {
            byte[] entryBytes = await this.serializer.SerializeAsync((object)(T)value);
            return await this.db.ListRightPushAsync((RedisKey)key, (RedisValue)entryBytes, When.Always, CommandFlags.FireAndForget);
        }

        public void FlushDb()
        {
            foreach (EndPoint endpoint in this.db.Multiplexer.GetEndPoints(false))
            {
                IServer server = this.db.Multiplexer.GetServer(endpoint, (object)null);
                if (!server.IsSlave && server.IsConnected)
                    server.FlushAllDatabases(CommandFlags.None);
            }
        }

        public async Task FlushDbAsync()
        {
            EndPoint[] endPoints = this.db.Multiplexer.GetEndPoints(false);
            foreach (EndPoint endpoint in endPoints)
                await this.db.Multiplexer.GetServer(endpoint, (object)null).FlushDatabaseAsync(0, CommandFlags.None);
            //bool flag;
            //int num = flag ? 1 : 0;
        }

        public void Save(SaveType saveType)
        {
            foreach (EndPoint endpoint in this.db.Multiplexer.GetEndPoints(false))
                this.db.Multiplexer.GetServer(endpoint, (object)null).Save(saveType, CommandFlags.None);
        }

        public async void SaveAsync(SaveType saveType)
        {
            EndPoint[] endPoints = this.db.Multiplexer.GetEndPoints(false);
            foreach (EndPoint endpoint in endPoints)
                await this.db.Multiplexer.GetServer(endpoint, (object)null).SaveAsync(saveType, CommandFlags.None);
           // bool flag;
            //int num = flag ? 1 : 0;
        }

        public Dictionary<string, string> GetInfo()
        {
            return this.ParseInfo(this.db.ScriptEvaluate("return redis.call('INFO')", (RedisKey[])null, (RedisValue[])null, CommandFlags.None).ToString());
        }

        public async Task<Dictionary<string, string>> GetInfoAsync()
        {
            string info = (await this.db.ScriptEvaluateAsync("return redis.call('INFO')", (RedisKey[])null, (RedisValue[])null, CommandFlags.None)).ToString();
            return this.ParseInfo(info);
        }

        public async Task<T> ListLeftPopAsync<T>(string key) where T : class
        {
            T result = default(T);
            RedisValue valueBytes = await this.db.ListLeftPopAsync((RedisKey)key, CommandFlags.None);
            if (valueBytes.HasValue)
                result = await this.serializer.DeserializeAsync<T>((byte[])valueBytes);
            return result;
        }

        public long ListLength(string key)
        {
            return this.db.ListLength((RedisKey)key, CommandFlags.None);
        }

        private string CreateLuaScriptForMset<T>(RedisKey[] redisKeys, RedisValue[] redisValues, IList<Tuple<string, T>> objects)
        {
            StringBuilder stringBuilder = new StringBuilder("return redis.call('mset',");
            for (int index = 0; index < objects.Count; ++index)
            {
                redisKeys[index] = (RedisKey)objects[index].Item1;
                redisValues[index] = (RedisValue)this.serializer.Serialize((object)objects[index].Item2);
                stringBuilder.AppendFormat("KEYS[{0}],ARGV[{0}]", (object)(index + 1));
                if (index < objects.Count - 1)
                    stringBuilder.Append(",");
            }
            stringBuilder.Append(")");
            return stringBuilder.ToString();
        }

        private string CreateLuaScriptForMget(RedisKey[] redisKeys, List<string> keysList)
        {
            StringBuilder stringBuilder = new StringBuilder("return redis.call('mget',");
            for (int index = 0; index < keysList.Count; ++index)
            {
                redisKeys[index] = (RedisKey)keysList[index];
                stringBuilder.AppendFormat("KEYS[{0}]", (object)(index + 1));
                if (index < keysList.Count - 1)
                    stringBuilder.Append(",");
            }
            stringBuilder.Append(")");
            return stringBuilder.ToString();
        }

        private Dictionary<string, string> ParseInfo(string info)
        {
            string[] strArray = info.Split(new string[1]
            {
        "\r\n"
            }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            for (int index = 0; index < strArray.Length; ++index)
            {
                string str1 = strArray[index];
                if (!string.IsNullOrEmpty(str1) && (int)str1[0] != 35)
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
