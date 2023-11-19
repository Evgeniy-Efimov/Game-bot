using EngineProject.Infrastructure;
using EngineProject.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineProject.Structures;
using EngineProject.Helpers;

namespace EngineProject.Managers
{
    //Main bot logic
    public static class AutobattleManager
    {
        //Timers for parallel processes
        //Skills dont coflict with control 
        private static SingleRunTimer BattleProcess;
        private static SingleRunTimer SkillsProcess;

        private static BattleStep CurrentStep; //Current step of bot actions
        private static List<Skill> Skills = new List<Skill>(); //Skills to use

        //Parallel input controllers
        private static InputHelper ControlInputManager;
        private static InputHelper SkillsInputManager;

        //Delegates to update form on start/stop
        private static Action FormUpdateOnStop { get; set; }
        private static Action FormUpdateOnStart { get; set; }

        //Count of retries to loot without weight changes (gold has no weight)
        private static int LootAttemptsInRow;
        private static int LootAttemptsDelay;
        private static int LootEmptyAttempts;
        private static int EmptyLootCount = 0; //Current number of loot attempts without results in weight

        private static string[] WantedTargets; //List of wanted targets (all if empty)
        private static int WantedTargetAttempts; //Max count of retries to target wanted
        private static int WantedTargetCount = 0; //Count of retries to target wanted

        //Watchdogs
        //To perfome action in case of block
        private static DateTime StuckTimestap;
        private static int StuckTimeout;
        //To press attack again, just in case to prevent some problems with target
        private static DateTime BattleTimestap;
        private static int BattleTimeout = 5;
        //To move around in case nothing to attack
        private static int NotargetCount;
        private static int NotargetAttempts; //number of camera rotations
        private static int NotargetRunTimeBase;
        private static int NotargetRunTimeMax;
        private static int NotargetRunTime;
        private static int SearchingRotateAngel;

        //Delay after skill is used
        private static int SkillsGlobalCooldown;

        #region Mathods for UI

        public static string GetControlInputQueue()
        {
            return ControlInputManager.GetInputQueue();
        }

        public static string GetSkillsInputQueue()
        {
            return SkillsInputManager.GetInputQueue();
        }

        public static string CurrentStepName
        {
            get
            {
                if (CurrentStep == BattleStep.Looting) return "Looting";
                if (CurrentStep == BattleStep.Fighting) return "Fighting";
                if (CurrentStep == BattleStep.LookingForTarget) return "Searching";
                if (CurrentStep == BattleStep.Escaping) return "Escaping";
                if (CurrentStep == BattleStep.Stop) return "Stop";
                return "Unknown";
            }
        }

        #endregion

        public static void Start(BotSetup setup)
        {
            try
            {
                SettingsManager.SaveCurrentBotSetupToFile(setup);
                UpdateTargets(setup.TargetsList);
                GameStatsManager.Weight.UpdateCurrentWeight();
                foreach (var skill in Skills) skill.ResetTimestamps();
                StuckTimestap = DateTime.Now;
                BattleTimestap = DateTime.Now;
                NotargetCount = 0;
                WantedTargetCount = 0;
                EmptyLootCount = 0;
                BattleProcess.Start();
                SkillsProcess.Start();
                ControlInputManager.Start();
                SkillsInputManager.Start();
                CurrentStep = BattleStep.LookingForTarget; //Initial step
                FormUpdateOnStart();
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, $"Autobattler start error");
                throw ex;
            }
        }

