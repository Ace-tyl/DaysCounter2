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
            new("日本語 (日本)", "ja", FlowDirection.LeftToRight),
            new("Svenska (Sverige)", "sv-SE", FlowDirection.LeftToRight),
        ];
    }
}
