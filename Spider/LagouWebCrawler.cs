using System;
using System.Collections.Generic;
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
using AngleSharp.Parser.Html;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Spider
{
    class LagouWebCrawler
    {
        string CerPath;//网站证书本地保存地址
        string XmlSavePath;//xml保存地址
        string[] PositionNames;//关联关键词组

        public LagouWebCrawler(string _cerPath, string _xmlSavePath, string[] _positionNames)
        {
            this.CerPath = _cerPath;
            this.XmlSavePath = _xmlSavePath;
            
            this.PositionNames = _positionNames;
        }
        private bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) { return true; }

        XDocument XWrite;//一组关联词搜索的所有职位放入一个XML文件中
        XElement XJobs;//XDocument根节点
        List<int> IndexKey;//寄存职位索引键，用于查重。
        int CountRepeat = 0;//搜索结果中的重复职位数
        int CountAdd = 0;//去重后的总职位数
                         /// <summary>
                         /// 爬取一组关联关键词的数据，格式清洗后存为xml文档
                         /// </summary>
                         /// <returns>int[0]+int[1]=总搜索结果数；int[0]=去重后的结果数；int[1]=重复数</returns>
        public int[] CrawlerStart()
        {
            XWrite = new XDocument();
            XJobs = new XElement("Jobs");//根节点
            IndexKey = new List<int>();

          
            foreach (string positionName in PositionNames)//挨个用词组中的关键词搜索
            {
                for (int i = 1; i <= 30; i++)//单个词搜索结果最多展示30页
                {
                    var jobsPageUrl = "https://www.lagou.com/jobs/positionAjax.json?city=杭州&kd=.net";
                    //string jobsPageUrl = "https://www.lagou.com/jobs/positionAjax.json?px=new&needAddtionalResult=false&first=false&kd=" + positionName + "&pn=" + i;
                    //回调证书验证-总是接受-跳过验证
                    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                    string json = GetHTMLToString(jobsPageUrl, CerPath);//爬取单页
                    Match math = Regex.Match(json, @"\[[\S\s]+\]");//贪婪模式匹配，取得单页职位数组，每个职位信息为json字符串。
                    if (!math.Success) { break; }//若搜索结果不足30页，超出末页时终止当前遍历；或出现异常返回空字符串时终止。
                    json = "{\"result\":" + math.Value + "}";
                    //JavaScriptSerializer jss = new JavaScriptSerializer();
                    try
                    {
                        Dictionary<string, object> jsonObj = (Dictionary<string, object>)JsonConvert.DeserializeObject(json);//序列化为多层级的object（字典）对象
                        foreach (var dict in (object[])jsonObj["result"])//对初级对象（职位集合）进行遍历
                        {
                            Dictionary<string, object> dtTemp = (Dictionary<string, object>)dict;
                            Dictionary<string, string> dt = new Dictionary<string, string>();
                            foreach (KeyValuePair<string, object> item in dtTemp)//职位信息中某些键的值可能为空或者也是个数组对象，需要转换成字符
                            {
                                string str = null;
                                if (item.Value == null)
                                {
                                    str = "";
                                }
                                else if (item.Value.ToString() == "System.Object[]")
                                {
                                    str = string.Join(" ", (object[])item.Value);
                                }
                                else
                                {
                                    str = item.Value.ToString();
                                }
                                dt[item.Key] = ReplaceIllegalClar(str);//清理特殊字符
                            }
                            if (!JobCopyToXML(dt))//将单个职位信息添加到XML根节点下。
                            {
                                return new int[] { 0, 0 };//如果失败直接退出
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                        return new int[] { 0, 0 };//如果失败直接退出
                    }
                }
            }
            try
            {
                if (CountAdd > 0)//可能关键词搜不到内容
                {
                    XWrite.Add(XJobs);//将根节点添加进XDocument
                                      //XmlDocument doc = new XmlDocument();
                                      //doc.Normalize();
                    XWrite.Save(XmlSavePath);

                }
                return new int[] { CountAdd, CountRepeat };
            }
            catch (Exception ex)
            {
                return new int[] { 0, 0 };
            }
            return new int[] { CountAdd, CountRepeat };
        }
        /// <summary>
        /// 将每个职位数据清洗后添加到XDocument对象的根节点下
        /// </summary>
        private bool JobCopyToXML(Dictionary<string, string> dt)
        {
            int id = Convert.ToInt32(dt["positionId"]);//职位详情页的文件名，当作索引键
            if (IndexKey.Contains(id))//用不同关联词搜出的职位可能有重复。
            {
                CountRepeat++;// 新增重复职位统计
                return true;
            }
            IndexKey.Add(id);//添加一个索引
            XElement xjob = new XElement("OneJob");
            xjob.SetAttributeValue("id", id);
            string positionUrl = @"https://www.lagou.com/jobs/" + id + ".html";//职位主页
            try
            {
                xjob.SetElementValue("职位名称", dt["positionName"]);
                xjob.SetElementValue("薪酬范围", dt["salary"]);
                xjob.SetElementValue("经验要求", dt["workYear"]);
                xjob.SetElementValue("学历要求", dt["education"]);
                xjob.SetElementValue("工作城市", dt["city"]);
                xjob.SetElementValue("工作性质", dt["jobNature"]);
                xjob.SetElementValue("发布时间", Regex.Match(dt["createTime"].ToString(), @"[\d]{4}-[\d]{1,2}-[\d]{1,2}").Value);
                xjob.SetElementValue("职位主页", positionUrl);
                xjob.SetElementValue("职位诱惑", dt["positionAdvantage"]);
                string html = GetHTMLToString(positionUrl, CerPath);//从职位主页爬取职位和企业的补充信息
                var dom = new HtmlParser().Parse(html);//HTML解析成IDocument,使用Nuget AngleSharp 安装包
                                                       //QuerySelector ：选择器语法 ，根据选择器选择dom元素，获取元素中的文本并进行格式清洗
                xjob.SetElementValue("工作部门", dom.QuerySelector("div.company").TextContent.Replace((string)dt["companyShortName"], "").Replace("招聘", ""));
                xjob.SetElementValue("工作地点", dom.QuerySelector("div.work_addr").TextContent.Replace("\n", "").Replace(" ", "").Replace("查看地图", ""));
                string temp = dom.QuerySelector("dd.job_bt>div").TextContent;//职位描述，分别去除多余的空格和换行符
                temp = string.Join(" ", temp.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                xjob.SetElementValue("职位描述", string.Join("\n", temp.Split(new string[] { "\n ", " \n", "\n" }, StringSplitOptions.RemoveEmptyEntries)));
                xjob.SetElementValue("企业官网", dom.QuerySelector("ul.c_feature a[rel=nofollow]").TextContent);
                xjob.SetElementValue("企业简称", dt["companyShortName"]);
                xjob.SetElementValue("企业全称", dt["companyFullName"]);
                xjob.SetElementValue("企业规模", dt["companySize"]);
                xjob.SetElementValue("发展阶段", dt["financeStage"]);
                xjob.SetElementValue("所属领域", dt["industryField"]);
                xjob.SetElementValue("企业主页", @"https://www.lagou.com/gongsi/" + dt["companyId"] + ".html");
                XJobs.Add(xjob);
                CountAdd++;//新增职位统计
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("职位转换为XElement时出错，文件：" + XmlSavePath + "，Id=" + id + ",错误信息：" + ex);
                return false;
            }
        }

        /// <summary>
        /// Get方式请求url，获取报文，转换为string格式
        /// </summary>
        private string GetHTMLToString(string url, string path)
        {
            Thread.Sleep(1500);//尽量模仿人正常的浏览行为，每次进来先休息1.5秒，防止拉勾网因为访问太频繁屏蔽本地IP
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.ClientCertificates.Add(new X509Certificate(path));//添加证书
                request.Method = "GET";
                request.KeepAlive = true;
                request.Accept = "application/json, text/javascript, */*; q=0.01";
                request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.81 Safari/537.36";
                request.Credentials = CredentialCache.DefaultCredentials;//添加身份验证
                //request.AllowAutoRedirect = false;
                byte[] responseByte = null;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (MemoryStream _stream = new MemoryStream())
                {
                    response.GetResponseStream().CopyTo(_stream);
                    responseByte = _stream.ToArray();
                }
                string html = Encoding.UTF8.GetString(responseByte);
                return ReplaceIllegalClar(html);//进行特殊字符处理
            }
            catch (Exception ex)
            {
                Console.WriteLine("网页：" + url + "，爬取时出现错误：" + ex);
                return "";
            }
        }

        private string ReplaceIllegalClar(string html)
        {
            StringBuilder info = new StringBuilder();
            foreach (char cc in html)
            {
                int ss = (int)cc;
                if (((ss >= 0) && (ss <= 8)) || ((ss >= 11) && (ss <= 12)) || ((ss >= 14) && (ss <= 31)))
                    info.AppendFormat(" ", ss);
                else
                {
                    info.Append(cc);
                }
            }
            return info.ToString();
        }
    }
}
