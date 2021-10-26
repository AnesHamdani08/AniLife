Public Class BarsItem
    Private _IsIndeterminate As Boolean = False
    Public Property IsIndeterminate As Boolean
        Get
            Return _IsIndeterminate
        End Get
        Set(value As Boolean)
            _IsIndeterminate = value
            If value Then
                IsIndeterminateAnimation()
            End If
        End Set
    End Property
    Public Property PingURL As String = "http://api.anidb.net:9001/httpapi"
    Private Async Sub IsIndeterminateAnimation()
        Do While IsIndeterminate
            HideAllBars()
            Bar_1.Opacity = 1
            Await Task.Delay(100)
            HideAllBars()
            Bar_2.Opacity = 1
            Await Task.Delay(100)
            HideAllBars()
            Bar_3.Opacity = 1
            Await Task.Delay(100)
            HideAllBars()
            Bar_4.Opacity = 1
            Await Task.Delay(100)
            HideAllBars()
            Bar_5.Opacity = 1
            Await Task.Delay(100)
        Loop
        HideAllBars()
    End Sub
    Private Sub HideAllBars()
        Bar_1.Opacity = 0.5
        Bar_2.Opacity = 0.5
        Bar_3.Opacity = 0.5
        Bar_4.Opacity = 0.5
        Bar_5.Opacity = 0.5
    End Sub
    Private Sub ShowAllBars()
        Bar_1.Opacity = 1
        Bar_2.Opacity = 1
        Bar_3.Opacity = 1
        Bar_4.Opacity = 1
        Bar_5.Opacity = 1
    End Sub
    Private Sub FillBars(brsh As Brush, i As Integer)
        Select Case i
            Case 0
                Bar_1.Fill = Application.Current.Resources("CONTENT")
                Bar_2.Fill = Application.Current.Resources("CONTENT")
                Bar_3.Fill = Application.Current.Resources("CONTENT")
                Bar_4.Fill = Application.Current.Resources("CONTENT")
                Bar_5.Fill = Application.Current.Resources("CONTENT")
            Case 1
                Bar_1.Fill = brsh
                Bar_2.Fill = Application.Current.Resources("CONTENT")
                Bar_3.Fill = Application.Current.Resources("CONTENT")
                Bar_4.Fill = Application.Current.Resources("CONTENT")
                Bar_5.Fill = Application.Current.Resources("CONTENT")
            Case 2
                Bar_1.Fill = brsh
                Bar_2.Fill = brsh
                Bar_3.Fill = Application.Current.Resources("CONTENT")
                Bar_4.Fill = Application.Current.Resources("CONTENT")
                Bar_5.Fill = Application.Current.Resources("CONTENT")
            Case 3
                Bar_1.Fill = brsh
                Bar_2.Fill = brsh
                Bar_3.Fill = brsh
                Bar_4.Fill = Application.Current.Resources("CONTENT")
                Bar_5.Fill = Application.Current.Resources("CONTENT")
            Case 4
                Bar_1.Fill = brsh
                Bar_2.Fill = brsh
                Bar_3.Fill = brsh
                Bar_4.Fill = brsh
                Bar_5.Fill = Application.Current.Resources("CONTENT")
            Case 5
                Bar_1.Fill = brsh
                Bar_2.Fill = brsh
                Bar_3.Fill = brsh
                Bar_4.Fill = brsh
                Bar_5.Fill = brsh
        End Select
    End Sub
    Private Async Sub BarsItem_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles Me.MouseLeftButtonUp
        FillBars(Nothing, 0)
        Ping_TB.Text = Nothing
        IsIndeterminate = True
        Try
            ShowAllBars()
            Dim SW As New Stopwatch
            Using HClient As New Net.Http.HttpClient(New Net.Http.HttpClientHandler With {.AutomaticDecompression = Net.DecompressionMethods.GZip Or Net.DecompressionMethods.Deflate})
                SW.Start()
                Await HClient.GetAsync(PingURL)
                SW.Stop()
            End Using
            IsIndeterminate = False
            Ping_TB.Text = SW.ElapsedMilliseconds & " Ms"
            FillBars(Application.Current.Resources("ACCENT"), 5 - Utils.PercentageToFiveMax(Utils.ValToPercentage(SW.ElapsedMilliseconds, 0, 2000)))
        Catch ex As Exception
            IsIndeterminate = False
            FillBars(Application.Current.Resources("ACCENT"), 0)
        End Try
    End Sub
End Class
