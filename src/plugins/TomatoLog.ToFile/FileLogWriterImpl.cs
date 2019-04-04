using TomatoLog.Common.Config;
using TomatoLog.Common.Repository;
using TomatoLog.Common.Utilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace TomatoLog.ToFile
{
    public class FileLogWriterImpl : LogWriter
    {
        public FileLogWriterImpl(StorageOptions options, ILogger log) : base(options, log)
        {
        }

        private string CreateFile(LogMessage message)
        {
            var path = Path.Combine(options.File, message.ProjectName);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var fileName = string.Format("{1}-{2}.log", path, message.LogLevel, DateTime.Now.ToString("yyyy-MM-dd"));
            var fullFileName = Path.Combine(path, fileName);
            return fullFileName;
        }

        private List<string> GetDir(string path)
        {
            List<string> dirs = new List<string>();
            if (Directory.Exists(path))
            {
                dirs.AddRange(Directory.GetDirectories(path));
            }

            return dirs;
        }

        public override async Task<int> Write(LogMessage message)
        {
            int affrows = 0;
            try
            {
                var fileName = CreateFile(message);
                var log = JsonConvert.SerializeObject(message, Formatting.None);
                using (FileStream fs = new FileStream(fileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                {
                    StreamWriter sw = new StreamWriter(fs);
                    await sw.WriteLineAsync(log);
                    await sw.FlushAsync();
                }
            }
            catch (Exception e)
            {
                logger.LogError(e.Message, e);
            }
            return affrows;
        }

        public override async Task<List<string>> GetProjects()
        {
            List<string> result = new List<string>();
            var dir = Path.Combine(options.File);
            if (Directory.Exists(dir))
            {
                var dirs = Directory.GetDirectories(dir);
                result.AddRange(dirs.Select(d => d.Substring(d.LastIndexOf('\\') + 1)));
            }

            return result.OrderBy(f => f).ToList();
        }

        public override async Task<List<string>> GetLabels(string proj)
        {
            List<string> fileList = new List<string>();
            try
            {
                List<FileDesc> fdList = new List<FileDesc>();
                var dir = Path.Combine(options.File, proj);
                if (Directory.Exists(dir))
                {
                    var files = Directory.GetFiles(dir);
                    foreach (var file in files)
                    {
                        FileInfo fi = new FileInfo(file);
                        FileDesc fd = new FileDesc
                        {
                            FileName = fi.Name,
                            Length = fi.Length,
                            ModifyTime = fi.LastWriteTime
                        };
                        fdList.Add(fd);
                    }
                }
                var orders = fdList.OrderByDescending(f => f.ModifyTime);
                foreach (var od in orders)
                {
                    fileList.Add(JsonConvert.SerializeObject(od));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);
            }
            return fileList;
        }

        public override async Task<List<string>> List(string proj, string logLevel, string keyWrod = null, int page = 1, int pageSize = 100)
        {
            page = page <= 1 ? 0 : page;
            int startLine = page * pageSize;
            List<string> result = new List<string>();
            var fileName = Path.Combine(options.File, proj, logLevel);
            try
            {
                ReverseLineReader reader = new ReverseLineReader(fileName);
                var doc = reader.GetEnumerator();
                int index = 0;
                while (doc.MoveNext())
                {
                    index++;
                    if (index > startLine)
                    {
                        if (!string.IsNullOrEmpty(keyWrod))
                        {
                            if (doc.Current.Contains(keyWrod))
                                result.Add(doc.Current);
                        }
                        else
                            result.Add(doc.Current);
                    }

                    if (result.Count == pageSize) break;
                }
            }
            catch (NotSupportedException ex)
            {
                logger.LogError(ex.Message, ex);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);
            }
            return result;
        }
    }
}