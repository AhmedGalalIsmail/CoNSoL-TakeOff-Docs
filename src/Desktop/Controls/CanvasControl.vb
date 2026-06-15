
'Filename: Desktop/Controls/CanvasControl.vb
#Region "Info. & Imports"
'Option Strict On
'Imports System.Drawing
Imports System.Drawing.Drawing2D
'Imports System.Windows.Forms
Imports System.Text.Json
'Imports System.Linq
Imports Domain.Entities
'Imports Domain



#End Region



'Namespace Controls
#Region "CanvasControl and related types"

''' <summary>
''' available Tools for the interactive canvas.
''' </summary>
Public Enum ToolType
    ''' <summary>Select and manipulate existing shapes.</summary>
    SelectTool
    ''' <summary>Draw straight lines.</summary>
    Line
    ''' <summary>Draw rectangles.</summary>
    Rectangle
    ''' <summary>Draw ellipses.</summary>
    Ellipse
    ''' <summary>Draw Polyline</summary>
    Polyline
    ''' <summary>Pan the viewport.</summary>
    Pan
End Enum
#End Region

#Region "canvas control"
''' <summary>
''' Interactive 2D drawing canvas with shape rendering and tool support.
''' </summary>
''' <remarks>
''' CanvasControl is the primary UI component for drawing operations.
''' It manages:
''' - Shape rendering with zoom/pan support
''' - Tool activation (Line, Rectangle, Select, Pan, Zoom)
''' - Selection highlighting and feedback
''' - Layer visibility and filtering
''' - Double-buffered rendering for smooth updates
''' 
''' Coordinate Systems:
''' - Physical: Screen pixels (0,0 at top-left)
''' - Logical: Canvas units (user coordinate system)
''' - Transform: physical = (logical * zoom) + pan
''' 
''' Related Use Cases:
''' - UC-001: Draw shapes
''' - UC-006: Edit multi-selection
''' - UC-008: Deployment (serialization)
''' </remarks>
Public Class CanvasControl
    Inherits UserControl

    Private ReadOnly _shapes As New List(Of ShapeBase)()
    Private _selected As ShapeBase = Nothing
    Private _isDrawing As Boolean = False
    Private _startPt As PointF
    Private _currPt As PointF
    Private _tempShape As ShapeBase = Nothing
    Private _tool As ToolType = ToolType.SelectTool

    Private ReadOnly _shapeMenu As New ContextMenuStrip()

    ''' <summary>Current layout being rendered.</summary>
    ''' <remarks>Invariant: Never null. Set via SetLayout().</remarks>
    Private _currentLayout As CanvasLayout

    ''' <summary>Zoom factor (1.0 = 100%).</summary>
    ''' <remarks>Valid range: 0.1 to 10.0</remarks>
    Private _zoom As Single = 1.0F

    ''' <summary>Pan offset in physical coordinates.</summary>
    ''' <remarks>Represents top-left corner displacement.</remarks>
    Private _pan As PointF = New PointF(0, 0)
    Private _gridSize As Integer = 20
    Private _showGrid As Boolean = True
    Private _snapToGrid As Boolean = True
    Private _showRulers As Boolean = True

    Private _backBuffer As Bitmap = Nothing
    Private _backGraphics As Graphics = Nothing

    Public Property BusinessJson As String

    ''' <summary>Sets the layout to render and clears selection.</summary>
    ''' <param name="layout">Canvas layout to display</param>
    ''' <remarks>
    ''' Resets zoom/pan to defaults and clears any selection.
    ''' Triggers full repaint of canvas.
    ''' </remarks>
    ''' <exception cref="ArgumentNullException">If layout is Nothing</exception>
    Public Sub SetLayout(layout As CanvasLayout)
        _currentLayout = layout
        _zoom = 1.0F
        _pan = New PointF(0, 0)
        Invalidate()
    End Sub

    Private Function FindShapeByElementId(id As Guid) As ShapeBase
        Return _shapes.FirstOrDefault(Function(s) s.ElementId = id)
    End Function

    Private Function FindShapeByElementId(id As String) As ShapeBase
        Dim gid As Guid
        If Not Guid.TryParse(id, gid) Then Return Nothing

        Return _shapes.FirstOrDefault(Function(s) s.ElementId = gid)
    End Function


    Private Sub DrawDashedOutline(
g As Graphics,
shape As ShapeBase,
zoom As Single,
pan As PointF,
color As Color,
thickness As Single
)
        Using pen As New Pen(color, thickness)
            pen.DashStyle = DashStyle.Dash
            Dim r = shape.GetBounds(zoom, pan)
            r.Inflate(3, 3) ' small visual offset
            g.DrawRectangle(pen, r)
        End Using
    End Sub


    Private Sub DrawNestedOverlays(g As Graphics, zoom As Single, pan As PointF)
        If _currentLayout Is Nothing Then Return
        If _currentLayout.Relationships Is Nothing Then Return

        For Each rel In _currentLayout.Relationships
            Dim parent = FindShapeByElementId(rel.ParentElementId)
            Dim child = FindShapeByElementId(rel.ChildElementId)

            If parent Is Nothing OrElse child Is Nothing Then Continue For

            Select Case rel.RelationshipType
                Case ElementRelationshipType.Nested
                    ' Child is part of parent (logical containment)
                    DrawDashedOutline(g, child, zoom, pan, Color.DimGray, 1)

                    ' Parent gets a heavier outline
                    DrawDashedOutline(g, parent, zoom, pan, Color.Gray, 2)

                Case ElementRelationshipType.Exclusion
                    ' Exclusion (subtract from parent)
                    DrawDashedOutline(g, child, zoom, pan, Color.Red, 2)
            End Select
        Next
    End Sub


    Private Sub AssignBlockToSelectedShape()
        If _selected Is Nothing Then Return

        Using dlg As New BlockAssignmentForm()
            dlg.BusinessJson = _selected.BusinessJson

            If dlg.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
                _selected.BusinessJson = dlg.BusinessJson
            End If
        End Using
    End Sub

    ''' <summary>
    ''' Initializes a new instance of <see cref="CanvasControl"/>.
    ''' Sets default styles for smooth painting and initializes the cursor/background.
    ''' </summary>
    Public Sub New()
        Me.DoubleBuffered = True
        Me.SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.OptimizedDoubleBuffer, True)
        Me.BackColor = Color.White
        Me.Cursor = Cursors.Cross

        Dim assignBlockItem = New ToolStripMenuItem("Assign Block...")
        AddHandler assignBlockItem.Click, AddressOf AssignBlockToSelectedShape

        _shapeMenu.Items.Add(assignBlockItem)

    End Sub


