Imports System.Text.Json
Imports System.Text.RegularExpressions
Imports Domain.Entities


Public Class GeometryDetector

	''' <summary>
	''' Detect basic shapes from OCR text
	''' </summary>
	Public Function Detect(textLines As List(Of String)) As List(Of CanvasElement)
		Dim elements As New List(Of CanvasElement)
		For Each line In textLines
			' ? Detect rectangle: "5.0 x 4.0"
			Dim rectMatch = Regex.Match(line, "(\d+(\.\d+)?)\s*x\s*(\d+(\.\d+)?)")
			If rectMatch.Success Then

				Dim width = Decimal.Parse(rectMatch.Groups(1).Value)
				Dim height = Decimal.Parse(rectMatch.Groups(3).Value)

				Dim geom = New With {
					.type = "Rectangle",
					.width = width,
					.height = height
				}

				elements.Add(New CanvasElement With {
					.Id = Guid.NewGuid(),
					.Type = "Rectangle",
					.GeometryJson = JsonSerializer.Serialize(geom)
				})

				Continue For
			End If
			' ? Detect line: "10m", "12.5 m"
			Dim lineMatch = Regex.Match(line, "(\d+(\.\d+)?)\s*m")
			If lineMatch.Success Then

				Dim length = Decimal.Parse(lineMatch.Groups(1).Value)

				Dim geom = New With {
					.type = "Line",
					.start = New With {.x = 0, .y = 0},
					.end = New With {.x = length, .y = 0}
				}

				elements.Add(New CanvasElement With {
					.Id = Guid.NewGuid(),
					.Type = "Line",
					.GeometryJson = JsonSerializer.Serialize(geom)
				})

			End If
		Next
		Return elements
	End Function
End Class