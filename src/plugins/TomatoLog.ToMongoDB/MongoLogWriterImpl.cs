using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using TomatoLog.Common.Config;
using TomatoLog.Common.Repository;
using TomatoLog.Common.Utilities;

namespace TomatoLog.ToMongoDB
{
    public class MongoLogWriterImpl : LogWriter
    {
        private readonly MongoClient client;
        public MongoLogWriterImpl(StorageOptions options, ILogger log) : base(options, log)
        {
            client = new MongoClient(options.MongoDB);
            var serializer = new MongoDB.Bson.Serialization.Serializers.DateTimeSerializer(DateTimeKind.Local, BsonType.DateTime);
            BsonSerializer.RegisterSerializer(typeof(DateTime), serializer);
        }

        public override async Task<List<string>> GetLabels(string proj)
        {
            var collections = new List<string>();
            var result = await client.GetDatabase(proj).ListCollectionNamesAsync();
            foreach (var od in result.ToList())
            {
                FileDesc fd = new FileDesc
                {
                    FileName = od,
                    Length = 0,
                    ModifyTime = DateTime.Now
                };
                collections.Add(JsonSerializer.Serialize(fd));
            }
            return collections;
        }

        public override async Task<List<string>> GetProjects()
        {
            var dbList = await client.ListDatabaseNamesAsync();
            return await dbList.ToListAsync();
        }

        /// <summary>
        ///  List
        /// </summary>
        /// <param name="proj"></param>
        /// <param name="logLevel"></param>
        /// <param name="keyWrod">json condition</param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public override async Task<List<string>> List(string proj, string logLevel, string keyWrod = null, int page = 1, int pageSize = 100)
        {
            List<string> result = new List<string>();
            try
            {
                var filter = string.IsNullOrEmpty(keyWrod) ? new BsonDocument() : BsonDocument.Parse(keyWrod);
                page = page <= 1 ? 0 : page;
                var coll = await client.GetDatabase(proj).GetCollection<BsonDocument>(logLevel)
                                                   .Find(filter)
                                                   .SortByDescending(f => f["Timestamp"])
                                                   .Skip(page * pageSize)
                                                   .Limit(pageSize)
                                                   .ToListAsync();
                foreach (var bson in coll)
                {
                    bson.RemoveAt(0);
                    var log = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<LogMessage>(bson.ToJson());
                    result.Add(JsonSerializer.Serialize(log));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);
            }
            return result;
        }

        public override async Task<int> Write(LogMessage message)
        {
            int affrows = 0;
            try
            {
                var db = client.GetDatabase(message.ProjectName);
                var collections = db.GetCollection<BsonDocument>(message.LogLevel.ToString());
                var doc = message.ToBsonDocument();
                await collections.InsertOneAsync(doc);
                affrows++;
            }
            catch (Exception e)
            {
                logger.LogError(e.Message, e);
            }
            return affrows;
        }
    }
}