using KeyMouseTool;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AUTOCLICK_PRO
{
    public partial class ChangeHotKeyPopup : Form
    {
        public HOTKEY GetKey { get; private set; }

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);

        public ChangeHotKeyPopup()
        {
            InitializeComponent();
            this.KeyDown += ChangeHotKeyPopup_KeyDown;
        }

        private void ChangeHotKeyPopup_KeyDown(object sender, KeyEventArgs e)
        {
            uint vk = (uint)e.KeyCode;
            GetKey = (HOTKEY)vk;
            this.Close();
        }
    }
}
