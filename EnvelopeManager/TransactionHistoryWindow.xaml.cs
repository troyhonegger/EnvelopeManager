using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EnvelopeManager
{
    public partial class TransactionHistoryWindow : Window, INotifyPropertyChanged
    {
        public TransactionHistoryWindow(IEnumerable<TransactionRecord> history)
        {
            InitializeComponent();
            TransactionHistory = history?.ToList() ?? new List<TransactionRecord>();
            FilteredTransactionHistory = TransactionHistory;
        }

        public List<TransactionRecord> TransactionHistory { get; set; }

        private IEnumerable<TransactionRecord> m_filteredTransactionHistory;
        public IEnumerable<TransactionRecord> FilteredTransactionHistory { get => m_filteredTransactionHistory; set => SetIfChanged(ref m_filteredTransactionHistory, value); }

        private string m_searchText = string.Empty;
        public string SearchText { get => m_searchText; set => SetIfChanged(ref m_searchText, value, FilterTransactionHistory); }

        public void FilterTransactionHistory()
        {
            FilteredTransactionHistory = string.IsNullOrEmpty(SearchText) ? TransactionHistory : TransactionHistory.Where(record => RecordContainsText(record, SearchText));
        }
        public bool RecordContainsText(TransactionRecord record, string text)
        {
            if (record == null)
                return false;
            text = text.ToUpper();
            return (record.Date.ToString().ToUpper().Contains(text)) ||
                   (record.Type.ToString().ToUpper().Contains(text)) ||
                   (record.FromEnvelope?.ToUpper()?.Contains(text) == true) ||
                   (record.ToEnvelope?.ToUpper()?.Contains(text) == true) ||
                   (record.Amount.ToString("C2")).Contains(text) ||
                   (record.Comment?.ToUpper()?.Contains(text) == true);
        }

        #region INotifyPropertyChanged

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

        #endregion INotifyPropertyChanged
    }
}
