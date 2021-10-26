Public Class EpisodeElement
    Public Const ImageScalingFactor As Double = 6
    Public Shadows Property Tag As API.AniDB.AniDBClient.EpisodeElement
    Public Property AID As Integer
    Private Property LoadCover As Boolean = False
    Public Sub New(ee As API.AniDB.AniDBClient.EpisodeElement, _AID As Integer, Optional Cover As ImageSource = Nothing, Optional PreloadCover As Boolean = True)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Tag = ee
        AID = _AID
        If ee IsNot Nothing Then
            Episode_Title.Text = ee.Title
            Episode_Num.Text = ee.Number
            Episode_Length.Text = Utils.TimeToString(ee.Length * 60)
            Episode_Rating.Text = ee.Rating.Value
            Episode_Rating.ToolTip = ee.Rating.Count & Space(1) & AniResolver.USER
            Episode_Summary.Text = ee.Summary
            For Each res In ee.Resources
                For Each url In res.URL.URLs
                    Dim ResIG As New Image With {.Height = 32, .Stretch = Stretch.Uniform, .Source = Utils.ResourceTypeToInternalImage(res.Type), .ToolTip = res.Type.ToString, .Margin = New Thickness(0, 0, 10, 0)}
                    AddHandler ResIG.MouseLeftButtonDown, Sub(sender As Object, e As MouseButtonEventArgs)
                                                              If e.ClickCount >= 2 Then
                                                                  Process.Start(url)
                                                              End If
                                                          End Sub
                    Episode_Resources.Children.Add(ResIG)
                Next
            Next
        End If
        If Cover IsNot Nothing Then
            Episode_Cover.Source = Cover
        Else
            LoadCover = PreloadCover
        End If
    End Sub
    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub

    Private Sub EpisodeElement_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        If LoadCover Then
#Disable Warning
            Dispatcher.BeginInvoke(Async Function()
                                       Dim Mwnd = TryCast(Application.Current.MainWindow, MainWindow)
                                       If Mwnd IsNot Nothing Then
                                           If Mwnd.MainCache.CheckIfAnimeExists(AID) Then
                                               Dim Picture = Mwnd.MainCache.GetPicture(AID)
                                               If Picture IsNot Nothing Then
                                                   Episode_Cover.Source = Utils.ImageSourceFromBitmap(Picture, ImageScalingFactor)
                                               End If
                                           Else
                                               Dim Picture = Await API.AniDB.AniDBClient.SharedFunctions.GetAniDBImageAsync(AID)
                                               If Picture IsNot Nothing Then
                                                   Episode_Cover.Source = Utils.ImageSourceFromBitmap(Picture, ImageScalingFactor)
                                                   Mwnd.MainCache.AddPictureToCache(AID, Picture)
                                               End If
                                           End If
                                       Else
                                           Dim Picture = Await API.AniDB.AniDBClient.SharedFunctions.GetAniDBImageAsync(AID)
                                           If Picture IsNot Nothing Then
                                               Episode_Cover.Source = Utils.ImageSourceFromBitmap(Picture, ImageScalingFactor)
                                               Mwnd.MainCache.AddPictureToCache(AID, Picture)
                                           End If
                                       End If
                                   End Function, System.Windows.Threading.DispatcherPriority.Render)
#Enable Warning
        End If
    End Sub
End Class
