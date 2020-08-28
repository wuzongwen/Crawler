using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Crawler.Model;
using HtmlAgilityPack;
using Newtonsoft.Json;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Crawler
{
    internal class Program
    {
        private const string Url = "https://www.zhihu.com/people/gu-shi-dang-an-ju-71/answers";

        private static async Task Main(string[] args)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            //执行方法
            await AngleSharp();

            stopwatch.Stop();
            Console.WriteLine($"耗时:{ stopwatch.ElapsedMilliseconds}");
            Console.ReadKey();
        }

        private static async Task AngleSharp()
        {
            //载入Html文档内容
            var htmlString = string.Empty;
            Console.WriteLine("网络准备中...");
            //Task.Run(() =>
            //  {
            //      for (int i = 1; i < 10; i++)
            //      {
            //          Console.SetCursorPosition(0, Console.CursorTop - 1);
            //          Console.WriteLine("网络准备中".ToString().PadRight(i,'.'));
            //          Thread.Sleep(100);
            //      }
            //  });
            
            //载入Html文档内容
            htmlString = await PuppeteerSharp(Url);

            Console.WriteLine("开始解析数据");
            /*
             * 解析HTML文档
             */
            var context = BrowsingContext.New(Configuration.Default);
            var parser = context.GetService<IHtmlParser>();
            var document = parser.ParseDocument(htmlString);

            //选择对应元素列表
            var carboxList = document.QuerySelectorAll("div.List-item");
            Console.WriteLine(string.Format("共{0}个故事", carboxList.Length));
            int i = 1;
            foreach (var carbox in carboxList)
            {
                try
                {
                    Console.WriteLine(string.Format("开始爬取第{0}个故事的内容", i));
                    //解析内容转换为实体模型
                    var model = CreateModelWithAngleSharp(carbox);
                    HtmlWeb web = new HtmlWeb();

                    //载入子页面文档内容，效率高
                    var htmlDoc = web.Load(model.Url.Replace("//", "https://"));

                    //载入子页面文档内容,内容完整,上面的方法不行可以用这个
                    //var c_htmlString = await PuppeteerSharp(model.Url.Replace("//", "https://"));

                    /*
                     * 解析HTML文档
                     */
                    var c_context = BrowsingContext.New(Configuration.Default);
                    var c_parser = c_context.GetService<IHtmlParser>();
                    var c_document = parser.ParseDocument(htmlDoc.Text);
                    //选择对应元素列表
                    var c_carboxList = c_document.QuerySelectorAll("div.ContentItem");
                    var c_model = CreateModelWithAngleSharp(c_carboxList.First());
                    //输出内容
                    Console.WriteLine(string.Format("标题:{0}", model.Title));
                    Console.WriteLine(string.Format("第{0}个故事爬取结束", i));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format("第{0}个故事爬取出错,原因:{1}", i,ex.Message));
                    Console.WriteLine(string.Format("第{0}个故事爬取结束", i));
                    throw;
                }
                i++;
                Console.WriteLine();
            }
        }

        private static async Task<string> PuppeteerSharp(string url)
        {
            //Enabled headless option
            var launchOptions = new LaunchOptions { Headless = true };
            //Starting headless browser
            var browser = await Puppeteer.LaunchAsync(launchOptions);

            //Get all(default) pages 
            var pages = await browser.PagesAsync();
            //Get first page or new tab page
            var firstPage = pages.Length > 0 ? pages[0] : await browser.NewPageAsync();
            //Request URL to get the page
            await firstPage.GoToAsync(url);

            //Get and return the HTML content of the page
            var htmlString = await firstPage.GetContentAsync();

            #region Dispose resources
            //Close tab page
            await firstPage.CloseAsync();

            //Close headless browser, all pages will be closed here.
            await browser.CloseAsync();
            #endregion

            return htmlString;
        }

        private static ZhiHu CreateModelWithAngleSharp(IParentNode node)
        {
            var model = new ZhiHu
            {
                Title = node.QuerySelector("a").TextContent,
                Content = node.QuerySelector("div.RichContent-inner span.RichText").TextContent,
                Url = node.QuerySelector("a").GetAttribute("href")
            };

            return model;
        }
    }
}
