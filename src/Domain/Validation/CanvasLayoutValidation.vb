Imports Domain.Entities
Imports Domain.Common

Namespace Validation

    Public Class CanvasLayoutValidation

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