---
color: "#04509a"
---
# CoNSoL-TakeOff Coding Assistance Plan

## Purpose: 
Comprehensive guide for consistent, high-quality implementation across all project layers  
**Date:** 2025  
## Aligned with: 
SDLC Documentation (Mega-File.md), OOAD Principles, VB.NET Best Practices

---
# 📋 Document Structure
The plan is organized into 5 architectural layers as you requested:

## Architectural Layers
### 🏗️ Foundation Layer
•	Domain Entities: CanvasLayout, CanvasElement, BusinessDefinition invariants, behavior, and validation
•	Infrastructure: AppConfig, JSON Serialization with round-trip validation
•	Coverage: Entry-point validation, entity lifecycle, constraint enforcement

### 🎨 Rendering Layer
•	Canvas Control: Drawing surface, coordinate transformation, shape rendering
•	Shape Rendering: Zoom-aware scaling, selection highlighting, hit testing
•	Coverage: Visual consistency rules, layer visibility, preview geometry

### ⌨️ Interaction Layer
•	Tool System: State machine for drawing tools (Line, Rectangle, Circle, etc.)
•	Multi-Selection: Duplicate prevention, selection consistency, shared properties
•	Coverage: Event routing, tool activation/deactivation, cursor management
### 💼 Business Layer
•	Calculation Engine: Dimension modes (D0-D3), nested object logic, cost aggregation
•	Aggregation Service: Material/layer grouping, CSV export, quantity calculations
•	Coverage: Deterministic calculations, cost validation, child quantity subtraction
### 🔗 Integration Layer
•	Dependency Injection: Composition root, singleton/scoped lifetimes, service resolution
•	Form Orchestration: Event handler coordination, modal dialogs, state propagation
•	Cross-Layer Communication: Domain event publisher, upward event bubbling

---
## 🎯 What's Included in Each Layer

For every layer, you get:
✅ Invariants - Rules that must always be true (e.g., "Scale factor must be > 0")
✅ Behavior - Actual code patterns with VB.NET examples
✅ Validation - Entry-point checks and constraint enforcement

---
## 📚 Quick Navigation Features

•	Cross-references to SDLC documentation (Mega-File.md)
•	Use case linkage (e.g., "UC-001: Draw a Line")
•	Reference tables for shape types, dimension modes, and dependency graphs
•	Testing hooks and mock patterns
•	Checklists for adding new features

---
## 💡 How to Use This Plan

1.	When starting a new feature: Review the relevant layer's invariants first
2.	When writing code: Copy the behavior patterns and adapt to your needs
3.	When debugging: Check the validation modules for constraint violations
4.	When refactoring: Use the dependency graph to ensure layering is maintained
5.	When onboarding: Have new team members read the Foundation layer first

---

## 📑 Quick Navigation

