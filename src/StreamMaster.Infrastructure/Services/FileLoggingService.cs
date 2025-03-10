﻿using System.Text;
using System.Threading.Channels;

using StreamMaster.Domain.Configuration;
using StreamMaster.Domain.Extensions;
namespace StreamMaster.Infrastructure.Services;
public class FileLoggingService : IFileLoggingService, IDisposable
{
    private readonly Channel<string> _logChannel = Channel.CreateUnbounded<string>();
    private static readonly SemaphoreSlim _writeLock = new(1, 1);
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _loggingTask;
    private readonly string _logFilePath;
    private readonly IOptionsMonitor<Setting> _settings;

    public FileLoggingService(string logFilePath, IOptionsMonitor<Setting> intSettings)
    {
        _settings = intSettings;
        _logFilePath = logFilePath;

        // Rotate the log file on startup
        RotateLogOnStartup();

        // Start the logging task
        _loggingTask = Task.Run(ProcessLogChannel);
    }

    public void EnqueueLogEntry(string format, params object[] args)
    {
        string formattedMessage = $"{SMDT.UtcNow:yyyy-MM-dd HH:mm:ss}" + string.Format(format, args);
        _logChannel.Writer.TryWrite(formattedMessage);
    }

    public void EnqueueLogEntry(string logEntry)
    {
        _logChannel.Writer.TryWrite(logEntry);
    }

    private async Task ProcessLogChannel()
    {
        int delay = 100;
        while (await _logChannel.Reader.WaitToReadAsync(_cts.Token).ConfigureAwait(false))
        {
            StringBuilder combinedLogEntries = new();
            while (_logChannel.Reader.TryRead(out string? logEntry))
            {
                if (logEntry != null)
                {
                    combinedLogEntries.AppendLine(logEntry);
                }
            }

            if (combinedLogEntries.Length > 0)
            {
                await WriteLogEntryAsync(combinedLogEntries.ToString()).ConfigureAwait(false);
            }

            // Exponential backoff delay with a maximum of 1000ms
            if (_logChannel.Reader.Count == 0)
            {
                delay = Math.Min(delay * 2, 1000);
            }
            else
            {
                delay = 100; // Reset delay when new entries are available
            }

            await Task.Delay(delay, _cts.Token).ConfigureAwait(false);
        }
    }

    private async Task WriteLogEntryAsync(string logEntry)
    {
        try
        {
            RotateLogIfNeeded();
            await _writeLock.WaitAsync(_cts.Token).ConfigureAwait(false);
            try
            {
                await using FileStream stream = new(_logFilePath, FileMode.Append, FileAccess.Write, FileShare.Read);
                await using StreamWriter writer = new(stream);
                await writer.WriteLineAsync(logEntry).ConfigureAwait(false);
            }
            finally
            {
                _writeLock.Release();
            }
        }
        catch (Exception ex)
        {
            // Log error to console to avoid infinite logging loop
            Console.Error.WriteLine($"Error writing log entry: {ex.Message}");
        }
    }

    private void RotateLogIfNeeded()
    {
        FileInfo logFileInfo = new(_logFilePath);
        long maxFileSizeInBytes = Math.Max(1 * 1024 * 1024, Math.Min(_settings.CurrentValue.MaxLogFileSizeMB * 1024 * 1024, 100 * 1024 * 1024)); // Convert MB to Bytes

        if (logFileInfo.Exists && logFileInfo.Length > maxFileSizeInBytes && !string.IsNullOrEmpty(logFileInfo.DirectoryName))
        {
            RotateLogFiles(logFileInfo);
        }
    }

    private void RotateLogOnStartup()
    {
        FileInfo logFileInfo = new(_logFilePath);
        if (logFileInfo.Exists && !string.IsNullOrEmpty(logFileInfo.DirectoryName))
        {
            RotateLogFiles(logFileInfo);
        }
    }

    private void RotateLogFiles(FileInfo logFileInfo)
    {
        if (string.IsNullOrEmpty(logFileInfo.DirectoryName))
        {
            throw new Exception("RotateLogFiles DirectoryName is null");
        }
        string directory = logFileInfo.DirectoryName;
        string baseFileName = Path.GetFileNameWithoutExtension(logFileInfo.FullName);
        string extension = logFileInfo.Extension;

        for (int i = _settings.CurrentValue.MaxLogFiles - 1; i >= 1; i--)
        {
            string oldFileName = Path.Combine(directory, $"{baseFileName}.{i}{extension}");
            string newFileName = Path.Combine(directory, $"{baseFileName}.{i + 1}{extension}");

            if (File.Exists(newFileName))
            {
                File.Delete(newFileName);
            }

            if (File.Exists(oldFileName))
            {
                File.Move(oldFileName, newFileName);
            }
        }

        string rotatedFileName = Path.Combine(directory, $"{baseFileName}.1{extension}");
        File.Move(_logFilePath, rotatedFileName);

        // Create a new, empty log file
        using (FileStream fs = File.Create(_logFilePath)) { }

        // Clean up old logs asynchronously
        _ = Task.Run(() => CleanUpOldLogFiles(logFileInfo));
    }

    private void CleanUpOldLogFiles(FileInfo logFileInfo)
    {
        string? directory = logFileInfo.DirectoryName;
        string baseFileName = Path.GetFileNameWithoutExtension(logFileInfo.FullName);
        string extension = logFileInfo.Extension;

        if (directory != null)
        {
            DirectoryInfo di = new(directory);
            FileInfo[] logFiles = [.. di.GetFiles($"{baseFileName}.*{extension}").OrderByDescending(f => f.Name)];

            if (logFiles.Length > _settings.CurrentValue.MaxLogFiles)
            {
                foreach (FileInfo file in logFiles.Skip(_settings.CurrentValue.MaxLogFiles))
                {
                    file.Delete();
                }
            }
        }
    }

    public async Task StopLoggingAsync()
    {
        _cts.Cancel();
        try
        {
            await _loggingTask.ConfigureAwait(false);
        }
        finally
        {
            // Flush remaining logs
            while (_logChannel.Reader.TryRead(out string? logEntry))
            {
                await WriteLogEntryAsync(logEntry).ConfigureAwait(false);
            }
        }
    }

    public void Dispose()
    {
        _cts.Dispose();
        _writeLock.Dispose();
        GC.SuppressFinalize(this);
    }
}