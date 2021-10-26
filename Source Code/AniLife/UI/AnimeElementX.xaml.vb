Imports AniLife.API
Public Class AnimeElementX
    Implements IDisposable
    Public Const ImageScalingFactor As Double = 4
    Public Shadows Property Tag As Object
    Public Property LoadAnimePicture As Boolean = True
    Public Sub New(Anime As AniDB.AniDBClient.AnimeElement, Optional PreloadAnimePicture As Boolean = True, Optional SkipScaling As Boolean = False)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Tag = Anime
        If Anime IsNot Nothing Then
            If Anime.Restricted Then CensorOV.Visibility = Visibility.Visible
            AnimeName.Text = Anime.Titles.FirstOrDefault(Function(k) k.Type.ToLower = "main")?.Value
            LoadAnimePicture = PreloadAnimePicture
            If Anime.Picture IsNot Nothing Then
                LoadAnimePicture = False
                AnimeCover.Source = Utils.ImageSourceFromBitmap(Anime.Picture, If(SkipScaling, 1, ImageScalingFactor))
            End If
            ToolTip = New AnimeElementXTip(Anime)
        End If
    End Sub
    Public Sub New(Anime As AniDB.AniDBClient.RecommendationElement, Optional PreloadAnimePicture As Boolean = True, Optional SkipScaling As Boolean = False)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Tag = Anime
        If Anime IsNot Nothing Then
            If Anime.Restricted Then CensorOV.Visibility = Visibility.Visible
            AnimeName.Text = CType(Anime, AniDB.AniDBClient.RecommendationElement).Title.Value
            LoadAnimePicture = PreloadAnimePicture
            If Anime.Picture IsNot Nothing Then
                LoadAnimePicture = False
                AnimeCover.Source = Utils.ImageSourceFromBitmap(Anime.Picture, If(SkipScaling, 1, ImageScalingFactor))
            End If
            ToolTip = New AnimeElementXTip(Anime)
        End If
    End Sub
    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub
    Private Sub AnimeElementX_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        If LoadAnimePicture Then
#Disable Warning
            Dispatcher.BeginInvoke(Async Function()
                                       Dim Mwnd = TryCast(Application.Current.MainWindow, MainWindow)
                                       If Mwnd IsNot Nothing Then
                                           If Mwnd.MainCache.CheckIfAnimeExists(Tag.ID) Then
                                               Tag.Picture = Mwnd.MainCache.GetPicture(Tag.ID)
                                               AnimeCover.Source = Utils.ImageSourceFromBitmap(Tag.Picture, ImageScalingFactor)
                                           Else
                                               Tag.Picture = Await AniDB.AniDBClient.SharedFunctions.GetAniDBImageAsync(Tag.PictureURL)
                                               AnimeCover.Source = Utils.ImageSourceFromBitmap(Tag.Picture, ImageScalingFactor)
                                               Mwnd.MainCache.AddPictureToCache(Tag.ID, Tag.Picture)
                                           End If
                                       Else
                                           Tag.Picture = Await AniDB.AniDBClient.SharedFunctions.GetAniDBImageAsync(Tag.PictureURL)
                                           AnimeCover.Source = Utils.ImageSourceFromBitmap(Tag.Picture, ImageScalingFactor)
                                           Mwnd.MainCache.AddPictureToCache(Tag.ID, Tag.Picture)
                                       End If
                                   End Function, System.Windows.Threading.DispatcherPriority.Render)
