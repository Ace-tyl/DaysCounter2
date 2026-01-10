using System.Collections.Generic;
using Avalonia.Media;

namespace DaysCounter2.Utils
{
    public class LanguageData(string name, string id, FlowDirection flowDirection)
    {
        public string name { get; set; } = name;
        public string id { get; set; } = id;
        public FlowDirection flowDirection { get; set; } = flowDirection;
    }

    public class LanguageManager
    {
        public static List<LanguageData> languages { get; set; } =
        [
            new("English (United States)", "", FlowDirection.LeftToRight),
            new("简体中文 (中国大陆)", "zh-CN", FlowDirection.LeftToRight),
            new("العربية (المملكة العربية السعودية)", "ar", FlowDirection.RightToLeft),
            new("日本語 (日本)", "ja", FlowDirection.LeftToRight),
            new("한국어 (대한민국)", "ko", FlowDirection.LeftToRight),
            new("Русский (Россия)", "ru", FlowDirection.LeftToRight),
            new("Svenska (Sverige)", "sv-SE", FlowDirection.LeftToRight),
            new("繁體中文 (香港特別行政區)", "zh-HK", FlowDirection.LeftToRight),
            new("繁體中文 (中國台灣)", "zh-TW", FlowDirection.LeftToRight)
        ];
    }
}
