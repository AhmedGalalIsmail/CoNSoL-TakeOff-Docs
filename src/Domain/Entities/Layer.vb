
Imports Domain.Common

Namespace Entities

    ''' <summary>
    ''' Represents a logical grouping of drawing elements.
    ''' Controls visibility, locking, and rendering behavior.
    ''' </summary>
    Public Class Layer

        Public Property Id As Guid = Guid.NewGuid()
        Public Property Name As String
        Public Property IsVisible As Boolean = True
        Public Property IsLocked As Boolean = False
        Public Property Color As String = "#FFFFFF"

        ''' <summary>
        ''' Validates layer invariants.
        ''' </summary>
        Public Sub Validate()
            If String.IsNullOrWhiteSpace(Name) Then
                Throw New ValidationException("Layer name cannot be empty")
            End If
        End Sub

    End Class

End Namespace