#Region "Public API"
    ''' <summary>
    ''' Switches the active tool used by the canvas.
    ''' </summary>
    ''' <param name="tool">The tool to set.</param>
    Public Sub SetTool(tool As ToolType)
        _tool = tool
        _isDrawing = False
        _tempShape = Nothing
        Me.Cursor = If(tool = ToolType.Pan, Cursors.Hand, Cursors.Cross)
        Invalidate()
    End Sub

    ''' <summary>
    ''' Increase the canvas zoom by a fixed factor and request repaint.
    ''' Zooms in 25%.
    ''' </summary>
    'Public Sub ZoomIn()
    '    _zoom *= 1.2F
    '    Invalidate()
    'End Sub

    ''' <summary>Zooms in 25%.</summary>
    Public Sub ZoomIn()
        _zoom = Math.Min(_zoom * 1.25F, 10.0F)
        Invalidate()
    End Sub

    '''' <summary>
    '''' Decrease the canvas zoom by a fixed factor and request repaint.
    '''' Zoom is clamped to a small positive minimum.
    '''' </summary>
    'Public Sub ZoomOut()
    '    _zoom /= 1.2F
    '    If _zoom < 0.1F Then _zoom = 0.1F
    '    Invalidate()
    'End Sub

    ''' <summary>Zooms out 20%.</summary>
    Public Sub ZoomOut()
        _zoom = Math.Max(_zoom * 0.8F, 0.1F)
        Invalidate()
    End Sub

    ''' <summary>
    ''' Toggle rendering of the snap/grid overlay and request repaint.
    ''' </summary>
    Public Sub ToggleGrid()
        _showGrid = Not _showGrid
        Invalidate()
    End Sub

    ''' <summary>
    ''' Toggle snapping behavior when placing or drawing shapes.
    ''' </summary>
    Public Sub ToggleSnap()
        _snapToGrid = Not _snapToGrid
    End Sub

    ''' <summary>
    ''' Remove all shapes from the canvas and clear selection.
    ''' </summary>
    Public Sub Clear()
        _shapes.Clear()
        _selected = Nothing
        Invalidate()
    End Sub

    ''' <summary>
    ''' Export current canvas contents into a domain <see cref="CanvasLayout"/> instance.
    ''' </summary>
    ''' <returns>A <see cref="CanvasLayout"/> representing current elements on the canvas.</returns>
    Public Function ToLayout() As CanvasLayout
        Dim layout As New CanvasLayout() With {.Unit = "meter", .ScaleFactor = 1.0}
        For Each s In _shapes
            Dim elem As New CanvasElement() With {
            .Type = If(TypeOf s Is LineShape, "line",
                    If(TypeOf s Is RectShape, "rectangle",
                    If(TypeOf s Is EllipseShape, "ellipse",
                    If(TypeOf s Is PolylineShape, "polyline", "unknown")))),
            .Layer = "default",
            .GeometryJson = s.ToGeometryJson(),
            .BusinessJson = "{}"
            }
            layout.Elements.Add(elem)
        Next
        Return layout
    End Function
#End Region


    ''' <summary>
    ''' Load shapes from a <see cref="CanvasLayout"/> instance, replacing current contents.
    ''' </summary>
    ''' <param name="layout">The layout to load from.</param>
    Public Sub LoadFromLayout(layout As CanvasLayout)
        _shapes.Clear()
        For Each e In layout.Elements
            Select Case e.Type
                Case "line"
                    Dim ls As New LineShape()
                    ls.FromGeometryJson(e.GeometryJson)
                    _shapes.Add(ls)
                Case "rectangle"
                    Dim rs As New RectShape()
                    rs.FromGeometryJson(e.GeometryJson)
                    _shapes.Add(rs)

                Case "ellipse"
                    Dim es As New EllipseShape()
                    es.FromGeometryJson(e.GeometryJson)
                    _shapes.Add(es)

                Case "polyline"
                    Dim ps As New PolylineShape()
                    ps.FromGeometryJson(e.GeometryJson)
                    _shapes.Add(ps)
            End Select
        Next
        Invalidate()
    End Sub

#Region "Input handling and rendering"
    ''' <summary>
    ''' Custom painting logic renders grid, rulers, shapes and transient drawing state to a back-buffer.
    ''' </summary>
    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)

        If _backBuffer Is Nothing OrElse _backBuffer.Size <> Me.ClientSize Then
            _backBuffer = New Bitmap(Me.Width, Me.Height)
            _backGraphics = Graphics.FromImage(_backBuffer)
        End If

        _backGraphics.SmoothingMode = SmoothingMode.AntiAlias
        _backGraphics.Clear(Me.BackColor)

        If _showGrid Then DrawGrid(_backGraphics)
        If _showRulers Then DrawRulers(_backGraphics)

        For Each s In _shapes
            s.Draw(_backGraphics, _zoom, _pan)
        Next

        If _selected IsNot Nothing Then
            Using pen As New Pen(Color.DarkOrange, 2)
                pen.DashStyle = DashStyle.Dash
                _backGraphics.DrawRectangle(pen, _selected.GetBounds(_zoom, _pan))
            End Using
        End If

        If _isDrawing AndAlso _tempShape IsNot Nothing Then
            Using pen As New Pen(Color.SteelBlue, 2)
                pen.DashStyle = DashStyle.Dot
                _tempShape.Draw(_backGraphics, _zoom, _pan, pen)
            End Using
        End If

        e.Graphics.DrawImageUnscaled(_backBuffer, 0, 0)
    End Sub

    ''' <summary>
    ''' Mouse down handler: begins drawing, selection or panning depending on the active tool.
    ''' </summary>
    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        Dim lp = ScreenToWorld(e.Location)
        If _snapToGrid Then lp = Snap(lp)

        If e.Button = MouseButtons.Right Then
            _selected = HitTest(lp)
            If _selected IsNot Nothing Then
                _shapeMenu.Show(Me, e.Location)
                Return
            End If
        End If

        Select Case _tool
            Case ToolType.Pan
                _startPt = e.Location
                Cursor = Cursors.SizeAll
                Return
            Case ToolType.SelectTool
                _selected = HitTest(lp)
                Invalidate()
            Case ToolType.Line
                _isDrawing = True
                _startPt = lp
                _tempShape = New LineShape() With {.Start = lp, .[End] = lp}
            Case ToolType.Rectangle
                _isDrawing = True
                _startPt = lp
                _tempShape = New RectShape() With {.TopLeft = lp, .Width = 0, .Height = 0}
            Case ToolType.Polyline
                '_isDrawing = True
                '_startPt = lp
                ''_tempShape = New PolylineShape()

                _isDrawing = True
                _tempShape = New PolylineShape()
                CType(_tempShape, PolylineShape).Points.Add(lp)
            Case ToolType.Ellipse
                _isDrawing = True
                _startPt = lp
                _tempShape = New EllipseShape() With {
                .Center = lp,
                .RadiusX = 0,
                .RadiusY = 0
                }
        End Select
    End Sub

    ''' <summary>
    ''' Mouse move handler: updates drawing preview or pans viewport when appropriate.
    ''' </summary>
    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e)
        Dim lp = ScreenToWorld(e.Location)
        If _snapToGrid Then lp = Snap(lp)

        If _tool = ToolType.Pan AndAlso e.Button = MouseButtons.Left Then
            Dim dx = e.Location.X - _startPt.X
            Dim dy = e.Location.Y - _startPt.Y
            _pan = New PointF(_pan.X + dx, _pan.Y + dy)
            _startPt = e.Location
            Invalidate()
            Return
        End If

        If _isDrawing Then
            _currPt = lp
            If TypeOf _tempShape Is LineShape Then
                CType(_tempShape, LineShape).[End] = lp
            ElseIf TypeOf _tempShape Is RectShape Then
                Dim r = CType(_tempShape, RectShape)
                r.Width = Math.Abs(lp.X - _startPt.X)
                r.Height = Math.Abs(lp.Y - _startPt.Y)
                r.TopLeft = New PointF(Math.Min(_startPt.X, lp.X), Math.Min(_startPt.Y, lp.Y))
            ElseIf TypeOf _tempShape Is EllipseShape Then
                Dim ee = CType(_tempShape, EllipseShape)
                ee.RadiusX = Math.Abs(lp.X - _startPt.X)
                ee.RadiusY = Math.Abs(lp.Y - _startPt.Y)
                ee.Center = New PointF(Math.Min(_startPt.X, lp.X), Math.Min(_startPt.Y, lp.Y))
            ElseIf TypeOf _tempShape Is PolylineShape Then
                'If _isDrawing Then
                Dim pl = CType(_tempShape, PolylineShape)
                    pl.Points.Add(lp)
                    Invalidate()
                'End If
            End If
            Invalidate()
        End If
    End Sub

    ''' <summary>
    ''' Mouse up handler: finalizes transient drawing state and commits new shapes.
    ''' </summary>
    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
        MyBase.OnMouseUp(e)

        If _tool = ToolType.Polyline AndAlso e.Clicks = 2 Then
            If _tempShape IsNot Nothing AndAlso _tempShape.IsValid() Then
                _shapes.Add(_tempShape)
            End If
            _isDrawing = False
            _tempShape = Nothing
            Invalidate()
            Return
        End If

        If _tool = ToolType.Pan Then
            Cursor = Cursors.Hand
            Return
        End If

        If Not _isDrawing Then Return

        If _tempShape IsNot Nothing AndAlso _tempShape.IsValid() Then
            _shapes.Add(_tempShape)
        End If

        _isDrawing = False
        _tempShape = Nothing
        Invalidate()
    End Sub

    ''' <summary>
    ''' Converts screen pixel coordinates to canvas logical coordinates.
    ''' </summary>
    ''' <param name="screenPoint">Point in screen pixels</param>
    ''' <returns>Point in canvas logical units</returns>
    ''' <remarks>
    ''' Formula: logical = (physical - pan) / zoom
    ''' Used for tool interaction (mouse clicks, drags).
    ''' </remarks>
    Private Function PhysicalToLogical(screenPoint As PointF) As PointF
        Return New PointF(
                (screenPoint.X - _pan.X) / _zoom,
                (screenPoint.Y - _pan.Y) / _zoom)
    End Function

    ''' <summary>
    ''' Convert a screen coordinate to world/canvas coordinates applying pan and zoom.
    ''' </summary>
    ''' <param name="p">Screen point in control coordinates.</param>
    ''' <returns>Point in world/canvas coordinate space.</returns>
    Private Function ScreenToWorld(p As Point) As PointF
        Return New PointF((p.X - _pan.X) / _zoom, (p.Y - _pan.Y) / _zoom)
    End Function

    ''' <summary>
    ''' Convert a world/canvas coordinate to screen coordinates applying pan and zoom.
    ''' </summary>
    ''' <param name="p">Point in world/canvas coordinate space.</param>
    ''' <returns>Point in control (screen) coordinate space.</returns>
    Private Function WorldToScreen(p As PointF) As Point
        Return New Point(CInt(p.X * _zoom + _pan.X), CInt(p.Y * _zoom + _pan.Y))
    End Function

    ''' <summary>
    ''' Snap a world point to the current grid.
    ''' </summary>
    ''' <param name="p">Point in world coordinates.</param>
    ''' <returns>Snapped point in world coordinates.</returns>
    Private Function Snap(p As PointF) As PointF
        Dim sx = Math.Round(p.X / _gridSize) * _gridSize
        Dim sy = Math.Round(p.Y / _gridSize) * _gridSize
        Return New PointF(CSng(sx), CSng(sy))
    End Function

    ''' <summary>
    ''' Hit-test shapes from top-most to bottom-most and return the first matching shape.
    ''' </summary>
    ''' <param name="lp">Location in world coordinates.</param>
    ''' <returns>The hit <see cref="ShapeBase"/> or <c>Nothing</c> if none match.</returns>
    Private Function HitTest(lp As PointF) As ShapeBase
        For i = _shapes.Count - 1 To 0 Step -1
            If _shapes(i).HitTest(lp) Then Return _shapes(i)
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Draw the grid lines to the provided graphics surface.
    ''' </summary>
    ''' <param name="g">Graphics surface to draw on.</param>
    Private Sub DrawGrid(g As Graphics)
        Using pen As New Pen(Color.Gainsboro)
            For x = 0 To Me.Width Step CInt(_gridSize * _zoom)
                g.DrawLine(pen, x + _pan.X Mod (CInt(_gridSize * _zoom)), 0, x + _pan.X Mod (CInt(_gridSize * _zoom)), Me.Height)
            Next
            For y = 0 To Me.Height Step CInt(_gridSize * _zoom)
                g.DrawLine(pen, 0, y + _pan.Y Mod (CInt(_gridSize * _zoom)), Me.Width, y + _pan.Y Mod (CInt(_gridSize * _zoom)))
            Next
        End Using
    End Sub

    ''' <summary>
    ''' Draw horizontal and vertical rulers on the top and left edges of the control.
    ''' </summary>
    ''' <param name="g">Graphics surface to draw on.</param>
    Private Sub DrawRulers(g As Graphics)
        Using br As New SolidBrush(Color.LightSteelBlue)
            g.FillRectangle(br, 0, 0, Me.Width, 20)
            g.FillRectangle(br, 0, 0, 20, Me.Height)
        End Using
        Using pen As New Pen(Color.DarkSlateGray)
            g.DrawLine(pen, 20, 20, Me.Width, 20)
            g.DrawLine(pen, 20, 20, 20, Me.Height)
        End Using
    End Sub
