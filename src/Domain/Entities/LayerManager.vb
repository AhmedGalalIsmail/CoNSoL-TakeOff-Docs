
Namespace Services

    Public Class LayerManager

        Private ReadOnly _layers As New List(Of Layer)

        Public Function AddLayer(name As String) As Layer
            Dim layer = New Layer With {.Name = name}
            layer.Validate()

            _layers.Add(layer)
            Return layer
        End Function

        Public Sub RemoveLayer(layerId As Guid)
            _layers.RemoveAll(Function(l) l.Id = layerId)
        End Sub

        Public Function GetAll() As List(Of Layer)
            Return _layers
        End Function

    End Class

End Namespace