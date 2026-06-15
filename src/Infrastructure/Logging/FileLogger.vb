'Filename: Infrastructure/Logging/FileLogger.vb
Option Strict On
Imports System.IO
Imports System.Text

Namespace Logging
    ''' <summary>
    ''' File-based logger implementation.
    ''' Writes log messages to disk with timestamp and severity level.
    ''' </summary>
    ''' <remarks>
    ''' Logs are written to: AppDomain.CurrentDomain.BaseDirectory\logs\console-takeoff.log
    ''' 
    ''' Severity Levels:
    ''' - DEBUG: Detailed diagnostic information
    ''' - INFO: General informational messages
    ''' - WARNING: Warning conditions (non-critical)
    ''' - ERROR: Error conditions (recoverable)
    ''' - FATAL: Fatal conditions (unrecoverable)
    ''' 
    ''' Thread-safe: Uses lock to ensure serialized writes.
    ''' </remarks>
    Public Class FileLogger
        Implements ILogger


        Private ReadOnly _logDir As String
        Private ReadOnly _appName As String

        'Private ReadOnly _logDir As String
        'Private ReadOnly _logPath As String
        'Private ReadOnly _appName As String
        'Private ReadOnly _lock As New Object()

        ''' <summary>Initializes logger with specified log file path.</summary>
        ''' <param name="logPath">Path to log file</param>
        Public Sub New(appName As String, logDir As String)
            _appName = appName
            _logDir = logDir
            ' Create directory if not exists
            If Not Directory.Exists(_logDir) Then
                Directory.CreateDirectory(_logDir)
            End If
        End Sub

        ''' <summary> Logs an informational message.</summary>
        ''' <param name="message">Message to log</param>
        ''' <remarks>Used for normal operational events.</remarks>
        Public Sub Info(message As String) Implements ILogger.Info
            Write("INFO", message)
        End Sub

        ''' <summary> Logs a warning message, which indicates a potential issue or something that should be noted but does not prevent the application from functioning.</summary>
        ''' <param name="message"></param>
        Public Sub Warn(message As String) Implements ILogger.Warn
            Write("WARN", message)
        End Sub

        'Debug
        ''' <summary>Logs a debug message, which is typically used for detailed diagnostic information that may be useful during development or troubleshooting, but is not usually needed in production environments.</summary>
        ''' <param name="message"></param>
        Public Sub Debug(message As String) Implements ILogger.Debug
            Write("Debug", message)
        End Sub

        ''' <summary>Logs an error message.</summary>
        ''' <param name="message">Error message to log</param>
        ''' <remarks>Used when an error occurs but operation can continue.</remarks>
        Public Sub [Error](message As String, Optional ex As Exception = Nothing) Implements ILogger.Error
            Dim full = If(ex Is Nothing, message, $"{message} | {ex.Message}")
            Write("ERROR", full)
        End Sub

        ''' <summary>
        ''' Writes a log entry to the file with the specified severity level and message.
        ''' </summary>
        ''' <param name="level"></param>
        ''' <param name="text"></param>
        Private Sub Write(level As String, message As String)
            ' Log file name includes date for daily rotation: logs/console-takeoff_20240601.log
            Dim file = Path.Combine(_logDir, $"{_appName}_{DateTime.UtcNow:yyyyMMdd}.log")
            ' Log format: 2024-06-01T12:34:56.789Z [INFO] Message text
            Dim line = $"{DateTime.UtcNow:O} [{level}] {message}"
            SyncLock Me
                Try
                    System.IO.File.AppendAllText(file, line & Environment.NewLine, Encoding.UTF8)
                Catch ex As Exception
                    ' Silent fail - don't throw on logging errors
                    System.Diagnostics.Debug.WriteLine($"Failed to write log: {ex.Message}")
                End Try
            End SyncLock
        End Sub

        ' Old code had a bug where it did not handle exceptions during file writing, which could crash the application. Fixed by adding try-catch around file write operation.
        'Private Sub Write(level As String, text As String)
        '    ' Log file name includes date for daily rotation: logs/console-takeoff_20240601.log
        '    Dim file = Path.Combine(_logDir, $"{_appName}_{DateTime.UtcNow:yyyyMMdd}.log")
        '    ' Log format: 2024-06-01T12:34:56.789Z [INFO] Message text
        '    Dim line = $"{DateTime.UtcNow:O} [{level}] {text}"
        '    SyncLock GetType(FileLogger)
        '        System.IO.File.AppendAllText(file, line & Environment.NewLine, Encoding.UTF8)
        '    End SyncLock
        'End Sub

        ' Old code had a bug where it used a static lock object, which could cause contention across multiple logger instances. Fixed by using SyncLock on the instance (Me) instead of a shared lock object.
        ''' <summary>Writes a log entry to the file with the specified severity level and message.</summary><param name="level"></param><param name="message"></param>
        'Private Sub WriteLog(level As String, message As String)
        '    SyncLock _lock
        '        Try
        '            ' Log format: 2024-06-01 12:34:56 [INFO] Message text
        '            Dim logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}"
        '            ' Ensure log file is created in the specified directory
        '            File.AppendAllText(_logPath, logEntry & vbCrLf)
        '        Catch ex As Exception
        '            ' Silent fail - don't throw on logging errors
        '            System.Diagnostics.Debug.WriteLine($"Failed to write log: {ex.Message}")
        '        End Try
        '    End SyncLock
        'End Sub
    End Class
End Namespace
