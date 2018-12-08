using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using LINQtoCSV;

namespace EnvelopeManager
{
    //TODO: record total
    public class EnvelopeManagerViewModel : INotifyPropertyChanged
    {
        private bool m_unsavedChanges = false;
        public bool UnsavedChanges { get => m_unsavedChanges; set => SetIfChanged(ref m_unsavedChanges, value); }

        public CsvContext CsvContext { get; protected set; }
        public CsvFileDescription CsvFileDescription { get; protected set; }
        public EnvelopeManagerViewModel()
        {
            CsvContext = new CsvContext();
            CsvFileDescription = new CsvFileDescription()
            {
                SeparatorChar = ',',
                IgnoreUnknownColumns = true,
                FirstLineHasColumnNames = true,
                UseOutputFormatForParsingCsvValue = true,
                MaximumNbrExceptions = 1
            };
        }

        #region UI-Bound Data Properties

        private string m_transactionHistoryFile = string.Empty;
        public string TransactionHistoryFile { get => m_transactionHistoryFile; set => SetIfChanged(ref m_transactionHistoryFile, value); }

        private ObservableCollection<Envelope> m_envelopes = new ObservableCollection<Envelope>();
        public ObservableCollection<Envelope> Envelopes { get => m_envelopes; protected set => SetIfChanged(ref m_envelopes, value); }

        private string m_addEnvelopeName = string.Empty;
        public string AddEnvelopeName { get => m_addEnvelopeName; set => SetIfChanged(ref m_addEnvelopeName, value, UpdateAddEnvelopeEnabled); }

        public IEnumerable<TransactionType> TransactionTypes => Enum.GetValues(typeof(TransactionType)).Cast<TransactionType>();

        private TransactionType m_selectedTransactionType = TransactionType.Deposit;
        public TransactionType SelectedTransactionType
        {
            get => m_selectedTransactionType;
            set => SetIfChanged(ref m_selectedTransactionType, value, () =>
            {
                UpdateFromEnvelopeDropDownVisibility();
                UpdateToEnvelopeDropDownVisibility();
                UpdateAddTransactionEnabled();
            });
        }

        private ObservableCollection<string> m_envelopeNames = new ObservableCollection<string>();
        public ObservableCollection<string> EnvelopeNames { get => m_envelopeNames; set => SetIfChanged(ref m_envelopeNames, value); }

        private string m_fromEnvelope = string.Empty;
        public string FromEnvelope { get => m_fromEnvelope; set => SetIfChanged(ref m_fromEnvelope, value, UpdateAddTransactionEnabled); }

        private string m_toEnvelope = string.Empty;
        public string ToEnvelope { get => m_toEnvelope; set => SetIfChanged(ref m_toEnvelope, value, UpdateAddTransactionEnabled); }

        private string m_transactionAmountStr = string.Empty;
        public string TransactionAmountStr { get => m_transactionAmountStr; set => SetIfChanged(ref m_transactionAmountStr, value, UpdateAddTransactionEnabled); }

        private string m_transactionComment = string.Empty;
        public string TransactionComment { get => m_transactionComment; set => SetIfChanged(ref m_transactionComment, value); }

        #endregion UI-Bound Data Properties

        // If the file data is corrupted, the VM will pass the exception up the stack to notify the window of the error.
        // If the file does not exist, the VM will do nothing, allowing the 
        public void ReadTransactionHistory()
        {
            TransactionHistory.Clear();
            Envelopes.Clear();
            EnvelopeNames.Clear();
            if (!string.IsNullOrEmpty(TransactionHistoryFile) && File.Exists(TransactionHistoryFile))
            {
                try
                {
                    foreach (TransactionRecord record in CsvContext.Read<TransactionRecord>(TransactionHistoryFile, CsvFileDescription))
                    {
                        AddTransaction(record);
                    }
                }
                catch
                {
                    Envelopes.Clear();
                    EnvelopeNames.Clear();
                    TransactionHistory.Clear();
                    throw;
                }
                finally
                {
                    UnsavedChanges = false;
                }
            }
        }

        // If no file is selected, the VM will do nothing. If the file does not exist, the VM will create it.
        // If another I/O error occurs (for example, a permissions error), the VM will pass the exception up
        // the stack to allow the window to notify the user of the error.
        public void SaveTransactonHistory()
        {
            if (!string.IsNullOrEmpty(TransactionHistoryFile))
            {
                CsvContext.Write(TransactionHistory, TransactionHistoryFile, CsvFileDescription);
                UnsavedChanges = false;
            }
        }

