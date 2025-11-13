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
    public class LanguageData
    {
        public string name { get; set; }
        public string id { get; set; }

        public LanguageData(string name, string id)
        {
            this.name = name;
            this.id = id;
        }
    }

    public class LanguageManager
    {
        public static List<LanguageData> languages { get; set; } = new List<LanguageData>
        {
            new LanguageData("English (United States)", ""),
            new LanguageData("简体中文 (中国大陆)", "zh-CN"),
        };

        public static void SwitchLanguage(LanguageData language)
        {
        }
    }
}
