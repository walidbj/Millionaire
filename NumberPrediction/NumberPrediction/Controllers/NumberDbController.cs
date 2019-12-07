using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NumberPrediction.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NumberPrediction.Controllers
{
    public class NumberDbController : Controller
    {
        private readonly IDocumentDBRepository<ResultatJsonFormat> Respository;
        public NumberDbController(IDocumentDBRepository<ResultatJsonFormat> Respository)
        {
            this.Respository = Respository;
        }

        [ActionName("Index")]
        public async Task<IActionResult> Index()
        {
            var items = await Respository.GetItemsAsync(d => d.Date);
            return View(items);
        }

        #pragma warning disable 1998
        [ActionName("Create")]
        public async Task<IActionResult> CreateAsync()
        {
            return View();
        }
#pragma warning restore 1998

        [HttpPost]
        [ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateAsync([Bind("Date,Column1,Column2,Column3,Column4,Column5,Column6,Bonus")] ResultatJsonFormat item)
        {
            if (ModelState.IsValid)
            {
                await Respository.CreateItemAsync(item);
                return RedirectToAction("Index");
            }

            return View(item);
        }

         [ActionName("Delete")]
        public async Task<ActionResult> DeleteAsync(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            ResultatJsonFormat item = await Respository.GetItemAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmedAsync([Bind("Id")] string id)
        {
            await Respository.DeleteItemAsync(id);
            return RedirectToAction("Index");
        }

        [ActionName("InitialLoad")]
        public async Task<IActionResult> InitialLoad()
        {
            //JsonSerializer serializer = new JsonSerializer();
            //List<ResultatJsonFormat> o = new List<ResultatJsonFormat>();
            //using (FileStream s = System.IO.File.Open("C:\\git_wbj\\Millionaire\\Millionaire\\CoreProject\\Data\\result.json", FileMode.Open))
            //using (StreamReader sr = new StreamReader(s))
            //using (JsonReader reader = new JsonTextReader(sr))
            //{
            //    while (reader.Read())
            //    {
            //        // deserialize only when there's "{" character in the stream
            //        if (reader.TokenType == JsonToken.StartObject)
            //        {
            //            //o.Add(serializer.Deserialize<ResultatJsonFormat>(reader));
            //            await Respository.CreateItemAsync(serializer.Deserialize<ResultatJsonFormat>(reader));
            //        }
            //    }
            //}
            return RedirectToAction("Index");
        }

    }
}
