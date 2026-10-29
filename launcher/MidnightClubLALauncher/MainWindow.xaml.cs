using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms; // OpenFileDialog
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace MidnightClubLA.Launcher
{
    public partial class MainWindow : Window
    {
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
            int scale = _config.ResolutionScale;
            if (scale == 1)
                Radio1x.IsChecked = true;
            else if (scale >= 3)
                Radio3x.IsChecked = true;
            else
                Radio2x.IsChecked = true;

            // FPS
            if (_config.TargetFps == 120 || _config.TargetFps == 0)
                RadioFps120.IsChecked = true;
            else
                RadioFps60.IsChecked = true;

            // Checkboxes / Flags
            FxaaCheck.IsChecked        = _config.UseFxaa;
            FullscreenCheck.IsChecked  = _config.Fullscreen;
            VsyncCheck.IsChecked       = _config.Vsync;
            PtBrCheck.IsChecked        = _config.PtBr;
            IntroCheck.IsChecked       = _config.EnableIntro;
            FastShadowsCheck.IsChecked = _config.FastCarShadows;
            EdramCheck.IsChecked       = _config.EdramTiles;
            FenceCheck.IsChecked       = _config.GpuFence;

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
                PlayStatusText.Text = "Jogo pronto para iniciar.";
                StatusDot.Fill = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Green
                PlayButton.IsEnabled = true;
            }
            else if (!exeExists)
            {
                PlayStatusText.Text = "Executável (midnight_club_la.exe) não encontrado.";
                StatusDot.Fill = new SolidColorBrush(Color.FromRgb(229, 57, 53)); // Red
                PlayButton.IsEnabled = false;
            }
            else
            {
                PlayStatusText.Text = "Arquivos não encontrados. Extraia a ISO na aba Instalação.";
                StatusDot.Fill = new SolidColorBrush(Color.FromRgb(255, 152, 0)); // Orange
                PlayButton.IsEnabled = true;
            }
        }

        private void CheckXisoTool()
        {
            string xisoPath = PathHelper.GetExtractXisoPath();
            if (!File.Exists(xisoPath))
            {
                ExtractionStatusText.Text =
                    "extract-xiso.exe não encontrado em:\n" + xisoPath +
                    "\n\nCertifique-se de manter o utilitário na pasta tools\\.";
            }
            else
            {
                ExtractionStatusText.Text =
                    "extract-xiso pronto em:\n" + xisoPath +
                    "\n\nSelecione um arquivo .iso e clique em Extrair Jogo.";
            }

            string xexPath = Path.Combine(PathHelper.GetGameDataPath(), "default.xex");
            if (File.Exists(xexPath))
            {
                ExtractionStatusText.Text =
                    "✔ Jogo já extraído (game\\default.xex presente).\n\n" +
                    "Você pode re-extrair a qualquer momento caso precise reinstalar os arquivos.";
            }
        }

        // ─────────────────────────────────────────────
        //  Save helpers
        // ─────────────────────────────────────────────

        private void SaveConfig()
        {
            if (Radio1x.IsChecked == true)
                _config.ResolutionScale = 1;
            else if (Radio3x.IsChecked == true)
                _config.ResolutionScale = 3;
            else
                _config.ResolutionScale = 2;

            _config.TargetFps       = (RadioFps120.IsChecked == true) ? 120 : 60;
            _config.UseFxaa         = (FxaaCheck.IsChecked == true);
            _config.Fullscreen      = (FullscreenCheck.IsChecked == true);
            _config.Vsync           = (VsyncCheck.IsChecked == true);
            _config.PtBr            = (PtBrCheck.IsChecked == true);
            _config.EnableIntro     = (IntroCheck.IsChecked == true);
            _config.FastCarShadows  = (FastShadowsCheck.IsChecked == true);
            _config.EdramTiles      = (EdramCheck.IsChecked == true);
            _config.GpuFence        = (FenceCheck.IsChecked == true);

            _config.Save();
        }

        // ─────────────────────────────────────────────
        //  Build launch arguments from checkboxes
        // ─────────────────────────────────────────────

        private string BuildArguments()
        {
            var sb = new StringBuilder();

            // PT-BR
            sb.Append(PtBrCheck.IsChecked == true ? "--mcla_ptbr=true" : "--mcla_ptbr=false");

            // FPS
            int fps = (RadioFps120.IsChecked == true) ? 120 : 60;
            sb.AppendFormat(" --mcla_fps={0}", fps);

            // Resolution scale
            int scale = 2;
            if (Radio1x.IsChecked == true) scale = 1;
            else if (Radio3x.IsChecked == true) scale = 3;
            sb.AppendFormat(" --resolution_scale={0}", scale);

            // Intro
            sb.Append(IntroCheck.IsChecked == true ? " --mcla_intro=normal" : " --mcla_intro=fast");

            // Window / Fullscreen
            sb.Append(FullscreenCheck.IsChecked == true ? " --fullscreen=true" : " --fullscreen=false");

            // VSync
            sb.Append(VsyncCheck.IsChecked == true ? " --vsync=true" : " --vsync=false");

            // Anti-Aliasing
            sb.Append(FxaaCheck.IsChecked == true ? " --mcla_anti_aliasing=fxaa" : " --mcla_anti_aliasing=off");

            // Shadows
            sb.Append(FastShadowsCheck.IsChecked == true ? " --mcla_fast_car_shadows=true" : " --mcla_fast_car_shadows=false");

            // eDRAM
            if (EdramCheck.IsChecked == true)
                sb.Append(" --edram_tiles=2048");

            // Fence / Cache
            if (FenceCheck.IsChecked == true)
                sb.Append(" --mcla_cache_flush=fence --mcla_fence_yield=true");

            // Native DirectX 12 fixed pipeline flags
            sb.Append(" --render_target_path_d3d12=rtv");
            sb.Append(" --clear_memory_page_state=false");
            sb.Append(" --readback_resolve=none");
            sb.Append(" --mcla_render_markers=false");
            sb.Append(" --mcla_msaa=false");
            sb.Append(" --mcla_motion_blur=false");
            sb.Append(" --mcla_fullscreen_blur=false");
            sb.Append(" --mcla_depth_of_field=false");
            sb.Append(" --mcla_foliage_shadows=false");

            return sb.ToString();
        }

        // ─────────────────────────────────────────────
        //  Stage / Tab Navigation & Fluid Animations
        // ─────────────────────────────────────────────

        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainTabControl == null) return;

            if (MainTabControl.SelectedItem == TabJogar)
                SwitchStage(PanelJogar);
            else if (MainTabControl.SelectedItem == TabGraficos)
                SwitchStage(PanelGraficos);
            else if (MainTabControl.SelectedItem == TabInstalacao)
                SwitchStage(PanelInstalacao);
            else if (MainTabControl.SelectedItem == TabSobre)
                SwitchStage(PanelSobre);
        }

        private void SwitchStage(FrameworkElement panelToShow)
        {
            if (PanelJogar == null) return;

            PanelJogar.Visibility = Visibility.Collapsed;
            PanelGraficos.Visibility = Visibility.Collapsed;
            PanelInstalacao.Visibility = Visibility.Collapsed;
            PanelSobre.Visibility = Visibility.Collapsed;

            if (panelToShow != null)
            {
                panelToShow.Visibility = Visibility.Visible;

                var transform = new TranslateTransform(0, 8);
                panelToShow.RenderTransform = transform;
                panelToShow.Opacity = 0;

                var animOpacity = new DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(200));
                animOpacity.DecelerationRatio = 0.3;

                var animTranslate = new DoubleAnimation(8.0, 0.0, TimeSpan.FromMilliseconds(200));
                animTranslate.DecelerationRatio = 0.3;

                panelToShow.BeginAnimation(UIElement.OpacityProperty, animOpacity);
                transform.BeginAnimation(TranslateTransform.YProperty, animTranslate);
            }
        }

        // ─────────────────────────────────────────────
        //  Custom Title Bar Events
        // ─────────────────────────────────────────────

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            SaveConfig();
            Close();
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

            // Garantir que vídeos .bik (intros e cutscenes) estejam restaurados e ativos
            try
            {
                string gameDir = System.IO.Path.Combine(PathHelper.GetRepositoryRoot(), "game");
                if (System.IO.Directory.Exists(gameDir))
                {
                    string[] bakFiles = System.IO.Directory.GetFiles(gameDir, "*.bik.bak", System.IO.SearchOption.AllDirectories);
                    foreach (var bak in bakFiles)
                    {
                        string target = bak.Substring(0, bak.Length - 4);
                        if (!System.IO.File.Exists(target))
                            System.IO.File.Move(bak, target);
                    }
                }
            }
            catch (Exception) { }

            string workDir   = PathHelper.GetRepositoryRoot();
            string arguments = BuildArguments();

            PlayNoteText.Text = "Iniciando processo...";
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

            string isoPath   = IsoPathBox.Text.Trim();
            string xisoExe   = PathHelper.GetExtractXisoPath();
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
                    "extract-xiso.exe não encontrado em:\n" + xisoExe,
                    "extract-xiso não encontrado",
                    global::System.Windows.MessageBoxButton.OK, global::System.Windows.MessageBoxImage.Warning);
                return;
            }

            Directory.CreateDirectory(outputDir);

            _extracting = true;
            ExtractButton.IsEnabled   = false;
            ExtractProgress.Visibility = Visibility.Visible;
            ExtractProgress.IsIndeterminate = true;
            ExtractionStatusText.Text = "Extraindo ISO... aguarde.\n(Esse processo pode levar alguns minutos dependendo da velocidade do seu disco)\n";

            SaveConfig();

            Task.Factory.StartNew(() =>
            {
                int    exitCode = -1;
                string stdErr   = string.Empty;
                string stdOut   = string.Empty;

                try
                {
                    if (!Directory.Exists(outputDir))
                        Directory.CreateDirectory(outputDir);

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
                        try
                        {
                            string repoRoot = PathHelper.GetRepositoryRoot();
                            string ptbrScript = Path.Combine(repoRoot, "localization", "build_ptbr.py");
                            if (File.Exists(ptbrScript))
                            {
                                var ptbrPsi = new ProcessStartInfo
                                {
                                    FileName = "python",
                                    Arguments = string.Format("\"{0}\" --game-root \"{1}\" --output \"{1}\"", ptbrScript, outputDir),
                                    WorkingDirectory = repoRoot,
                                    UseShellExecute = false,
                                    CreateNoWindow = true,
                                };
                                using (var ptbrProc = Process.Start(ptbrPsi))
                                {
                                    if (ptbrProc != null) ptbrProc.WaitForExit(120000);
                                }
                            }
                        }
                        catch { }

                        ExtractionStatusText.Text = "Extração concluída com sucesso!";
                        ExtractProgress.Value = 100;
                        global::System.Windows.MessageBox.Show("O jogo foi extraído com sucesso e a estrutura de pastas foi gerada. Você já pode jogar!", "Extração Concluída", global::System.Windows.MessageBoxButton.OK, global::System.Windows.MessageBoxImage.Information);
                        RefreshPlayStatus();
                    }
                    else
                    {
                        ExtractionStatusText.Text = "Falha ao extrair (código " + exitCode.ToString() + ")";
                        global::System.Windows.MessageBox.Show("Falha ao extrair (Código " + exitCode.ToString() + "). Verifique a integridade da ISO.", "Erro de Extração", global::System.Windows.MessageBoxButton.OK, global::System.Windows.MessageBoxImage.Error);
                    }

                    ExtractButton.IsEnabled = true;
                });
            });
        }

        // ─────────────────────────────────────────────
        //  Shortcuts
        // ─────────────────────────────────────────────

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

                global::System.Windows.MessageBox.Show("Atalhos criados com sucesso na Área de Trabalho e no Menu Iniciar!", "Sucesso", global::System.Windows.MessageBoxButton.OK, global::System.Windows.MessageBoxImage.Information);
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
