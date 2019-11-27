using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Generateur.Enum;
using Generateur.Model;
using Microsoft.ML;
using Microsoft.ML.Data;
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
        private static bool _includeDay;

        static void Main(string[] args)
        {
            Console.WriteLine("Please enter a date for Prediction dd/MM/yyyy");
            _datePrediction = DateTime.ParseExact(Console.ReadLine(),"dd/MM/yyyy", null);
            Console.WriteLine("Include day? Y/N");
            _includeDay = Console.ReadLine() == "Y";
            _mlContext = new MLContext();
            Console.WriteLine("Loading Data set");
            var data = JsonToList();
            data.ForEach(d =>
            {
                d.DateString = d.Date.ToShortDateString();
            });

            _trainingDataView = _mlContext.Data.LoadFromEnumerable<ResultatJsonFormat>(data);
            _trainingDataView = _mlContext.Data.FilterRowsByColumn(_trainingDataView,"Column1", 1, 49);
            _trainingDataView = _mlContext.Data.FilterRowsByColumn(_trainingDataView,"Column2", 1, 49);
            _trainingDataView = _mlContext.Data.FilterRowsByColumn(_trainingDataView,"Column3", 1, 49);
            _trainingDataView = _mlContext.Data.FilterRowsByColumn(_trainingDataView,"Column4", 1, 49);
            _trainingDataView = _mlContext.Data.FilterRowsByColumn(_trainingDataView,"Column5", 1, 49);
            _trainingDataView = _mlContext.Data.FilterRowsByColumn(_trainingDataView,"Column6", 1, 49);
            DataOperationsCatalog.TrainTestData dataSplit = _mlContext.Data.TrainTestSplit(_trainingDataView, 0.99);
            _trainData = dataSplit.TrainSet;
            _testData = dataSplit.TestSet;
            Console.WriteLine("Finished loading data set");

            PredictColumn1();
            PredictColumn2();
            PredictColumn3();
            PredictColumn4();
            PredictColumn5();
            PredictColumn6();

            Console.WriteLine(
                $"Predicted combinaison is {_predictedCombinaison.Column1}, {_predictedCombinaison.Column2}, {_predictedCombinaison.Column3}, {_predictedCombinaison.Column4}, {_predictedCombinaison.Column5}, {_predictedCombinaison.Column6}");
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

        //private static void BuildTrainEvaluateAndSaveModel(MLContext mlContext, List<ResultatJsonFormat> data )
        //{
        //    IDataView baseTrainigData = mlContext.Data.LoadFromEnumerable(data);


        //    var cnt = baseTrainigData.GetColumn<double>(nameof(ResultatJsonFormat.Column1)).Count();
        //    IDataView trainigDataView = mlContext.Data.FilterRowsByColumn(baseTrainigData,
        //        nameof(ResultatJsonFormat.Column1), (double) 1, (double) 49);
        //    trainigDataView = mlContext.Data.FilterRowsByColumn(trainigDataView, nameof(ResultatJsonFormat.Column2),
        //        (double) 1, (double) 49);
        //    trainigDataView = mlContext.Data.FilterRowsByColumn(trainigDataView, nameof(ResultatJsonFormat.Column3),
        //        (double) 1, (double) 49);
        //    trainigDataView = mlContext.Data.FilterRowsByColumn(trainigDataView, nameof(ResultatJsonFormat.Column4),
        //        (double) 1, (double) 49);
        //    trainigDataView = mlContext.Data.FilterRowsByColumn(trainigDataView, nameof(ResultatJsonFormat.Column5),
        //        (double) 1, (double) 49);
        //    trainigDataView = mlContext.Data.FilterRowsByColumn(trainigDataView, nameof(ResultatJsonFormat.Column6),
        //        (double) 1, (double) 49);
        //    trainigDataView = mlContext.Data.FilterRowsByColumn(trainigDataView, nameof(ResultatJsonFormat.Bonus),
        //        (double) 1, (double) 49);
     

        //  var pipeline = mlContext.Transforms.Conversion.MapValueToKey("Column1Predicted", "Label")
        //      .Append(mlContext.Transforms.Text.FeaturizeText("DateString"))
        //      .Append(mlContext.Transforms.Concatenate("Features", "DateString"))
        //      .Append(mlContext.Regression.Trainers.Sdca(labelColumnName: "Column1", maximumNumberOfIterations: 100));
        //  //ITransformer dataPrepTransformer = dataPrepEstimator.Fit(baseTrainigData);
        //  //IDataView transformedTrainingData = dataPrepTransformer.Transform(baseTrainigData); 
        //  var model = pipeline.Fit(baseTrainigData);

        //  var newValue = new ResultatJsonFormat() { Date = DateTime.Now.Date };
        //  var column1 = mlContext.Model.CreatePredictionEngine<ResultatJsonFormat, PredictionColumn1>(model).Predict(newValue);

        //    Console.WriteLine($"Predicted number 1 is {newValue.Column1}");


        //}

       /* public static IEstimator<ITransformer> TransformData()
        {
            Console.WriteLine($"=============== Processing Data ===============");
            var pipeline = _mlContext.Transforms.Conversion.MapValueToKey(inputColumnName:"Column1", outputColumnName:"Label")
                .Append(_mlContext.Transforms.Text.FeaturizeText(inputColumnName: "DateString",
                    outputColumnName: "DateStringFeaturized"))
                .Append(_mlContext.Transforms.CopyColumns(inputColumnName:"Column1", outputColumnName: "Column1Featurized"))
                .Append(_mlContext.Transforms.Concatenate("Feature", "DateStringFeaturized","Column1Featurized"))
                .AppendCacheCheckpoint(_mlContext);

          
            return pipeline;
        }*/

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

                           if (_includeDay)
                           {
                               var trainingPipelineDay = _mlContext.Transforms.Text.FeaturizeText(outputColumnName: "FeatureDate", "DateString")
                                   .Append(_mlContext.Transforms.Text.FeaturizeText("DayString", "Day"))
                                   .Append(_mlContext.Transforms.Concatenate("Features", "FeatureDate", "DayString"))
                                   .Append(_mlContext.Transforms.CopyColumns(outputColumnName:"Label", "Column1"))
                                   .Append(_mlContext.Regression.Trainers.FastTree());
                               _trainedModel = trainingPipelineDay.Fit(_trainData);
                               return _trainedModel;
                           } 
                           var trainingPipeline = _mlContext.Transforms.Text.FeaturizeText(outputColumnName: "Features", "DateString")
                               .Append(_mlContext.Transforms.CopyColumns(outputColumnName:"Label", "Column1"))
                               .Append(_mlContext.Regression.Trainers.FastTree());
                           _trainedModel = trainingPipeline.Fit(_trainData);
                           return _trainedModel;

                       case ColumnEnum.Column2:
                           if (_includeDay)
                           {
                               var trainingPipeline2Day = _mlContext.Transforms.Text.FeaturizeText("FeatureDate", "DateString")
                                   .Append(_mlContext.Transforms.Text.FeaturizeText("DayString", "Day"))
                                   .Append(_mlContext.Transforms.Concatenate("Features", "FeatureDate", "DayString", "Column1"))
                                   .Append(_mlContext.Transforms.CopyColumns("Label", "Column2"))
                                   .Append(_mlContext.Regression.Trainers.FastTree());
                               _trainedModel = trainingPipeline2Day.Fit(_trainData);
                               return _trainedModel;
                           }
                           var trainingPipeline2 = _mlContext.Transforms.Text.FeaturizeText("FeatureDate", "DateString")
                               .Append(_mlContext.Transforms.Concatenate("Features", "FeatureDate", "Column1"))
                               .Append(_mlContext.Transforms.CopyColumns("Label", "Column2"))
                               .Append(_mlContext.Regression.Trainers.FastTree());
                           _trainedModel = trainingPipeline2.Fit(_trainData);
                           return _trainedModel;

                       case ColumnEnum.Column3:
                           if (_includeDay)
                           {
                               var trainingPipeline3Day = _mlContext.Transforms.Text.FeaturizeText("FeatureDate", "DateString")
                                   .Append(_mlContext.Transforms.Text.FeaturizeText("DayString", "Day"))
                                   .Append(_mlContext.Transforms.Concatenate("Features", "FeatureDate", "DayString", "Column1", "Column2"))
                                   .Append(_mlContext.Transforms.CopyColumns("Label", "Column3"))
                                   .Append(_mlContext.Regression.Trainers.FastTree());
                               _trainedModel = trainingPipeline3Day.Fit(_trainData);
                               return _trainedModel;
                           }
                           var trainingPipeline3 = _mlContext.Transforms.Text.FeaturizeText("FeatureDate", "DateString")
                               .Append(_mlContext.Transforms.Concatenate("Features", "FeatureDate", "Column1", "Column2"))
                               .Append(_mlContext.Transforms.CopyColumns("Label", "Column3"))
                               .Append(_mlContext.Regression.Trainers.FastTree());
                           _trainedModel = trainingPipeline3.Fit(_trainData);
                           return _trainedModel;

                       case ColumnEnum.Column4:
                           if (_includeDay)
                           {
                               var trainingPipeline4Day = _mlContext.Transforms.Text.FeaturizeText("FeatureDate", "DateString")
                                   .Append(_mlContext.Transforms.Text.FeaturizeText("DayString", "Day"))
                                   .Append(_mlContext.Transforms.Concatenate("Features", "FeatureDate","DayString", "Column1", "Column2", "Column3"))
                                   .Append(_mlContext.Transforms.CopyColumns("Label", "Column4"))
                                   .Append(_mlContext.Regression.Trainers.FastTree());
                               _trainedModel = trainingPipeline4Day.Fit(_trainData);
                               return _trainedModel;
                           }
                           var trainingPipeline4 = _mlContext.Transforms.Text.FeaturizeText("FeatureDate", "DateString")
                               .Append(_mlContext.Transforms.Concatenate("Features", "FeatureDate", "Column1", "Column2", "Column3"))
                               .Append(_mlContext.Transforms.CopyColumns("Label", "Column4"))
                               .Append(_mlContext.Regression.Trainers.FastTree());
                           _trainedModel = trainingPipeline4.Fit(_trainData);
                           return _trainedModel;

                       case ColumnEnum.Column5:
                           if (_includeDay)
                           {
                               var trainingPipeline5Day = _mlContext.Transforms.Text.FeaturizeText("FeatureDate", "DateString")
                                   .Append(_mlContext.Transforms.Text.FeaturizeText("DayString", "Day"))
                                   .Append(_mlContext.Transforms.Concatenate("Features", "FeatureDate","DayString", "Column1", "Column2", "Column3", "Column4"))
                                   .Append(_mlContext.Transforms.CopyColumns("Label", "Column5"))
                                   .Append(_mlContext.Regression.Trainers.FastTree());
                               _trainedModel = trainingPipeline5Day.Fit(_trainData);
                               return _trainedModel;
                           }
                           var trainingPipeline5 = _mlContext.Transforms.Text.FeaturizeText("FeatureDate", "DateString")
                               .Append(_mlContext.Transforms.Concatenate("Features", "FeatureDate", "Column1", "Column2", "Column3", "Column4"))
                               .Append(_mlContext.Transforms.CopyColumns("Label", "Column5"))
                               .Append(_mlContext.Regression.Trainers.FastTree());
                           _trainedModel = trainingPipeline5.Fit(_trainData);
                           return _trainedModel;

                       case ColumnEnum.Column6:
                           if (_includeDay)
                           {
                               var trainingPipeline6Day = _mlContext.Transforms.Text.FeaturizeText("FeatureDate", "DateString")
                                   .Append(_mlContext.Transforms.Text.FeaturizeText("DayString", "Day"))
                                   .Append(_mlContext.Transforms.Concatenate("Features", "FeatureDate","DayString",  "Column1", "Column2", "Column3", "Column4", "Column5"))
                                   .Append(_mlContext.Transforms.CopyColumns("Label", "Column6"))
                                   .Append(_mlContext.Regression.Trainers.FastTree());
                               _trainedModel = trainingPipeline6Day.Fit(_trainData);
                               return _trainedModel;
                           }
                           var trainingPipeline6 = _mlContext.Transforms.Text.FeaturizeText("FeatureDate", "DateString")
                               .Append(_mlContext.Transforms.Concatenate("Features", "FeatureDate", "Column1", "Column2", "Column3", "Column4", "Column5"))
                               .Append(_mlContext.Transforms.CopyColumns("Label", "Column6"))
                               .Append(_mlContext.Regression.Trainers.FastTree());
                           _trainedModel = trainingPipeline6.Fit(_trainData);
                           return _trainedModel;

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
        private static void Evaluate(MLContext mlContext, ITransformer model)
        {
            var prediction = model.Transform(_testData);

            var metrics = _mlContext.Regression.Evaluate(prediction, "Label", "Score");
        }
     /*   public static void Evaluate(DataViewSchema trainingDataViewSchema, List<ResultatJsonFormat> data)
        {
            // STEP 5:  Evaluate the model in order to get the model's accuracy metrics
            Console.WriteLine($"=============== Evaluating to get model's accuracy metrics - Starting time: {DateTime.Now.ToString()} ===============");

            //Load the test dataset into the IDataView
            // <SnippetLoadTestDataset>
            var testDataView = _mlContext.Data.LoadFromEnumerable(data);
            // </SnippetLoadTestDataset>

            //Evaluate the model on a test dataset and calculate metrics of the model on the test data.
            // <SnippetEvaluate>
            var testMetrics = _mlContext.MulticlassClassification.Evaluate(_trainedModel.Transform(testDataView));
            // </SnippetEvaluate>

            Console.WriteLine($"=============== Evaluating to get model's accuracy metrics - Ending time: {DateTime.Now.ToString()} ===============");
            // <SnippetDisplayMetrics>
            Console.WriteLine($"*************************************************************************************************************");
            Console.WriteLine($"*       Metrics for Multi-class Classification model - Test Data     ");
            Console.WriteLine($"*------------------------------------------------------------------------------------------------------------");
            Console.WriteLine($"*       MicroAccuracy:    {testMetrics.MicroAccuracy:0.###}");
            Console.WriteLine($"*       MacroAccuracy:    {testMetrics.MacroAccuracy:0.###}");
            Console.WriteLine($"*       LogLoss:          {testMetrics.LogLoss:#.###}");
            Console.WriteLine($"*       LogLossReduction: {testMetrics.LogLossReduction:#.###}");
            Console.WriteLine($"*************************************************************************************************************");
            // </SnippetDisplayMetrics>

            // Save the new model to .ZIP file
            // <SnippetCallSaveModel>
            SaveModelAsFile(_mlContext, trainingDataViewSchema, _trainedModel);
            // </SnippetCallSaveModel>

        }*/
                private static void SaveModelAsFile(MLContext mlContext,DataViewSchema trainingDataViewSchema, ITransformer model)
                {
                    // <SnippetSaveModel> 
                    mlContext.Model.Save(model, trainingDataViewSchema, _modelPath);
                    // </SnippetSaveModel>

                    Console.WriteLine("The model is saved to {0}", _modelPath);
                }
                public static void PredictIssue()
                {
                    // <SnippetLoadModel>
                    ITransformer loadedModel = _mlContext.Model.Load(_modelPath, out var modelInputSchema);            
                    // </SnippetLoadModel>

                    // <SnippetAddTestIssue> 
                    ResultatJsonFormat singleIssue = new ResultatJsonFormat() { DateString = (new DateTime(2019,11,02)).ToShortDateString() };

                    _predEngine = _mlContext.Model.CreatePredictionEngine<ResultatJsonFormat, PredictionColumn1>(loadedModel);

                    var prediction = _predEngine.Predict(singleIssue);

                }

        private static float TestPrediction(ITransformer model, ColumnEnum column)
        {
            switch (column)
            {
                case ColumnEnum.Column1:
                    var predictionFunction = _mlContext.Model.CreatePredictionEngine<ResultatJsonFormat, PredictionColumn1>(model);
                    var test = new ResultatJsonFormat()
                    {
                        Date = _datePrediction,
                        DateString = _datePrediction.ToShortDateString()
                    };
                    return predictionFunction.Predict(test).Column1;

                case ColumnEnum.Column2:
                    var predictionFunction2 = _mlContext.Model.CreatePredictionEngine<ResultatJsonFormat, PredictionColumn2>(model);
                    var test2 = new ResultatJsonFormat()
                    {
                        Date = _datePrediction,
                        DateString = _datePrediction.ToShortDateString(),
                        Column1 =  _predictedCombinaison.Column1
                    };
                    return predictionFunction2.Predict(test2).Column2;

                case ColumnEnum.Column3:
                    var predictionFunction3 = _mlContext.Model.CreatePredictionEngine<ResultatJsonFormat, PredictionColumn3>(model);
                    var test3 = new ResultatJsonFormat()
                    {
                        Date = _datePrediction,
                        DateString = _datePrediction.ToShortDateString(),
                        Column1 =  _predictedCombinaison.Column1,
                        Column2 = _predictedCombinaison.Column2
                    };
                    return predictionFunction3.Predict(test3).Column3;

                case ColumnEnum.Column4:
                    var predictionFunction4 = _mlContext.Model.CreatePredictionEngine<ResultatJsonFormat, PredictionColumn4>(model);
                    var test4 = new ResultatJsonFormat()
                    {
                        Date = _datePrediction,
                        DateString = _datePrediction.ToShortDateString(),
                        Column1 =  _predictedCombinaison.Column1,
                        Column2 = _predictedCombinaison.Column2,
                        Column3 = _predictedCombinaison.Column3
                    };
                    return predictionFunction4.Predict(test4).Column4;

                case ColumnEnum.Column5:
                    var predictionFunction5 = _mlContext.Model.CreatePredictionEngine<ResultatJsonFormat, PredictionColumn5>(model);
                    var test5 = new ResultatJsonFormat()
                    {
                        Date = _datePrediction,
                        DateString = _datePrediction.ToShortDateString(),
                        Column1 =  _predictedCombinaison.Column1,
                        Column2 = _predictedCombinaison.Column2,
                        Column3 = _predictedCombinaison.Column3,
                        Column4 = _predictedCombinaison.Column4
                    };
                    return predictionFunction5.Predict(test5).Column5;

                case ColumnEnum.Column6:
                    var predictionFunction6 = _mlContext.Model.CreatePredictionEngine<ResultatJsonFormat, PredictionColumn6>(model);
                    var test6 = new ResultatJsonFormat()
                    {
                        Date = _datePrediction,
                        DateString = _datePrediction.ToShortDateString(),
                        Column1 =  _predictedCombinaison.Column1,
                        Column2 = _predictedCombinaison.Column2,
                        Column3 = _predictedCombinaison.Column3,
                        Column4 = _predictedCombinaison.Column4,
                        Column5 = _predictedCombinaison.Column5
                    };
                    return predictionFunction6.Predict(test6).Column6;
            }

            return 0;
        }
    }
}
