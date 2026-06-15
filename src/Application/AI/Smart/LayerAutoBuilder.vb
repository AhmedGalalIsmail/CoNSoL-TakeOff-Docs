Imports Domain.Entities

Public Class LayerAutoBuilder
	Public Function BuildLayers(
		layout As CanvasLayout,
		classification As Dictionary(Of Guid, String)
	) As List(Of Layer)
		Dim layers As New List(Of Layer)
		Dim categories = classification.Values.Distinct()
		For Each cat In categories
			Dim layer As New Layer With {
				.Name = cat
			}
			layers.Add(layer)
			' Assign elements
			For Each el In layout.Elements
				If classification(el.Id) = cat Then
					el.LayerId = layer.Id ' ? IMPORTANT
				End If
			Next
		Next
		Return layers
	End Function
End Class