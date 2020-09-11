using CoreProject.Enum;
using CoreProject.Model;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Text;
using System;
using System.Collections.Generic;

namespace CoreProject
{
    public class CoreFacade
    {
        private readonly bool _includeDay;
        private readonly bool _includeMonth;
        private readonly bool _includeWeek;
        private readonly MLContext _mlContext;
        private readonly IDataView _trainData;
        private readonly IDataView _testData;
        private readonly DateTime _datePrediction;
        public CoreFacade(bool includeDay, 
                          bool includeMonth, 
                          bool includeWeek, 
                          MLContext mLContext,
                          IDataView trainData,
                          IDataView testData,
                          DateTime datePrediction)
        {
            _includeDay = includeDay;
            _includeMonth = includeMonth;
            _includeWeek = includeWeek;
            _mlContext = mLContext;
            _trainData = trainData;
            _testData = testData;
            _datePrediction = datePrediction;
        }
        public PredictedCombinaison PredictCombinaison()
        {
            PredictedCombinaison result = new PredictedCombinaison();
            var trainingColumn1 = BuildAndTrainUsingParams(ColumnEnum.Column1);
            Evaluate(trainingColumn1);
            result.Column1 = PredictColumn(trainingColumn1, ColumnEnum.Column1);

            var trainingColumn2 = BuildAndTrainUsingParams(ColumnEnum.Column2);
            Evaluate(trainingColumn2);
            result.Column2 = PredictColumn(trainingColumn2, ColumnEnum.Column2);

            var trainingColumn3 = BuildAndTrainUsingParams(ColumnEnum.Column3);
            Evaluate(trainingColumn3);
            result.Column3 = PredictColumn(trainingColumn3, ColumnEnum.Column3);

            var trainingColumn4 = BuildAndTrainUsingParams(ColumnEnum.Column4);
            Evaluate(trainingColumn4);
            result.Column4 = PredictColumn(trainingColumn4, ColumnEnum.Column4);

            var trainingColumn5 = BuildAndTrainUsingParams(ColumnEnum.Column5);
            Evaluate(trainingColumn5);
            result.Column5 = PredictColumn(trainingColumn5, ColumnEnum.Column5);

           var trainingColumn6 = BuildAndTrainUsingParams(ColumnEnum.Column6);
            Evaluate(trainingColumn5);
            result.Column6 = PredictColumn(trainingColumn6, ColumnEnum.Column6);
                       
            var trainingColumnBonus = BuildAndTrainUsingParams(ColumnEnum.Bonus);
            Evaluate(trainingColumnBonus);
            result.Bonus = PredictColumn(trainingColumnBonus, ColumnEnum.Bonus);

            return result;
        }

        private ITransformer BuildAndTrainUsingParams(ColumnEnum column)
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
                            .Append(_mlContext.Regression.Trainers.LbfgsPoissonRegression());

                      return res.Fit(_trainData);

            }
            if(estimatorTransformer != null)
            {
                var res2 = estimatorTransformer.Append(_mlContext.Transforms.Concatenate("Features", features.ToArray()))
                                 .Append(_mlContext.Transforms.CopyColumns("Label", System.Enum.GetName(typeof(ColumnEnum), column)))
                                 .Append(_mlContext.Regression.Trainers.LbfgsPoissonRegression());
                return res2.Fit(_trainData);
            }
            var res3 = textTransformer.Append(_mlContext.Transforms.Concatenate("Features", features.ToArray()))
                                 .Append(_mlContext.Transforms.CopyColumns("Label", System.Enum.GetName(typeof(ColumnEnum), column)))
                                 .Append(_mlContext.Regression.Trainers.LbfgsPoissonRegression());
            return res3.Fit(_trainData);
        }

        private void Evaluate(ITransformer model)
        {
            var prediction = model.Transform(_testData);
            var metrics = _mlContext.Regression.Evaluate(prediction, "Label", "Score");
        }
        private int PredictColumn(ITransformer model, ColumnEnum column)
        {
                var test = new ResultatJsonFormat()
                {
                    Date = _datePrediction,
                    DateString = _datePrediction.ToShortDateString()
                };
            switch(column)
            {
                case ColumnEnum.Column1:
                    var predictionFunction = _mlContext.Model.CreatePredictionEngine<ResultatJsonFormat, PredictionColumn1>(model);
                    return (int)predictionFunction.Predict(test).Column1;
                case ColumnEnum.Column2:
                    var predictionFunction2 = _mlContext.Model.CreatePredictionEngine<ResultatJsonFormat, PredictionColumn2>(model);
                    return (int)predictionFunction2.Predict(test).Column2;
                case ColumnEnum.Column3:
                    var predictionFunction3 = _mlContext.Model.CreatePredictionEngine<ResultatJsonFormat, PredictionColumn3>(model);
                    return (int)predictionFunction3.Predict(test).Column3;
                case ColumnEnum.Column4:
                    var predictionFunction4 = _mlContext.Model.CreatePredictionEngine<ResultatJsonFormat, PredictionColumn4>(model);
                    return (int)predictionFunction4.Predict(test).Column4;
                case ColumnEnum.Column5:
                    var predictionFunction5 = _mlContext.Model.CreatePredictionEngine<ResultatJsonFormat, PredictionColumn5>(model);
                    return (int)predictionFunction5.Predict(test).Column5;
                case ColumnEnum.Column6:
                    var predictionFunction6 = _mlContext.Model.CreatePredictionEngine<ResultatJsonFormat, PredictionColumn6>(model);
                    return (int)predictionFunction6.Predict(test).Column6;
                 case ColumnEnum.Bonus:
                    var predictionFunctionBonus = _mlContext.Model.CreatePredictionEngine<ResultatJsonFormat, PredictionBonus>(model);
                    return (int)predictionFunctionBonus.Predict(test).Bonus;
            }
            return 0;
        }
    }
}
