using JsonLocalizer.Contracts;
using JsonLocalizer.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace JsonLocalizerWpf.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            (this.DataContext as MainViewModel).MainScrollRequested += ViewModel_OnMainScrollRequested;
            (this.DataContext as MainViewModel).SubScrollRequested += ViewModel_OnSubScrollRequested;
        }

        private void ViewModel_OnMainScrollRequested(object sender, ILocalizationKey e)
        {
            if (e == null)
                return;

            foreach (var item in MainLanguageGrid.Items)
            {
                var keyValue = item as ILocalizationKey;
                if (keyValue.Key == e.Key)
                {
                    MainLanguageGrid.ScrollIntoView(MainLanguageGrid.Items[MainLanguageGrid.Items.Count - 1]);
                    MainLanguageGrid.UpdateLayout();
                    MainLanguageGrid.ScrollIntoView(item);
                    break;
                }
            }
        }

        private void ViewModel_OnSubScrollRequested(object sender, ILocalizationKey e)
        {
            if (e == null)
                return;

            foreach (var item in SubLanguageGrid.Items)
            {
                var keyValue = item as ILocalizationKey;
                if (keyValue.Key == e.Key)
                {
                    SubLanguageGrid.ScrollIntoView(SubLanguageGrid.Items[SubLanguageGrid.Items.Count - 1]);
                    SubLanguageGrid.UpdateLayout();
                    SubLanguageGrid.ScrollIntoView(item);
                    break;
                }
            }
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (e.Key == Key.N)
                {
                    e.Handled = true;
                    (this.DataContext as MainViewModel).AddKeyCommand.Execute(null);

                }
            }
        }
    }
}
