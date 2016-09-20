using JsonLocalizer.Base;
using JsonLocalizer.Contracts;

namespace JsonLocalizer.Models {
    public class LocalizationKey : BaseNotifyChanged, ILocalizationKey {
        private string m_key;
        public string Key {
            get { return m_key; }
            set { m_key = value; RaisePropertyChanged(); }
        }


        private string m_value;
        public string Value {
            get { return m_value; }
            set { m_value = value; RaisePropertyChanged(); }
        }

    }
}
