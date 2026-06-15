Imports Domain.Entities
Imports Domain.Common

Namespace Validation

    Public Class CanvasElementValidation

        Private Shared ReadOnly ValidTypes As String() =
            {"Line", "Rectangle", "Circle", "Ellipse"}

        Public Shared Sub Validate(element As CanvasElement)
            If element.Id = Guid.Empty Then
                Throw New ValidationException("Element ID invalid")
            End If

            If Not ValidTypes.Contains(element.Type) Then
                Throw New ValidationException($"Invalid element type: {element.Type}")
            End If

            If String.IsNullOrWhiteSpace(element.GeometryJson) Then
                Throw New ValidationException("Geometry cannot be empty")
            End If
        End Sub

    End Class

End Namespace