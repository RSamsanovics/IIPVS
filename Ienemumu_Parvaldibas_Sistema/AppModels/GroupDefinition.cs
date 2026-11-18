using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ienemumu_Parvaldibas_Sistema.AppModels
{
    //  apraksta vienu grupu ( ienākumu/izdevumu kategoriju)
    // Implementē INotifyPropertyChanged lai UI reagetu 
    public class GroupDefinition : INotifyPropertyChanged
    {
        // tiek glabātas faktiskās vērtības
        private string _name = "";
        private bool _isEnabled = true;
        private List<string> _keywords = new();

        // Grupas nosaukums 
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                // Paziņo ka Name ir mainījies
                OnPropertyChanged(nameof(Name));
            }
        }

        //  vai grupa ir aktīva?? (piem., vai to ņemt vērā filtrēšanā / grafikā)
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                // UI atjaunojas on/off
                OnPropertyChanged(nameof(IsEnabled));
            }
        }

        // keyword sarakststiek izmantots datu klasificēšanai
        public List<string> Keywords
        {
            get => _keywords;
            set
            {
                _keywords = value;
               
                OnPropertyChanged(nameof(Keywords));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        //  izsauc PropertyChanged notikumu konkrētai īpašībai
        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // helper prieks  UI :
        // - UI redz vienu teksta lauku
        // - Ieseji dati tiek glabati ka List<string>
        public string KeywordsText
        {
            // Apvieno atslēgvārdus vienā tekstā, atdalītus ar komatiem
            get => string.Join(", ", Keywords);
            set
            {
              
                // - sadala pēc komatiem
                // - noņem liekās atstarpes
                // - normalizē uz mazajiem burtiem
                Keywords = value
                    .Split(',', System.StringSplitOptions.RemoveEmptyEntries)
                    .Select(k => k.Trim().ToLowerInvariant())
                    .ToList();

                // nosuta info UI
                OnPropertyChanged(nameof(Keywords));
                OnPropertyChanged(nameof(KeywordsText));
            }
        }
    }
}
