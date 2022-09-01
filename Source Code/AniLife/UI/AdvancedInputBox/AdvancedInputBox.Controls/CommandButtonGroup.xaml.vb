Imports AUDX
Namespace AdvancedInputBox.Controls
    Public Class CommandButtonGroup
        Implements AdvancedInputBox.IAdvancedInputBoxElement
        Event OnCommandSelected()

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
                Dim SelectedButton As CommandButton = Nothing
                For Each button As CommandButton In CommandButtons_Container.Children
                    If button.IsSelected = True Then
                        SelectedButton = button
                        Exit For
                    End If
                Next
                Return New AdvancedInputBox.InputBoxElementResult(SelectedButton, IsRequired, AdvancedInputBox.InputBoxElementResult.ReturnType.Object, Tag)
            End Get
        End Property
        Private _Name As String
        Private ReadOnly Property IAdvancedInputBoxElement_Name As String Implements AdvancedInputBox.IAdvancedInputBoxElement.Name
            Get
                Return _Name
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

            _Name = Name
            IsRequired = _IsRequired
        End Sub

        Public Sub New(Name As String, Items As IEnumerable(Of CommandButton), _IsRequired As Boolean)
            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.

            _Name = Name
            For Each button In Items
                If button IsNot Nothing Then
                    AddHandler button.Click, Sub(sender As Object)
                                                 For Each butt As CommandButton In CommandButtons_Container.Children
                                                     If butt Is sender Then
                                                         butt.IsSelected = True
                                                     Else
                                                         butt.IsSelected = False
                                                     End If
                                                 Next
                                                 RaiseEvent OnCommandSelected()
                                             End Sub
                    CommandButtons_Container.Children.Add(button)
                End If
            Next
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
