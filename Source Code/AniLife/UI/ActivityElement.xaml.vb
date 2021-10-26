Imports AniLife.API
Public Class ActivityElement
    Implements IDisposable
    Public Const ImageScalingFactor As Double = 6
    Public Shadows Property Tag As AniLibrary.LibraryActivityElement
    Public Sub New(_ActivityElement As AniLibrary.LibraryActivityElement, Optional AnimePicture As ImageSource = Nothing, Optional PreloadPicture As Boolean = True, Optional CacheOnly As Boolean = False)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Tag = _ActivityElement
        If Not String.IsNullOrEmpty(_ActivityElement.Value) Then Activity_Value.Text = _ActivityElement.Value
        If AnimePicture IsNot Nothing Then
            Anime_Picture.Source = AnimePicture
        Else
            If PreloadPicture Then
                Dispatcher.InvokeAsync(Async Function()
                                           Dim Mwnd = TryCast(Application.Current.MainWindow, MainWindow)
                                           If Mwnd IsNot Nothing Then
                                               If Mwnd.MainCache.CheckIfAnimeExists(Tag.ID) Then
                                                   Dim anime = Await Mwnd.MainCache.GetAnime(Tag.ID)
                                                   If anime.Picture Is Nothing AndAlso CacheOnly = False Then
                                                       anime.Picture = Await AniDB.AniDBClient.SharedFunctions.GetAniDBImageAsync(anime.PictureURL)
                                                       Mwnd.MainCache.AddPictureToCache(anime.ID, anime.Picture)
                                                   End If
                                                   Anime_Picture.Source = If(Utils.ImageSourceFromBitmap(anime.Picture, ImageScalingFactor), AniResolver.WARNING)
                                               End If
                                           End If
                                       End Function, System.Windows.Threading.DispatcherPriority.Render)
            End If
        End If
    End Sub
#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
                Anime_Picture.ClearValue(Image.SourceProperty)
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
            ' TODO: set large fields to null.
            Tag = Nothing
        End If
        disposedValue = True
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
