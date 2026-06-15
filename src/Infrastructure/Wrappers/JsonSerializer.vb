Option Strict On
Imports System.Text.Json

Namespace Wrappers
    Public Class JsonSerializerWrapper
        Public Shared Function Serialize(Of T)(obj As T) As String
            Return JsonSerializer.Serialize(obj)
        End Function
        Public Shared Function Deserialize(Of T)(json As String) As T
            Return JsonSerializer.Deserialize(Of T)(json)
        End Function
    End Class
End Namespace