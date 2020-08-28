using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Crawler2.Model;
using HtmlAgilityPack;
using System;
using System.Linq;

namespace Crawler2
{
    class Program
    {
        static void Main(string[] args)
        {
            var MainUrl= "https://www.zhihu.com/people/gu-shi-dang-an-ju-71/answers";
            HtmlWeb web = new HtmlWeb();
            var htmlDoc = web.Load(MainUrl);
            /*
              * 解析HTML文档
              */
            var context = BrowsingContext.New(Configuration.Default);
            var parser = context.GetService<IHtmlParser>();
            var document = parser.ParseDocument(htmlDoc.Text);

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

                    htmlDoc = web.Load(model.Url.Replace("//", "https://"));

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
                    Console.WriteLine(string.Format("第{0}个故事爬取出错,原因:{1}", i, ex.Message));
                    Console.WriteLine(string.Format("第{0}个故事爬取结束", i));
                    throw;
                }
                i++;
                Console.WriteLine();
            }
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
