Option Strict On
Imports System.Text.Json

Namespace Wrappers
    ''' <summary>
    ''' A simple wrapper around System.Text.Json to centralize JSON serialization logic.
    ''' </summary>
    Public Class JsonSerializerWrapper
        ''' <summary>
        ''' Serializes an object to a JSON string using System.Text.Json.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="obj"></param>
        ''' <returns>
        ''' A JSON string representation of the object.
        ''' </returns>
        Public Shared Function Serialize(Of T)(obj As T) As String
            Return JsonSerializer.Serialize(obj)
        End Function

        ''' <summary>
        ''' Deserializes a JSON string to an object of type T using System.Text.Json.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="json"></param>
        ''' <returns>
        ''' An object of type T deserialized from the JSON string.
        ''' </returns>
        Public Shared Function Deserialize(Of T)(json As String) As T
            Return JsonSerializer.Deserialize(Of T)(json)
        End Function
    End Class
End Namespace
