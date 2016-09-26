using JsonLocalizer.Base;
using JsonLocalizer.Contracts;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace JsonLocalizer.Models
{
    public class Language : BaseNotifyChanged, ILanguage
    {
        private FileInfo m_file;
        public FileInfo File
        {
            get { return m_file; }
            private set
            {
                m_file = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Path));
            }
        }

        public string Path
        {
            get { return m_file.FullName; }
        }

        private ObservableCollection<ILocalizationKey> m_items;
        public ObservableCollection<ILocalizationKey> Items
        {
            get { return m_items; }
            private set { m_items = value; RaisePropertyChanged(); }
        }

        private string m_name;
        public string Name
        {
            get { return m_name; }
            private set { m_name = value; RaisePropertyChanged(); }
        }

        public Language(FileInfo file, IList<ILocalizationKey> languageKeys)
        {
            File = file;
            Name = file.Name;
            Items = new ObservableCollection<ILocalizationKey>(languageKeys);
        }

        public bool ContainsLocalizationKey(string key)
        {
            return Items.Any((item) => item.Key == key);
        }
    }
}
