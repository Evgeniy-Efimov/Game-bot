using EngineProject.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineProject.Helpers
{
    //Helps update form items from other thread
    public static class UIHelper
    {
        delegate void SetTextboxTextCallBack(Form form, TextBox textbox, string text);
        public static void SetTextboxText(Form form, TextBox textbox, string text)
        {
            try
            {
                if (textbox.InvokeRequired)
                {
                    SetTextboxTextCallBack callBackDelegate = new SetTextboxTextCallBack(SetTextboxText);
                    form.Invoke(callBackDelegate, new object[] { form, textbox, text });
                }
                else
                {
                    textbox.Text = text;
                }
            }
            catch { }
        }

        delegate void SetTextboxEnabledCallBack(Form form, TextBox textbox, bool enabled);
        public static void SetTextboxEnabled(Form form, TextBox textbox, bool enabled)
        {
            try
            {
                if (textbox.InvokeRequired)
                {
                    SetTextboxEnabledCallBack callBackDelegate = new SetTextboxEnabledCallBack(SetTextboxEnabled);
                    form.Invoke(callBackDelegate, new object[] { form, textbox, enabled });
                }
                else
                {
                    textbox.Enabled = enabled;
                }
            }
            catch { }
        }

        delegate void SetLabelEnabledCallBack(Form form, Label label, bool enabled);
        public static void SetLabelEnabled(Form form, Label label, bool enabled)
        {
            try
            {
                if (label.InvokeRequired)
                {
                    SetLabelEnabledCallBack callBackDelegate = new SetLabelEnabledCallBack(SetLabelEnabled);
                    form.Invoke(callBackDelegate, new object[] { form, label, enabled });
                }
                else
                {
                    label.Enabled = enabled;
                }
            }
            catch { }
        }

        delegate void SetCheckboxCheckedCallBack(Form form, CheckBox checkBox, bool isChecked);
        public static void SetCheckboxChecked(Form form, CheckBox checkBox, bool isChecked)
        {
            try
            {
                if (checkBox.InvokeRequired)
                {
                    SetCheckboxCheckedCallBack callBackDelegate = new SetCheckboxCheckedCallBack(SetCheckboxChecked);
                    form.Invoke(callBackDelegate, new object[] { form, checkBox, isChecked });
                }
                else
                {
                    checkBox.Checked = isChecked;
                }
            }
            catch { }
        }

        delegate void SetCheckboxEnabledCallBack(Form form, CheckBox checkBox, bool enabled);
        public static void SetCheckboxEnabled(Form form, CheckBox checkBox, bool enabled)
        {
            try
            {
                if (checkBox.InvokeRequired)
                {
                    SetCheckboxEnabledCallBack callBackDelegate = new SetCheckboxEnabledCallBack(SetCheckboxEnabled);
                    form.Invoke(callBackDelegate, new object[] { form, checkBox, enabled });
                }
                else
                {
                    checkBox.Enabled = enabled;
                }
            }
            catch { }
        }

        delegate void SetLabelTextCallBack(Form form, Label label, string text);
        public static void SetLabelText(Form form, Label label, string text)
        {
            try
            {
                if (label.InvokeRequired)
                {
                    SetLabelTextCallBack callBackDelegate = new SetLabelTextCallBack(SetLabelText);
                    form.Invoke(callBackDelegate, new object[] { form, label, text });
                }
                else
                {
                    label.Text = text;
                }
            }
            catch { }
        }

        delegate void SetButtonEnabledCallBack(Form form, Button button, bool enabled);
        public static void SetButtonEnabled(Form form, Button button, bool enabled)
        {
            try
            {
                if (button.InvokeRequired)
                {
                    SetButtonEnabledCallBack callBackDelegate = new SetButtonEnabledCallBack(SetButtonEnabled);
                    form.Invoke(callBackDelegate, new object[] { form, button, enabled });
                }
                else
                {
                    button.Enabled = enabled;
                }
            }
            catch { }
        }

        delegate void SetButtonTextCallBack(Form form, Button button, string text);
        public static void SetButtonText(Form form, Button button, string text)
        {
            try
            {
                if (button.InvokeRequired)
                {
                    SetButtonTextCallBack callBackDelegate = new SetButtonTextCallBack(SetButtonText);
                    form.Invoke(callBackDelegate, new object[] { form, button, text });
                }
                else
                {
                    button.Text = text;
                }
            }
            catch { }
        }
    }
}
