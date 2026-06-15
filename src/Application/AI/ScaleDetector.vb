Imports System.Text.RegularExpressions

Public Class ScaleDetector

	''' <summary>
	''' Detect scale from OCR text
	''' </summary>
	Public Function DetectScale(textLines As List(Of String)) As String

		For Each line In textLines

			' Match: SCALE 1:100
			Dim match = Regex.Match(line, "(\d+:\d+)")

			If match.Success Then
				Return match.Value
			End If

		Next

		Return Nothing ' Not found

	End Function

End Class