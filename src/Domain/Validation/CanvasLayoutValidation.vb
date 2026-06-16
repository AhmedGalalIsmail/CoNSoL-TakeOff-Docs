Imports Domain.Entities
Imports Domain.Common

Namespace Validation
	''' <summary>
	''' Validates a CanvasLayout object, ensuring that it and its elements are not null.
	''' </summary>
	Public Class CanvasLayoutValidation
		''' <summary>
		''' Validates the provided CanvasLayout object. Throws a ValidationException if the layout or its elements are null.
		''' </summary>
		''' <param name="layout"></param>
		Public Shared Sub Validate(layout As CanvasLayout)
			If layout Is Nothing Then
				Throw New ValidationException("Layout cannot be null")
			End If

			If layout.Elements Is Nothing Then
				Throw New ValidationException("Elements collection cannot be null")
			End If

			For Each el In layout.Elements
				CanvasElementValidation.Validate(el)
			Next
		End Sub
	End Class
End Namespace