        private List<TransactionRecord> m_transactionHistory = new List<TransactionRecord>();
        public List<TransactionRecord> TransactionHistory { get => m_transactionHistory; set => SetIfChanged(ref m_transactionHistory, value); }

        public void AddTransaction(TransactionRecord transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));
            
            TransactionHistory.Add(transaction);
            if (transaction.Type == TransactionType.Deposit || transaction.Type == TransactionType.Transfer)
            {
                if (EnvelopeNames.Contains(transaction.ToEnvelope))
                {
                    Envelopes.Where(env => env.Name == transaction.ToEnvelope).Single().Amount += transaction.Amount;
                }
                else
                {
                    Envelopes.Add(new Envelope(transaction.ToEnvelope, transaction.Amount));
                    EnvelopeNames.Add(transaction.ToEnvelope);
                }
            }
            if (transaction.Type == TransactionType.Withdrawal || transaction.Type == TransactionType.Transfer)
            {
                if (EnvelopeNames.Contains(transaction.FromEnvelope))
                {
                    Envelopes.Where(env => env.Name == transaction.FromEnvelope).Single().Amount -= transaction.Amount;
                }
                else
                {
                    Envelopes.Add(new Envelope(transaction.FromEnvelope, -transaction.Amount));
                    EnvelopeNames.Add(transaction.FromEnvelope);
                }
            }
            UnsavedChanges = true;
        }
        
        #region Xaml Enabled/Visibility Bindings
        
        private bool m_addEnvelopeEnabled = false;
        public bool AddEnvelopeEnabled { get => m_addEnvelopeEnabled; set => SetIfChanged(ref m_addEnvelopeEnabled, value); }
        public void UpdateAddEnvelopeEnabled()
        {
            AddEnvelopeEnabled = !string.IsNullOrEmpty(AddEnvelopeName) && !EnvelopeNames.Contains(AddEnvelopeName);
        }

        private bool m_addTransactionEnabled = false;
        public bool AddTransactionEnabled { get => m_addTransactionEnabled; set => SetIfChanged(ref m_addTransactionEnabled, value); }
        public void UpdateAddTransactionEnabled()
        {
            bool result = decimal.TryParse(TransactionAmountStr, NumberStyles.Currency, null, out decimal dummy) &&
                          (SelectedTransactionType == TransactionType.Deposit || SelectedTransactionType == TransactionType.Transfer || SelectedTransactionType == TransactionType.Withdrawal);
            if (SelectedTransactionType != TransactionType.Withdrawal)
                result &= EnvelopeNames.Contains(ToEnvelope);
            if (SelectedTransactionType != TransactionType.Deposit)
                result &= EnvelopeNames.Contains(FromEnvelope);
            AddTransactionEnabled = result;
        }

        private Visibility m_fromEnvelopeDropDownVisibility = Visibility.Collapsed;
        public Visibility FromEnvelopeDropDownVisibility { get => m_fromEnvelopeDropDownVisibility; set => SetIfChanged(ref m_fromEnvelopeDropDownVisibility, value); }
        public void UpdateFromEnvelopeDropDownVisibility()
        {
            FromEnvelopeDropDownVisibility = SelectedTransactionType == TransactionType.Deposit ? Visibility.Collapsed : Visibility.Visible;
        }

        private Visibility m_toEnvelopeDropDownVisibility = Visibility.Visible;
        public Visibility ToEnvelopeDropDownVisibility { get => m_toEnvelopeDropDownVisibility; set => SetIfChanged(ref m_toEnvelopeDropDownVisibility, value); }
        public void UpdateToEnvelopeDropDownVisibility()
        {
            ToEnvelopeDropDownVisibility = SelectedTransactionType == TransactionType.Withdrawal ? Visibility.Collapsed : Visibility.Visible;
        }

        #endregion Xaml Enabled/Visibility Bindings

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

    public class TransactionRecord
    {
        [CsvColumn(Name = nameof(Date))]
        public DateTime Date { get; set; }
        [CsvColumn(Name = nameof(Type))]
        public TransactionType Type { get; set; }
        // Ignored for deposits
        [CsvColumn(Name = nameof(FromEnvelope))]
        public string FromEnvelope { get; set; }
        // Ignored for withdrawals
        [CsvColumn(Name = nameof(ToEnvelope))]
        public string ToEnvelope { get; set; }
        [CsvColumn(Name = nameof(Amount), OutputFormat = "C2")]
        public decimal Amount { get; set; }
        [CsvColumn(Name = nameof(Comment))]
        public string Comment { get; set; }

        public TransactionRecord() { }
    }

    public enum TransactionType
    {
        Deposit = 0,
        Withdrawal = 1,
        Transfer = 2
    }
}
