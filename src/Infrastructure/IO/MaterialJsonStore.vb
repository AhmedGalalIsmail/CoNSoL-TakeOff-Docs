Option Strict On
Imports System.Text.Json
Imports Domain.Entities.BlockModels
Imports Domain.Entities
Imports System.IO

Namespace IO
    ''' <summary>
    ''' Handles loading and saving materials to a JSON file.
    ''' </summary>
    Public Class MaterialJsonStore

        Private ReadOnly _path As String
        Private ReadOnly _filePath As String

        ''' <summary>
        ''' Initializes the MaterialJsonStore with a file path. If the file does not exist, it creates an empty JSON file.
        ''' </summary>
        ''' <param name="path"></param>
        ''' <param name="filePath"></param>
        Public Sub New(path As String, Optional filePath As String = "")
            _path = path
            If Not File.Exists(_path) Then
                SaveAll(New List(Of Material))
            End If
        End Sub

        ''' <summary>
        ''' Initializes the MaterialJsonStore with a file path. If the file does not exist, it creates an empty JSON file.
        ''' </summary>
        ''' <param name="filePath"></param>
        Public Sub New(filePath As String)
            _filePath = filePath
            EnsureFile()
        End Sub

        ''' <summary>
        ''' Ensures that the JSON file exists. If it does not, creates an empty JSON file with an empty list of materials.
        ''' </summary>
        Private Sub EnsureFile()
            If Not File.Exists(_filePath) Then
                SaveAll(New List(Of Material))
            End If
        End Sub

        ''' <summary>
        ''' Loads all materials from the JSON file and returns them as a list of Material objects.
        ''' </summary>
        ''' <returns>
        ''' A list of Material objects loaded from the JSON file. If the file is empty or contains invalid JSON, it returns an empty list.
        ''' </returns>
        Public Function LoadAll() As List(Of Material)
            Dim json = File.ReadAllText(_path)
            Return JsonSerializer.Deserialize(Of List(Of Material))(json)
        End Function

        ''' <summary>
        ''' Saves the provided list of Material objects to the JSON file, overwriting any existing content. The JSON is formatted with indentation for readability.
        ''' </summary>
        ''' <param name="materials"></param>
        Public Sub SaveAll(materials As List(Of Material))
            Dim json = JsonSerializer.Serialize(materials, New JsonSerializerOptions With {.WriteIndented = True})
            File.WriteAllText(_path, json)
        End Sub
    End Class
End Namespace