End Class
#End Region
#End Region

#Region "Shape definitions"
''' <summary>
''' Base class for drawable shapes on the canvas.
''' Implementations must provide drawing, hit testing, bounds and (de)serialization behavior.
''' </summary>
Public MustInherit Class ShapeBase
    ''' <summary>Unique identifier for this shape, linking to domain elements.</summary>
    Public Property ElementId As Guid = Guid.NewGuid()

    ''' <summary>Layer identifier for organizing shapes (e.g., "default", "overlay").</summary>
    Public Property LayerId As String = "default"

    Public Property BusinessJson As String
    ''' <summary>
    ''' Draw the shape to the given graphics surface using the specified zoom and pan.
    ''' </summary>
    ''' <param name="g">Graphics surface to draw on.</param>
    ''' <param name="zoom">Current zoom factor.</param>
    ''' <param name="pan">Current pan offset.</param>
    ''' <param name="pen">Optional pen to use for drawing (default is shape-specific).</param>
    Public MustOverride Sub Draw(g As Graphics, zoom As Single, pan As PointF, Optional pen As Pen = Nothing)
    ''' <summary>
    ''' Determine whether the given world point intersects the shape.
    ''' </summary>
    ''' <param name="lp">Point in world coordinates.</param>
    ''' <returns><c>True</c> if the point hits the shape; otherwise <c>False</c>.</returns>
    Public MustOverride Function HitTest(lp As PointF) As Boolean
    ''' <summary>
    ''' Return the bounding rectangle in screen coordinates for selection visuals.
    ''' </summary>
    ''' <param name="zoom">Current zoom factor.</param>
    ''' <param name="pan">Current pan offset.</param>
    ''' <returns>A <see cref="Rectangle"/> representing screen-space bounds.</returns>
    Public MustOverride Function GetBounds(zoom As Single, pan As PointF) As Rectangle
    ''' <summary>
    ''' Validate whether the shape has sufficient size/definition to be committed.
    ''' </summary>
    ''' <returns><c>True</c> if valid; otherwise <c>False</c>.</returns>
    Public MustOverride Function IsValid() As Boolean
    ''' <summary>
    ''' Serialize the shape geometry to a JSON string appropriate for storage in a <see cref="CanvasElement"/>.
    ''' </summary>
    ''' <returns>JSON string representing the shape geometry.</returns>
    Public MustOverride Function ToGeometryJson() As String
    ''' <summary>
    ''' Populate shape geometry from a previously serialized JSON payload.
    ''' </summary>
    ''' <param name="json">JSON geometry payload.</param>
    Public MustOverride Sub FromGeometryJson(json As String)
