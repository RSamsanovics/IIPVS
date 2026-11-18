using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ienemumu_Parvaldibas_Sistema.ViewModels
{

    internal class TimeRanges
    {
    }

    // Mēnesa diapazona modelis

    public class MonthRange : INotifyPropertyChanged
    {
        // Notikums klausās ViewModel kad parbūvet grafiku
        public event PropertyChangedEventHandler? PropertyChanged;

        // helper PropertyChanged izsauksanai
        private void Notify(string n)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

        // Perioda sakuma gads
        private int _fromYear;
        public int FromYear
        {
            get => _fromYear;
            set
            {
                _fromYear = value;
                Notify(nameof(FromYear));
            }
        }

        // Perioda sākuma mēnesis
        private int _fromMonth;
        public int FromMonth
        {
            get => _fromMonth;
            set
            {
                _fromMonth = value;
                Notify(nameof(FromMonth));
            }
        }

        // Perioda beigu gads
        private int _toYear;
        public int ToYear
        {
            get => _toYear;
            set
            {
                _toYear = value;
                Notify(nameof(ToYear));
            }
        }

        // Perioda beigu mēnesis
        private int _toMonth;
        public int ToMonth
        {
            get => _toMonth;
            set
            {
                _toMonth = value;
                Notify(nameof(ToMonth));
            }
        }
    }

    // Dienu diapazona modelis
    // Izmanto DailySpending grafikam (konkrēts datumu intervāls)
    public class DayRange : INotifyPropertyChanged
    {
        // Notikums, ko klausās ViewModel, lai atjaunotu grafiku
        public event PropertyChangedEventHandler? PropertyChanged;

        // Palīgmetode PropertyChanged izsaukšanai
        private void Notify(string n)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

        // Perioda sākuma gads
        private int _fromYear;
        public int FromYear
        {
            get => _fromYear;
            set
            {
                _fromYear = value;
                Notify(nameof(FromYear));
            }
        }

        // Perioda sākuma mēnesis
        private int _fromMonth;
        public int FromMonth
        {
            get => _fromMonth;
            set
            {
                _fromMonth = value;
                Notify(nameof(FromMonth));
            }
        }

        // Perioda sākuma diena
        private int _fromDay;
        public int FromDay
        {
            get => _fromDay;
            set
            {
                _fromDay = value;
                Notify(nameof(FromDay));
            }
        }

        // Perioda beigu gads
        private int _toYear;
        public int ToYear
        {
            get => _toYear;
            set
            {
                _toYear = value;
                Notify(nameof(ToYear));
            }
        }

        // Perioda beigu mēnesis
        private int _toMonth;
        public int ToMonth
        {
            get => _toMonth;
            set
            {
                _toMonth = value;
                Notify(nameof(ToMonth));
            }
        }

        // Perioda beigu diena
        private int _toDay;
        public int ToDay
        {
            get => _toDay;
            set
            {
                _toDay = value;
                Notify(nameof(ToDay));
            }
        }
    }
}
