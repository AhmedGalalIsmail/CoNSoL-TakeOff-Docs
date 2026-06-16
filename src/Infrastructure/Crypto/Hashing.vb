
Option Strict On
Imports System.Security.Cryptography.SHA256
Imports System.Text

Namespace Crypto
    ''' <summary>
    ''' Provides hashing utilities for cryptographic operations.
    ''' </summary>
    Public Class Hashing
        ''' <summary>Computes the SHA-256 hash of the given text and returns it as a lowercase hexadecimal string.</summary>
        ''' <param name="text"></param>
        ''' <returns>A lowercase hexadecimal string representing the SHA-256 hash of the input text.</returns>
        Public Shared Function Sha256(text As String) As String
            Dim bytes = Encoding.UTF8.GetBytes(text)
            Dim hash = HashData(bytes)
            Return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant()
        End Function
    End Class
End Namespace