End Class
#End Region

#Region "Specific shape implementations"

#Region "LineShape"
''' <summary>
''' Straight line shape defined by a start and end point in world coordinates.
''' </summary>
Public Class LineShape
    Inherits ShapeBase
    ''' <summary>Start point in world coordinates.</summary>
    Public Property Start As PointF
    ''' <summary>End point in world coordinates.</summary>
    Public Property [End] As PointF

    ''' <inheritdoc/>
    Public Overrides Sub Draw(g As Graphics, zoom As Single, pan As PointF, Optional pen As Pen = Nothing)
        Dim p1 = New PointF(Start.X * zoom + pan.X, Start.Y * zoom + pan.Y)
        Dim p2 = New PointF([End].X * zoom + pan.X, [End].Y * zoom + pan.Y)
        Using p As Pen = If(pen, New Pen(Color.DodgerBlue, 2))
            g.DrawLine(p, p1, p2)
        End Using
    End Sub

    ''' <inheritdoc/>
    Public Overrides Function HitTest(lp As PointF) As Boolean
        Dim dx = [End].X - Start.X
        Dim dy = [End].Y - Start.Y
        Dim length2 = dx * dx + dy * dy
        If length2 = 0 Then Return False
        Dim t = ((lp.X - Start.X) * dx + (lp.Y - Start.Y) * dy) / length2
        t = Math.Max(0, Math.Min(1, t))
        Dim proj = New PointF(Start.X + t * dx, Start.Y + t * dy)
        Dim dist = Math.Sqrt((lp.X - proj.X) ^ 2 + (lp.Y - proj.Y) ^ 2)
        Return dist < 5
    End Function

    ''' <inheritdoc/>
    Public Overrides Function GetBounds(zoom As Single, pan As PointF) As Rectangle
        Dim p1 = New Point(CInt(Start.X * zoom + pan.X), CInt(Start.Y * zoom + pan.Y))
        Dim p2 = New Point(CInt([End].X * zoom + pan.X), CInt([End].Y * zoom + pan.Y))
        Dim minX = Math.Min(p1.X, p2.X)
        Dim minY = Math.Min(p1.Y, p2.Y)
        Dim maxX = Math.Max(p1.X, p2.X)
        Dim maxY = Math.Max(p1.Y, p2.Y)
        Return New Rectangle(minX, minY, maxX - minX, maxY - minY)
    End Function

    ''' <inheritdoc/>
    Public Overrides Function IsValid() As Boolean
        Return Math.Abs(Start.X - [End].X) + Math.Abs(Start.Y - [End].Y) > 2
    End Function

    ''' <inheritdoc/>
    Public Overrides Function ToGeometryJson() As String
        Return JsonSerializer.Serialize(New With {.start = New With {.x = Start.X, .y = Start.Y}, .[end] = New With {.x = [End].X, .y = [End].Y}})
    End Function

    ''' <inheritdoc/>
    Public Overrides Sub FromGeometryJson(json As String)
        Dim doc = JsonDocument.Parse(json)
        Dim s = doc.RootElement.GetProperty("start")
        Dim e = doc.RootElement.GetProperty("end")
        Start = New PointF(s.GetProperty("x").GetSingle(), s.GetProperty("y").GetSingle())
        [End] = New PointF(e.GetProperty("x").GetSingle(), e.GetProperty("y").GetSingle())
    End Sub
End Class
#End Region

#Region "RectShape"
''' <summary>
''' Axis-aligned rectangle shape in world coordinates.
''' </summary>
Public Class RectShape
    Inherits ShapeBase
    ''' <summary>Top-left corner in world coordinates.</summary>
    Public Property TopLeft As PointF
    ''' <summary>Width in world units.</summary>
    Public Property Width As Single
    ''' <summary>Height in world units.</summary>
    Public Property Height As Single

    ''' <inheritdoc/>
    Public Overrides Sub Draw(g As Graphics, zoom As Single, pan As PointF, Optional pen As Pen = Nothing)
        Dim r = New RectangleF(TopLeft.X * zoom + pan.X, TopLeft.Y * zoom + pan.Y, Width * zoom, Height * zoom)
        Using p As Pen = If(pen, New Pen(Color.ForestGreen, 2))
            g.DrawRectangle(p, Rectangle.Round(r))
        End Using
    End Sub

    ''' <inheritdoc/>
    Public Overrides Function HitTest(lp As PointF) As Boolean
        Return lp.X >= TopLeft.X AndAlso lp.X <= TopLeft.X + Width AndAlso lp.Y >= TopLeft.Y AndAlso lp.Y <= TopLeft.Y + Height
    End Function

    ''' <inheritdoc/>
    Public Overrides Function GetBounds(zoom As Single, pan As PointF) As Rectangle
        Dim r = New RectangleF(TopLeft.X * zoom + pan.X, TopLeft.Y * zoom + pan.Y, Width * zoom, Height * zoom)
        Return Rectangle.Round(r)
    End Function

    ''' <inheritdoc/>
    Public Overrides Function IsValid() As Boolean
        Return Width >= 5 AndAlso Height >= 5
    End Function

    ''' <inheritdoc/>
    Public Overrides Function ToGeometryJson() As String
        Return JsonSerializer.Serialize(New With {.topLeft = New With {.x = TopLeft.X, .y = TopLeft.Y}, .width = Width, .height = Height})
    End Function

    ''' <inheritdoc/>
    Public Overrides Sub FromGeometryJson(json As String)
        Dim doc = JsonDocument.Parse(json)
        Dim tl = doc.RootElement.GetProperty("topLeft")
        TopLeft = New PointF(tl.GetProperty("x").GetSingle(), tl.GetProperty("y").GetSingle())
        Width = doc.RootElement.GetProperty("width").GetSingle()
        Height = doc.RootElement.GetProperty("height").GetSingle()
    End Sub
End Class
#End Region

#Region "EllipseShape"
''' <summary>
''' Axis-aligned ellipse shape defined by a bounding rectangle in world coordinates.
''' </summary>
Public Class EllipseShape
    Inherits ShapeBase

    'Public Property TopLeft As PointF
    'Public Property Width As Single
    'Public Property Height As Single

    Public Property Center As PointF
    Public Property RadiusX As Single
    Public Property RadiusY As Single


    Public Overrides Sub Draw(g As Graphics, zoom As Single, pan As PointF, Optional pen As Pen = Nothing)
        Dim r = New RectangleF(
        Center.X * zoom + pan.X,
        Center.Y * zoom + pan.Y,
        RadiusX * zoom,
        RadiusY * zoom
    )
        Using p As Pen = If(pen, New Pen(Color.MediumPurple, 2))
            g.DrawEllipse(p, r)
        End Using
    End Sub

    Public Overrides Function HitTest(lp As PointF) As Boolean
        Dim rx = RadiusX / 2.0F
        Dim ry = RadiusY / 2.0F
        If rx <= 0 OrElse ry <= 0 Then Return False

        Dim cx = Center.X + rx
        Dim cy = Center.Y + ry

        Dim dx = (lp.X - cx) / rx
        Dim dy = (lp.Y - cy) / ry

        Return dx * dx + dy * dy <= 1.0F
    End Function

    Public Overrides Function GetBounds(zoom As Single, pan As PointF) As Rectangle
        Dim r = New RectangleF(
        Center.X * zoom + pan.X,
        Center.Y * zoom + pan.Y,
        RadiusX * zoom,
        RadiusY * zoom
    )
        Return Rectangle.Round(r)
    End Function

    Public Overrides Function IsValid() As Boolean
        Return RadiusX >= 5 AndAlso RadiusY >= 5
    End Function

    Public Overrides Function ToGeometryJson() As String
        Return JsonSerializer.Serialize(New With {
        .topLeft = New With {.x = Center.X, .y = Center.Y},
        .width = RadiusX,
        .height = RadiusY
    })
    End Function

    Public Overrides Sub FromGeometryJson(json As String)
        Dim doc = JsonDocument.Parse(json)
        Dim tl = doc.RootElement.GetProperty("topLeft")
        Center = New PointF(tl.GetProperty("x").GetSingle(), tl.GetProperty("y").GetSingle())
        RadiusX = doc.RootElement.GetProperty("width").GetSingle()
        RadiusY = doc.RootElement.GetProperty("height").GetSingle()
    End Sub
