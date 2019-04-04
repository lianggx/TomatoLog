using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TomatoLog.Server.ViewModels
{
    public class MessageViewModel
    {
        public string Project { get; set; }
        public string Label { get; set; }
        public string Keyword { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 100;
    }
}
