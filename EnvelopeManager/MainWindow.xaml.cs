using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Win32;

namespace EnvelopeManager
{
    public partial class MainWindow : Window
    {
        public EnvelopeManagerViewModel VM { get; set; } = new EnvelopeManagerViewModel();

        public MainWindow()
        {
            OpenTransactionHistoryFile(Properties.Settings.Default.TransactionHistoryFile);
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (VM.UnsavedChanges)
            {
                var result = MessageBox.Show("Would you like to save your changes?", "Save changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);
                if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                else if (result == MessageBoxResult.Yes)
                {
                    bool? saveResult = SaveTransactionHistory();
                    if (saveResult == null) // user cancelled saving
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            }
            Properties.Settings.Default.TransactionHistoryFile = VM.TransactionHistoryFile;
            Properties.Settings.Default.Save();
        }

        private void btnViewHistory_Click(object sender, RoutedEventArgs e)
        {
            new TransactionHistoryWindow(VM.TransactionHistory).Show();
        }
        
        // return value: true = save operation successful; false = error; null = user cancelled operation
        private bool? SaveTransactionHistory()
        {
            if (string.IsNullOrEmpty(VM.TransactionHistoryFile))
            {
                SaveFileDialog sfd = new SaveFileDialog()
                {
                    CheckFileExists = false,
                    CheckPathExists = true,
                    ValidateNames = true,
                    Title = "Save as...",
                    AddExtension = true,
                    DefaultExt = ".csv",
                    CreatePrompt = true,
                    Filter = "CSV files (*.csv)|*.csv|All files|*.*"
                };
                bool? result = sfd.ShowDialog();
                if (result == true) { VM.TransactionHistoryFile = sfd.FileName; }
                else { return null; }
            }
            try
            {
                VM.SaveTransactonHistory();
                return true;
            }
            catch (Exception ex)
            {
                return MessageBox.Show("Exception details: " + ex.Message,
                                       "Error saving transaction history",
                                       MessageBoxButton.OKCancel,
                                       MessageBoxImage.Error,
                                       MessageBoxResult.Cancel) == MessageBoxResult.OK ? (bool?) false : null;
            }
        }

        private void OpenTransactionHistoryFile(string fileName)
        {
            VM.TransactionHistoryFile = fileName;
            try { VM.ReadTransactionHistory(); }
            catch (Exception ex)
            {
                MessageBox.Show("Exception details: " + ex.Message, "Error reading transaction history", MessageBoxButton.OK, MessageBoxImage.Error);
                VM.TransactionHistoryFile = string.Empty;
            }
        }
        
        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog()
            {
                CheckFileExists = false,
                CheckPathExists = true,
                ValidateNames = true,
                Title = "Select a file",
                AddExtension = true,
                DefaultExt = ".csv",
                CreatePrompt = true,
                Filter = "CSV files (*.csv)|*.csv|All files|*.*"
            };
            if (sfd.ShowDialog() == true)
            {
                if (VM.UnsavedChanges)
                {
                    var result = MessageBox.Show("Save your changes first?",
                                                 "You have unsaved transactions",
                                                 MessageBoxButton.YesNoCancel,
                                                 MessageBoxImage.Question,
                                                 MessageBoxResult.Cancel);
                    if (result == MessageBoxResult.Yes)
                    {
                        bool? saveResult = SaveTransactionHistory();
                        if (saveResult == null) { return; }
                    }
                    else if (result == MessageBoxResult.Cancel) { return; }
                }
                OpenTransactionHistoryFile(sfd.FileName);
            }
        }

        private void btnAddTransaction_Click(object sender, RoutedEventArgs e)
        {
            TransactionRecord transaction = new TransactionRecord()
            {
                Date = DateTime.Now,
                Type = VM.SelectedTransactionType,
                Amount = decimal.Parse(VM.TransactionAmountStr, System.Globalization.NumberStyles.Currency),
                Comment = VM.TransactionComment
            };
            if (transaction.Type == TransactionType.Deposit || transaction.Type == TransactionType.Transfer)
                transaction.ToEnvelope = VM.ToEnvelope;
            if (transaction.Type == TransactionType.Withdrawal || transaction.Type == TransactionType.Transfer)
                transaction.FromEnvelope = VM.FromEnvelope;
            VM.AddTransaction(transaction);
            VM.TransactionComment = string.Empty;
        }

        private void NewEnvelope()
        {
            VM.AddTransaction(new TransactionRecord() { Date = DateTime.Now, Type = TransactionType.Deposit, ToEnvelope = VM.AddEnvelopeName, Amount = 0.00m });
            VM.AddEnvelopeName = string.Empty;
        }

        private void btnAddEnvelope_Click(object sender, RoutedEventArgs e) { NewEnvelope(); }

        private void txtAddEnvelope_KeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Return && VM.AddEnvelopeEnabled) { NewEnvelope(); } }

        private void btnSave_Click(object sender, RoutedEventArgs e) { SaveTransactionHistory(); }
    }
}
