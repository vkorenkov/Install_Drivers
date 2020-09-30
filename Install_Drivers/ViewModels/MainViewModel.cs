using Install_Drivers.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Install_Drivers
{
    class MainViewModel : PropChange
    {
        private List<Driver> driversProp;
        /// <summary>
        /// Свойство найденных драйверов
        /// </summary>
        public List<Driver> DriversProp
        {
            get { return driversProp; }
            set { driversProp = value; OnPropertyChanged(nameof(DriversProp)); }
        }

        private string installMsg;
        /// <summary>
        /// Свойство вывода сообщений о статусе установки
        /// </summary>
        public string InstallMsg
        {
            get { return installMsg; }
            set { installMsg = value; OnPropertyChanged(nameof(InstallMsg)); }
        }

        private string driversPath = "Не выбрано";
        /// <summary>
        /// Свойство пути к выбранной папке с драйверами
        /// </summary>
        public string DriversPath
        {
            get { return driversPath; }
            set { driversPath = value; OnPropertyChanged(nameof(DriversPath)); }
        }

        private string driverPath = "Не выбрано";
        /// <summary>
        /// Свойство пути к выбранному exe файлу
        /// </summary>
        public string DriverPath
        {
            get { return driverPath; }
            set { driverPath = value; OnPropertyChanged(nameof(DriverPath)); }
        }

        private long allProgBarNow = 0;
        /// <summary>
        /// Свойство текущего положения общего прогесс-бара
        /// </summary>
        public long AllProgBarNow
        {
            get { return allProgBarNow; }
            set { allProgBarNow = value; OnPropertyChanged(nameof(AllProgBarNow)); }
        }

        private long allProgBarMax = 1;
        /// <summary>
        /// Свойство максимального положения общего прогесс-бара
        /// </summary>
        public long AllProgBarMax
        {
            get { return allProgBarMax; }
            set { allProgBarMax = value; OnPropertyChanged(nameof(AllProgBarMax)); }
        }

        private bool controlEnable = true;
        /// <summary>
        /// Свойство активности элементов управления установкой
        /// </summary>
        public bool ControlEnable
        {
            get { return controlEnable; }
            set { controlEnable = value; OnPropertyChanged(nameof(ControlEnable)); }
        }

        /// <summary>
        /// Поле экзкмпляра объекта текущей установки
        /// </summary>
        private InstallDriver installDrivers;

        private DownLoadDrivers dDrivers;

        /// <summary>
        /// Свойство коллекция доступных моделей устройств
        /// </summary>
        public List<string> DevicesModels { get; set; }

        private string modelName;
        /// <summary>
        /// Свойство имени модели устройства
        /// </summary>
        public string ModelName
        {
            get { return modelName; }
            set { modelName = value; OnPropertyChanged(nameof(ModelName)); DriversProp = GetPathes($@"{File.ReadAllText("Resources/GeneralPath")}\{ModelName}"); }
        }

        private bool checkDownload;
        public bool CheckDownload
        {
            get { return checkDownload; }
            set { checkDownload = value; OnPropertyChanged(nameof(CheckDownload)); }
        }

        private bool indeterminated;
        public bool Indeterminated
        {
            get { return indeterminated; }
            set { indeterminated = value; OnPropertyChanged(nameof(Indeterminated)); }
        }

        /// <summary>
        /// Команда открытия диалогового окна выбора пути к папке
        /// </summary>
        public ICommand OpenFolder => new RelayCommand<object>(obj =>
        {
            DriversPath = OpenFileFolder.OpenFolder();

            if (!string.IsNullOrEmpty(DriversPath))
            {
                DriversProp = GetPathes(DriversPath);
            }

        });
        /// <summary>
        /// Команда запуска установки драйверов
        /// </summary>
        public ICommand Install => new RelayCommand<object>(obj =>
        {
            List<Driver> checkDrv = DriversProp.Where(x => x.CheckedDrv == true).ToList();

            AllProgBarNow = 0;

            if ((AllProgBarMax = checkDrv.Count) > 0)
            {
                if (CheckDownload)
                {
                    dDrivers.abortDownload = false;

                    localInstall(checkDrv);
                }
                else
                {
                    ControlEnable = false;

                    installDrivers = new InstallDriver(checkDrv);
                }
            }
            else
            {
                MessageBox.Show("Не выбрано ни одного драйвера");
            }
        });
        /// <summary>
        /// Команда обновления найденных драйверов в соответствии с текущей моделью
        /// </summary>
        public ICommand Refresh => new RelayCommand<object>(obj =>
        {
            Task.Run(() =>
            {
                GetModelName();

                DriversPath = "Не выбрано";
                DriverPath = "Не выбрано";
            });
        });
        /// <summary>
        /// Комамнда открытия диалогового окна выбора пути к файлу
        /// </summary>
        public ICommand OpenFile => new RelayCommand<object>(obj =>
        {
            Task.Run(() =>
            {
                try
                {
                    DriverPath = OpenFileFolder.OpenFile();

                    Process.Start(DriverPath);
                }
                catch { }
            });
        });
        /// <summary>
        /// Команда "отметить все" или "снять все"
        /// </summary>
        public ICommand Mark => new RelayCommand<object>(obj =>
        {
            if (DriversProp != null)
            {
                switch (obj)
                {
                    case "+":
                        foreach (var t in DriversProp)
                        {
                            t.CheckedDrv = true;
                        }
                        break;
                    case "-":
                        foreach (var t in DriversProp)
                        {
                            t.CheckedDrv = false;
                        }
                        break;
                }
            }
        });
        /// <summary>
        /// Команда очистки всех путей и найденных драйверов
        /// </summary>
        public ICommand Clear => new RelayCommand<object>(obj =>
        {
            Task.Run(() =>
            {
                CheckDownload = false;
                DriversProp = null;
                DriversPath = "Не выбрано";
                DriverPath = "Не выбрано";
            });
        });
        /// <summary>
        /// Команда выхода из программы
        /// </summary>
        public ICommand Exit => new RelayCommand<object>(obj =>
        {
            foreach (var t in Process.GetProcesses())
            {
                if (t.ProcessName.Contains("pnputil"))
                {
                    t.Kill();
                }
            }

            Application.Current.Shutdown();
        });
        /// <summary>
        /// Команда перезагрузки или выключения компьютера
        /// </summary>
        public ICommand Restart => new RelayCommand<object>(obj =>
        {
            switch (obj)
            {
                case "R":
                    if (MessageBox.Show("Перезагрузить компьютер?", "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        Process.Start("Shutdown", "/r /t 0");
                    }
                    break;
                case "S":
                    if (MessageBox.Show("Выключить компьютер?", "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        Process.Start("Shutdown", "/s /t 0");
                    }
                    break;
            }

        });
        /// <summary>
        /// Команда отмены установки
        /// </summary>
        public ICommand Abort => new RelayCommand<object>(obj =>
        {
            if (installDrivers != null)
            {
                installDrivers.abortInstall = true;

                InstallMsg = "Отмена установки. Ждите";
            }

            if (dDrivers != null)
            {
                dDrivers.abortDownload = true;

                InstallMsg = "Отмена скачивания. Ждите";
            }
        });
        /// <summary>
        /// Команда пасхалки
        /// </summary>
        public ICommand Egg => new RelayCommand<object>(obj =>
        {
            new Egg().ShowDialog();

        });
        /// <summary>
        /// Конструктор ViewModel
        /// </summary>
        public MainViewModel()
        {
            dDrivers = new DownLoadDrivers();

            InstallDriver.OutputEvent += InstallDriver_OutputEvent;

            InstallDriver.ConnectionEvent += InstallDriver_ConnectionEvent;

            GetModelName();

            DevicesModels = new List<string>().GetModelsFolders();
        }

        /// <summary>
        /// Метод получения путей к папкам с отдельными драйверами
        /// </summary>
        /// <param name="getDir"></param>
        /// <returns></returns>
        private List<Driver> GetPathes(string getDir)
        {
            var tempList = Directory.GetDirectories(getDir);

            List<Driver> tempDrv = new List<Driver>();

            foreach (var t in tempList)
            {
                Driver driver = new Driver(t);

                tempDrv.Add(driver);
            }

            return tempDrv;
        }

        /// <summary>
        /// Метод получения имени модели устройства
        /// </summary>
        private void GetModelName()
        {
            ManagementObjectSearcher ProductName = new ManagementObjectSearcher("root\\cimv2", "SELECT * FROM Win32_ComputerSystemProduct");

            foreach (ManagementObject queryObj in ProductName.Get())
            {
                ModelName = queryObj["Name"].ToString();
            }

            try
            {
                DriversProp = GetPathes($@"{File.ReadAllText("Resources/GeneralPath")}\{ModelName}");
            }
            catch
            {
                InstallMsg = "Нет доступа к хранилищу. Выберите драйверы вручную.";
                DriversProp = null;
            }

        }

        private void localInstall(List<Driver> localFiles)
        {
            ControlEnable = false;

            dDrivers.SizeDelegateEvent += DownLoadDrivers_SizeDelegateEvent;

            InstallMsg = "Скачивание: в процессе";

            Task.Run(() =>
            {
                List<Driver> newDrvPath = dDrivers.DownloadDrivers(localFiles);

                if (!dDrivers.abortDownload)
                {
                    AllProgBarMax = newDrvPath.Count;

                    AllProgBarNow = 0;

                    installDrivers = new InstallDriver(newDrvPath);
                }
                else
                {
                    ControlEnable = true;

                    GetModelName();

                    InstallMsg = "Скачивание: отменено";
                }
            });
        }

        /// <summary>
        /// Обработчик события устанвки
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="check"></param>
        private void InstallDriver_OutputEvent(string msg, bool check)
        {
            if (msg.Contains(@"\"))
            {
                InstallMsg = $@"Установка: {msg.Substring(msg.LastIndexOf(@"\") + 1)}";
            }
            else
            {
                ControlEnable = true;
                InstallMsg = $@"Установка: {msg}";

                if (CheckDownload)
                {
                    dDrivers.DeleteDowloadFiles();

                    GetModelName();

                    CheckDownload = false;
                }
            }

            if (!check)
            {
                AllProgBarNow++;
            }
        }

        /// <summary>
        /// Обработчик события соединения
        /// </summary>
        /// <param name="msg"></param>
        private void InstallDriver_ConnectionEvent(string msg)
        {
            InstallMsg = msg;
        }

        private void DownLoadDrivers_SizeDelegateEvent(long max, long value)
        {
            AllProgBarMax = max;
            AllProgBarNow = value;
        }
    }
}
