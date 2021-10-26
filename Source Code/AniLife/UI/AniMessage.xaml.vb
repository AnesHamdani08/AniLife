Public Class AniMessage
    Public Result As Button
    Private _AnimateOnShow As Boolean = False
    Public Property AnimateOnShow As Boolean
        Get
            Return _AnimateOnShow
        End Get
        Set(value As Boolean)
            _AnimateOnShow = value
            If value Then
                Opacity = 0
            Else
                Opacity = 1
            End If
        End Set
    End Property
    Public Property AnimateOnHide As Boolean = False
    Public Sub New(Owner As Window, Title As String, Message As String, Image As ImageSource, Buttons As IEnumerable(Of Button))

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.        
        Owner = Owner
        WindowStartupLocation = WindowStartupLocation.CenterOwner
        Me.Title = Title
        Title_TB.Text = Title
        Message_TB.Text = Message
        Image_MG.Source = Image
        Icon = Image
        For Each button In Buttons
            button.Margin = New Thickness(10, 0, 0, 0)
            button.Background = Application.Current.Resources("BG")
            button.BorderThickness = New Thickness(1, 1, 1, 1)
            button.BorderBrush = Application.Current.Resources("ACCENT")
            AddHandler button.Click, Sub()
                                         Result = button
                                         If AnimateOnHide Then
                                             Dim HAnim As New Animation.DoubleAnimation(0, New Duration(TimeSpan.FromMilliseconds(250))) With {.AccelerationRatio = 0.9}
                                             AddHandler HAnim.Completed, Sub()
                                                                             DialogResult = True
                                                                         End Sub
                                             BeginAnimation(OpacityProperty, HAnim)
                                         Else
                                             DialogResult = True
                                         End If
                                     End Sub
            Buttons_SP.Children.Add(button)
        Next
    End Sub
    Public Sub New(Owner As Window, Title As String, Message As String, Image As AniImage, Buttons As IEnumerable(Of Button))

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.        
        Owner = Owner
        WindowStartupLocation = WindowStartupLocation.CenterOwner
        Me.Title = Title
        Title_TB.Text = Title
        Message_TB.Text = Message
        Image_MG.Source = Application.Current.Resources(Image.ToString)
        Icon = Image_MG.Source
        For Each button In Buttons
            button.Margin = New Thickness(10, 0, 0, 0)
            button.Background = Application.Current.Resources("BG")
            button.BorderThickness = New Thickness(1, 1, 1, 1)
            button.BorderBrush = Application.Current.Resources("ACCENT")
            AddHandler button.Click, Sub()
                                         Result = button
                                         If AnimateOnHide Then
                                             Dim HAnim As New Animation.DoubleAnimation(0, New Duration(TimeSpan.FromMilliseconds(250))) With {.AccelerationRatio = 0.9}
                                             AddHandler HAnim.Completed, Sub()
                                                                             DialogResult = True
                                                                         End Sub
                                             BeginAnimation(OpacityProperty, HAnim)
                                         Else
                                             DialogResult = True
                                         End If
                                     End Sub
            Buttons_SP.Children.Add(button)
        Next
    End Sub
    Public Sub New(Title As String, Message As String, Image As ImageSource, Buttons As IEnumerable(Of Button))

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.                
        Me.Title = Title
        Title_TB.Text = Title
        Message_TB.Text = Message
        Image_MG.Source = Image
        Icon = Image
        If Buttons IsNot Nothing Then
            For Each button In Buttons
                button.Margin = New Thickness(10, 0, 0, 0)
                button.Background = Application.Current.Resources("BG")
                button.BorderThickness = New Thickness(1, 1, 1, 1)
                button.BorderBrush = Application.Current.Resources("ACCENT")
                AddHandler button.Click, Sub()
                                             Result = button
                                             If AnimateOnHide Then
                                                 Dim HAnim As New Animation.DoubleAnimation(0, New Duration(TimeSpan.FromMilliseconds(250))) With {.AccelerationRatio = 0.9}
                                                 AddHandler HAnim.Completed, Sub()
                                                                                 DialogResult = True
                                                                             End Sub
                                                 BeginAnimation(OpacityProperty, HAnim)
                                             Else
                                                 DialogResult = True
                                             End If
                                         End Sub
                Buttons_SP.Children.Add(button)
            Next
        End If
    End Sub
    Public Sub New(Title As String, Message As String, Image As AniImage, Buttons As IEnumerable(Of Button))

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.                
        Me.Title = Title
        Title_TB.Text = Title
        Message_TB.Text = Message
        Image_MG.Source = Application.Current.Resources(Image.ToString)
        Icon = Image_MG.Source
        If Buttons IsNot Nothing Then
            For Each button In Buttons
                button.Margin = New Thickness(10, 0, 0, 0)
                button.Background = Application.Current.Resources("BG")
                button.BorderThickness = New Thickness(1, 1, 1, 1)
                button.BorderBrush = Application.Current.Resources("ACCENT")
                AddHandler button.Click, Sub()
                                             Result = button
                                             If AnimateOnHide Then
                                                 Dim HAnim As New Animation.DoubleAnimation(0, New Duration(TimeSpan.FromMilliseconds(250))) With {.AccelerationRatio = 0.9}
                                                 AddHandler HAnim.Completed, Sub()
                                                                                 DialogResult = True
                                                                             End Sub
                                                 BeginAnimation(OpacityProperty, HAnim)
                                             Else
                                                 DialogResult = True
                                             End If
                                         End Sub
                Buttons_SP.Children.Add(button)
            Next
        End If
    End Sub
    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub
    Public Shared Function ShowMessage(Title As String, Message As String, Image As ImageSource, Buttons As IEnumerable(Of Button), Optional AnimateOnShow As Boolean = False, Optional AnimateOnHide As Boolean = False) As Button
        Dim Msg As New AniMessage(Title, Message, Image, Buttons) With {.AnimateOnShow = AnimateOnShow, .AnimateOnHide = AnimateOnHide}
        Msg.ShowDialog()
        Return Msg.Result
    End Function
    Public Shared Function ShowMessage(Title As String, Message As String, Image As AniImage, Buttons As IEnumerable(Of Button), Optional AnimateOnShow As Boolean = False, Optional AnimateOnHide As Boolean = False) As Button
        Dim Msg As New AniMessage(Title, Message, Image, Buttons) With {.AnimateOnShow = AnimateOnShow, .AnimateOnHide = AnimateOnHide}
        Msg.ShowDialog()
        Return Msg.Result
    End Function
    Public Shared Function ShowMessage(Owner As Window, Title As String, Message As String, Image As ImageSource, Buttons As IEnumerable(Of Button), Optional AnimateOnShow As Boolean = False, Optional AnimateOnHide As Boolean = False) As Button
        Dim Msg As New AniMessage(Owner, Title, Message, Image, Buttons) With {.AnimateOnShow = AnimateOnShow, .AnimateOnHide = AnimateOnHide}
        Msg.ShowDialog()
        Return Msg.Result
    End Function
    Public Shared Function ShowMessage(Owner As Window, Title As String, Message As String, Image As AniImage, Buttons As IEnumerable(Of Button), Optional AnimateOnShow As Boolean = False, Optional AnimateOnHide As Boolean = False) As Button
        Dim Msg As New AniMessage(Owner, Title, Message, Image, Buttons) With {.AnimateOnShow = AnimateOnShow, .AnimateOnHide = AnimateOnHide}
        Msg.ShowDialog()
        Return Msg.Result
    End Function

    Private Sub AniMessage_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        If AnimateOnShow Then
            BeginAnimation(OpacityProperty, New Animation.DoubleAnimation(1, New Duration(TimeSpan.FromMilliseconds(250))) With {.AccelerationRatio = 0.9})
        End If
    End Sub

    Public Enum AniImage
        WARNING
    End Enum
End Class
