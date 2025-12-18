using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace soundPlayer;

public partial class MainWindow : Window
{
    private readonly AudioEngine _engine = new();

    // --- DEVELOPER OPTIONS (DEFAULTS) ---
    private string _rootPath = @"C:\Windows\Media\.sounds"; // Default Home
    private string _startupPath = @"C:\Windows\Media\.sounds"; // Default Start
    private bool _allowMaximize = false; // Default: No maximize
    private bool _startInSingleMode = true; // Default: Checked

    private string _currentPath;

    // --- WIN32 API ---
    private const int GWL_STYLE = -16;
    private const int WS_MAXIMIZEBOX = 0x10000;

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hwnd, int index);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

    public MainWindow()
    {
        // 1. Načíst argumenty PŘED startem UI
        ProcessArgs();

        InitializeComponent();

        // 2. Aplikovat nastavení do UI
        _currentPath = _startupPath;
        SingleModeCheckbox.IsChecked = _startInSingleMode;

        // Pokud je povolen maximize, ukážeme tlačítko
        if (_allowMaximize)
        {
            BtnMaximize.Visibility = Visibility.Visible;
        }

        Loaded += async (_, _) => await NavigateTo(_startupPath);
        Closing += (_, _) => _engine.Dispose();
    }

    private void ProcessArgs()
    {
        string[] args = Environment.GetCommandLineArgs();

        // Procházíme argumenty (index 0 je cesta k exe, začínáme od 1)
        for (int i = 1; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "-start": // Nastaví startovní složku
                    if (i + 1 < args.Length) _startupPath = args[++i];
                    break;
                case "-home": // Nastaví domovskou složku
                    if (i + 1 < args.Length) _rootPath = args[++i];
                    break;
                case "-multi": // Vypne "One at a time" (odškrtne checkbox)
                    _startInSingleMode = false;
                    break;
                case "-max": // Povolí maximalizaci
                    _allowMaximize = true;
                    break;
            }
        }
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        // POUZE pokud NENÍ povolen maximize, odebereme styl okna (Anti-Snap)
        // Pokud je povolen (-max), necháme Windows ať si to řídí.
        if (!_allowMaximize)
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            int currentStyle = GetWindowLong(hwnd, GWL_STYLE);
            SetWindowLong(hwnd, GWL_STYLE, currentStyle & ~WS_MAXIMIZEBOX);
        }
    }

    // --- UI EVENT HANDLERS ---

    private void OnStopAllClick(object sender, RoutedEventArgs e)
    {
        _engine.StopAll();
        NotificationStack.Children.Clear();
    }

    private async void OnHomeClick(object sender, RoutedEventArgs e)
    {
        ErrorPanel.Visibility = Visibility.Collapsed;
        SoundList.Visibility = Visibility.Visible;

        if (Directory.Exists(_rootPath)) await NavigateTo(_rootPath);
        else await NavigateTo(@"C:\");
    }

    private void OnMinimizeClick(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void OnCloseClick(object sender, RoutedEventArgs e) => Close();

    // Nová metoda pro Maximize (pokud je povolen)
    private void OnMaximizeClick(object sender, RoutedEventArgs e)
    {
        WindowState = (WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
    }

    // --- PLAYBACK LOGIC ---

    private async void PlayItem(FileSystemItem item)
    {
        if (item.Type == ItemType.File)
        {
            if (SingleModeCheckbox.IsChecked == true)
            {
                _engine.StopAll();
                NotificationStack.Children.Clear();
            }

            _engine.Preload(item.Path);
            _engine.Play(item.Path);
            ShowNotification(item.Name);
        }
        else
        {
            await NavigateTo(item.Path);
        }
    }

    private void OnItemMouseClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is ListBoxItem { DataContext: FileSystemItem item })
        {
            PlayItem(item);
        }
    }

    private void OnItemKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (sender is ListBoxItem { DataContext: FileSystemItem item })
            {
                PlayItem(item);
                e.Handled = true;
            }
        }
    }

    // --- NAVIGATION ---

    private async Task NavigateTo(string path)
    {
        if (!Directory.Exists(path))
        {
            SoundList.Visibility = Visibility.Collapsed;
            ErrorPanel.Visibility = Visibility.Visible;
            ErrorDetailText.Text = $"Path does not exist:\n{path}";
            CurrentPathText.Text = path;
            _currentPath = path;
            return;
        }

        ErrorPanel.Visibility = Visibility.Collapsed;
        SoundList.Visibility = Visibility.Visible;

        _currentPath = path;
        CurrentPathText.Text = _currentPath;

        var items = await Task.Run(() =>
        {
            List<FileSystemItem> list = [];
            DirectoryInfo parent = null;
            try { parent = Directory.GetParent(path); } catch { }

            if (parent != null)
                list.Add(new FileSystemItem("..", parent.FullName, ItemType.Back, "UP"));

            try
            {
                foreach (var dir in Directory.GetDirectories(path))
                    list.Add(new FileSystemItem(System.IO.Path.GetFileName(dir), dir, ItemType.Folder, "DIR"));
            }
            catch { }

            try
            {
                foreach (var file in Directory.GetFiles(path, "*.wav"))
                {
                    string ext = System.IO.Path.GetExtension(file).TrimStart('.').ToUpper();
                    list.Add(new FileSystemItem(System.IO.Path.GetFileNameWithoutExtension(file), file, ItemType.File, ext));
                }
            }
            catch { }

            return list;
        });

        SoundList.ItemsSource = items;

        if (SoundList.Items.Count > 0)
        {
            SoundList.SelectedIndex = 0;
            SoundList.Focus();
        }
    }

    // --- NOTIFICATIONS (Nezměněno) ---
    private void ShowNotification(string soundName)
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(230, 32, 32, 32)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(64, 64, 64)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(10, 6, 12, 6),
            Margin = new Thickness(0, 3, 0, 3),
            Opacity = 0,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        var shadow = new DropShadowEffect
        {
            BlurRadius = 8,
            ShadowDepth = 2,
            Opacity = 0.4,
            RenderingBias = RenderingBias.Performance
        };
        border.Effect = shadow;

        var stack = new StackPanel { Orientation = Orientation.Horizontal };

        var iconPath = new System.Windows.Shapes.Path
        {
            Data = (Geometry)FindResource("Icon_Audio"),
            Fill = new SolidColorBrush(Color.FromRgb(216, 160, 32)),
            Width = 10,
            Height = 10,
            Stretch = Stretch.Uniform,
            Margin = new Thickness(0, 0, 8, 0)
        };

        var text = new TextBlock
        {
            Text = soundName,
            Foreground = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
            FontWeight = FontWeights.Normal,
            FontSize = 11,
            FontFamily = new FontFamily("Segoe UI")
        };

        stack.Children.Add(iconPath);
        stack.Children.Add(text);
        border.Child = stack;

        NotificationStack.Children.Add(border);

        var anim = new DoubleAnimationUsingKeyFrames { Duration = TimeSpan.FromSeconds(2.0) };
        anim.KeyFrames.Add(new LinearDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.2))));
        anim.KeyFrames.Add(new LinearDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1.5))));
        anim.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(2.0))));

        anim.Completed += (s, e) => NotificationStack.Children.Remove(border);
        border.BeginAnimation(UIElement.OpacityProperty, anim);
    }
}

