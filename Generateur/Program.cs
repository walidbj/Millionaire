using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CoreProject;
using CoreProject.Enum;
using CoreProject.Model;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Text;
using Newtonsoft.Json;

namespace Generateur
{
    class Program
    {
        
        private static MLContext _mlContext;
        private static PredictionEngine<ResultatJsonFormat,PredictionColumn1> _predEngine;
        private static ITransformer _trainedModel;
        private static PredictedCombinaison _predictedCombinaison = new PredictedCombinaison();
        private static string _modelPath => @"C:\\model.zip";
        private static IDataView _trainingDataView;
        private static IDataView _trainData;
        private static IDataView _testData;

        private static DateTime _datePrediction;
        private static bool _includeMonth;
        private static bool _includeWeek = false;
        private static bool _includeDay = true;


        static void Main(string[] args)
        {
            Console.WriteLine("Please enter a date for Prediction dd/MM/yyyy");
            _datePrediction = DateTime.ParseExact(Console.ReadLine(),"dd/MM/yyyy", null);

            _includeDay = args.Contains("-d");
            _includeMonth = args.Contains("-m");
            _includeWeek = args.Contains("-w");


            _mlContext = new MLContext();
            Console.WriteLine("Loading Data set");
            var data = JsonToList();


            _trainingDataView = _mlContext.Data.LoadFromEnumerable<ResultatJsonFormat>(data);

            DataOperationsCatalog.TrainTestData dataSplit = _mlContext.Data.TrainTestSplit(_trainingDataView, 0.99);
            _trainData = dataSplit.TrainSet;
            _testData = dataSplit.TestSet;

            var coreFacde = new CoreFacade(_includeDay, _includeMonth, _includeWeek, _mlContext, _trainData, _testData, _datePrediction);
            _predictedCombinaison = coreFacde.PredictCombinaison();


            Console.WriteLine(
                $"Predicted combinaison is {_predictedCombinaison.Column1}, {_predictedCombinaison.Column2}, {_predictedCombinaison.Column3}, {_predictedCombinaison.Column4}, {_predictedCombinaison.Column5}, {_predictedCombinaison.Column6}, Bonus {_predictedCombinaison.Bonus}");
        }

       

        private static List<ResultatJsonFormat> JsonToList()
        {
            JsonSerializer serializer = new JsonSerializer();
            List<ResultatJsonFormat> o = new List<ResultatJsonFormat>();
            using (FileStream s = File.Open("..\\..\\..\\Data\\result.json", FileMode.Open))
            using (StreamReader sr = new StreamReader(s))
            using (JsonReader reader = new JsonTextReader(sr))
            {
                while (reader.Read())
                {
                    // deserialize only when there's "{" character in the stream
                    if (reader.TokenType == JsonToken.StartObject)
                    {
                        o.Add(serializer.Deserialize<ResultatJsonFormat>(reader));
                    }
                }
            }
            return o;
        }

     

      
    }
}
