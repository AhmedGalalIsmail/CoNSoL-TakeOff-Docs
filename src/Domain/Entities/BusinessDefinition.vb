'src\ CoNSoL.Domain \ Entities \ BusinessDefinition.vb
'New: domain business types required by other projects
Option Strict On
Imports System.Collections.Generic
Imports Domain.Entities

Namespace Entities
    'Namespace BusinessDefinition
    ''' <summary>
    ''' Minimal business definition used when shapes have block assignments.
    ''' Kept intentionally small — expand as needed.
    ''' </summary>
    Public Class BusinessDefinition
        Public Property BlockCode As String
        Public Property DimensionMode As String
        Public Property Parameters As New Dictionary(Of String, Object)()
    End Class
End Namespace