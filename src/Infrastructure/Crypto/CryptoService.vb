
Option Strict On
Imports System.Security.Cryptography
Imports System.IO

Namespace Crypto
    Public Class CryptoService
        ' AES-GCM
        Public Sub EncryptFile(inputPath As String, outputPath As String, key As Byte(), nonce As Byte())
            Dim plaintext = File.ReadAllBytes(inputPath)
            Dim tag(15) As Byte
            Dim ciphertext(plaintext.Length - 1) As Byte

            Using aesgcm As New AesGcm(key)
                aesgcm.Encrypt(nonce, plaintext, ciphertext, tag)
            End Using

            Using fs As New FileStream(outputPath, FileMode.Create, FileAccess.Write)
                fs.Write(nonce, 0, nonce.Length)
                fs.Write(tag, 0, tag.Length)
                fs.Write(ciphertext, 0, ciphertext.Length)
            End Using
        End Sub

        Public Sub DecryptFile(inputPath As String, outputPath As String, key As Byte())
            Dim all = File.ReadAllBytes(inputPath)
            Dim nonce = all.Take(12).ToArray()
            Dim tag = all.Skip(12).Take(16).ToArray()
            Dim ciphertext = all.Skip(28).ToArray()
            Dim plaintext(ciphertext.Length - 1) As Byte

            Using aesgcm As New AesGcm(key)
                aesgcm.Decrypt(nonce, ciphertext, tag, plaintext)
            End Using
            File.WriteAllBytes(outputPath, plaintext)
        End Sub
    End Class
End Namespace
