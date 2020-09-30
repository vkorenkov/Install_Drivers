using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Install_Drivers
{
    public class OpenFileFolder
    {
        /// <summary>
        /// Метод открытия диалогового окна выбора папки
        /// </summary>
        /// <returns></returns>
        public static string OpenFolder()
        {
            CommonOpenFileDialog cofd = new CommonOpenFileDialog();

            cofd.InitialDirectory = File.ReadAllText("Resources/GeneralPath");
            cofd.IsFolderPicker = true;

            if(cofd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                return cofd.FileName;
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Метод открытия диалогового окна выбора файла
        /// </summary>
        /// <returns></returns>
        public static string OpenFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.InitialDirectory = File.ReadAllText("Resources/GeneralPath");
            ofd.Filter = "Фалы установщиков (*.exe; *.msi)|*.exe; *.msi|Все файлы (*.*)|*.*";

            if (ofd.ShowDialog() == true)
            {
                return ofd.FileName;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
