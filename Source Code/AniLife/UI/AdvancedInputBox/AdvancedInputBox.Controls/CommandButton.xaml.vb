Imports AUDX
Namespace AdvancedInputBox.Controls
    Public Class CommandButton
        Implements AdvancedInputBox.IAdvancedInputBoxElement

        Event Click(sender As Object)

        ''' <summary>
        ''' Gets or Sets wether or not this command button is clicked in the group
        ''' </summary>
        ''' <returns></returns>
        Public Property IsSelected As Boolean
        ''' <summary>
        ''' Gets or Sets the color to show when the mouse is over this element
        ''' </summary>
        ''' <returns></returns>
        Public Property HoverColor As Color = Colors.Green

        Public ReadOnly Property IsNull As Boolean Implements AdvancedInputBox.IAdvancedInputBoxElement.IsNull
            Get
                Return False
            End Get
        End Property
        Private _IsRequired As Boolean = False
        Public Property IsRequired As Boolean Implements AdvancedInputBox.IAdvancedInputBoxElement.IsRequired
            Get
                Return _IsRequired
            End Get
            Set(value As Boolean)
                _IsRequired = value
            End Set
        End Property

        Public ReadOnly Property Result As AdvancedInputBox.InputBoxElementResult Implements AdvancedInputBox.IAdvancedInputBoxElement.Result
            Get
                Return New AdvancedInputBox.InputBoxElementResult(Nothing, IsRequired, AdvancedInputBox.InputBoxElementResult.ReturnType.Object, Tag)
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

        Public Sub New(Name As String, Content As String, _IsRequired As Boolean)
            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.

            Name_TB.Text = Name
            Content_TB.Text = Content
            IsRequired = _IsRequired
        End Sub

        Public Sub New(Name As String, Content As String, Icon As Geometry, _IsRequired As Boolean)
            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.

            Name_TB.Text = Name
            Content_TB.Text = Content
            If Icon IsNot Nothing Then HandyControl.Controls.IconElement.SetGeometry(Icon_BTN, Icon)
            IsRequired = _IsRequired
        End Sub

        Public Sub New(Name As String, Content As String, Description As String, _IsRequired As Boolean)
            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.

            Name_TB.Text = Name
            Content_TB.Text = Content
            Description_TB.Text = Description
            IsRequired = _IsRequired
        End Sub
        Public Sub New(Name As String, Content As String, Description As String, Icon As Geometry, _IsRequired As Boolean)
            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.

            Name_TB.Text = Name
            Content_TB.Text = Content
            Description_TB.Text = Description
            If Icon IsNot Nothing Then HandyControl.Controls.IconElement.SetGeometry(Icon_BTN, Icon)
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

        Private Sub CommandButton_MouseEnter(sender As Object, e As MouseEventArgs) Handles Me.MouseEnter
            Icon_BTN.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, New Animation.ColorAnimation(HoverColor, New Duration(TimeSpan.FromMilliseconds(150))) With {.AccelerationRatio = 0.9})
        End Sub

        Private Sub CommandButton_MouseLeave(sender As Object, e As MouseEventArgs) Handles Me.MouseLeave
            Icon_BTN.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, New Animation.ColorAnimation(Colors.Black, New Duration(TimeSpan.FromMilliseconds(150))))
        End Sub

        Private Sub Icon_BTN_Click(sender As Object, e As RoutedEventArgs) Handles Icon_BTN.Click
            RaiseEvent Click(Me)
        End Sub

        Private Sub CommandButton_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles Me.MouseLeftButtonUp
            RaiseEvent Click(Me)
        End Sub
    End Class
End Namespace
