using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NumberPrediction.Models
{

    public class TagModel
    {
        [Required]
        [DataType(DataType.MultilineText)]
        public string TableTag { get; set; }
    }
}