End Class
#End Region

#Region "PolylineShape"
''' <summary>
''' Polyline shape consisting of multiple connected segments.
''' </summary>
Public Class PolylineShape
    Inherits ShapeBase

    Public ReadOnly Property Points As New List(Of PointF)

    Public Overrides Sub Draw(g As Graphics, zoom As Single, pan As PointF, Optional pen As Pen = Nothing)
        If Points.Count < 2 Then Return

        Using p As Pen = If(pen, New Pen(Color.IndianRed, 2))
            Dim screenPts = Points.Select(
                Function(pt) New PointF(pt.X * zoom + pan.X, pt.Y * zoom + pan.Y)
            ).ToArray()

            g.DrawLines(p, screenPts)
        End Using
    End Sub

    Public Overrides Function HitTest(lp As PointF) As Boolean
        For i = 0 To Points.Count - 2
            Dim a = Points(i)
            Dim b = Points(i + 1)

            Dim dx = b.X - a.X
            Dim dy = b.Y - a.Y
            Dim len2 = dx * dx + dy * dy
            If len2 = 0 Then Continue For

            Dim t = ((lp.X - a.X) * dx + (lp.Y - a.Y) * dy) / len2
            t = Math.Max(0, Math.Min(1, t))

            Dim proj = New PointF(a.X + t * dx, a.Y + t * dy)
            Dim dist = Math.Sqrt((lp.X - proj.X) ^ 2 + (lp.Y - proj.Y) ^ 2)

            If dist < 5 Then Return True
        Next
        Return False
    End Function

    Public Overrides Function GetBounds(zoom As Single, pan As PointF) As Rectangle
        Dim minX = Points.Min(Function(p) p.X)
        Dim minY = Points.Min(Function(p) p.Y)
        Dim maxX = Points.Max(Function(p) p.X)
        Dim maxY = Points.Max(Function(p) p.Y)

        Dim r = New RectangleF(
            minX * zoom + pan.X,
            minY * zoom + pan.Y,
            (maxX - minX) * zoom,
            (maxY - minY) * zoom
        )
        Return Rectangle.Round(r)
    End Function

    Public Overrides Function IsValid() As Boolean
        Return Points.Count >= 2
    End Function

    Public Overrides Function ToGeometryJson() As String
        Return JsonSerializer.Serialize(
            Points.Select(Function(p) New With {.x = p.X, .y = p.Y})
        )
    End Function

    Public Overrides Sub FromGeometryJson(json As String)
        Points.Clear()
        Dim doc = JsonDocument.Parse(json)
        For Each pt In doc.RootElement.EnumerateArray()
            Points.Add(New PointF(
                pt.GetProperty("x").GetSingle(),
                pt.GetProperty("y").GetSingle()
            ))
        Next
    End Sub
End Class
#End Region

#End Region







'End Namespace

'-----------------
'#Region "Info. & Imports"

'' Enforce strict typing rules (no implicit narrowing conversions)
'Option Strict On

'' Core GDI+ drawing types (Graphics, Pen, PointF, etc.)
'Imports System.Drawing

'' Advanced drawing features (smoothing, dash styles, transforms)
'Imports System.Drawing.Drawing2D

'' Windows Forms base classes (UserControl, MouseEventArgs, etc.)
'Imports System.Windows.Forms

'' JSON serialization for geometry persistence
'Imports System.Text.Json

'' Domain entities used for saving/loading layouts
'Imports CoNSoL.Domain.Entities
'#End Region


'Namespace Controls

'    ''' <summary>
'    ''' Tools available for the interactive canvas.
'    ''' Controls how mouse input is interpreted.
'    ''' </summary>
'    Public Enum ToolType
'        ''' <summary>Select and manipulate existing shapes.</summary>
'        SelectTool
'        ''' <summary>Draw straight lines.</summary>
'        Line
'        ''' <summary>Draw rectangles.</summary>
'        Rectangle
'        ''' <summary>Draw ellipses.</summary>
'        Ellipse
'        ''' <summary>Draw connected line segments.</summary>
'        Polyline
'        ''' <summary>Pan (move) the viewport.</summary>
'        Pan
'    End Enum


