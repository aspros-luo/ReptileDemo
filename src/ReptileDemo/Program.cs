using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Scheduler;
using DotnetSpider.Core.Selector;
using ReptileDemo.BizModel;

namespace ReptileDemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            NewBlogs.AddBlogs();
        }
    }
}
