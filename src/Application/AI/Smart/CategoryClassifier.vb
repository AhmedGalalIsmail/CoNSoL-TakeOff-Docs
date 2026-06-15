
Imports Domain.Entities

Public Class CategoryClassifier
	''' <summary>
	''' Classify objects into categories (Concrete, Plumbing, Electrical...)
	''' </summary>
	Public Function Classify(
		elements As List(Of CanvasElement),
		textLines As List(Of String)
	) As Dictionary(Of Guid, String)
		Dim result As New Dictionary(Of Guid, String)
		For Each el In elements
			Dim category = "Unknown"
			' ? basic rules (expandable)
			If textLines.Any(Function(t) t.ToLower().Contains("wall")) Then
				category = "Concrete"
			End If
			If textLines.Any(Function(t) t.ToLower().Contains("pipe")) Then
				category = "Plumbing"
			End If
			If textLines.Any(Function(t) t.ToLower().Contains("elect")) Then
				category = "Electrical"
			End If
			result(el.Id) = category
		Next
		Return result
	End Function
End Class