using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LINQtoCSV;

namespace EnvelopeManager
{

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
