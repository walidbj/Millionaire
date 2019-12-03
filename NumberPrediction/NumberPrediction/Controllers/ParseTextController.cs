using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CoreProject.Model;
using Microsoft.AspNetCore.Mvc;
using Nancy.Json;
using NumberPrediction.Models;

namespace NumberPrediction.Controllers
{
    public class ParseTextController : Controller
    {
        // GET: ParseText
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Send(TextModel textMessage)
        {
            var text = textMessage.Message;
            List<Models.ResultatJsonFormat> res = new List<Models.ResultatJsonFormat>();

            foreach (var line in text.Split(new string[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] lineParsed = line.Split(',');
                var data = new Models.ResultatJsonFormat()
                {
                    Date = DateTime.ParseExact(lineParsed[0], "dd/MM/yyyy", null),
                    Column1 = int.Parse(lineParsed[1]),
                    Column2 = int.Parse(lineParsed[2]),
                    Column3 = int.Parse(lineParsed[3]),
                    Column4 = int.Parse(lineParsed[4]),
                    Column5 = int.Parse(lineParsed[5]),
                    Column6 = int.Parse(lineParsed[6]),
                    Bonus = int.Parse(lineParsed[7])
                };
                res.Add(data);
            }

            var json = new JavaScriptSerializer().Serialize(res);
            return Content(json);
        }
    }
}