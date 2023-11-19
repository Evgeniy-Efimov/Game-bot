using EngineProject.Enums;
using EngineProject.Helpers;
using EngineProject.Infrastructure;
using EngineProject.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineProject.Managers
{
    //Collect information for logic
    public static class GameStatsManager
    {
        public static Target Target { get; set; }
        public static Weight Weight { get; set; }
        public static HP HP { get; set; }

        public static void SetupTracking(OnScreenArea hpBarArea, OnScreenArea targetArea, OnScreenArea weightArea)
        {
            try
            {
                HP = new HP(hpBarArea, TrackableColors.Red);
                Target = new Target(targetArea, TrackableColors.Green);
                Weight = new Weight(weightArea, TrackableColors.Grey);
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, $"Stats monitor setup error");
                throw ex;
            }
        }
    }
}
