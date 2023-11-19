using EngineProject.Enums;
using EngineProject.Helpers;
using EngineProject.Infrastructure;
using EngineProject.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineProject.Structures
{
    public class HP
    {
        private double MaxHpColorPercent { get; set; }
        private double HpColorPercent { get; set; }
        private OnScreenArea HpBarArea { get; set; }
        private ThreadSafeQueue<HPSnapshot> HPSnapshots { get; set; }
        private TrackableColors HPColor { get; set; }
        public string HpPercentForLabel { get; set; } //last value for UI

        //Get current HP and save value for UI (optimization)
        public int GetHpPercent()
        {
            try
            {
                HpColorPercent = ScreenHelper.GetPercentOfColor(HpBarArea, (int)HPColor);
                HpColorPercent = HpColorPercent <= 0 ? 0 : HpColorPercent;
                var newHPPercent = (int)(Math.Round(HpColorPercent / MaxHpColorPercent, 2) * 100);
                newHPPercent = newHPPercent > 100 ? 100 : newHPPercent <= 0 ? 0 : newHPPercent;
                HPSnapshots.Add(new HPSnapshot(newHPPercent, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, "Getting HP percent error");
            }
            var hpPercent = HPSnapshots.GetLast().HPPercent;
            HpPercentForLabel = $"{hpPercent}%";
            return hpPercent;
        }

        //Return change flag and last HP value in percents
        public HPChangeInfo GetHpChangeInfo(int seconds = 1, bool? descending = null, uint diffPercent = 1)
        {
            var newestHPPercent = GetHpPercent();
            var hpSnapshots = HPSnapshots.GetCopy()
                .Where(w => (DateTime.Now - w.SnapshotTime).TotalSeconds <= seconds)
                .OrderBy(o => o.SnapshotTime);
            if (!hpSnapshots.Any()) return new HPChangeInfo(false, newestHPPercent);
            var oldestSnapshot = hpSnapshots.First();
            if (newestHPPercent == 0) return new HPChangeInfo(true, newestHPPercent); ; //in case no HP label
            var difference = newestHPPercent - oldestSnapshot.HPPercent;
            difference = Math.Abs(difference) < diffPercent ? 0 : difference;
            return new HPChangeInfo( 
                (difference != 0 && descending == null)
                || (difference < 0 && descending == true)
                || (difference > 0 && descending == false),
                newestHPPercent);
        }

        public bool IsNeedHeeling { get { return GetHpPercent() <= SettingsManager.HpPercentToHeal; } }

        public bool IsLowHP { get { return GetHpPercent() <= SettingsManager.LowHpPercent; } }

        //Write max HP pixels for new target/during initialization
        public void UpdateMaxHP()
        {
            MaxHpColorPercent = ScreenHelper.GetPercentOfColor(HpBarArea, (int)HPColor);
        }

        public HP(OnScreenArea hpBarArea, TrackableColors hpColor)
        {
            HpBarArea = hpBarArea;
            HPColor = hpColor;
            UpdateMaxHP();
            HPSnapshots = new ThreadSafeQueue<HPSnapshot>();
            HPSnapshots.Add(new HPSnapshot(100, DateTime.Now));
            GetHpPercent();
        }
    }

    //Snapshot for HP history (to check changes)
    public class HPSnapshot
    {
        public int HPPercent { get; set; }
        public DateTime SnapshotTime { get; set; }
        public HPSnapshot(int hpPercent, DateTime snapshotTime)
        {
            HPPercent = hpPercent;
            SnapshotTime = snapshotTime;
        }
    }

    //Current HP and change flag
    public class HPChangeInfo
    {
        public int HPPercent { get; set; }
        public bool HasChanged { get; set; }
        public HPChangeInfo(bool hasChanged, int hpPercent)
        {
            HPPercent = hpPercent;
            HasChanged = hasChanged;
        }
    }
}
