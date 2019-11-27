using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using HtmlAgilityPack;
using Table2JsonMvc.Models;

namespace Table2JsonMvc.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        public ActionResult Index(string contentText)
        {
            return Content(contentText);
        }

        [HttpPost]
        public ActionResult Send(TagModel tagModel)
        {
            var res = new List<ResultatJsonFormat>();
            var doc = new HtmlDocument();
            doc.LoadHtml(tagModel.TableTag);
            var nodes = doc.DocumentNode.Descendants("tr").ToList();
            for (int i = 1; i < nodes.Count(); i++)
            {
                res.Add(TrToJson(nodes[i]));
            }

            var json = new JavaScriptSerializer().Serialize(res);
            return Content(json);
        }

        private ResultatJsonFormat TrToJson(HtmlNode node)
        {
            var childs = node.ChildNodes;
            return new ResultatJsonFormat
            {
                Date = DateTime.Parse(childs[0].InnerHtml.Replace("nd", "")
                    .Replace("st", "")
                    .Replace("th", "")
                    .Replace("rd","")
                    .Replace("Augu", "August")),
                Column1 = Convert.ToInt32(childs[1].InnerHtml),
                Column2 = Convert.ToInt32(childs[2].InnerHtml),
                Column3 = Convert.ToInt32(childs[3].InnerHtml),
                Column4 = Convert.ToInt32(childs[4].InnerHtml),
                Column5 = Convert.ToInt32(childs[5].InnerHtml),
                Column6 = Convert.ToInt32(childs[6].InnerHtml),
                Bonus = Convert.ToInt32(childs[7].InnerHtml)
            };

        }

    }
}