Public Class VolumeControl
    ''' <summary>
    ''' Get or Set the value in range of 0 to 100
    ''' </summary>
    ''' <returns></returns>
    Public Property Value As Double
        Get
            Return GetValue(ValueProperty)
        End Get
        Set(value As Double)
            SetValue(ValueProperty, value)
        End Set
    End Property
    Public Shared ReadOnly ValueProperty As DependencyProperty = DependencyProperty.Register("Value", GetType(Double), GetType(VolumeControl), New UIPropertyMetadata(AddressOf ValueChanged))
    Public Event OnValueChanged(value As Double)

    Private _IsIndeterminateRequestStop As Boolean = False
    Public Property IsIndeterminate As Boolean
        Get
            Return GetValue(IsIndeterminateProperty)
        End Get
        Set(value As Boolean)
            SetValue(IsIndeterminateProperty, value)
        End Set
    End Property
    Public Shared ReadOnly IsIndeterminateProperty As DependencyProperty = DependencyProperty.Register("IsIndeterminate", GetType(Boolean), GetType(VolumeControl), New UIPropertyMetadata(AddressOf IsIndeterminateChanged))
    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub
    Private Shared Sub IsIndeterminateChanged(ByVal d As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)
        If TryCast(d, VolumeControl) IsNot Nothing Then
            CType(d, VolumeControl).UpdateIsIndeterminate(e.NewValue)
        End If
    End Sub
    Private Sub UpdateIsIndeterminate(val As Boolean)
        If val Then
            _IsIndeterminateRequestStop = False
            AnimateIsIndeterminate()
        Else
            _IsIndeterminateRequestStop = True
        End If
    End Sub
    Private Async Sub AnimateIsIndeterminate()
        Dim i = 1
        Do While Not _IsIndeterminateRequestStop
            Select Case i
                Case 1
                    Await UpdateValue(20, False)
                    i = 2
                Case 2
                    Await UpdateValue(40, False)
                    i = 3
                Case 3
                    Await UpdateValue(60, False)
                    i = 4
                Case 4
                    Await UpdateValue(80, False)
                    i = 5
                Case 5
                    Await UpdateValue(100, False)
                    i = 6
                Case 6
                    Await UpdateValue(80, False)
                    i = 7
                Case 7
                    Await UpdateValue(60, False)
                    i = 8
                Case 8
                    Await UpdateValue(40, False)
                    i = 9
                Case 9
                    Await UpdateValue(20, False)
                    i = 1
            End Select
        Loop
        Await UpdateValue(Value, False)
    End Sub
    Private Shared Async Sub ValueChanged(ByVal d As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)
        If TryCast(d, VolumeControl) IsNot Nothing Then
            Await CType(d, VolumeControl).UpdateValue(e.NewValue)
        End If
    End Sub
    Private Async Function UpdateValue(val As Integer, Optional RaiseEvents As Boolean = True) As Task
        Select Case Utils.PercentageToFiveMax(val)
            Case 0, 1
                Await HideVals(False)
                Await ShowVals(True, False, False, False, False)
            Case 2
                Await HideVals(False, False)
                Await ShowVals(True, True, False, False, False)
            Case 3
                Await HideVals(False, False, False)
                Await ShowVals(True, True, True, False, False)
            Case 4
                Await HideVals(False, False, False, False)
                Await ShowVals(True, True, True, True, False)
            Case 5
                Await ShowVals()
        End Select
        If RaiseEvents Then RaiseEvent OnValueChanged(val)
    End Function
    Private Async Function HideVals(Optional a As Boolean = True, Optional b As Boolean = True, Optional c As Boolean = True, Optional d As Boolean = True, Optional e As Boolean = True) As Task
        Dim OAnim As New Animation.DoubleAnimation(0, New Duration(TimeSpan.FromMilliseconds(100)))
        If a Then ValF_1.BeginAnimation(OpacityProperty, OAnim)
        If b Then ValF_2.BeginAnimation(OpacityProperty, OAnim)
        If c Then ValF_3.BeginAnimation(OpacityProperty, OAnim)
        If d Then ValF_4.BeginAnimation(OpacityProperty, OAnim)
        If e Then ValF_5.BeginAnimation(OpacityProperty, OAnim)
        Await Task.Delay(120)
    End Function
    Private Async Function ShowVals(Optional a As Boolean = True, Optional b As Boolean = True, Optional c As Boolean = True, Optional d As Boolean = True, Optional e As Boolean = True) As Task
        Dim OAnim As New Animation.DoubleAnimation(1, New Duration(TimeSpan.FromMilliseconds(100)))
        If a Then ValF_1.BeginAnimation(OpacityProperty, OAnim)
        If b Then ValF_2.BeginAnimation(OpacityProperty, OAnim)
        If c Then ValF_3.BeginAnimation(OpacityProperty, OAnim)
        If d Then ValF_4.BeginAnimation(OpacityProperty, OAnim)
        If e Then ValF_5.BeginAnimation(OpacityProperty, OAnim)
        Await Task.Delay(120)
    End Function
    'Ref
    'Margin=160,120,80,40,0
    Private Sub Val_1_MouseEnter(sender As Object, e As MouseEventArgs) Handles Val_1.MouseEnter, ValF_1.MouseEnter
        Val_1.BeginAnimation(MarginProperty, New Animation.ThicknessAnimation(New Thickness(0, 150, 0, 0), New Duration(TimeSpan.FromMilliseconds(100))))
        ValF_1.BeginAnimation(MarginProperty, New Animation.ThicknessAnimation(New Thickness(0, 150, 0, 0), New Duration(TimeSpan.FromMilliseconds(100))))
    End Sub

    Private Sub Val_1_MouseLeave(sender As Object, e As MouseEventArgs) Handles Val_1.MouseLeave, ValF_1.MouseLeave
        Val_1.BeginAnimation(MarginProperty, New Animation.ThicknessAnimation(New Thickness(0, 170, 0, 0), New Duration(TimeSpan.FromMilliseconds(100))))
        ValF_1.BeginAnimation(MarginProperty, New Animation.ThicknessAnimation(New Thickness(0, 170, 0, 0), New Duration(TimeSpan.FromMilliseconds(100))))
    End Sub
    Private Sub Val_2_MouseEnter(sender As Object, e As MouseEventArgs) Handles Val_2.MouseEnter, ValF_2.MouseEnter
        Val_2.BeginAnimation(MarginProperty, New Animation.ThicknessAnimation(New Thickness(60, 110, 0, 0), New Duration(TimeSpan.FromMilliseconds(100))))
        ValF_2.BeginAnimation(MarginProperty, New Animation.ThicknessAnimation(New Thickness(60, 110, 0, 0), New Duration(TimeSpan.FromMilliseconds(100))))
    End Sub

    Private Sub Val_2_MouseLeave(sender As Object, e As MouseEventArgs) Handles Val_2.MouseLeave, ValF_2.MouseLeave
        Val_2.BeginAnimation(MarginProperty, New Animation.ThicknessAnimation(New Thickness(60, 130, 0, 0), New Duration(TimeSpan.FromMilliseconds(100))))
        ValF_2.BeginAnimation(MarginProperty, New Animation.ThicknessAnimation(New Thickness(60, 130, 0, 0), New Duration(TimeSpan.FromMilliseconds(100))))
    End Sub
    Private Sub Val_3_MouseEnter(sender As Object, e As MouseEventArgs) Handles Val_3.MouseEnter, ValF_3.MouseEnter
        Val_3.BeginAnimation(MarginProperty, New Animation.ThicknessAnimation(New Thickness(120, 70, 0, 0), New Duration(TimeSpan.FromMilliseconds(100))))
        ValF_3.BeginAnimation(MarginProperty, New Animation.ThicknessAnimation(New Thickness(120, 70, 0, 0), New Duration(TimeSpan.FromMilliseconds(100))))
    End Sub

    Private Sub Val_3_MouseLeave(sender As Object, e As MouseEventArgs) Handles Val_3.MouseLeave, ValF_3.MouseLeave
        Val_3.BeginAnimation(MarginProperty, New Animation.ThicknessAnimation(New Thickness(120, 90, 0, 0), New Duration(TimeSpan.FromMilliseconds(100))))
        ValF_3.BeginAnimation(MarginProperty, New Animation.ThicknessAnimation(New Thickness(120, 90, 0, 0), New Duration(TimeSpan.FromMilliseconds(100))))
    End Sub
    Private Sub Val_4_MouseEnter(sender As Object, e As MouseEventArgs) Handles Val_4.MouseEnter, ValF_4.MouseEnter
        Val_4.BeginAnimation(MarginProperty, New Animation.ThicknessAnimation(New Thickness(180, 30, 0, 0), New Duration(TimeSpan.FromMilliseconds(100))))
        ValF_4.BeginAnimation(MarginProperty, New Animation.ThicknessAnimation(New Thickness(180, 30, 0, 0), New Duration(TimeSpan.FromMilliseconds(100))))
    End Sub

    Private Sub Val_4_MouseLeave(sender As Object, e As MouseEventArgs) Handles Val_4.MouseLeave, ValF_4.MouseLeave
        Val_4.BeginAnimation(MarginProperty, New Animation.ThicknessAnimation(New Thickness(180, 50, 0, 0), New Duration(TimeSpan.FromMilliseconds(100))))
        ValF_4.BeginAnimation(MarginProperty, New Animation.ThicknessAnimation(New Thickness(180, 50, 0, 0), New Duration(TimeSpan.FromMilliseconds(100))))
    End Sub
    Private Sub Val_5_MouseEnter(sender As Object, e As MouseEventArgs) Handles Val_5.MouseEnter, ValF_5.MouseEnter
        Val_5.BeginAnimation(MarginProperty, New Animation.ThicknessAnimation(New Thickness(240, 0, 0, 0), New Duration(TimeSpan.FromMilliseconds(100))))
        ValF_5.BeginAnimation(MarginProperty, New Animation.ThicknessAnimation(New Thickness(240, 0, 0, 0), New Duration(TimeSpan.FromMilliseconds(100))))
    End Sub

    Private Sub Val_5_MouseLeave(sender As Object, e As MouseEventArgs) Handles Val_5.MouseLeave, ValF_5.MouseLeave
        Val_5.BeginAnimation(MarginProperty, New Animation.ThicknessAnimation(New Thickness(240, 10, 0, 0), New Duration(TimeSpan.FromMilliseconds(100))))
        ValF_5.BeginAnimation(MarginProperty, New Animation.ThicknessAnimation(New Thickness(240, 10, 0, 0), New Duration(TimeSpan.FromMilliseconds(100))))
    End Sub

    Private Sub Val_1_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles Val_1.MouseLeftButtonUp, ValF_1.MouseLeftButtonUp
        SetValue(ValueProperty, 20.0)
    End Sub

    Private Sub Val_2_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles Val_2.MouseLeftButtonUp, ValF_2.MouseLeftButtonUp
        SetValue(ValueProperty, 40.0)
    End Sub

    Private Sub Val_3_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles Val_3.MouseLeftButtonUp, ValF_3.MouseLeftButtonUp
        SetValue(ValueProperty, 60.0)
    End Sub

    Private Sub Val_4_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles Val_4.MouseLeftButtonUp, ValF_4.MouseLeftButtonUp
        SetValue(ValueProperty, 80.0)
    End Sub

    Private Sub Val_5_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles Val_5.MouseLeftButtonUp, ValF_5.MouseLeftButtonUp
        SetValue(ValueProperty, 100.0)
    End Sub
End Class
