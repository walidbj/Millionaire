using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CoreProject.Model;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nancy.Json;
using NumberPrediction.Models;

namespace NumberPrediction.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public ActionResult Index(string contentText)
        {
            return Content(contentText);
        }

        [HttpPost]
        public ActionResult Send(TagModel tagModel)
        {
            var res = new List<Models.ResultatJsonFormat>();
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

        private Models.ResultatJsonFormat TrToJson(HtmlNode node)
        {
            var childs = node.ChildNodes;
            return new Models.ResultatJsonFormat
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
