using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Table2JsonMvc.Models
{

    public class TagModel
    {
        [Required]
        [AllowHtml]
        [DataType(DataType.MultilineText)]
        public string TableTag { get; set; }
    }
}