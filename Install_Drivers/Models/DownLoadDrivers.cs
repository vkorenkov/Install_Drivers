using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Install_Drivers.Models
{
    class DownLoadDrivers
    {
        public delegate void SizeDelegate(long max, long value);
        public event SizeDelegate SizeDelegateEvent;

        private string log;
        private string newPath;
        private long size;
        private long localSize;
        public bool abortDownload;

        public List<Driver> DownloadDrivers(List<Driver> DriversPath)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                Application.Current.Dispatcher.Invoke(() => newPath = OpenFileFolder.OpenFolder() + @"\temp");

                Directory.CreateDirectory(newPath);

                GetSize(DriversPath);

                foreach (Driver drvPath in DriversPath)
                {
                    Directory.CreateDirectory($@"{newPath}\{drvPath.DriverPath.RemoveText()}");

                    log.Log($@"{DateTime.Now} Скачивание: {newPath}\{drvPath.DriverPath.RemoveText()} Построение дерева каталогов" + "\n");

                    if (!CreateFolderTree(drvPath.DriverPath, $@"{newPath}\{drvPath.DriverPath.RemoveText()}"))
                    {
                        drvPath.DriverPath = $@"{newPath}\{drvPath.DriverPath.RemoveText()}";
                    }
                    else
                    {
                        DeleteDowloadFiles();

                        break;
                    }

                    log.Log($@"{DateTime.Now} Скачивание: {newPath}\{drvPath.DriverPath.RemoveText()} Построение дерева каталогов завершено" + "\n");
                }

                if (!abortDownload)
                {
                    log.Log($"{DateTime.Now} Скачивание: Завершено\n");
                }
                else
                {
                    log.Log($"{DateTime.Now} Скачивание: отменено\n");
                }

                return DriversPath;
            }
            else
            {
                MessageBox.Show("Отсутствует подключение");

                return DriversPath;
            }
        }

        private bool CreateFolderTree(string path, string localPath)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(path);

            foreach (DirectoryInfo t in dirInfo.GetDirectories())
            {
                var test = $@"{localPath}\{t.Name}";

                Directory.CreateDirectory(test);

                CreateFolderTree(t.FullName, $@"{localPath}\{t.Name}");
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(newPath);

            foreach (var file in Directory.GetFiles(path))
            {
                if (!abortDownload)
                {
                    log.Log($"{DateTime.Now} Скачивание: {path} Копирование файлов\n");

                    string temp = $"{file.RemoveText()}";

                    File.Copy(file, $@"{localPath}\{temp}", true);

                    SizeDelegateEvent?.Invoke(size, ++localSize);
                }
                else
                {
                    return true;
                }
            }

            return abortDownload;
        }

        private void GetSize(List<Driver> DriversPath)
        {
            foreach (var r in DriversPath)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(r.DriverPath);

                size += directoryInfo.GetFiles("*", SearchOption.AllDirectories).Count();
            }
        }

        public void DeleteDowloadFiles()
        {
            try
            {
                Directory.Delete(newPath, true);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    }
}
