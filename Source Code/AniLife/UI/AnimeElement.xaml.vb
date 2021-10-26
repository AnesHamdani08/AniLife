Imports AniLife.API
Public Class AnimeElement
    Public Const ImageScalingFactor As Double = 6
    Public Property AnimePicture As ImageSource
        Get
            Return Anime_Picture.Source
        End Get
        Set(value As ImageSource)
            Anime_Picture.Source = value
        End Set
    End Property
    Private _EpProg As Integer
    Public Property EpProgress As Integer
        Get
            Return _EpProg
        End Get
        Set(value As Integer)
            _EpProg = value
            EpisodeProgress.Text = AniResolver.EP & Space(1) & value
            Dim TT = TryCast(ToolTip, AnimeElementTip)
            If TT IsNot Nothing Then
                TT.EpisodeProgress = value
            End If
        End Set
    End Property
    Private _EpCount As Integer
    Public Property EpCount As Integer
        Get
            Return _EpCount
        End Get
        Set(value As Integer)
            _EpCount = value
            Dim MyAniTip = Resources("AniTip")
            ToolTip = New AnimeElementTip(AnimeName, EpProgress, EpCount)
        End Set
    End Property
    Property _AnimeName As String
    Public Property AnimeName As String
        Get
            Return _AnimeName
        End Get
        Set(value As String)
            _AnimeName = value
            ToolTip = New AnimeElementTip(AnimeName, EpProgress, EpCount)
        End Set
    End Property
    Public Property IncrementVisibility As Visibility
        Get
            Return Anime_IncrProgress.Visibility
        End Get
        Set(value As Visibility)
            Anime_IncrProgress.Visibility = value
        End Set
    End Property
    Public Property EpProVisibility As Visibility
        Get
            Return EpisodeProgress.Visibility
        End Get
        Set(value As Visibility)
            EpisodeProgress.Visibility = value
        End Set
    End Property
    Public Property RedirectToUpdateSearchTab
    Public Shadows Property Tag As AniDB.AniDBClient.AnimeElement
    Public Property LoadAnimePicture As Boolean = True
    Public Sub New(AnimeElement As AniDB.AniDBClient.AnimeElement, EpProg As Integer, Optional PreloadAnimePicture As Boolean = True, Optional SkipScaling As Boolean = False)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Tag = AnimeElement
        _AnimeName = AnimeElement.Title
        LoadAnimePicture = PreloadAnimePicture
        If AnimeElement.Picture IsNot Nothing Then
            LoadAnimePicture = False
            AnimePicture = Utils.ImageSourceFromBitmap(AnimeElement.Picture, ImageScalingFactor)
        End If
        EpProgress = EpProg
        EpCount = AnimeElement.EpisodeCount
        ToolTip = New AnimeElementTip(AnimeName, EpProg, EpCount)
    End Sub
    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub
    Private Sub AnimeElement_MouseEnter(sender As Object, e As MouseEventArgs) Handles Me.MouseEnter
        BeginAnimation(BorderThicknessProperty, New Animation.ThicknessAnimation(New Thickness(3, 3, 3, 3), New Duration(TimeSpan.FromMilliseconds(100))))
        Anime_IncrProgress.BeginAnimation(MarginProperty, New Animation.ThicknessAnimation(New Thickness(30, 50, 30, 50), New Duration(TimeSpan.FromMilliseconds(100))))
    End Sub

    Private Sub AnimeElement_MouseLeave(sender As Object, e As MouseEventArgs) Handles Me.MouseLeave
        Anime_IncrProgress.BeginAnimation(MarginProperty, New Animation.ThicknessAnimation(New Thickness(50, 70, 50, 70), New Duration(TimeSpan.FromMilliseconds(100))))
        BeginAnimation(BorderThicknessProperty, New Animation.ThicknessAnimation(New Thickness(1, 1, 1, 1), New Duration(TimeSpan.FromMilliseconds(100))))
    End Sub

    Private Sub AnimeElement_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles Me.MouseLeftButtonUp
        If RedirectToUpdateSearchTab Then
            Dim MWnd = TryCast(Application.Current.MainWindow, MainWindow)
            If MWnd IsNot Nothing Then
                If Tag IsNot Nothing Then
                    MWnd.UpdateSearchTab(Tag)
                End If
            End If
        End If
    End Sub

    Private Sub Anime_IncrProgress_Click(sender As Object, e As RoutedEventArgs) Handles Anime_IncrProgress.Click
        If EpProgress <> EpCount Then
            Dim MWnd = TryCast(Application.Current.MainWindow, MainWindow)
            If MWnd IsNot Nothing Then
                If TypeOf Tag Is AniDB.AniDBClient.AnimeElement Then
                    If EpProgress + 1 = EpCount Then
                        MWnd.MainLibrary.UpdateItemProgress(Tag.ID, 1, True)
                        MWnd.MainLibrary.MoveFromCollection(MWnd.MainLibrary.GetByID(Tag.ID), AniLibrary.Status.Completed)
                        EpProgress += 1
                    Else
                        MWnd.MainLibrary.UpdateItemProgress(Tag.ID, 1, True)
                        EpProgress += 1
                        ToolTip = New AnimeElementTip(AnimeName, EpProgress, EpCount)
                    End If
                End If
            End If
        End If
    End Sub

    Private Sub AnimeElement_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        If LoadAnimePicture Then
#Disable Warning
            Dispatcher.BeginInvoke(Async Function()
                                       Dim Mwnd = TryCast(Application.Current.MainWindow, MainWindow)
                                       If Mwnd IsNot Nothing Then
                                           If Mwnd.MainCache.CheckIfAnimeExists(Tag.ID) Then
                                               Tag.Picture = Mwnd.MainCache.GetPicture(Tag.ID)
                                               Anime_Picture.Source = Utils.ImageSourceFromBitmap(Tag.Picture, ImageScalingFactor)
                                           Else
                                               Tag.Picture = Await API.AniDB.AniDBClient.SharedFunctions.GetAniDBImageAsync(Tag.PictureURL)
                                               Anime_Picture.Source = Utils.ImageSourceFromBitmap(Tag.Picture, ImageScalingFactor)
                                               Mwnd.MainCache.AddPictureToCache(Tag.ID, Tag.Picture)
                                           End If
                                       Else
                                           Tag.Picture = Await API.AniDB.AniDBClient.SharedFunctions.GetAniDBImageAsync(Tag.PictureURL)
                                           Anime_Picture.Source = Utils.ImageSourceFromBitmap(Tag.Picture, ImageScalingFactor)
                                           Mwnd.MainCache.AddPictureToCache(Tag.ID, Tag.Picture)
                                       End If
                                   End Function, System.Windows.Threading.DispatcherPriority.Render)
#Enable Warning
        End If
    End Sub
End Class
