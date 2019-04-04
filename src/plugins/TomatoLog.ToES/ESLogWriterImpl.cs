using Elasticsearch.Net;
using TomatoLog.Common.Config;
using TomatoLog.Common.Repository;
using TomatoLog.Common.Utilities;
using Microsoft.Extensions.Logging;
using Nest;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;

namespace TomatoLog.ToES
{
    public class ESLogWriterImpl : LogWriter
    {
        private ElasticClient client;
        public ESLogWriterImpl(StorageOptions options, ILogger log) : base(options, log)
        {
            Check.NotNull(options.ES, nameof(options.ES));

            var uris = options.ES.Split(';').Select(s => new Uri(s));
            var pool = new StaticConnectionPool(uris);
            var settings = new ConnectionSettings(pool).DefaultIndex("tomatolog-server");
            settings.DisableDirectStreaming(true);
            settings.SniffOnStartup(false);
            client = new ElasticClient(settings);
        }

        /// <summary>
        /// 未实现，请使用 List 方法进行查询
        /// </summary>
        /// <param name="proj"></param>
        /// <returns></returns>
        public override Task<List<string>> GetLabels(string proj)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///  未实现，请使用 List 方法进行查询
        /// </summary>
        /// <returns></returns>
        public override Task<List<string>> GetProjects()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 查询日志
        /// </summary>
        /// <param name="proj"></param>
        /// <param name="logLevel"></param>
        /// <param name="keyWrod">ES Json 查询语法</param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async override Task<List<string>> List(string proj, string logLevel, string keyWrod = null, int page = 1, int pageSize = 100)
        {
            page = page <= 1 ? 0 : page;
            List<string> result = new List<string>();
            try
            {
                var descriptor = new SearchDescriptor<LogMessage>();
                descriptor.Index(proj.ToLower())
                          .Type(logLevel.ToLower())
                          .Sort(f => f.Descending(l => l.Timestamp))
                          .From(1)
                          .Size(pageSize);
                if (!string.IsNullOrEmpty(keyWrod))
                    descriptor.Query(q => q.SimpleQueryString(s => s.Query(keyWrod)));

                var res = await client.SearchAsync<LogMessage>(descriptor);
                result.AddRange(res.Hits.Select(s => JsonConvert.SerializeObject(s.Source)));
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);
            }
            return result;
        }

        public async override Task<int> Write(LogMessage message)
        {
            var affrows = 0;
            try
            {
                IndexName indexName = message.ProjectName.ToLower();
                TypeName typeName = message.LogLevel.ToString().ToLower();

                if (!client.IndexExists(indexName).Exists)
                {
                    IIndexState indexState = new IndexState
                    {
                        Settings = new IndexSettings
                        {
                            NumberOfReplicas = 1,//副本数
                            NumberOfShards = 6//分片数
                        }
                    };

                    ICreateIndexResponse response = client.CreateIndex(indexName, p => p
                    .InitializeUsing(indexState)
                    .Mappings(m => m.Map<LogMessage>(r => r.AutoMap()))
                );
                }

                IndexRequest<LogMessage> req = new IndexRequest<LogMessage>(indexName, typeName);
                var res = await client.IndexAsync(message, selector => selector.Type(typeName).Index(indexName));
                if (res.Result == Result.Created)
                    affrows = 1;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);
            }


            return affrows;
        }
    }
}