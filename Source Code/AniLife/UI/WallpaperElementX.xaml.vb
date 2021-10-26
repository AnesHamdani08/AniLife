Imports AniLife.API
Public Class WallpaperElementX
    Implements IDisposable
    Public Const ImageScalingFactor As Double = 1
    Public Shadows Property Tag As MinitokyoClient.WallpaperElement
    Public Property LoadAnimePicture As Boolean = True
    Public Sub New(Wallpaper As MinitokyoClient.WallpaperElement, Optional PreloadAnimePicture As Boolean = True, Optional SkipScaling As Boolean = False)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Tag = Wallpaper
        If Wallpaper IsNot Nothing Then
            AnimeName.Text = Wallpaper.Name & "(" & Wallpaper.Resolution & ")"
            LoadAnimePicture = PreloadAnimePicture
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
                                       MainWindow.LoadingStateHelper.IsElementLoading = True
                                       MainWindow.LoadingStateHelper.ElementLoadingState = AniResolver.RETRIEVINGANIMEDATA
                                       Dim PIC = Await API.MinitokyoClient.GetImageAsync(Tag.ThumbLink)
                                       Tag.Thumb = PIC
                                       AnimeCover.Source = Utils.ImageSourceFromBitmap(Tag.Thumb)
                                       MainWindow.LoadingStateHelper.IsElementLoading = False
                                   End Function, System.Windows.Threading.DispatcherPriority.Render)
#Enable Warning
        End If
    End Sub
    Private Sub AnimeElement_MouseEnter(sender As Object, e As MouseEventArgs) Handles Me.MouseEnter
        BeginAnimation(BorderThicknessProperty, New Animation.ThicknessAnimation(New Thickness(3, 3, 3, 3), New Duration(TimeSpan.FromMilliseconds(100))))
        LibraryControls.BeginAnimation(OpacityProperty, New Animation.DoubleAnimation(1, New Duration(TimeSpan.FromMilliseconds(100))))
        SetAsDesktopWallpaper.BeginAnimation(OpacityProperty, New Animation.DoubleAnimation(1, New Duration(TimeSpan.FromMilliseconds(200))) With {.AccelerationRatio = 0.9})
        SetAsLockscreen.BeginAnimation(OpacityProperty, New Animation.DoubleAnimation(1, New Duration(TimeSpan.FromMilliseconds(250))) With {.AccelerationRatio = 0.9})
        SetAsView.BeginAnimation(OpacityProperty, New Animation.DoubleAnimation(1, New Duration(TimeSpan.FromMilliseconds(300))) With {.AccelerationRatio = 0.9})
    End Sub

    Private Sub AnimeElement_MouseLeave(sender As Object, e As MouseEventArgs) Handles Me.MouseLeave
        BeginAnimation(BorderThicknessProperty, New Animation.ThicknessAnimation(New Thickness(1, 1, 1, 1), New Duration(TimeSpan.FromMilliseconds(100))))
        SetAsView.BeginAnimation(OpacityProperty, New Animation.DoubleAnimation(0, New Duration(TimeSpan.FromMilliseconds(200))) With {.AccelerationRatio = 0.9})
        SetAsLockscreen.BeginAnimation(OpacityProperty, New Animation.DoubleAnimation(0, New Duration(TimeSpan.FromMilliseconds(250))) With {.AccelerationRatio = 0.9})
        SetAsDesktopWallpaper.BeginAnimation(OpacityProperty, New Animation.DoubleAnimation(0, New Duration(TimeSpan.FromMilliseconds(300))) With {.AccelerationRatio = 0.9})
        LibraryControls.BeginAnimation(OpacityProperty, New Animation.DoubleAnimation(0, New Duration(TimeSpan.FromMilliseconds(500))))
    End Sub

