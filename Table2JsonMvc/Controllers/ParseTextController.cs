using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Table2JsonMvc.Models;

namespace Table2JsonMvc.Controllers
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
            List<ResultatJsonFormat> res = new List<ResultatJsonFormat>();

            foreach (var line in text.Split(new string[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] lineParsed = line.Split(',');
                var data = new ResultatJsonFormat()
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