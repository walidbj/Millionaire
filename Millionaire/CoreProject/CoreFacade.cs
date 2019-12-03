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
            BuildAndTrainUsingParams(ColumnEnum.Column1);
            return null;
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
                            .Append(_mlContext.Regression.Trainers.FastTree());

                      return res.Fit(_trainData);

            }
            if(estimatorTransformer != null)
            {
                var res2 = estimatorTransformer.Append(_mlContext.Transforms.Concatenate("Features", features.ToArray()))
                                 .Append(_mlContext.Transforms.CopyColumns("Label", System.Enum.GetName(typeof(ColumnEnum), column)))
                                 .Append(_mlContext.Regression.Trainers.FastTree());
                return res2.Fit(_trainData);
            }
            var res3 = textTransformer.Append(_mlContext.Transforms.Concatenate("Features", features.ToArray()))
                                 .Append(_mlContext.Transforms.CopyColumns("Label", System.Enum.GetName(typeof(ColumnEnum), column)))
                                 .Append(_mlContext.Regression.Trainers.FastTree());
            return res3.Fit(_trainData);
        }

        private void Evaluate(ITransformer model)
        {
            var prediction = model.Transform(_testData);
            var metrics = _mlContext.Regression.Evaluate(prediction, "Label", "Score");
        }
        private float PredictColumn(ITransformer model, ColumnEnum column)
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
                    return predictionFunction.Predict(test).Column1;
                case ColumnEnum.Column2:
                    var predictionFunction2 = _mlContext.Model.CreatePredictionEngine<ResultatJsonFormat, PredictionColumn2>(model);
                    return predictionFunction2.Predict(test).Column2;
                case ColumnEnum.Column3:
                    var predictionFunction3 = _mlContext.Model.CreatePredictionEngine<ResultatJsonFormat, PredictionColumn3>(model);
                    return predictionFunction3.Predict(test).Column3;
                case ColumnEnum.Column4:
                    var predictionFunction4 = _mlContext.Model.CreatePredictionEngine<ResultatJsonFormat, PredictionColumn4>(model);
                    return predictionFunction4.Predict(test).Column4;
                case ColumnEnum.Column5:
                    var predictionFunction5 = _mlContext.Model.CreatePredictionEngine<ResultatJsonFormat, PredictionColumn5>(model);
                    return predictionFunction5.Predict(test).Column5;
                case ColumnEnum.Column6:
                    var predictionFunction6 = _mlContext.Model.CreatePredictionEngine<ResultatJsonFormat, PredictionColumn6>(model);
                    return predictionFunction6.Predict(test).Column6;
                 case ColumnEnum.Bonus:
                    var predictionFunctionBonus = _mlContext.Model.CreatePredictionEngine<ResultatJsonFormat, PredictionBonus>(model);
                    return predictionFunctionBonus.Predict(test).Bonus;
            }
            return 0;
        }
    }
}
