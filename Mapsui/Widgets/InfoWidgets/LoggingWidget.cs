﻿using Mapsui.Logging;
using Mapsui.Styles;
using Mapsui.Widgets.BoxWidgets;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Mapsui.Widgets.InfoWidgets;

/// <summary>
/// Widget which shows log entries
/// </summary>
/// <remarks>
/// With this, the user could see the log entries on the screen.
/// without saving them to a file or somewhere else.
/// </remarks>
public class LoggingWidget : TextBoxWidget
{
    public struct LogEntry
    {
        public LogLevel LogLevel;
        public string Description;
        public Exception? Exception;
    }

    public LoggingWidget()
    {
        _listOfLogEntries = new ConcurrentQueue<LogEntry>();

        UpdateNumOfLogEntries();

        // Add event handle, so that LoggingWidget gets all logs
        Logger.LogDelegate += Log;

#if DEBUG
        Enabled = true;
#else
        Enabled = false;
#endif

        Width = 250;
        Height = 142;
    }

    /// <summary>
    ///  Event handler for logging
    /// </summary>
    public void Log(LogLevel level, string description, Exception? exception)
    {
        if (level > LogLevelFilter)
            return;

        var entry = new LogEntry { LogLevel = level, Description = description, Exception = exception };

        _listOfLogEntries.Enqueue(entry);

        while (_listOfLogEntries.Count > _maxNumOfLogEntries)
        {
            _listOfLogEntries.TryDequeue(out var outObj);
        }

        OnPropertyChanged(nameof(Text));
    }

    public void Clear()
    {
        while (_listOfLogEntries.Count > 0)
        {
            _listOfLogEntries.TryDequeue(out var outObj);
        }

        OnPropertyChanged(nameof(Text));
    }

    private int _maxNumOfLogEntries;

    private ConcurrentQueue<LogEntry> _listOfLogEntries;

    public ConcurrentQueue<LogEntry> ListOfLogEntries
    {
        get
        {
            return _listOfLogEntries;
        }
    }

    private LogLevel _logLevelFilter = LogLevel.Information;

    /// <summary>
    /// Filter for LogLevel
    /// Only this or higher levels are printed
    /// </summary>
    public LogLevel LogLevelFilter
    {
        get => _logLevelFilter;
        set
        {
            if (_logLevelFilter == value)
                return;
            _logLevelFilter = value;
            OnPropertyChanged();
        }
    }

    private Color _errorTextColor = Color.Red;

    /// <summary>
    /// Color for errors
    /// </summary>
    public Color ErrorTextColor
    {
        get => _errorTextColor;
        set
        {
            if (_errorTextColor == value)
                return;
            _errorTextColor = value;
            OnPropertyChanged();
        }
    }

    private Color _warningTextColor = Color.Orange;

    /// <summary>
    /// Color for warnings
    /// </summary>
    public Color WarningTextColor
    {
        get => _warningTextColor;
        set
        {
            if (_warningTextColor == value)
                return;
            _warningTextColor = value;
            OnPropertyChanged();
        }
    }

    private Color _informationTextColor = Color.Black;

    /// <summary>
    /// Color for information text
    /// </summary>
    public Color InformationTextColor
    {
        get => _informationTextColor;
        set
        {
            if (_informationTextColor == value)
                return;
            _informationTextColor = value;
            OnPropertyChanged();
        }
    }
    private void UpdateNumOfLogEntries()
    {
        var newNumOfLogEntries = (int)((Height - PaddingY) / (TextSize + PaddingY));

        while (_listOfLogEntries.Count > newNumOfLogEntries)
        {
            _listOfLogEntries.TryDequeue(out var outObj);
        }

        _maxNumOfLogEntries = newNumOfLogEntries;

        OnPropertyChanged(nameof(Text));
    }

    private void UpdateLogEntries()
    {
        var entries = _listOfLogEntries.ToList();
        var pos = 0;

        while (pos < entries.Count)
        {
            if (entries[pos].LogLevel > LogLevelFilter)
            {
                entries.Remove(entries[pos]);
            }
            else
            {
                pos++;
            }
        }

        Clear();

        foreach (var entry in entries)
        {
            _listOfLogEntries.Enqueue(entry);
        }
    }

    public override void OnPropertyChanged([CallerMemberName] string name = "")
    {
        if (name == nameof(TextSize) || name == nameof(PaddingY) || name == nameof(Height))
            UpdateNumOfLogEntries();

        if (name == nameof(MarginX) || name == nameof(MarginY) || name == nameof(Width) || name == nameof(Height))
            Envelope = new MRect(MarginX, MarginY, MarginX + Width, MarginY + Height);

        if (name == nameof(Enabled))
        {
            if (Enabled)
                Logger.LogDelegate += Log;
            else
                Logger.LogDelegate -= Log;
        }

        if (name == nameof(LogLevelFilter))
            UpdateLogEntries();

        base.OnPropertyChanged(name);
    }
}
