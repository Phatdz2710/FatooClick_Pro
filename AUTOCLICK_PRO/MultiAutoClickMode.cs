using KeyMouseTool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AUTOCLICK_PRO
{
    public class MultiAutoClickMode
    {
        private MultipleAutoClick multiTool;
        private DataGridView _dataGridView;
        public MultiAutoClickMode()
        {
            multiTool = new MultipleAutoClick();
        }

        public void SetFocusHandle(IntPtr hwnd)
        {
            multiTool.SetFocusHandle(hwnd);
        }
        public void AddAction(DeviceAction action)
        {
            this.multiTool.AddAction(action);
        }

        public void ClearActions()
        {
            this.multiTool.ClearActions();
        }

        public void Run()
        {
            this.multiTool.Run();
        }

        public void Stop()
        {
            this.multiTool.Stop();
        }

        public void SetLoop(bool isLoop)
        {
            this.multiTool.SetLoop(isLoop);
        }

        public void RunRecord()
        {
            this.multiTool.RunRecord();
        }
        
        public void StopRecord()
        {
            this.multiTool.StopRecord();
        }

        public List<DeviceAction> GetListActions()
        {
            return this.multiTool.Actions;
        }
    }
}
