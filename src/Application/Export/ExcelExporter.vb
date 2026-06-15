Imports ClosedXML.Excel

Public Class ExcelExporter
	''' <summary>
	''' Exports the takeoff results to an Excel file.
	''' </summary>
	''' <param name="result">
	''' The takeoff results to export, containing a dictionary of block names and their corresponding values.
	''' </param>
	''' <param name="path">
	''' The file path where the Excel file will be saved. The file will be created if it does not exist, or overwritten if it does.
	''' </param>
	Public Sub Export(result As TakeOffResult, path As String)
		Dim wb = New XLWorkbook()
		Dim ws = wb.Worksheets.Add("TakeOff")
		ws.Cell(1, 1).Value = "Block"
		ws.Cell(1, 2).Value = "Value"
		Dim row = 2
		For Each kv In result.Results
			ws.Cell(row, 1).Value = kv.Key
			ws.Cell(row, 2).Value = kv.Value
			row += 1
		Next
		wb.SaveAs(path)
	End Sub
End Class