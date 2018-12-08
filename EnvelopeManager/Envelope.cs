using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvelopeManager
{
    public class Envelope : INotifyPropertyChanged
    {
        private string m_name;
        public string Name { get => m_name; set => SetIfChanged(ref m_name, value); }
        private decimal m_amount = 0.00m;
        public decimal Amount { get => m_amount; set => SetIfChanged(ref m_amount, value); }

        public Envelope() : this("") { }
        public Envelope(string name) : this(name, 0.00m) { }
        public Envelope(string name, decimal amount) { Name = name; Amount = amount; }

        private void SetIfChanged<T>(ref T member, T value, Action callback = null, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(member, value))
                SetValue(ref member, value, callback, propertyName);
        }
        private void SetValue<T>(ref T member, T value, Action callback = null, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            member = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            callback?.Invoke();
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
