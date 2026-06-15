Imports Domain.Entities

''' <summary>
''' Represents the result of an AI intake process, containing detected text and scale information.
''' </summary>
''' <returns></returns>
Public Class AiIntakeResult

	Public Property DetectedText As List(Of String)
	Public Property DetectedScale As String
	Public Property DetectedElements As List(Of CanvasElement)
End Class