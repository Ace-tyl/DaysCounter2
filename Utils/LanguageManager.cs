using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using HarfBuzzSharp;

namespace DaysCounter2.Utils
{
    public class LanguageData(string name, string id)
    {
        public string name { get; set; } = name;
        public string id { get; set; } = id;
    }

    public class LanguageManager
    {
        public static List<LanguageData> languages { get; set; } =
        [
            new("English (United States)", ""),
            new("简体中文 (中国大陆)", "zh-CN"),
            new("日本語 (日本)", "ja"),
            new("Svenska (Sverige)", "sv-SE"),
        ];
    }
}
