using EngineProject.Helpers;
using EngineProject.Enums;
using System.Windows.Forms;
using System.Drawing;
using EngineProject.Structures;
using EngineProject.Managers;
using EngineProject.Infrastructure;

namespace UI
{
    public partial class Form1 : Form
    {
        private BotSetup Setup = new BotSetup();

        private KeyListener setupTracker = null;
        private KeyListener autobattleStarter = null;
        private KeyListener autobattleStopper = null;
        private KeyListener resetTracker = null;
        private KeyListener loadSetupTracker = null;

        private Action GetHpPos1 = null;
        private Action GetHpPos2 = null;
        private Action GetWeightPos1 = null;
        private Action GetWeightPos2 = null;
        private Action GetTargetPos1 = null;
        private Action GetTargetPos2 = null;
        private Action StartAutoBattler = null;
        private Action StopAutoBattler = null;

        private SingleRunTimer displayTimer;

        private bool AutobattleEnabled = false;
        private bool AutobattleSetupFinished = false;

        private void SetEnabledtargetMostersTextbox(bool enabled)
        {
            UIHelper.SetLabelEnabled(this, label17, enabled);
            UIHelper.SetLabelEnabled(this, label18, enabled);
            UIHelper.SetTextboxEnabled(this, textBox1, enabled);
        }

        private void SetEnabledIngameStateLabels(bool enabled)
        {
            UIHelper.SetLabelEnabled(this, label5, enabled);
            UIHelper.SetLabelEnabled(this, label6, enabled);
            UIHelper.SetLabelEnabled(this, label7, enabled);
            UIHelper.SetLabelEnabled(this, label8, enabled);
            UIHelper.SetLabelEnabled(this, label9, enabled);
            UIHelper.SetLabelEnabled(this, label10, enabled);
            UIHelper.SetLabelEnabled(this, label11, enabled);
            UIHelper.SetLabelEnabled(this, label12, enabled);
            UIHelper.SetLabelEnabled(this, label13, enabled);
            UIHelper.SetLabelEnabled(this, label14, enabled);
            UIHelper.SetLabelEnabled(this, label15, enabled);
            UIHelper.SetLabelEnabled(this, label16, enabled);
        }

        private void LoadLastBotSetup()
        {
            try
            {
                if (!AutobattleEnabled)
                {
                    Setup = SettingsManager.LoadLastSavedBotSetupFromFile();
                    UIHelper.SetCheckboxChecked(this, checkBox1, true);
                    UIHelper.SetCheckboxChecked(this, checkBox2, true);
                    UIHelper.SetCheckboxChecked(this, checkBox3, true);
                    UIHelper.SetCheckboxChecked(this, checkBox4, true);
                    UIHelper.SetCheckboxChecked(this, checkBox5, true);
                    UIHelper.SetCheckboxChecked(this, checkBox6, true);

                    UIHelper.SetCheckboxEnabled(this, checkBox1, false);
                    UIHelper.SetCheckboxEnabled(this, checkBox2, false);
                    UIHelper.SetCheckboxEnabled(this, checkBox3, false);
                    UIHelper.SetCheckboxEnabled(this, checkBox4, false);
                    UIHelper.SetCheckboxEnabled(this, checkBox5, false);
                    UIHelper.SetCheckboxEnabled(this, checkBox6, false);

                    UIHelper.SetTextboxText(this, textBox1, Setup.TargetsList);
                    setupTracker.StopListening();
                    GameStatsManager.SetupTracking(Setup.HpBarArea, Setup.TargetArea, Setup.WeightArea);
                    AutobattleManager.SetupAutobattler(SettingsManager.Skills, StopAutoBattler, StartAutoBattler);
                    AutobattleSetupFinished = true;
                    displayTimer.Start();
                    UIHelper.SetButtonEnabled(this, button2, true);
                    SetEnabledIngameStateLabels(true);
                }
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, $"Loading last saved setup error");
            }
        }