// --- DATA MODELY ---
public enum ItemType { File, Folder, Back }
public record FileSystemItem(string Name, string Path, ItemType Type, string Extension);

// --- AUDIO ENGINE ---
public class AudioEngine : IDisposable
{
    private readonly WaveOutEvent _outputDevice;
    private readonly MixingSampleProvider _mixer;
    private readonly Dictionary<string, CachedSound> _cache = [];
    private readonly List<StoppableSampleProvider> _activeSounds = [];
    private readonly WaveFormat _format = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);

    public AudioEngine()
    {
        _outputDevice = new WaveOutEvent { DesiredLatency = 50 };
        _mixer = new MixingSampleProvider(_format) { ReadFully = true };
        _outputDevice.Init(_mixer);
        _outputDevice.Play();
    }
    public void Play(string path)
    {
        if (_cache.TryGetValue(path, out var sound))
        {
            _activeSounds.RemoveAll(x => x.HasFinished);
            var stoppable = new StoppableSampleProvider(new CachedSoundProvider(sound));
            _activeSounds.Add(stoppable);
            _mixer.AddMixerInput(stoppable);
        }
    }
    public void StopAll()
    {
        foreach (var sound in _activeSounds) sound.Stop();
        _activeSounds.Clear();
    }
    public void Preload(string path)
    {
        if (_cache.ContainsKey(path)) return;
        try
        {
            using var reader = new AudioFileReader(path);
            var resampler = new WdlResamplingSampleProvider(reader, _format.SampleRate);
            var stereo = resampler.ToStereo();
            List<float> data = [];
            var buffer = new float[stereo.WaveFormat.SampleRate * stereo.WaveFormat.Channels];
            int read;
            while ((read = stereo.Read(buffer, 0, buffer.Length)) > 0)
                data.AddRange(buffer.Take(read));
            _cache[path] = new CachedSound(data.ToArray(), stereo.WaveFormat);
        }
        catch { }
    }
    public void Dispose()
    {
        _outputDevice.Stop();
        _outputDevice.Dispose();
    }
}
public record CachedSound(float[] Data, WaveFormat Format);
public class CachedSoundProvider(CachedSound sound) : ISampleProvider
{
    private long _position;
    public WaveFormat WaveFormat => sound.Format;
    public int Read(float[] buffer, int offset, int count)
    {
        var available = sound.Data.Length - _position;
        var toCopy = Math.Min(available, count);
        Array.Copy(sound.Data, _position, buffer, offset, toCopy);
        _position += toCopy;
        return (int)toCopy;
    }
}
public class StoppableSampleProvider(ISampleProvider source) : ISampleProvider
{
    private bool _stopRequested = false;
    public bool HasFinished { get; private set; } = false;
    public WaveFormat WaveFormat => source.WaveFormat;
    public void Stop() => _stopRequested = true;
    public int Read(float[] buffer, int offset, int count)
    {
        if (_stopRequested) { HasFinished = true; return 0; }
        int read = source.Read(buffer, offset, count);
        if (read == 0) HasFinished = true;
        return read;
    }
}