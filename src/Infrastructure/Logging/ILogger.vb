
Option Strict On

Namespace Logging
    Public Interface ILogger
        Sub Info(message As String)
        Sub Warn(message As String)
        Sub Debug(message As String)
        Sub [Error](message As String, Optional ex As Exception = Nothing)
    End Interface
End Namespace