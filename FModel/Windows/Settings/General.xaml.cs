﻿using FModel.Discord;
using FModel.Logger;
using FModel.Utils;
using FModel.ViewModels.ComboBox;
using FModel.Windows.DarkMessageBox;
using Ookii.Dialogs.Wpf;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FModel.Windows.Settings
{
    /// <summary>
    /// Logique d'interaction pour General.xaml
    /// </summary>
    public partial class General : Window
    {
        private string _inputPath;
        private string _outputPath;
        private bool _useDiscordRpc;
        private bool _useEnglish;

        public General()
        {
            InitializeComponent();
        }

        private void OnClosing(object sender, CancelEventArgs e) => Application.Current.Dispatcher.Invoke(async delegate
        {
            await SaveAndExit().ConfigureAwait(false);
        });
        private void OnClick(object sender, RoutedEventArgs e) => Close();
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _inputPath = Properties.Settings.Default.PakPath;
            _outputPath = Properties.Settings.Default.OutputPath;
            _useDiscordRpc = Properties.Settings.Default.UseDiscordRpc;
            _useEnglish = Properties.Settings.Default.UseEnglish;
            Languages_CbBox.ItemsSource = ComboBoxVm.languageCbViewModel;
            Languages_CbBox.SelectedItem = ComboBoxVm.languageCbViewModel.Where(x => x.Id == Properties.Settings.Default.AssetsLanguage).FirstOrDefault();
            Json_CbBox.ItemsSource = ComboBoxVm.jsonCbViewModel;
            Json_CbBox.SelectedItem = ComboBoxVm.jsonCbViewModel.Where(x => x.Id == Properties.Settings.Default.AssetsJsonType).FirstOrDefault();
        }

        private async Task SaveAndExit()
        {
            if (_useDiscordRpc && !Properties.Settings.Default.UseDiscordRpc) // previously enabled
                DiscordIntegration.Deinitialize();

            if (Properties.Settings.Default.AssetsLanguage != Languages_CbBox.SelectedIndex)
            {
                Properties.Settings.Default.AssetsLanguage = Languages_CbBox.SelectedIndex;
                await Localizations.SetLocalization(Properties.Settings.Default.AssetsLanguage, true).ConfigureAwait(false);
            }
            if (Properties.Settings.Default.AssetsJsonType != Json_CbBox.SelectedIndex)
            {
                Properties.Settings.Default.AssetsJsonType = Json_CbBox.SelectedIndex;
                Assets.ClearCachedFiles();
            }

            if (!_inputPath.Equals(Properties.Settings.Default.PakPath) ||
                !_outputPath.Equals(Properties.Settings.Default.OutputPath) ||
                _useEnglish != Properties.Settings.Default.UseEnglish)
            {
                DarkMessageBoxHelper.Show(Properties.Resources.PathChangedRestart, Properties.Resources.PathChanged, MessageBoxButton.OK, MessageBoxImage.Information);
                DebugHelper.WriteLine("{0} {1} {2}", "[FModel]", "[Restarting]", "Path(s) changed");

                Properties.Settings.Default.Save();
                Process.Start(Process.GetCurrentProcess().MainModule.FileName);
                Application.Current.Shutdown();
            }
            else
            {
                Properties.Settings.Default.Save();
                DebugHelper.WriteUserSettings();
                DebugHelper.WriteLine("{0} {1} {2}", "[FModel]", "[Window]", "Closing General Settings");
            }
        }

        private void OnInputClick(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog
            {
                ShowNewFolderButton = false,
                Description = Properties.Resources.SelectFolder,
                UseDescriptionForTitle = true
            };

            if ((bool)dialog.ShowDialog(this))
            {
                Properties.Settings.Default.PakPath = dialog.SelectedPath;
            }
        }

        private void OnOutputClick(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog
            {
                ShowNewFolderButton = false,
                Description = Properties.Resources.SelectFolder,
                UseDescriptionForTitle = true
            };

            if ((bool)dialog.ShowDialog(this))
            {
                Properties.Settings.Default.OutputPath = dialog.SelectedPath;
            }
        }
    }
}
