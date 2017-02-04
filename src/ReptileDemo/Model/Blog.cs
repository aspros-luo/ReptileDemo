using System;
using System.Collections.Generic;

namespace ReptileDemo.Model
{
    public class Blog
    {
        public string BlogName { get; set; }
        public string BlogUrl { get; set; }
        public DateTime BlogPubTime { get; set; }
        public List<string> BlogDownLoadUrls { get; set; }
    }
}
