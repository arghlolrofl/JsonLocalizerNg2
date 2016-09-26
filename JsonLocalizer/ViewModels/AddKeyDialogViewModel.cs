using JsonLocalizer.Base;
using System;
using System.Text.RegularExpressions;
using System.Windows;

namespace JsonLocalizer.ViewModels
{
    public class AddKeyDialogViewModel : BaseNotifyChanged
    {
        private Regex regex = new Regex(@"\b[A-Z_]+\b", RegexOptions.Compiled);

        private string m_newKey;
        public string NewKey
        {
            get { return m_newKey; }
            set
            {
                m_newKey = value;
                RaisePropertyChanged();

                IsInvalid = !regex.Match(value).Success;
                if (!IsInvalid)
                    IsInvalid = value.Contains(" ");
            }
        }

        private string m_mainLanguageValue;
        public string MainLanguageValue
        {
            get { return m_mainLanguageValue; }
            set { m_mainLanguageValue = value; RaisePropertyChanged(); }
        }

        private string m_subLanguageValue;
        public string SubLanguageValue
        {
            get { return m_subLanguageValue; }
            set { m_subLanguageValue = value; RaisePropertyChanged(); }
        }

        public bool HasBeenCancelled { get; set; }
        private bool m_isInvalid;
        public bool IsInvalid
        {
            get { return m_isInvalid; }
            set { m_isInvalid = value; RaisePropertyChanged(); }
        }

        public AddKeyDialogViewModel()
        {
            if (!Clipboard.ContainsText())
                return;

            string text = Clipboard.GetText(TextDataFormat.Text);
            if (text.Contains(Environment.NewLine))
                return;

            if (regex.Match(text).Success)
                NewKey = text;
        }

    }
}