#Region "IDisposable Support"

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If disposing Then
            ' TODO: dispose managed state (managed objects).         
            AnimeCover.ClearValue(Image.SourceProperty)
            Tag.Wallpaper?.Dispose()
            Tag.Wallpaper = Nothing
            Tag.Thumb?.Dispose()
            Tag.Thumb = Nothing
            Tag.Cover?.Dispose()
            Tag.Cover = Nothing
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

    Private Async Sub SetAsDesktopWallpaper_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles SetAsDesktopWallpaper.MouseLeftButtonUp
        If Tag.Wallpaper Is Nothing Then
            MainWindow.LoadingStateHelper.IsElementLoading = True
            MainWindow.LoadingStateHelper.ElementLoadingState = AniResolver.FETCHING
            Dim WP = Await API.MinitokyoClient.GetImageAsync(Tag.WallpaperDirectLink)
            Tag.Wallpaper = WP
            MainWindow.LoadingStateHelper.IsElementLoading = False
        End If
        If Tag.Wallpaper IsNot Nothing Then
            Try
                Console.WriteLine(Utils.ConsoleInfoText & "Saving Wallpaper to Data Folder")
                Dim ImagePath As String = IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AniLife", "Wallpapers", "WALLPAPER.PNG")
                Tag.Wallpaper.Save(ImagePath, System.Drawing.Imaging.ImageFormat.Png)
                MinitokyoClient.SetAsDesktopWallpaper(ImagePath)
            Catch ex As Exception
                Console.WriteLine(Utils.ConsoleDebugText & ex.ToString)
            End Try
        End If
    End Sub

    Private Async Sub SetAsLockscreen_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles SetAsLockscreen.MouseLeftButtonUp
        If Tag.Wallpaper Is Nothing Then
            MainWindow.LoadingStateHelper.IsElementLoading = True
            MainWindow.LoadingStateHelper.ElementLoadingState = AniResolver.FETCHING
            Dim WP = Await API.MinitokyoClient.GetImageAsync(Tag.WallpaperDirectLink)
            Tag.Wallpaper = WP
            MainWindow.LoadingStateHelper.IsElementLoading = False
        End If
        If Tag.Wallpaper IsNot Nothing Then
            MainWindow.LoadingStateHelper.IsElementLoading = True
            MainWindow.LoadingStateHelper.ElementLoadingState = AniResolver.SET & Space(1) & AniResolver.WALLPAPER
            Try
                Console.WriteLine(Utils.ConsoleInfoText & "Saving Wallpaper to Data Folder")
                If Not IO.Directory.Exists(IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AniLife", "Wallpapers")) Then IO.Directory.CreateDirectory(IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AniLife", "Wallpapers"))
                Dim ImagePath As String = IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AniLife", "Wallpapers", "LOCKSCREENWALLPAPER.PNG")
                Tag.Wallpaper.Save(ImagePath, System.Drawing.Imaging.ImageFormat.Png)
                Await Task.Run(Sub()
                                   Dim PSI As New ProcessStartInfo With {.FileName = IO.Path.Combine(My.Application.Info.DirectoryPath, "AniLife.exe"), .UseShellExecute = True, .Verb = "runas", .Arguments = "-SET_LOCKWALL """ & ImagePath & """"}
                                   Dim Proc = Process.Start(PSI)
                                   Proc.WaitForExit(2000)
                                   Try
                                       Proc.Kill()
                                   Catch
                                   End Try
                               End Sub)
            Catch ex As Exception
                Console.WriteLine(Utils.ConsoleDebugText & ex.ToString)
            End Try
            MainWindow.LoadingStateHelper.IsElementLoading = False
        End If
    End Sub

    Private Async Sub SetAsView_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles SetAsView.MouseLeftButtonUp
        If Tag.Wallpaper Is Nothing Then
            MainWindow.LoadingStateHelper.IsElementLoading = True
            MainWindow.LoadingStateHelper.ElementLoadingState = AniResolver.FETCHING
            Dim WP = Await API.MinitokyoClient.GetImageAsync(Tag.WallpaperDirectLink)
            Tag.Wallpaper = WP
            MainWindow.LoadingStateHelper.IsElementLoading = False
        End If
        Dim IV As New ImageViewer(TryCast(Utils.ImageSourceFromBitmap(Tag.Wallpaper.Clone), ImageSource)) With {.Owner = Application.Current.MainWindow}
        IV.ShowDialog()
    End Sub
End Class
