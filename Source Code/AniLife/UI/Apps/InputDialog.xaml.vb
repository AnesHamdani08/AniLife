Imports System.ComponentModel

Public Class InputDialog
    Public Property Input
    Public Enum DialogType
        TextInput
        BooleanSwitch
        IntegerInput
        DoubleInput
    End Enum
    Public Property InputType As DialogType
    Private Property _MaxVal As Integer = Integer.MaxValue
    Public Property MaximumValue As Integer
        Get
            Return _MaxVal
        End Get
        Set(value As Integer)
            _MaxVal = value
            IntegerValue.Maximum = value
        End Set
    End Property
    Private Property _MinVal As Integer = Integer.MinValue
    Public Property MinimumValue As Integer
        Get
            Return _MinVal
        End Get
        Set(value As Integer)
            _MinVal = value
            IntegerValue.Minimum = value
        End Set
    End Property
    Public Sub New(msg As String, Optional type As DialogType = DialogType.TextInput)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Dim s As New Style
        s.Setters.Add(New Setter(VisibilityProperty, Visibility.Collapsed))
        MainTabControl.ItemContainerStyle = s
        ib_msg.Text = msg
        InputType = type
        Select Case type
            Case DialogType.TextInput
                MainTabControl.SelectedIndex = 0
            Case DialogType.BooleanSwitch
                Title = msg
                MainTabControl.SelectedIndex = 1
            Case DialogType.DoubleInput, DialogType.IntegerInput
                MainTabControl.SelectedIndex = 2
        End Select
        IntegerValue.Maximum = MaximumValue
        IntegerValue.Minimum = MinimumValue
    End Sub
    Public Sub New(WndO As Window, msg As String, Optional type As DialogType = DialogType.TextInput)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Owner = WndO
        Dim s As New Style
        s.Setters.Add(New Setter(VisibilityProperty, Visibility.Collapsed))
        MainTabControl.ItemContainerStyle = s
        ib_msg.Text = msg
        InputType = type
        Select Case type
            Case DialogType.TextInput
                MainTabControl.SelectedIndex = 0
            Case DialogType.BooleanSwitch
                Title = msg
                MainTabControl.SelectedIndex = 1
            Case DialogType.DoubleInput, DialogType.IntegerInput
                MainTabControl.SelectedIndex = 2
        End Select
        IntegerValue.Maximum = MaximumValue
        IntegerValue.Minimum = MinimumValue
    End Sub
    Private Sub ib_done_Click(sender As Object, e As RoutedEventArgs) Handles ib_done.Click
        If Not String.IsNullOrEmpty(ib_input.Text) AndAlso Not String.IsNullOrWhiteSpace(ib_input.Text) Then
            Input = ib_input.Text
            DialogResult = True
        End If
    End Sub

    Private Sub BooleanTrue_Click(sender As Object, e As RoutedEventArgs) Handles BooleanTrue.Click
        Input = True
        DialogResult = True
    End Sub

    Private Sub BooleanFalse_Click(sender As Object, e As RoutedEventArgs) Handles BooleanFalse.Click
        Input = False
        DialogResult = True
    End Sub

    Private Sub BooleanTrue_MouseEnter(sender As Object, e As MouseEventArgs) Handles BooleanTrue.MouseEnter
        BooleanTrue.BeginAnimation(FontSizeProperty, New Animation.DoubleAnimation(136, New Duration(TimeSpan.FromMilliseconds(100))))
        'BooleanFalse.BeginAnimation(FontSizeProperty, New Animation.DoubleAnimation(100, New Duration(TimeSpan.FromMilliseconds(100))))
    End Sub
    Private Sub BooleanTrue_MouseLeave(sender As Object, e As MouseEventArgs) Handles BooleanTrue.MouseLeave
        BooleanTrue.BeginAnimation(FontSizeProperty, New Animation.DoubleAnimation(100, New Duration(TimeSpan.FromMilliseconds(100))))
    End Sub

    Private Sub BooleanFalse_MouseEnter(sender As Object, e As MouseEventArgs) Handles BooleanFalse.MouseEnter
        'BooleanTrue.BeginAnimation(FontSizeProperty, New Animation.DoubleAnimation(100, New Duration(TimeSpan.FromMilliseconds(100))))
        BooleanFalse.BeginAnimation(FontSizeProperty, New Animation.DoubleAnimation(136, New Duration(TimeSpan.FromMilliseconds(100))))
    End Sub
    Private Sub BooleanFalse_MouseLeave(sender As Object, e As MouseEventArgs) Handles BooleanFalse.MouseLeave
        BooleanFalse.BeginAnimation(FontSizeProperty, New Animation.DoubleAnimation(100, New Duration(TimeSpan.FromMilliseconds(100))))
    End Sub

    Private Sub IntegerDone_Click(sender As Object, e As RoutedEventArgs) Handles IntegerDone.Click
        If InputType = DialogType.IntegerInput Then
            Input = CInt(IntegerValue.Value)
        ElseIf InputType = DialogType.DoubleInput Then
            Input = IntegerValue.Value
        End If
        DialogResult = True
    End Sub
End Class
