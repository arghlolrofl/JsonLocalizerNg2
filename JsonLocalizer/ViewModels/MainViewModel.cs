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
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JsonLocalizer.ViewModels {
    public class MainViewModel : BaseNotifyChanged {
        #region Events

        public event EventHandler<ILocalizationKey> MainScrollRequested;
        public event EventHandler<ILocalizationKey> SubScrollRequested;

        #endregion

        #region Properties

        #region Enabled Bindings

        private bool m_isWindowEnabled = true;
        public bool IsWindowEnabled {
            get { return m_isWindowEnabled; }
            set { m_isWindowEnabled = value; RaisePropertyChanged(); }
        }

        private bool m_hasChanges;
        public bool HasChanges {
            get { return m_hasChanges; }
            set { m_hasChanges = value; RaisePropertyChanged(); }
        }

        public bool CanAddKey {
            get { return MainLanguage != null; }
        }
        public bool CanDeleteKey {
            get { return SelectedLocalizationKey != null && SelectedSubLocalizationKey != null; }
        }
        public bool CanSyncKeys {
            get { return MainLanguage != null && SelectedSubLanguage != null; }
        }

        #endregion

        private ILanguage m_mainLanguage;
        public ILanguage MainLanguage {
            get { return m_mainLanguage; }
            set {
                m_mainLanguage = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(CanAddKey));
            }
        }

        private ILanguage m_selectedSubLanguage;
        public ILanguage SelectedSubLanguage {
            get { return m_selectedSubLanguage; }
            set {
                m_selectedSubLanguage = value;

                RaisePropertyChanged();
                RaisePropertyChanged(nameof(CanSyncKeys));
            }
        }

        private ILocalizationKey m_selectedLocalizationKey;
        public ILocalizationKey SelectedLocalizationKey {
            get { return m_selectedLocalizationKey; }
            set {
                m_selectedLocalizationKey = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(CanDeleteKey));

                if (value != null) {
                    MainScrollRequested?.Invoke(this, value);
                    SelectedSubLocalizationKey = SelectedSubLanguage.Items.First((item) => item.Key == value.Key);
                }
            }
        }

        private ILocalizationKey m_selectedSubLocalizationKey;
        public ILocalizationKey SelectedSubLocalizationKey {
            get { return m_selectedSubLocalizationKey; }
            set {
                m_selectedSubLocalizationKey = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(CanDeleteKey));

                SubScrollRequested?.Invoke(this, value);
            }
        }

        private ObservableCollection<ILanguage> m_subLanguage;
        public ObservableCollection<ILanguage> SubLanguages {
            get { return m_subLanguage; }
            set { m_subLanguage = value; RaisePropertyChanged(); }
        }

        private string m_statusMessage;
        public string StatusMessage {
            get { return m_statusMessage; }
            set { m_statusMessage = value; RaisePropertyChanged(); }
        }

        #endregion

        #region Commands

        private ICommand m_browseCommand;
        public ICommand BrowseCommand {
            get { return m_browseCommand ?? (m_browseCommand = new DelegateCommand(() => { BrowseCommand_OnExecuteAsync(); })); }
            set { m_browseCommand = value; RaisePropertyChanged(); }
        }

        private ICommand m_syncKeysCommand;
        public ICommand SyncKeysCommand {
            get { return m_syncKeysCommand ?? (m_syncKeysCommand = new DelegateCommand(() => { SyncKeysCommand_OnExecute(); })); }
            set { m_syncKeysCommand = value; RaisePropertyChanged(); }
        }

        private ICommand m_addKeyCommand;
        public ICommand AddKeyCommand {
            get { return m_addKeyCommand ?? (m_addKeyCommand = new DelegateCommand(() => { AddKeyCommand_OnExecute(); })); }
            set { m_addKeyCommand = value; RaisePropertyChanged(); }
        }

        private ICommand m_delKeyCommand;
        public ICommand DelKeyCommand {
            get { return m_delKeyCommand ?? (m_delKeyCommand = new DelegateCommand(() => { DelKeyCommand_OnExecute(); })); }
            set { m_delKeyCommand = value; RaisePropertyChanged(); }
        }

        private ICommand m_saveCommand;
        public ICommand SaveCommand {
            get { return m_saveCommand ?? (m_saveCommand = new DelegateCommand(() => { SaveCommand_OnExecute(); })); }
            set { m_saveCommand = value; RaisePropertyChanged(); }
        }

        #endregion

        #region Initialization

        public MainViewModel() {
            FileInfo f = null;
            try {
                f = new FileInfo(ConfigurationManager.AppSettings.Get("LastFileUsed"));
            } catch (Exception ex) {
                StatusMessage = ex.Message;
            }

            if (f != null && f.Exists) {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                ParseFiles(f);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        #endregion

        private async void BrowseCommand_OnExecuteAsync() {
            OpenFileDialog ofd = DialogService.CreateOpenFileDialog(
                "json", "Select main localization file ...", true
            );
            ofd.ShowDialog();

            FileInfo selectedFile = null;
            try {
                selectedFile = new FileInfo(ofd.FileName);
                await ParseFiles(selectedFile);
            } catch (Exception ex) {
                StatusMessage = ex.Message;
                return;
            }
        }

        private async Task<bool> ParseFiles(FileInfo inputFile) {
            try {
                IEnumerable<FileInfo> subLanguageFiles = FilesystemHelper.FindSubLanguages(inputFile);

                IList<ILocalizationKey> keys = await JsonHelper.ParseLanguage(inputFile);
                MainLanguage = new Language(inputFile, keys);

                SubLanguages = new ObservableCollection<ILanguage>();
                foreach (FileInfo subLanguageFile in subLanguageFiles) {
                    keys = await JsonHelper.ParseLanguage(subLanguageFile);

                    ILanguage language = new Language(subLanguageFile, keys);
                    SubLanguages.Add(language);
                }

                if (SubLanguages.Any())
                    SelectedSubLanguage = SubLanguages.First();

                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                string oldValue = config.AppSettings.Settings["LastFileUsed"].Value;
                config.AppSettings.Settings["LastFileUsed"].Value = MainLanguage.Path;
                config.Save(ConfigurationSaveMode.Modified);
                HasChanges = false;

                return true;
            } catch (Exception ex) {
                StatusMessage = ex.Message;
                return false;
            }
        }

        #region Command Handlers

        private void SyncKeysCommand_OnExecute() {
            List<string> addedKeys = new List<string>();
            int addedKeyCount = 0;

            for (int i = 0; i < MainLanguage.Items.Count; i++) {
                string keyName = MainLanguage.Items[i].Key;

                if (SelectedSubLanguage.ContainsLocalizationKey(keyName))
                    continue;

                SelectedSubLanguage.Items.Add(new LocalizationKey() { Key = keyName, Value = "" });
                addedKeys.Add(keyName);
                addedKeyCount++;
            }

            if (addedKeyCount > 0)
                HasChanges = true;

            StatusMessage = ($"Added {addedKeyCount} to {SelectedSubLanguage.Name}: {string.Join(", ", addedKeys.ToArray())}");
        }

        private void AddKeyCommand_OnExecute() {
            StatusMessage = "Adding new key ...";

            var dialog = new AddKeyDialogView(new AddKeyDialogViewModel());
            dialog.Owner = App.Current.MainWindow;
            dialog.DialogClosed += AddKeyDialog_OnDialogClosed;
            IsWindowEnabled = false;
            dialog.Show();
        }

        private void AddKeyDialog_OnDialogClosed(object sender, AddKeyDialogViewModel e) {
            IsWindowEnabled = true;
            (sender as AddKeyDialogView).DialogClosed -= AddKeyDialog_OnDialogClosed;

            if (e == null)
                return;

            MainLanguage.Items.Add(new LocalizationKey() { Key = e.NewKey, Value = e.MainLanguageValue });
            SelectedSubLanguage.Items.Add(new LocalizationKey() { Key = e.NewKey, Value = e.SubLanguageValue });
            SelectedLocalizationKey = SelectedSubLanguage.Items.LastOrDefault();

            HasChanges = true;
            StatusMessage = "Key added:" + SelectedSubLocalizationKey.Key;
        }

        private void DelKeyCommand_OnExecute() {
            ILocalizationKey keyToDelete = SelectedLocalizationKey;

            MainLanguage.Items.Remove(keyToDelete);
            SelectedSubLanguage.Items.Remove(SelectedSubLanguage.Items.First((item) => item.Key == keyToDelete.Key));

            HasChanges = true;
            StatusMessage = "Key deleted:" + SelectedSubLocalizationKey.Key;
            SelectedLocalizationKey = null;
        }

        private void SaveCommand_OnExecute() {
            try {
                StatusMessage = "Saving main language file ...";
                JsonHelper.SaveLanguage(MainLanguage);
                StatusMessage = "Saving sub language file ...";
                JsonHelper.SaveLanguage(SelectedSubLanguage);
                HasChanges = false;
                StatusMessage = "Files saved successfully";
            } catch (Exception e) {
                StatusMessage = $"ERROR during save: {e.Message}";
            }
        }

        #endregion
    }
}
