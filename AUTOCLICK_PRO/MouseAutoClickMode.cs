using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KeyMouseTool;
using Microsoft.Win32.SafeHandles;

namespace AUTOCLICK_PRO
{
    internal class MouseAutoClickMode
    {

        MouseAutoClick mouseAutoClick;

        public MouseAutoClickMode()
        {
            mouseAutoClick = new MouseAutoClick();
        }

        public void SetEventHandlerStopped(EventHandler handler)
        {
            mouseAutoClick.Stopped = handler;
        }

        public void Run()
        {
            mouseAutoClick.Run();
        }

        public void Stop()
        {
            mouseAutoClick.Stop();
        }

        public void SetFocusHandle(IntPtr hwnd)
        {
            mouseAutoClick.SetFocusHandle(hwnd);
        }

        public void TurnOnOffLockPosition(bool isLock)
        {
            mouseAutoClick.TurnOnOffLockPos(isLock);
        }

        public void SetLockPosition(Win32Point pos)
        {
            mouseAutoClick.SetLockPos(pos);
        }

        public void SetTimeInterval(int timeInterval)
        {
            mouseAutoClick.SetDelayTime(timeInterval);
        }

        public void SetRepeat(int repeat)
        {
            mouseAutoClick.SetRepeatTime(repeat);
        }

        public void SetMouseClickMode(EMouseMode eMouseMode)
        {
            return;
        }

    }
}
