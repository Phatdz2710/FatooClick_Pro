using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KeyMouseTool;
using IniParse;
using System.Runtime.InteropServices;
using System.Globalization;
using System.IO;

namespace AUTOCLICK_PRO
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern int GetWindowTextA(IntPtr hWnd, StringBuilder lpString, int nMaxCount);


        MouseAutoClickMode mouseAutoClickMode = new MouseAutoClickMode();
        KeyboardAutoPressMode keyboardAutoPressMode = new KeyboardAutoPressMode();
        MultiAutoClickMode multiAutoClickMode = new MultiAutoClickMode();

        GlobalHotKey ChangeMouseHotKey;
        GlobalHotKey ChangeKeyboardHotKey;
        GlobalHotKey ChangeMultipleHotKey;
        GlobalHotKey SelectForegroundWindow;

        GlobalHotKey Pointer = null;
        Button btnPointer = null; 

        private const int MOUSE_HOTKEY_ID = 9000;
        private const int KEYBOARD_HOTKEY_ID = 9001;
        private const int MULTI_HOTKEY_ID = 9002;
        private const int FOREGROUND_HOTKEY_ID = 9003;

        private const int WM_HOTKEY = 0x0312;

        private bool isKeyboardRunning = false;
        private bool isMouseRunning = false;
        private bool isMultiModeRunning = false;

        private Dictionary<HOTKEY, uint> HotKeyRegisted = new Dictionary<HOTKEY, uint>();

        bool isHiding = false;

        IniFile iniReader;
        ChangeHotKeyPopup popup;

        public Form1()
        {
            InitializeComponent();

            iniReader = new IniFile(AppDomain.CurrentDomain.BaseDirectory + "config.ini");
            popup = new ChangeHotKeyPopup();
            popup.Closed += Popup_Closed;
            SelectForegroundWindow = new GlobalHotKey(this.Handle, HOTKEY.F11, FOREGROUND_HOTKEY_ID);

            DataGridViewTextBoxColumn deviceColumn = new DataGridViewTextBoxColumn();
            deviceColumn.HeaderText = "Device";
            deviceColumn.Name = "deviceColumn"; 
            deviceColumn.Width = 50;
            dgvScript.Columns.Add(deviceColumn);

            // Tạo và thêm cột "Delay"
            DataGridViewTextBoxColumn delayColumn = new DataGridViewTextBoxColumn();
            delayColumn.HeaderText = "Delay";
            delayColumn.Name = "delayColumn";
            delayColumn.Width = 50;
            dgvScript.Columns.Add(delayColumn);

            // Tạo và thêm cột "Mouse Mode"
            DataGridViewTextBoxColumn mouseModeColumn = new DataGridViewTextBoxColumn();
            mouseModeColumn.HeaderText = "Mouse Mode";
            mouseModeColumn.Name = "mouseModeColumn";
            mouseModeColumn.Width = 100;
            dgvScript.Columns.Add(mouseModeColumn);

            // Tạo và thêm cột "Keyboard Mode"
            DataGridViewTextBoxColumn keyboardModeColumn = new DataGridViewTextBoxColumn();
            keyboardModeColumn.HeaderText = "Keyboard Mode";
            keyboardModeColumn.Name = "keyboardModeColumn";
            keyboardModeColumn.Width = 107;
            dgvScript.Columns.Add(keyboardModeColumn);

            // Tạo và thêm cột "Key"
            DataGridViewTextBoxColumn keyColumn = new DataGridViewTextBoxColumn();
            keyColumn.HeaderText = "Key";
            keyColumn.Name = "keyColumn";
            keyColumn.Width = 30;
            dgvScript.Columns.Add(keyColumn);

            // Tạo và thêm cột "Position"
            DataGridViewTextBoxColumn positionColumn = new DataGridViewTextBoxColumn();
            positionColumn.HeaderText = "Position";
            positionColumn.Name = "positionColumn";
            positionColumn.Width = 100;
            dgvScript.Columns.Add(positionColumn);

            dgvScript.RowHeadersVisible = false;
            dgvScript.AllowUserToAddRows = false;
        }


        private void btnMouseHotKey_Click(object sender, EventArgs e)
        {
            Pointer = ChangeMouseHotKey;
            btnPointer = (Button)sender;
            popup.ShowDialog();
        }

        private void btnHotKeyKeyboard_Click(object sender, EventArgs e)
        {
            Pointer = ChangeKeyboardHotKey;
            btnPointer = (Button)sender;
            popup.ShowDialog();
        }

        private void Popup_Closed(object sender, EventArgs e)
        {
            Pointer.ChangeHotKey(popup.GetKey);
            btnPointer.Text = Enum.GetName(popup.GetKey.GetType(), popup.GetKey);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == WM_HOTKEY)
            {
                if (m.WParam.ToInt32() == MOUSE_HOTKEY_ID)
                {
                    if (isMouseRunning)
                    {
                        mouseAutoClickMode.Stop();
                        isMouseRunning = false;
                    }
                    else
                    {
                        SetupForMouseClick();
                        mouseAutoClickMode.Run();
                        isMouseRunning = true;
                    }
                }
                else if (m.WParam.ToInt32() == KEYBOARD_HOTKEY_ID)
                {
                    if (isKeyboardRunning)
                    {
                        keyboardAutoPressMode.Stop();
                        isKeyboardRunning = false;
                    }
                    else
                    {
                        SetupForKeyboardPress();
                        keyboardAutoPressMode.Run();
                        isKeyboardRunning = true;
                    }
                }
                else if (m.WParam.ToInt32() == MULTI_HOTKEY_ID)
                {
                    if (isMultiModeRunning)
                    {
                        multiAutoClickMode.Stop();
                        isMultiModeRunning = false;
                    }
                    else
                    {
                        SetupForMultiMode();
                        multiAutoClickMode.Run();
                        isMultiModeRunning = true;
                    }
                }
                else if (m.WParam.ToInt32() == FOREGROUND_HOTKEY_ID)
                {
                    FindForegroundWindow();
                }
            }
        }

        private void FindForegroundWindow()
        {
            IntPtr handleForegroundWindow = GetForegroundWindow();
            StringBuilder text = new StringBuilder(256);
            GetWindowTextA(handleForegroundWindow, text, 256);
            txbFocusHandleMouse.Text = text.ToString();
        }

        private void SetupForMouseClick()
        {
            txbClickRepeat.Text = txbClickRepeat.Text == String.Empty ? "0" : txbClickRepeat.Text;
            txbMsMouse.Text = txbMsMouse.Text == String.Empty ? "1000" : txbMsMouse.Text;
            txbSecsMouse.Text = txbSecsMouse.Text == String.Empty ? "0" : txbSecsMouse.Text;
            txbMinsMouse.Text = txbMinsMouse.Text == String.Empty ? "0" : txbMinsMouse.Text;
            txbPosX.Text = txbPosX.Text == String.Empty ? "0" : txbPosX.Text;
            txbPosY.Text = txbPosY.Text == String.Empty ? "0" : txbPosY.Text;

            mouseAutoClickMode.SetRepeat(Convert.ToInt32(txbClickRepeat.Text));
            mouseAutoClickMode.SetTimeInterval(Convert.ToInt32(txbMinsMouse.Text) * 60000 + Convert.ToInt32(txbSecsMouse.Text) * 60000 * 60 + Convert.ToInt32(txbMsMouse.Text));
            mouseAutoClickMode.TurnOnOffLockPosition(cbxLockPos.Checked);
            if (cbxLockAtCursor.Checked)
            {
                mouseAutoClickMode.SetLockPosition(Win32Point.NULL);
            }
            else
            {
                Win32Point pos = new Win32Point()
                {
                    X = Convert.ToInt32(txbPosX.Text),
                    Y = Convert.ToInt32(txbPosY.Text)
                };
                mouseAutoClickMode.SetLockPosition(pos);
            }

            if (cbxFocusHandleMouse.Checked)
            {
                IntPtr handle = FindWindow(null, txbFocusHandleMouse.Text);
                mouseAutoClickMode.SetFocusHandle(handle);
            }
            else
            {
                mouseAutoClickMode.SetFocusHandle(IntPtr.Zero);
            }
        }

        private void SetupForKeyboardPress()
        {
            if (txbKey.Text == String.Empty) txbKey.Text = "A";
            keyboardAutoPressMode.SetKey(HotKeyConvert.StringToHotkey(txbKey.Text));

            txbPressRepeat.Text = txbPressRepeat.Text == String.Empty ? "0" : txbPressRepeat.Text;
            txbMsKb.Text = txbMsKb.Text == String.Empty ? "1000" : txbMsKb.Text;
            txbSecsKb.Text = txbSecsKb.Text == String.Empty ? "0" : txbSecsKb.Text;
            txbMinsKb.Text = txbMinsKb.Text == String.Empty ? "0" : txbMinsKb.Text;

            if (cbxHold.Checked)
            {
                keyboardAutoPressMode.SetMode(EKeyboardMode.HOLD);
                keyboardAutoPressMode.SetPressRepeat(0);
                keyboardAutoPressMode.SetTimeInterval(100000);
            }
            else
            {
                keyboardAutoPressMode.SetMode(EKeyboardMode.CLICK);
                keyboardAutoPressMode.SetPressRepeat(Convert.ToInt32(txbPressRepeat.Text));
            }

            keyboardAutoPressMode.SetTimeInterval(Convert.ToInt32(txbMinsKb.Text) * 60000 + Convert.ToInt32(txbSecsKb.Text) * 60000 * 60 + Convert.ToInt32(txbMsKb.Text));
        }

        private void SetupForMultiMode()
        {
            if (cbxFocusHandleMulti.Checked)
            {
                IntPtr handle = FindWindow(null, txbFocusHandleMouse.Text);
                multiAutoClickMode.SetFocusHandle(handle);
            }
            else
            {
                mouseAutoClickMode.SetFocusHandle(IntPtr.Zero);
            }

            if (cbxLoop.Checked)
            {
                multiAutoClickMode.SetLoop(true);
            }
            else
            {
                multiAutoClickMode.SetLoop(false);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            string mouseHotKey = iniReader.GetSection("MouseAutoClick").GetValue("MouseHotKey");
            string mouseClickRepeat = iniReader.GetSection("MouseAutoClick").GetValue("ClickRepeat");
            string lockPosition = iniReader.GetSection("MouseAutoClick").GetValue("LockPosition");
            string lockX = iniReader.GetSection("MouseAutoClick").GetValue("LockX");
            string lockY = iniReader.GetSection("MouseAutoClick").GetValue("LockY");
            string lockAtCursor = iniReader.GetSection("MouseAutoClick").GetValue("LockAtCursor");
            string focusHandle = iniReader.GetSection("MouseAutoClick").GetValue("FocusHandle");
            string mouseMs = iniReader.GetSection("MouseAutoClick").GetValue("MouseMs");
            string mouseSecs = iniReader.GetSection("MouseAutoClick").GetValue("MouseSecs");
            string mouseMins = iniReader.GetSection("MouseAutoClick").GetValue("MouseMins");

            HOTKEY mouseStr = GetValidHotKey(mouseHotKey);
            ChangeMouseHotKey = new GlobalHotKey(this.Handle, mouseStr, MOUSE_HOTKEY_ID);

            btnMouseHotKey.Text = Enum.GetName(mouseStr.GetType(), mouseStr);
            txbClickRepeat.Text = mouseClickRepeat == String.Empty ? "0" : mouseClickRepeat;
            txbMinsMouse.Text = mouseMins == String.Empty ? "0" : mouseMins;
            txbSecsMouse.Text = mouseSecs == String.Empty ? "0" : mouseSecs;
            txbMsMouse.Text = mouseMs == String.Empty ? "1000" : mouseMs;

            if ((cbxLockPos.Checked = Convert.ToBoolean(lockPosition)))
            {
                cbxLockAtCursor.Enabled = true;
                if ((cbxLockAtCursor.Checked = Convert.ToBoolean(lockAtCursor)))
                {
                    txbPosX.Enabled = false;
                    txbPosY.Enabled = false;
                }
                else
                {
                    txbPosX.Enabled = true;
                    txbPosY.Enabled = true;
                }
            }
            else
            {
                cbxLockAtCursor.Enabled = false;
                txbPosX.Enabled = false;
                txbPosY.Enabled = false;
            }

            txbPosX.Text = lockX;
            txbPosY.Text = lockY;
            cbxFocusHandleMouse.Checked = Convert.ToBoolean(focusHandle);


            string keyboardHotKey = iniReader.GetSection("KeyboardAutoClick").GetValue("KeyboardHotKey");
            string pressRepeat = iniReader.GetSection("KeyboardAutoClick").GetValue("PressRepeat");
            string Key = iniReader.GetSection("KeyboardAutoClick").GetValue("Key");
            string keyMins = iniReader.GetSection("KeyboardAutoClick").GetValue("KeyMins");
            string keySecs = iniReader.GetSection("KeyboardAutoClick").GetValue("KeySecs");
            string keyMs = iniReader.GetSection("KeyboardAutoClick").GetValue("KeyMs");
            string keyHold = iniReader.GetSection("KeyboardAutoClick").GetValue("Hold");

            HOTKEY keyboardStr = GetValidHotKey(keyboardHotKey);
            ChangeKeyboardHotKey = new GlobalHotKey(this.Handle, keyboardStr, KEYBOARD_HOTKEY_ID);
            btnHotKeyKeyboard.Text = Enum.GetName(keyboardStr.GetType(), keyboardStr);
            txbPressRepeat.Text = pressRepeat == String.Empty ? "0" : pressRepeat;
            txbKey.Text = Key == String.Empty ? "0" : Key;
            txbMinsKb.Text = keyMins == String.Empty ? "0" : keyMins;
            txbSecsKb.Text = keySecs == String.Empty ? "0" : keySecs;
            txbMsKb.Text = keyMs == String.Empty ? "1000" : keyMs;
            cbxHold.Checked = Convert.ToBoolean(keyHold);


            string multiHotKey = iniReader.GetSection("MultiAutoClick").GetValue("MultiHotKey");
            string focusHandleMulti = iniReader.GetSection("MultiAutoClick").GetValue("FocusHandle");

            HOTKEY multiStr = GetValidHotKey(multiHotKey);
            btnHotKeyMulti.Text = Enum.GetName(multiStr.GetType(), multiStr);

            cbxFocusHandleMulti.Checked = Convert.ToBoolean(focusHandleMulti);
            ChangeMultipleHotKey = new GlobalHotKey(this.Handle, multiStr, MULTI_HOTKEY_ID);
        }

        private void MainForm_Closing(object sender, EventArgs e)
        {
            iniReader.ChangeValueKeyOfSection("MouseAutoClick", "MouseHotKey", btnMouseHotKey.Text);
            iniReader.ChangeValueKeyOfSection("MouseAutoClick", "ClickRepeat", txbClickRepeat.Text);
            iniReader.ChangeValueKeyOfSection("MouseAutoClick", "LockPosition", cbxLockPos.Checked.ToString());
            iniReader.ChangeValueKeyOfSection("MouseAutoClick", "LockX", txbPosX.Text);
            iniReader.ChangeValueKeyOfSection("MouseAutoClick", "LockY", txbPosY.Text);
            iniReader.ChangeValueKeyOfSection("MouseAutoClick", "LockAtCursor", cbxLockAtCursor.Checked.ToString());
            iniReader.ChangeValueKeyOfSection("MouseAutoClick", "FocusHandle", cbxFocusHandleMouse.Checked.ToString());
            iniReader.ChangeValueKeyOfSection("MouseAutoClick", "MouseMs", txbMsMouse.Text);
            iniReader.ChangeValueKeyOfSection("MouseAutoClick", "MouseSecs", txbSecsMouse.Text);
            iniReader.ChangeValueKeyOfSection("MouseAutoClick", "MouseMins", txbMinsMouse.Text);

            iniReader.ChangeValueKeyOfSection("KeyboardAutoClick", "KeyboardHotKey", btnHotKeyKeyboard.Text);
            iniReader.ChangeValueKeyOfSection("KeyboardAutoClick", "PressRepeat", txbPressRepeat.Text);
            iniReader.ChangeValueKeyOfSection("KeyboardAutoClick", "Key", txbKey.Text);
            iniReader.ChangeValueKeyOfSection("KeyboardAutoClick", "KeyMins", txbMinsKb.Text);
            iniReader.ChangeValueKeyOfSection("KeyboardAutoClick", "KeySecs", txbSecsKb.Text);
            iniReader.ChangeValueKeyOfSection("KeyboardAutoClick", "KeyMs", txbMsKb.Text);
            iniReader.ChangeValueKeyOfSection("KeyboardAutoClick", "Hold", cbxHold.Checked.ToString());

            iniReader.ChangeValueKeyOfSection("MultiAutoClick", "MultiHotKey", btnHotKeyMulti.Text);
            iniReader.ChangeValueKeyOfSection("MultiAutoClick", "FocusHandle", cbxFocusHandleMulti.Checked.ToString());
            iniReader.Save();
        }

        private HOTKEY GetValidHotKey(string str)
        {
            try
            {
                HOTKEY res = (HOTKEY)Enum.Parse(typeof(HOTKEY), str);
                if (HotKeyRegisted.ContainsKey(res))
                {
                    while (HotKeyRegisted.ContainsKey(res))
                    {
                        res++;
                    }
                    HotKeyRegisted[res] = (uint)res;
                    return res;
                }
                HotKeyRegisted[res] = (uint)res;
                return res;
            }
            catch (Exception)
            {
                HOTKEY HotKey = HOTKEY.F1;
                while (HotKeyRegisted.ContainsKey(HotKey))
                {
                    HotKey++;
                }
                HotKeyRegisted[HotKey] = (uint)HotKey;
                return HotKey;
            }
        }

        private void cbxLockPos_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            if (checkBox.Checked)
            {
                cbxLockAtCursor.Enabled = true;
                if (cbxLockAtCursor.Checked)
                {
                    txbPosX.Enabled = false;
                    txbPosY.Enabled = false;
                }
                else
                {
                    txbPosX.Enabled = true;
                    txbPosY.Enabled = true;
                }
            }
            else
            {
                cbxLockAtCursor.Enabled = false;
                txbPosX.Enabled = false;
                txbPosY.Enabled = false;
            }
        }

        private void cbxLockAtCursor_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cbx = (CheckBox)sender;
            if (cbx.Checked)
            {
                txbPosX.Enabled = false;
                txbPosY.Enabled = false;
            }
            else
            {
                txbPosX.Enabled = true;
                txbPosY.Enabled = true;
            }
        }

        private void cbxHold_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cbx = (CheckBox)sender;
            if (cbx.Checked)
            {
                txbPressRepeat.Enabled = false;
            }
            else
            {
                txbPressRepeat.Enabled = true;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select Script File";

            openFileDialog.Filter = "Script file (*.srp)|*.srp";
            
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedFilePath = openFileDialog.FileName;
                ReadScript(selectedFilePath);
            }
        }

        private void ReadScript(string pathScript)
        {
            using (StreamReader reader = new StreamReader(pathScript))
            {
                string line;

                while((line = reader.ReadLine()) != null)
                {
                    string[] words = line.Split(' ');
                    DeviceAction newAction = new DeviceAction();

                    foreach (string word in words)
                    {
                        if (words[0] == "0") newAction.IsMouse = false; // 0 : keyboard, 1: mouse
                        else newAction.IsMouse = true;

                        newAction.WaitTime = int.Parse(words[1]);
                        newAction.MouseMode = (EMouseMode)int.Parse(words[2]);
                        newAction.keyboardMode = (EKeyboardMode)int.Parse(words[3]);
                        newAction.Key = (HOTKEY)Enum.Parse(typeof(HOTKEY), words[4], true);
                        newAction.Pos = new Win32Point(int.Parse(words[5]), int.Parse(words[6]));
                    }
                   

                    dgvScript.Rows.Add(
                            words[0] == "0" ? "Keyboard" : "Mouse",
                            newAction.WaitTime,
                            newAction.MouseMode,
                            newAction.keyboardMode,
                            HotKeyConvert.HotkeyToString(newAction.Key),
                            newAction.Pos.X.ToString() + " , " + newAction.Pos.Y.ToString()
                        );

                    this.multiAutoClickMode.AddAction(newAction);
                }
            }
        }
    }

}
