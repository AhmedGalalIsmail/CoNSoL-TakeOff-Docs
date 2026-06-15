Imports Domain.Entities

Public Class AiIntakeService

	Private ReadOnly _loader As New DrawingLoader()
	Private ReadOnly _ocr As New OcrService()
	Private ReadOnly _scaleDetector As New ScaleDetector()
	Private ReadOnly _geometry As New GeometryDetector()

	Private ReadOnly _shape As New ShapeDetector()
	Private ReadOnly _scale As New ScaleDetector()


	Private ReadOnly _classifier As New CategoryClassifier()
	Private ReadOnly _layerBuilder As New LayerAutoBuilder()
	Private ReadOnly _mapper As New MaterialMapper()



	''' <summary>
	''' Processes a drawing file, extracting text and detecting scale.
	''' </summary>
	''' <param name="filePath"></param>
	''' <returns></returns>
	Public Function ProcessDrawing(filePath As String, x As String) As AiIntakeResult

		Dim image = _loader.LoadImage(filePath)

		Dim text = _ocr.ExtractText(image)

		Dim scale = _scaleDetector.DetectScale(text)

		Dim elements = _geometry.Detect(text) ' ? NEW

		Return New AiIntakeResult With {
		.DetectedText = text,
		.DetectedScale = scale,
		.DetectedElements = elements
	}

	End Function
	'Public Function ProcessDrawing(filePath As String) As AiIntakeResult
	'	Dim image = _loader.LoadImage(filePath)
	'	Dim text = _ocr.ExtractText(image)
	'	Dim scale = _scaleDetector.DetectScale(text)
	'	Return New AiIntakeResult With {
	'		.DetectedText = text,
	'		.DetectedScale = scale
	'	}
	'End Function

	'Public Function ProcessDrawing(filePath As String) As AiIntakeResult
	'	'Dim elements = _shape.DetectShapes(filePath)
	'	' ? Real OCR
	'	Dim text = _ocr.ExtractText(filePath)

	'	' ? Real Scale Detection
	'	Dim scale = _scale.DetectScale(text)

	'	' ? Real Shape Detection
	'	Dim elements = _shape.DetectShapes(filePath)
	'	Dim layout As New CanvasLayout()
	'	layout.Elements.AddRange(elements)
	'	' ? 1. CLASSIFY
	'	Dim classification = _classifier.Classify(elements, text)
	'	' ? 2. BUILD LAYERS
	'	Dim layers = _layerBuilder.BuildLayers(layout, classification)
	'	' ? 3. APPLY MATERIALS
	'	_mapper.Apply(elements, classification)
	'	Return New AiIntakeResult With {
	'		.DetectedText = text,
	'		.DetectedScale = scale,
	'		.DetectedElements = elements
	'	}

	'End Function

End Class