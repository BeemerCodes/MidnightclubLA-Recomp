using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;   // OpenFileDialog
using System.Windows.Media.Imaging;

namespace MidnightClubLA.Launcher
{
    public partial class MainWindow : Window
    {
        // ───────── fixed command-line flags ─────────
        private const string FixedArgs =
            "--game_data_root=game" +
            " --async_shader_compilation=true" +
            " --texture_cache_memory_limit_soft=1536" +
            " --texture_cache_memory_limit_hard=2560" +
            " --texture_cache_memory_limit_soft_lifetime=5" +
            " --d3d12_host_vsync=false" +
            " --clear_memory_page_state=false" +
            " --log_level=off";

        private GameConfig _config;
        private bool _gameRunning;
        private bool _extracting;

        // ─────────────────────────────────────────────
        //  Construction
        // ─────────────────────────────────────────────

        public MainWindow()
        {
            InitializeComponent();
            LoadConfig();
            LoadCoverImage();
            LoadIconImage();
            PopulateControls();
            RefreshPlayStatus();
            CheckXisoTool();
        }

        // ─────────────────────────────────────────────
        //  Initialisation helpers
        // ─────────────────────────────────────────────

        private void LoadConfig()
        {
            string cfgPath = PathHelper.GetGameConfigPath();
            _config = GameConfig.Load(cfgPath);
        }

        private void LoadCoverImage()
        {
            try
            {
                string path = PathHelper.GetCoverImagePath();
                if (File.Exists(path))
                    CoverImage.Source = new BitmapImage(new Uri(path, UriKind.Absolute));
            }
            catch { }
        }

        private void LoadIconImage()
        {
            try
            {
                string path = Path.Combine(PathHelper.ExecutableDirectory, "icon.ico");
                if (File.Exists(path))
                    this.Icon = BitmapFrame.Create(new Uri(path, UriKind.Absolute));
            }
            catch { }
        }

        private void PopulateControls()
        {
            // Resolution
            if (_config.ResolutionScale >= 2)
                Radio2x.IsChecked = true;
            else
                RadioNative.IsChecked = true;

            // FXAA
            FxaaCheck.IsChecked = _config.UseFxaa;

            // ISO path
            if (!string.IsNullOrWhiteSpace(_config.LastIsoPath) &&
                File.Exists(_config.LastIsoPath))
            {
                IsoPathBox.Text = _config.LastIsoPath;
                ExtractButton.IsEnabled = true;
            }
        }

        private void RefreshPlayStatus()
        {
            string exePath   = PathHelper.GetGameExecutablePath();
            string gameData  = PathHelper.GetGameDataPath();
            string xexPath   = Path.Combine(gameData, "default.xex");

            bool exeExists  = File.Exists(exePath);
            bool dataExists = Directory.Exists(gameData) && File.Exists(xexPath);

            if (exeExists && dataExists)
            {
                PlayStatusText.Text = "✔  Jogo pronto para jogar.";
                PlayButton.IsEnabled = true;
            }
            else if (!exeExists)
            {
                PlayStatusText.Text = "✘  midnight_club_la.exe não encontrado. Compile o projeto primeiro.";
                PlayButton.IsEnabled = false;
            }
            else
            {
                PlayStatusText.Text = "✘  Arquivos do jogo não encontrados. Use CONFIG → Extrair Jogo.";
                PlayButton.IsEnabled = true; // allow attempt; error shown at launch
            }
        }

        private void CheckXisoTool()
        {
            string xisoPath = PathHelper.GetExtractXisoPath();
            if (!File.Exists(xisoPath))
            {
                ExtractionStatusText.Text =
                    "extract-xiso.exe não encontrado em:\n" + xisoPath +
                    "\n\nClique em \"Baixar extract-xiso\" para obter a ferramenta.";
            }
            else
            {
                ExtractionStatusText.Text =
                    "extract-xiso encontrado em:\n" + xisoPath +
                    "\n\nSelecione um ISO e clique em Extrair Jogo.";
            }

            // If game already extracted, show that status
            string xexPath = Path.Combine(PathHelper.GetGameDataPath(), "default.xex");
            if (File.Exists(xexPath))
            {
                ExtractionStatusText.Text =
                    "✔  Jogo já extraído (game\\default.xex encontrado).\n\n" +
                    "Você pode re-extrair se quiser substituir os arquivos.";
            }
        }

        // ─────────────────────────────────────────────
        //  Save helpers
        // ─────────────────────────────────────────────

        private void SaveConfig()
        {
            _config.ResolutionScale = (Radio2x.IsChecked == true) ? 2 : 1;
            _config.UseFxaa         = (FxaaCheck.IsChecked == true);
            _config.Save();
        }

        // ─────────────────────────────────────────────
        //  Build launch arguments
        // ─────────────────────────────────────────────

        private string BuildArguments()
        {
            var sb = new StringBuilder(FixedArgs);

            if (Radio2x.IsChecked == true)
                sb.Append(" --draw_resolution_scale_x=2 --draw_resolution_scale_y=2");

            if (FxaaCheck.IsChecked == true)
                sb.Append(" --swap_post_effect=fxaa");

            return sb.ToString();
        }

