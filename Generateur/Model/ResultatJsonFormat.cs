using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.ML.Data;

namespace Generateur.Model
{
    public class ResultatJsonFormat
    {
        [LoadColumn(0)]
        public DateTime Date { get; set; }
        [LoadColumn(1)]
        public float Column1 { get; set; }
        [LoadColumn(2)]
        public float Column2 { get; set; }
        [LoadColumn(3)]
        public float Column3 { get; set; }
        [LoadColumn(4)]
        public float Column4 { get; set; }
        [LoadColumn(5)]
        public float Column5 { get; set; }
        [LoadColumn(6)]
        public float Column6 { get; set; }
        [LoadColumn(7)]
        public int Bonus { get; set; }
        [LoadColumn(7)]
        public string DateString
        {
            get; set;
            //get { return Date.ToShortDateString(); 
        }

        [LoadColumn(8)] public string Day => Date.DayOfWeek.ToString();


    }

    /*public class ResultatJsonFormatTransformed : ResultatJsonFormat
    {
        //[LoadColumn(0)]
        public float ConvertedDate { get; set; }
       // [LoadColumn(1)]
        public float ConvertedColumn1 { get; set; }
     //   [LoadColumn(2)]
        public float ConvertedColumn2 { get; set; }
     //   [LoadColumn(3)]
        public float ConvertedColumn3 { get; set; }
     //   [LoadColumn(4)]
        public float ConvertedColumn4 { get; set; }
      //  [LoadColumn(5)]
        public float ConvertedColumn5 { get; set; }
      //  [LoadColumn(6)]
        public float ConvertedColumn6 { get; set; }
     //   [LoadColumn(7)]
     //   public float ConvertedBonus { get; set; }
        /*public string DateString
        {
            get { return Date.ToShortDateString(); }
        }
    }*/
    public class PredictedCombinaison
    {
        public int Column1 {get; set;}
        public int Column2 { get; set; }
        public int Column3 { get; set; }
        public int Column4 { get; set; }
        public int Column5 { get; set; }
        public int Column6 { get; set; }
    }

    public class PredictionColumn1 { 
    
        [ColumnName("Score")]
        public float Column1 { get; set; }
    }

    public class PredictionColumn2 { 
    
        [ColumnName("Score")]
        public float Column2 { get; set; }
    }
    public class PredictionColumn3 { 
    
        [ColumnName("Score")]
        public float Column3 { get; set; }
    }

    public class PredictionColumn4 { 
    
        [ColumnName("Score")]
        public float Column4 { get; set; }
    }

    public class PredictionColumn5 { 
    
        [ColumnName("Score")]
        public float Column5 { get; set; }
    }

    public class PredictionColumn6 { 
    
        [ColumnName("Score")]
        public float Column6 { get; set; }
    }


}