
Option Strict On
Imports System.Security.Cryptography.SHA256
Imports System.Text

Namespace Crypto
    Public Class Hashing
        Public Shared Function Sha256(text As String) As String
            Dim bytes = Encoding.UTF8.GetBytes(text)
            Dim hash = HashData(bytes)
            Return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant()
        End Function
    End Class
End Namespace