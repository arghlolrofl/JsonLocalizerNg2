using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;

namespace JsonLocalizer.Contracts
{
    public interface ILanguage : INotifyPropertyChanged
    {
        string Name { get; }
        string Path { get; }
        FileInfo File { get; }
        ObservableCollection<ILocalizationKey> Items { get; }

        bool ContainsLocalizationKey(string key);
    }
}
