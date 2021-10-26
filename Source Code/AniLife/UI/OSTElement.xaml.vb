''' <summary>
''' AniUIKit.OSTElement
''' </summary>
Public Class OSTElement
    Implements IDisposable
    Public Shadows Property Tag As AniServer.Data.DataItem.OSTElement
    Public Property LoadAnimePicture As Boolean = True
    ''' <summary>
    ''' Create a new instance using CharacterElement
    ''' </summary>
    ''' <param name="OSTElement">Element to use</param>
    ''' <param name="PreLoadAnimePicture">Self-Explanatory</param>
    Public Sub New(OSTElement As AniServer.Data.DataItem.OSTElement, Optional PreLoadAnimePicture As Boolean = True)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Tag = OSTElement
        If OSTElement IsNot Nothing Then
            OST_Title.Text = OSTElement.Title
            OST_Artist.Text = OSTElement.Artist
            OST_Anime.Text = AniResolver.FROM & Space(1) & OSTElement.Anime
            LoadAnimePicture = PreLoadAnimePicture
            If OSTElement.Picture Is Nothing Then
                If PreLoadAnimePicture Then
                    Dispatcher.InvokeAsync(Async Function()
                                               If Await OSTElement.LoadPicture Then
                                                   OST_Image.Source = Utils.ImageSourceFromBitmap(OSTElement.Picture, 4)
                                               End If
                                           End Function, System.Windows.Threading.DispatcherPriority.Render)
                End If
            Else
                OST_Image.Source = Utils.ImageSourceFromBitmap(OSTElement.Picture, 4)
            End If
        End If
    End Sub
    ''' <summary>
    ''' Creates a new blank instance
    ''' </summary>
    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub
    Private Sub AnimeElement_MouseEnter(sender As Object, e As MouseEventArgs) Handles Me.MouseEnter
        BeginAnimation(BorderThicknessProperty, New Animation.ThicknessAnimation(New Thickness(3, 3, 3, 3), New Duration(TimeSpan.FromMilliseconds(100))))
    End Sub

    Private Sub AnimeElement_MouseLeave(sender As Object, e As MouseEventArgs) Handles Me.MouseLeave
        BeginAnimation(BorderThicknessProperty, New Animation.ThicknessAnimation(New Thickness(1, 1, 1, 1), New Duration(TimeSpan.FromMilliseconds(100))))
    End Sub

    Private Sub OSTElement_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles Me.MouseLeftButtonUp
        Dim MWnd = TryCast(Application.Current.MainWindow, MainWindow)
        If MWnd IsNot Nothing Then
            MWnd.IsLoading = True
            MWnd.LoadingState = AniResolver.FETCHING
            MWnd.LoadSong(Tag.ID, True)
            MWnd.IsLoading = False
        End If
    End Sub

#Region "IDisposable Support"

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If disposing Then
            ' TODO: dispose managed state (managed objects).
            Tag.Picture?.Dispose()
            Tag.Picture = Nothing
            OST_Image.ClearValue(Image.SourceProperty)
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
