Imports AniLife.API
''' <summary>
''' AniUIKit.CharacterElement
''' </summary>
Public Class CharacterElement
    Implements IDisposable
    Public Shadows Property Tag As AniDB.AniDBClient.CharacterElement
    Public Property LoadAnimePicture As Boolean = True
    ''' <summary>
    ''' Create a new instance using CharacterElement
    ''' </summary>
    ''' <param name="CharacterElement">Element to use</param>
    ''' <param name="PreLoadAnimePicture">Self-Explanatory</param>
    Public Sub New(CharacterElement As AniDB.AniDBClient.CharacterElement, Optional PreLoadAnimePicture As Boolean = True)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Tag = CharacterElement
        If CharacterElement IsNot Nothing Then
            Character_Name.Text = CharacterElement.Name
            Character_Type.Text = CharacterElement.Type.ToString
            Select Case CharacterElement.Gender.ToLower
                Case "male"
                    Character_Gender.Source = AniResolver.MALE
                Case "female"
                    Character_Gender.Source = AniResolver.FEMALE
            End Select
            Character_Description.Text = CharacterElement.Description
            LoadAnimePicture = PreLoadAnimePicture
            If CharacterElement.Picture Is Nothing Then
                If PreLoadAnimePicture Then
                    Dispatcher.InvokeAsync(Async Function()
                                               Dim Mwnd = TryCast(Application.Current.MainWindow, MainWindow)
                                               If Mwnd IsNot Nothing Then
                                                   Dim pic = Mwnd.MainCache.GetPicture(CharacterElement.ID, 4)
                                                   If pic IsNot Nothing Then
                                                       Character_Image.Source = Utils.ImageSourceFromBitmap(pic)
                                                   Else
                                                       Dim CharacterPicture = Await AniDB.AniDBClient.SharedFunctions.GetAniDBImageAsync(CharacterElement.PictureURL)
                                                       Mwnd.MainCache.AddPictureToCache(CharacterElement.ID, CharacterPicture)
                                                       Character_Image.Source = Utils.ImageSourceFromBitmap(CharacterPicture)
                                                   End If
                                               Else
                                                   Dim CharacterPicture = Await AniDB.AniDBClient.SharedFunctions.GetAniDBImageAsync(CharacterElement.PictureURL)
                                                   Mwnd.MainCache.AddPictureToCache(CharacterElement.ID, CharacterPicture)
                                                   Character_Image.Source = Utils.ImageSourceFromBitmap(CharacterPicture)
                                               End If
                                           End Function, System.Windows.Threading.DispatcherPriority.Render)
                End If
            Else
                Character_Image.Source = Utils.ImageSourceFromBitmap(CharacterElement.Picture)
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

    Public Sub LoadCharacterImages(Optional CacheOnly As Boolean = False)
        If Tag.Picture Is Nothing Then
            Dispatcher.InvokeAsync(Async Function()
                                       Dim Mwnd = TryCast(Application.Current.MainWindow, MainWindow)
                                       If Mwnd IsNot Nothing Then
                                           Dim pic = Mwnd.MainCache.GetPicture(Tag.ID)
                                           If pic IsNot Nothing Then
                                               Character_Image.Source = Utils.ImageSourceFromBitmap(pic)
                                           Else
                                               If CacheOnly = False Then
                                                   Dim CharacterPicture = Await AniDB.AniDBClient.SharedFunctions.GetAniDBImageAsync(Tag.PictureURL)
                                                   Mwnd.MainCache.AddPictureToCache(Tag.ID, CharacterPicture)
                                                   Character_Image.Source = Utils.ImageSourceFromBitmap(CharacterPicture)
                                               End If
                                           End If
                                       Else
                                           If CacheOnly = False Then
                                               Dim CharacterPicture = Await AniDB.AniDBClient.SharedFunctions.GetAniDBImageAsync(Tag.PictureURL)
                                               Mwnd.MainCache.AddPictureToCache(Tag.ID, CharacterPicture)
                                               Character_Image.Source = Utils.ImageSourceFromBitmap(CharacterPicture)
                                           End If
                                       End If
                                   End Function, System.Windows.Threading.DispatcherPriority.Render)
        Else
            Character_Image.Source = Utils.ImageSourceFromBitmap(Tag.Picture)
        End If
    End Sub

#Region "IDisposable Support"

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If disposing Then
            ' TODO: dispose managed state (managed objects).
            Tag.Picture?.Dispose()
            Tag.Picture = Nothing
            Character_Image.ClearValue(Image.SourceProperty)
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
