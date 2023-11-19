using EngineProject.Enums;
using EngineProject.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EngineProject.Helpers
{
    //Tracker for bot commands, one object for one hotkey
    public class KeyListener
    {
        #region Win api procs

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        #endregion

        //Listener parameters
        private Action OnKeyDown { get; set; }
        private string Key { get; set; }
        private bool IsHoldCtrl { get; set; }
        private bool IsRepeating { get; set; }
        private bool HasFired { get; set; }

        //Delegate for hotkey action on input
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private LowLevelKeyboardProc _proc { get; set; }
        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }
        private IntPtr HotkeyId = IntPtr.Zero; //Listener win api id

        //Constants
        private const int WH_KEYBOARD_LL = 13; //Type of Hook - Low Level Keyboard
        private const int WM_KEYDOWN = 0x0100; //Value passed on KeyDown
        private const int WM_KEYUP = 0x0101; //Value passed on KeyUp
        //Bool to use as a flag for control key
        //Global for all listeners to enable global hold
        private static bool CONTROL_DOWN = false;

        public KeyListener(Action onKeyDown, string key, bool isRepeating = true, bool isHoldCtrl = false)
        {
            _proc = HookCallback;

            OnKeyDown = onKeyDown;
            Key = key;
            IsHoldCtrl = isHoldCtrl;
            IsRepeating = isRepeating;

            HotkeyId = SetHook(_proc);
        }

        //Main input proc
        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                if (IsRepeating || !HasFired)
                {
                    if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)         //A Key was pressed down
                    {
                        int vkCode = Marshal.ReadInt32(lParam);             //Get the keycode
                        string theKey = ((Keys)vkCode).ToString();          //Name of the key
                        if (theKey.Contains(InputKeys.Ctrl) && IsHoldCtrl)  //If they pressed control
                        {
                            CONTROL_DOWN = true;                            //Flag control as down
                        }
                        else if ((CONTROL_DOWN || !IsHoldCtrl)
                            && theKey == Key)                               //If they held CTRL and pressed B
                        {
                            OnKeyDown();
                            HasFired = true;
                        }
                        /*
                        else if (theKey == InputKeys.Esc)                   //If they press escape
                        {
                            UnhookWindowsHookEx(HotkeyId);                  //Release our hook
                        }
                        */
                    }
                    else if (nCode >= 0 && wParam == (IntPtr)WM_KEYUP)      //KeyUP
                    {
                        int vkCode = Marshal.ReadInt32(lParam);             //Get Keycode
                        string theKey = ((Keys)vkCode).ToString();          //Get Key name
                        if (theKey.Contains(InputKeys.Ctrl) && IsHoldCtrl)  //If they let go of control
                        {
                            CONTROL_DOWN = false;                           //Unflag control
                        }
                    }

                }
                return CallNextHookEx(HotkeyId, nCode, wParam, lParam);     //Call the next hook
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, "Key listener error");
                throw ex;
            }
        }

        public void StopListening()
        {
            try
            {
                if (HotkeyId != IntPtr.Zero) UnhookWindowsHookEx(HotkeyId);
                HotkeyId = IntPtr.Zero;
            }
            catch { }
        }
    }
}
