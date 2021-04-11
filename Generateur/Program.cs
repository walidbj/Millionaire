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
        private static PredictionEngine<ResultatJsonFormat, PredictionColumn1> _predEngine;
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
            _datePrediction = DateTime.ParseExact(Console.ReadLine(), "dd/MM/yyyy", null);

            //_includeDay = args.Contains("-d");
            //_includeMonth = args.Contains("-m");
            //_includeWeek = args.Contains("-w");


            _mlContext = new MLContext();
            Console.WriteLine("Loading Data set");
            var data = JsonToList();


            _trainingDataView = _mlContext.Data.LoadFromEnumerable<ResultatJsonFormat>(data);

            DataOperationsCatalog.TrainTestData dataSplit = _mlContext.Data.TrainTestSplit(_trainingDataView, 0.99);
            _trainData = dataSplit.TrainSet;
            _testData = dataSplit.TestSet;

            Console.WriteLine("Prediction for all");
            MainTraitement(true, true, true);
            Console.WriteLine("Prediction for day-week");
            MainTraitement(true, true, false);
            Console.WriteLine("Prediction for week-month");
            MainTraitement(false, true, true);

            // DisplayNumberOccurences(data, _datePrediction);
        }

        private static void MainTraitement(bool includeDay, bool includeWeek, bool includeMonth)
        {
            _includeDay = includeDay;
            _includeMonth = includeMonth;
            _includeWeek = includeWeek;
            var coreFacde = new CoreFacade(includeDay, includeMonth, includeWeek, _mlContext, _trainData, _testData, _datePrediction);
            _predictedCombinaison = coreFacde.PredictCombinaison();

            Console.WriteLine(
                $"Predicted combinaison is {_predictedCombinaison.Column1}, {_predictedCombinaison.Column2}, {_predictedCombinaison.Column3}, {_predictedCombinaison.Column4}, {_predictedCombinaison.Column5}, {_predictedCombinaison.Column6}, Bonus {_predictedCombinaison.Bonus}");
        }

        private static void DisplayNumberOccurences(List<ResultatJsonFormat> data, DateTime dateSelected)
        {
            var datafiltered = data;
            ResultatJsonFormat newDateSelected = new ResultatJsonFormat
            {
                Date = dateSelected
            };
            if (_includeDay)
                datafiltered = datafiltered.Where(d => d.Day == newDateSelected.Day).ToList();
            if (_includeWeek)
                datafiltered = datafiltered.Where(d => d.Week == newDateSelected.Week).ToList();
            if (_includeMonth)
                datafiltered = datafiltered.Where(d => d.Month == newDateSelected.Month).ToList();

            List<float> res = new List<float>();
            foreach(var item in datafiltered)
            {
                res.AddRange(new float[] { item.Column1, item.Column2, item.Column3, item.Column4, item.Column5, item.Column6 });  
            }

            foreach (var line in res.GroupBy(info => info)
                        .Select(group => new {
                            Metric = group.Key,
                            Count = group.Count()
                        })
                        .OrderBy(x => x.Count))
            {
                Console.WriteLine("{0} {1}", line.Metric, line.Count);
            }
        }

        private static void PredictColumn1()
        {
            var trainingPipeline = BuildAndTrainModel(ColumnEnum.Column1);

            Evaluate(_mlContext, trainingPipeline);

            _predictedCombinaison.Column1  = (int)TestPrediction(trainingPipeline, ColumnEnum.Column1);
        }

        private static void PredictColumn2()
        {
            var trainingPipeline = BuildAndTrainModel(ColumnEnum.Column2);
            Evaluate(_mlContext, trainingPipeline);
            _predictedCombinaison.Column2 = (int) TestPrediction(trainingPipeline, ColumnEnum.Column2);
        }

        private static void PredictColumn3()
        {
            var trainingPipeline = BuildAndTrainModel(ColumnEnum.Column3);
            Evaluate(_mlContext, trainingPipeline);
            _predictedCombinaison.Column3 = (int) TestPrediction(trainingPipeline, ColumnEnum.Column3);
        }

        private static void PredictColumn4()
        {
            var trainingPipeline = BuildAndTrainModel(ColumnEnum.Column4);
            Evaluate(_mlContext, trainingPipeline);
            _predictedCombinaison.Column4 = (int) TestPrediction(trainingPipeline, ColumnEnum.Column4);
        }

        private static void PredictColumn5()
        {
            var trainingPipeline = BuildAndTrainModel(ColumnEnum.Column5);
            Evaluate(_mlContext, trainingPipeline);
            _predictedCombinaison.Column5 = (int) TestPrediction(trainingPipeline, ColumnEnum.Column5);
        }

        private static void PredictColumn6()
        {
            var trainingPipeline = BuildAndTrainModel(ColumnEnum.Column6);
            Evaluate(_mlContext, trainingPipeline);
            _predictedCombinaison.Column6 = (int) TestPrediction(trainingPipeline, ColumnEnum.Column6);
        }

        private static void PredictBonus()
        {
            var trainingPipeline = BuildAndTrainModel(ColumnEnum.Bonus);
            Evaluate(_mlContext, trainingPipeline);
            _predictedCombinaison.Bonus = (int) TestPrediction(trainingPipeline, ColumnEnum.Bonus);
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

        public static ITransformer BuildAndTrainModel(ColumnEnum column)
        {
            // STEP 3: Create the training algorithm/trainer
            // Use the multi-class SDCA algorithm to predict the label using features.
            //Set the trainer/algorithm and map label to value (original readable state)
            // <SnippetAddTrainer> 
           // var trainingPipeline = pipeline.Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy("Label", "Feature"))
                   /* .Append(_mlContext.Transforms.Conversion.MapKeyToValue("Column1Predicted"));*/

                   switch (column)
                   {
                       case ColumnEnum.Column1:   
                                return BuildAndTrainUsingParams(ColumnEnum.Column1);

                       case ColumnEnum.Column2:
                                return BuildAndTrainUsingParams(ColumnEnum.Column2);

                       case ColumnEnum.Column3:
                                return BuildAndTrainUsingParams(ColumnEnum.Column3);
                       
                       case ColumnEnum.Column4:
                                return BuildAndTrainUsingParams(ColumnEnum.Column4);
                                                
                       case ColumnEnum.Column5:
                                return BuildAndTrainUsingParams(ColumnEnum.Column5);

                       case ColumnEnum.Column6:
                                return BuildAndTrainUsingParams(ColumnEnum.Column6);

                        case ColumnEnum.Bonus:
                                return BuildAndTrainUsingParams(ColumnEnum.Bonus);            

                   }
                   
            // </SnippetAddTrainer> 
            // STEP 4: Train the model fitting to the DataSet
            Console.WriteLine($"=============== Training the model  ===============");

            // <SnippetTrainModel> 

            // </SnippetTrainModel> 
            Console.WriteLine($"=============== Finished Training the model Ending time: {DateTime.Now.ToString()} ===============");

            // (OPTIONAL) Try/test a single prediction with the "just-trained model" (Before saving the model)
            Console.WriteLine($"=============== Single Prediction just-trained-model ===============");

            return _trainedModel;

        }

        private static  ITransformer BuildAndTrainUsingParams(ColumnEnum column)
        {
            List<string> features = new List<string>();
            TextFeaturizingEstimator textTransformer = null;
            EstimatorChain<ColumnConcatenatingTransformer> estimatorColumn = null; 
            EstimatorChain<ITransformer> estimatorTransformer = null;
            if(_includeDay)
            {
                textTransformer = _mlContext.Transforms.Text.FeaturizeText("DayString", "Day");
                features.Add("DayString");
            }
            if(_includeMonth)
            {
                if(textTransformer != null)
                {
                    estimatorTransformer = textTransformer.Append(_mlContext.Transforms.Text.FeaturizeText("MonthString", "Month"));
                }else
                {
                    textTransformer = _mlContext.Transforms.Text.FeaturizeText("MonthString", "Month");
                }
                features.Add("MonthString");
            }
            if(_includeWeek)
            {
                features.Add("Week");
            }

            if(textTransformer == null)
            {
                var res = _mlContext.Transforms.Concatenate("Features", features.ToArray())
                            .Append(_mlContext.Transforms.CopyColumns("Label", System.Enum.GetName(typeof(ColumnEnum), column)))
                            .Append(_mlContext.Regression.Trainers.FastTreeTweedie());

                      return res.Fit(_trainData);

            }
            if(estimatorTransformer != null)
            {
                var res2 = estimatorTransformer.Append(_mlContext.Transforms.Concatenate("Features", features.ToArray()))
                                 .Append(_mlContext.Transforms.CopyColumns("Label", System.Enum.GetName(typeof(ColumnEnum), column)))
                                 .Append(_mlContext.Regression.Trainers.FastTreeTweedie());
                return res2.Fit(_trainData);
            }
            var res3 = textTransformer.Append(_mlContext.Transforms.Concatenate("Features", features.ToArray()))
                                 .Append(_mlContext.Transforms.CopyColumns("Label", System.Enum.GetName(typeof(ColumnEnum), column)))
                                 .Append(_mlContext.Regression.Trainers.FastTreeTweedie());
            return res3.Fit(_trainData);
               
        }

   
        private static void Evaluate(MLContext mlContext, ITransformer model)
        {
            var prediction = model.Transform(_testData);

            var metrics = _mlContext.Regression.Evaluate(prediction, "Label", "Score");
        }
     

        private static float TestPrediction(ITransformer model, ColumnEnum column)
        {
            switch (column)
            {
                case ColumnEnum.Column1:
                    var predictionFunction = _mlContext.Model.CreatePredictionEngine<ResultatJsonFormat, PredictionColumn1>(model);
                    var test = new ResultatJsonFormat()
                    {
                        Date = _datePrediction
                    };
                    return predictionFunction.Predict(test).Column1;

                case ColumnEnum.Column2:
                    var predictionFunction2 = _mlContext.Model.CreatePredictionEngine<ResultatJsonFormat, PredictionColumn2>(model);
                    var test2 = new ResultatJsonFormat()
                    {
                        Date = _datePrediction
                    };
                    return predictionFunction2.Predict(test2).Column2;

                case ColumnEnum.Column3:
                    var predictionFunction3 = _mlContext.Model.CreatePredictionEngine<ResultatJsonFormat, PredictionColumn3>(model);
                    var test3 = new ResultatJsonFormat()
                    {
                        Date = _datePrediction
                    };
                    return predictionFunction3.Predict(test3).Column3;

                case ColumnEnum.Column4:
                    var predictionFunction4 = _mlContext.Model.CreatePredictionEngine<ResultatJsonFormat, PredictionColumn4>(model);
                    var test4 = new ResultatJsonFormat()
                    {
                        Date = _datePrediction
                    };
                    return predictionFunction4.Predict(test4).Column4;

                case ColumnEnum.Column5:
                    var predictionFunction5 = _mlContext.Model.CreatePredictionEngine<ResultatJsonFormat, PredictionColumn5>(model);
                    var test5 = new ResultatJsonFormat()
                    {
                        Date = _datePrediction
                    };
                    return predictionFunction5.Predict(test5).Column5;

                case ColumnEnum.Column6:
                    var predictionFunction6 = _mlContext.Model.CreatePredictionEngine<ResultatJsonFormat, PredictionColumn6>(model);
                    var test6 = new ResultatJsonFormat()
                    {
                        Date = _datePrediction
                    };
                    return predictionFunction6.Predict(test6).Column6;

               case ColumnEnum.Bonus:
                    var predictionFunctionBonus = _mlContext.Model.CreatePredictionEngine<ResultatJsonFormat, PredictionBonus>(model);
                    var testBonus = new ResultatJsonFormat()
                    {
                        Date = _datePrediction
                    };
                    return predictionFunctionBonus.Predict(testBonus).Bonus;
            }

            return 0;
        }
    }
}
