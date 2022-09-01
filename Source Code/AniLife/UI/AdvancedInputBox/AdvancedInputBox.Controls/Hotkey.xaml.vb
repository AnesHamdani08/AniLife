Imports AUDX
Namespace AdvancedInputBox.Controls
    Public Class HotkeySwitch
        Implements AdvancedInputBox.IAdvancedInputBoxElement

        Public ReadOnly Property IsNull As Boolean Implements AdvancedInputBox.IAdvancedInputBoxElement.IsNull
            Get
                Try
                    Return If(Key_TB.Tag = 0, True, False)
                Catch ex As Exception
                    Return False
                End Try
            End Get
        End Property

        Public Property IsRequired As Boolean Implements AdvancedInputBox.IAdvancedInputBoxElement.IsRequired
            Get
                Return If(Required_TB.Visibility = Visibility.Visible, True, False)
            End Get
            Set(value As Boolean)
                If value Then
                    Required_TB.Visibility = Visibility.Visible
                Else
                    Required_TB.Visibility = Visibility.Collapsed
                End If
            End Set
        End Property

        Public ReadOnly Property Result As AdvancedInputBox.InputBoxElementResult Implements AdvancedInputBox.IAdvancedInputBoxElement.Result
            Get
                Return New AdvancedInputBox.InputBoxElementResult(New KeyModifierPair(KeyInterop.VirtualKeyFromKey(Key_TB.Tag), Modifier_TB.Tag), IsRequired, AdvancedInputBox.InputBoxElementResult.ReturnType.Object, Tag)
            End Get
        End Property

        Private ReadOnly Property IAdvancedInputBoxElement_Name As String Implements AdvancedInputBox.IAdvancedInputBoxElement.Name
            Get
                Return Name_TB.Text
            End Get
        End Property

        Private Property IAdvancedInputBoxElement_Tag As Object Implements AdvancedInputBox.IAdvancedInputBoxElement.Tag
            Get
                Return Tag
            End Get
            Set(value As Object)
                Tag = value
            End Set
        End Property

        Public Sub New(Name As String, _IsRequired As Boolean)
            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.

            Name_TB.Text = Name
            IsRequired = _IsRequired
        End Sub
        Public Sub New(Name As String, Key As Forms.Keys, Modifier As Integer, _IsRequired As Boolean)
            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.

            Name_TB.Text = Name
            Key_TB.Tag = Key
            Key_TB.Content = Key.ToString
            Modifier_TB.Tag = Modifier
            Modifier_TB.Content = MODToSTR(Modifier)
            IsRequired = _IsRequired
        End Sub
        Public Sub Notify() Implements AdvancedInputBox.IAdvancedInputBoxElement.Notify
            Background = New SolidColorBrush(Color.FromArgb(0, 255, 0, 0))
            Dim NAnim As New Animation.ColorAnimation(Color.FromArgb(255, 255, 0, 0), New Duration(TimeSpan.FromMilliseconds(250))) With {.RepeatBehavior = New Animation.RepeatBehavior(2), .AccelerationRatio = 0.8, .AutoReverse = True, .EasingFunction = New Animation.QuadraticEase With {.EasingMode = Animation.EasingMode.EaseInOut}}
            AddHandler NAnim.Completed, Sub()
                                            ClearValue(BackgroundProperty)
                                        End Sub
            Background.BeginAnimation(SolidColorBrush.ColorProperty, NAnim)
        End Sub

        Private Sub Record_BTN_Checked(sender As Object, e As RoutedEventArgs) Handles Record_BTN.Checked
            Focus()
        End Sub

        Private Sub HotkeySwitch_KeyDown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown
            If Record_BTN.IsChecked Then
                Key_TB.Content = e.Key.ToString
                Key_TB.Tag = e.Key
                If My.Computer.Keyboard.CtrlKeyDown Then
                    Modifier_TB.Content = "CTRL"
                    Modifier_TB.Tag = Constants.CTRL
                ElseIf My.Computer.Keyboard.AltKeyDown Then
                    Modifier_TB.Content = "ALT"
                    Modifier_TB.Tag = Constants.ALT
                ElseIf My.Computer.Keyboard.ShiftKeyDown Then
                    Modifier_TB.Content = "SHIFT"
                    Modifier_TB.Tag = Constants.SHIFT
                Else
                    Modifier_TB.Content = "None"
                    Modifier_TB.Tag = Constants.NOMOD
                End If
            End If
        End Sub

        Private Sub HotkeySwitch_GotFocus(sender As Object, e As RoutedEventArgs) Handles Me.GotFocus
            Record_BTN.IsChecked = True
        End Sub

        Private Sub HotkeySwitch_LostFocus(sender As Object, e As RoutedEventArgs) Handles Me.LostFocus
            Record_BTN.IsChecked = False
        End Sub
        Public Structure KeyModifierPair
            Property Key As Forms.Keys
            Property Modifier As Integer
            Public Sub New(Key As Forms.Keys, Modifier As Integer)
                Me.Key = Key
                Me.Modifier = Modifier
            End Sub
        End Structure
        Public Shared Function MODToSTR(i As Integer) As String
            Select Case i
                Case Constants.NOMOD 'NO MOD
                    Return "NO MOD"
                Case Constants.SHIFT 'SHIFT
                    Return "SHIFT"
                Case Constants.CTRL 'CTRL
                    Return "CTRL"
                Case Constants.CTRL 'ALT
                    Return "ALT"
                Case Constants.WIN 'WIN
                    Return "WIN"
                Case Else
                    Return Nothing
            End Select
        End Function
    End Class
    Public Module Constants

        'modifiers
        Public Const NOMOD = &H0

        Public Const ALT = &H1
        Public Const CTRL = &H2
        Public Const SHIFT = &H4
        Public Const WIN = &H8

        'windows message id for hotkey
        Public Const WM_HOTKEY_MSG_ID = &H312

    End Module
End Namespace
