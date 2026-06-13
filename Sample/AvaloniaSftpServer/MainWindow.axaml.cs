// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the MIT License. See LICENSE file in the repository root for full license text.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;
using ApacheMinaSSHD.NET.Wrapper;
using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;
using ApacheMinaSSHD.NET.Wrapper.Factories;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AvaloniaSftpServer;

public partial class MainWindow : Window
{
    private AMNetSshServer? _server;
    private readonly ObservableCollection<LogEntry> _log = [];
    private readonly ObservableCollection<SessionInfo> _sessions = [];
    private readonly System.Timers.Timer _uiTimer = new(1000);
    private DateTime _startTime;

    public MainWindow()
    {
        InitializeComponent();
        LogList.ItemsSource = _log;
        SessionGrid.ItemsSource = _sessions;
        _uiTimer.Elapsed += OnTimerTick;
        Log("Application started");
    }

    private void OnStartClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var host = HostBox.Text ?? "127.0.0.1";
            if (!int.TryParse(PortBox.Text, out var port)) port = 2222;

            _server = AMNetSshServer.SetUpDefaultServer();
            _server.Host = host;
            _server.Port = port;

            if (ApplyProductionDefaults.IsChecked == true)
                _server.Config.ApplyProductionDefaults();
            if (ApplyModernAlgorithms.IsChecked == true)
                _server.Config.ApplyModernAlgorithmDefaults();

            _server.Config.WELCOME_BANNER = "ApacheMinaSSHD.NET — Avalonia Manager";

            _server.SetFixedPasswordAuthenticator("admin", PasswordBox.Text ?? "changeme");
            _server.setKeyPairProvider(new AMNetSimpleGeneratorHostKeyProvider(HostKeyBox.Text ?? "hostkey.ser"));

            var root = StorageRootBox.Text ?? "sftp-storage";
            EnsureDirectory(root);
            _server.setFileSystemFactory(new AMNetVirtualFileSystemFactory(root));

            if (EnabledSCP.IsChecked == true)
                _server.setCommandFactory(new AMNetScpCommandFactory(new AMNetScpFileOpener(root)));

            _server.addSessionListener(new ManagerSessionListener(this));
            _server.setIoServiceEventListener(new ManagerIoListener(this));

            _server.Start();
            _startTime = DateTime.UtcNow;

            StartBtn.IsEnabled = false;
            StopBtn.IsEnabled = true;
            StatusText.Text = "Running";
            StatusText.Foreground = Avalonia.Media.Brushes.LimeGreen;
            _uiTimer.Start();
            Log($"Server started on {host}:{port}");
        }
        catch (Exception ex)
        {
            Log($"Failed to start: {ex.Message}");
        }
    }

    private void OnStopClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            _uiTimer.Stop();
            _server?.Stop();
            _server = null;

            StartBtn.IsEnabled = true;
            StopBtn.IsEnabled = false;
            StatusText.Text = "Stopped";
            StatusText.Foreground = Avalonia.Media.Brushes.Gray;
            UptimeText.Text = "";
            Log("Server stopped");
        }
        catch (Exception ex)
        {
            Log($"Error stopping: {ex.Message}");
        }
    }

    private void OnClearLogClick(object? sender, RoutedEventArgs e)
    {
        _log.Clear();
    }

    private void OnTimerTick(object? sender, ElapsedEventArgs e)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            if (_server != null)
            {
                var elapsed = DateTime.UtcNow - _startTime;
                UptimeText.Text = $"Uptime: {elapsed.Hours}h {elapsed.Minutes}m {elapsed.Seconds}s";
            }
        });
    }

    internal void Log(string message)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            _log.Add(new LogEntry(message));
            if (_log.Count > 500)
                _log.RemoveAt(0);
            if (LogList.ItemCount > 0)
                LogList.ScrollIntoView(_log[^1]);
        });
    }

    internal void AddSession(ISshSession session)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            _sessions.Add(new SessionInfo(session));
            SessionCount.Text = $"{_sessions.Count} active sessions";
        });
    }

    internal void RemoveSession(ISshSession session)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            var info = _sessions.FirstOrDefault(s => s.Id == session.SessionId);
            if (info != null)
                _sessions.Remove(info);
            SessionCount.Text = $"{_sessions.Count} active sessions";
        });
    }

    private static void EnsureDirectory(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }
}

public class LogEntry(string text)
{
    public string Timestamp { get; } = DateTime.Now.ToString("HH:mm:ss");
    public string Text { get; } = text;
    public override string ToString() => $"[{Timestamp}] {Text}";
}

public class SessionInfo(ISshSession session) : INotifyPropertyChanged
{
    public Guid Id => session.SessionId;
    public string RemoteAddress => session.RemoteAddress ?? "unknown";
    public string StartedAt => DateTime.Now.ToString("HH:mm:ss");

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

class ManagerSessionListener(MainWindow window) : AMNetSessionListener
{
    public override void OnSessionCreated(ISshSession session)
    {
        window.Log($"Session opened: {session.RemoteAddress}");
        window.AddSession(session);
    }

    public override void OnSessionClosed(ISshSession session)
    {
        window.Log($"Session closed: {session.RemoteAddress}");
        window.RemoveSession(session);
    }
}

class ManagerIoListener(MainWindow window) : AMNetIoServiceEventListener
{
    public override bool OnConnectionAccepted(ISshServiceConnection context)
    {
        window.Log($"Connection accepted: {context.RemoteEndPoint}");
        return true;
    }

    public override void OnConnectionAborted(ISshServiceConnection context)
    {
        window.Log($"Connection aborted: {context.RemoteEndPoint}");
    }
}
