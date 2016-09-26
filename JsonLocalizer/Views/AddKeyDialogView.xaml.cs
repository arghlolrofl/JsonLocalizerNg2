using JsonLocalizer.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;

namespace JsonLocalizer.Views
{
    /// <summary>
    /// Interaction logic for AddKeyDialogView.xaml
    /// </summary>
    public partial class AddKeyDialogView : Window
    {
        public event EventHandler<AddKeyDialogViewModel> DialogClosed;

        private AddKeyDialogViewModel m_viewModel;
        public AddKeyDialogViewModel ViewModel
        {
            get { return m_viewModel; }
            set { m_viewModel = value; }
        }


        public AddKeyDialogView(AddKeyDialogViewModel vm)
        {
            InitializeComponent();
            this.DataContext = ViewModel = vm;
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
            DialogClosed?.Invoke(this, ViewModel);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
            DialogClosed?.Invoke(this, null);
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            KeyInputBox.Focus();
            KeyInputBox.SelectAll();
        }

        private void KeyInputBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                ViewModel.HasBeenCancelled = false;
                Close();
                DialogClosed?.Invoke(this, ViewModel);
            }
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                ViewModel.HasBeenCancelled = true;
                Close();
                DialogClosed?.Invoke(this, null);
            }
        }
    }
}
