using Avalonia.Threading;
using System;
using System.Windows.Input;

namespace Ienemumu_Parvaldibas_Sistema.ViewModels
{
    //   CanExecuteChanged vienmēr tiek izsaukts UI pavedienā
    public sealed class DispatcherMarshalledCommand : ICommand, IDisposable
    {
        // oriģinālā komanda no klases
        private readonly ICommand _inner;

        // Konstruktors saņem esošu ICommand (piem  ViewModel loģikas)
        public DispatcherMarshalledCommand(ICommand inner)
        {
            // nelaiz 0 cauri
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));

            // nolasa iekšējās komandas 
            _inner.CanExecuteChanged += Inner_CanExecuteChanged;
        }

        // Reagee uz ieksejas komandas CanExecuteChanged
        private void Inner_CanExecuteChanged(object? sender, EventArgs e)
        {

            // notikumu var izsaukt tieši ja ir  ICommand, IDisposable
            if (Dispatcher.UIThread.CheckAccess())
            {
                CanExecuteChanged?.Invoke(this, e);
            }
            else
            {
                // Ja izsaukums nak no fona pavedien tas tiek parsutits uz UI pavedienu
                Dispatcher.UIThread.Post(() => CanExecuteChanged?.Invoke(this, e));
            }
        }

        // define CanExecute izsaukumu uz ieksejo komandu
        public bool CanExecute(object? parameter) => _inner.CanExecute(parameter);

        // definee Execute izsaukumu uz iekšējo komandu
        public void Execute(object? parameter) => _inner.Execute(parameter);

        // ICommand notikums, kuru sanem UI
        public event EventHandler? CanExecuteChanged;

        // settings lai katram chart savs
        public void Dispose()
        {
            _inner.CanExecuteChanged -= Inner_CanExecuteChanged;
            GC.SuppressFinalize(this);
        }
    }
}
