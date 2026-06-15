'Filename: Desktop/Program.vb
Option Strict On
Imports System.Windows.Forms.Application
Imports System.Threading
Imports System.Windows.Forms
Imports Desktop.Forms

' Namespace CoNSoL.Desktop.Program
Module Program
    Sub Main()
        SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException)
        AddHandler ThreadException, AddressOf OnThreadException
        AddHandler AppDomain.CurrentDomain.UnhandledException, AddressOf OnUnhandledException

        CompositionRoot.Bootstrap()
        EnableVisualStyles()
        SetCompatibleTextRenderingDefault(False)
        Run(New MainForm())
    End Sub

    Private Sub OnThreadException(sender As Object, e As Threading.ThreadExceptionEventArgs)
        CompositionRoot.Logger.Error("UI exception", e.Exception)
        MessageBox.Show("An unexpected error occurred. Details were logged.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
    End Sub

    ' v1.1 Original code had a bug where e.ExceptionObject was not cast to Exception, causing a crash when logging. Fixed by adding TryCast and null check.
    'Private Sub OnUnhandledException(sender As Object, e As UnhandledExceptionEventArgs)
    '    Dim ex = TryCast(e.ExceptionObject, Exception)
    '    ' CompositionRoot.Logger.Error("Unhandled exception", ex)
    'End Sub

    ' V1.2 Added null check for logger to prevent potential NullReferenceException if logger is not initialized.
    Private Sub OnUnhandledException(sender As Object, e As UnhandledExceptionEventArgs)
        Dim ex = TryCast(e.ExceptionObject, Exception)
        If CompositionRoot.Logger IsNot Nothing Then
            CompositionRoot.Logger.Error("Unhandled exception", ex)
        End If
    End Sub

End Module
'End Namespace
