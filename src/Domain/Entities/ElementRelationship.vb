' src\CoNSoL.Domain\Entities\ElementRelationship.vb
' New: element relationship model + enum
Option Strict On

Namespace Entities
    ''' <summary>
    ''' Define Element Relationship Type
    ''' </summary>
    Public Enum ElementRelationshipType
        Nested
        Exclusion
    End Enum

    ''' <summary>
    ''' Represents a parent/child relationship between elements.
    ''' IDs are Guids for type safety and to ensure valid GUIDs.
    ''' </summary>
    Public Class ElementRelationship
        Public Property ParentElementId As Guid
        Public Property ChildElementId As Guid
        Public Property RelationshipType As ElementRelationshipType
    End Class

    ''' <summary>
    ''' Represents a parent/child relationship between elements.
    ''' IDs are strings to allow simple GUID string usage from UI/serialization.
    ''' </summary>
    'Public Class ElementRelationship
    '    Public Property ParentElementId As String
    '    Public Property ChildElementId As String
    '    Public Property RelationshipType As ElementRelationshipType
    'End Class
End Namespace