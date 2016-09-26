using JsonLocalizer.Base;
using JsonLocalizer.Contracts;
using JsonLocalizer.Helpers;
using JsonLocalizer.Models;
using JsonLocalizer.Views;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace JsonLocalizer.ViewModels
{
    public class MainViewModel : BaseNotifyChanged
    {
        public event EventHandler<ILocalizationKey> MainScrollRequested;
        public event EventHandler<ILocalizationKey> SubScrollRequested;

        private bool m_isWindowEnabled = true;
        public bool IsWindowEnabled
        {
            get { return m_isWindowEnabled; }
            set { m_isWindowEnabled = value; RaisePropertyChanged(); }
        }

        private string m_statusMessage;
        public string StatusMessage
        {
            get { return m_statusMessage; }
            set { m_statusMessage = value; RaisePropertyChanged(); }
        }


        private ILanguage m_mainLanguage;
        public ILanguage MainLanguage
        {
            get { return m_mainLanguage; }
            set { m_mainLanguage = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(AreKeyOperationsEnabled)); }
        }

        public bool AreKeyOperationsEnabled
        {
            get { return MainLanguage != null; }
        }

        private ILocalizationKey m_selectedLocalizationKey;
        public ILocalizationKey SelectedLocalizationKey
        {
            get { return m_selectedLocalizationKey; }
            set
            {
                m_selectedLocalizationKey = value;
                RaisePropertyChanged();

                if (value != null)
                {
                    MainScrollRequested?.Invoke(this, value);
                    SelectedSubLocalizationKey = SelectedSubLanguage.Items.First((item) => item.Key == value.Key);
                }
            }
        }


        private ObservableCollection<ILanguage> m_subLanguage;
        public ObservableCollection<ILanguage> SubLanguages
        {
            get { return m_subLanguage; }
            set { m_subLanguage = value; RaisePropertyChanged(); }
        }

        private ILanguage m_selectedSubLanguage;
        public ILanguage SelectedSubLanguage
        {
            get { return m_selectedSubLanguage; }
            set { m_selectedSubLanguage = value; RaisePropertyChanged(); }
        }

        private ILocalizationKey m_selectedSubLocalizationKey;

        public ILocalizationKey SelectedSubLocalizationKey
        {
            get { return m_selectedSubLocalizationKey; }
            set
            {
                m_selectedSubLocalizationKey = value;
                RaisePropertyChanged();

                SubScrollRequested?.Invoke(this, value);
            }
        }


        private ICommand m_browseCommand;
        public ICommand BrowseCommand
        {
            get { return m_browseCommand ?? (m_browseCommand = new DelegateCommand(() => { BrowseCommand_OnExecuteAsync(); })); }
            set { m_browseCommand = value; RaisePropertyChanged(); }
        }

        private ICommand m_syncKeysCommand;
        public ICommand SyncKeysCommand
        {
            get { return m_syncKeysCommand ?? (m_syncKeysCommand = new DelegateCommand(() => { SyncKeysCommand_OnExecute(); })); }
            set { m_syncKeysCommand = value; RaisePropertyChanged(); }
        }

        private ICommand m_addKeyCommand;
        public ICommand AddKeyCommand
        {
            get { return m_addKeyCommand ?? (m_addKeyCommand = new DelegateCommand(() => { AddKeyCommand_OnExecute(); })); }
            set { m_addKeyCommand = value; RaisePropertyChanged(); }
        }

        private ICommand m_delKeyCommand;
        public ICommand DelKeyCommand
        {
            get { return m_delKeyCommand ?? (m_delKeyCommand = new DelegateCommand(() => { DelKeyCommand_OnExecute(); })); }
            set { m_delKeyCommand = value; RaisePropertyChanged(); }
        }

        private ICommand m_saveCommand;
        public ICommand SaveCommand
        {
            get { return m_saveCommand ?? (m_saveCommand = new DelegateCommand(() => { SaveCommand_OnExecute(); })); }
            set { m_saveCommand = value; RaisePropertyChanged(); }
        }

        private async void BrowseCommand_OnExecuteAsync()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "JSON localization file (*.json)|*.json";
            ofd.CheckFileExists = true;
            ofd.Multiselect = false;
            ofd.Title = "Select Localiazation File ...";
            ofd.ShowDialog();

            FileInfo selectedFile = new FileInfo(ofd.FileName);
            IEnumerable<FileInfo> subLanguageFiles = FilesystemHelper.FindSubLanguages(selectedFile);

            IList<ILocalizationKey> keys = await JsonHelper.ParseLanguage(selectedFile);
            MainLanguage = new Language(selectedFile, keys);

            SubLanguages = new ObservableCollection<ILanguage>();
            foreach (FileInfo subLanguageFile in subLanguageFiles)
            {
                keys = await JsonHelper.ParseLanguage(subLanguageFile);

                ILanguage language = new Language(subLanguageFile, keys);
                SubLanguages.Add(language);
            }

            if (SubLanguages.Any())
                SelectedSubLanguage = SubLanguages.First();
        }

        private void SyncKeysCommand_OnExecute()
        {
            List<string> addedKeys = new List<string>();
            int addedKeyCount = 0;

            for (int i = 0; i < MainLanguage.Items.Count; i++)
            {
                string keyName = MainLanguage.Items[i].Key;

                if (SelectedSubLanguage.ContainsLocalizationKey(keyName))
                    continue;

                SelectedSubLanguage.Items.Add(new LocalizationKey() { Key = keyName, Value = "" });
                addedKeys.Add(keyName);
                addedKeyCount++;
            }

            StatusMessage = ($"Added {addedKeyCount} to {SelectedSubLanguage.Name}: {string.Join(", ", addedKeys.ToArray())}");
        }

        private void AddKeyCommand_OnExecute()
        {
            var dialog = new AddKeyDialogView(new AddKeyDialogViewModel());
            dialog.Owner = App.Current.MainWindow;
            dialog.DialogClosed += AddKeyDialog_OnDialogClosed;
            dialog.Show();
            IsWindowEnabled = false;
        }

        private void AddKeyDialog_OnDialogClosed(object sender, AddKeyDialogViewModel e)
        {
            IsWindowEnabled = true;

            (sender as AddKeyDialogView).DialogClosed -= AddKeyDialog_OnDialogClosed;

            if (e == null)
                return;

            MainLanguage.Items.Add(new LocalizationKey() { Key = e.NewKey, Value = e.MainLanguageValue });

            SelectedSubLanguage.Items.Add(new LocalizationKey() { Key = e.NewKey, Value = e.SubLanguageValue });

            SelectedLocalizationKey = SelectedSubLanguage.Items.LastOrDefault();
        }

        private void DelKeyCommand_OnExecute()
        {
            ILocalizationKey keyToDelete = SelectedLocalizationKey;
            SelectedLocalizationKey = null;

            MainLanguage.Items.Remove(keyToDelete);
            SelectedSubLanguage.Items.Remove(SelectedSubLanguage.Items.First((item) => item.Key == keyToDelete.Key));
        }

        private void SaveCommand_OnExecute()
        {
            try
            {
                JsonHelper.SaveLanguage(MainLanguage);
                JsonHelper.SaveLanguage(SelectedSubLanguage);
                StatusMessage = "Files saved successfully!";
            }
            catch (Exception e)
            {
                StatusMessage = $"ERROR during save: {e.Message}";
            }
        }

    }
}
