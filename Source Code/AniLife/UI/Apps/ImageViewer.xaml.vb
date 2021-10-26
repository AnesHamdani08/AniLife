Imports System.ComponentModel

Public Class ImageViewer
    Public Sub New(File As String)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        IGViewer.ImageSource = BitmapFrame.Create(New Uri(File, UriKind.Absolute))
    End Sub
    Public Sub New(File As ImageSource)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        IGViewer.ImageSource = BitmapFrame.Create(File)
    End Sub

    Private Sub ImageViewer_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        IGViewer.ImageSource = Nothing
    End Sub
End Class
