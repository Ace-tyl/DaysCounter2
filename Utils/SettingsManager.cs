using Avalonia.Media;

namespace DaysCounter2.Utils
{
    public class SettingsManager
    {
        public string languageId = "";

        public byte futureColor_a = 160, futureColor_r = 102, futureColor_g = 204, futureColor_b = 255;
        public byte pastColor_a = 160, pastColor_r = 238, pastColor_g = 0, pastColor_b = 0;
        public byte distantColor_a = 0, distantColor_r = 0, distantColor_g = 0, distantColor_b = 0;

        public string dateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        public int dateTimeCalendar = 0;

        public byte destinationShowingMode = 0;

        public string windowFont = "";

        public Color futureColor
        {
            get
            {
                return Color.FromArgb(futureColor_a, futureColor_r, futureColor_g, futureColor_b);
            }
            set
            {
                futureColor_a = value.A;
                futureColor_r = value.R;
                futureColor_g = value.G;
                futureColor_b = value.B;
            }
        }
        public Color pastColor
        {
            get
            {
                return Color.FromArgb(pastColor_a, pastColor_r, pastColor_g, pastColor_b);
            }
            set
            {
                pastColor_a = value.A;
                pastColor_r = value.R;
                pastColor_g = value.G;
                pastColor_b = value.B;
            }
        }
        public Color distantColor
        {
            get
            {
                return Color.FromArgb(distantColor_a, distantColor_r, distantColor_g, distantColor_b);
            }
            set
            {
                distantColor_a = value.A;
                distantColor_r = value.R;
                distantColor_g = value.G;
                distantColor_b = value.B;
            }
        }
    }
}
