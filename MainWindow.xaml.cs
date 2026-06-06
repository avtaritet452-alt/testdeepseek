using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;

namespace LSDLauncher
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<Client> allClients = new();
        private Client? selectedClient;
        private System.Windows.Forms.NotifyIcon? trayIcon;
        private bool isQuitting = false;
        private Settings settings;
        private bool isNicknamePlaceholder = true;
        private CancellationTokenSource? cts;
        private string? selectedZipPath = null;
        private Client? editingClient = null;

        public MainWindow()
        {
            InitializeComponent();
            
            LoadVersion();
            
            settings = Settings.Load();
            
            InitializeTrayIcon();
            SetupNicknamePlaceholder();
            LoadClients();
            
            ClientsList.ItemsSource = allClients;
            StatusText.Text = $"❤️ Загружено {allClients.Count} клиентов";
            
            RandomNickCheckBox.IsChecked = settings.RandomNicknameEachLaunch;
            KeepFilesCheckBox.IsChecked = settings.KeepFiles;
        }

        private void LoadVersion()
        {
            string versionPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "version.txt");
            if (File.Exists(versionPath))
            {
                string version = File.ReadAllText(versionPath).Trim();
                VersionText.Text = $"v{version}";
            }
            else
            {
                VersionText.Text = "v1.0.0";
            }
        }

        private void LoadClients()
        {
            var clients = new List<Client>
            {
                new Client { Name = "Ae86.Client.vAe6.Beta", Tags = new List<string> { "#ddnet", "#cheat" }, DownloadUrl = "https://github.com/avtaritet452-alt/LSDv2/releases/download/v2.0/Ae86.Client.vAe6.Beta.zip", Executable = "DDNet.exe" },
                new Client { Name = "Ae86Client.vAe4", Tags = new List<string> { "#teeworlds", "#cheat" }, DownloadUrl = "https://github.com/avtaritet452-alt/LSDv2/releases/download/v2.0/Ae86Client.vAe4.zip", Executable = "DDNet.exe" },
                new Client { Name = "BestClient.1", Tags = new List<string> { "#ddnet", "#vibecoded" }, DownloadUrl = "https://github.com/avtaritet452-alt/LSDv2/releases/download/v2.0/BestClient.1.zip", Executable = "DDNet.exe" },
                new Client { Name = "Cactus-1.12.1-private-cracked", Tags = new List<string> { "#ddnet", "#crack" }, DownloadUrl = "https://github.com/avtaritet452-alt/LSDv2/releases/download/v2.0/Cactus-1.12.1-private-cracked.zip", Executable = "DDNet.exe" },
                new Client { Name = "Cactus-1.14-public-win64", Tags = new List<string> { "#ddnet", "#legit" }, DownloadUrl = "https://github.com/avtaritet452-alt/LSDv2/releases/download/v2.0/Cactus-1.14-public-win64.zip", Executable = "DDNet.exe" },
                new Client { Name = "ChillerBot-ux.Client", Tags = new List<string> { "#teeworlds", "#legit" }, DownloadUrl = "https://github.com/avtaritet452-alt/LSDv2/releases/download/v2.0/ChillerBot-ux.Client.zip", Executable = "chillerbot-ux.exe" },
                new Client { Name = "DD.CFF-2.35-public-test-9", Tags = new List<string> { "#ddnet", "#cheat" }, DownloadUrl = "https://github.com/avtaritet452-alt/LSDv2/releases/download/v2.0/DD.CFF-2.35-public-test-9.zip", Executable = "DDNet.exe" },
                new Client { Name = "DDNetPP-18.5.1-win64-20260503T074419Z-3-001", Tags = new List<string> { "#ddnet", "#legit" }, DownloadUrl = "https://github.com/avtaritet452-alt/LSDv2/releases/download/v2.0/DDNetPP-18.5.1-win64-20260503T074419Z-3-001.zip", Executable = "DDNet.exe" },
                new Client { Name = "DMDClient-1.3-BetaTest", Tags = new List<string> { "#ddnet", "#vibecoded" }, DownloadUrl = "https://github.com/avtaritet452-alt/LSDv2/releases/download/v2.0/DMDClient-1.3-BetaTest.zip", Executable = "DDNet.exe" },
                new Client { Name = "duck-infclass-client-4.2", Tags = new List<string> { "#ddnet", "#legit" }, DownloadUrl = "https://github.com/avtaritet452-alt/LSDv2/releases/download/v2.0/duck-infclass-client-4.2-windows.zip", Executable = "DDNet.exe" },
                new Client { Name = "Entity.Client", Tags = new List<string> { "#ddnet", "#legit" }, DownloadUrl = "https://github.com/avtaritet452-alt/LSDv2/releases/download/v2.0/Entity.Client.zip", Executable = "DDNet.exe" },
                new Client { Name = "FeX-v2.Client", Tags = new List<string> { "#ddnet", "#legit" }, DownloadUrl = "https://github.com/avtaritet452-alt/LSDv2/releases/download/v2.0/FeX-v2.Client.zip", Executable = "DDNet.exe" },
                new Client { Name = "FeX.Client", Tags = new List<string> { "#ddnet", "#legit" }, DownloadUrl = "https://github.com/avtaritet452-alt/LSDv2/releases/download/v2.0/FeX.Client.zip", Executable = "DDNet.exe" },
                new Client { Name = "goreworlds", Tags = new List<string> { "#teeworlds", "#cheat" }, DownloadUrl = "https://github.com/avtaritet452-alt/LSDv2/releases/download/v2.0/goreworlds.zip", Executable = "DDNet.exe" },
                new Client { Name = "JimJam-Client", Tags = new List<string> { "#teeworlds", "#legit" }, DownloadUrl = "https://github.com/avtaritet452-alt/LSDv2/releases/download/v2.0/JimJam-Client.zip", Executable = "DDNet.exe" },
                new Client { Name = "K-Client-win32", Tags = new List<string> { "#teeworlds", "#cheat" }, DownloadUrl = "https://github.com/avtaritet452-alt/LSDv2/releases/download/v2.0/K-Client-win32.zip", Executable = "DDNet.exe" },
                new Client { Name = "Kaizo.Client", Tags = new List<string> { "#ddnet", "#legit" }, DownloadUrl = "https://github.com/avtaritet452-alt/LSDv2/releases/download/v2.0/Kaizo.Client.zip", Executable = "DDNet.exe" },
                new Client { Name = "Koshka.Client.1", Tags = new List<string> { "#ddnet", "#legit" }, DownloadUrl = "https://github.com/avtaritet452-alt/LSDv2/releases/download/v2.0/Koshka.Client.1.zip", Executable = "DDNet.exe" },
                new Client { Name = "Kot.Client.v1.09", Tags = new List<string> { "#ddnet", "#vibecoded" }, DownloadUrl = "https://github.com/avtaritet452-alt/LSDv2/releases/download/v2.0/Kot.Client.v1.09.zip", Executable = "DDNet.exe" },
                new Client { Name = "LowC", Tags = new List<string> { "#ddnet", "#cheat" }, DownloadUrl = "https://github.com/avtaritet452-alt/LSDv2/releases/download/v2.0/LowC.zip", Executable = "DDNet.exe" },
                new Client { Name = "MarioTEE", Tags = new List<string> { "#teeworlds", "#legit" }, DownloadUrl = "https://github.com/avtaritet452-alt/LSDv2/releases/download/v2.0/MarioTEE.zip", Executable = "DDNet.exe" },
                new Client { Name = "MRX.Client-19.5-win64", Tags = new List<string> { "#ddnet", "#vibecoded" }, DownloadUrl = "https://github.com/avtaritet452-alt/LSDv2/releases/download/v2.0/MRX.Client-19.5-win64.zip", Executable = "DDNet.exe" },
                new Client { Name = "NnClientCrack", Tags = new List<string> { "#ddnet", "#crack" }, DownloadUrl = "https://github.com/avtaritet452-alt/LSDv2/releases/download/v2.0/NnClientCrack.zip", Executable = "DDNet.exe" },
                new Client { Name = "pulse", Tags = new List<string> { "#ddnet", "#legit" }, DownloadUrl = "https://github.com/avtaritet452-alt/LSDv2/releases/download/v2.0/pulse.zip", Executable = "DDNet.exe" },
                new Client { Name = "QmClient-windows", Tags = new List<string> { "#ddnet", "#legit" }, DownloadUrl = "https://github.com/avtaritet452-alt/LSDv2/releases/download/v2.0/QmClient-windows.zip", Executable = "DDNet.exe" },
                new Client { Name = "RClient-windows", Tags = new List<string> { "#ddnet", "#legit" }, DownloadUrl = "https://github.com/avtaritet452-alt/LSDv2/releases/download/v2.0/RClient-windows.zip", Executable = "DDNet.exe" },
                new Client { Name = "S-Client-2.6-win64", Tags = new List<string> { "#teeworlds", "#legit" }, DownloadUrl = "https://github.com/avtaritet452-alt/LSDv2/releases/download/v2.0/S-Client-2.6-win64.zip", Executable = "DDNet.exe" },
                new Client { Name = "saikoclient2", Tags = new List<string> { "#ddnet", "#cheat" }, DownloadUrl = "https://github.com/avtaritet452-alt/LSDv2/releases/download/v2.0/saikoclient2.zip", Executable = "DDNet.exe" },
                new Client { Name = "Sakura.Client", Tags = new List<string> { "#ddnet", "#legit" }, DownloadUrl = "https://github.com/avtaritet452-alt/LSDv2/releases/download/v2.0/Sakura.Client.zip", Executable = "DDNet.exe" },
                new Client { Name = "SashCrackByAkrD1338", Tags = new List<string> { "#teeworlds", "#crack" }, DownloadUrl = "https://github.com/avtaritet452-alt/LSDv2/releases/download/v2.0/SashCrackByAkrD1338.zip", Executable = "DDNet.exe" },
                new Client { Name = "SashUpgraded-V32", Tags = new List<string> { "#teeworlds", "#cheat" }, DownloadUrl = "https://github.com/avtaritet452-alt/LSDv2/releases/download/v2.0/SashUpgraded-V32.zip", Executable = "DDNet.exe" },
                new Client { Name = "SClient", Tags = new List<string> { "#teeworlds", "#cheat" }, DownloadUrl = "https://github.com/avtaritet452-alt/LSDv2/releases/download/v2.0/SClient.zip", Executable = "DDNet.exe" },
                new Client { Name = "ShalawaRelease.Client", Tags = new List<string> { "#ddnet", "#cheat" }, DownloadUrl = "https://github.com/avtaritet452-alt/LSDv2/releases/download/v2.0/ShalawaRelease.Client.zip", Executable = "DDNet.exe" },
                new Client { Name = "Soup.Client.Beta.1.0.1.free", Tags = new List<string> { "#vibecoded", "#ddnet", "#cheat" }, DownloadUrl = "https://github.com/avtaritet452-alt/LSDv2/releases/download/v2.0/Soup.Client.Beta.1.0.1.free.zip", Executable = "DDNet.exe" },
                new Client { Name = "soup", Tags = new List<string> { "#vibecoded", "#ddnet", "#cheat" }, DownloadUrl = "https://github.com/avtaritet452-alt/LSDv2/releases/download/v2.0/soup.zip", Executable = "DDNet.exe" },
                new Client { Name = "StA-win64", Tags = new List<string> { "#teeworlds", "#cheat" }, DownloadUrl = "https://github.com/avtaritet452-alt/LSDv2/releases/download/v2.0/StA-win64.zip", Executable = "DDNet.exe" },
                new Client { Name = "TClient-windows", Tags = new List<string> { "#ddnet", "#legit" }, DownloadUrl = "https://github.com/avtaritet452-alt/LSDv2/releases/download/v2.0/TClient-windows.zip", Executable = "DDNet.exe" },
                new Client { Name = "TClientV1+-windows", Tags = new List<string> { "#ddnet", "#cheat" }, DownloadUrl = "https://github.com/avtaritet452-alt/LSDv2/releases/download/v2.0/TClientV1+-windows.zip", Executable = "DDNet.exe" },
                new Client { Name = "UBR.Client", Tags = new List<string> { "#ddnet", "#legit" }, DownloadUrl = "https://github.com/avtaritet452-alt/LSDv2/releases/download/v2.0/UBR.Client.zip", Executable = "DDNet.exe" },
                new Client { Name = "vBot_p_alpha_0.1", Tags = new List<string> { "#teeworlds", "#cheat" }, DownloadUrl = "https://github.com/avtaritet452-alt/LSDv2/releases/download/v2.0/vBot_p_alpha_0.1.zip", Executable = "DDNet.exe" },
                new Client { Name = "YairClient-main", Tags = new List<string> { "#ddnet", "#legit" }, DownloadUrl = "https://github.com/avtaritet452-alt/LSDv2/releases/download/v2.0/YairClient-main.zip", Executable = "DDNet.exe" },
                new Client { Name = "Zyro-18.9.1-win64.1", Tags = new List<string> { "#ddnet", "#cheat" }, DownloadUrl = "https://github.com/avtaritet452-alt/LSDv2/releases/download/v2.0/Zyro-18.9.1-win64.1.zip", Executable = "DDNet.exe" }
            };

            foreach (var client in clients)
            {
                client.Status = LinkStatus.Unknown;
                allClients.Add(client);
            }
        }

        // Удаление клиента
        private void DeleteClient_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var border = button?.Parent as StackPanel;
            var grid = border?.Parent as Grid;
            var client = (grid?.DataContext as Client);

            if (client != null)
            {
                if (MessageBox.Show($"Удалить клиента \"{client.Name}\"?", "Подтверждение", 
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    if (client.IsLocal)
                    {
                        string clientDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "clients", SanitizeFileName(client.Name));
                        try { if (Directory.Exists(clientDir)) Directory.Delete(clientDir, true); } catch { }
                    }
                    
                    allClients.Remove(client);
                    RefreshClientsList();
                    StatusText.Text = $"🗑️ Удалён клиент: {client.Name}";
                    
                    if (selectedClient == client)
                    {
                        selectedClient = null;
                        ClientNameText.Text = "";
                        ClientTagsText.Text = "";
                        LaunchButton.IsEnabled = false;
                    }
                }
            }
        }

        // Начало переименования
        private void StartRename_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var border = button?.Parent as StackPanel;
            var grid = border?.Parent as Grid;
            var client = (grid?.DataContext as Client);
            
            if (client != null)
            {
                editingClient = client;
                
                var nameBlock = grid?.FindName("NameBlock") as TextBlock;
                var editBox = grid?.FindName("EditNameBox") as TextBox;
                
                if (nameBlock != null && editBox != null)
                {
                    nameBlock.Visibility = Visibility.Collapsed;
                    editBox.Visibility = Visibility.Visible;
                    editBox.Text = client.Name;
                    editBox.Focus();
                    editBox.SelectAll();
                }
            }
        }

        // Завершение переименования (потеря фокуса)
        private void EditNameBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var editBox = sender as TextBox;
            var grid = editBox?.Parent as Grid;
            var client = grid?.DataContext as Client;
            
            if (client != null && editingClient == client)
            {
                string newName = editBox?.Text.Trim();
                if (!string.IsNullOrEmpty(newName) && newName != client.Name)
                {
                    string oldName = client.Name;
                    client.Name = newName;
                    
                    if (client.IsLocal)
                    {
                        string oldDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "clients", SanitizeFileName(oldName));
                        string newDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "clients", SanitizeFileName(newName));
                        try { if (Directory.Exists(oldDir)) Directory.Move(oldDir, newDir); } catch { }
                    }
                    
                    RefreshClientsList();
                    StatusText.Text = $"✏️ Переименован: {oldName} → {newName}";
                }
                
                var nameBlock = grid?.FindName("NameBlock") as TextBlock;
                if (nameBlock != null && editBox != null)
                {
                    nameBlock.Visibility = Visibility.Visible;
                    editBox.Visibility = Visibility.Collapsed;
                }
                editingClient = null;
            }
        }

        // Завершение переименования (Enter)
        private void EditNameBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var editBox = sender as TextBox;
                editBox?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        private void RandomNickButton_Click(object sender, RoutedEventArgs e)
        {
            string randomNick = GenerateRandomNick();
            NicknameBox.Text = randomNick;
            isNicknamePlaceholder = false;
            NicknameBox.Foreground = System.Windows.Media.Brushes.White;
            settings.LastNickname = randomNick;
            settings.Save();
        }

        private string GenerateRandomNick()
        {
            string[] prefixes = { "Pro", "Super", "Mega", "Ultra", "Cool", "Fast", "Dark", "Light", "Crazy", "Smart" };
            string[] suffixes = { "Player", "Gamer", "Tee", "Runner", "Master", "Killer", "Hunter", "Lord", "King", "Wolf" };
            string[] numbers = { "", "1", "2", "3", "4", "5", "7", "9", "13", "42", "99" };
            Random rnd = new Random();
            return prefixes[rnd.Next(prefixes.Length)] + suffixes[rnd.Next(suffixes.Length)] + numbers[rnd.Next(numbers.Length)];
        }

        private void SelectZipButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = "ZIP files (*.zip)|*.zip";
            dialog.Title = "Выберите ZIP-файл с клиентом";
            
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                selectedZipPath = dialog.FileName;
                SelectedZipPath.Text = System.IO.Path.GetFileName(selectedZipPath);
                StatusText.Text = $"📁 Выбран файл: {System.IO.Path.GetFileName(selectedZipPath)}";
                
                string name = LocalClientName.Text.Trim();
                if (string.IsNullOrEmpty(name))
                {
                    name = System.IO.Path.GetFileNameWithoutExtension(selectedZipPath);
                    LocalClientName.Text = name;
                }
                
                string tagsText = LocalClientTags.Text.Trim();
                string clientDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "clients", SanitizeFileName(name));
                
                try
                {
                    Directory.CreateDirectory(clientDir);
                    ZipFile.ExtractToDirectory(selectedZipPath, clientDir, true);
                    
                    string exePath = Directory.GetFiles(clientDir, "*.exe", SearchOption.AllDirectories).FirstOrDefault();
                    string exeName = exePath != null ? Path.GetFileName(exePath) : "DDNet.exe";
                    
                    var newClient = new Client
                    {
                        Name = name,
                        DownloadUrl = "",
                        Tags = tagsText.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries).Select(t => t.StartsWith("#") ? t : $"#{t}").ToList(),
                        Executable = exeName,
                        Status = LinkStatus.Available,
                        IsLocal = true
                    };
                    
                    allClients.Add(newClient);
                    RefreshClientsList();
                    
                    LocalClientName.Text = "";
                    LocalClientTags.Text = "";
                    selectedZipPath = null;
                    SelectedZipPath.Text = "Файл не выбран";
                    
                    StatusText.Text = $"✅ Добавлен локальный клиент: {name}";
                }
                catch (Exception ex)
                {
                    StatusText.Text = $"❌ Ошибка: {ex.Message}";
                }
            }
        }

        private async void CheckLinksButton_Click(object sender, RoutedEventArgs e)
        {
            CheckLinksButton.IsEnabled = false;
            StatusText.Text = "🔍 Проверка доступности клиентов...";
            
            var tasks = allClients.Select(async client =>
            {
                if (client.IsLocal || string.IsNullOrEmpty(client.DownloadUrl))
                {
                    client.Status = client.IsLocal ? LinkStatus.Available : LinkStatus.NoLink;
                    return;
                }
                
                client.Status = LinkStatus.Checking;
                await Task.Delay(50);
                
                try
                {
                    using (var handler = new HttpClientHandler())
                    {
                        handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                        using (var http = new HttpClient(handler))
                        {
                            http.DefaultRequestHeaders.Add("User-Agent", GetUserAgent());
                            var request = new HttpRequestMessage(HttpMethod.Head, client.DownloadUrl);
                            using (var response = await http.SendAsync(request))
                            {
                                client.Status = response.IsSuccessStatusCode ? LinkStatus.Available : LinkStatus.NotFound;
                            }
                        }
                    }
                }
                catch
                {
                    client.Status = LinkStatus.Error;
                }
            });
            
            await Task.WhenAll(tasks);
            
            var available = allClients.Count(c => c.Status == LinkStatus.Available);
            var notFound = allClients.Count(c => c.Status == LinkStatus.NotFound);
            
            StatusText.Text = $"✅ Доступно: {available} | ❌ Не найдено: {notFound}";
            CheckLinksButton.IsEnabled = true;
            RefreshClientsList();
        }
        
        private void RefreshClientsList()
        {
            var temp = allClients.ToList();
            ClientsList.ItemsSource = null;
            ClientsList.ItemsSource = temp;
        }

        private string GetUserAgent()
        {
            return "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/134.0.0.0 Safari/537.36";
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilters();

        private void ApplyFilters()
        {
            string search = SearchBox.Text?.Trim().ToLower() ?? "";
            var filtered = allClients.Where(c =>
                string.IsNullOrEmpty(search) || 
                c.Name.ToLower().Contains(search) || 
                c.TagsDisplay.ToLower().Contains(search)
            ).ToList();
            ClientsList.ItemsSource = filtered;
            StatusText.Text = filtered.Count == allClients.Count ? $"❤️ {filtered.Count} клиентов" : $"🔍 Найдено {filtered.Count} из {allClients.Count}";
        }

        private void InitializeTrayIcon()
        {
            trayIcon = new System.Windows.Forms.NotifyIcon();
            trayIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Process.GetCurrentProcess().MainModule?.FileName ?? "LSDLauncher.exe");
            trayIcon.Text = "LSD Launcher";
            trayIcon.Visible = true;
            trayIcon.DoubleClick += (s, e) => ShowWindow();
            
            var contextMenu = new System.Windows.Forms.ContextMenuStrip();
            var openItem = new System.Windows.Forms.ToolStripMenuItem("Открыть");
            openItem.Click += (s, e) => ShowWindow();
            contextMenu.Items.Add(openItem);
            var exitItem = new System.Windows.Forms.ToolStripMenuItem("Выйти");
            exitItem.Click += (s, e) => Quit();
            contextMenu.Items.Add(exitItem);
            trayIcon.ContextMenuStrip = contextMenu;
        }

        private void SetupNicknamePlaceholder()
        {
            if (!string.IsNullOrEmpty(settings.LastNickname))
            {
                NicknameBox.Text = settings.LastNickname;
                isNicknamePlaceholder = false;
                NicknameBox.Foreground = System.Windows.Media.Brushes.White;
            }
            else
            {
                NicknameBox.Text = "Player";
                isNicknamePlaceholder = true;
                NicknameBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
            
            NicknameBox.GotFocus += (s, e) =>
            {
                if (isNicknamePlaceholder) { NicknameBox.Text = ""; isNicknamePlaceholder = false; NicknameBox.Foreground = System.Windows.Media.Brushes.White; }
            };
            
            NicknameBox.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(NicknameBox.Text))
                {
                    NicknameBox.Text = "Player";
                    isNicknamePlaceholder = true;
                    NicknameBox.Foreground = System.Windows.Media.Brushes.Gray;
                }
                else
                {
                    settings.LastNickname = NicknameBox.Text;
                    settings.Save();
                }
            };
        }

        private void ClientsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClientsList.SelectedItem is Client client)
            {
                selectedClient = client;
                ClientNameText.Text = client.Name;
                ClientTagsText.Text = client.TagsDisplay;
                LaunchButton.IsEnabled = true;
            }
        }

        private async void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedClient == null) return;
            
            settings.RandomNicknameEachLaunch = RandomNickCheckBox.IsChecked ?? false;
            settings.KeepFiles = KeepFilesCheckBox.IsChecked ?? false;
            settings.Save();
            
            LaunchButton.IsEnabled = false;
            StatusText.Text = $"🚀 Запуск {selectedClient.Name}...";

            string nickname = settings.RandomNicknameEachLaunch ? GenerateRandomNick() : (isNicknamePlaceholder || string.IsNullOrWhiteSpace(NicknameBox.Text) ? "Player" : NicknameBox.Text.Trim());
            if (settings.RandomNicknameEachLaunch) { NicknameBox.Text = nickname; isNicknamePlaceholder = false; NicknameBox.Foreground = System.Windows.Media.Brushes.White; }

            try
            {
                string tempDir = Path.Combine(Path.GetTempPath(), "LSDLauncher", SanitizeFileName(selectedClient.Name));
                string exePath = Path.Combine(tempDir, selectedClient.Executable);

                if (selectedClient.IsLocal)
                {
                    string clientDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "clients", SanitizeFileName(selectedClient.Name));
                    exePath = Path.Combine(clientDir, selectedClient.Executable);
                    
                    if (!File.Exists(exePath))
                    {
                        var found = Directory.GetFiles(clientDir, "*.exe", SearchOption.AllDirectories);
                        if (found.Length > 0) exePath = found[0];
                    }
                }
                else if (!File.Exists(exePath) && !string.IsNullOrEmpty(selectedClient.DownloadUrl))
                {
                    ProgressBorder.Visibility = Visibility.Visible;
                    DownloadProgress.Value = 0;
                    
                    string zipPath = tempDir + ".zip";
                    Directory.CreateDirectory(tempDir);

                    cts = new CancellationTokenSource();
                    var progress = new Progress<float>(p => DownloadProgress.Value = p);

                    try
                    {
                        await DownloadFileWithIgnoreSSL(selectedClient.DownloadUrl, zipPath, progress, cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        StatusText.Text = "❌ Скачивание отменено";
                        ProgressBorder.Visibility = Visibility.Collapsed;
                        return;
                    }

                    if (File.Exists(zipPath))
                    {
                        StatusText.Text = "📦 Распаковка...";
                        ZipFile.ExtractToDirectory(zipPath, tempDir, true);
                        File.Delete(zipPath);
                    }

                    var found = Directory.GetFiles(tempDir, "*.exe", SearchOption.AllDirectories);
                    if (found.Length > 0) exePath = found[0];
                    ProgressBorder.Visibility = Visibility.Collapsed;
                }

                if (File.Exists(exePath))
                {
                    var process = new Process { StartInfo = new ProcessStartInfo { FileName = exePath, Arguments = $"+name \"{nickname}\"", UseShellExecute = false } };
                    process.Start();
                    HideWindow();
                    StatusText.Text = $"✅ Запущен {selectedClient.Name}";
                    await process.WaitForExitAsync();
                    if (!settings.KeepFiles && !selectedClient.IsLocal) 
                    { 
                        try { Directory.Delete(tempDir, true); } catch { } 
                    }
                    ShowWindow();
                    StatusText.Text = $"✅ {selectedClient.Name} завершён";
                }
                else
                {
                    StatusText.Text = $"❌ Клиент не найден. Искали: {selectedClient.Executable}";
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"❌ {ex.Message}";
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                ProgressBorder.Visibility = Visibility.Collapsed;
            }
            finally { LaunchButton.IsEnabled = true; }
        }

        private async Task DownloadFileWithIgnoreSSL(string url, string savePath, IProgress<float> progress, CancellationToken token)
        {
            using (var handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                using (var http = new HttpClient(handler))
                {
                    http.DefaultRequestHeaders.Add("User-Agent", GetUserAgent());
                    using (var response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token))
                    {
                        response.EnsureSuccessStatusCode();
                        var total = response.Content.Headers.ContentLength ?? -1;
                        using (var fs = new FileStream(savePath, FileMode.Create))
                        using (var stream = await response.Content.ReadAsStreamAsync(token))
                        {
                            var buffer = new byte[81920];
                            long totalRead = 0;
                            int bytesRead;
                            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
                            {
                                await fs.WriteAsync(buffer, 0, bytesRead, token);
                                totalRead += bytesRead;
                                if (total > 0)
                                    progress?.Report((float)totalRead / total * 100);
                            }
                        }
                    }
                }
            }
        }
        
        private void CancelDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            cts?.Cancel();
            StatusText.Text = "❌ Скачивание отменено";
            ProgressBorder.Visibility = Visibility.Collapsed;
            LaunchButton.IsEnabled = true;
        }

        private void HideWindow() { this.Hide(); trayIcon?.ShowBalloonTip(1000, "LSD Launcher", "Игра запущена в фоне", System.Windows.Forms.ToolTipIcon.Info); }
        private void ShowWindow() { this.Show(); this.WindowState = WindowState.Normal; this.Activate(); }
        private void Quit() { isQuitting = true; trayIcon?.Dispose(); Application.Current.Shutdown(); }
        private string SanitizeFileName(string name) { foreach (char c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_'); return name; }
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { if (e.ClickCount == 2) HideWindow(); else DragMove(); }
        private void CloseButton_Click(object sender, RoutedEventArgs e) { Quit(); }
        private void TelegramLink_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { try { Process.Start(new ProcessStartInfo { FileName = "https://t.me/LSDLauncher", UseShellExecute = true }); } catch { } }
        protected override void OnStateChanged(EventArgs e) { base.OnStateChanged(e); if (WindowState == WindowState.Minimized && !isQuitting) HideWindow(); }
    }

    public enum LinkStatus
    {
        Unknown, Checking, Available, NotFound, Error, NoLink
    }

    public class Client : System.ComponentModel.INotifyPropertyChanged
    {
        public string Name { get; set; } = "";
        public List<string> Tags { get; set; } = new();
        public string DownloadUrl { get; set; } = "";
        public string Executable { get; set; } = "DDNet.exe";
        public bool IsLocal { get; set; } = false;
        public string TagsDisplay => string.Join(" ", Tags);
        
        private LinkStatus _status = LinkStatus.Unknown;
        public LinkStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(StatusIcon));
                OnPropertyChanged(nameof(StatusColor));
                OnPropertyChanged(nameof(StatusTooltip));
            }
        }
        
        public string StatusIcon => Status switch
        {
            LinkStatus.Available => "✅",
            LinkStatus.NotFound => "❌",
            LinkStatus.Error => "⚠️",
            LinkStatus.Checking => "⏳",
            LinkStatus.NoLink => "🚫",
            _ => "❓"
        };
        
        public SolidColorBrush StatusColor => Status switch
        {
            LinkStatus.Available => new SolidColorBrush(Color.FromRgb(0x4c, 0xaf, 0x50)),
            LinkStatus.NotFound => new SolidColorBrush(Color.FromRgb(0xf4, 0x43, 0x36)),
            LinkStatus.Error => new SolidColorBrush(Color.FromRgb(0xff, 0x98, 0x00)),
            LinkStatus.Checking => new SolidColorBrush(Color.FromRgb(0xff, 0xff, 0x00)),
            LinkStatus.NoLink => new SolidColorBrush(Color.FromRgb(0x80, 0x80, 0x80)),
            _ => new SolidColorBrush(Color.FromRgb(0xa0, 0xa0, 0xa0))
        };
        
        public string StatusTooltip => Status switch
        {
            LinkStatus.Available => "✅ Доступен",
            LinkStatus.NotFound => "❌ Не найден",
            LinkStatus.Error => "⚠️ Ошибка",
            LinkStatus.Checking => "⏳ Проверка...",
            LinkStatus.NoLink => "🚫 Нет ссылки",
            _ => "❓ Неизвестно"
        };
        
        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }
}