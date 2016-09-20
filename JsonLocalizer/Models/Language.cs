using JsonLocalizer.Base;
using JsonLocalizer.Contracts;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.Storage;

namespace JsonLocalizer.Models {
    public class Language : BaseNotifyChanged, ILanguage {
        private StorageFile m_file;
        public StorageFile File {
            get { return m_file; }
            private set {
                m_file = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Path));
            }
        }

        public string Path {
            get { return m_file.Path; }
        }

        private ObservableCollection<ILocalizationKey> m_items;
        public ObservableCollection<ILocalizationKey> Items {
            get { return m_items; }
            private set { m_items = value; RaisePropertyChanged(); }
        }

        private string m_name;
        public string Name {
            get { return m_name; }
            private set { m_name = value; RaisePropertyChanged(); }
        }

        public Language(StorageFile file, IList<ILocalizationKey> languageKeys) {
            File = file;
            Name = file.Name;
            Items = new ObservableCollection<ILocalizationKey>(languageKeys);
        }
    }
}
