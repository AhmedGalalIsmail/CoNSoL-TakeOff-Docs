Option Strict On
Imports System.IO
Imports System.Text.Json
Imports System.Windows.Forms
Imports Domain.Entities
Imports Domain.Entities.BlockModels

Public Class MaterialCrudForm
    Inherits Form

    ' === Controls (your names) ===
    Private TVMaterial As TreeView
    Private TxtBlockCode As TextBox
    Private CboDimensionMode As ComboBox
    Friend WithEvents TxtUnit As TextBox
    Private CboDensity As ComboBox
    Private TxtPricePerUnit As TextBox
    Private ListView1 As ListView
    Friend WithEvents BtnSave As Button
    Friend WithEvents BtnDelete As Button

    ' === Storage ===
    Private ReadOnly _dataFile As String = "data\materials.json"
    Friend WithEvents Panel1 As Panel
    Friend WithEvents Panel4 As Panel
    Friend WithEvents Panel2 As Panel
    Friend WithEvents TxtCode As TextBox
    Friend WithEvents TxtPrice As TextBox
    Friend WithEvents TxtDensity As TextBox
    Friend WithEvents Panel3 As Panel
    Friend WithEvents BtnEdit As Button
    Friend WithEvents TVMaterials As TreeView
    Private ReadOnly _materials As New List(Of BlockModels.Material)

    Public Sub New()
        Me.Text = "Material CRUD (D0)"
        Me.Width = 820
        Me.Height = 520
        Me.StartPosition = FormStartPosition.CenterParent
        BuildUI()
        LoadMaterials()
        RefreshTree()
    End Sub

    ' ================= UI =================
    Private Sub BuildUI()
        Dim root As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 2
        }
        root.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 260))
        root.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))

        ' --- Left: TreeView ---
        TVMaterial = New TreeView With {.Dock = DockStyle.Fill}
        AddHandler TVMaterial.AfterSelect, AddressOf OnMaterialSelected
        root.Controls.Add(TVMaterial, 0, 0)
        root.SetRowSpan(TVMaterial, 2)

        ' --- Right: Editor ---
        Dim editor As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .RowCount = 8,
            .ColumnCount = 2,
            .Padding = New Padding(10)
        }
        editor.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 140))
        editor.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))

        TxtBlockCode = AddField(editor, "Code:")
        TxtUnit = AddField(editor, "Unit:")
        TxtPricePerUnit = AddField(editor, "Price / Unit:")

        CboDensity = New ComboBox With {.Dock = DockStyle.Fill}
        CboDensity.Items.AddRange(New Object() {"N/A", "Light", "Medium", "Heavy"})
        AddRow(editor, "Density:", CboDensity)

        CboDimensionMode = New ComboBox With {.Dock = DockStyle.Fill, .Enabled = False}
        CboDimensionMode.Items.Add("D0")
        CboDimensionMode.SelectedIndex = 0
        AddRow(editor, "Dim Mode:", CboDimensionMode)

        ' Placeholder (you asked 😁)
        ListView1 = New ListView With {.Dock = DockStyle.Fill, .Height = 80}
        AddRow(editor, "Preview:", ListView1)

        Dim btnPanel As New FlowLayoutPanel With {.Dock = DockStyle.Fill}
        BtnSave = New Button With {.Text = "Save", .Width = 80}
        BtnDelete = New Button With {.Text = "Delete", .Width = 80}
        AddHandler BtnSave.Click, AddressOf OnSave
        AddHandler BtnDelete.Click, AddressOf OnDelete
        btnPanel.Controls.AddRange({BtnSave, BtnDelete})
        editor.Controls.Add(btnPanel, 1, 7)

        root.Controls.Add(editor, 1, 0)
        Me.Controls.Add(root)
    End Sub

    Private Function AddField(panel As TableLayoutPanel, label As String) As TextBox
        Dim tb As New TextBox With {.Dock = DockStyle.Fill}
        AddRow(panel, label, tb)
        Return tb
    End Function

    Private Sub AddRow(panel As TableLayoutPanel, label As String, ctrl As Control)
        Dim r = panel.RowCount - 1
        panel.RowStyles.Add(New RowStyle(SizeType.Absolute, 32))
        panel.Controls.Add(New Label With {.Text = label, .Dock = DockStyle.Fill}, 0, r)
        panel.Controls.Add(ctrl, 1, r)
        panel.RowCount += 1
    End Sub

    ' ================= Data =================
    Private Sub LoadMaterials()
        _materials.Clear()
        If Not File.Exists(_dataFile) Then Return
        Dim json = File.ReadAllText(_dataFile)
        Dim list = JsonSerializer.Deserialize(Of List(Of BlockModels.Material))(json)
        If list IsNot Nothing Then _materials.AddRange(list)
    End Sub

    Private Sub SaveMaterials()
        Directory.CreateDirectory("data")
        Dim json = JsonSerializer.Serialize(_materials, New JsonSerializerOptions With {.WriteIndented = True})
        File.WriteAllText(_dataFile, json)
    End Sub

    ' ================= Logic =================
    Private Sub RefreshTree()
        TVMaterial.Nodes.Clear()
        For Each m In _materials
            Dim n = TVMaterial.Nodes.Add(m.Code)
            n.Nodes.Add($"Unit: {m.Unit}")
            n.Nodes.Add($"Density: {m.Density}")
            n.Nodes.Add($"Price: {m.PricePerUnit}")
        Next
        TVMaterial.ExpandAll()
    End Sub

    Private Sub OnSave(sender As Object, e As EventArgs)
        Dim code = TxtBlockCode.Text.Trim()
        If code = "" Then
            MessageBox.Show("Code is required")
            Return
        End If

        Dim m = _materials.FirstOrDefault(Function(x) x.Code = code)
        If m Is Nothing Then
            m = New BlockModels.Material()
            _materials.Add(m)
        End If

        m.Code = code
        m.Unit = TxtUnit.Text.Trim()
        m.Density = CboDensity.Text
        Decimal.TryParse(TxtPricePerUnit.Text, m.PricePerUnit)

        SaveMaterials()
        RefreshTree()
    End Sub

    Private Sub OnDelete(sender As Object, e As EventArgs)
        If TVMaterial.SelectedNode Is Nothing Then Return
        Dim code = TVMaterial.SelectedNode.Text
        _materials.RemoveAll(Function(m) m.Code = code)
        SaveMaterials()
        RefreshTree()
    End Sub

    Private Sub InitializeComponent()
        Panel1 = New Panel()
        Panel4 = New Panel()
        Panel2 = New Panel()
        TxtCode = New TextBox()
        TxtPrice = New TextBox()
        TxtUnit = New TextBox()
        TxtDensity = New TextBox()
        Panel3 = New Panel()
        BtnEdit = New Button()
        BtnDelete = New Button()
        BtnSave = New Button()
        TVMaterials = New TreeView()
        Panel1.SuspendLayout()
        Panel4.SuspendLayout()
        Panel2.SuspendLayout()
        Panel3.SuspendLayout()
        SuspendLayout()
        ' 
        ' Panel1
        ' 
        Panel1.Controls.Add(Panel4)
        Panel1.Controls.Add(TVMaterials)
        Panel1.Location = New System.Drawing.Point(12, 12)
        Panel1.Name = "Panel1"
        Panel1.Size = New System.Drawing.Size(717, 465)
        Panel1.TabIndex = 1
        ' 
        ' Panel4
        ' 
        Panel4.Controls.Add(Panel2)
        Panel4.Controls.Add(Panel3)
        Panel4.Location = New System.Drawing.Point(237, 3)
        Panel4.Name = "Panel4"
        Panel4.Size = New System.Drawing.Size(244, 227)
        Panel4.TabIndex = 10
        ' 
        ' Panel2
        ' 
        Panel2.Controls.Add(TxtCode)
        Panel2.Controls.Add(TxtPrice)
        Panel2.Controls.Add(TxtUnit)
        Panel2.Controls.Add(TxtDensity)
        Panel2.Location = New System.Drawing.Point(15, 16)
        Panel2.Name = "Panel2"
        Panel2.Size = New System.Drawing.Size(219, 151)
        Panel2.TabIndex = 5
        ' 
        ' TxtCode
        ' 
        TxtCode.Location = New System.Drawing.Point(13, 21)
        TxtCode.Name = "TxtCode"
        TxtCode.Size = New System.Drawing.Size(192, 23)
        TxtCode.TabIndex = 1
        ' 
        ' TxtPrice
        ' 
        TxtPrice.Location = New System.Drawing.Point(13, 108)
        TxtPrice.Name = "TxtPrice"
        TxtPrice.Size = New System.Drawing.Size(192, 23)
        TxtPrice.TabIndex = 4
        ' 
        ' TxtUnit
        ' 
        TxtUnit.Location = New System.Drawing.Point(13, 50)
        TxtUnit.Name = "TxtUnit"
        TxtUnit.Size = New System.Drawing.Size(192, 23)
        TxtUnit.TabIndex = 2
        ' 
        ' TxtDensity
        ' 
        TxtDensity.Location = New System.Drawing.Point(13, 79)
        TxtDensity.Name = "TxtDensity"
        TxtDensity.Size = New System.Drawing.Size(192, 23)
        TxtDensity.TabIndex = 3
        ' 
        ' Panel3
        ' 
        Panel3.Controls.Add(BtnEdit)
        Panel3.Controls.Add(BtnDelete)
        Panel3.Controls.Add(BtnSave)
        Panel3.Location = New System.Drawing.Point(15, 173)
        Panel3.Name = "Panel3"
        Panel3.Size = New System.Drawing.Size(219, 45)
        Panel3.TabIndex = 9
        ' 
        ' BtnEdit
        ' 
        BtnEdit.Location = New System.Drawing.Point(3, 7)
        BtnEdit.Name = "BtnEdit"
        BtnEdit.Size = New System.Drawing.Size(65, 28)
        BtnEdit.TabIndex = 7
        BtnEdit.Text = "Edit"
        BtnEdit.UseVisualStyleBackColor = True
        ' 
        ' BtnDelete
        ' 
        BtnDelete.Location = New System.Drawing.Point(74, 7)
        BtnDelete.Name = "BtnDelete"
        BtnDelete.Size = New System.Drawing.Size(68, 28)
        BtnDelete.TabIndex = 8
        BtnDelete.Text = "Delete"
        BtnDelete.UseVisualStyleBackColor = True
        ' 
        ' BtnSave
        ' 
        BtnSave.Location = New System.Drawing.Point(148, 7)
        BtnSave.Name = "BtnSave"
        BtnSave.Size = New System.Drawing.Size(66, 28)
        BtnSave.TabIndex = 6
        BtnSave.Text = "Save"
        BtnSave.UseVisualStyleBackColor = True
        ' 
        ' TVMaterials
        ' 
        TVMaterials.Location = New System.Drawing.Point(3, 3)
        TVMaterials.Name = "TVMaterials"
        TVMaterials.Size = New System.Drawing.Size(228, 459)
        TVMaterials.TabIndex = 0
        ' 
        ' MaterialCrudForm
        ' 
        ClientSize = New System.Drawing.Size(748, 490)
        Controls.Add(Panel1)
        Name = "MaterialCrudForm"
        Panel1.ResumeLayout(False)
        Panel4.ResumeLayout(False)
        Panel2.ResumeLayout(False)
        Panel2.PerformLayout()
        Panel3.ResumeLayout(False)
        ResumeLayout(False)

    End Sub

    Private Sub OnMaterialSelected(sender As Object, e As TreeViewEventArgs)
        Dim m = _materials.FirstOrDefault(Function(x) x.Code = e.Node.Text)
        If m Is Nothing Then Return

        TxtBlockCode.Text = m.Code
        TxtUnit.Text = m.Unit
        CboDensity.Text = m.Density
        TxtPricePerUnit.Text = m.PricePerUnit.ToString()
    End Sub

    ' ListView1
    ' -------------------------
    ' Current (Demo):
    ' - Placeholder UI element
    ' - No logic attached
    '
    ' Near Future:
    ' - Show material usage preview
    ' - Show calculated quantity per block
    ' - Show validation hints (e.g. missing unit/density)
    '
    ' Important:
    ' - Keep it even if unused now
    ' - Prevents UI rework later

End Class