'#Region "canvas control"

'    ''' <summary>
'    ''' Interactive canvas control that supports drawing, selection,
'    ''' panning, zooming, grid snapping, rulers, and serialization.
'    ''' </summary>
'    Public Class CanvasControl
'        Inherits UserControl

'        ' Collection of all committed shapes on the canvas
'        Private ReadOnly _shapes As New List(Of ShapeBase)()

'        ' Currently selected shape (if any)
'        Private _selected As ShapeBase = Nothing

'        ' Indicates an active draw operation
'        Private _isDrawing As Boolean = False

'        ' Mouse positions in world coordinates
'        Private _startPt As PointF
'        Private _currPt As PointF

'        ' Temporary shape while drawing (preview)
'        Private _tempShape As ShapeBase = Nothing

'        ' Currently active tool
'        Private _tool As ToolType = ToolType.SelectTool

'        ' View transform state
'        Private _zoom As Single = 1.0F
'        Private _pan As PointF = New PointF(0, 0)

'        ' Grid and snapping configuration
'        Private _gridSize As Integer = 20
'        Private _showGrid As Boolean = True
'        Private _snapToGrid As Boolean = True
'        Private _showRulers As Boolean = True

'        ' Manual back-buffering to avoid flicker
'        Private _backBuffer As Bitmap = Nothing
'        Private _backGraphics As Graphics = Nothing


'        ''' <summary>
'        ''' Constructor sets up painting styles and defaults.
'        ''' </summary>
'        Public Sub New()
'            Me.DoubleBuffered = True
'            Me.SetStyle(ControlStyles.AllPaintingInWmPaint Or
'                        ControlStyles.UserPaint Or
'                        ControlStyles.OptimizedDoubleBuffer, True)

'            Me.BackColor = Color.White
'            Me.Cursor = Cursors.Cross
'        End Sub


'#Region "Public API"

'        ''' <summary>
'        ''' Switches the active drawing or interaction tool.
'        ''' Resets transient state.
'        ''' </summary>
'        Public Sub SetTool(tool As ToolType)
'            _tool = tool
'            _isDrawing = False
'            _tempShape = Nothing

