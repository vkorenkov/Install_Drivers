using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Install_Drivers.Models
{
    static class Extentions
    {
        /// <summary>
        /// (Расширение) Метод получения папок доступных моделей устройств
        /// </summary>
        /// <param name="devicesFolders"></param>
        /// <returns></returns>
        public static List<string> GetModelsFolders(this List<string> devicesFolders)
        {
            var temp = Directory.GetDirectories(File.ReadAllText("Resources/GeneralPath")).ToList();

            foreach(var t in temp)
            {
                devicesFolders.Add(t.Substring(t.LastIndexOf(@"\") + 1));
            }

            return devicesFolders;
        }

        /// <summary>
        /// Метод записи действий в лог файл
        /// </summary>
        /// <param name="logText"></param>
        public static void Log(this string logString, string logText)
        {
            logString = logText;

            File.AppendAllText("Install_log.txt", logString);
        }

        public static string RemoveText(this string allText)
        {
            allText = allText.Substring(allText.LastIndexOf(@"\") + 1);

            return allText;
        }
    }
}
