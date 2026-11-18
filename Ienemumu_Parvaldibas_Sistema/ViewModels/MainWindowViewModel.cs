using Ienemumu_Parvaldibas_Sistema.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Input;

namespace Ienemumu_Parvaldibas_Sistema.ViewModels
{
    // - lietotaju ielade / saglabasana
    // - login)
    // - register
    // -  AppWindow atversaana
    public class MainWindowViewModel : ViewModelBase
    {
        // Statiskie teksti UI
        public string Greeting => "Sveicināti ieņēmumu pārvaldības sistēmā!";
        public string Choice => "Izvēlies profilu:";

        // Lietotaju saraksts 
        public ObservableCollection<UserEntry> Users { get; } = new();

        // Palgipašības UI redzamībai
        public bool HasUsers => Users.Count > 0;
        public bool HasNoUsers => Users.Count == 0;

        // sobridejais lietotājs
        private UserEntry? _selectedUser;
        public UserEntry? SelectedUser
        {
            get => _selectedUser;
            set
            {
                // ReactiveUI – paziņo UI par SelectedUser izmainam
                this.RaiseAndSetIfChanged(ref _selectedUser, value);

                // Papildu atjauninājumi, kas NAV automātiski
                this.RaisePropertyChanged(nameof(HasSelectedUser));

                //claen
                LoginPassword = "";
                LoginStatus = "";
            }
        }

        // Vai lietotājs ir izvēlēts ?
        public bool HasSelectedUser => SelectedUser != null;

        // passwd
        private string _loginPassword = "";
        public string LoginPassword
        {
            get => _loginPassword;
            set => this.RaiseAndSetIfChanged(ref _loginPassword, value);
        }

        // Statuss
        private string _loginStatus = "";
        public string LoginStatus
        {
            get => _loginStatus;
            set => this.RaiseAndSetIfChanged(ref _loginStatus, value);
        }

        // =======================
        // CREATE USER
        // =======================

     
        private bool _isCreatingUser;
        public bool IsCreatingUser
        {
            get => _isCreatingUser;
            set => this.RaiseAndSetIfChanged(ref _isCreatingUser, value);
        }

        // Jauna liet lauki
        public string NewUsername { get; set; } = "";
        public string NewPassword { get; set; } = "";
        public string NewConfirmPassword { get; set; } = "";

        // =======================
        // COMMANDS
        // =======================

        // register log
        public ICommand ShowCreateUserCommand { get; }

        // noew user
        public ICommand CreateUserCommand { get; }

        // loginnn
        public ICommand LoginCommand { get; }

        
        public MainWindowViewModel()
        {
   

            // Ielādē lietotājus no users.json
            LoadUsersIntoCollection();

            // Reaģē uz Users kolekcijas izmaiņām
 
            Users.CollectionChanged += (_, _) =>
            {
                this.RaisePropertyChanged(nameof(HasUsers));
                this.RaisePropertyChanged(nameof(HasNoUsers));
            };

            // Komanda, kas parāda lietotāja izveides skatu
            ShowCreateUserCommand = new SimpleCommand(() =>
            {
                IsCreatingUser = true;
            });

            // Komandu piesaiste metodēm
            CreateUserCommand = new SimpleCommand(CreateUser);
            LoginCommand = new SimpleCommand(Login);
        }

        // Izveido jaunu lietotāju
        private void CreateUser()
        {
            // Validācija
            if (string.IsNullOrWhiteSpace(NewUsername))
                return;


            if (Users.Any(u => u.Name == NewUsername))
                return;

         
            if (NewPassword != NewConfirmPassword)
                return;

            // Pievieno jaunu lietotāju
            Users.Add(new UserEntry
            {
                Id = Users.Count + 1,
                Name = NewUsername,
                Password = NewPassword
            });

            // Saglabā users.json
            SaveUsers();

            // Notīra ievadi
            NewUsername = "";
            NewPassword = "";
            NewConfirmPassword = "";
            IsCreatingUser = false;
        }

        // Login
        private void Login()
        {
            
            if (SelectedUser == null)
                return;

            // Paroles pārbaude
            if (LoginPassword != SelectedUser.Password)
            {
                LoginStatus = "Wrong password";
                return;
            }

            // OPEN MAIN APP WINDOW
    
            var appWindow = new AppWindow(SelectedUser.Name);

            // close login window + open app window
            if (Avalonia.Application.Current?.ApplicationLifetime
                is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime lifetime)
            {
                appWindow.Show();
                lifetime.MainWindow?.Close();
            }
            else
            {
                LoginStatus = "Wrong password";
            }
        }

        // =======================
        // Persistence
        // =======================

        // Atgriež aplikācijas AppData ceļu
        private string GetAppDataPath()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "IenemumuParvaldibasSistema"
            );
        }

        // Saglabā lietotājus users.json failā
        private void SaveUsers()
        {
            var dir = GetAppDataPath();
            Directory.CreateDirectory(dir);

            var file = Path.Combine(dir, "users.json");
            var json = JsonSerializer.Serialize(Users);

            File.WriteAllText(file, json);
        }

        // Dzēš lietotāja kontu (statiska metode)
        public static void DeleteUserAccount(string username)
        {
            var appData = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "IenemumuParvaldibasSistema");

            // 1. Dzēš lietotāja mapi
            var userDir = Path.Combine(appData, username);
            if (Directory.Exists(userDir))
                Directory.Delete(userDir, recursive: true);

            // 2. Dzēš lietotāju no users.json
            var usersFile = Path.Combine(appData, "users.json");
            if (!File.Exists(usersFile))
                return;

            var json = File.ReadAllText(usersFile);
            var users = JsonSerializer.Deserialize<List<UserEntry>>(json) ?? new();

            users.RemoveAll(u => u.Name == username);

            File.WriteAllText(usersFile, JsonSerializer.Serialize(users));
        }

        // Ielādē lietotājus no users.json kolekcijā
        private void LoadUsersIntoCollection()
        {
            var file = Path.Combine(GetAppDataPath(), "users.json");

            if (!File.Exists(file))
                return;

            var json = File.ReadAllText(file);
            var loaded = JsonSerializer.Deserialize<List<UserEntry>>(json);

            if (loaded == null)
                return;

            Users.Clear();
            foreach (var user in loaded)
                Users.Add(user);
        }
    }
}
