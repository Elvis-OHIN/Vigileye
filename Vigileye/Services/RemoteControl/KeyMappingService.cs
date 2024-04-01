using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsInput.Native;

namespace Vigileye.Services.RemoteControl
{
    public class KeyMappingService
    {
        private readonly Dictionary<string, VirtualKeyCode> keyMappings = new Dictionary<string, VirtualKeyCode>()
        {
            // Initialisation du dictionnaire avec les mappages clé à VirtualKeyCode
            {"a", VirtualKeyCode.VK_A},
            {"b", VirtualKeyCode.VK_B},
            {"c", VirtualKeyCode.VK_C},
            {"d", VirtualKeyCode.VK_D},
            {"e", VirtualKeyCode.VK_E},
            {"f", VirtualKeyCode.VK_F},
            {"g", VirtualKeyCode.VK_G},
            {"h", VirtualKeyCode.VK_H},
            {"i", VirtualKeyCode.VK_I},
            {"j", VirtualKeyCode.VK_J},
            {"k", VirtualKeyCode.VK_K},
            {"l", VirtualKeyCode.VK_L},
            {"m", VirtualKeyCode.VK_M},
            {"n", VirtualKeyCode.VK_N},
            {"o", VirtualKeyCode.VK_O},
            {"p", VirtualKeyCode.VK_P},
            {"q", VirtualKeyCode.VK_Q},
            {"r", VirtualKeyCode.VK_R},
            {"s", VirtualKeyCode.VK_S},
            {"t", VirtualKeyCode.VK_T},
            {"u", VirtualKeyCode.VK_U},
            {"v", VirtualKeyCode.VK_V},
            {"w", VirtualKeyCode.VK_W},
            {"x", VirtualKeyCode.VK_X},
            {"y", VirtualKeyCode.VK_Y},
            {"z", VirtualKeyCode.VK_Z},

            {"ArrowUp", VirtualKeyCode.UP},
            {"ArrowDown", VirtualKeyCode.DOWN},
            {"ArrowLeft", VirtualKeyCode.LEFT},
            {"ArrowRight", VirtualKeyCode.RIGHT},

            {"Escape", VirtualKeyCode.ESCAPE},
            {"Enter", VirtualKeyCode.RETURN},
            {"Tab", VirtualKeyCode.TAB},
            {"Space", VirtualKeyCode.SPACE},
            {"Backspace", VirtualKeyCode.BACK},
            {"Delete", VirtualKeyCode.DELETE},
            {"Insert", VirtualKeyCode.INSERT},
            {"Home", VirtualKeyCode.HOME},
            {"End", VirtualKeyCode.END},
            {"PageUp", VirtualKeyCode.PRIOR},
            {"PageDown", VirtualKeyCode.NEXT},
            {"LeftShift", VirtualKeyCode.LSHIFT},
            {"RightShift", VirtualKeyCode.RSHIFT},
            {"LeftControl", VirtualKeyCode.LCONTROL},
            {"RightControl", VirtualKeyCode.RCONTROL},
            {"LeftMenu", VirtualKeyCode.LMENU}, 
            {"RightMenu", VirtualKeyCode.RMENU}, 
    
        };

        public VirtualKeyCode ConvertToVirtualKeyCode(string key)
        {
            if (keyMappings.TryGetValue(key, out VirtualKeyCode keyCode))
            {
                return keyCode;
            }
            else
            {
                return VirtualKeyCode.OEM_1;
            }

        }
    }
}
