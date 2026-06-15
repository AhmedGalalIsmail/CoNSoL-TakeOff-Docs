Imports Tesseract


Public Class OcrService

    ''' <summary>
    ''' Extract text from drawing image
    ''' </summary>
    Public Class OcrService

        Public Function ExtractText(imagePath As String) As List(Of String)

            Dim lines As New List(Of String)

            Using engine As New TesseractEngine("./tessdata", "eng", EngineMode.Default)
                Using img = Pix.LoadFromFile(imagePath)
                    Using page = engine.Process(img)

                        Dim text = page.GetText()

                        lines = text.Split({Environment.NewLine},
                            StringSplitOptions.RemoveEmptyEntries).ToList()

                    End Using
                End Using
            End Using

            Return lines

        End Function

    End Class

    Public Function ExtractText(imageBytes As Byte()) As List(Of String)

        ' ?? TEMP MOCK (replace with real OCR later)
        Dim detectedText As New List(Of String)

        ' Simulated examples (from real drawings)
        detectedText.Add("SCALE 1:100")
        detectedText.Add("WALL 300mm")
        detectedText.Add("ROOM 5.0 x 4.0")

        Return detectedText

    End Function


End Class