        // ─────────────────────────────────────────────
        //  PLAY button
        // ─────────────────────────────────────────────

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (_gameRunning)
            {
                global::System.Windows.MessageBox.Show(
                    "O jogo já está em execução.",
                    "Já rodando",
                    global::System.Windows.MessageBoxButton.OK, global::System.Windows.MessageBoxImage.Information);
                return;
            }

            string exePath = PathHelper.GetGameExecutablePath();
            if (!File.Exists(exePath))
            {
                global::System.Windows.MessageBox.Show(
                    "midnight_club_la.exe não foi encontrado em:\n" + exePath +
                    "\n\nCompile o projeto antes de jogar.",
                    "Executável não encontrado",
                    global::System.Windows.MessageBoxButton.OK, global::System.Windows.MessageBoxImage.Error);
                return;
            }

            SaveConfig();

            string workDir  = PathHelper.GetRepositoryRoot();
            string arguments = BuildArguments();

            PlayNoteText.Text = "Iniciando o jogo...";
            PlayButton.IsEnabled = false;
            _gameRunning = true;

            Task.Factory.StartNew(() =>
            {
                int exitCode = -1;
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName         = exePath,
                        Arguments        = arguments,
                        WorkingDirectory = workDir,
                        UseShellExecute  = false,
                    };

                    using (var proc = Process.Start(psi))
                    {
                        proc.WaitForExit();
                        exitCode = proc.ExitCode;
                    }
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        global::System.Windows.MessageBox.Show(
                            "Falha ao iniciar o jogo:\n" + ex.Message,
                            "Erro ao iniciar",
                            global::System.Windows.MessageBoxButton.OK, global::System.Windows.MessageBoxImage.Error);
                    });
                }

                Dispatcher.Invoke(() =>
                {
                    _gameRunning = false;
                    PlayButton.IsEnabled = true;
                    PlayNoteText.Text = exitCode == 0 ? "" : "O jogo encerrou com código " + exitCode + ".";
                });
            });
        }

        // ─────────────────────────────────────────────
        //  EXIT button
        // ─────────────────────────────────────────────

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            SaveConfig();
            Close();
        }

        // ─────────────────────────────────────────────
        //  ISO browse
        // ─────────────────────────────────────────────

        private void BrowseIso_Click(object sender, RoutedEventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Title  = "Selecionar ISO do Xbox 360";
                dlg.Filter = "ISO files (*.iso;*.xiso)|*.iso;*.xiso|All files (*.*)|*.*";

                string lastPath = _config.LastIsoPath;
                if (!string.IsNullOrWhiteSpace(lastPath) && Directory.Exists(Path.GetDirectoryName(lastPath)))
                    dlg.InitialDirectory = Path.GetDirectoryName(lastPath);

                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    IsoPathBox.Text         = dlg.FileName;
                    _config.LastIsoPath     = dlg.FileName;
                    ExtractButton.IsEnabled = true;
                }
            }
        }

        // ─────────────────────────────────────────────
        //  Extract game
        // ─────────────────────────────────────────────

        private void ExtractButton_Click(object sender, RoutedEventArgs e)
        {
            if (_extracting) return;

            string isoPath  = IsoPathBox.Text.Trim();
            string xisoExe  = PathHelper.GetExtractXisoPath();
            string outputDir = PathHelper.GetGameDataPath();

            if (!File.Exists(isoPath))
            {
                global::System.Windows.MessageBox.Show(
                    "ISO não encontrado:\n" + isoPath,
                    "Arquivo não encontrado",
                    global::System.Windows.MessageBoxButton.OK, global::System.Windows.MessageBoxImage.Warning);
                return;
            }

            if (!File.Exists(xisoExe))
            {
                global::System.Windows.MessageBox.Show(
                    "extract-xiso.exe não encontrado em:\n" + xisoExe +
                    "\n\nUse o botão \"Baixar extract-xiso\" para obter a ferramenta.",
                    "extract-xiso não encontrado",
                    global::System.Windows.MessageBoxButton.OK, global::System.Windows.MessageBoxImage.Warning);
                return;
            }

            Directory.CreateDirectory(outputDir);

            _extracting = true;
            ExtractButton.IsEnabled   = false;
            ExtractProgress.Visibility = Visibility.Visible;
            ExtractProgress.IsIndeterminate = true;
            ExtractionStatusText.Text = "Extraindo ISO... aguarde.\n(Isso pode levar alguns minutos)\n";

            SaveConfig();

            Task.Factory.StartNew(() =>
            {
                int    exitCode = -1;
                string stdErr   = string.Empty;
                string stdOut   = string.Empty;

                try
                {
                    if (!Directory.Exists(outputDir))
                    {
                        Directory.CreateDirectory(outputDir);
                    }
                    var psi = new ProcessStartInfo
                    {
                        FileName               = xisoExe,
                        Arguments              = string.Format("-d \"{1}\" -x \"{0}\"", isoPath, outputDir),
                        WorkingDirectory       = PathHelper.GetRepositoryRoot(),
                        UseShellExecute        = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError  = true,
                        CreateNoWindow         = true,
                    };

                    var outSb = new StringBuilder();
                    var errSb = new StringBuilder();

                    using (var proc = Process.Start(psi))
                    {
                        proc.OutputDataReceived += (s, ev) =>
                        {
                            if (ev.Data != null)
                            {
                                outSb.AppendLine(ev.Data);
                                Dispatcher.Invoke(() =>
                                    ExtractionStatusText.Text = outSb.ToString());
                            }
                        };
                        proc.ErrorDataReceived += (s, ev) =>
                        {
                            if (ev.Data != null)
                                errSb.AppendLine(ev.Data);
                        };

                        proc.BeginOutputReadLine();
                        proc.BeginErrorReadLine();
                        proc.WaitForExit();
                        exitCode = proc.ExitCode;
                    }

                    stdOut = outSb.ToString();
                    stdErr = errSb.ToString();
                }
                catch (Exception ex)
                {
                    stdErr = "Exceção: " + ex.Message;
                }

                Dispatcher.Invoke(() =>
                {
                    _extracting = false;
                    ExtractProgress.IsIndeterminate = false;
                    ExtractProgress.Value = 100;

                    if (exitCode == 0)
                    {
                        ExtractionStatusText.Text = "Extracao concluida com sucesso!";
                        ExtractProgress.Value = 100;
                        global::System.Windows.MessageBox.Show("O jogo foi extraido com sucesso e os diretorios foram gerados. Voce ja pode jogar!", "Extracao Concluida", global::System.Windows.MessageBoxButton.OK, global::System.Windows.MessageBoxImage.Information);
                        RefreshPlayStatus();
                    }
                    else
                    {
                        ExtractionStatusText.Text = "Falha ao extrair (codigo " + exitCode.ToString() + ")";
                        global::System.Windows.MessageBox.Show("Falha ao extrair (Codigo " + exitCode.ToString() + "). Verifique a ISO.", "Erro de Extracao", global::System.Windows.MessageBoxButton.OK, global::System.Windows.MessageBoxImage.Error);
                    }

                    ExtractButton.IsEnabled = true;
                });
            });
        }

        // ─────────────────────────────────────────────
        //  Download extract-xiso
        // ─────────────────────────────────────────────

        // ---------------------------------------------
        //  Shortcuts
        // ---------------------------------------------

        

        private void CreateShortcuts_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string iconPath = System.IO.Path.Combine(PathHelper.GetRepositoryRoot(), "assets", "icon.ico");

                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string startMenu = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
                
                CreateShortcut(System.IO.Path.Combine(desktop, "Midnight Club LA Recomp.lnk"), exePath, iconPath);
                CreateShortcut(System.IO.Path.Combine(startMenu, "Midnight Club LA Recomp.lnk"), exePath, iconPath);
                
                global::System.Windows.MessageBox.Show("Atalhos criados com sucesso na Area de Trabalho e no Menu Iniciar!", "Sucesso", global::System.Windows.MessageBoxButton.OK, global::System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                global::System.Windows.MessageBox.Show("Erro ao criar atalhos: " + ex.Message, "Erro", global::System.Windows.MessageBoxButton.OK, global::System.Windows.MessageBoxImage.Error);
            }
        }

        private void RemoveShortcuts_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string startMenu = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
                
                string desktopLink = System.IO.Path.Combine(desktop, "Midnight Club LA Recomp.lnk");
                string startMenuLink = System.IO.Path.Combine(startMenu, "Midnight Club LA Recomp.lnk");
                
                if (System.IO.File.Exists(desktopLink)) System.IO.File.Delete(desktopLink);
                if (System.IO.File.Exists(startMenuLink)) System.IO.File.Delete(startMenuLink);
                
                global::System.Windows.MessageBox.Show("Atalhos removidos com sucesso!", "Sucesso", global::System.Windows.MessageBoxButton.OK, global::System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                global::System.Windows.MessageBox.Show("Erro ao remover atalhos: " + ex.Message, "Erro", global::System.Windows.MessageBoxButton.OK, global::System.Windows.MessageBoxImage.Error);
            }
        }

        private void CreateShortcut(string shortcutPath, string targetPath, string iconPath)
        {
            string psCommand = "$s=(New-Object -COM WScript.Shell).CreateShortcut('" + shortcutPath + "');$s.TargetPath='" + targetPath + "';$s.IconLocation='" + iconPath + "';$s.Save()";
            var processInfo = new System.Diagnostics.ProcessStartInfo("powershell.exe", "-ExecutionPolicy Bypass -NoProfile -Command \"" + psCommand + "\"")
            {
                CreateNoWindow = true,
                UseShellExecute = false
            };
            System.Diagnostics.Process.Start(processInfo).WaitForExit();
        }
    }
}