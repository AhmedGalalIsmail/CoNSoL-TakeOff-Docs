
Option Strict On

Namespace Logging
    ''' <summary>
    ''' Defines a simple logging interface for the application, allowing for different logging implementations (e.g., console, file, external services).
    ''' </summary>
    Public Interface ILogger
        ''' <summary>
        ''' Logs an informational message, typically used for general application flow and state information.
        ''' </summary>
        ''' <param name="message"></param>
        Sub Info(message As String)

        ''' <summary>
        ''' Logs a warning message, indicating a potential issue or important event that does not necessarily indicate a failure but should be noted.
        ''' </summary>
        ''' <param name="message"></param>
        Sub Warn(message As String)

        ''' <summary>
        ''' Logs a debug message, used for detailed diagnostic information that is typically only relevant during development or troubleshooting.
        ''' </summary>
        ''' <param name="message"></param>
        Sub Debug(message As String)

        ''' <summary>
        ''' Logs an error message, indicating a failure or issue that has occurred, optionally including an exception for more context.
        ''' </summary>
        ''' <param name="message"></param>
        ''' <param name="ex"></param>
        Sub [Error](message As String, Optional ex As Exception = Nothing)
    End Interface
End Namespace