'            ' Use a hand cursor for panning, crosshair otherwise
'            Me.Cursor = If(tool = ToolType.Pan, Cursors.Hand, Cursors.Cross)
'            Invalidate()
'        End Sub


'        ''' <summary>
'        ''' Zooms in by scaling view by a fixed multiplier.
'        ''' </summary>
'        Public Sub ZoomIn()
'            _zoom *= 1.2F
'            Invalidate()
'        End Sub


'        ''' <summary>
'        ''' Zooms out with a lower bound to avoid inversion.
'        ''' </summary>
'        Public Sub ZoomOut()
'            _zoom /= 1.2F
'            If _zoom < 0.1F Then _zoom = 0.1F
'            Invalidate()
'        End Sub


'        ''' <summary>
'        ''' Enable or disable background grid display.
'        ''' </summary>
'        Public Sub ToggleGrid()
'            _showGrid = Not _showGrid
'            Invalidate()
'        End Sub


'        ''' <summary>
'        ''' Enable or disable snapping to grid intersections.
'        ''' </summary>
'        Public Sub ToggleSnap()
'            _snapToGrid = Not _snapToGrid
'        End Sub


'        ''' <summary>
'        ''' Remove all shapes and reset selection.
'        ''' </summary>
'        Public Sub Clear()
'            _shapes.Clear()
'            _selected = Nothing
'            Invalidate()
'        End Sub


'        ''' <summary>
'        ''' Serialize all shapes into a domain CanvasLayout object.
'        ''' </summary>
'        Public Function ToLayout() As CanvasLayout
'            Dim layout As New CanvasLayout With {
'                .Unit = "meter",
'                .ScaleFactor = 1.0
'            }

'            ' Convert each shape to a CanvasElement
'            For Each s In _shapes
'                Dim elem As New CanvasElement With {
'                    .Type =
'                        If(TypeOf s Is LineShape, "line",
'                        If(TypeOf s Is RectShape, "rectangle",
'                        If(TypeOf s Is EllipseShape, "ellipse",
'                        If(TypeOf s Is PolylineShape, "polyline", "unknown")))),
'                    .Layer = "default",
'                    .GeometryJson = s.ToGeometryJson(),
'                    .BusinessJson = "{}"
'                }
'                layout.Elements.Add(elem)
'            Next

'            Return layout
'        End Function
'#End Region


'        ''' <summary>
'        ''' Clears the canvas and reconstructs shapes
'        ''' from a previously saved CanvasLayout.
'        ''' </summary>
'        Public Sub LoadFromLayout(layout As CanvasLayout)
'            _shapes.Clear()

'            For Each e In layout.Elements
'                Select Case e.Type
'                    Case "line"
'                        Dim ls As New LineShape()
'                        ls.FromGeometryJson(e.GeometryJson)
'                        _shapes.Add(ls)

'                    Case "rectangle"
'                        Dim rs As New RectShape()
'                        rs.FromGeometryJson(e.GeometryJson)
'                        _shapes.Add(rs)

'                    Case "ellipse"
'                        Dim es As New EllipseShape()
'                        es.FromGeometryJson(e.GeometryJson)
'                        _shapes.Add(es)

'                    Case "polyline"
'                        Dim ps As New PolylineShape()
'                        ps.FromGeometryJson(e.GeometryJson)
'                        _shapes.Add(ps)
'                End Select
'            Next

'            Invalidate()
'        End Sub


'#Region "Input handling and rendering"

'        ''' <summary>
'        ''' Main rendering routine.
'        ''' Draws grid, rulers, shapes, selection, and preview state.
'        ''' </summary>
'        Protected Overrides Sub OnPaint(e As PaintEventArgs)
'            MyBase.OnPaint(e)

'            ' Recreate back buffer if control size changes
'            If _backBuffer Is Nothing OrElse _backBuffer.Size <> Me.ClientSize Then
'                _backBuffer = New Bitmap(Me.Width, Me.Height)
'                _backGraphics = Graphics.FromImage(_backBuffer)
'            End If

'            _backGraphics.SmoothingMode = SmoothingMode.AntiAlias
'            _backGraphics.Clear(Me.BackColor)

'            If _showGrid Then DrawGrid(_backGraphics)
'            If _showRulers Then DrawRulers(_backGraphics)

'            ' Draw all committed shapes
'            For Each s In _shapes
'                s.Draw(_backGraphics, _zoom, _pan)
'            Next

'            ' Draw dashed selection rectangle
'            If _selected IsNot Nothing Then
'                Using pen As New Pen(Color.DarkOrange, 2)
'                    pen.DashStyle = DashStyle.Dash
'                    _backGraphics.DrawRectangle(pen, _selected.GetBounds(_zoom, _pan))
'                End Using
'            End If

'            ' Draw preview of currently drawn shape
'            If _isDrawing AndAlso _tempShape IsNot Nothing Then
'                Using pen As New Pen(Color.SteelBlue, 2)
'                    pen.DashStyle = DashStyle.Dot
'                    _tempShape.Draw(_backGraphics, _zoom, _pan, pen)
'                End Using
'            End If

'            ' Blit back buffer to screen
'            e.Graphics.DrawImageUnscaled(_backBuffer, 0, 0)
'        End Sub


'        ''' <summary>
'        ''' Mouse down initializes drawing, selection, or panning.
'        ''' </summary>
'        Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
'            MyBase.OnMouseDown(e)

'            Dim lp = ScreenToWorld(e.Location)
'            If _snapToGrid Then lp = Snap(lp)

'            ' Start panning
'            If _tool = ToolType.Pan Then
'                _startPt = e.Location
'                Cursor = Cursors.SizeAll
'                Return
'            End If

'            Select Case _tool
'                Case ToolType.SelectTool
'                    _selected = HitTest(lp)
'                    Invalidate()

'                Case ToolType.Line
'                    _isDrawing = True
'                    _startPt = lp
'                    _tempShape = New LineShape With {.Start = lp, .End = lp}

'                Case ToolType.Rectangle
'                    _isDrawing = True
'                    _startPt = lp
'                    _tempShape = New RectShape With {.TopLeft = lp, .Width = 0, .Height = 0}
'            End Select
'        End Sub


'        ''' <summary>
'        ''' Mouse move updates panning or drawing preview.
'        ''' </summary>
'        Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
'            MyBase.OnMouseMove(e)

