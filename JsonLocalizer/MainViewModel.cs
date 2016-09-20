using JsonLocalizer.Base;
using JsonLocalizer.Contracts;
using JsonLocalizer.Helpers;
using JsonLocalizer.Models;
using Microsoft.Practices.Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace JsonLocalizer {
    public class MainViewModel : BaseNotifyChanged {
        private ILanguage m_mainLanguage;
        public ILanguage MainLanguage {
            get { return m_mainLanguage; }
            set { m_mainLanguage = value; RaisePropertyChanged(); }
        }

        private ILanguage m_selectedSubLanguage;
        public ILanguage SelectedSubLanguage {
            get { return m_selectedSubLanguage; }
            set { m_selectedSubLanguage = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<ILanguage> m_subLanguage;
        public ObservableCollection<ILanguage> SubLanguages {
            get { return m_subLanguage; }
            set { m_subLanguage = value; RaisePropertyChanged(); }
        }

        private ICommand m_browseCommand;

        public ICommand BrowseCommand {
            get {
                return m_browseCommand ?? (m_browseCommand = new DelegateCommand(
                () => { BrowseCommand_OnExecuteAsync(); }));
            }
            set { m_browseCommand = value; }
        }

        private async void BrowseCommand_OnExecuteAsync() {
            FileOpenPicker picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".json");

            StorageFile selectedFile = await picker.PickSingleFileAsync();
            IList<StorageFile> subLanguageFiles = await FilesystemHelper.FindSubLanguages(selectedFile);

            IList<ILocalizationKey> keys = await JsonHelper.ParseLanguage(selectedFile);
            MainLanguage = new Language(selectedFile, keys);

            SubLanguages = new ObservableCollection<ILanguage>();
            foreach (StorageFile subLanguageFile in subLanguageFiles) {
                keys = await JsonHelper.ParseLanguage(subLanguageFile);

                ILanguage language = new Language(subLanguageFile, keys);
                SubLanguages.Add(language);
            }
        }
    }
}
