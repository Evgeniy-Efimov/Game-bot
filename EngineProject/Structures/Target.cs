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
    //Current target stats
    public class Target
    {
        private OnScreenArea TargetArea { get; set; }
        private HP HP { get; set; }

        //Last values for UI
        public string TargetNameForLabel { get; set; } 
        public string TargetHPForLabel { get; set; }
        public bool HasTargetForLabel { get; set; }

        public Target(OnScreenArea targetArea, TrackableColors targetHPColor)
        {
            TargetArea = targetArea;
            HP = new HP(targetArea, targetHPColor);
            TargetNameForLabel = "";
            TargetHPForLabel = "";
            HasTargetForLabel = false;
        }

        public bool HpHasChanged(int seconds = 1, bool? descending = null, uint diffPercent = 1)
        {
            var hpChangeInfo = HP.GetHpChangeInfo(seconds = 1, descending, diffPercent);
            TargetHPForLabel = GetTargetHPforLabel(hpChangeInfo.HPPercent);
            return hpChangeInfo.HasChanged;
        }

        public void UpdateTargetMaxHP()
        {
            HP.UpdateMaxHP();
        }

        public bool HasTarget
        {
            get
            {
                HasTargetForLabel = ScreenHelper.AreAreasEqual(TargetArea, SettingsManager.TargetSampleFileName, SettingsManager.TargetSectorsCount);
                return HasTargetForLabel;
            }
        }

        public string TargetName 
        { 
            get 
            {
                TargetNameForLabel = ScreenHelper.GetTextInArea(TargetArea, (int)TrackableColors.White);
                return TargetNameForLabel;
            } 
        }

        private string GetTargetHPforLabel(int percent)
        {
            if (!HasTarget) return "";
            percent =
                percent <= 5 ? 0 :
                percent <= 20 ? 20 :
                percent <= 40 ? 40 :
                percent <= 60 ? 60 :
                percent <= 80 ? 80 :
                percent <= 100 ? 100 : 100;
            return $"{percent}%";
        }

        public bool IsTargetInList(string[] targets)
        {
            return StringHelper.IsTextMatchToList(TargetName, targets);
        }
    }
}