#Enable Warning
        End If
    End Sub
    Private Sub AnimeElement_MouseEnter(sender As Object, e As MouseEventArgs) Handles Me.MouseEnter
        BeginAnimation(BorderThicknessProperty, New Animation.ThicknessAnimation(New Thickness(3, 3, 3, 3), New Duration(TimeSpan.FromMilliseconds(100))))
        LibraryControls.BeginAnimation(OpacityProperty, New Animation.DoubleAnimation(1, New Duration(TimeSpan.FromMilliseconds(100))))
        SetAsWatching.BeginAnimation(OpacityProperty, New Animation.DoubleAnimation(1, New Duration(TimeSpan.FromMilliseconds(200))) With {.AccelerationRatio = 0.9})
        SetAsCompleted.BeginAnimation(OpacityProperty, New Animation.DoubleAnimation(1, New Duration(TimeSpan.FromMilliseconds(250))) With {.AccelerationRatio = 0.9})
        SetAsPlanning.BeginAnimation(OpacityProperty, New Animation.DoubleAnimation(1, New Duration(TimeSpan.FromMilliseconds(300))) With {.AccelerationRatio = 0.9})
    End Sub

    Private Sub AnimeElement_MouseLeave(sender As Object, e As MouseEventArgs) Handles Me.MouseLeave
        BeginAnimation(BorderThicknessProperty, New Animation.ThicknessAnimation(New Thickness(1, 1, 1, 1), New Duration(TimeSpan.FromMilliseconds(100))))
        SetAsPlanning.BeginAnimation(OpacityProperty, New Animation.DoubleAnimation(0, New Duration(TimeSpan.FromMilliseconds(200))) With {.AccelerationRatio = 0.9})
        SetAsCompleted.BeginAnimation(OpacityProperty, New Animation.DoubleAnimation(0, New Duration(TimeSpan.FromMilliseconds(250))) With {.AccelerationRatio = 0.9})
        SetAsWatching.BeginAnimation(OpacityProperty, New Animation.DoubleAnimation(0, New Duration(TimeSpan.FromMilliseconds(300))) With {.AccelerationRatio = 0.9})
        LibraryControls.BeginAnimation(OpacityProperty, New Animation.DoubleAnimation(0, New Duration(TimeSpan.FromMilliseconds(500))))
    End Sub
    Public Sub LoadAnimeImages(Optional CacheOnly As Boolean = False)
        If TypeOf Tag Is AniDB.AniDBClient.AnimeElement Then
            Dim anime = CType(Tag, AniDB.AniDBClient.AnimeElement)
            Dispatcher.BeginInvoke(Async Function()
                                       Dim Mwnd = TryCast(Application.Current.MainWindow, MainWindow)
                                       If Mwnd IsNot Nothing Then
                                           If Mwnd.MainCache.CheckIfAnimeExists(anime.ID) Then
                                               anime.Picture = Mwnd.MainCache.GetPicture(anime.ID)
                                               If anime.Picture Is Nothing Then
                                                   If CacheOnly = False Then
                                                       anime.Picture = Await AniDB.AniDBClient.SharedFunctions.GetAniDBImageAsync(anime.PictureURL)
                                                       Mwnd.MainCache.AddPictureToCache(anime.ID, anime.Picture)
                                                   End If
                                               End If
                                               AnimeCover.Source = If(Utils.ImageSourceFromBitmap(anime.Picture, ImageScalingFactor), AniResolver.WARNING)
                                           Else
                                               If CacheOnly = False Then
                                                   anime.Picture = Await AniDB.AniDBClient.SharedFunctions.GetAniDBImageAsync(anime.PictureURL)
                                                   AnimeCover.Source = If(Utils.ImageSourceFromBitmap(anime.Picture, ImageScalingFactor), AniResolver.WARNING)
                                                   Mwnd.MainCache.AddPictureToCache(anime.ID, anime.Picture)
                                               End If
                                           End If
                                       Else
                                           If CacheOnly = False Then
                                               anime.Picture = Await AniDB.AniDBClient.SharedFunctions.GetAniDBImageAsync(anime.PictureURL)
                                               AnimeCover.Source = If(Utils.ImageSourceFromBitmap(anime.Picture, ImageScalingFactor), AniResolver.WARNING)
                                               Mwnd.MainCache.AddPictureToCache(anime.ID, anime.Picture)
                                           End If
                                       End If
                                   End Function)
        ElseIf TypeOf Tag Is AniDB.AniDBClient.RecommendationElement Then
            Dim anime = CType(Tag, AniDB.AniDBClient.RecommendationElement)
            Dispatcher.BeginInvoke(Async Function()
                                       Dim Mwnd = TryCast(Application.Current.MainWindow, MainWindow)
                                       If Mwnd IsNot Nothing Then
                                           If Mwnd.MainCache.CheckIfAnimeExists(anime.ID) Then
                                               anime.Picture = Mwnd.MainCache.GetPicture(anime.ID)
                                               If anime.Picture Is Nothing Then
                                                   anime.Picture = Await AniDB.AniDBClient.SharedFunctions.GetAniDBImageAsync(anime.PictureURL)
                                                   Mwnd.MainCache.AddPictureToCache(anime.ID, anime.Picture)
                                               End If
                                               AnimeCover.Source = If(Utils.ImageSourceFromBitmap(anime.Picture, ImageScalingFactor), AniResolver.WARNING)
                                           Else
                                               anime.Picture = Await AniDB.AniDBClient.SharedFunctions.GetAniDBImageAsync(anime.PictureURL)
                                               AnimeCover.Source = If(Utils.ImageSourceFromBitmap(anime.Picture, ImageScalingFactor), AniResolver.WARNING)
                                               Mwnd.MainCache.AddPictureToCache(anime.ID, anime.Picture)
                                           End If
                                       Else
                                           anime.Picture = Await AniDB.AniDBClient.SharedFunctions.GetAniDBImageAsync(anime.PictureURL)
                                           AnimeCover.Source = If(Utils.ImageSourceFromBitmap(anime.Picture, ImageScalingFactor), AniResolver.WARNING)
                                           Mwnd.MainCache.AddPictureToCache(anime.ID, anime.Picture)
                                       End If
                                   End Function)
        End If
    End Sub
    Private Async Sub SetAsCompleted_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles SetAsCompleted.MouseLeftButtonUp
        Dim MWnd = TryCast(Application.Current.MainWindow, MainWindow)
        If MWnd IsNot Nothing Then
            If TypeOf Tag Is AniDB.AniDBClient.AnimeElement Then
                With CType(Tag, AniDB.AniDBClient.AnimeElement)
                    MWnd.MainLibrary.AddToCollection(New AniLibrary.AnimeElement(.ID, 0, .EpisodeCount, Nothing, AniLibrary.Status.Completed))
                End With
                MWnd.MainCache.AddToCache(CType(Tag, AniDB.AniDBClient.AnimeElement))
            ElseIf TypeOf Tag Is AniDB.AniDBClient.RecommendationElement Then
                With CType(Tag, AniDB.AniDBClient.RecommendationElement)
                    MWnd.LoadingState = AniResolver.RETRIEVING & Space(1) & AniResolver.FOR & Space(1) & .Title.Value
                    MWnd.IsLoading = True
                    If MWnd.MainCache.CheckIfAnimeExists(.ID) Then
                        Dim Data = Await MWnd.MainCache.GetAnime(.ID)
                        MWnd.MainLibrary.AddToCollection(New AniLibrary.AnimeElement(.ID, 0, .EpisodeCount, Nothing, AniLibrary.Status.Completed))
                        MWnd.IsLoading = False
                    Else
                        Dim Data = Await MWnd.MainClient.Anime(.ID, My.Settings.CLIENT_LOADANIMEPICTURES, My.Settings.CLIENT_LOADCHARACTERS, My.Settings.CLIENT_LOADCHARACTERSPICTURES)
                        If Data IsNot Nothing Then
                            MWnd.MainLibrary.AddToCollection(New AniLibrary.AnimeElement(.ID, 0, .EpisodeCount, Nothing, AniLibrary.Status.Completed))
                            MWnd.MainCache.AddToCache(Data)
                            MWnd.IsLoading = False
                        Else
                            MWnd.LoadingState = AniResolver.RETRIEVING & Space(1) & AniResolver.DATA & Space(1) & AniResolver.FAILED & ", " & AniResolver.RETRYING
                            Await Task.Delay(2000)
                            MWnd.LoadingState = AniResolver.RETRIEVING & Space(1) & AniResolver.FOR & Space(1) & .Title.Value
                            Dim _AniClient As New AniDB.AniDBClient(My.Settings.APP_CLIENT, My.Settings.APP_CLIENTVER)
                            Dim _Data = Await _AniClient.Anime(.ID, My.Settings.CLIENT_LOADANIMEPICTURES, My.Settings.CLIENT_LOADCHARACTERS, My.Settings.CLIENT_LOADCHARACTERSPICTURES)
                            If _Data IsNot Nothing Then
                                MWnd.MainLibrary.AddToCollection(New AniLibrary.AnimeElement(.ID, 0, .EpisodeCount, Nothing, AniLibrary.Status.Completed))
                                MWnd.MainCache.AddToCache(_Data)
                                MWnd.IsLoading = False
                            Else
                                MWnd.LoadingState = AniResolver.RETRIEVING & Space(1) & AniResolver.DATA & Space(1) & AniResolver.FAILED & ", " & AniResolver.ABORTING
                                Await Task.Delay(2000)
                                MWnd.IsLoading = False
                            End If
                        End If
                    End If
                End With
            End If
        End If
        e.Handled = True
    End Sub

    Private Async Sub SetAsPlanning_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles SetAsPlanning.MouseLeftButtonUp
        Dim MWnd = TryCast(Application.Current.MainWindow, MainWindow)
        If MWnd IsNot Nothing Then
            If TypeOf Tag Is AniDB.AniDBClient.AnimeElement Then
                With CType(Tag, AniDB.AniDBClient.AnimeElement)
                    MWnd.MainLibrary.AddToCollection(New AniLibrary.AnimeElement(.ID, 0, 0, Nothing, AniLibrary.Status.Planning))
                End With
                MWnd.MainCache.AddToCache(CType(Tag, AniDB.AniDBClient.AnimeElement))
            ElseIf TypeOf Tag Is AniDB.AniDBClient.RecommendationElement Then
                With CType(Tag, AniDB.AniDBClient.RecommendationElement)
                    MWnd.LoadingState = AniResolver.RETRIEVING & Space(1) & AniResolver.FOR & Space(1) & .Title.Value
                    MWnd.IsLoading = True
                    If MWnd.MainCache.CheckIfAnimeExists(.ID) Then
                        Dim Data = Await MWnd.MainCache.GetAnime(.ID)
                        MWnd.MainLibrary.AddToCollection(New AniLibrary.AnimeElement(.ID, 0, .EpisodeCount, Nothing, AniLibrary.Status.Planning))
                        MWnd.IsLoading = False
                    Else
                        Dim Data = Await MWnd.MainClient.Anime(.ID, My.Settings.CLIENT_LOADANIMEPICTURES, My.Settings.CLIENT_LOADCHARACTERS, My.Settings.CLIENT_LOADCHARACTERSPICTURES)
                        If Data IsNot Nothing Then
                            MWnd.MainLibrary.AddToCollection(New AniLibrary.AnimeElement(.ID, 0, .EpisodeCount, Nothing, AniLibrary.Status.Planning))
                            MWnd.MainCache.AddToCache(Data)
                            MWnd.IsLoading = False
                        Else
                            MWnd.LoadingState = AniResolver.RETRIEVING & Space(1) & AniResolver.DATA & Space(1) & AniResolver.FAILED & ", " & AniResolver.RETRYING
                            Await Task.Delay(2000)
                            MWnd.LoadingState = AniResolver.RETRIEVING & Space(1) & AniResolver.FOR & Space(1) & .Title.Value
                            Dim _AniClient As New AniDB.AniDBClient(My.Settings.APP_CLIENT, My.Settings.APP_CLIENTVER)
                            Dim _Data = Await _AniClient.Anime(.ID, My.Settings.CLIENT_LOADANIMEPICTURES, My.Settings.CLIENT_LOADCHARACTERS, My.Settings.CLIENT_LOADCHARACTERSPICTURES)
                            If _Data IsNot Nothing Then
                                MWnd.MainLibrary.AddToCollection(New AniLibrary.AnimeElement(.ID, 0, .EpisodeCount, Nothing, AniLibrary.Status.Planning))
                                MWnd.MainCache.AddToCache(_Data)
                                MWnd.IsLoading = False
                            Else
                                MWnd.LoadingState = AniResolver.RETRIEVING & Space(1) & AniResolver.DATA & Space(1) & AniResolver.FAILED & ", " & AniResolver.ABORTING
                                Await Task.Delay(2000)
                                MWnd.IsLoading = False
                            End If
                        End If
                    End If
                End With
            End If
        End If
        e.Handled = True
    End Sub

    Private Async Sub SetAsWatching_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles SetAsWatching.MouseLeftButtonUp
        Dim MWnd = TryCast(Application.Current.MainWindow, MainWindow)
        If MWnd IsNot Nothing Then
            If TypeOf Tag Is AniDB.AniDBClient.AnimeElement Then
                With CType(Tag, AniDB.AniDBClient.AnimeElement)
                    MWnd.MainLibrary.AddToCollection(New AniLibrary.AnimeElement(.ID, 0, 0, Nothing, AniLibrary.Status.Watching))
                End With
                MWnd.MainCache.AddToCache(CType(Tag, AniDB.AniDBClient.AnimeElement))
            ElseIf TypeOf Tag Is AniDB.AniDBClient.RecommendationElement Then
                With CType(Tag, AniDB.AniDBClient.RecommendationElement)
                    MWnd.LoadingState = AniResolver.RETRIEVING & Space(1) & AniResolver.FOR & Space(1) & .Title.Value
                    MWnd.IsLoading = True
                    If MWnd.MainCache.CheckIfAnimeExists(.ID) Then
                        Dim Data = Await MWnd.MainCache.GetAnime(.ID)
                        MWnd.MainLibrary.AddToCollection(New AniLibrary.AnimeElement(.ID, 0, .EpisodeCount, Nothing, AniLibrary.Status.Watching))
                        MWnd.IsLoading = False
                    Else
                        Dim Data = Await MWnd.MainClient.Anime(.ID, My.Settings.CLIENT_LOADANIMEPICTURES, My.Settings.CLIENT_LOADCHARACTERS, My.Settings.CLIENT_LOADCHARACTERSPICTURES)
                        If Data IsNot Nothing Then
                            MWnd.MainLibrary.AddToCollection(New AniLibrary.AnimeElement(.ID, 0, .EpisodeCount, Nothing, AniLibrary.Status.Watching))
                            MWnd.MainCache.AddToCache(Data)
                            MWnd.IsLoading = False
                        Else
                            MWnd.LoadingState = AniResolver.RETRIEVING & Space(1) & AniResolver.DATA & Space(1) & AniResolver.FAILED & ", " & AniResolver.RETRYING
                            Await Task.Delay(2000)
                            MWnd.LoadingState = AniResolver.RETRIEVING & Space(1) & AniResolver.FOR & Space(1) & .Title.Value
                            Dim _AniClient As New AniDB.AniDBClient(My.Settings.APP_CLIENT, My.Settings.APP_CLIENTVER)
                            Dim _Data = Await _AniClient.Anime(.ID, My.Settings.CLIENT_LOADANIMEPICTURES, My.Settings.CLIENT_LOADCHARACTERS, My.Settings.CLIENT_LOADCHARACTERSPICTURES)
                            If _Data IsNot Nothing Then
                                MWnd.MainLibrary.AddToCollection(New AniLibrary.AnimeElement(.ID, 0, .EpisodeCount, Nothing, AniLibrary.Status.Watching))
                                MWnd.MainCache.AddToCache(_Data)
                                MWnd.IsLoading = False
                            Else
                                MWnd.LoadingState = AniResolver.RETRIEVING & Space(1) & AniResolver.DATA & Space(1) & AniResolver.FAILED & ", " & AniResolver.ABORTING
                                Await Task.Delay(2000)
                                MWnd.IsLoading = False
                            End If
                        End If
                    End If
                End With
            End If
        End If
        e.Handled = True
    End Sub

    Private Async Sub AnimeElementX_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles Me.MouseLeftButtonUp
        Dim MWnd = TryCast(Application.Current.MainWindow, MainWindow)
        If MWnd IsNot Nothing Then
            If TypeOf Tag Is AniDB.AniDBClient.AnimeElement Then
                MWnd.UpdateSearchTab(Tag)
            ElseIf TypeOf Tag Is AniDB.AniDBClient.RecommendationElement Then
                MWnd.LoadingState = AniResolver.FETCHING & Space(1) & AniResolver.DATA
                MWnd.IsLoading = True
                If MWnd.MainCache.CheckIfAnimeExists(CType(Tag, AniDB.AniDBClient.RecommendationElement).ID) Then
                    Dim Anime = Await MWnd.MainCache.GetAnime(CType(Tag, AniDB.AniDBClient.RecommendationElement).ID, My.Settings.CLIENT_LOADANIMEPICTURES, My.Settings.CLIENT_LOADCHARACTERS, My.Settings.CLIENT_LOADCHARACTERSPICTURES)
                    MWnd.UpdateSearchTab(Anime)
                    MWnd.IsLoading = False
                Else
                    If CType(Tag, AniDB.AniDBClient.RecommendationElement).Picture Is Nothing Then
                        Dim Anime = Await MWnd.MainClient.Anime(CType(Tag, AniDB.AniDBClient.RecommendationElement).ID, My.Settings.CLIENT_LOADANIMEPICTURES, My.Settings.CLIENT_LOADCHARACTERS, My.Settings.CLIENT_LOADCHARACTERSPICTURES)
                        MWnd.UpdateSearchTab(Anime)
                        MWnd.IsLoading = False
                    Else
                        Dim Anime = Await MWnd.MainClient.Anime(CType(Tag, AniDB.AniDBClient.RecommendationElement).ID, My.Settings.CLIENT_LOADANIMEPICTURES, My.Settings.CLIENT_LOADCHARACTERS, My.Settings.CLIENT_LOADCHARACTERSPICTURES)
                        Anime.Picture = CType(Tag, AniDB.AniDBClient.RecommendationElement).Picture
                        MWnd.UpdateSearchTab(Anime)
                        MWnd.IsLoading = False
                    End If
                End If
            End If
        End If
    End Sub

    Private Sub CensorOV_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs)
        CensorOV.Visibility = Visibility.Collapsed
    End Sub
#Region "IDisposable Support"

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If disposing Then
            ' TODO: dispose managed state (managed objects).
            If TypeOf Tag Is AniDB.AniDBClient.AnimeElement Then
                With CType(Tag, AniDB.AniDBClient.AnimeElement)
                    If .Picture IsNot Nothing Then .Picture.Dispose()
                    .Picture = Nothing
                    For Each character In .Characters
                        If character.Picture IsNot Nothing Then character.Picture.Dispose()
                        character.Picture = Nothing
                    Next
                End With
            ElseIf TypeOf Tag Is AniDB.AniDBClient.RecommendationElement Then
                With CType(Tag, AniDB.AniDBClient.RecommendationElement)
                    If .Picture IsNot Nothing Then .Picture.Dispose()
                    .Picture = Nothing
                End With
            End If
            AnimeCover.Source = AniResolver.WARNING
        End If
        ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
        ' TODO: set large fields to null.                    
    End Sub

    ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
    'Protected Overrides Sub Finalize()
    '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(True)
        ' TODO: uncomment the following line if Finalize() is overridden above.
        ' GC.SuppressFinalize(Me)
    End Sub
#End Region
End Class
