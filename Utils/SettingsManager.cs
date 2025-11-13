using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media;

namespace DaysCounter2.Utils
{
    public class SettingsManager
    {
        public string languageId = "";
        public Color futureColor = Color.FromArgb(160, 102, 204, 255);
        public Color pastColor = Color.FromArgb(160, 238, 0, 0);
        public Color distantColor = Color.FromArgb(0, 0, 0, 0);
    }
}
