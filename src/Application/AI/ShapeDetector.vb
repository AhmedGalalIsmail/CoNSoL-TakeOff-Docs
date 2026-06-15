Imports System.Text.Json
Imports Domain.Entities
Imports OpenCvSharp

Public Class ShapeDetector

	Public Function DetectShapes(imagePath As String) As List(Of CanvasElement)

		Dim elements As New List(Of CanvasElement)

		Dim img = Cv2.ImRead(imagePath, ImreadModes.Grayscale)

		' Edge detection
		Dim edges As New Mat()
		Cv2.Canny(img, edges, 50, 150)

		' Detect lines
		Dim lines = Cv2.HoughLinesP(edges, 1, Math.PI / 180, 100, 50, 10)
		For Each l In lines

			Dim geom = New With {
				.type = "Line",
				.start = New With {.x = l.P1.X, .y = l.P1.Y},
				.end = New With {.x = l.P2.X, .y = l.P2.Y}
			}

			elements.Add(New CanvasElement With {
				.Id = Guid.NewGuid(),
				.Type = "Line",
				.GeometryJson = JsonSerializer.Serialize(geom)
			})
		Next
		Return elements
	End Function

End Class