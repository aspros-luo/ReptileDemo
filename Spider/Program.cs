using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace Spider
{
    class Program
    {
      
        /// <summary>
        /// 回调验证证书-总是返回true-跳过验证
        /// </summary>
     
        static void Main(string[] args)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            string cerPath = @"F:\lagou.cer";//证书所在位置
            string xmlSavePath = @"F:\lagouCrawler.xml";//xml文件存放位置

            string[] positionNames = new string[] { ".Net", "C#" };//搜索关键词组
            LagouWebCrawler lwc = new LagouWebCrawler(cerPath, xmlSavePath, positionNames);
            int[] count = lwc.CrawlerStart();
            sw.Stop();
            if (count[0] + count[1] > 0)
            {
                string str = xmlSavePath + "：用时" + sw.Elapsed + "；去重后的总搜索结果数=" + count[0] + "，搜索结果中的重复数=" + count[1];
                Console.WriteLine(str);
            }
            else
            {
                Console.WriteLine("遇到错误，详情请检查日志");
            }
            Console.ReadKey();

            //var positions = "CTO/技术总监,net,前端开发,运维,架构师,产品总监,产品经理,产品专员,交互设计师,UI设计师,运营总监,运营主管,运营专员,店小二,客服专员,平台运营,BD总监,BD经理,BD主管,BD专员,HRD,HRM,HRG,HRBP,人事专员,行政专员,前台,主办会计,会计";
            //foreach (var position in positions.Split(','))
            //{
                
            //    var jobJson = GetHtml(string.Format("https://www.lagou.com/jobs/positionAjax.json?city={0}&kd={1}", "杭州", position));

            //    var pageSize = Convert.ToInt32(jobJson["content"]["positionResult"]["resultSize"].ToString());
            //    var totalCount = Convert.ToInt32(jobJson["content"]["positionResult"]["totalCount"].ToString());
            //    var pageCount = Convert.ToInt32(totalCount / pageSize) + 1;

            //    for (int pageNo = 1; pageNo <= pageCount; pageNo++)
            //    {
            //        var jobPageResult = GetHtml(string.Format("https://www.lagou.com/jobs/positionAjax.json?city={0}&kd={1}&pn={2}", "杭州", position, pageNo));
            //        var jobs = JsonConvert.DeserializeObject<dynamic>(jobPageResult["content"]["positionResult"]["result"].ToString());
            //        foreach (var job in jobs)
            //        {
            //            //add to db
            //        }
            //    }
            //}
        }

      

    }
}