        private void ResetAll()
        {
            try
            {
                if (AutobattleEnabled) AutobattleManager.Stop();
                displayTimer.Stop();
                SetEnabledIngameStateLabels(false);
                SetEnabledtargetMostersTextbox(true);
                UIHelper.SetButtonEnabled(this, button3, true);
                UIHelper.SetButtonEnabled(this, button2, false);
                UIHelper.SetTextboxText(this, textBox1, "");
                Setup.TargetsList = "";
                AutobattleEnabled = false;
                AutobattleSetupFinished = false;

                UIHelper.SetCheckboxChecked(this, checkBox1, false);
                UIHelper.SetCheckboxChecked(this, checkBox2, false);
                UIHelper.SetCheckboxChecked(this, checkBox3, false);
                UIHelper.SetCheckboxChecked(this, checkBox4, false);
                UIHelper.SetCheckboxChecked(this, checkBox5, false);
                UIHelper.SetCheckboxChecked(this, checkBox6, false);

                UIHelper.SetCheckboxEnabled(this, checkBox1, true);
                UIHelper.SetCheckboxEnabled(this, checkBox2, false);
                UIHelper.SetCheckboxEnabled(this, checkBox3, false);
                UIHelper.SetCheckboxEnabled(this, checkBox4, false);
                UIHelper.SetCheckboxEnabled(this, checkBox5, false);
                UIHelper.SetCheckboxEnabled(this, checkBox6, false);

                setupTracker.StopListening();
                Setup = new BotSetup();
                setupTracker = new KeyListener(GetHpPos1, InputKeys.A, false, true);
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, $"Reset settings error");
            }
        }

        public Form1()
        {
            InitializeComponent();
            try
            {
                SettingsManager.SetupSettings();
                ScreenHelper.SetupScreenHelper();

                displayTimer = new SingleRunTimer(SettingsManager.DisplayTimerFrequency);
                displayTimer.Elapsed += (sender, e) =>
                {
                    UIHelper.SetLabelText(this, label6, GameStatsManager.HP.HpPercentForLabel);
                    UIHelper.SetLabelText(this, label8, GameStatsManager.Weight.WeightStatusName + 
                        (!string.IsNullOrWhiteSpace(GameStatsManager.Weight.WeightText) ? $" ({GameStatsManager.Weight.WeightText})" : ""));
                    UIHelper.SetLabelText(this, label13, AutobattleManager.GetControlInputQueue());
                    UIHelper.SetLabelText(this, label14, AutobattleManager.GetSkillsInputQueue());
                    UIHelper.SetLabelText(this, label16, AutobattleManager.CurrentStepName);

                    var hasTarget = GameStatsManager.Target?.HasTargetForLabel ?? false;
                    if (!hasTarget) 
                    { 
                        UIHelper.SetLabelText(this, label9, "Empty");
                    }
                    else
                    {
                        var targetName = GameStatsManager.Target?.TargetNameForLabel;
                        var targetHP = GameStatsManager.Target?.TargetHPForLabel;
                        UIHelper.SetLabelText(this, label9, "Set" 
                            + (!string.IsNullOrWhiteSpace(targetName) 
                                ? $" ({targetName}) {targetHP}" 
                                : ""));
                    }
                };

                GetHpPos1 = () =>
                {
                    Setup.HpBarPos1 = System.Windows.Forms.Cursor.Position;
                    setupTracker.StopListening();
                    setupTracker = new KeyListener(GetHpPos2, InputKeys.A, false, true);

                    UIHelper.SetCheckboxChecked(this, checkBox1, true);
                    UIHelper.SetCheckboxEnabled(this, checkBox1, false);
                    UIHelper.SetCheckboxEnabled(this, checkBox2, true);
                };

                GetHpPos2 = () =>
                {
                    Setup.HpBarPos2 = System.Windows.Forms.Cursor.Position;
                    Setup.HpBarArea = new OnScreenArea(Setup.HpBarPos1, Setup.HpBarPos2);
                    setupTracker.StopListening();
                    setupTracker = new KeyListener(GetWeightPos1, InputKeys.A, false, true);

                    UIHelper.SetCheckboxChecked(this, checkBox2, true);
                    UIHelper.SetCheckboxEnabled(this, checkBox2, false);
                    UIHelper.SetCheckboxEnabled(this, checkBox3, true);
                };

                GetWeightPos1 = () =>
                {
                    Setup.WeightPos1 = System.Windows.Forms.Cursor.Position;
                    setupTracker.StopListening();
                    setupTracker = new KeyListener(GetWeightPos2, InputKeys.A, false, true);

                    UIHelper.SetCheckboxChecked(this, checkBox3, true);
                    UIHelper.SetCheckboxEnabled(this, checkBox3, false);
                    UIHelper.SetCheckboxEnabled(this, checkBox4, true);
                };

                GetWeightPos2 = () =>
                {
                    Setup.WeightPos2 = System.Windows.Forms.Cursor.Position;
                    Setup.WeightArea = new OnScreenArea(Setup.WeightPos1, Setup.WeightPos2);
                    setupTracker.StopListening();
                    setupTracker = new KeyListener(GetTargetPos1, InputKeys.A, false, true);

                    UIHelper.SetCheckboxChecked(this, checkBox4, true);
                    UIHelper.SetCheckboxEnabled(this, checkBox4, false);
                    UIHelper.SetCheckboxEnabled(this, checkBox5, true);
                };

                GetTargetPos1 = () =>
                {
                    Setup.TargetPos1 = System.Windows.Forms.Cursor.Position;
                    setupTracker.StopListening();
                    setupTracker = new KeyListener(GetTargetPos2, InputKeys.A, false, true);

                    UIHelper.SetCheckboxChecked(this, checkBox5, true);
                    UIHelper.SetCheckboxEnabled(this, checkBox5, false);
                    UIHelper.SetCheckboxEnabled(this, checkBox6, true);
                };

                GetTargetPos2 = () =>
                {
                    Setup.TargetPos2 = System.Windows.Forms.Cursor.Position;
                    Setup.TargetArea = new OnScreenArea(Setup.TargetPos1, Setup.TargetPos2);
                    GameStatsManager.SetupTracking(Setup.HpBarArea, Setup.TargetArea, Setup.WeightArea);
                    AutobattleManager.SetupAutobattler(SettingsManager.Skills, StopAutoBattler, StartAutoBattler);
                    AutobattleSetupFinished = true;
                    displayTimer.Start();

                    UIHelper.SetCheckboxChecked(this, checkBox6, true);
                    UIHelper.SetCheckboxEnabled(this, checkBox6, false);
                    UIHelper.SetButtonEnabled(this, button2, true);

                    SetEnabledIngameStateLabels(true);
                };

                StartAutoBattler = () =>
                {
                    UIHelper.SetButtonText(this, button2, "STOP (Ctrl + S)");
                    SetEnabledtargetMostersTextbox(false);
                    UIHelper.SetButtonEnabled(this, button3, false);
                    AutobattleEnabled = true;
                };

                StopAutoBattler = () =>
                {
                    UIHelper.SetButtonText(this, button2, "START (Ctrl + W)");
                    SetEnabledtargetMostersTextbox(true);
                    UIHelper.SetButtonEnabled(this, button3, true);
                    AutobattleEnabled = false;
                };

                setupTracker = new KeyListener(GetHpPos1, InputKeys.A, false, true);
                autobattleStarter = new KeyListener(() => { if (!AutobattleEnabled && AutobattleSetupFinished) AutobattleManager.Start(Setup); }, InputKeys.W, true, true);
                autobattleStopper = new KeyListener(() => { if (AutobattleEnabled && AutobattleSetupFinished) AutobattleManager.Stop(); }, InputKeys.S, true, true);
                resetTracker = new KeyListener(ResetAll, InputKeys.R, true, true);
                loadSetupTracker = new KeyListener(LoadLastBotSetup, InputKeys.T, true, true);
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, $"Initialization error");
                throw ex;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ResetAll();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (AutobattleSetupFinished)
            {
                if (!AutobattleEnabled)
                {
                    AutobattleManager.Start(Setup);
                }
                else
                {
                    AutobattleManager.Stop();
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                setupTracker.StopListening();
                autobattleStarter.StopListening();
                autobattleStopper.StopListening();
                resetTracker.StopListening();
                loadSetupTracker.StopListening();
            }
            catch { }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Setup.TargetsList = textBox1.Text;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            LoadLastBotSetup();
        }
    }
}