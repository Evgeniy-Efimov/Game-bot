using EngineProject.Enums;
using EngineProject.Structures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineProject.Managers
{
    //Access to bot settings in app.config file
    public static class SettingsManager
    {
        //Settings
        public static int ScreenWidth { get; set; }
        public static int ScreenHeight { get; set; }
        public static double PixelsInOneDegreeOfMouseRotate { get; set; }
        public static int TargetSectorsCount { get; set; }
        public static string TargetSampleFileName { get; set; }
        public static double MaxDupDifferenceInProcent { get; set; }
        public static double MaxColorDifferenceInProcent { get; set; }
        public static int InputTimerFrequency { get; set; }
        public static double HpPercentToHeal { get; set; }
        public static double LowHpPercent { get; set; }
        public static int AutobattleTimerFrequency { get; set; }
        public static int HealingTimerFrequency { get; set; }
        public static int EscapingTimerFrequency { get; set; }
        public static int DisplayTimerFrequency { get; set; }
        public static int SkillsTimerFrequency { get; set; }
        public static int StuckTimeout { get; set; }
        public static int NotargetRunTimeBase { get; set; }
        public static int NotargetRunTimeMax { get; set; }
        public static int LootAttemptsInRow { get; set; }
        public static int LootEmptyAttempts { get; set; }
        public static int LootAttemptsDelay { get; set; }
        public static bool EscapeOnOverweight { get; set; }
        public static int EscapeTimeout { get; set; }
        public static List<Skill> Skills { get; set; }
        public static int SkillsGlobalCooldown { get; set; }
        public static string EscapePotionButton { get; set; }
        public static int StuckRunTime { get; set; }
        public static int SearchingRotateAngel { get; set; }
        public static int SearchingAttemptsToRun { get; set; }
        public static int SearchingInputPerDirection { get; set; }

        private static Dictionary<string, string> SettingsDictionary;

        //Constants
        private static string SaveFilePrefix = "setup_";
        private static string SaveFileExt = ".json";

        #region Save and load setup methods

        public static BotSetup LoadLastSavedBotSetupFromFile()
        {
            var saveDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Saves");
            if (!Directory.Exists(saveDirectoryPath)) throw new Exception("No saves were found");

            var fileToLoad = Directory.GetFiles(saveDirectoryPath).Where(w => Path.GetFileName(w).StartsWith(SaveFilePrefix)).FirstOrDefault();
            if (fileToLoad == null) throw new Exception("No saves were found");

            var botSetup = JsonConvert.DeserializeObject<BotSetup>(File.ReadAllText(fileToLoad));
            if (botSetup == null) throw new Exception("Can't read save file");
            return botSetup;
        }

        public static void SaveCurrentBotSetupToFile(BotSetup setup)
        {
            try
            {
                var saveDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Saves");
                if (!Directory.Exists(saveDirectoryPath)) Directory.CreateDirectory(saveDirectoryPath);

                var filesToDelete = Directory.GetFiles(saveDirectoryPath)
                    .Select(s => Path.GetFileName(s))
                    .Where(w => w.StartsWith(SaveFilePrefix));

                foreach (var oldSaveFile in filesToDelete)
                {
                    File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Saves", oldSaveFile));
                }

                string jsonContent = JsonConvert.SerializeObject(setup);
                var saveFileName = $"{SaveFilePrefix}{DateTime.Now.ToString("yyyy'-'MM'-'dd")}{SaveFileExt}";
                File.WriteAllText(Path.Combine(saveDirectoryPath, saveFileName), jsonContent);
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, "Can't save bot setup");
            }
        }

        #endregion

        #region Utility methods

        private static string GetSettingValue(string settingName)
        {
            if (SettingsDictionary == null)
            {
                var settingsCollection = ConfigurationManager.AppSettings;
                SettingsDictionary = ConfigurationManager.AppSettings.AllKeys.Distinct()
                    .ToDictionary(key => key, val => settingsCollection[val]);
            }
            if (!SettingsDictionary.ContainsKey(settingName)) throw new Exception($"Can't get value of setting {settingName} from app.config, setting is missing");
            return SettingsDictionary[settingName];
        }
        
        private static Skill TryParseSkillInfo(string skillInfo)
        {
            try
            {
                var skillFields = skillInfo.Split(';');
                return new Skill(skillFields[0], skillFields[1], int.Parse(skillFields[2]), Convert.ToBoolean(skillFields[3]), Convert.ToBoolean(skillFields[4]));
            }
            catch (Exception ex)
            {
                throw new Exception($"Can't parse skill data - '{skillInfo}': {ex.Message}");
            }
        }

        #endregion

        //Load settings from app.configs
        public static void SetupSettings()
        {
            try
            {
                ScreenWidth = Screen.PrimaryScreen.Bounds.Width;
                ScreenHeight = Screen.PrimaryScreen.Bounds.Height;
                PixelsInOneDegreeOfMouseRotate = double.Parse(GetSettingValue("PixelsInOneDegreeOfMouseRotate"));
                TargetSectorsCount = int.Parse(GetSettingValue("TargetSectorsCount"));
                MaxDupDifferenceInProcent = double.Parse(GetSettingValue("MaxDupDifferenceInProcent"));
                MaxColorDifferenceInProcent = double.Parse(GetSettingValue("MaxColorDifferenceInProcent"));
                TargetSampleFileName = GetSettingValue("TargetSampleFileName");
                InputTimerFrequency = int.Parse(GetSettingValue("InputTimerFrequency"));
                HpPercentToHeal = double.Parse(GetSettingValue("HpPercentToHeal"));
                LowHpPercent = double.Parse(GetSettingValue("LowHpPercent"));
                AutobattleTimerFrequency = int.Parse(GetSettingValue("AutobattleTimerFrequency"));
                HealingTimerFrequency = int.Parse(GetSettingValue("HealingTimerFrequency"));
                DisplayTimerFrequency = int.Parse(GetSettingValue("DisplayTimerFrequency"));
                EscapingTimerFrequency = int.Parse(GetSettingValue("EscapingTimerFrequency"));
                SkillsTimerFrequency = int.Parse(GetSettingValue("SkillsTimerFrequency"));
                EscapePotionButton = GetSettingValue("EscapePotionButton");
                StuckTimeout = int.Parse(GetSettingValue("StuckTimeout"));
                NotargetRunTimeBase = int.Parse(GetSettingValue("NotargetRunTimeBase"));
                NotargetRunTimeMax = int.Parse(GetSettingValue("NotargetRunTimeMax"));
                LootAttemptsInRow = int.Parse(GetSettingValue("LootAttemptsInRow"));
                LootEmptyAttempts = int.Parse(GetSettingValue("LootEmptyAttempts"));
                LootAttemptsDelay = int.Parse(GetSettingValue("LootAttemptsDelay"));
                EscapeOnOverweight = Convert.ToBoolean(GetSettingValue("EscapeOnOverweight"));
                EscapeTimeout = int.Parse(GetSettingValue("EscapeTimeout"));
                SkillsGlobalCooldown = int.Parse(GetSettingValue("SkillsGlobalCooldown"));
                StuckRunTime = int.Parse(GetSettingValue("StuckRunTime"));
                SearchingRotateAngel = int.Parse(GetSettingValue("SearchingRotateAngel"));
                SearchingAttemptsToRun = int.Parse(GetSettingValue("SearchingAttemptsToRun"));
                SearchingInputPerDirection = int.Parse(GetSettingValue("SearchingInputPerDirection"));

                Skills = new List<Skill>();
                var skillInfoList = new List<string>();
                var skill1Info = GetSettingValue("Skill1");
                if (!string.IsNullOrEmpty(skill1Info)) skillInfoList.Add(skill1Info);
                var skill2Info = GetSettingValue("Skill2");
                if (!string.IsNullOrEmpty(skill2Info)) skillInfoList.Add(skill2Info);
                var skill3Info = GetSettingValue("Skill3");
                if (!string.IsNullOrEmpty(skill3Info)) skillInfoList.Add(skill3Info);
                var skill4Info = GetSettingValue("Skill4");
                if (!string.IsNullOrEmpty(skill4Info)) skillInfoList.Add(skill4Info);
                var skill5Info = GetSettingValue("Skill5");
                if (!string.IsNullOrEmpty(skill5Info)) skillInfoList.Add(skill5Info);
                var skill6Info = GetSettingValue("Skill6");
                if (!string.IsNullOrEmpty(skill6Info)) skillInfoList.Add(skill6Info);
                var skill7Info = GetSettingValue("Skill7");
                if (!string.IsNullOrEmpty(skill7Info)) skillInfoList.Add(skill7Info);
                var skill8Info = GetSettingValue("Skill8");
                if (!string.IsNullOrEmpty(skill8Info)) skillInfoList.Add(skill8Info);

                foreach (var skillInfo in skillInfoList)
                {
                    Skills.Add(TryParseSkillInfo(skillInfo));
                }
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, "Settings loading error");
            }
        }
    }

    //Setup state to save and load
    public class BotSetup
    {
        public Point HpBarPos1 { get; set; }
        public Point HpBarPos2 { get; set; }
        public OnScreenArea HpBarArea { get; set; }
        public Point TargetPos1 { get; set; }
        public Point TargetPos2 { get; set; }
        public OnScreenArea TargetArea { get; set; }
        public Point WeightPos1 { get; set; }
        public Point WeightPos2 { get; set; }
        public OnScreenArea WeightArea { get; set; }
        public string TargetsList { get; set; }
    }
}
