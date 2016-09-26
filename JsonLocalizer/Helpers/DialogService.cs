using Microsoft.Win32;

namespace JsonLocalizer.Helpers {
    public class DialogService {
        public static OpenFileDialog CreateOpenFileDialog(string filter, string title, bool checkFileExists, bool multiSelect = false) {
            return new OpenFileDialog() {
                Filter = $"{filter.ToUpper()} file (*.{filter})|*.{filter}",
                Title = title,
                CheckFileExists = checkFileExists,
                Multiselect = multiSelect
            };
        }
    }
}
