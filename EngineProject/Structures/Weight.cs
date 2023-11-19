using EngineProject.Enums;
using EngineProject.Helpers;
using EngineProject.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineProject.Structures
{
    //Weight stats
    public class Weight
    {
        private OnScreenArea WeightArea { get; set; }
        private WeightFormat WeightNumber = WeightFormat.Default;
        private List<TrackableColors> WeightIgnoredColors { get; set; } 
        public string WeightText { get; set; } //Value for UI

        private void SetupWeight(OnScreenArea weightArea, List<TrackableColors> weightIgnoredColors)
        {
            WeightArea = weightArea;
            WeightIgnoredColors = weightIgnoredColors;
            WeightText = "";
        }

        public Weight(OnScreenArea weightArea, List<TrackableColors> weightIgnoredColors)
        {
            SetupWeight(weightArea, weightIgnoredColors);
        }

        public Weight(OnScreenArea weightArea, TrackableColors weightIgnoredColor)
        {
            SetupWeight(weightArea, new List<TrackableColors>() { weightIgnoredColor });
        }

        public WeightStatuses WeightStatus
        {
            get
            {
                return WeightNumber.WeightStatus;
            }
        }

        public string WeightStatusName
        {
            get
            {
                return WeightNumber.WeightStatusName;
            }
        }

        
        public void UpdateCurrentWeight()
        {
            try
            {
                var weightColorCode = ScreenHelper.GetMostPopularColorInArea(WeightArea, WeightIgnoredColors);
                var newWeightNumber = WeightFormat.Parse(ScreenHelper.GetTextInArea(WeightArea, weightColorCode));
                //unsuccessful attempt
                if (newWeightNumber.Equal(WeightFormat.Default) && WeightNumber != null) return;
                WeightNumber = newWeightNumber;
                WeightText = $"{WeightNumber.CurrentWeight}{WeightFormat.WeightDelimiter}{WeightNumber.LimitWeight}";
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, $"Updating current weight error");
            }
        }

        public bool WeightHasChanged()
        {
            var hasChanged = false;
            try
            {
                var oldWeight = WeightNumber;
                UpdateCurrentWeight();
                hasChanged = WeightNumber.CurrentWeight != oldWeight.CurrentWeight;
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, $"Checking weight changes error");
            }
            return hasChanged;
        }
    }
}
