'Filename: Desktop/Forms/MainForm.vb
Option Strict On
Imports System.Security.Cryptography
Imports Desktop.CompositionRoot
Imports Desktop.Controls
Imports Domain.Entities
Imports Domain.Services
Imports Infrastructure.IO
Imports Application
Imports Application.AI
Imports Desktop


Namespace Forms
	''' <summary>
	''' Main application window for CoNSoL-TakeOff.
	''' 
	''' Provides the primary user interface including:
	''' - Drawing canvas (CanvasControl)
	''' - Tool buttons (Select, Line, Rectangle, Circle, Ellipse, Polyline, Pan, Zoom)
	''' - File operations (New, Open, Save)
	''' - Utility functions (Grid toggle)
	''' - Status bar for feedback
	''' </summary>
	''' <remarks>
	''' Architecture:
	''' - CanvasControl: 2D drawing surface with rendering and tool support
	''' - Left Panel: Contains tool buttons and file operations
	''' - StatusStrip: Shows messages and application state
	''' - PropertiesPanel: (Future) Shows properties of selected objects
	''' 
	''' The form coordinates between UI and domain layers:
	''' - Receives user input (drawing, tool selection)
	''' - Calls application services for persistence
	''' - Updates canvas with drawing state
	''' 
	''' Related Use Cases:
	''' - UC-001: Draw shapes on canvas
	''' - UC-008: File operations (New, Open, Save)
	''' </remarks>
	Public Class MainForm
		Inherits Form

		''' <summary>Primary 2D drawing canvas control.</summary>
		Private ReadOnly _canvas As New CanvasControl With {.Dock = DockStyle.Fill}

		''' <summary>Left sidebar panel containing tool buttons.</summary>
		Private ReadOnly _left As New Panel With {.Dock = DockStyle.Left, .Width = 250}

		''' <summary>Status bar for displaying messages and state.</summary>
		Private ReadOnly _status As New StatusStrip()

		''' <summary>Properties panel for editing selected object properties (future use).</summary>
		Private ReadOnly _propertiesPanel As New PropertiesPanel()

		''' <summary>Stores the last takeoff result for export and reference.</summary>
		Private ReadOnly lastResult As TakeOffResult

		''' <summary>Current drawing layout (canvas state).</summary>
		Private CurrentLayout As CanvasLayout

		'Private propertiesPanel As New PropertiesPanel()

		' ? CONTROL NAME: layerPanel
		Private layerPanel As LayerPanel
		Private layerManager As New LayerManager()

		''' <summary>
		''' Initializes the main form with UI components and event handlers.
		''' </summary>
		''' <remarks>
		''' Initialization sequence (order is critical):
		''' 1. Call InitializeComponent() (designer-generated code)
		''' 2. Set form properties (title, size)
		''' 3. Create and wire tool buttons
		''' 4. Add controls to form
		''' 5. Load README files (informational)
		''' 
		''' All exceptions are logged but not re-thrown to allow application startup.
		''' </remarks>
		Public Sub New()
			layerPanel = New LayerPanel()
			layerPanel.Initialize(layerManager)
			Try
				Logger.Info("Initializing MainForm")
				' ? MUST BE FIRST: Designer-generated initialization
				InitializeComponent()
				' Set form properties
				Me.Text = "CoNSoL-TakeOff"
				Me.Width = 1200
				Me.Height = 800

				' Initialize UI components
				InitializeToolButtons()

				' Add controls to form

				Me.Controls.Add(_canvas)
				Me.Controls.Add(_left)
				Me.Controls.Add(_status)
				Me.Controls.Add(layerPanel)
				Me.Controls.Add(_propertiesPanel)

				AddHandler _canvas.ElementSelected, AddressOf OnElementSelected

				' Load documentation (non-critical)
				LoadReadmeFiles()

				Logger.Info("MainForm initialization complete")

			Catch ex As Exception
				Logger.Error($"MainForm initialization failed: {ex.Message}", ex)
				MessageBox.Show(
					$"Failed to initialize application: {ex.Message}",
					"Initialization Error",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error)
			End Try
		End Sub


		Private Sub OnElementSelected(el As CanvasElement)
			_propertiesPanel.SetElement(el)
		End Sub


		''' <summary>
		''' Initializes all tool buttons and their event handlers.
		''' </summary>
		''' <remarks>
		''' Creates buttons for:
		''' - Drawing tools: Select, Line, Rectangle, Ellipse, Polyline
		''' - Navigation: Pan, Zoom In/Out
		''' - Utilities: Toggle Grid
		''' - File ops: New Layout, Open Layout, Save Layout
		''' 
		''' Buttons are added in reverse order to left panel (docking from top).
		''' </remarks>
		Private Sub InitializeToolButtons()
			Try
				Logger.Info("Initializing tool buttons")

				' Create tool buttons
				Dim btnSelect = CreateToolButton("Select", Sub() HandleToolClick(ToolType.SelectTool, "Select"))
				Dim btnLine = CreateToolButton("Line", Sub() HandleToolClick(ToolType.Line, "Line"))
				Dim btnRect = CreateToolButton("Rectangle", Sub() HandleToolClick(ToolType.Rectangle, "Rectangle"))
				Dim btnEllipse = CreateToolButton("Ellipse", Sub() HandleToolClick(ToolType.Ellipse, "Ellipse"))
				Dim btnPolyline = CreateToolButton("Polyline", Sub() HandleToolClick(ToolType.Polyline, "Polyline"))
				Dim btnPan = CreateToolButton("Pan", Sub() HandleToolClick(ToolType.Pan, "Pan"))

				' Create utility buttons
				Dim btnZoomIn = CreateToolButton("Zoom +", Sub() HandleZoomIn())
				Dim btnZoomOut = CreateToolButton("Zoom -", Sub() HandleZoomOut())
				Dim btnGrid = CreateToolButton("Toggle Grid", Sub() HandleGridToggle())

				' Create file operation buttons
				Dim btnNew = CreateToolButton("New Layout", Sub() NewLayout(), heightOverride:=40)
				Dim btnOpen = CreateToolButton("Open Layout", Sub() OpenLayout(), heightOverride:=40)
				Dim btnSave = CreateToolButton("Save Layout", Sub() SaveLayout(), heightOverride:=40)
				Dim btnExportExcel = CreateToolButton("Export Excel", Sub() ExportExcel_Click(), heightOverride:=40)

				' Import AI button 
				Dim btnImportAI = CreateToolButton("Import", Sub() ImportAI_Click(), heightOverride:=40)
				btnImportAI.Dock = DockStyle.Top



				' Add buttons to panel (in reverse order due to docking from top)
				_left.Controls.Add(btnSave)
				_left.Controls.Add(btnOpen)
				_left.Controls.Add(btnNew)
				_left.Controls.Add(btnGrid)
				_left.Controls.Add(btnZoomOut)
				_left.Controls.Add(btnZoomIn)
				_left.Controls.Add(btnPan)
				_left.Controls.Add(btnRect)
				_left.Controls.Add(btnLine)
				_left.Controls.Add(btnSelect)
				_left.Controls.Add(btnEllipse)
				_left.Controls.Add(btnPolyline)
				_left.Controls.Add(btnExportExcel)
				Me.Controls.Add(btnImportAI)


				'layerManager.Initialize()
				layerManager.EnsureDefaultLayer()


				Logger.Info("Tool buttons created and added to panel")

			Catch ex As Exception
				Logger.Error($"Failed to initialize tool buttons: {ex.Message}", ex)
				Throw
			End Try
		End Sub

		Private Sub ImportAI_Click() 'sender As Object, e As EventArgs)
			Dim dlg As New OpenFileDialog()
			dlg.Filter = "Images|*.png;*.jpg;*.jpeg"
			If dlg.ShowDialog() <> DialogResult.OK Then Exit Sub
			Dim ai As New AiIntakeService()
			Dim result = ai.ProcessDrawing(dlg.FileName)
			Dim scale = result.DetectedScale
			If String.IsNullOrEmpty(scale) Then
				scale = "1:100" ' ✅ fallback default
			End If
			Dim confirmed = MessageBox.Show(
				$"Detected Scale: {scale}" & vbCrLf & "Confirm?",
				"Scale Detection",
				MessageBoxButtons.YesNo)
			' After confirmation
			Dim layout As New CanvasLayout()
			For Each el In result.DetectedElements
				layout.Elements.Add(el)
			Next
			' Fix: call instance method on the _canvas instance instead of referencing the type.
			' Use the same method used elsewhere in this class for consistency.
			_canvas.LoadFromLayout(layout)
			MessageBox.Show($"Detected {layout.Elements.Count} elements!")
			' User cancel/select No
			If confirmed = DialogResult.No Then
				scale = InputBox("Enter correct scale:", "Scale", scale)
			End If
			MessageBox.Show($"Final Scale: {scale}")
		End Sub

		''' <summary>
		''' Exports the takeoff results to an Excel file using the ExcelExporter service.
		''' </summary>
		''' <param name=""></param>
		Private Sub ExportExcel_Click()
			If lastResult Is Nothing Then
				MessageBox.Show("No calculation result available ❌")
				Exit Sub
			End If
			Dim exporter As New ExcelExporter()
			ExcelExporter.Export(lastResult, "takeoff.xlsx")
			MessageBox.Show("Exported ✅")
		End Sub


		''' <summary>
		''' Creates a tool button with standard styling.
		''' </summary>
		''' <param name="text">Button label text</param>
		''' <param name="clickHandler">Event handler for button click</param>
		''' <param name="heightOverride">Optional custom height (default: 40)</param>
		''' <returns>Configured button control</returns>
		Private Function CreateToolButton(text As String, clickHandler As Action, Optional heightOverride As Integer = 40) As Button
			Dim btn = New Button With {
				.Text = text,
				.Dock = DockStyle.Top,
				.Height = heightOverride,
				.BackColor = SystemColors.Control,
				.FlatStyle = FlatStyle.Standard
			}
			AddHandler btn.Click, Sub() clickHandler()
			Return btn
		End Function

		''' <summary>Handles tool button click with logging.</summary>
		''' <param name="toolType">Tool type to activate</param>
		''' <param name="toolName">Human-readable tool name for logging</param>
		Private Sub HandleToolClick(toolType As ToolType, toolName As String)
			Try
				Logger.Info($"Tool clicked: {toolName}")
				_canvas.SetTool(toolType)
				UpdateStatusBar($"Tool: {toolName}")
			Catch ex As Exception
				Logger.Error($"Failed to set tool {toolName}: {ex.Message}", ex)
				UpdateStatusBar($"Error setting {toolName}", isError:=True)
			End Try
		End Sub

		''' <summary>
		''' Handles Zoom In button click.</summary>
		Private Sub HandleZoomIn()
			Try
				Logger.Info("Zoom In clicked")
				_canvas.ZoomIn()
				UpdateStatusBar("Zoom In")
			Catch ex As Exception
				Logger.Error($"Zoom In failed: {ex.Message}", ex)
				UpdateStatusBar("Zoom In failed", isError:=True)
			End Try
		End Sub

		''' <summary>Handles Zoom Out button click.</summary>
		Private Sub HandleZoomOut()
			Try
				Logger.Info("Zoom Out clicked")
				_canvas.ZoomOut()
				UpdateStatusBar("Zoom Out")
			Catch ex As Exception
				Logger.Error($"Zoom Out failed: {ex.Message}", ex)
				UpdateStatusBar("Zoom Out failed", isError:=True)
			End Try
		End Sub

		''' <summary>Handles Grid Toggle button click.</summary>
		Private Sub HandleGridToggle()
			Try
				Logger.Info("Grid toggle clicked")
				_canvas.ToggleGrid()
				UpdateStatusBar("Grid toggled")
			Catch ex As Exception
				Logger.Error($"Grid toggle failed: {ex.Message}", ex)
				UpdateStatusBar("Grid toggle failed", isError:=True)
			End Try
		End Sub

		''' <summary>
		''' Updates status bar message.
		''' </summary>
		''' <param name="message">Message to display</param>
		''' <param name="isError">Whether this is an error message (false for info)</param>
		Private Sub UpdateStatusBar(message As String, Optional isError As Boolean = False)
			Dim item = New ToolStripStatusLabel With {
				.Text = $"{DateTime.Now:HH:mm:ss} | {message}",
				.ForeColor = If(isError, Color.Red, SystemColors.ControlText)
			}
			_status.Items.Clear()
			_status.Items.Add(item)
		End Sub

		''' <summary>
		''' Loads and displays README files from each layer.
		''' </summary>
		''' <remarks>
		''' Non-critical function. Displays README files in message boxes.
		''' Errors are logged but do not prevent application startup.
		''' </remarks>
		Private Sub LoadReadmeFiles()
			Try
				Logger.Info("Loading README files")

				Dim readmePaths = New String() {
					"E:\Users\GoingIForMal\CoNSoL-TakeOff\Desktop\README.md", '"Desktop/README.md", 'E:\Users\GoingIForMal\CoNSoL-TakeOff\Desktop\README.md
					"E:\Users\GoingIForMal\CoNSoL-TakeOff\Infrastructure\README.md", '"Infrastructure/README.md",
					"E:\Users\GoingIForMal\CoNSoL-TakeOff\Application\README.md", '"Application/README.md",
					"E:\Users\GoingIForMal\CoNSoL-TakeOff\Domain\README.md" '"Domain/README.md"
				}

				Dim foundCount = 0

				For Each path In readmePaths
					Try
						Dim fullPath = IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path)
						If IO.File.Exists(fullPath) Then
							Logger.Info($"Found README: {path}")
							' Intentionally skip showing these to avoid message boxes on startup
							'Dim content = IO.File.ReadAllText(fullPath)
							'MessageBox.Show(content, $"README - {IO.Path.GetFileNameWithoutExtension(path)}", MessageBoxButtons.OK, MessageBoxIcon.Information)
							foundCount += 1
						Else
							Logger.Info($"README not found: {path}")
						End If
					Catch ex As Exception
						Logger.Warn($"Failed to load README {path}: {ex.Message}")
					End Try
				Next

				Logger.Info($"README load complete: {foundCount} files found")

			Catch ex As Exception
				Logger.Error($"Unexpected error loading README files: {ex.Message}", ex)
				' Non-critical, don't show error dialog
			End Try
		End Sub

		''' <summary>
		''' Creates a new blank drawing layout.
		''' </summary>
		''' <remarks>
		''' Clears the canvas and starts with a fresh CanvasLayout.
		''' The layout is empty until elements are drawn.
		''' </remarks>
		Private Sub NewLayout()
			Try
				Logger.Info("Creating new layout")

				' Create new layout
				CurrentLayout = New CanvasLayout()
				Logger.Info($"New layout created: {CurrentLayout.CanvasId}")

				' Clear canvas
				_canvas.Clear()

				UpdateStatusBar("New layout created")
				Logger.Info("New layout ready for drawing")

			Catch ex As Exception
				Logger.Error($"Failed to create new layout: {ex.Message}", ex)
				UpdateStatusBar("Error creating layout", isError:=True)
				MessageBox.Show(
					$"Failed to create new layout: {ex.Message}",
					"New Layout Error",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error)
			End Try
		End Sub

		''' <summary>
		''' Opens a previously saved drawing layout from disk.
		''' </summary>
		''' <remarks>
		''' Supports two formats:
		''' - .takeoff: Encrypted binary format
		''' - .json: Unencrypted JSON format
		''' 
		''' File format is auto-detected by extension.
		''' </remarks>
		Private Sub OpenLayout()
			Dim openPath As String = Nothing
			Try
				Logger.Info("Opening layout file dialog")
				Using ofd As New OpenFileDialog With {
					.Filter = "TakeOff (*.takeoff)|*.takeoff|JSON (*.json)|*.json",
					.Title = "Open Drawing Layout"
					}
					If ofd.ShowDialog() <> DialogResult.OK Then
						Logger.Info("Open dialog canceled")
						Return
					End If
					openPath = ofd.FileName
					Logger.Info($"Opening file: {openPath}")
				End Using
				' Determine if file is encrypted based on extension
				Dim encrypted = openPath.EndsWith(".takeoff", StringComparison.OrdinalIgnoreCase)

				' Load layout from file
				Dim store = New TakeOffFileStore(CompositionRoot.Crypto)
				CurrentLayout = store.Load(openPath, encrypted:=encrypted)
				Logger.Info($"File loaded. Layout ID: {CurrentLayout.CanvasId}")

				' Update canvas with loaded layout
				_canvas.LoadFromLayout(CurrentLayout)

				UpdateStatusBar($"Layout opened: {IO.Path.GetFileName(openPath)}")
				Logger.Info($"Layout opened successfully: {CurrentLayout.CanvasId}")

			Catch ex As Exception
				Logger.Error($"Failed to open layout: {ex.Message}", ex)
				UpdateStatusBar("Failed to open layout", isError:=True)
				MessageBox.Show(
					$"Failed to open layout: {ex.Message}",
					"Open Layout Error",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error)
			End Try
		End Sub

		''' <summary>
		''' Saves the current drawing layout to disk.
		''' </summary>
		''' <remarks>
		''' Supports two formats:
		''' - .takeoff: Encrypted binary format (encrypted with AES + HMAC)
		''' - .json: Unencrypted JSON format
		''' 
		''' File format is auto-detected by extension.
		''' A random nonce is generated for each save to ensure encryption security.
		''' </remarks>
		Private Sub SaveLayout()
			Using sfd As New SaveFileDialog With {.Filter = "TakeOff (*.takeoff)|*.takeoff|JSON (*.json)|*.json"}
				If sfd.ShowDialog() = DialogResult.OK Then
					Dim store = New TakeOffFileStore(CompositionRoot.Crypto)
					Dim encrypted = sfd.FileName.EndsWith(".takeoff", StringComparison.OrdinalIgnoreCase)
					Dim nonce = New Byte(11) {}
					RandomNumberGenerator.Fill(nonce)
					Dim layout = _canvas.ToLayout()
					store.Save(sfd.FileName, layout, encrypt:=encrypted, nonce:=nonce)
					CompositionRoot.Logger.Info("Layout saved")
				End If
			End Using
		End Sub

		''' <summary>
		''' Designer-generated initialization method (auto-generated code).
		''' </summary>
		''' <remarks>
		''' This method is generated by the Windows Forms designer.
		''' It initializes designer-managed components.
		''' 
		''' WARNING: Do not manually edit this method. Changes may be lost
		''' when the designer regenerates the code.
		''' </remarks>
		Private Sub InitializeComponent()
			SuspendLayout()
			' 
			' MainForm
			' 
			ClientSize = New Size(606, 428)
			Name = "MainForm"
			ResumeLayout(False)
		End Sub
	End Class
End Namespace
