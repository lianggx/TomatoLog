using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TomatoLog.Common.Config;
using TomatoLog.Common.Repository;
using TomatoLog.Common.Utilities;

namespace TomatoLog.ToFile
{
    public class FileLogWriterImpl : LogWriter
    {
        private const int period = 3000;
        private const int dueTime = 5000;
        private readonly Timer timer = null;
        private readonly ConcurrentQueue<LogMessage> queue = null;
        private bool runing = false;
        public FileLogWriterImpl(StorageOptions options, ILogger log) : base(options, log)
        {
            timer = new Timer(new TimerCallback(TimerCallbackFlush));
            timer.Change(dueTime, period);
            queue = new ConcurrentQueue<LogMessage>();
        }

        private void TimerCallbackFlush(object sender)
        {
            if (runing)
                return;
            else
                runing = true;

            try
            {
                int count = queue.Count;
                if (count == 0)
                {
                    runing = false;
                    return;
                }
                for (int i = 0; i < count; i++)
                {
                    queue.TryDequeue(out LogMessage message);
                    if (message == null)
                        continue;

                    var fileName = CreateFile(message);
                    using (FileStream fs = new FileStream(fileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    {
                        var log = JsonSerializer.Serialize(message);
                        StreamWriter sw = new StreamWriter(fs);
                        sw.WriteLine(log);
                        sw.Flush();
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError(e.Message, e);
            }
            runing = false;
        }

        private string CreateFile(LogMessage message)
        {
            var path = Path.Combine(options.File, message.ProjectName);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var fileName = string.Format("{0}-{1}.log", message.LogLevel, DateTime.Now.ToString("yyyy-MM-dd"));
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
            try
            {
                queue.Enqueue(message);
                return queue.Count;
            }
            catch (Exception e)
            {
                logger.LogError(e.Message, e);
            }
            return 0;
        }

        public override async Task<List<string>> GetProjects()
        {
            List<string> result = new List<string>();
            var dir = Path.Combine(options.File);
            if (Directory.Exists(dir))
            {
                var dirs = Directory.GetDirectories(dir);
                result.AddRange(dirs.Select(d => d[(d.LastIndexOf('\\') + 1)..]));
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
                        //var fName = file.Replace(proj, "");
                        var subChar = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\\" : "/";
                        var fName = file.Substring(file.LastIndexOf(subChar)+1);
                        FileDesc fd = new FileDesc
                        {
                            FileName = fName,
                            Length = 0,
                            ModifyTime = DateTime.Now
                        };
                        fdList.Add(fd);
                    }
                }
                var orders = fdList.OrderByDescending(f => f.ModifyTime);
                foreach (var od in orders)
                {
                    fileList.Add(JsonSerializer.Serialize(od));
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