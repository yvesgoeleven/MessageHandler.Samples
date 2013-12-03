using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PhoneApp1
{
    public class Temperature : INotifyPropertyChanged
    {
        private double _current;

        public double Current
        {
            get { return _current; }
            set
            {
                _current = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}