'            Dim lp = ScreenToWorld(e.Location)
'            If _snapToGrid Then lp = Snap(lp)

'            ' Pan while dragging
'            If _tool = ToolType.Pan AndAlso e.Button = MouseButtons.Left Then
'                Dim dx = e.Location.X - _startPt.X
'                Dim dy = e.Location.Y - _startPt.Y
'                _pan = New PointF(_pan.X + dx, _pan.Y + dy)
'                _startPt = e.Location
'                Invalidate()
'                Return
'            End If

'            ' Update drawing preview
'            If _isDrawing Then
'                _currPt = lp
'                If TypeOf _tempShape Is LineShape Then
'                    CType(_tempShape, LineShape).[End] = lp
'                ElseIf TypeOf _tempShape Is RectShape Then
'                    Dim r = CType(_tempShape, RectShape)
'                    r.Width = Math.Abs(lp.X - _startPt.X)
'                    r.Height = Math.Abs(lp.Y - _startPt.Y)
'                    r.TopLeft = New PointF(Math.Min(_startPt.X, lp.X), Math.Min(_startPt.Y, lp.Y))
'                ElseIf TypeOf _tempShape Is EllipseShape Then
'                    Dim ee = CType(_tempShape, EllipseShape)
'                    ee.Width = Math.Abs(lp.X - _startPt.X)
'                    ee.Height = Math.Abs(lp.Y - _startPt.Y)
'                    ee.TopLeft = New PointF(Math.Min(_startPt.X, lp.X), Math.Min(_startPt.Y, lp.Y))
'                ElseIf TypeOf _tempShape Is PolylineShape Then
'                    'preview handled naturally via Draw()
'                End If
'                Invalidate()
'                'If TypeOf _tempShape Is LineShape Then
'                '    CType(_tempShape, LineShape).[End] = lp

'                'ElseIf TypeOf _tempShape Is RectShape Then
'                '    Dim r = CType(_tempShape, RectShape)
'                '    r.Width = Math.Abs(lp.X - _startPt.X)
'                '    r.Height = Math.Abs(lp.Y - _startPt.Y)
'                '    r.TopLeft = New PointF(Math.Min(_startPt.X, lp.X), Math.Min(_startPt.Y, lp.Y))
'                'End If

'                'Invalidate()
'            End If
'        End Sub


'        ''' <summary>
'        ''' Mouse up finalizes drawing operations.
'        ''' </summary>
'        Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
'            MyBase.OnMouseUp(e)

'            If _tool = ToolType.Pan Then
'                Cursor = Cursors.Hand
'                Return
'            End If

'            If Not _isDrawing Then Return

'            If _tempShape IsNot Nothing AndAlso _tempShape.IsValid() Then
'                _shapes.Add(_tempShape)
'            End If

'            _isDrawing = False
'            _tempShape = Nothing
'            Invalidate()
'        End Sub


'        ' ---------------- Coordinate helpers ----------------

'        ' Convert from screen pixels to world coordinates
'        Private Function ScreenToWorld(p As Point) As PointF
'            Return New PointF((p.X - _pan.X) / _zoom, (p.Y - _pan.Y) / _zoom)
'        End Function

'        ' Convert from world coordinates to screen pixels
'        Private Function WorldToScreen(p As PointF) As Point
'            Return New Point(CInt(p.X * _zoom + _pan.X), CInt(p.Y * _zoom + _pan.Y))
'        End Function

'        ' Snap point to nearest grid intersection
'        Private Function Snap(p As PointF) As PointF
'            Dim sx = Math.Round(p.X / _gridSize) * _gridSize
'            Dim sy = Math.Round(p.Y / _gridSize) * _gridSize
'            Return New PointF(CSng(sx), CSng(sy))
'        End Function


'        ' ---------------- Hit testing & overlays ----------------

'        ' Returns top-most shape under the cursor
'        Private Function HitTest(lp As PointF) As ShapeBase
'            For i = _shapes.Count - 1 To 0 Step -1
'                If _shapes(i).HitTest(lp) Then Return _shapes(i)
'            Next
'            Return Nothing
'        End Function

'        ' Draws background grid lines
'        Private Sub DrawGrid(g As Graphics)
'            Using pen As New Pen(Color.Gainsboro)
'                For x = 0 To Me.Width Step CInt(_gridSize * _zoom)
'                    g.DrawLine(pen, x + _pan.X Mod (CInt(_gridSize * _zoom)), 0,
'                                     x + _pan.X Mod (CInt(_gridSize * _zoom)), Me.Height)
'                Next
'                For y = 0 To Me.Height Step CInt(_gridSize * _zoom)
'                    g.DrawLine(pen, 0, y + _pan.Y Mod (CInt(_gridSize * _zoom)),
'                                     Me.Width, y + _pan.Y Mod (CInt(_gridSize * _zoom)))
'                Next
'            End Using
'        End Sub

'        ' Draws top and left rulers
'        Private Sub DrawRulers(g As Graphics)
'            Using br As New SolidBrush(Color.LightSteelBlue)
'                g.FillRectangle(br, 0, 0, Me.Width, 20)
'                g.FillRectangle(br, 0, 0, 20, Me.Height)
'            End Using
'            Using pen As New Pen(Color.DarkSlateGray)
'                g.DrawLine(pen, 20, 20, Me.Width, 20)
'                g.DrawLine(pen, 20, 20, 20, Me.Height)
'            End Using
'        End Sub

'    End Class
'#End Region


'    ' =========================================================================================
'    ' SHAPE MODEL LAYER
'    ' =========================================================================================

'#Region "Shape definitions"

'    ''' <summary>
'    ''' Abstract base class for all drawable canvas shapes.
'    ''' </summary>
'    Public MustInherit Class ShapeBase
'        Public MustOverride Sub Draw(g As Graphics, zoom As Single, pan As PointF, Optional pen As Pen = Nothing)
'        Public MustOverride Function HitTest(lp As PointF) As Boolean
'        Public MustOverride Function GetBounds(zoom As Single, pan As PointF) As Rectangle
'        Public MustOverride Function IsValid() As Boolean
'        Public MustOverride Function ToGeometryJson() As String
'        Public MustOverride Sub FromGeometryJson(json As String)
'    End Class

'#End Region
'#End Region


'End Namespace
'-------------------------------------------
