using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineProject.Enums;
using EngineProject.Helpers;

namespace EngineProject.Structures
{
    //Weight value class
    public class WeightFormat
    {
        public int CurrentWeight { get; set; }
        public int LimitWeight { get; set; }

        private WeightFormat() { }

        public WeightFormat(int currentWeight, int limitWeight)
        {
            if (currentWeight > limitWeight || currentWeight < 1 || limitWeight < 1)
            {
                currentWeight = Default.CurrentWeight;
                limitWeight = Default.LimitWeight;
            }
            LimitWeight = limitWeight;
            CurrentWeight = currentWeight;
        }

        public WeightStatuses WeightStatus
        {
            get 
            { 
                var overWeightPercent = (double)CurrentWeight / (double)LimitWeight;
                if (overWeightPercent >= 0.9d) return WeightStatuses.CriticalOverweight;
                if (overWeightPercent >= 0.7d) return WeightStatuses.Overweight;
                return WeightStatuses.Normal;
            }
        } 

        public string WeightStatusName
        {
            get 
            {
                var weightStatus = WeightStatus;
                if (weightStatus == WeightStatuses.CriticalOverweight) return "Critical overweight";
                if (weightStatus == WeightStatuses.Overweight) return "Overweight";
                if (weightStatus == WeightStatuses.Normal) return "Normal";
                return ""; 
            }
        }

        public bool Equal(WeightFormat weightFormat)
        {
            if (weightFormat.CurrentWeight == CurrentWeight && weightFormat.LimitWeight == LimitWeight) return true;
            return false;
        }

        //Clear readed weight value
        private static int WeightDelimiterIndex = 0;
        public static WeightFormat Parse(string weightString)
        {
            if (string.IsNullOrWhiteSpace(weightString)) return WeightFormat.Default;
            weightString = weightString.Trim().Replace(" ", "");
            var firstNumberIndex = StringHelper.GetIndexOfFirstNumber(weightString);
            weightString = weightString.Substring(firstNumberIndex);

            var delimiterIndex = weightString.IndexOf(WeightFormat.WeightDelimiter);
            if (delimiterIndex != -1)
            {
                WeightDelimiterIndex = delimiterIndex;
            }
            if (WeightDelimiterIndex != 0)
            {
                var currentWeight = StringHelper.GetNumbersFromText(weightString.Substring(0, WeightDelimiterIndex + 1));
                var limitWeight = StringHelper.GetNumbersFromText(weightString.Substring(WeightDelimiterIndex + 1, weightString.Length - WeightDelimiterIndex - 1));
                return new WeightFormat(currentWeight, limitWeight);
            }
            return WeightFormat.Default;
        }

        public static WeightFormat Default = new WeightFormat() { CurrentWeight = 1, LimitWeight = 1000 };
        public static char WeightDelimiter = '/';
    }
}
