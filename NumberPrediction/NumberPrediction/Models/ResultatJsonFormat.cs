using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NumberPrediction.Models
{
    public class ResultatJsonFormat
    {
        public DateTime Date { get; set; }
        public int Column1 { get; set; }
        public int Column2 { get; set; }
        public int Column3 { get; set; }
        public int Column4 { get; set; }
        public int Column5 { get; set; }
        public int Column6 { get; set; }
        public int Bonus { get; set; }
    }

    public class Prediction
    {
        public int Column1 { get; set; }
        public int Column2 { get; set; }
        public int Column3 { get; set; }
        public int Column4 { get; set; }
        public int Column5 { get; set; }
        public int Column6 { get; set; }
    }
}