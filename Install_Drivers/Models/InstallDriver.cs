using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Runtime.InteropServices;
using Nefarius.Devcon;
using System.Net.NetworkInformation;
using System.Threading;
using Install_Drivers.Models;

namespace Install_Drivers
{
    class InstallDriver
    {
        string log;
        /// <summary>
        /// Делегат вывода сообщений
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="check"></param>
        public delegate void ProcessStream(string msg, bool check);
        /// <summary>
        /// Событие вывода сообщений
        /// </summary>
        public static event ProcessStream OutputEvent;
        /// <summary>
        /// Делегат вывода сообщений о соединении
        /// </summary>
        /// <param name="msg"></param>
        public delegate void Connection(string msg);
        /// <summary>
        /// Событие вывода сообщений о соединении
        /// </summary>
        public static event Connection ConnectionEvent;
        /// <summary>
        /// Поле отмены установки
        /// </summary>
        public bool abortInstall;
        /// <summary>
        /// Поле массива inf файлов
        /// </summary>
        private string[] infMass;
        /// <summary>
        /// Конструктор экземпляра класса InstallDriver
        /// </summary>
        /// <param name="drivers"></param>
        public InstallDriver(List<Driver> drivers)
        {
            CheckSize();

            Task.Run(() => InstallDrv(drivers));
        }

        /// <summary>
        /// Метод запуска установки драйверов
        /// </summary>
        /// <param name="drivers"></param>
        private void InstallDrv(List<Driver> drivers)
        {
            foreach (var t in drivers)
            {
                while (!GetInf(t.DriverPath))
                {
                    int i = 1;

                    ConnectionEvent?.Invoke($"Отсутствие подключения. Попытка {i}");

                    log.Log($"{DateTime.Now} Установка: {t.DriverPath.RemoveText()} Ожидание получения *.inf\n");

                    Thread.Sleep(3000);

                    i++;
                }

                t.InfProgBarMax = infMass.Length;

                log.Log($"{DateTime.Now} Установка: {t.DriverPath} Начало установки\n");

                if (!InstallInf(t))
                {
                    log.Log($"{DateTime.Now} Установка: {t.DriverPath} завершена\n");

                    OutputEvent?.Invoke($"{t.DriverPath} завершена", t.CheckedDrv = false);
                }
                else
                {
                    break;
                }
            }

            if (!abortInstall)
            {
                OutputEvent?.Invoke("завершена", false);
                log.Log($"{DateTime.Now} Установка: полностью завершена\n");
            }
            else
            {
                File.AppendAllText("Install_log.txt", $"{DateTime.Now} Установка: отменена\n");
                OutputEvent?.Invoke($"отменена", true);
            }
        }

        /// <summary>
        /// Метод получения inf файлов
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool GetInf(string path)
        {
            try
            {
                ConnectionEvent?.Invoke($"Получение *.inf {path.RemoveText()}");

                infMass = Directory.GetFiles(path, "*.inf", SearchOption.AllDirectories);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Метод установки inf файлов
        /// </summary>
        /// <param name="driver"></param>
        /// <returns></returns>
        private bool InstallInf(Driver driver)
        {
            driver.InfProgBarNow = 0;

            for (int i = 0; i < infMass.Length; i++)
            {
                if (!abortInstall)
                {
                    if (NetworkInterface.GetIsNetworkAvailable())
                    {
                        ConnectionEvent?.Invoke($@"Установка: {driver.DriverPath.RemoveText()}");

                        log.Log($"{DateTime.Now} Установка: {infMass[i]}\n");

                        driver.InfProgBarNow++;

                        Devcon.Install(infMass[i], out _);

                        File.AppendAllText("Install_log.txt", $"{DateTime.Now} Установка: {infMass[i]} завершена\n");
                    }
                    else
                    {
                        log.Log($"{DateTime.Now} Установка: {infMass[i]} Ожидание подключения для установки\n");

                        ConnectionEvent?.Invoke($"Ожидание подключения к сети");

                        i--;

                        Thread.Sleep(3000);
                    }
                }
                else
                {
                    return true;
                }
            }

            return abortInstall;
        }

        /// <summary>
        /// Метод проверки размера лог файла
        /// </summary>
        private void CheckSize()
        {
            FileInfo fileInfo = new FileInfo(@"Install_log.txt");

            if (File.Exists(@"Install_log.txt") && (fileInfo.Length / 1048576) > 1)
            {
                File.Delete(@"Install_log.txt");
            }
        }
    }
}
