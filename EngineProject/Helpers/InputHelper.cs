using EngineProject.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using EngineProject.Infrastructure;
using EngineProject.Managers;

namespace EngineProject.Helpers
{
    //Bot input controller
    //One object for control, one for skills (control input and skills input doesn't conflict)
    public class InputHelper
    {
        #region Win api procs

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)] 
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, int cButtons, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        #endregion

        //Constants
        private const uint KEY_DOWN = 0;
        private const uint KEY_UP = 0x0002;
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        private const uint MOUSEEVENTF_ABSOLUTE = 0x8000;
        private const uint MOUSEEVENTF_MOVE = 0x0001;
        private const uint MOUSE_MAX_DELTA = 65535;
        private const uint MOUSEEVENTF_WHEEL = 0x0800;

        private Guid NextInputId { get { return System.Guid.NewGuid(); } }

        //Use buffer with unique keys (excluding E for multiple loot command)
        //Should be thread-safe to read and modify from parallel processes
        public ThreadSafeQueue<InputData> InputActionsBuffer = new ThreadSafeQueue<InputData>();

        //Timer to read input queue
        private SingleRunTimer InputTimer;
        
        //To display queue
        public string GetInputQueue()
        {
            try
            {
                var queue = InputActionsBuffer.GetCopy().Select(s => s.Key).ToArray();
                if (queue != null && queue.Any()) return string.Join(", ", queue);
                return "...";
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, "Getting key input queue error");
                throw ex;
            }
        }

        public InputHelper()
        {
            InputTimer = new SingleRunTimer(SettingsManager.InputTimerFrequency);
            InputTimer.Elapsed += (sender, e) =>
            {
                try
                {
                    var actionToRun = InputActionsBuffer.PullFirst();
                    if (actionToRun != null) actionToRun.Action();
                }
                catch (Exception ex)
                {
                    LogManager.LogException(ex, $"Input error");
                    throw ex;
                }
            }; 
        }

        public void Start()
        {
            InputTimer.Start();
        }

        public void Stop()
        {
            InputTimer.Stop();
        }

        //Input queue delay to stop input processing
        //Outer proc delay to stop main process after adding key to input queue/input is over
        public void PressButton(string key, int inputQueueAddDelay = 0, bool outerProcWaitPress = false, int outerProcAddDelay = 0)
        {
            HoldButton(key, 50, inputQueueAddDelay, outerProcWaitPress, outerProcAddDelay);
        }

        //Exclude duplicates (excluding E for multiple loot attempts)
        private bool CanAddInputAction(string key)
        {
            return !InputActionsBuffer.GetCopy().Any(a => a.Key == key) || key == InputKeys.E;
        }

        public void HoldButton(string key, int milliseconds, int inputQueueAddDelay = 0, bool outerProcWaitPress = false, int outerProcAddDelay = 0)
        {
            if (CanAddInputAction(key))
            {
                var inputId = NextInputId;
                InputActionsBuffer.Add(new InputData
                    (inputId, key, new Action(() =>
                    {
                        keybd_event((byte)InputKeys.VCodes[key], 0, KEY_DOWN, (UIntPtr)0);
                        Thread.Sleep(milliseconds);
                        keybd_event((byte)InputKeys.VCodes[key], 0, KEY_UP, (UIntPtr)0);
                        Thread.Sleep(inputQueueAddDelay);
                    }))
                );
                if (outerProcWaitPress)
                {
                    while (InputActionsBuffer.GetCopy().Any(a => a.InputId == inputId)) Thread.Sleep(20);
                }
                Thread.Sleep(outerProcAddDelay);
            }
        }

        //Mouse moves doesn't correlate with screen pixels by default
        private int MouseDeltaToPixels(int delta, bool byX)
        {
            var screenMaxMove = byX ? SettingsManager.ScreenWidth : SettingsManager.ScreenHeight;
            return (delta * (int)MOUSE_MAX_DELTA / screenMaxMove) + 1;
        }

        public void RotateByMouse(int angel, int inputQueueAddDelay = 0, bool outerProcWaitPress = false, int outerProcAddDelay = 0)
        {
            if (CanAddInputAction(InputKeys.MouseRotate))
            {
                var inputId = NextInputId;
                InputActionsBuffer.Add(new InputData
                    (inputId, InputKeys.MouseRotate, new Action(() =>
                    {
                        int delta = (int)((double)angel * SettingsManager.PixelsInOneDegreeOfMouseRotate);
                        var centerPoint = new Point(MouseDeltaToPixels(SettingsManager.ScreenWidth / 2, true), MouseDeltaToPixels(SettingsManager.ScreenHeight / 2, false));
                        mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE, (uint)centerPoint.X, (uint)centerPoint.Y, 0, (UIntPtr)0);
                        Thread.Sleep(50);
                        mouse_event(MOUSEEVENTF_MOVE, (uint)delta, (uint)0, 0, (UIntPtr)0);
                        Thread.Sleep(50);
                        mouse_event(MOUSEEVENTF_RIGHTUP | MOUSEEVENTF_MOVE, (uint)0, (uint)0, 0, (UIntPtr)0);
                        Thread.Sleep(inputQueueAddDelay);
                    }))
                );
                if (outerProcWaitPress)
                {
                    while (InputActionsBuffer.GetCopy().Any(a => a.InputId == inputId)) Thread.Sleep(20);
                }
                Thread.Sleep(outerProcAddDelay);
            }
        }

        #region Mouse wheel samples
        /*
        private void MouseWheelUp(uint delta) //one click = 120 
        {
            InputActionsBuffer.Add(new Action(() =>
            {
                mouse_event(MOUSEEVENTF_WHEEL, 0, 0, (int)delta, (UIntPtr)0);
            }));
        }

        private void MouseWheelDown(uint delta) //one click = 120 
        {
            InputActionsBuffer.Add(new Action(() =>
            {
                mouse_event(MOUSEEVENTF_WHEEL, 0, 0, -(int)delta, (UIntPtr)0);
            }));
        }
        */

        /*
        public void MaxZoomIn()
        {
            InputActionsBuffer.Add(new Action(() =>
            {
                MouseWheelUp(MaxMouseWheelDelta);
                MouseWheelDown(MaxZoomOffset);
            }));
        }

        public void MaxZoomOut()
        {
            InputActionsBuffer.Add(new Action(() =>
            {
                MouseWheelDown(MaxMouseWheelDelta);
            }));
        }
        */
        #endregion
    }

    //Item of input buffer
    public class InputData
    {
        public Guid InputId { get; set; } //Track id to check input is over
        public string Key { get; set; }
        public Action Action { get; set; }

        public InputData(Guid inputId, string key, Action action)
        {
            InputId = inputId;
            Key = key;
            Action = action;
        }
    }
}
