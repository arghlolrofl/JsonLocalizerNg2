using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.Storage;

namespace JsonLocalizer.Contracts {
    public interface ILanguage : INotifyPropertyChanged {
        string Name { get; }
        string Path { get; }
        StorageFile File { get; }
        ObservableCollection<ILocalizationKey> Items { get; }
    }
}
