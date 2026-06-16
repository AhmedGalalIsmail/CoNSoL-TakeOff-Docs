Imports Domain.Entities

Namespace Services
	''' <summary>
	''' Manages a collection of layers, allowing for adding, removing, and retrieving layers. It ensures that there is always at least one default layer present in the collection.
	''' </summary>
	Public Class LayerManager

		''' <summary>
		''' A private list that holds all the layers managed by this LayerManager instance. Each layer is represented by a Layer object, which contains properties such as Id, Name, IsVisible, and IsLocked.
		''' </summary>
		Private ReadOnly _layers As New List(Of Layer)

		'Public Sub New(layers As List(Of Layer))
		'	Initialize()
		'	_layers = layers
		'End Sub

		Public Sub Initialize()
			EnsureDefaultLayer()
		End Sub
		''' <summary>Adds a new layer to the collection with the specified name. The layer is validated before being added to ensure it meets any necessary criteria.</summary>
		''' <param name="name">The name of the layer to be added.</param>
		''' <returns>The newly created Layer object.</returns>
		Public Function AddLayer(name As String) As Layer
			Dim layer = New Layer With {.Name = name}
			layer.Validate()

			_layers.Add(layer)
			Return layer
		End Function

		''' <summary>Removes a layer from the collection based on its unique identifier (layerId). If a layer with the specified ID exists in the collection, it will be removed. If no such layer exists, the method does nothing.</summary>
		''' <param name="layerId"></param>
		Public Sub RemoveLayer(layerId As Guid)
			_layers.RemoveAll(Function(l) l.Id = layerId)
		End Sub

		''' <summary>Returns all layers in the collection.</summary>
		''' <returns>A list of Layer objects representing all layers in the collection.</returns>
		Public Function GetAll() As List(Of Layer)
			Return _layers
		End Function

		''' <summary>
		''' Ensures that there is always at least one default layer in the collection. If no layers exist, a default layer is created and added to the collection.
		''' </summary>
		Public Sub EnsureDefaultLayer()
			' default layer is always present
			If _layers.Count = 0 Then
				Dim def As New Layer With {
					.Id = Guid.NewGuid(),
					.Name = "Default",
					.IsVisible = True,
					.IsLocked = False
				}
				_layers.Add(def)
			End If
		End Sub
	End Class

End Namespace
