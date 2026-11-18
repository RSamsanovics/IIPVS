using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ienemumu_Parvaldibas_Sistema.ViewModels
{
    // helper  grupesanas rezultatam
    internal class GroupingResult
    {
        // Grupas nosaukums ( "Food", "Transport", "Other")
        public string GroupName { get; set; } = "";

        // ja izvelets skaitlis
        public decimal Amount { get; set; }
    }
}
