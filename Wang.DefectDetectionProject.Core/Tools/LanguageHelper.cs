using System.Collections;
using System.Windows;

namespace Wang.DefectDetectionProject.Core.Tools
{
    public static class LanguageHelper
    {
        /// <summary>
        /// 当前全局语言
        /// </summary>
        public static string? AppCurrentLanguage { get; set; }

        /// <summary>
        /// 缓存的语言键值对
        /// </summary>
        public static Dictionary<string, string>? TranslationKeyValues { get; set; }

        /// <summary>
        /// 更改语言方法。
        /// 更改资源顺序，更改当前全局语言，更改缓存的语言键值对
        /// </summary>
        /// <param name="key"></param>
        public static void SetLanguage(string key)
        {
            if (key == null) return;
            // App.xaml中定义了多语言资源
            var resource = Application.Current.Resources.MergedDictionaries
                           .FirstOrDefault(t => t.Source != null && t.Source.OriginalString != null
                                           && t.Source.OriginalString.Contains(key));
            if (resource != null)
            {
                Application.Current.Resources.MergedDictionaries.Remove(resource);
            }

            // 添加到尾部的资源字典会被优先应用
            Application.Current.Resources.MergedDictionaries.Add(resource);

            Dictionary<string, string> keyValues = new Dictionary<string, string>();

            foreach (DictionaryEntry item in resource!)
            {
                keyValues.Add(item.Key.ToString()!, item.Value!.ToString()!);
            }

            AppCurrentLanguage = key;
            // 更新多语言功能键值对
            TranslationKeyValues = keyValues;

            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(key);
        }
    }
}
