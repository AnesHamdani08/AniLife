Imports AUDX
Namespace AdvancedInputBox.Controls
    Public Class NumberField
        Implements AdvancedInputBox.IAdvancedInputBoxElement

        Public ReadOnly Property IsNull As Boolean Implements AdvancedInputBox.IAdvancedInputBoxElement.IsNull
            Get
                Return False
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
                Return New AdvancedInputBox.InputBoxElementResult(Input_TB.Value, IsRequired, AdvancedInputBox.InputBoxElementResult.ReturnType.Number, Tag)
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

        Public Sub New(Name As String, Min As Double, Max As Double, _IsRequired As Boolean)
            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.

            Name_TB.Text = Name
            Input_TB.Minimum = Min
            Input_TB.Maximum = Max
            IsRequired = _IsRequired
        End Sub
        Public Sub New(Name As String, Min As Double, Max As Double, Value As Double, _IsRequired As Boolean)
            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.

            Name_TB.Text = Name
            Input_TB.Minimum = Min
            Input_TB.Maximum = Max
            Input_TB.Value = Value
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
    End Class
End Namespace