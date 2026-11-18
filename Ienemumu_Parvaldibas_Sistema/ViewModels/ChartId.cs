using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ienemumu_Parvaldibas_Sistema.ViewModels
{
//grafiku nosaukumi lai filtretu
    public enum ChartId
    {
        // default
        None,

        // 1 diag
        SpendingByCategory,

        // 2diag
        DailySpending,

        // 3 diag
        IncomeUsage,

        // 4diag
        IncomeVsSpent
    }
}
