Public Class LoadingLine
    Public Property Interval As Double = 500
    Dim _IsAnimating As Boolean = True
    Public Property IsLoading As Boolean
        Get
            Return _IsAnimating
        End Get
        Set(value As Boolean)
            _IsAnimating = value
            If value Then
                LoadingDancer.BeginAnimation(MarginProperty, New Animation.ThicknessAnimation(New Thickness(-32, 0, 0, 0), New Thickness(ActualWidth - 32, 0, 0, 0), New Duration(TimeSpan.FromMilliseconds(Interval))) With {.AutoReverse = True, .RepeatBehavior = Animation.RepeatBehavior.Forever})
            Else
                LoadingDancer.BeginAnimation(MarginProperty, New Animation.ThicknessAnimation(New Thickness(0, 0, 0, 0), New Duration(TimeSpan.FromMilliseconds(50))))
            End If
        End Set
    End Property
    Private Sub LoadingLine_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        _IsAnimating = True
        LoadingDancer.BeginAnimation(MarginProperty, New Animation.ThicknessAnimation(New Thickness(-32, 0, 0, 0), New Thickness(ActualWidth - 32, 0, 0, 0), New Duration(TimeSpan.FromMilliseconds(Interval))) With {.AutoReverse = True, .RepeatBehavior = Animation.RepeatBehavior.Forever})
    End Sub

    Private Sub LoadingLine_SizeChanged(sender As Object, e As SizeChangedEventArgs) Handles Me.SizeChanged
        If IsLoading Then
            LoadingDancer.BeginAnimation(MarginProperty, New Animation.ThicknessAnimation(New Thickness(-32, 0, 0, 0), New Thickness(ActualWidth - 32, 0, 0, 0), New Duration(TimeSpan.FromMilliseconds(Interval))) With {.AutoReverse = True, .RepeatBehavior = Animation.RepeatBehavior.Forever})
        End If
    End Sub
End Class
