Public Class CustomElementX
    Public OnClick As Action = Nothing
    Public Sub New(OnMouseClick As Action, Optional Image As ImageSource = Nothing)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.        
        OnClick = OnMouseClick
        If Image IsNot Nothing Then
            ItemCover.Source = Image
        End If
    End Sub
    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub
    Private Sub AnimeElement_MouseEnter(sender As Object, e As MouseEventArgs) Handles Me.MouseEnter
        BeginAnimation(BorderThicknessProperty, New Animation.ThicknessAnimation(New Thickness(3, 3, 3, 3), New Duration(TimeSpan.FromMilliseconds(100))))
    End Sub

    Private Sub AnimeElement_MouseLeave(sender As Object, e As MouseEventArgs) Handles Me.MouseLeave
        BeginAnimation(BorderThicknessProperty, New Animation.ThicknessAnimation(New Thickness(1, 1, 1, 1), New Duration(TimeSpan.FromMilliseconds(100))))
    End Sub
    Private Async Sub AnimeElementX_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles Me.MouseLeftButtonUp
        If OnClick IsNot Nothing Then
            Application.Current.Dispatcher.BeginInvoke(OnClick)
        End If
    End Sub
End Class
