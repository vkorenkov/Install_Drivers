using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Install_Drivers
{
    class Driver : PropChange
    {
        private string driverPath;
        /// <summary>
        /// Свойство пути к драйверам
        /// </summary>
        public string DriverPath
        {
            get { return driverPath; }
            set { driverPath = value; OnPropertyChanged(nameof(DriverPath)); }
        }

        private bool checkedDrv;
        /// <summary>
        /// Свойство отметки драйверов для установки
        /// </summary>
        public bool CheckedDrv
        {
            get { return checkedDrv; }
            set { checkedDrv = value; OnPropertyChanged(nameof(CheckedDrv)); }
        }

        private int infProgBarNow = 0;
        /// <summary>
        /// Cвоство текущего положения прогресс бара отдельного драйвера
        /// </summary>
        public int InfProgBarNow
        {
            get { return infProgBarNow; }
            set { infProgBarNow = value; OnPropertyChanged(nameof(InfProgBarNow)); }
        }

        private int infProgBarMax = 1;
        /// <summary>
        /// Свойство максимального значения прогресс бара отдельного драйвера
        /// </summary>
        public int InfProgBarMax
        {
            get { return infProgBarMax; }
            set { infProgBarMax = value; OnPropertyChanged(nameof(InfProgBarMax)); }
        }

        /// <summary>
        /// Конструктор экземпляра класса Driver
        /// </summary>
        /// <param name="path"></param>
        public Driver(string path)
        {
            DriverPath = path;
            CheckedDrv = true;
        }
    }
}
