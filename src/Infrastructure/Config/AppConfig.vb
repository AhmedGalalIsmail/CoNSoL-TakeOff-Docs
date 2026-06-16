
Option Strict On
Imports System.Text.Json
Imports System.IO
Imports Infrastructure.Logging

Namespace Config
    ''' <summary>
    ''' 
    ''' </summary>
    Public Class AppConfig
        Public Property AppName As String = "CoNSoL-TakeOff"
        Public Property LogDir As String = "logs"
        Public Property DataDir As String = "data"

        Public _logger As ILogger

        ''' <summary>
        ''' Loads configuration from specified path. 
        ''' If file does not exist, creates default config and saves it.
        ''' </summary>
        ''' <param name="path"></param>
        ''' <param name="logger"></param>
        Public Function Load(path As String, logger As ILogger) As AppConfig
            Return Load(path, logger, _logger)
        End Function

        ''' <summary>
        ''' Loads configuration from specified path. 
        ''' </summary>
        ''' <param name="path"></param>
        ''' <param name="logger"></param>
        ''' <param name="_logger"></param>
        Public Shared Function Load(path As String, logger As ILogger, _logger As ILogger) As AppConfig
            _logger = logger
            If Not File.Exists(path) Then
                Dim def = New AppConfig()
                File.WriteAllText(path, JsonSerializer.Serialize(def, New JsonSerializerOptions With {.WriteIndented = True}))
                Return def
            End If
            Dim json = File.ReadAllText(path)
            Return JsonSerializer.Deserialize(Of AppConfig)(json)
        End Function

        ''' <summary>
        ''' Loads configuration from specified path.
        ''' </summary>
        ''' <param name="path"></param>
        Public Shared Function Load(path As String) As AppConfig
            If Not File.Exists(path) Then
                Dim def = New AppConfig()
                File.WriteAllText(path, JsonSerializer.Serialize(def, New JsonSerializerOptions With {.WriteIndented = True}))
                Return def
            End If
            Dim json = File.ReadAllText(path)
            Return JsonSerializer.Deserialize(Of AppConfig)(json)
        End Function
    End Class
End Namespace
