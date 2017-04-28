using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Parser.Html;
using ReptileDemo.Infrastructure;
using ReptileDemo.Model;

namespace ReptileDemo.BizModel
{
    public class NewBlogs
    {
        private static readonly List<string> Lists = new List<string>();
        public static void AddBlogs()
        {
            //Create a (re-usable) parser front-end
            var parser = new HtmlParser();
            //Source to be pared
            var source = HttpHelper.GetHtmlByUrl("https://www.lagou.com/jobs/positionAjax.json?city=杭州&kd=.net");
            //var source = "<h1>Some example source</h1><p>This is a paragraph element";
            //Parse source to document
            var document = parser.Parse(source);
            //Do something with document like the following
            var divInfos = document.QuerySelectorAll("div.post_item");
            foreach (var divInfo in divInfos)
            {
                Console.WriteLine("---------------------");
                var links = divInfo.QuerySelectorAll("a.titlelnk").ToList();
                foreach (var a in links)
                {
                    Lists.Add(a.GetAttribute("href"));
                    Console.WriteLine(a.GetAttribute("href"));
                }
            }
            JsonHelper.WriteJsonFile(Lists);
        }
    }
}