        public static void Stop()
        {
            try 
            {
                BattleProcess.Stop();
                SkillsProcess.Stop();
                CurrentStep = BattleStep.Stop;
                FormUpdateOnStop();
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, $"Autobattler stop error");
                throw ex;
            }
        }

        public static void UpdateTargets(string targetsList)
        {
            WantedTargets = new string[] { };
            if (!string.IsNullOrWhiteSpace(targetsList))
            {
                WantedTargets = targetsList.Split(',').Where(w => !string.IsNullOrWhiteSpace(w)).ToArray();
            }
        }

        #region Methods to update current state for next step

        private static void ChangeStatusToSearching()
        {
            if (CurrentStep != BattleStep.Stop)
            {
                CurrentStep = BattleStep.LookingForTarget;
                ControlInputManager.RotateByMouse(120, 0, true, 100);
                NotargetCount = 0;
                NotargetRunTime = NotargetRunTimeBase;
            }
        }

        private static void ChangeStatusToFighting()
        {
            if (CurrentStep != BattleStep.Stop)
            {
                CurrentStep = BattleStep.Fighting;
                StuckTimestap = DateTime.Now;
                BattleTimestap = DateTime.Now;
                NotargetCount = 0;
                WantedTargetCount = 0;
                NotargetRunTime = NotargetRunTimeBase;
                ControlInputManager.PressButton(InputKeys.Home);
            }
        }

        private static void ChangeStatusToLooting()
        {
            if (CurrentStep != BattleStep.Stop)
            {
                CurrentStep = BattleStep.Looting;
                EmptyLootCount = 0;
                //Wait loot drop
                if (SkillsInputManager.InputActionsBuffer.GetCopy().Any())
                {
                    //Wait till skills are used before looting
                    while (SkillsInputManager.InputActionsBuffer.GetCopy().Any()) Thread.Sleep(20);
                    Thread.Sleep(100);
                }
                else
                {
                    Thread.Sleep(300);
                }
                GameStatsManager.Weight.UpdateCurrentWeight(); //Update weight data after skills were used
            }
        }

        #endregion

        //Main proc
        public static void SetupAutobattler(List<Skill> skills, Action formUpdateOnStop, Action formUpdateOnStart)
        {
            try
            {
                Skills = skills;
                FormUpdateOnStop = formUpdateOnStop;
                FormUpdateOnStart = formUpdateOnStart;

                NotargetRunTimeBase = SettingsManager.NotargetRunTimeBase;
                NotargetRunTimeMax = SettingsManager.NotargetRunTimeMax;
                NotargetRunTime = NotargetRunTimeBase;
                NotargetAttempts = SettingsManager.SearchingAttemptsToRun;
                SearchingRotateAngel = SettingsManager.SearchingRotateAngel;
                WantedTargetAttempts = SettingsManager.SearchingInputPerDirection;

                StuckTimeout = SettingsManager.StuckTimeout;

                LootAttemptsInRow = SettingsManager.LootAttemptsInRow;
                LootAttemptsDelay = SettingsManager.LootAttemptsDelay;
                LootEmptyAttempts = SettingsManager.LootEmptyAttempts;

                SkillsGlobalCooldown = SettingsManager.SkillsGlobalCooldown;

                CurrentStep = BattleStep.Stop;
                ControlInputManager = new InputHelper();
                SkillsInputManager = new InputHelper();

                //Parallel processes for battle, healing, skills, escaping
                BattleProcess = new SingleRunTimer(SettingsManager.AutobattleTimerFrequency);
                BattleProcess.Elapsed += (sender, e) =>
                {
                    if (CurrentStep == BattleStep.LookingForTarget) //Find new target, initial step
                    {
                        ControlInputManager.PressButton(InputKeys.Insert, 0, true, 200); //Quick target ingame hotkey
                        //Target was set, reset watchdogs, go to fighting
                        if (GameStatsManager.Target.HasTarget && GameStatsManager.Target.IsTargetInList(WantedTargets))
                        {
                            GameStatsManager.Target.UpdateTargetMaxHP(); //Get target max HP
                            ChangeStatusToFighting();
                        }
                        //Try again, select target or rotate/move on N attempt
                        else
                        {
                            WantedTargetCount++;
                            if (WantedTargetCount > WantedTargetAttempts)
                            {
                                //Move around every N rotation
                                if (NotargetCount >= NotargetAttempts)
                                {
                                    ControlInputManager.HoldButton(InputKeys.W, NotargetRunTime, 0, true);
                                    NotargetCount = 0;
                                    NotargetRunTime += NotargetRunTimeBase;
                                    if (NotargetRunTime > NotargetRunTimeMax) NotargetRunTime = NotargetRunTimeMax;
                                }
                                //Just rotate and check another direction
                                else
                                {
                                    ControlInputManager.RotateByMouse(SearchingRotateAngel, 0, true, 100);
                                    NotargetCount++;
                                }
                                //After rotation/run select target without rotation again
                                WantedTargetCount = 0;
                            }
                        }
                    }
                    else if (CurrentStep == BattleStep.Fighting) //Wait till target is dead/disappear
                    {
                        if (!GameStatsManager.Target.HasTarget) //Target is dead/disappear, move to looting
                        {
                            ChangeStatusToLooting();
                        }
                        else //Keep fighting
                        {
                            if (GameStatsManager.Target.HpHasChanged(descending: true, diffPercent: 10)) StuckTimestap = DateTime.Now;
                            if (StuckTimeout < (DateTime.Now - StuckTimestap).TotalSeconds) //Case of block
                            {
                                ControlInputManager.RotateByMouse(180, 0, true, 100);
                                ControlInputManager.HoldButton(InputKeys.W, SettingsManager.StuckRunTime, 0, true);
                                ChangeStatusToSearching();
                            }
                            else if (BattleTimeout < (DateTime.Now - BattleTimestap).TotalSeconds) //Press attack again, just in case
                            {
                                ControlInputManager.PressButton(InputKeys.Home);
                                BattleTimestap = DateTime.Now;
                            }
                        }
                    }
                    else if (CurrentStep == BattleStep.Looting) //Loot all items
                    {
                        for (int i = 0; i < LootAttemptsInRow; i++) ControlInputManager.PressButton(InputKeys.E, 0, true, LootAttemptsDelay);
                        //Weight is same
                        if (!GameStatsManager.Weight.WeightHasChanged())
                        {
                            EmptyLootCount++;
                            //After several attempts move to looking for a target
                            if (EmptyLootCount >= LootEmptyAttempts)
                            {
                                ChangeStatusToSearching();
                            }
                        }
                        //Continue looting, reset empty attempts count
                        else
                        {
                            EmptyLootCount = 0;
                        }
                    }
                    else if (CurrentStep == BattleStep.Escaping) //Escape from battle
                    {
                        SkillsInputManager.PressButton(SettingsManager.EscapePotionButton, SkillsGlobalCooldown, true, (SettingsManager.EscapeTimeout + 1) * 1000);

                        if (!GameStatsManager.HP.GetHpChangeInfo(SettingsManager.EscapeTimeout, true).HasChanged) //Check in safe place
                        {
                            Stop();
                            Thread.Sleep(3000);
                            ControlInputManager.PressButton(InputKeys.Space); //Jump to process loading screen (client problem)
                        }
                    }
                };

                SkillsProcess = new SingleRunTimer(SettingsManager.SkillsTimerFrequency);
                SkillsProcess.Elapsed += (sender, e) =>
                {
                    //Escape and stop if HP is critically low or overweight
                    if (GameStatsManager.HP.IsLowHP 
                        || (SettingsManager.EscapeOnOverweight && GameStatsManager.Weight.WeightStatus == WeightStatuses.Overweight)
                        || GameStatsManager.Weight.WeightStatus == WeightStatuses.CriticalOverweight)
                    {         
                        CurrentStep = BattleStep.Escaping;
                    }
                    else
                    {
                        //Add each skill to separated input queue checking personal cooldown
                        foreach (var skill in skills)
                        {
                            if (CurrentStep == BattleStep.Fighting
                                && (DateTime.Now - skill.LastUsedTimestamp).TotalSeconds >= skill.SecondsDelay
                                && (GameStatsManager.Weight.WeightStatus != WeightStatuses.Overweight || skill.UseIfOverweight))
                            {
                                SkillsInputManager.PressButton(skill.InputKey, SkillsGlobalCooldown);
                                skill.LastUsedTimestamp = DateTime.Now;
                            };
                        }
                        //Add healing to queue if HP is low (but not criticall to escape quicker) and fighting/searching
                        if (GameStatsManager.HP.IsNeedHeeling && !GameStatsManager.HP.IsLowHP 
                            && (CurrentStep == BattleStep.Fighting || CurrentStep == BattleStep.LookingForTarget))
                        {
                            SkillsInputManager.PressButton(InputKeys.Q, SkillsGlobalCooldown);
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, $"Autobattler setup error");
                throw ex;
            }
        }
    }
}