- [Foundation Layer](#-foundation-layer)
- [Rendering Layer](#-rendering-layer)
- [Interaction Layer](#-interaction-layer)
- [Business Layer](#-business-layer)
- [Integration Layer](#-integration-layer)

---

# 🏗️ Foundation Layer ^Foundation

**Scope:** Domain + Infrastructure layers  
**Responsibility:** Core data entities, utilities, configuration, persistence, serialization  
**Independence:** UI-agnostic, framework-free core


## 📦 Foundation Layer - Domain Entities ^Domain

### **1. CanvasLayout Entity** ^Dom-1CanvasLayout

#### Invariants ^Dom-1Invariants

- ✅ `Id` must be a valid Guid (not empty)
- ✅ `Name` must be non-null and not whitespace-only
- ✅ `Unit` must be a recognized unit system (metric, imperial)
- ✅ `ScaleFactor` must be > 0
- ✅ `Elements` list must not be null (empty list allowed)
- ✅ `Layers` list must contain at least one layer
- ✅ Only one layer can be marked as active at a time
- ✅ All `Elements` must reference a valid `LayerId` from `Layers`

#### Behavior ^Dom-1Behavior

```vb
' Add element with layer validation
Public Sub AddElement(element As CanvasElement)
    Validate.NotNull(element, NameOf(element))
    Validate.ElementLayerExists(element, Me.Layers)
    Me.Elements.Add(element)
End Sub

' Create new layer with uniqueness check
Public Function CreateLayer(name As String) As Layer
    Validate.LayerNameUnique(name, Me.Layers)
    Validate.StringNotEmpty(name)
    Dim layer = New Layer With { .Id = Guid.NewGuid, .Name = name }
    Me.Layers.Add(layer)
    Return layer
End Function

' Delete layer with reassignment or confirmation
Public Sub DeleteLayer(layerId As Guid, Optional moveToLayerId As Guid = Nothing)
    Dim layer = Me.Layers.FirstOrDefault(Function(l) l.Id = layerId)
    Validate.NotNull(layer, "Layer not found")
    Validate.LayerNotActive(layer, Me.ActiveLayerId, "Cannot delete active layer")

    If moveToLayerId <> Guid.Empty Then
        ' Reassign all elements to new layer
        Dim elementsOnLayer = Me.Elements.Where(Function(e) e.Layer = layerId.ToString())
        For Each elem In elementsOnLayer
            elem.Layer = moveToLayerId.ToString()
        Next
    Else
        ' Delete all elements on this layer
        Me.Elements = Me.Elements.Where(Function(e) e.Layer <> layerId.ToString()).ToList()
    End If

    Me.Layers.Remove(layer)
End Sub
```

#### Validation ^Dom-1Validation

```vb
Public Module CanvasLayoutValidation
    ''' <summary>Validate canvas layout integrity</summary>
    Public Sub ValidateLayout(layout As CanvasLayout)
        ' Check invariants
        If layout Is Nothing Then Throw New ArgumentNullException(NameOf(layout))
        If layout.Id = Guid.Empty Then Throw New InvalidOperationException("Canvas ID must not be empty")
        If String.IsNullOrWhiteSpace(layout.Name) Then Throw New InvalidOperationException("Canvas name required")
        If layout.ScaleFactor <= 0 Then Throw New InvalidOperationException("Scale factor must be > 0")
        If layout.Layers Is Nothing OrElse layout.Layers.Count = 0 Then
            Throw New InvalidOperationException("Canvas must have at least one layer")
        End If

        ' Check all elements reference valid layers
        Dim validLayerIds = layout.Layers.Select(Function(l) l.Id).ToHashSet()
        For Each elem In layout.Elements
            If Not validLayerIds.Contains(Guid.Parse(elem.Layer)) Then
                Throw New InvalidOperationException($"Element {elem.Id} references invalid layer {elem.Layer}")
            End If
        Next

        ' Check active layer exists
        If layout.ActiveLayerId <> Guid.Empty AndAlso Not validLayerIds.Contains(layout.ActiveLayerId) Then
            Throw New InvalidOperationException($"Active layer {layout.ActiveLayerId} does not exist")
        End If
    End Sub
End Module
```

---

### **2. CanvasElement Entity** ^Dom-2CanvasElement

#### Invariants ^Dom-2Invariants

- ✅ `Id` must be a valid Guid (not empty)
- ✅ `Type` must be one of: Line, Rectangle, Circle, Polyline, Text, Symbol, Dimension, Arc, Spline, Bezier
- ✅ `Layer` must be a non-empty GUID string (valid layer reference)
- ✅ `GeometryJson` must be valid JSON when deserialized
- ✅ `BusinessJson` must be valid JSON when deserialized
- ✅ Geometry must define a valid shape (no zero-size shapes)
- ✅ Parent-child relationships must not form cycles
- ✅ Parent must exist in canvas if `ParentElementId` is set

#### Behavior ^Dom-2Behavior

```vb
' Create element with validation
Public Shared Function Create(
    type As String,
    layerId As Guid,
    geometry As Object,
    Optional business As Object = Nothing) As CanvasElement

    Validate.ShapeTypeValid(type)
    Validate.GeometryValid(type, geometry)

    Dim elem = New CanvasElement With {
        .Id = Guid.NewGuid,
        .Type = type,
        .Layer = layerId.ToString,
        .GeometryJson = JsonConvert.SerializeObject(geometry),
        .BusinessJson = If(business Is Nothing, "{}", JsonConvert.SerializeObject(business))
    }

    Return elem
End Function

' Update geometry with validation
Public Sub UpdateGeometry(geometry As Object)
    Validate.GeometryValid(Me.Type, geometry)
    Me.GeometryJson = JsonConvert.SerializeObject(geometry)
End Sub

' Link to parent with cycle detection
Public Sub SetParent(parentElement As CanvasElement, Optional relationship As ElementRelationshipType = ElementRelationshipType.Nested)
    Validate.NotNull(parentElement, NameOf(parentElement))
    Validate.NoCyclicReference(parentElement.Id, Me.Id, "Parent-child relationship would create cycle")

    Me.ParentElementId = parentElement.Id.ToString
    Me.RelationshipType = relationship
End Sub
```

#### Validation ^Dom-2Validation

```vb
Public Module CanvasElementValidation
    Private ReadOnly ValidShapeTypes = New HashSet(Of String) From {
        "Line", "Rectangle", "Circle", "Polyline", "Text", "Symbol",
        "Dimension", "Arc", "Spline", "Bezier"
    }

    Public Sub ValidateElement(element As CanvasElement)
        If element Is Nothing Then Throw New ArgumentNullException(NameOf(element))
        If element.Id = Guid.Empty Then Throw New InvalidOperationException("Element ID must not be empty")
        If Not ValidShapeTypes.Contains(element.Type) Then
            Throw New InvalidOperationException($"Unknown shape type: {element.Type}")
        End If

        ' Validate JSON fields
        Try
            Dim _ = JObject.Parse(element.GeometryJson)
        Catch ex As JsonException
            Throw New InvalidOperationException("Invalid geometry JSON", ex)
        End Try

        Try
            Dim _ = JObject.Parse(element.BusinessJson)
        Catch ex As JsonException
            Throw New InvalidOperationException("Invalid business JSON", ex)
        End Try

        ' Validate geometry for the type
        ValidateGeometryForType(element.Type, element.GeometryJson)
    End Sub

    Private Sub ValidateGeometryForType(shapeType As String, geometryJson As String)
        Dim geometry = JObject.Parse(geometryJson)

        Select Case shapeType
            Case "Rectangle"
                If geometry("width") Is Nothing OrElse geometry("height") Is Nothing Then
                    Throw New InvalidOperationException("Rectangle must have width and height")
                End If
                If CDbl(geometry("width")) <= 0 OrElse CDbl(geometry("height")) <= 0 Then
                    Throw New InvalidOperationException("Rectangle dimensions must be > 0")
                End If

            Case "Circle"
                If geometry("radius") Is Nothing Then
                    Throw New InvalidOperationException("Circle must have radius")
                End If
                If CDbl(geometry("radius")) <= 0 Then
                    Throw New InvalidOperationException("Circle radius must be > 0")
                End If

            Case "Line"
                If geometry("startPoint") Is Nothing OrElse geometry("endPoint") Is Nothing Then
                    Throw New InvalidOperationException("Line must have start and end points")
                End If
                ' Ensure points are different
                Dim start = geometry("startPoint")
                Dim endPt = geometry("endPoint")
                If start("x") = endPt("x") AndAlso start("y") = endPt("y") Then
                    Throw New InvalidOperationException("Line start and end points must differ")
                End If
        End Select
    End Sub

    Public Sub ValidateCyclicReference(element As CanvasElement, allElements As IEnumerable(Of CanvasElement))
        ' BFS to detect cycle
        Dim visited = New HashSet(Of Guid)
        Dim queue = New Queue(Of Guid)

        If String.IsNullOrEmpty(element.ParentElementId) Then Return

        queue.Enqueue(Guid.Parse(element.ParentElementId))

        While queue.Count > 0
            Dim current = queue.Dequeue()
            If current = element.Id Then
                Throw New InvalidOperationException("Cyclic parent-child reference detected")
            End If
            If visited.Contains(current) Then Continue While
            visited.Add(current)

            Dim parent = allElements.FirstOrDefault(Function(e) e.Id = current)
            If parent IsNot Nothing AndAlso Not String.IsNullOrEmpty(parent.ParentElementId) Then
                queue.Enqueue(Guid.Parse(parent.ParentElementId))
            End If
        End While
    End Sub
End Module
```

---

### **3. BusinessDefinition Entity** ^Dom-3BusinessDefinition

#### Invariants ^3Invariants

- ✅ `BlockId` must reference a valid Block definition
- ✅ `DimensionMode` must be D0, D1, D2, or D3
- ✅ `FormulaCode` may be null (optional) or reference valid formula
- ✅ `MaterialId` may be null (optional) or reference valid material
- ✅ `Quantity` must be >= 0 if set
- ✅ `UnitPrice` must be >= 0 if set

#### Behavior ^3Behavior

```vb
Public Class BusinessDefinition
    Public Property BlockId As String
    Public Property DimensionMode As String  ' D0, D1, D2, D3
    Public Property FormulaCode As String
    Public Property MaterialId As String
    Public Property Quantity As Double
    Public Property UnitPrice As Double

    ' Calculate total cost
    Public Function GetTotalCost() As Double
        If Me.Quantity <= 0 OrElse Me.UnitPrice < 0 Then Return 0
        Return Me.Quantity * Me.UnitPrice
    End Function

    ' Validate dimension mode matches expected geometry
    Public Sub ValidateDimensionMode(geometryType As String)
        Dim validModes = GetValidDimensionModesForGeometry(geometryType)
        If Not validModes.Contains(Me.DimensionMode) Then
            Throw New InvalidOperationException(
                $"Dimension mode {Me.DimensionMode} invalid for {geometryType}. Valid: {String.Join(",", validModes)}")
        End If
    End Sub

    Private Shared Function GetValidDimensionModesForGeometry(geometryType As String) As IEnumerable(Of String)
        Select Case geometryType
            Case "Text" : Return New String() {} ' Text objects don't support dimension modes
            Case "Line" : Return New String() {"D0", "D1"}
            Case "Rectangle" : Return New String() {"D0", "D1", "D2"}
            Case "Circle", "Ellipse" : Return New String() {"D0", "D2"}
            Case Else : Return New String() {"D0", "D1", "D2", "D3"}
        End Select
    End Function
End Class
```

#### Validation ^3Validation

```vb
Public Module BusinessDefinitionValidation
    Public Sub ValidateBusinessDefinition(def As BusinessDefinition)
        If def Is Nothing Then Throw New ArgumentNullException(NameOf(def))

        ' Validate dimension mode
        Dim validModes = New HashSet(Of String) From {"D0", "D1", "D2", "D3"}
        If Not validModes.Contains(def.DimensionMode) Then
            Throw New InvalidOperationException($"Invalid dimension mode: {def.DimensionMode}")
        End If

        ' Validate quantities and prices
        If def.Quantity < 0 Then Throw New InvalidOperationException("Quantity cannot be negative")
        If def.UnitPrice < 0 Then Throw New InvalidOperationException("Unit price cannot be negative")
    End Sub
End Module
```

---

## 🔧 Foundation Layer - Infrastructure ^Infrastructure

### **1. AppConfig** ^Inf-1AppConfig

#### Invariants ^Inf-1Invariants

- ✅ `DatabaseConnectionString` must be non-null and valid for target DB
- ✅ `DeploymentMode` must be "Standalone" or "Integrated"
- ✅ `LogFilePath` must be writable directory path
- ✅ `EncryptionKey` must be minimum 32 characters for production
- ✅ `DefaultUnitSystem` must be "metric" or "imperial"
- ✅ All feature flags must have boolean values

#### Behavior ^Inf-1Behavior

```vb
Public Class AppConfig
    Public Shared ReadOnly Instance As AppConfig

    Public Property DatabaseConnectionString As String
    Public Property DeploymentMode As String
    Public Property LogFilePath As String
    Public Property DebugMode As Boolean
    Public Property Features As Dictionary(Of String, Boolean)
    Public Property DefaultUnitSystem As String
    Public Property EncryptionKey As String

    Shared Sub New()
        ' Lazy-load on first access
        Instance = LoadFromFile(GetConfigPath())
    End Sub

    ' Environment-aware config path
    Private Shared Function GetConfigPath() As String
        Dim env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"
        Return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"config.{env}.json")
    End Function

    Public Shared Function LoadFromFile(configPath As String) As AppConfig
        Validate.FileExists(configPath)
        Dim json = File.ReadAllText(configPath)
        Dim config = JsonConvert.DeserializeObject(Of AppConfig)(json)
        config.Validate()
        Return config
    End Function

    Public Sub Validate()
        If String.IsNullOrWhiteSpace(DatabaseConnectionString) Then
            Throw New InvalidOperationException("DatabaseConnectionString cannot be empty")
        End If

        If DeploymentMode <> "Standalone" AndAlso DeploymentMode <> "Integrated" Then
            Throw New InvalidOperationException($"Invalid DeploymentMode: {DeploymentMode}")
        End If

        If String.IsNullOrWhiteSpace(LogFilePath) Then
            Throw New InvalidOperationException("LogFilePath cannot be empty")
        End If

        If DefaultUnitSystem <> "metric" AndAlso DefaultUnitSystem <> "imperial" Then
            Throw New InvalidOperationException($"Invalid unit system: {DefaultUnitSystem}")
        End If

        If Features Is Nothing Then Features = New Dictionary(Of String, Boolean)
    End Sub
End Class
```

#### Validation ^Inf-1Validation

```vb
Public Module AppConfigValidation
    Public Sub ValidateConnectionString(connectionString As String, mode As String)
        Try
            Select Case mode
                Case "Standalone"
                    ' Validate SQLite connection
                    Using conn = New SQLiteConnection(connectionString)
                        conn.Open()
                    End Using

                Case "Integrated"
                    ' Validate SQL Server connection
                    Using conn = New SqlConnection(connectionString)
                        conn.Open()
                    End Using
            End Select
        Catch ex As Exception
            Throw New InvalidOperationException($"Connection string validation failed: {ex.Message}", ex)
        End Try
    End Sub

    Public Sub ValidateEncryptionKey(key As String, isProduction As Boolean)
        If isProduction Then
            If key.Length < 32 Then
                Throw New InvalidOperationException("Production encryption key must be at least 32 characters")
            End If
        End If
    End Sub
End Module
```

---

### **2. JSON Serialization** ^Inf-2Serial

#### Invariants ^Inf-2Invariants

- ✅ All serialized objects must preserve round-trip equality (serialize → deserialize → identical)
- ✅ Null values must be explicitly handled or configured
- ✅ Date/DateTime must use ISO 8601 format
- ✅ Guid must serialize as hyphenated string
- ✅ Custom converters must not lose type information

#### Behavior ^Inf-2Behavior

```vb
Public Module JsonSerializer
    Private ReadOnly Settings = New JsonSerializerSettings With {
        .NullValueHandling = NullValueHandling.Ignore,
        .DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffZ",
        .Converters = New List(Of JsonConverter) From {
            New StringEnumConverter(),
            New GuidConverter(),
            New IsoDateTimeConverter()
        }
    }

    Public Function Serialize(Of T)(obj As T) As String
        Validate.NotNull(obj, NameOf(obj))
        Try
            Return JsonConvert.SerializeObject(obj, Formatting.Indented, Settings)
        Catch ex As JsonException
            Throw New SerializationException($"Failed to serialize {GetType(T).Name}", ex)
        End Try
    End Function

    Public Function Deserialize(Of T)(json As String) As T
        Validate.StringNotEmpty(json, NameOf(json))
        Try
            Return JsonConvert.DeserializeObject(Of T)(json, Settings)
        Catch ex As JsonException
            Throw New SerializationException($"Failed to deserialize {GetType(T).Name}: {ex.Message}", ex)
        End Try
    End Function

    ' Round-trip validation
    Public Sub ValidateRoundTrip(Of T)(original As T)
        Dim json = Serialize(original)
        Dim restored = Deserialize(Of T)(json)

        If Not original.Equals(restored) Then
            Throw New InvalidOperationException($"Round-trip serialization failed for {GetType(T).Name}")
        End If
    End Sub
End Module
```

#### Validation ^Inf-2Validation

```vb
Public Module SerializationValidation
    Public Sub ValidateJson(json As String)
        If String.IsNullOrWhiteSpace(json) Then
            Throw New ArgumentException("JSON cannot be empty")
        End If

        Try
            JObject.Parse(json)
        Catch ex As JsonException
            Throw New InvalidOperationException($"Invalid JSON: {ex.Message}", ex)
        End Try
    End Sub
End Module
```

---

# 🎨 Rendering Layer ^Rendering

**Scope:** Desktop.Controls (CanvasControl, LineShape)  
**Responsibility:** 2D drawing, shape rendering, visual feedback  
**Independence:** Domain-aware, UI framework-coupled

---

## 🖼️ Canvas Control ^Canvas-Control

#### Invariants ^Ren-1Invariants

- ✅ Canvas must always have a valid `CanvasLayout` reference (not null)
- ✅ Current drawing state must match visual display
- ✅ All rendered shapes must exist in `CurrentLayout.Elements`
- ✅ Scale factor must be > 0 (no invalid zoom states)
- ✅ No shapes can be rendered outside valid layer bounds
- ✅ Selection state must be consistent with rendered highlights

#### Behavior ^Ren-1Behavior

```vb
Public Class CanvasControl
    Inherits UserControl

    Private _currentLayout As CanvasLayout
    Private _selectedElements As List(Of CanvasElement)
    Private _previewGeometry As Object
    Private _isDrawing As Boolean = False

    ' Current drawing tool
    Public Property CurrentTool As ToolType

    ' Zoom level (scale factor)
    Public Property ZoomLevel As Double = 1.0

    ' Pan offset
    Public Property PanOffset As PointF = New PointF(0, 0)

    ''' <summary>Set the canvas layout and refresh</summary>
    Public Sub SetLayout(layout As CanvasLayout)
        Validate.NotNull(layout, NameOf(layout))
        CanvasLayoutValidation.ValidateLayout(layout)

        _currentLayout = layout
        _selectedElements = New List(Of CanvasElement)
        Invalidate()  ' Request redraw
    End Sub

    ''' <summary>Render all elements based on current view state</summary>
    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)

        If _currentLayout Is Nothing Then Return

        ' Setup graphics for high quality
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias
        e.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias

        ' Render grid (optional)
        RenderGrid(e.Graphics)

        ' Render visible layers only
        For Each layer In _currentLayout.Layers
            If Not layer.Visible Then Continue For

            Dim layerElements = _currentLayout.Elements.Where(
                Function(elem) Guid.Parse(elem.Layer) = layer.Id).ToList()

            For Each element In layerElements
                RenderElement(e.Graphics, element, layer)
            Next
        Next

        ' Render selection highlights
        For Each selected In _selectedElements
            RenderSelectionHighlight(e.Graphics, selected)
        Next

        ' Render preview geometry (while drawing)
        If _isDrawing AndAlso _previewGeometry IsNot Nothing Then
            RenderPreview(e.Graphics, _previewGeometry)
        End If
    End Sub

    ''' <summary>Render a single element based on its type</summary>
    Private Sub RenderElement(g As Graphics, element As CanvasElement, layer As Layer)
        Try
            Dim geometry = JsonConvert.DeserializeObject(Of Object)(element.GeometryJson)

            Select Case element.Type
                Case "Line"
                    RenderLine(g, geometry, layer)
                Case "Rectangle"
                    RenderRectangle(g, geometry, layer)
                Case "Circle"
                    RenderCircle(g, geometry, layer)
                Case Else
                    ' Handle other types
            End Select
        Catch ex As Exception
            Logger.LogError($"Failed to render element {element.Id}: {ex.Message}")
        End Try
    End Sub

    ''' <summary>Handle mouse down for drawing interaction</summary>
    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)

        ' Convert physical coordinates to logical
        Dim logicalPoint = PhysicalToLogical(e.Location)

        Select Case CurrentTool
            Case ToolType.LineTool
                HandleLineToolMouseDown(logicalPoint)
            Case ToolType.RectangleTool
                HandleRectangleToolMouseDown(logicalPoint)
            Case ToolType.SelectTool
                HandleSelectToolMouseDown(logicalPoint)
        End Select
    End Sub

    ''' <summary>Convert screen coordinates to canvas logical coordinates</summary>
    Private Function PhysicalToLogical(screenPoint As PointF) As PointF
        Return New PointF(
            (screenPoint.X - PanOffset.X) / ZoomLevel,
            (screenPoint.Y - PanOffset.Y) / ZoomLevel)
    End Function

    ''' <summary>Convert canvas logical coordinates to screen coordinates</summary>
    Private Function LogicalToPhysical(logicalPoint As PointF) As PointF
        Return New PointF(
            logicalPoint.X * ZoomLevel + PanOffset.X,
            logicalPoint.Y * ZoomLevel + PanOffset.Y)
    End Function
End Class
```

#### Validation ^Ren-1Validation

```vb
Public Module CanvasControlValidation
    Public Sub ValidateCanvasState(canvas As CanvasControl)
        If canvas.CurrentLayout Is Nothing Then
            Throw New InvalidOperationException("Canvas layout not set")
        End If

        If canvas.ZoomLevel <= 0 Then
            Throw New InvalidOperationException("Zoom level must be > 0")
        End If

        ' Verify all rendered elements exist in layout
        For Each element In canvas.CurrentLayout.Elements
            Dim layer = canvas.CurrentLayout.Layers.FirstOrDefault(
                Function(l) l.Id = Guid.Parse(element.Layer))
            If layer Is Nothing Then
                Throw New InvalidOperationException($"Element {element.Id} references non-existent layer")
            End If
        Next
    End Sub
End Module
```

---

## 🔷 Shape Rendering ^Shape-Rendering

#### Invariants ^Ren-2Invariants

- ✅ All rendered shapes must use consistent coordinate systems (logical)
- ✅ Line width must scale with zoom level
- ✅ Colors must respect layer defaults unless explicitly overridden
- ✅ Selection highlighting must be visually distinct but non-destructive
- ✅ Text must render with proper alignment and clipping

#### Behavior ^Ren-2Behavior

```vb
Public Class LineShape
    Public Property StartPoint As PointF
    Public Property EndPoint As PointF
    Public Property Color As Color = Color.Black
    Public Property LineWidth As Single = 1.0

    ''' <summary>Render line with proper scaling</summary>
    Public Sub Render(g As Graphics, zoomLevel As Double, isSelected As Boolean)
        Dim scaledLineWidth = CInt(LineWidth * zoomLevel)
        Dim pen = New Pen(Color, scaledLineWidth)

        Try
            g.DrawLine(pen, StartPoint, EndPoint)

            If isSelected Then
                ' Draw selection handles
                Dim handleSize = 4
                Dim brushHandle = New SolidBrush(Color.Blue)
                g.FillEllipse(brushHandle, StartPoint.X - handleSize, StartPoint.Y - handleSize, 
                              handleSize * 2, handleSize * 2)
                g.FillEllipse(brushHandle, EndPoint.X - handleSize, EndPoint.Y - handleSize, 
                              handleSize * 2, handleSize * 2)
                brushHandle.Dispose()
            End If
        Finally
            pen.Dispose()
        End Try
    End Sub

    ''' <summary>Hit test for selection</summary>
    Public Function HitTest(point As PointF, tolerance As Single) As Boolean
        ' Point-to-line distance
        Dim distance = PointToLineDistance(point, StartPoint, EndPoint)
        Return distance <= tolerance
    End Function

    Private Shared Function PointToLineDistance(
        point As PointF, 
        lineStart As PointF, 
        lineEnd As PointF) As Double

        Dim dx = lineEnd.X - lineStart.X
        Dim dy = lineEnd.Y - lineStart.Y

        If dx = 0 AndAlso dy = 0 Then
            Return Math.Sqrt((point.X - lineStart.X) ^ 2 + (point.Y - lineStart.Y) ^ 2)
        End If

        Dim t = ((point.X - lineStart.X) * dx + (point.Y - lineStart.Y) * dy) / (dx * dx + dy * dy)
        t = Math.Max(0, Math.Min(1, t))

        Dim closestX = lineStart.X + t * dx
        Dim closestY = lineStart.Y + t * dy

        Return Math.Sqrt((point.X - closestX) ^ 2 + (point.Y - closestY) ^ 2)
    End Function
End Class
```

#### Validation ^Ren-2Validation

```vb
Public Module ShapeRenderingValidation
    Public Sub ValidateRenderState(shape As LineShape, zoomLevel As Double)
        If zoomLevel <= 0 Then
            Throw New InvalidOperationException("Zoom level must be > 0")
        End If

        If shape.LineWidth <= 0 Then
            Throw New InvalidOperationException("Line width must be > 0")
        End If
    End Sub
End Module
```

---

# ⌨️ Interaction Layer

**Scope:** Desktop.Controls, Desktop.Forms  
**Responsibility:** User input handling, tool state machines, multi-selection, UI orchestration  
**Independence:** Depends on Domain + Rendering

---

## 🖱️ Tool System & State Machine ^ToolSystem-StateMachine

#### Invariants ^Inter-1Invariants

- ✅ Only one tool can be active at a time
- ✅ Tool must have a valid state machine (idle, drawing, previewing)
- ✅ Mouse events must be routed only to active tool
- ✅ Drawing state must be cleared on Escape or tool switch
- ✅ Selection state must be consistent with visual highlights

#### Behavior ^Inter-1Behavior

```vb
Public Enum ToolType
    SelectTool
    LineTool
    RectangleTool
    CircleTool
    PolylineTool
    TextTool
    PanTool
    ZoomTool
End Enum

Public MustInherit Class BaseTool
    Protected _canvas As CanvasControl
    Protected _isActive As Boolean = False
    Protected _previewGeometry As Object

    Protected Sub New(canvas As CanvasControl)
        _canvas = canvas
    End Sub

    Public Sub Activate()
        _isActive = True
        _canvas.Cursor = GetCursor()
        OnActivate()
    End Sub

    Public Sub Deactivate()
        _isActive = False
        ClearPreview()
        OnDeactivate()
    End Sub

    Public MustOverride Sub OnMouseDown(location As PointF)
    Public MustOverride Sub OnMouseMove(location As PointF)
    Public MustOverride Sub OnMouseUp(location As PointF)
    Public MustOverride Function GetCursor() As Cursor

    Protected Overridable Sub OnActivate()
        ' Override in subclasses
    End Sub

    Protected Overridable Sub OnDeactivate()
        ClearPreview()
    End Sub

    Protected Sub ClearPreview()
        _previewGeometry = Nothing
        _canvas.Invalidate()
    End Sub
End Class

' Example: Line Tool
Public Class LineTool
    Inherits BaseTool

    Private _startPoint As PointF?
    Private _currentPoint As PointF?

    Public Sub New(canvas As CanvasControl)
        MyBase.New(canvas)
    End Sub

    Public Overrides Sub OnMouseDown(location As PointF)
        If Not _startPoint.HasValue Then
            ' First click: set start point
            _startPoint = location
            _currentPoint = location
        Else
            ' Second click: commit line
            CommitLine(_startPoint.Value, location)
            _startPoint = Nothing
            _currentPoint = Nothing
        End If
    End Sub

    Public Overrides Sub OnMouseMove(location As PointF)
        If _startPoint.HasValue Then
            ' Show rubber-band preview
            _currentPoint = location
            _previewGeometry = New With {
                .startPoint = _startPoint.Value,
                .endPoint = location
            }
            _canvas.Invalidate()
        End If
    End Sub

    Public Overrides Sub OnMouseUp(location As PointF)
        ' Handled in OnMouseDown
    End Sub

    Private Sub CommitLine(start As PointF, endPoint As PointF)
        Try
            Dim line = CanvasElement.Create(
                "Line",
                _canvas.CurrentLayout.ActiveLayerId,
                New With {.startPoint = start, .endPoint = endPoint})

            _canvas.CurrentLayout.AddElement(line)
            _canvas.Invalidate()
        Catch ex As Exception
            Logger.LogError($"Failed to create line: {ex.Message}")
        End Try
    End Sub

    Public Overrides Function GetCursor() As Cursor
        Return Cursors.Crosshair
    End Function
End Class
```

#### Validation ^Inter-1Validation

```vb
Public Module ToolValidation
    Public Sub ValidateToolState(tool As BaseTool, canvas As CanvasControl)
        If canvas.CurrentLayout Is Nothing Then
            Throw New InvalidOperationException("Canvas layout not set")
        End If

        If tool Is Nothing Then
            Throw New ArgumentNullException(NameOf(tool))
        End If
    End Sub
End Module
```

---

## 📋 Multi-Selection ^Multi-Selection

#### Invariants ^Inter-2Invariants

- ✅ Selection list must never contain duplicate elements
- ✅ All selected elements must exist in current layout
- ✅ Empty selection is valid state
- ✅ Selection must be cleared when layout changes

#### Behavior ^Inter-2Behavior

```vb
Public Class SelectionManager
    Private _selected As List(Of CanvasElement)
    Private _canvas As CanvasControl

    Public ReadOnly Property SelectedElements As IReadOnlyList(Of CanvasElement)
        Get
            Return _selected.AsReadOnly()
        End Get
    End Property

    Public Property Count As Integer
        Get
            Return _selected.Count
        End Get
    End Property

    ''' <summary>Select single element</summary>
    Public Sub SelectSingle(element As CanvasElement)
        Validate.NotNull(element, NameOf(element))
        _selected.Clear()
        _selected.Add(element)
        NotifySelectionChanged()
    End Sub

    ''' <summary>Toggle element selection</summary>
    Public Sub ToggleSelection(element As CanvasElement)
        Validate.NotNull(element, NameOf(element))
        Dim idx = _selected.FindIndex(Function(e) e.Id = element.Id)
        If idx >= 0 Then
            _selected.RemoveAt(idx)
        Else
            _selected.Add(element)
        End If
        NotifySelectionChanged()
    End Sub

    ''' <summary>Add element to current selection</summary>
    Public Sub AddToSelection(element As CanvasElement)
        Validate.NotNull(element, NameOf(element))
        If Not _selected.Any(Function(e) e.Id = element.Id) Then
            _selected.Add(element)
            NotifySelectionChanged()
        End If
    End Sub

    ''' <summary>Clear all selections</summary>
    Public Sub ClearSelection()
        If _selected.Count > 0 Then
            _selected.Clear()
            NotifySelectionChanged()
        End If
    End Sub

    ''' <summary>Check if all selected elements are of same type</summary>
    Public Function IsUniformSelection() As Boolean
        If _selected.Count <= 1 Then Return True
        Dim firstType = _selected(0).Type
        Return _selected.All(Function(e) e.Type = firstType)
    End Function

    ''' <summary>Get shared properties of all selected elements</summary>
    Public Function GetSharedProperties() As Dictionary(Of String, Object)
        If _selected.Count = 0 Then Return New Dictionary(Of String, Object)

        Dim shared = New Dictionary(Of String, Object)
        Dim first = _selected(0)

        ' Collect properties that match across all selected
        Dim layer = first.Layer
        If _selected.All(Function(e) e.Layer = layer) Then
            shared.Add("layer", layer)
        End If

        Return shared
    End Function

    Public Event SelectionChanged As EventHandler

    Protected Sub NotifySelectionChanged()
        RaiseEvent SelectionChanged(Me, EventArgs.Empty)
    End Sub
End Class
```

#### Validation ^Inter-2Validation

```vb
Public Module SelectionValidation
    Public Sub ValidateSelection(selection As SelectionManager, layout As CanvasLayout)
        ' All selected elements must exist in layout
        For Each elem In selection.SelectedElements
            If Not layout.Elements.Any(Function(e) e.Id = elem.Id) Then
                Throw New InvalidOperationException($"Selected element {elem.Id} not in current layout")
            End If
        Next
    End Sub
End Module
```

---

# 💼 Business Layer ^Business

**Scope:** Application layer (Services, Calculators)  
**Responsibility:** Use case orchestration, calculations, aggregations  
**Independence:** Depends on Domain + Infrastructure

---

## 🧮 Calculation Engine ^Calculation

#### Invariants ^Bus-1Invariants

- ✅ Calculations must be deterministic (same input → same output)
- ✅ All quantities must be >= 0
- ✅ All prices must be >= 0
- ✅ Total cost = quantity × unit price
- ✅ Nested object relationships must be respected (subtractions)
- ✅ Division by zero must never occur

#### Behavior ^Bus-1Behavior

```vb
Public Class TakeOffCalculator
    Private _logger As ILogger

    Public Sub New(logger As ILogger)
        _logger = logger
    End Sub

    ''' <summary>Calculate quantities and costs for all elements</summary>
    Public Function Calculate(
        layout As CanvasLayout,
        ctx As TakeOffContext) As TakeOffResult

        Validate.NotNull(layout, NameOf(layout))
        Validate.NotNull(ctx, NameOf(ctx))
        CanvasLayoutValidation.ValidateLayout(layout)

        Dim result = New TakeOffResult

        Try
            ' Group elements by layer or material
            Dim groupedElements = GroupElements(layout, ctx)

            For Each group In groupedElements
                Dim groupTotal = CalculateGroupTotal(group, layout, ctx)
                result.AddGroupResult(group.Key, groupTotal)
            Next

            ' Handle nested objects (subtractions)
            ApplyNestedObjectLogic(result, layout, ctx)

            ' Calculate final costs
            CalculateCosts(result, ctx)

            _logger.LogInfo($"Calculation complete: {result.MaterialGroups.Count} material groups")

        Catch ex As Exception
            _logger.LogError($"Calculation failed: {ex.Message}")
            Throw
        End Try

        Return result
    End Function

    ''' <summary>Calculate quantity for a single element</summary>
    Private Function CalculateElementQuantity(
        element As CanvasElement,
        business As BusinessDefinition,
        layout As CanvasLayout) As Double

        ' Parse geometry to extract dimensions
        Dim geometry = JsonConvert.DeserializeObject(Of JObject)(element.GeometryJson)

        ' Apply dimension mode
        Dim quantity = business.DimensionMode Switch {
            "D0" => 1.0,  ' Count
            "D1" => ExtractLength(element.Type, geometry),
            "D2" => ExtractArea(element.Type, geometry),
            "D3" => ExtractVolume(element.Type, geometry, business),
            _ => 1.0
        }

        ' Apply quantity multiplier
        quantity = quantity * business.Quantity

        ' Handle nested objects (subtract child quantities)
        Dim childQuantity = GetChildQuantityToSubtract(element, layout)
        quantity = Math.Max(0, quantity - childQuantity)

        Return quantity
    End Function

    ''' <summary>Extract length from geometry</summary>
    Private Function ExtractLength(shapeType As String, geometry As JObject) As Double
        Select Case shapeType
            Case "Line"
                Dim start = geometry("startPoint").ToObject(Of PointF)()
                Dim endPt = geometry("endPoint").ToObject(Of PointF)()
                Return Math.Sqrt((endPt.X - start.X) ^ 2 + (endPt.Y - start.Y) ^ 2)

            Case "Rectangle"
                Return CDbl(geometry("width"))

            Case Else
                Return 0.0
        End Select
    End Function

    ''' <summary>Extract area from geometry</summary>
    Private Function ExtractArea(shapeType As String, geometry As JObject) As Double
        Select Case shapeType
            Case "Rectangle"
                Dim width = CDbl(geometry("width"))
                Dim height = CDbl(geometry("height"))
                Return width * height

            Case "Circle"
                Dim radius = CDbl(geometry("radius"))
                Return Math.PI * radius * radius

            Case Else
                Return 0.0
        End Select
    End Function

    ''' <summary>Extract volume from geometry and logical 3D</summary>
    Private Function ExtractVolume(
        shapeType As String,
        geometry As JObject,
        business As BusinessDefinition) As Double

        ' Volume = Area × Depth (or logical 3D properties)
        Dim area = ExtractArea(shapeType, geometry)
        Return area * 1.0  ' depth would come from business.Depth
    End Function

    ''' <summary>Get total quantity to subtract for child elements</summary>
    Private Function GetChildQuantityToSubtract(
        element As CanvasElement,
        layout As CanvasLayout) As Double

        Dim childrenOfElement = layout.Elements.Where(
            Function(e) e.ParentElementId = element.Id.ToString).ToList()

        Dim total = 0.0
        For Each child In childrenOfElement
            Try
                Dim childBusiness = JsonConvert.DeserializeObject(
                    Of BusinessDefinition)(child.BusinessJson)
                If childBusiness IsNot Nothing Then
                    ' Recursively get child quantity
                    total += CalculateElementQuantity(child, childBusiness, layout)
                End If
            Catch ex As Exception
                _logger.LogWarning($"Failed to process child element {child.Id}: {ex.Message}")
            End Try
        Next

        Return total
    End Function

    ''' <summary>Apply cost calculations</summary>
    Private Sub CalculateCosts(result As TakeOffResult, ctx As TakeOffContext)
        For Each group In result.MaterialGroups
            Dim totalCost = group.Quantity * group.UnitPrice
            group.TotalCost = totalCost
        Next
    End Sub
End Class
```

#### Validation ^Bus-1Validation

```vb
Public Module CalculationValidation
    Public Sub ValidateCalculationResult(result As TakeOffResult)
        If result Is Nothing Then
            Throw New ArgumentNullException(NameOf(result))
        End If

        ' All quantities must be >= 0
        For Each group In result.MaterialGroups
            If group.Quantity < 0 Then
                Throw New InvalidOperationException($"Negative quantity in group {group.Name}")
            End If

            If group.UnitPrice < 0 Then
                Throw New InvalidOperationException($"Negative price in group {group.Name}")
            End If

            ' Verify total cost calculation
            Dim expectedCost = group.Quantity * group.UnitPrice
            If Math.Abs(group.TotalCost - expectedCost) > 0.01 Then
                Throw New InvalidOperationException($"Cost calculation mismatch in {group.Name}")
            End If
        Next
    End Sub
End Module
```

---

## 📊 Aggregation Service ^Aggregation

#### Invariants ^Ren-2Invariants

- ✅ Aggregation must not modify source data
- ✅ Aggregation function must match data type (Sum/Avg only for numeric)
- ✅ Result must include source information (layer, object type)
- ✅ Zero-quantity items should still be included in results

#### Behavior ^Bus-2Behavior

```vb
Public Class TakeOffService
    Private _calculator As TakeOffCalculator
    Private _materialService As MaterialService

    ''' <summary>Generate aggregation by material</summary>
    Public Function AggregateByMaterial(
        layout As CanvasLayout,
        ctx As TakeOffContext) As TakeOffResult

        Dim result = _calculator.Calculate(layout, ctx)
        ' Result is already grouped by material
        Return result
    End Function

    ''' <summary>Generate aggregation by layer</summary>
    Public Function AggregateByLayer(
        layout As CanvasLayout,
        ctx As TakeOffContext) As Dictionary(Of String, TakeOffResult)

        Dim result = New Dictionary(Of String, TakeOffResult)

        For Each layer In layout.Layers
            Dim layerElements = layout.Elements.Where(
                Function(e) Guid.Parse(e.Layer) = layer.Id).ToList()

            Dim tempLayout = New CanvasLayout With {
                .Elements = layerElements,
                .Layers = layout.Layers
            }

            result(layer.Name) = _calculator.Calculate(tempLayout, ctx)
        Next

        Return result
    End Function

    ''' <summary>Export aggregation to CSV</summary>
    Public Function ExportToCsv(result As TakeOffResult, filePath As String) As Boolean
        Try
            Validate.NotNull(result, NameOf(result))
            Validate.StringNotEmpty(filePath, NameOf(filePath))

            Using writer = New StreamWriter(filePath)
                ' Write header
                writer.WriteLine("Material,Quantity,Unit,Price,Total")

                ' Write rows
                For Each group In result.MaterialGroups
                    writer.WriteLine($"{group.Name},{group.Quantity},{group.Unit},{group.UnitPrice},{group.TotalCost}")
                Next
            End Using

            Return True
        Catch ex As Exception
            Logger.LogError($"CSV export failed: {ex.Message}")
            Return False
        End Try
    End Function
End Class
```

#### Validation ^Bus-2Validation

```vb
Public Module AggregationValidation
    Public Sub ValidateAggregationRequest(
        layout As CanvasLayout,
        groupBy As String,
        aggregateFunction As String)

        If layout Is Nothing Then
            Throw New ArgumentNullException(NameOf(layout))
        End If

        Dim validGrouping = New HashSet(Of String) From {"Material", "Layer", "ObjectType"}
        If Not validGrouping.Contains(groupBy) Then
            Throw New InvalidOperationException($"Invalid grouping: {groupBy}")
        End If

        Dim validFunctions = New HashSet(Of String) From {"Count", "Sum", "Average", "Min", "Max"}
        If Not validFunctions.Contains(aggregateFunction) Then
            Throw New InvalidOperationException($"Invalid aggregation function: {aggregateFunction}")
        End If
    End Sub
End Module
```

---

# 🔗 Integration Layer ^Integration

**Scope:** Desktop.Forms, Desktop.CompositionRoot, Application Services  
**Responsibility:** Dependency injection, form orchestration, inter-layer communication  
**Independence:** Coordinates all layers

---

## 🏗️ Composition Root & Dependency Injection ^Composition-Root

#### Invariants ^Integ-1Invariants

- ✅ All singletons must be thread-safe
- ✅ Circular dependencies must not exist
- ✅ All services must be properly registered before use
- ✅ Lifetime scope must match component expectations

#### Behavior ^Integ-1Behavior

```vb
Public Module CompositionRoot
    Private _serviceProvider As IServiceProvider

    ''' <summary>Bootstrap dependency injection container</summary>
    Public Sub Initialize()
        Dim services = New ServiceCollection()

        ' Infrastructure
        services.AddSingleton(Of AppConfig)(AddressOf CreateAppConfig)
        services.AddSingleton(Of ILogger)(AddressOf CreateLogger)

        ' Application Services
        services.AddScoped(Of TakeOffCalculator)
        services.AddScoped(Of TakeOffService)
        services.AddScoped(Of MaterialService)

        ' UI Components (typically transient or singleton)
        services.AddTransient(Of MainForm)
        services.AddTransient(Of BlockAssignmentForm)
        services.AddTransient(Of MaterialCrudForm)

        _serviceProvider = services.BuildServiceProvider()

        Logger.LogInfo("Dependency injection initialized")
    End Sub

    ''' <summary>Resolve service instance</summary>
    Public Function Resolve(Of T As Class)() As T
        Try
            Return _serviceProvider.GetRequiredService(Of T)()
        Catch ex As Exception
            Logger.LogError($"Dependency resolution failed for {GetType(T).Name}: {ex.Message}")
            Throw
        End Try
    End Function

    Private Function CreateAppConfig() As AppConfig
        Try
            Dim configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json")
            Return AppConfig.LoadFromFile(configPath)
        Catch ex As Exception
            Logger.LogWarning($"Failed to load config: {ex.Message}, using defaults")
            Return New AppConfig With {
                .DatabaseConnectionString = "Data Source=:memory:",
                .DeploymentMode = "Standalone"
            }
        End Try
    End Function

    Private Function CreateLogger() As ILogger
        Dim config = Resolve(Of AppConfig)()
        Return New FileLogger(config.LogFilePath)
    End Function
End Module
```

#### Validation ^Integ-1Validation

```vb
Public Module DIValidation
    Public Sub ValidateDependencyGraph()
        ' Verify all registered services
        ' Check for circular dependencies
        ' Validate scope lifetimes
    End Sub
End Module
```

---

## 📝 Form Orchestration ^Form-Orch

#### Invariants ^Integ-2Invariants

- ✅ Each form must have clear responsibility
- ✅ Form data must flow through MVVM or coordinator pattern
- ✅ Modal dialogs must return `DialogResult`
- ✅ Form state must be validated before commit

#### Behavior ^Integ-2Behavior

```vb
Public Class MainForm
    Inherits Form

    Private _canvas As CanvasControl
    Private _propertiesPanel As PropertiesPanel
    Private _selectionManager As SelectionManager
    Private _currentLayout As CanvasLayout
    Private _takeOffService As TakeOffService

    ''' <summary>Main form initialization</summary>
    Public Sub New()
        InitializeComponent()
        InitializeServices()
        InitializeEventHandlers()
    End Sub

    ''' <summary>Wire up all event handlers</summary>
    Private Sub InitializeEventHandlers()
        ' Canvas events
        AddHandler _canvas.SelectionChanged, AddressOf OnCanvasSelectionChanged
        AddHandler _canvas.ObjectCreated, AddressOf OnObjectCreated

        ' Property panel events
        AddHandler _propertiesPanel.PropertyChanged, AddressOf OnPropertyChanged

        ' Menu events
        AddHandler mnuFileNew.Click, AddressOf OnFileNew
        AddHandler mnuFileOpen.Click, AddressOf OnFileOpen
        AddHandler mnuFileSave.Click, AddressOf OnFileSave

        ' Tool events
        AddHandler btnSelect.Click, AddressOf OnToolSelect
        AddHandler btnLine.Click, AddressOf OnToolLine
        AddHandler btnRectangle.Click, AddressOf OnToolRectangle
    End Sub

    ''' <summary>Handle tool selection</summary>
    Private Sub OnToolSelect(sender As Object, e As EventArgs)
        _canvas.SetTool(ToolType.SelectTool)
        UpdateToolbarState(sender)
    End Sub

    Private Sub OnToolLine(sender As Object, e As EventArgs)
        _canvas.SetTool(ToolType.LineTool)
        UpdateToolbarState(sender)
    End Sub

    ''' <summary>Handle canvas selection changes</summary>
    Private Sub OnCanvasSelectionChanged(sender As Object, e As EventArgs)
        _propertiesPanel.SetSelection(_selectionManager.SelectedElements)
    End Sub

    ''' <summary>Handle property value changes</summary>
    Private Sub OnPropertyChanged(sender As Object, e As PropertyChangedEventArgs)
        Try
            ApplyPropertyChangesToSelection(e.PropertyName, e.NewValue)
            _canvas.Invalidate()
        Catch ex As Exception
            Logger.LogError($"Failed to apply property change: {ex.Message}")
            MessageBox.Show($"Error: {ex.Message}", "Property Update Failed")
        End Try
    End Sub

    ''' <summary>Create new drawing</summary>
    Private Sub OnFileNew(sender As Object, e As EventArgs)
        _currentLayout = New CanvasLayout With {
            .Id = Guid.NewGuid,
            .Name = "New Drawing",
            .Unit = "metric",
            .ScaleFactor = 1.0,
            .CreatedAt = DateTime.Now
        }

        ' Add default layer
        _currentLayout.CreateLayer("Default")

        _canvas.SetLayout(_currentLayout)
        _selectionManager.ClearSelection()
    End Sub
End Class
```

#### Validation ^Integ-2Validation

```vb
Public Module FormOrchestrationValidation
    Public Sub ValidateFormState(form As MainForm)
        ' Verify all child controls are initialized
        ' Check that layout is set in canvas
        ' Verify services are resolved
    End Sub
End Module
```

---

## 🔄 Cross-Layer Communication ^Cross-Layer

#### Invariants ^Integ-3Invariants

- ✅ Communication must flow downward (UI → Application → Domain)
- ✅ Events must bubble upward (Domain state → Application → UI)
- ✅ No layer may directly access UI from business logic
- ✅ All data must be validated at layer boundaries

#### Behavior ^Integ-3Behavior

```vb
''' <summary>Event system for cross-layer communication</summary>
Public Interface IDomainEventPublisher
    Event ElementCreated As EventHandler(Of ElementCreatedEvent)
    Event ElementModified As EventHandler(Of ElementModifiedEvent)
    Event ElementDeleted As EventHandler(Of ElementDeletedEvent)
    Event CalculationCompleted As EventHandler(Of CalculationCompletedEvent)

    Sub PublishElementCreated(element As CanvasElement)
    Sub PublishElementModified(element As CanvasElement)
    Sub PublishElementDeleted(elementId As Guid)
    Sub PublishCalculationCompleted(result As TakeOffResult)
End Interface

Public Class DomainEventPublisher
    Implements IDomainEventPublisher

    Public Event ElementCreated As EventHandler(Of ElementCreatedEvent)
    Public Event ElementModified As EventHandler(Of ElementModifiedEvent)
    Public Event ElementDeleted As EventHandler(Of ElementDeletedEvent)
    Public Event CalculationCompleted As EventHandler(Of CalculationCompletedEvent)

    Public Sub PublishElementCreated(element As CanvasElement) Implements IDomainEventPublisher.PublishElementCreated
        RaiseEvent ElementCreated(Me, New ElementCreatedEvent With {.Element = element})
    End Sub

    Public Sub PublishElementModified(element As CanvasElement) Implements IDomainEventPublisher.PublishElementModified
        RaiseEvent ElementModified(Me, New ElementModifiedEvent With {.Element = element})
    End Sub

    Public Sub PublishElementDeleted(elementId As Guid) Implements IDomainEventPublisher.PublishElementDeleted
        RaiseEvent ElementDeleted(Me, New ElementDeletedEvent With {.ElementId = elementId})
    End Sub

    Public Sub PublishCalculationCompleted(result As TakeOffResult) Implements IDomainEventPublisher.PublishCalculationCompleted
        RaiseEvent CalculationCompleted(Me, New CalculationCompletedEvent With {.Result = result})
    End Sub
End Class

' Usage in Application Service
Public Class TakeOffService
    Private _eventPublisher As IDomainEventPublisher

    Public Sub New(eventPublisher As IDomainEventPublisher)
        _eventPublisher = eventPublisher
    End Sub

    Public Sub CreateElement(element As CanvasElement)
        ' Domain logic
        ' ...
        ' Publish event
        _eventPublisher.PublishElementCreated(element)
    End Sub
End Class

' UI subscribes to events
' In MainForm.InitializeEventHandlers()
AddHandler _eventPublisher.ElementCreated, AddressOf OnElementCreated
```

#### Validation ^Integ-3Validation

```vb
Public Module CommunicationValidation
    Public Sub ValidateEventHandlerRegistration(form As MainForm, publisher As IDomainEventPublisher)
        ' Verify all domain events have handlers
        ' Check for memory leaks (unsubscribed events)
    End Sub
End Module
```

---

# 📋 Cross-Cutting Patterns ^Cross-Cutting

---

## ✅ Validation Strategy ^Validation-Strategy

### Entry-Point Validation (Per-Layer) ^Entry-Point-Validation

```vb
' UI Layer: User input validation
Private Sub TextBoxQuantity_Leave(sender As Object, e As EventArgs)
    If Not Double.TryParse(textQuantity.Text, _quantity) Then
        textQuantity.BackColor = Color.Red
        lblError.Text = "Quantity must be a number"
        Return
    End If
    If _quantity < 0 Then
        textQuantity.BackColor = Color.Red
        lblError.Text = "Quantity cannot be negative"
        Return
    End If
    textQuantity.BackColor = Color.White
    lblError.Text = ""
End Sub

' Application Layer: Business logic validation
Public Sub UpdateQuantity(elementId As Guid, newQuantity As Double)
    Dim element = _layout.Elements.FirstOrDefault(Function(e) e.Id = elementId)
    If element Is Nothing Then
        Throw New ArgumentException("Element not found")
    End If

    If newQuantity < 0 Then
        Throw New InvalidOperationException("Quantity cannot be negative")
    End If

    Dim business = JsonConvert.DeserializeObject(Of BusinessDefinition)(element.BusinessJson)
    business.Quantity = newQuantity
    element.BusinessJson = JsonConvert.SerializeObject(business)
End Sub

' Domain Layer: Entity validation
Public Class CanvasElement
    Private _quantity As Double

    Public Property Quantity As Double
        Get
            Return _quantity
        End Get
        Set(value As Double)
            If value < 0 Then
                Throw New InvalidOperationException("Quantity cannot be negative")
            End If
            _quantity = value
        End Set
    End Property
End Class
```

---

## 🔍 Error Handling Strategy ^Error-Handling

```vb
' Global exception handler
Public Class GlobalExceptionHandler
    Public Shared Sub Configure()
        AddHandler AppDomain.CurrentDomain.UnhandledException, AddressOf HandleUnhandledException
        AddHandler Application.ThreadException, AddressOf HandleThreadException
    End Sub

    Private Shared Sub HandleUnhandledException(sender As Object, e As UnhandledExceptionEventArgs)
        Dim ex = CType(e.ExceptionObject, Exception)
        Logger.LogError($"Unhandled exception: {ex.Message}{vbCrLf}{ex.StackTrace}")
        MessageBox.Show("An unexpected error occurred. Please check logs.", "Error")
        Application.Exit()
    End Sub

    Private Shared Sub HandleThreadException(sender As Object, e As ThreadExceptionEventArgs)
        Logger.LogError($"Thread exception: {e.Exception.Message}")
        MessageBox.Show($"An error occurred: {e.Exception.Message}", "Error")
    End Sub
End Class
```

---

## 🧪 Testing Hooks ^Testing-Hooks

```vb
' Dependency injection for testing
Public Interface ICanvasControlFactory
    Function CreateCanvas() As CanvasControl
End Interface

Public Class TestableCanvasControlFactory
    Implements ICanvasControlFactory

    Public Function CreateCanvas() As CanvasControl Implements ICanvasControlFactory.CreateCanvas
        ' Return mock or test canvas
        Return New CanvasControl()
    End Function
End Class

' Usage in tests
<Test>
Public Sub TestLineCreation()
    Dim factory = New TestableCanvasControlFactory()
    Dim canvas = factory.CreateCanvas()

    ' Create test layout
    Dim layout = New CanvasLayout With {
        .Elements = New List(Of CanvasElement)
    }
    layout.CreateLayer("Test")

    canvas.SetLayout(layout)

    ' Test line tool
    Dim lineTool = New LineTool(canvas)
    lineTool.OnMouseDown(New PointF(0, 0))
    lineTool.OnMouseDown(New PointF(10, 10))

    ' Verify element created
    Assert.AreEqual(1, layout.Elements.Count)
    Assert.AreEqual("Line", layout.Elements(0).Type)
End Sub
```

---

# 📊 Reference Tables ^Reference

---

## Shape Type & Valid Dimension Modes ^Valid-Dimension

| Shape | D0 | D1 | D2 | D3 |
|-------|----|----|----|----|
| Text | ❌ | ❌ | ❌ | ❌ |
| Line | ✅ | ✅ | ❌ | ❌ |
| Rectangle | ✅ | ✅ | ✅ | ❌ |
| Circle/Ellipse | ✅ | ❌ | ✅ | ❌ |
| Polyline | ✅ | ✅ | ❌ | ❌ |
| Symbol | ✅ | ✅ | ✅ | ✅ |

---

## Component Dependency Graph ^Component-Dependency

```
Foundation
  ├── Domain/Entities
  ├── Domain/Utilities
  └── Infrastructure/*

Rendering (depends on Foundation)
  ├── CanvasControl
  ├── LineShape
  └── ShapeFactory

Interaction (depends on Foundation + Rendering)
  ├── ToolSystem
  ├── SelectionManager
  └── EventHandlers

Business (depends on Foundation + Infrastructure)
  ├── TakeOffCalculator
  ├── TakeOffService
  └── MaterialService

Integration (depends on all)
  ├── CompositionRoot
  ├── MainForm
  └── DomainEventPublisher
```

---

## Checklist: Adding New Feature ^Adding-New-Feature

- [ ] Define entity invariants (Domain layer)
- [ ] Add validation module (Domain layer)
- [ ] Implement behavior in entity or service (Business layer)
- [ ] Create UI control or form (Rendering/Interaction layer)
- [ ] Wire events through DomainEventPublisher (Integration layer)
- [ ] Add unit tests for validation
- [ ] Add integration tests for workflows
- [ ] Update this guide with new invariants
- [ ] Document in SDLC (Mega-File.md)
- [ ] Code review checklist

---

# 🚀 Quick Start Commands ^Quick-Start

---

## Running Tests ^Running-Tests

```powershell
# Unit tests only
dotnet test --filter "Category=Unit"

# Integration tests
dotnet test --filter "Category=Integration"

# All tests
dotnet test

# With coverage
dotnet test /p:CollectCoverage=true
```

---

## Building & Deploying ^Building-Deploying

```powershell
# Clean build
dotnet clean
dotnet build -c Release

# Standalone executable
dotnet publish -c Release -r win-x64 --self-contained

# Create installer
# (configured in project file)
msbuild /p:Configuration=Release /p:Platform=x64
```

---

# 📞 Contact & Support ^Support

- **Architecture Questions:** Review SDLC docs (Mega-File.md)
- **Code Questions:** Check relevant README.md in each project
- **Issues:** Log with affected layer and use case reference

---

**Document Version:** 1.0  
**Last Updated:** 2025  
**Maintained by:** Architecture Team

