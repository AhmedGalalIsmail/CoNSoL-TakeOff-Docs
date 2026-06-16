
Option Strict On
Imports System.Text.Json
Imports Domain.Entities
Imports Infrastructure.Crypto


Namespace IO
    ''' <summary>
    ''' Handles saving and loading of canvas layouts, with optional encryption for security.
    ''' </summary>
    Public Class TakeOffFileStore
        Private ReadOnly _crypto As CryptoService

        ''' <summary>
        ''' Initialize with optional CryptoService for encryption support.
        ''' </summary>
        ''' <param name="crypto"></param>
        Public Sub New(Optional crypto As CryptoService = Nothing)
            _crypto = crypto
        End Sub

        ''' <summary>
        ''' Saves the canvas layout to a file, optionally encrypting it for security.
        ''' </summary>
        ''' <param name="path"></param>
        ''' <param name="layout"></param>
        ''' <param name="encrypt"></param>
        ''' <param name="nonce"></param>
        Public Sub Save(path As String, layout As CanvasLayout, Optional encrypt As Boolean = False, Optional nonce As Byte() = Nothing)
            Dim json = JsonSerializer.Serialize(layout, New JsonSerializerOptions With {.WriteIndented = True})
            Dim temp = path & ".tmp"
            System.IO.File.WriteAllText(temp, json)

            If encrypt AndAlso _crypto IsNot Nothing Then
                Dim key = DeriveKey()
                _crypto.EncryptFile(temp, path, key, If(nonce, New Byte(11) {}))
                System.IO.File.Delete(temp)
            Else
                System.IO.File.Copy(temp, path, True)
                System.IO.File.Delete(temp)
            End If
        End Sub

        ''' <summary>
        ''' Loads the canvas layout from a file, decrypting it if necessary.
        ''' </summary>
        ''' <param name="path"></param>
        ''' <param name="encrypted"></param>
        ''' <returns>
        ''' The deserialized CanvasLayout object representing the saved state of the canvas.
        ''' </returns>
        Public Function Load(path As String, Optional encrypted As Boolean = False) As CanvasLayout
            Dim json As String
            If encrypted AndAlso _crypto IsNot Nothing Then
                Dim key = DeriveKey()
                Dim temp = path & ".dec"
                _crypto.DecryptFile(path, temp, key)
                json = System.IO.File.ReadAllText(temp)
                System.IO.File.Delete(temp)
            Else
                json = System.IO.File.ReadAllText(path)
            End If
            Return JsonSerializer.Deserialize(Of CanvasLayout)(json)
        End Function

        ''' <summary>
        ''' Derives a cryptographic key from a password using PBKDF2 with a fixed salt and iteration count.
        ''' </summary>
        ''' <returns>
        ''' A byte array containing the derived key, suitable for use in encryption and decryption operations.
        ''' </returns>
        Private Function DeriveKey() As Byte()
            Dim salt = System.Text.Encoding.UTF8.GetBytes("CoNSoL-Salt")
            Using kdf As New System.Security.Cryptography.Rfc2898DeriveBytes("password123", salt, 100000, System.Security.Cryptography.HashAlgorithmName.SHA256)
                Return kdf.GetBytes(32)
            End Using
        End Function
    End Class
End Namespace
