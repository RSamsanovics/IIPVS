using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ienemumu_Parvaldibas_Sistema.AppModels
{
    // Darijums/ direction :
    // In  – ienākums
    // Out – izdevums
    public enum TransactionDirection
    {
        In,
        Out
    }

    // transaction-  viens finansu darijums
    public class Transaction
    {
        // Darījuma datums 
        public DateTime Date { get; set; }

        // Darījuma summa
        public decimal Amount { get; set; }

        // Darījuma partneris 
        public string Partner { get; set; } = string.Empty;

        // Darījuma virziens (pievienots ar kodu)
        public TransactionDirection Direction { get; set; }
    }
}
