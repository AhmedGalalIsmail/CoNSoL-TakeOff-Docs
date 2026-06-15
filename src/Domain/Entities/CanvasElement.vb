Option Strict On

Imports Domain.Entities

Namespace Entities
    ''' <summary>
    ''' Represents a single element drawn on the canvas, 
    ''' including its geometry, business data, and relationships.
    ''' </summary>
    Public Class CanvasElement
        Public Property ParentElementId As String
        Public Property Id As Guid = Guid.NewGuid()
        Public Property Type As String
        Public Property Layer As String
        Public Property GeometryJson As String
        Public Property BusinessJson As String
        Public Property RelationshipType As ElementRelationshipType
        Public Property ChildElementId As String
        Public Property LayerId As Guid
    End Class
End Namespace