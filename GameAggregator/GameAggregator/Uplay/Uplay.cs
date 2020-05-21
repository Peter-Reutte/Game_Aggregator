using GameAggregator.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GameAggregator.UplayStore
{
    public class Uplay
    {
        /// <summary>
        /// Поиск установленных игр Uplay
        /// </summary>
        /// <returns>Список установленных игр, с AppID, названиями и директорией установки</returns>
        public static List<IInstalledGame> Search_UplayInstalled()
        {
            List<IInstalledGame> uplayGames = new List<IInstalledGame>();
            string regkey = "SOFTWARE\\WOW6432Node\\Ubisoft\\Launcher\\Installs";
            RegistryKey key;
            try
            {
                key = Registry.LocalMachine.OpenSubKey(regkey);
            }
            catch
            {
                throw new Exception("Uplay не установлен?");
            }

            foreach (string ksubKey in key.GetSubKeyNames())
            {
                string dir = "", name = "";
                using (RegistryKey subKey = key.OpenSubKey(ksubKey))
                {
                    dir = subKey.GetValue("InstallDir").ToString();
                    name = dir.Remove(dir.Length - 1);
                    name = name.Substring(name.LastIndexOf('/') + 1);
                }
                uplayGames.Add(new Uplay_InstalledGame(name, @"uplay://launch/" + ksubKey, Launchers.Uplay));
            }

            return uplayGames;
        }
    }
}
