using KeyMouseTool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AUTOCLICK_PRO
{
    public class MultiAutoClickMode
    {
        private MultipleAutoClick multiTool;
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
    }
}
