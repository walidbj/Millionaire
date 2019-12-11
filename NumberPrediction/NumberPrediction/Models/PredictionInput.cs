using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NumberPrediction.Models
{
    public class PredictionInput
    {
        public DateTime InputDate { get; set; }
        public bool IncludeDay { get; set; }
        public bool IncludeMonth { get; set; }
        public bool IncludeWeek { get; set; }
    }
}
