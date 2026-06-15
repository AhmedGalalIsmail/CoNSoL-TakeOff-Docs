Option Strict On
Imports System.Text.Json
Imports Domain.Entities.BlockModels
Imports Domain.Entities
Imports System.IO

Namespace IO
    Public Class MaterialJsonStore
        Private ReadOnly _path As String
        Private ReadOnly _filePath As String
        Public Sub New(path As String, Optional filePath As String = "")
            _path = path
            If Not File.Exists(_path) Then
                SaveAll(New List(Of Material))
            End If
        End Sub

        Public Sub New(filePath As String)
            _filePath = filePath
            EnsureFile()
        End Sub

        Private Sub EnsureFile()
            If Not File.Exists(_filePath) Then
                SaveAll(New List(Of Material))
            End If
        End Sub

        Public Function LoadAll() As List(Of Material)
            Dim json = File.ReadAllText(_path)
            Return JsonSerializer.Deserialize(Of List(Of Material))(json)
        End Function

        Public Sub SaveAll(materials As List(Of Material))
            Dim json = JsonSerializer.Serialize(materials, New JsonSerializerOptions With {.WriteIndented = True})
            File.WriteAllText(_path, json)
        End Sub

        'Org. Code
        '----------------------------
        'Public Function LoadAll() As List(Of Material)
        '    Dim json = File.ReadAllText(_filePath)
        '    Return JsonSerializer.Deserialize(Of List(Of Material))(json) _
        '           ?? New List(Of Material)
        'End Function


        ' fixed code
        'Public Function LoadAll() As List(Of Material) '(materials As List(Of Material)) As List(Of Material)
        '    Dim json = File.ReadAllText(_filePath)
        '    Return JsonSerializer.Deserialize(Of List(Of Material))(json)
        'End Function

        'Public Sub SaveAll(materials As List(Of Material))
        '    Dim json = JsonSerializer.Serialize(
        '        materials,
        '        New JsonSerializerOptions With {.WriteIndented = True}
        '    )
        '    File.WriteAllText(_filePath, json)
        'End Sub
    End Class
End Namespace