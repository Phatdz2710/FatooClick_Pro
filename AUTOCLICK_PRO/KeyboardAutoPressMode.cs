using KeyMouseTool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace AUTOCLICK_PRO
{
    internal class KeyboardAutoPressMode
    {
        KeyboardAutoPress keyboard;
        public KeyboardAutoPressMode()
        {
            keyboard = new KeyboardAutoPress();
        }

        public void SetKey(HOTKEY key)
        {
            keyboard.SetKey(key);
        }

        public void SetPressRepeat(int repeat)
        {
            keyboard.SetRepeatTime(repeat);
        }

        public void SetTimeInterval(int timeInterval)
        {
            keyboard.SetDelayTime(timeInterval);
        }

        public void SetEventHandlerStopped(EventHandler handler)
        {
            keyboard.Stopped = handler;
        }

        public void Run()
        {
            keyboard.Run();
        }
        public void Stop()
        {
            keyboard.Stop();
        }

        public void SetMode(EKeyboardMode mode)
        {
            keyboard.SetMode(mode);
        }
    }
}
