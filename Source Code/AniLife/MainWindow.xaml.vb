Imports System.ComponentModel
Imports System.Drawing
Imports AniLife
Imports AniLife.API
Imports AniLife.API.AniDB
Imports Un4seen.Bass

Class MainWindow
    Public Const Version As String = "0.0.0.1-Beta.4"
#Const ISDEBUG = False
#If ISDEBUG Then
    Public Const AniLifeClient As String = "anilifedesktop"
    Public Const AniLifeClientVersion As Integer = 1
    Public WithEvents MainClient As New AniDB.AniDBClient(AniLifeClient, AniLifeClientVersion)
#Else
    Public WithEvents MainClient As New AniDB.AniDBClient(My.Settings.APP_CLIENT, My.Settings.APP_CLIENTVER) With {.AniDBRequestDelay = My.Settings.CLIENT_REQUESTDELAY, .CharacterCap = My.Settings.CLIENT_CHARACTERCAP}
#End If
    Public WithEvents MainLibrary As New AniLibrary()
    Public WithEvents MainCache As New AniCache()
    Public WithEvents MainQuoteClient As New AnimechanClient()
    Private WatchingLVItems As New ObjectModel.ObservableCollection(Of LVItem)
    Private CompletedLVItems As New ObjectModel.ObservableCollection(Of LVItem)
    Private PausedLVItems As New ObjectModel.ObservableCollection(Of LVItem)
    Private DroppedLVItems As New ObjectModel.ObservableCollection(Of LVItem)
    Private PlanningLVItems As New ObjectModel.ObservableCollection(Of LVItem)
    Private CreatorsLVItems As New ObjectModel.ObservableCollection(Of LVCreatorItem)
    Private RelatedLVItems As New ObjectModel.ObservableCollection(Of LVRelatedItem)
    Private SimilarLVItems As New ObjectModel.ObservableCollection(Of LVSimilarItem)
    Private TitlesLVItems As New ObjectModel.ObservableCollection(Of LVTitleItem)

    Public Property IsOverlay As Boolean
        Get
            Return If(LoadingScreen.Visibility = Visibility.Visible, True, False)
        End Get
        Set(value As Boolean)
            If value Then
                LoadingScreen.Visibility = Visibility.Visible
            Else
                LoadingScreen.Visibility = Visibility.Collapsed
                LoadingScreen_State.Text = Nothing
            End If
        End Set
    End Property
    Public Property IsLoading As Boolean
        Get
            If IsBackgroundLoading Then
                Return If(TopBar_Loading.Visibility = Visibility.Visible, True, False)
            Else
                Return If(LoadingScreen_Dancer.Visibility = Visibility.Visible, True, False)
            End If
        End Get
        Set(value As Boolean)
            If IsBackgroundLoading Then
                If value Then
                    TopBar_Loading.IsIndeterminate = True
                    TopBar_Loading.Visibility = Visibility.Visible
                Else
                    TopBar_Loading.Visibility = Visibility.Collapsed
                    TopBar_Loading.IsIndeterminate = False

                    LoadingScreen.Visibility = Visibility.Collapsed
                    LoadingScreen_State.Text = Nothing
                    LoadingScreen_Dancer.Visibility = Visibility.Collapsed
                    LoadingScreen_Dancer.IsLoading = False
                End If
            Else
                If value Then
                    LoadingScreen.Visibility = Visibility.Visible
                    LoadingScreen_Dancer.Visibility = Visibility.Visible
                    LoadingScreen_Dancer.IsLoading = True
                Else
                    LoadingScreen.Visibility = Visibility.Collapsed
                    LoadingScreen_State.Text = Nothing
                    LoadingScreen_Dancer.Visibility = Visibility.Collapsed
                    LoadingScreen_Dancer.IsLoading = False

                    TopBar_Loading.Visibility = Visibility.Collapsed
                    TopBar_Loading.IsIndeterminate = False
                End If
            End If
        End Set
    End Property
    Public Property IsBackgroundLoading As Boolean
    Public Property LoadingState As String
        Get
            Return LoadingScreen_State.Text
        End Get
        Set(value As String)
            If IsBackgroundLoading Then
                Notify(value)
            Else
                LoadingScreen_State.Text = value
            End If
        End Set
    End Property
    Public Sub Notify(message As String)
        Dim Sz = Forms.TextRenderer.MeasureText(message, New Font(AniResolver.FONT.ToString, 12))
        TopBar_MAIL_Notification_Text.Text = message
        Dim OAnim As New Animation.DoubleAnimation(1, 0, New Duration(TimeSpan.FromMilliseconds(250))) With {.AutoReverse = True, .RepeatBehavior = New Animation.RepeatBehavior(2)}
        AddHandler OAnim.Completed, Async Sub()
                                        TopBar_MAIL_Notification_Text_Container.BeginAnimation(WidthProperty, New Animation.DoubleAnimation(Sz.Width + 15, New Duration(TimeSpan.FromMilliseconds(500))) With {.AccelerationRatio = 0.9})
                                        Await Task.Delay(2000)
                                        TopBar_MAIL_Notification_Text_Container.BeginAnimation(WidthProperty, New Animation.DoubleAnimation(0, New Duration(TimeSpan.FromMilliseconds(500))) With {.DecelerationRatio = 0.9})
                                    End Sub
        TopBar_MAIL.BeginAnimation(OpacityProperty, OAnim)
    End Sub

    Private IsFullScreenMode As Boolean = False
    Public Sub ToggleFullscreen()
        IsFullScreen = Not IsFullScreen
        IsFullScreenMode = IsFullScreen
        If IsFullScreen Then
            Utils.NotificationBucket.AddToBucket({New Utils.NotificationBucket.NotificationItem(AniResolver.APPNAME, AniResolver.FULLSCREENWARNING, HandyControl.Data.NotifyIconInfoType.Info)}, True)
        End If
    End Sub
    Public Async Sub LoadCacheElements()
        Dim CCount = Browse_OfflineBrowsing.Children.Count
        Try
            If TypeOf Browse_OfflineBrowsing.Children.Item(CCount - 1) Is CustomElementX Then
                Browse_OfflineBrowsing.Children.RemoveAt(CCount - 1)
            End If
        Catch
        End Try
        If CCount < MainCache.AnimeCount Then
            Task.Run(Async Sub()
                         Console.WriteLine(Utils.ConsoleInfoText & "Fetching Anime Cache")
                         Dim _Animes = Await MainCache.GetAnimes(CCount, CCount + 9, My.Settings.CLIENT_LOADANIMEPICTURES, AnimeElementX.ImageScalingFactor, My.Settings.CLIENT_LOADCHARACTERS, My.Settings.CLIENT_LOADCHARACTERSPICTURES)
                         Console.WriteLine(Utils.ConsoleInfoText & "Anime Cache :" & _Animes.Count)
                         For Each anime In _Animes
                             If anime IsNot Nothing Then
                                 Dispatcher.BeginInvoke(Sub()
                                                            If anime.Restricted AndAlso My.Settings.APP_R18 = False Then Exit Sub
                                                            Browse_OfflineBrowsing.Children.Add(New AnimeElementX(anime, My.Settings.CLIENT_LOADANIMEPICTURES, True) With {.Margin = New Thickness(10, 10, 0, 0)})
                                                        End Sub, System.Windows.Threading.DispatcherPriority.Render)
                             End If
                         Next
                         Dispatcher.BeginInvoke(Sub()
                                                    If CCount < MainCache.AnimeCount Then
                                                        Browse_OfflineBrowsing.Children.Add(New CustomElementX(Async Sub()
                                                                                                                   LoadCacheElements()
                                                                                                               End Sub) With {.Margin = New Thickness(10, 10, 0, 0)})
                                                    End If
                                                End Sub, System.Windows.Threading.DispatcherPriority.Render)
                     End Sub)
        End If
    End Sub
    Public Sub ChangeClient(name As String, ver As Integer)
        MainClient = New AniDBClient(name, ver)
        MainClient_OnClientValidityChanged(MainWindow.LoadingStateHelper.MainWindow.MainClient.IsClientValid)
        MainClient.ValidClient()
    End Sub
    Public Class LoadingStateHelper
        Public Shared Property MainWindow As MainWindow = TryCast(Application.Current.MainWindow, MainWindow)
        Public Shared Property IsElementLoading As Boolean
            Get
                Return If(MainWindow Is Nothing, Nothing, MainWindow.IsLoading)
            End Get
            Set(value As Boolean)
                If MainWindow IsNot Nothing Then MainWindow.IsLoading = value
            End Set
        End Property
        Public Shared Property ElementLoadingState As String
            Get
                Return If(MainWindow Is Nothing, Nothing, MainWindow.LoadingState)
            End Get
            Set(value As String)
                If MainWindow IsNot Nothing Then MainWindow.LoadingState = value
            End Set
        End Property
        Public Shared Sub OpenVideo(path As String)
            MainWindow.Watch_MediaElement.Stop()
            MainWindow.Watch_MediaElement.ClearValue(MediaElement.SourceProperty)
            MainWindow.Watch_MediaElement.SetValue(MediaElement.SourceProperty, New Uri(path, UriKind.Absolute))
            MainWindow.Dispatcher.InvokeAsync(Sub()
                                                  MainWindow.MainTabControl.SelectedIndex = 7
                                              End Sub)
        End Sub
    End Class
    Private Async Sub MainWindow_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized
        Application.Current.MainWindow = Me
        IsBackgroundLoading = My.Settings.APP_USEBACKGROUNDLOADING

        Console.WriteLine(Utils.ConsoleInfoText & "Begin Init at MAINWINDOW Level")
        Utils.NotificationBucket.Delay = 5000
        AddHandler Utils.NotificationBucket.OnNotificationArrived, AddressOf OnNotificationArrived
        Console.WriteLine(Utils.ConsoleInfoText & "NotificationBucket.OnNotificationArrived Handled Successfuly")

        If AniResolver.BG_ISPATH Then
            If AniResolver.BG_ISPATHRELATIVE Then
                Background = New ImageBrush(New BitmapImage(New Uri(AniResolver.BG_PATH, UriKind.Relative)))
            ElseIf AniResolver.BG_ISPATHINTERNAL Then
                Background = New ImageBrush(New BitmapImage(New Uri("pack://application:,,,/AniLife;component/" & AniResolver.BG_PATH)))
            Else
                Background = New ImageBrush(New BitmapImage(New Uri(AniResolver.BG_PATH, UriKind.Absolute)))
            End If
        End If

        If My.Settings.ISFIRSTRUN Then
            Console.WriteLine(Utils.ConsoleInfoText & "Preparing For First Run")
            My.Windows.InitialSetup.ShowDialog()
            MainLibrary.Load(IO.Path.Combine(My.Settings.DATA_LOCATION, "Library", "Library.xml"))
            MainCache.Load(IO.Path.Combine(My.Settings.DATA_LOCATION, "Cache", "Cache.xml"))
            Await MainClient.LoadSearchCache(IO.Path.Combine(My.Settings.DATA_LOCATION, "Cache", "SearchCache.xml"))
        Else
            Console.WriteLine(Utils.ConsoleInfoText & "Loading Libray at " & IO.Path.Combine(My.Settings.DATA_LOCATION, "Library", "Library.xml"))
            MainLibrary.Load(IO.Path.Combine(My.Settings.DATA_LOCATION, "Library", "Library.xml"))
            Console.WriteLine(Utils.ConsoleInfoText & "Loading Cache at " & IO.Path.Combine(My.Settings.DATA_LOCATION, "Cache", "Cache.xml"))
            MainCache.Load(IO.Path.Combine(My.Settings.DATA_LOCATION, "Cache", "Cache.xml"))
            Console.WriteLine(Utils.ConsoleInfoText & "Loading Search Cache at " & IO.Path.Combine(My.Settings.DATA_LOCATION, "Cache", "SearchCache.xml"))
            Await MainClient.LoadSearchCache(IO.Path.Combine(My.Settings.DATA_LOCATION, "Cache", "SearchCache.xml"))
        End If

        If Not Await MainClient.ValidClient Then Notify(AniResolver.INVALIDCLIENT)
        '/////////////////
        Exit Sub
        '////////////////
        About_CacheSize.Text = AniResolver.CALCULATING & "..."
        Task.Run(Async Function()
                     Dim Files = IO.Directory.GetFiles(MainCache.CacheLocation, "*.*", IO.SearchOption.AllDirectories)
                     Dim TotalSize As Long
                     For Each file In Files
                         Dim FI As New IO.FileInfo(file)
                         TotalSize += FI.Length
                         FI = Nothing
                     Next
                     Dispatcher.BeginInvoke(Sub()
                                                Console.WriteLine(Utils.ConsoleInfoText & "Cache Size: " & Utils.FileSizeConverterSTR(TotalSize))
                                                About_CacheSize.Text = AniResolver.CACHESIZE & ": " & Utils.FileSizeConverterSTR(TotalSize)
                                            End Sub)
                 End Function)

        About_CacheLife.Text &= Space(1) & MainClient.CheckCacheLife(IO.Path.Combine(My.Settings.DATA_LOCATION, "Cache", "SearchCache.xml")).ToString
        About_APIRequestDelay.Text &= Space(1) & MainClient.AniDBRequestDelay
        About_AniCache.Text &= Space(1) & AniCache.Version & ";" & "Anime:" & MainCache.AnimeCount & ";Thumbs" & MainCache.ThumbCount
        About_AniDB.Text &= Space(1) & AniDB.AniDBClient.Version
        About_AniDB_Version.Text &= Space(1) & "DataCommands:" & AniDB.AniDBClient.DataCommandsVersion & ";SearchCommands:" & AniDB.AniDBClient.SearchCommandsVersion & ";Helpers" & AniDB.AniDBClient.HelpersVersion
        About_Animechan.Text &= Space(1) & AnimechanClient.Version
        About_AniLibrary.Text &= Space(1) & AniLibrary.Version & ";Anime:" & MainLibrary.Collection.Count
        About_AniResolver.Text &= Space(1) & AniResolver.LinkVersion
        About_AniUIKit.Text &= Space(1) & "0.0.0.1-Beta.2"
        About_AniServer.Text &= Space(1) & AniServer.Version
        About_AUDX.Text &= Space(1) & Player.Version

        Console.WriteLine(Utils.ConsoleInfoText & "Connecting Items")
        LoadingState = AniResolver.CONNECTINGITEMS
        IsLoading = True

        Collection_Watching.ItemsSource = WatchingLVItems
        Collection_Completed.ItemsSource = CompletedLVItems
        Collection_Paused.ItemsSource = PausedLVItems
        Collection_Dropped.ItemsSource = DroppedLVItems
        Collection_Planning.ItemsSource = PlanningLVItems
        Search_Creators.ItemsSource = CreatorsLVItems
        Search_Related.ItemsSource = RelatedLVItems
        Search_Similar.ItemsSource = SimilarLVItems
        Search_Titles.ItemsSource = TitlesLVItems

        Console.WriteLine(Utils.ConsoleInfoText & "Connected Items Successfuly")
        If My.Settings.APP_ISCUSTOMTHEME Then SET_THEME.SelectedIndex = SET_THEME.Items.Count - 1

        SET_LOADANIMEPICTURES.IsChecked = My.Settings.CLIENT_LOADANIMEPICTURES
        SET_LOADCHARACTERS.IsChecked = My.Settings.CLIENT_LOADCHARACTERS
        SET_LOADCHARACTERSPICTURES.IsChecked = My.Settings.CLIENT_LOADCHARACTERSPICTURES
        SET_R18.IsChecked = My.Settings.APP_R18
        SET_AUDX.IsChecked = My.Settings.APP_AUDX
        SET_BACKGROUNDLOADING.IsChecked = My.Settings.APP_USEBACKGROUNDLOADING
        SET_OFFLINEBROWSING.IsChecked = My.Settings.CLIENT_OFFLINEBROWSING

        For Each path In My.Settings.LIBRARY_PATHS
            SET_LIBRARY.Items.Add(path)
        Next

        If Utils.CheckInternetConnection Then
            TopBar_UPDATES.IsEnabled = False
            Notify(AniResolver.UPDATESCHECKC)
            Dim Updates = Await AniServer.Updates.CheckForUpdates(True)
            If Updates IsNot Nothing Then
                If Updates.Count > 0 Then
                    Dim CUpd = Updates.FirstOrDefault(Function(k) k.Type = AniServer.Updates.Update.UpdateType.Required)
                    If CUpd Is Nothing Then
                        Utils.NotificationBucket.AddToBucket({New Utils.NotificationBucket.NotificationItem(AniResolver.APPNAME, AniResolver.UPDATES, HandyControl.Data.NotifyIconInfoType.Info)}, True)
                    Else
                        Utils.NotificationBucket.AddToBucket({New Utils.NotificationBucket.NotificationItem(AniResolver.APPNAME, AniResolver.UPDATESCRITICAL, HandyControl.Data.NotifyIconInfoType.Info)}, True)
                        Dim UPDBtn = New Button With {.Content = AniResolver.UPDATE_S}
                        If AniMessage.ShowMessage(AniResolver.APPNAME, AniResolver.UPDATESCRITICALWARNING, AniMessage.AniImage.WARNING, {New Button With {.Content = AniResolver.OK}, UPDBtn}, True, True) Is UPDBtn Then
                            Process.Start(AniServer.Updates.ReleaseLink)
                            Dim ExitBtn As New Button With {.Content = AniResolver.CLOSE}
                            If AniMessage.ShowMessage(AniResolver.APPNAME, AniResolver.CONTINUE, AniMessage.AniImage.WARNING, {New Button With {.Content = AniResolver.YES}, ExitBtn}, True, True) Is ExitBtn Then
                                Process.GetCurrentProcess.Kill()
                            End If
                        End If
                    End If
                End If
            End If
            TopBar_UPDATES.IsEnabled = True
            Else
                Notify(AniResolver.UPDATESERROR)
        End If

        Dim LS As Boolean() = {False, False, False, False, False, False, False, False, False, False}

#Disable Warning
        Home_Quote_MouseLeftButtonUp(Nothing, New MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left))
        Console.WriteLine(Utils.ConsoleInfoText & "Fetching Library Data")
        Task.Run(Async Function()
                     Console.WriteLine(Utils.ConsoleInfoText & "Library Watching Count: " & MainLibrary.Watching.Count)
                     For Each item In MainLibrary.Watching
                         Dim Anime = Await MainCache.GetAnime(item.ID)
                         If Anime IsNot Nothing Then
                             Dispatcher.BeginInvoke(Sub()
                                                        If Anime.Restricted AndAlso My.Settings.APP_R18 = False Then Exit Sub
                                                        Home_AnimeInProgress.Children.Add(New AnimeElement(Anime, item.EpisodeProgress) With {.Margin = New Thickness(10, 0, 0, 0)})
                                                        WatchingLVItems.Add(New LVItem(Anime.Title, item.Score, item.EpisodeProgress, Anime.Type.ToString) With {.Tag = Anime})
                                                    End Sub, System.Windows.Threading.DispatcherPriority.Render)
                         End If
                     Next
                     LS(0) = True
                 End Function)
        Task.Run(Async Function()
                     Console.WriteLine(Utils.ConsoleInfoText & "Library Completed Count: " & MainLibrary.Completed.Count)
                     For Each item In MainLibrary.Completed
                         Dim Anime = Await MainCache.GetAnime(item.ID, False)
                         If Anime IsNot Nothing Then
                             Dispatcher.BeginInvoke(Sub()
                                                        If Anime.Restricted AndAlso My.Settings.APP_R18 = False Then Exit Sub
                                                        CompletedLVItems.Add(New LVItem(Anime.Title, item.Score, item.EpisodeProgress, Anime.Type.ToString) With {.Tag = Anime})
                                                    End Sub, System.Windows.Threading.DispatcherPriority.DataBind)
                         End If
                     Next
                     LS(1) = True
                 End Function)
        Task.Run(Async Function()
                     Console.WriteLine(Utils.ConsoleInfoText & "Library Paused Count: " & MainLibrary.Paused.Count)
                     For Each item In MainLibrary.Paused
                         Dim Anime = Await MainCache.GetAnime(item.ID, False)
                         If Anime IsNot Nothing Then
                             Dispatcher.BeginInvoke(Sub()
                                                        If Anime.Restricted AndAlso My.Settings.APP_R18 = False Then Exit Sub
                                                        PausedLVItems.Add(New LVItem(Anime.Title, item.Score, item.EpisodeProgress, Anime.Type.ToString) With {.Tag = Anime})
                                                    End Sub, System.Windows.Threading.DispatcherPriority.DataBind)
                         End If
                     Next
                     LS(2) = True
                 End Function)
        Task.Run(Async Function()
                     Console.WriteLine(Utils.ConsoleInfoText & "Library Dropped Count: " & MainLibrary.Dropped.Count)
                     For Each item In MainLibrary.Dropped
                         Dim Anime = Await MainCache.GetAnime(item.ID, False)
                         If Anime IsNot Nothing Then
                             Dispatcher.BeginInvoke(Sub()
                                                        If Anime.Restricted AndAlso My.Settings.APP_R18 = False Then Exit Sub
                                                        DroppedLVItems.Add(New LVItem(Anime.Title, item.Score, item.EpisodeProgress, Anime.Type.ToString) With {.Tag = Anime})
                                                    End Sub, System.Windows.Threading.DispatcherPriority.DataBind)
                         End If
                     Next
                     LS(3) = True
                 End Function)
        Task.Run(Async Function()
                     Console.WriteLine(Utils.ConsoleInfoText & "Library Planning Count: " & MainLibrary.Planning.Count)
                     For Each item In MainLibrary.Planning
                         Dim Anime = Await MainCache.GetAnime(item.ID, False)
                         If Anime IsNot Nothing Then
                             Dispatcher.BeginInvoke(Sub()
                                                        If Anime.Restricted AndAlso My.Settings.APP_R18 = False Then Exit Sub
                                                        PlanningLVItems.Add(New LVItem(Anime.Title, item.Score, item.EpisodeProgress, Anime.Type.ToString) With {.Tag = Anime})
                                                    End Sub, System.Windows.Threading.DispatcherPriority.DataBind)
                         End If
                     Next
                     LS(4) = True
                 End Function)
        Task.Run(Async Function()
                     Console.WriteLine(Utils.ConsoleInfoText & "Library Activities Count: " & MainLibrary.Activities.Count)
                     For Each item In MainLibrary.Activities
                         Dispatcher.BeginInvoke(Sub()
                                                    Home_Activity.Children.Insert(0, New ActivityElement(item, Nothing, My.Settings.CLIENT_LOADANIMEPICTURES, Not My.Settings.CLIENT_LOADANIMEPICTURES))
                                                End Sub, System.Windows.Threading.DispatcherPriority.Render)
                     Next
                     LS(5) = True
                 End Function)
        LoadCacheElements()
        LS(6) = True
        Task.Run(Async Function()
                     Console.WriteLine(Utils.ConsoleInfoText & "Fetching Local Collection")
                     Dim i = 0
                     For Each path In My.Settings.LIBRARY_PATHS
                         Dim files = IO.Directory.GetDirectories(path, "*", IO.SearchOption.AllDirectories)
                         For Each file In files
                             i += 1
                             Dim info = New IO.DirectoryInfo(file)
                             Dim Xanime = Await MainClient.SearchByName(MainClient.SearchCache, info.Name)
                             If Xanime.Count > 0 Then
                                 Dim anime = Await MainCache.GetAnime(MainClient.IDFromSearch(Xanime(0)), False)
                                 If anime IsNot Nothing Then
                                     Dispatcher.BeginInvoke(Sub()
                                                                If anime.Restricted AndAlso My.Settings.APP_R18 = False Then Exit Sub
                                                                'Browse_Library.Children.Add(New AnimeElementX(anime) With {.Margin = New Thickness(10, 10, 0, 0)})                                                                
                                                                Dim CVI As New HandyControl.Controls.CoverViewItem
                                                                CVI.Header = New AnimeElement(anime, 0) With {.IncrementVisibility = Visibility.Collapsed, .EpProVisibility = Visibility.Collapsed, .RedirectToUpdateSearchTab = False}
                                                                Dim CVICW As New WrapPanel With {.Orientation = Orientation.Horizontal, .Margin = New Thickness(10)}
                                                                Dim CVIC As New ScrollViewer With {.VerticalScrollBarVisibility = ScrollBarVisibility.Auto, .Content = CVICW}
                                                                For Each ep In anime.Episodes
                                                                    CVICW.Children.Add(New EpisodeElementS(ep, anime.ID, True, file) With {.Width = 400, .Margin = New Thickness(10, 10, 0, 0)})
                                                                Next
                                                                CVI.Content = CVIC
                                                                Browse_Library.Items.Add(CVI)
                                                            End Sub, System.Windows.Threading.DispatcherPriority.Render)
                                 Else
                                     Dispatcher.BeginInvoke(Sub()
                                                                LoadingState = AniResolver.RETRIEVINGANIMEDATA & " (" & i & ")"
                                                            End Sub)
                                     anime = Await MainClient.Anime(MainClient.IDFromSearch(Xanime(0)), False, My.Settings.CLIENT_LOADCHARACTERS, My.Settings.CLIENT_LOADCHARACTERSPICTURES)
                                     If anime IsNot Nothing Then
                                         Dispatcher.BeginInvoke(Sub()
                                                                    If anime.Restricted AndAlso My.Settings.APP_R18 = False Then Exit Sub
                                                                    'Browse_Library.Children.Add(New AnimeElementX(anime) With {.Margin = New Thickness(10, 10, 0, 0)})
                                                                    Dim CVI As New HandyControl.Controls.CoverViewItem
                                                                    CVI.Header = New AnimeElement(anime, 0) With {.IncrementVisibility = Visibility.Collapsed, .EpProVisibility = Visibility.Collapsed, .RedirectToUpdateSearchTab = False}
                                                                    Dim CVICW As New WrapPanel With {.Orientation = Orientation.Horizontal, .Margin = New Thickness(10)}
                                                                    Dim CVIC As New ScrollViewer With {.VerticalScrollBarVisibility = ScrollBarVisibility.Auto, .Content = CVICW}
                                                                    For Each ep In anime.Episodes
                                                                        CVICW.Children.Add(New EpisodeElementS(ep, anime.ID, True, file) With {.Width = 400, .Margin = New Thickness(10, 10, 0, 0)})
                                                                    Next
                                                                    CVI.Content = CVIC
                                                                    Browse_Library.Items.Add(CVI)
                                                                End Sub, System.Windows.Threading.DispatcherPriority.Render)
                                     End If
                                 End If
                             End If
                         Next
                     Next
                     Console.WriteLine(Utils.ConsoleInfoText & "Local Collection Count: " & i)
                     LS(7) = True
                 End Function)
        Task.Run(Async Function()
                     If Utils.CheckInternetConnection Then
                         Console.WriteLine(Utils.ConsoleInfoText & "Fetching Data From Backend")
                         Dim Data = Await AniServer.Data.GetData
                         If Data IsNot Nothing Then
                             If Data.Featured IsNot Nothing Then
                                 For Each XAnime In Data.Featured
                                     If MainCache.CheckIfAnimeExists(XAnime) Then
                                         Dim Anime = Await MainCache.GetAnime(XAnime, False)
                                         Dispatcher.BeginInvoke(Sub()
                                                                    AniLife_Featured.Children.Add(New AnimeElementX(Anime, My.Settings.CLIENT_LOADANIMEPICTURES) With {.Margin = New Thickness(10, 10, 0, 0)})
                                                                End Sub, System.Windows.Threading.DispatcherPriority.Render)
                                     Else
                                         Dim Anime = Await MainClient.Anime(XAnime, My.Settings.CLIENT_LOADANIMEPICTURES, My.Settings.CLIENT_LOADCHARACTERS, My.Settings.CLIENT_LOADCHARACTERSPICTURES)
                                         If Anime IsNot Nothing Then
                                             MainCache.AddToCache(Anime)
                                             Dispatcher.BeginInvoke(Sub()
                                                                        AniLife_Featured.Children.Add(New AnimeElementX(Anime, My.Settings.CLIENT_LOADANIMEPICTURES) With {.Margin = New Thickness(10, 10, 0, 0)})
                                                                    End Sub, System.Windows.Threading.DispatcherPriority.Render)
                                         End If
                                     End If
                                 Next
                             End If
                             If Data.Waifu IsNot Nothing Then
                                 For Each waifu In Data.Waifu
                                     Await waifu.LoadPicture
                                     If MainCache.CheckIfAnimeExists(waifu.Target) Then
                                         Dim XAnime = MainCache.GetXAnime(waifu.Target)
                                         Dim Character = Await AniDB.AniDBClient.SharedFunctions.ParseExactCharacterFromAnimeAniDBXML(XAnime, waifu.ID, If(waifu.Picture Is Nothing, My.Settings.CLIENT_LOADCHARACTERSPICTURES, False))
                                         If waifu.Picture IsNot Nothing Then Character.Picture = waifu.Picture
                                         Dispatcher.BeginInvoke(Sub()
                                                                    AniLife_Waifu.Children.Add(New CharacterElement(Character, My.Settings.CLIENT_LOADCHARACTERSPICTURES) With {.Margin = New Thickness(10, 10, 0, 0)})
                                                                End Sub, System.Windows.Threading.DispatcherPriority.Render)
                                     Else
                                         Dim Anime = Await MainClient.Anime(waifu.Target, My.Settings.CLIENT_LOADCHARACTERS, True, If(waifu.Picture Is Nothing, My.Settings.CLIENT_LOADCHARACTERSPICTURES, False))
                                         If Anime IsNot Nothing Then
                                             MainCache.AddToCache(Anime)
                                             Dim Character = Await AniDB.AniDBClient.SharedFunctions.ParseExactCharacterFromAnimeAniDBXML(Anime.UnmanagedXMLData, waifu.ID, If(waifu.Picture Is Nothing, My.Settings.CLIENT_LOADCHARACTERSPICTURES, False))
                                             If waifu.Picture IsNot Nothing Then Character.Picture = waifu.Picture
                                             Dispatcher.BeginInvoke(Sub()
                                                                        AniLife_Waifu.Children.Add(New CharacterElement(Character, My.Settings.CLIENT_LOADCHARACTERSPICTURES) With {.Margin = New Thickness(10, 10, 0, 0)})
                                                                    End Sub, System.Windows.Threading.DispatcherPriority.Render)
                                         End If
                                     End If
                                 Next
                             End If
                             If Data.OST IsNot Nothing Then
                                 For Each OST In Data.OST
                                     Dispatcher.BeginInvoke(Sub()
                                                                AniLife_SoundTracks.Children.Add(New OSTElement(OST, My.Settings.CLIENT_LOADANIMEPICTURES) With {.Margin = New Thickness(10, 10, 0, 0)})
                                                            End Sub, System.Windows.Threading.DispatcherPriority.Render)
                                 Next
                             End If
                         End If
                     End If
                     LS(8) = True
                 End Function)
        Task.Run(Async Function()
                     If Utils.CheckInternetConnection Then
                         Console.WriteLine(Utils.ConsoleInfoText & "Fetching Main Anime Data")
                         Dim DailyMain = Await MainClient.Main(False, False, True, False)
                         Console.WriteLine(Utils.ConsoleInfoText & "Returned Main Anime Data Successfuly, Adding To Containers")
                         If DailyMain IsNot Nothing Then
                             If DailyMain.HotAnime IsNot Nothing Then
                                 For Each anime In DailyMain.HotAnime
                                     Dispatcher.BeginInvoke(Sub()
                                                                If anime.Restricted AndAlso My.Settings.APP_R18 = False Then Exit Sub
                                                                Browse_HotAnimes.Children.Add(New AnimeElementX(anime, My.Settings.CLIENT_LOADANIMEPICTURES) With {.Margin = New Thickness(10, 10, 0, 0)})
                                                            End Sub, System.Windows.Threading.DispatcherPriority.Render)
                                 Next
                             End If
                             If DailyMain.RandomRecommendation IsNot Nothing Then
                                 For Each anime In DailyMain.RandomRecommendation
                                     Dispatcher.BeginInvoke(Sub()
                                                                If anime.Restricted AndAlso My.Settings.APP_R18 = False Then Exit Sub
                                                                Browse_RandomRecommendations.Children.Add(New AnimeElementX(anime, My.Settings.CLIENT_LOADANIMEPICTURES) With {.Margin = New Thickness(10, 10, 0, 0)})
                                                            End Sub, System.Windows.Threading.DispatcherPriority.Render)
                                 Next
                             End If
                         End If
                     End If
                     LS(9) = True
                 End Function)
        Task.Run(Async Sub()
                     Do While Not Utils.BooleanSum(LS)
                         Await Task.Delay(500)
                     Loop
                     Console.WriteLine(Utils.ConsoleInfoText & "Finished Loading")
                     Dispatcher.Invoke(Sub()
                                           IsLoading = False
                                           If Utils.CheckInternetConnection Then
                                               TopBar_NONET.Visibility = Visibility.Collapsed
                                           End If
                                           Notify("Welcome Back!")
                                       End Sub)
                 End Sub)
#Enable Warning
    End Sub
    Public Async Sub UpdateSearchTab(AnimeElement As AniDB.AniDBClient.AnimeElement, Optional SelectTab As Boolean = True)     
        If AnimeElement IsNot Nothing Then
            With AnimeElement
                If TryCast(Search_TAB.Tag, AniDBClient.AnimeElement) IsNot Nothing Then
                    For Each character As CharacterElement In Search_Characters.Children
                        character.Dispose()
                    Next
                    If CType(Search_TAB.Tag, AniDBClient.AnimeElement).Picture IsNot Nothing Then
                        CType(Search_TAB.Tag, AniDBClient.AnimeElement).Picture.Dispose()
                    End If
                    CType(Search_TAB.Tag, AniDBClient.AnimeElement).Picture = Nothing
                End If
                IsLoading = True
                LoadingState = AniResolver.RETRIEVINGANIMEDATA
                If .Picture IsNot Nothing Then
                    Search_Cover.Source = Utils.ImageSourceFromBitmap(.Picture)
                Else
                    If My.Settings.CLIENT_LOADANIMEPICTURES Then
                        If MainCache.CheckIfAnimeExists(.ID) Then
                            Dim Pic = MainCache.GetPicture(.ID)
                            If Pic IsNot Nothing Then
                                Search_Cover.Source = Utils.ImageSourceFromBitmap(Pic)
                            Else
                                Pic = Await AniDB.AniDBClient.SharedFunctions.GetAniDBImageAsync(.PictureURL)
                                If Pic IsNot Nothing Then
                                    MainCache.AddPictureToCache(.ID, Pic)
                                    .Picture = Pic
                                    Search_Cover.Source = Utils.ImageSourceFromBitmap(Pic)
                                Else
                                    Search_Cover.Source = AniResolver.WARNING
                                End If
                            End If
                        Else
                            Dim pic = Await AniDB.AniDBClient.SharedFunctions.GetAniDBImageAsync(.PictureURL)
                            If pic IsNot Nothing Then
                                MainCache.AddPictureToCache(.ID, pic)
                                .Picture = pic
                                Search_Cover.Source = Utils.ImageSourceFromBitmap(pic)
                            Else
                                Search_Cover.Source = AniResolver.WARNING
                            End If
                        End If
                    Else
                        Search_Cover.Source = AniResolver.WARNING
                    End If
                    End If
                    Search_Title.Text = .Titles.FirstOrDefault(Function(k) k.Type.ToLower = "main")?.Value
                Search_Description.Text = .Description
                Search_Prequel.Text = If(.Prequel IsNot Nothing, "Prequel: " & .Prequel.Value, String.Empty)
                Search_Prequel.Tag = If(.Prequel IsNot Nothing, .Prequel.ID, Nothing)
                Search_Sequel.Text = If(.Sequel IsNot Nothing, "Sequel: " & .Sequel.Value, String.Empty)
                Search_Sequel.Tag = If(.Sequel IsNot Nothing, .Sequel.ID, Nothing)
                With .Ratings.FirstOrDefault(Function(k) k.Type = AniDB.AniDBClient.RatingElement.RatingType.Parmanent)
                    Try
                        Search_PermaRating.Text = .Value & Space(1) & AniResolver.BY & Space(1) & .Count & Space(1) & AniResolver.USER
                    Catch
                        Search_PermaRating.Text = AniResolver.NOTAVAILABLE
                    End Try
                End With
                With .Ratings.FirstOrDefault(Function(k) k.Type = AniDB.AniDBClient.RatingElement.RatingType.Temporary)
                    Try
                        Search_AvgRating.Text = .Value & Space(1) & AniResolver.BY & Space(1) & .Count & Space(1) & AniResolver.USER
                    Catch
                        Search_AvgRating.Text = AniResolver.NOTAVAILABLE
                    End Try
                End With
                Search_Format.Text = AniResolver.FORMAT & ": " & .Type.ToString
                Search_EpisodeCount.Text = AniResolver.EPISODECOUNT & ": " & .EpisodeCount
                Search_Status.Text = AniResolver.STATUS & ": " & If(.EndDate = Date.MinValue, AniResolver.AIRING, AniResolver.FINISHED)
                Search_StartDate.Text = AniResolver.STARTDATE & ": " & .StartDate.ToShortDateString
                Search_EndDate.Text = AniResolver.ENDDATE & ": " & .EndDate.ToShortDateString
                Search_SeasonDate.Text = AniResolver.SEASON & ": " & Utils.DateToSeason(.StartDate) & Space(1) & .StartDate.Year
                Search_Tags.Children.Clear()
                For Each animetag In .Tags
                    Search_Tags.Children.Add(New Button With {.Content = animetag.Name, .ToolTip = animetag.Description, .Margin = New Thickness(10, 10, 0, 0)})
                Next
                Search_Characters.Children.Clear()
                For Each character In .Characters
                    Dim characterE = New CharacterElement(character, False) With {.Margin = New Thickness(10, 10, 0, 0)}
                    characterE.LoadCharacterImages()
                    Search_Characters.Children.Add(characterE)
                Next
                CreatorsLVItems.Clear()
                For Each creator In .Creators
                    CreatorsLVItems.Add(New LVCreatorItem(creator.Value, creator.Type) With {.Tag = creator})
                Next
                TitlesLVItems.Clear()
                For Each AnimeTitle In .Titles
                    TitlesLVItems.Add(New LVTitleItem(AnimeTitle.Value, AnimeTitle.Type, AnimeTitle.Lang) With {.Tag = AnimeTitle})
                Next
                RelatedLVItems.Clear()
                For Each related In .RelatedAnimes
                    RelatedLVItems.Add(New LVRelatedItem(related.Value, related.Type.ToString) With {.Tag = related})
                Next
                SimilarLVItems.Clear()
                For Each similar In .SimilarAnimes
                    SimilarLVItems.Add(New LVSimilarItem(similar.Value, similar.Approval.ToString) With {.Tag = similar})
                Next
                Search_Episodes.Children.Clear()
                For Each ep In .Episodes
                    Search_Episodes.Children.Add(New EpisodeElement(ep, .ID) With {.Width = Search_Episodes.ActualWidth, .Margin = New Thickness(0, 0, 0, 10)})
                Next
                Search_Episodes_Unrelated.Children.Clear()
                For Each ep In .UnRelatedEpisodes
                    Search_Episodes_Unrelated.Children.Add(New EpisodeElement(ep, .ID) With {.Width = Search_Episodes.ActualWidth, .Margin = New Thickness(0, 0, 0, 10)})
                Next
                Search_Resources.Children.Clear()
                For Each res In .Resources
                    For Each url In res.URL.URLs
                        Dim ResIG As New Controls.Image With {.Height = 32, .Stretch = Stretch.Uniform, .Source = Utils.ResourceTypeToInternalImage(res.Type), .ToolTip = res.Type.ToString, .Margin = New Thickness(0, 0, 10, 0)}
                        AddHandler ResIG.MouseLeftButtonDown, Sub(sender As Object, e As MouseButtonEventArgs)
                                                                  If e.ClickCount >= 2 Then
                                                                      Process.Start(url)
                                                                  End If
                                                              End Sub
                        Search_Resources.Children.Add(ResIG)
                    Next
                Next
            End With
            If MainLibrary.CheckIfExists(AnimeElement.ID) Then
                Search_SetAs_Watching.Visibility = Visibility.Collapsed
                Search_SetAs_Completed.Visibility = Visibility.Collapsed
                Search_SetAs_Planning.Visibility = Visibility.Collapsed
                Search_SetAs_Paused.Visibility = Visibility.Collapsed
                Search_SetAs_Dropped.Visibility = Visibility.Collapsed
                Search_SetAs_Edit.Visibility = Visibility.Visible
            Else
                Search_SetAs_Watching.Visibility = Visibility.Visible
                Search_SetAs_Completed.Visibility = Visibility.Visible
                Search_SetAs_Planning.Visibility = Visibility.Visible
                Search_SetAs_Paused.Visibility = Visibility.Visible
                Search_SetAs_Dropped.Visibility = Visibility.Visible
                Search_SetAs_Edit.Visibility = Visibility.Collapsed
            End If
            Search_TAB.Tag = AnimeElement
            If SelectTab Then MainTabControl.SelectedIndex = 5
            IsLoading = False
        End If
    End Sub

    Private Async Sub Search_Prequel_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles Search_Prequel.MouseLeftButtonUp
        If Search_Prequel.Tag IsNot Nothing Then
            If MainCache.CheckIfAnimeExists(Search_Prequel.Tag) Then
                IsLoading = True
                LoadingState = AniResolver.RETRIEVINGANIMEDATA
                Dim Anime = Await MainCache.GetAnime(Search_Prequel.Tag)
                UpdateSearchTab(Anime)
                IsLoading = False
            Else
                IsLoading = True
                LoadingState = AniResolver.RETRIEVINGANIMEDATA
                Dim Anime = Await MainClient.Anime(Search_Prequel.Tag, My.Settings.CLIENT_LOADANIMEPICTURES, My.Settings.CLIENT_LOADCHARACTERS, My.Settings.CLIENT_LOADCHARACTERSPICTURES)
                UpdateSearchTab(Anime)
                IsLoading = False
            End If
        End If
    End Sub

    Private Async Sub Search_Sequel_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles Search_Sequel.MouseLeftButtonUp
        If Search_Sequel.Tag IsNot Nothing Then
            If MainCache.CheckIfAnimeExists(Search_Sequel.Tag) Then
                IsLoading = True
                LoadingState = AniResolver.RETRIEVINGANIMEDATA
                Dim Anime = Await MainCache.GetAnime(Search_Sequel.Tag)
                UpdateSearchTab(Anime)
                IsLoading = False
            Else
                IsLoading = True
                LoadingState = AniResolver.RETRIEVINGANIMEDATA
                Dim Anime = Await MainClient.Anime(Search_Sequel.Tag, My.Settings.CLIENT_LOADANIMEPICTURES, My.Settings.CLIENT_LOADCHARACTERS, My.Settings.CLIENT_LOADCHARACTERSPICTURES)
                UpdateSearchTab(Anime)
                IsLoading = False
            End If
        End If
    End Sub

    Public Sub OnNotificationArrived(e As Utils.NotificationBucket.NotificationArrivedEventArgs)
        If TopBar_MAIL_ToolTip_WP.Children.Count >= 10 Then
            TopBar_MAIL_ToolTip_WP.Children.RemoveAt(TopBar_MAIL_ToolTip_WP.Children.Count - 1)
        End If
        Notify(e.Notification.Msg)
        TopBar_MAIL_ToolTip_WP.Children.Insert(0, New NotificationElement(e.Notification.Msg) With {.VerticalAlignment = VerticalAlignment.Top, .Margin = New Thickness(0, 0, 10, 0)})
    End Sub
    Private Async Sub TopBar_Search_Click(sender As Object, e As RoutedEventArgs) Handles TopBar_Search.Click
        If MainClient.SearchCache IsNot Nothing Then
            If TopBar_Search_Data.Width = 0 Then
                TopBar_Search_Data.BeginAnimation(WidthProperty, New Animation.DoubleAnimation(250, New Duration(TimeSpan.FromMilliseconds(250))) With {.AccelerationRatio = 0.9})
            Else
                If String.IsNullOrEmpty(TopBar_Search_Data.Text) Then
                    TopBar_Search_Data.BeginAnimation(MarginProperty, New Animation.ThicknessAnimation(New Thickness(0, 0, 68, 0), New Duration(TimeSpan.FromMilliseconds(250))) With {.AutoReverse = True, .EasingFunction = New Animation.BounceEase With {.Bounces = 3, .Bounciness = 0.5}})
                Else
                    Await Task.Run(Async Function()
                                       Await Dispatcher.InvokeAsync(Sub()
                                                                        TopBar_Search_Data.BeginAnimation(WidthProperty, New Animation.DoubleAnimation(0, New Duration(TimeSpan.FromMilliseconds(250))) With {.AccelerationRatio = 0.9})
                                                                        LoadingState = AniResolver.SEARCHING
                                                                        IsLoading = True
                                                                    End Sub)
                                       Dim SearchResult As New List(Of XElement)
                                       Dim SearchVal As String
                                       Await Dispatcher.InvokeAsync(Sub()
                                                                        SearchVal = TopBar_Search_Data.Text
                                                                    End Sub)
                                       If System.Text.RegularExpressions.Regex.IsMatch(SearchVal, "^[0-9 ]+$") Then
                                           SearchResult.Add(Await MainClient.SearchExactByID(MainClient.SearchCache, SearchVal))
                                       Else
                                           SearchResult = Await MainClient.SearchByName(MainClient.SearchCache, SearchVal)
                                       End If
                                       Await Dispatcher.InvokeAsync(Sub()
                                                                        LoadingState = AniResolver.FOUND & Space(1) & SearchResult.Count & Space(1) & AniResolver.RETRIEVINGANIMEDATA
                                                                        For Each child In SearchItems_Items.Children
                                                                            If TypeOf child Is IDisposable Then
                                                                                TryCast(child, IDisposable)?.Dispose()
                                                                            End If
                                                                        Next
                                                                        SearchItems_Items.Children.Clear()
                                                                    End Sub)
                                       Dim i = 1
                                       For Each result In SearchResult
                                           Await Dispatcher.InvokeAsync(Sub()
                                                                            LoadingState = AniResolver.FOUND & Space(1) & SearchResult.Count & Space(1) & AniResolver.RETRIEVINGANIMEDATA & " (" & i & ")"
                                                                        End Sub)
                                           If result IsNot Nothing Then
                                               Dim ID = MainClient.IDFromSearch(result)
                                               If MainCache.CheckIfAnimeExists(ID) Then
                                                   Dim Anime = Await MainCache.GetAnime(ID, False)
                                                   If Anime IsNot Nothing Then
                                                       Await Dispatcher.InvokeAsync(Sub()
                                                                                        If Anime.Restricted Then
                                                                                            If My.Settings.APP_R18 Then
                                                                                                SearchItems_Items.Children.Add(New AnimeElementX(Anime) With {.Margin = New Thickness(10, 10, 0, 0)})
                                                                                            End If
                                                                                        Else
                                                                                            SearchItems_Items.Children.Add(New AnimeElementX(Anime) With {.Margin = New Thickness(10, 10, 0, 0)})
                                                                                        End If
                                                                                    End Sub)
                                                   End If
                                               Else
                                                   Dim Anime = Await MainClient.Anime(ID, My.Settings.CLIENT_LOADANIMEPICTURES, My.Settings.CLIENT_LOADCHARACTERS, My.Settings.CLIENT_LOADCHARACTERSPICTURES)
                                                   If Anime IsNot Nothing Then
                                                       Await Dispatcher.InvokeAsync(Sub()
                                                                                        If Anime.Restricted Then
                                                                                            If My.Settings.APP_R18 Then
                                                                                                LoadingState = AniResolver.CACHING
                                                                                                MainCache.AddToCache(Anime)
                                                                                                SearchItems_Items.Children.Add(New AnimeElementX(Anime) With {.Margin = New Thickness(10, 10, 0, 0)})
                                                                                            End If
                                                                                        Else
                                                                                            LoadingState = AniResolver.CACHING
                                                                                            MainCache.AddToCache(Anime)
                                                                                            SearchItems_Items.Children.Add(New AnimeElementX(Anime) With {.Margin = New Thickness(10, 10, 0, 0)})
                                                                                        End If
                                                                                    End Sub)
                                                   End If
                                               End If
                                           End If
                                           i += 1
                                       Next
                                   End Function)
                End If
                MainTabControl.SelectedIndex = 6
                IsLoading = False
            End If
        Else
            Dim OkBtn As New Button With {.Content = AniResolver.OK}
            Dim LoadBtn As New Button With {.Content = AniResolver.LOADFROMFILE}
            Dim GetBtn As New Button With {.Content = AniResolver.DOWNLOADMANUALLY}
            Dim result = AniMessage.ShowMessage(AniResolver.APPNAME, AniResolver.SEARCHMISSING & Environment.NewLine & AniResolver.SEARCHMISSINGNOTE, AniMessage.AniImage.WARNING, {OkBtn, LoadBtn, GetBtn}, True, True)
            If result Is LoadBtn Then
                Dim OFD As New Forms.OpenFileDialog With {.CheckFileExists = True, .CheckPathExists = True, .Filter = "XML File|*.xml"}
                If OFD.ShowDialog <> Forms.DialogResult.Cancel Then
                    LoadingState = AniResolver.LOADING
                    IsLoading = True
                    If Await MainClient.LoadSearchCache(OFD.FileName) IsNot Nothing Then
                        Await MainClient.SaveSearchCache(MainClient.SearchCache, IO.Path.Combine(My.Settings.DATA_LOCATION, "Cache", "SearchCache.xml"))
                    Else
                        AniMessage.ShowMessage(AniResolver.APPNAME, AniResolver.SEARCHNOTRECOGNIZED, AniMessage.AniImage.WARNING, {New Button With {.Content = AniResolver.OK}}, True, True)
                    End If
                    IsLoading = False
                End If
            ElseIf result Is GetBtn Then
                LoadingState = AniResolver.DOWNLOADING
                IsLoading = True
                Dim Cache = Await MainClient.GetSearchData
                If MainClient.LoadSearchCacheFromDocument(Cache) Then
                    Await MainClient.SaveSearchCache(Cache, IO.Path.Combine(My.Settings.DATA_LOCATION, "Cache", "SearchCache.xml"))
                Else
                    AniMessage.ShowMessage(AniResolver.APPNAME, AniResolver.SEARCHNOTREADING, AniMessage.AniImage.WARNING, {New Button With {.Content = AniResolver.OK}}, True, True)
                End If
                IsLoading = False
            End If
        End If
    End Sub

    Private Sub Collection_Watching_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Collection_Watching.SelectionChanged
        If Collection_Watching.SelectedIndex <> -1 Then
            UpdateSearchTab(WatchingLVItems.Item(Collection_Watching.SelectedIndex).Tag)
        End If
    End Sub

    Private Sub Collection_Completed_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Collection_Completed.SelectionChanged
        If Collection_Completed.SelectedIndex <> -1 Then
            UpdateSearchTab(CompletedLVItems.Item(Collection_Completed.SelectedIndex).Tag)
        End If
    End Sub

    Private Sub Collection_Paused_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Collection_Paused.SelectionChanged
        If Collection_Paused.SelectedIndex <> -1 Then
            UpdateSearchTab(PausedLVItems.Item(Collection_Paused.SelectedIndex).Tag)
        End If
    End Sub

    Private Sub Collection_Dropped_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Collection_Dropped.SelectionChanged
        If Collection_Dropped.SelectedIndex <> -1 Then
            UpdateSearchTab(DroppedLVItems.Item(Collection_Dropped.SelectedIndex).Tag)
        End If
    End Sub

    Private Sub Collection_Planning_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Collection_Planning.SelectionChanged
        If Collection_Planning.SelectedIndex <> -1 Then
            UpdateSearchTab(PlanningLVItems.Item(Collection_Planning.SelectedIndex).Tag)
        End If
    End Sub

    Private Sub Search_SetAs_Watching_Click(sender As Object, e As RoutedEventArgs) Handles Search_SetAs_Watching.Click
        With CType(Search_TAB.Tag, AniDB.AniDBClient.AnimeElement)
            MainLibrary.AddToCollection(New AniLibrary.AnimeElement(.ID, 0, 0, Nothing, AniLibrary.Status.Watching))
        End With
        MainCache.AddToCache(CType(Search_TAB.Tag, AniDB.AniDBClient.AnimeElement))
    End Sub

    Private Sub Search_SetAs_Completed_Click(sender As Object, e As RoutedEventArgs) Handles Search_SetAs_Completed.Click
        With CType(Search_TAB.Tag, AniDB.AniDBClient.AnimeElement)
            MainLibrary.AddToCollection(New AniLibrary.AnimeElement(.ID, 0, 0, Nothing, AniLibrary.Status.Completed))
        End With
        MainCache.AddToCache(CType(Search_TAB.Tag, AniDB.AniDBClient.AnimeElement))
    End Sub

    Private Sub Search_SetAs_Planning_Click(sender As Object, e As RoutedEventArgs) Handles Search_SetAs_Planning.Click
        With CType(Search_TAB.Tag, AniDB.AniDBClient.AnimeElement)
            MainLibrary.AddToCollection(New AniLibrary.AnimeElement(.ID, 0, 0, Nothing, AniLibrary.Status.Planning))
        End With
        MainCache.AddToCache(CType(Search_TAB.Tag, AniDB.AniDBClient.AnimeElement))
    End Sub

    Private Sub Search_SetAs_Edit_Click(sender As Object, e As RoutedEventArgs) Handles Search_SetAs_Edit.Click
        IsOverlay = True
        Dim EEditor As New EntryEditor(Me, Search_TAB.Tag, MainLibrary.GetByID(CType(Search_TAB.Tag, AniDB.AniDBClient.AnimeElement).ID))
        If EEditor.ShowDialog Then
            If EEditor.OriginalResult.Status = EEditor.Result.Status AndAlso EEditor.OriginalResult.Score = EEditor.Result.Score AndAlso EEditor.OriginalResult.Note = EEditor.Result.Note AndAlso EEditor.OriginalResult.EpisodeProgress <> EEditor.Result.EpisodeProgress Then
                MainLibrary.UpdateItemProgress(EEditor.Result.ID, EEditor.Result.EpisodeProgress, False)
            Else
                MainLibrary.UpdateItem(EEditor.OriginalResult, EEditor.Result)
            End If
        End If
        IsOverlay = False
    End Sub

    Private Sub Search_SetAs_Dropped_Click(sender As Object, e As RoutedEventArgs) Handles Search_SetAs_Dropped.Click
        With CType(Search_TAB.Tag, AniDB.AniDBClient.AnimeElement)
            MainLibrary.AddToCollection(New AniLibrary.AnimeElement(.ID, 0, 0, Nothing, AniLibrary.Status.Dropped))
        End With
        MainCache.AddToCache(CType(Search_TAB.Tag, AniDB.AniDBClient.AnimeElement))
    End Sub

    Private Sub Search_SetAs_Paused_Click(sender As Object, e As RoutedEventArgs) Handles Search_SetAs_Paused.Click
        With CType(Search_TAB.Tag, AniDB.AniDBClient.AnimeElement)
            MainLibrary.AddToCollection(New AniLibrary.AnimeElement(.ID, 0, 0, Nothing, AniLibrary.Status.Paused))
        End With
        MainCache.AddToCache(CType(Search_TAB.Tag, AniDB.AniDBClient.AnimeElement))
    End Sub

    Private Sub TopBar_Search_Data_KeyUp(sender As Object, e As KeyEventArgs) Handles TopBar_Search_Data.KeyUp
        If e.Key = Key.Enter Then
            TopBar_Search_Click(Nothing, New RoutedEventArgs)
        End If
    End Sub

    Private Sub MainClient_OnError(Code As Integer, Value As String) Handles MainClient.OnError
        Dispatcher.BeginInvoke(Sub()
                                   AniMessage.ShowMessage(AniResolver.APPNAME, AniResolver.ERROR & ": " & Code & Space(1) & AniResolver.WITHVALUE & ": " & Value, AniMessage.AniImage.WARNING, {New Button With {.Content = AniResolver.OK}}, True, True)
                               End Sub)
    End Sub

    Private Sub MainClient_OnImageDownloaded(ID As Integer, Image As Image) Handles MainClient.OnImageDownloaded
        MainCache.AddPictureToCache(ID, Image)
    End Sub

    Private Sub MainClient_OnAnimeElementDownloaded(ID As Integer, Data As AniDBClient.AnimeElement) Handles MainClient.OnAnimeElementDownloaded
        MainCache.AddToCache(Data)
    End Sub

    Private Sub MainClient_OnNetworkError() Handles MainClient.OnNetworkError
        Dispatcher.BeginInvoke(Sub()
                                   TopBar_NONET.Visibility = Visibility.Visible
                               End Sub)
    End Sub

    Public Sub MainClient_OnClientValidityChanged(Value As Boolean) Handles MainClient.OnClientValidityChanged
        If Value Then
            TopBar_WARN.Visibility = Visibility.Collapsed
        Else
            TopBar_WARN.Visibility = Visibility.Visible
            Notify(AniResolver.INVALIDCLIENT)
        End If
    End Sub

    Private Async Sub MainLibrary_OnItemAdded(Item As AniLibrary.AnimeElement) Handles MainLibrary.OnItemAdded
        Dim Anime = Await MainCache.GetAnime(Item.ID)
        If Anime IsNot Nothing Then
            Select Case Item.Status
                Case AniLibrary.Status.Watching
                    WatchingLVItems.Add(New LVItem(Anime.Titles.FirstOrDefault(Function(k) k.Type.ToLower = "main")?.Value, Item.Score, Item.EpisodeProgress, Anime.Type.ToString) With {.Tag = Anime})
                    Home_AnimeInProgress.Children.Add(New AnimeElement(Anime, Item.EpisodeProgress) With {.Margin = New Thickness(10, 0, 0, 0)})
                Case AniLibrary.Status.Completed
                    CompletedLVItems.Add(New LVItem(Anime.Titles.FirstOrDefault(Function(k) k.Type.ToLower = "main")?.Value, Item.Score, Item.EpisodeProgress, Anime.Type.ToString) With {.Tag = Anime})
                Case AniLibrary.Status.Paused
                    PausedLVItems.Add(New LVItem(Anime.Titles.FirstOrDefault(Function(k) k.Type.ToLower = "main")?.Value, Item.Score, Item.EpisodeProgress, Anime.Type.ToString) With {.Tag = Anime})
                Case AniLibrary.Status.Dropped
                    DroppedLVItems.Add(New LVItem(Anime.Titles.FirstOrDefault(Function(k) k.Type.ToLower = "main")?.Value, Item.Score, Item.EpisodeProgress, Anime.Type.ToString) With {.Tag = Anime})
                Case AniLibrary.Status.Planning
                    PlanningLVItems.Add(New LVItem(Anime.Titles.FirstOrDefault(Function(k) k.Type.ToLower = "main")?.Value, Item.Score, Item.EpisodeProgress, Anime.Type.ToString) With {.Tag = Anime})
            End Select
        End If
    End Sub

    Private Sub MainLibrary_OnItemRemoved(Item As AniLibrary.AnimeElement) Handles MainLibrary.OnItemRemoved
        Select Case Item.Status
            Case AniLibrary.Status.Watching
                WatchingLVItems.Remove(WatchingLVItems.FirstOrDefault(Function(k) CType(k.Tag, AniDBClient.AnimeElement).ID = Item.ID))
                Dim TBRAnime As AnimeElement = Nothing
                For Each anime As AnimeElement In Home_AnimeInProgress.Children
                    If anime.Tag.ID = Item.ID Then
                        TBRAnime = anime
                        Exit For
                    End If
                Next
                If TBRAnime IsNot Nothing Then
                    Home_AnimeInProgress.Children.Remove(TBRAnime)
                End If
            Case AniLibrary.Status.Completed
                CompletedLVItems.Remove(WatchingLVItems.FirstOrDefault(Function(k) CType(k.Tag, AniDBClient.AnimeElement).ID = Item.ID))
            Case AniLibrary.Status.Paused
                PausedLVItems.Remove(WatchingLVItems.FirstOrDefault(Function(k) CType(k.Tag, AniDBClient.AnimeElement).ID = Item.ID))
            Case AniLibrary.Status.Dropped
                DroppedLVItems.Remove(WatchingLVItems.FirstOrDefault(Function(k) CType(k.Tag, AniDBClient.AnimeElement).ID = Item.ID))
            Case AniLibrary.Status.Planning
                PlanningLVItems.Remove(WatchingLVItems.FirstOrDefault(Function(k) CType(k.Tag, AniDBClient.AnimeElement).ID = Item.ID))
        End Select
    End Sub

    Private Async Sub MainLibrary_OnItemMoved(Item As AniLibrary.AnimeElement, OldStatus As AniLibrary.Status) Handles MainLibrary.OnItemMoved
        Select Case OldStatus
            Case AniLibrary.Status.Watching
                Dim anime = WatchingLVItems.FirstOrDefault(Function(k) CType(k.Tag, AniDBClient.AnimeElement).ID = Item.ID)
                If anime IsNot Nothing Then
                    WatchingLVItems.Remove(anime)
                    Dim TBRAnime As AnimeElement = Nothing
                    For Each _anime As AnimeElement In Home_AnimeInProgress.Children
                        If _anime.Tag.ID = Item.ID Then
                            TBRAnime = _anime
                            Exit For
                        End If
                    Next
                    If TBRAnime IsNot Nothing Then
                        Home_AnimeInProgress.Children.Remove(TBRAnime)
                    End If
                    Select Case Item.Status
                        Case AniLibrary.Status.Watching
                            WatchingLVItems.Add(anime)
                            Home_AnimeInProgress.Children.Add(New AnimeElement(Await MainCache.GetAnime(Item.ID, My.Settings.CLIENT_LOADANIMEPICTURES, 4, My.Settings.CLIENT_LOADCHARACTERS, My.Settings.CLIENT_LOADCHARACTERSPICTURES), Item.EpisodeProgress))
                        Case AniLibrary.Status.Completed
                            CompletedLVItems.Add(anime)
                        Case AniLibrary.Status.Paused
                            PausedLVItems.Add(anime)
                        Case AniLibrary.Status.Dropped
                            DroppedLVItems.Add(anime)
                        Case AniLibrary.Status.Planning
                            PlanningLVItems.Add(anime)
                    End Select
                End If
            Case AniLibrary.Status.Completed
                Dim anime = CompletedLVItems.FirstOrDefault(Function(k) CType(k.Tag, AniDBClient.AnimeElement).ID = Item.ID)
                If anime IsNot Nothing Then
                    CompletedLVItems.Remove(anime)
                    Select Case Item.Status
                        Case AniLibrary.Status.Watching
                            WatchingLVItems.Add(anime)
                        Case AniLibrary.Status.Completed
                            CompletedLVItems.Add(anime)
                        Case AniLibrary.Status.Paused
                            PausedLVItems.Add(anime)
                        Case AniLibrary.Status.Dropped
                            DroppedLVItems.Add(anime)
                        Case AniLibrary.Status.Planning
                            PlanningLVItems.Add(anime)
                    End Select
                End If
            Case AniLibrary.Status.Paused
                Dim anime = PausedLVItems.FirstOrDefault(Function(k) CType(k.Tag, AniDBClient.AnimeElement).ID = Item.ID)
                If anime IsNot Nothing Then
                    PausedLVItems.Remove(anime)
                    Select Case Item.Status
                        Case AniLibrary.Status.Watching
                            WatchingLVItems.Add(anime)
                        Case AniLibrary.Status.Completed
                            CompletedLVItems.Add(anime)
                        Case AniLibrary.Status.Paused
                            PausedLVItems.Add(anime)
                        Case AniLibrary.Status.Dropped
                            DroppedLVItems.Add(anime)
                        Case AniLibrary.Status.Planning
                            PlanningLVItems.Add(anime)
                    End Select
                End If
            Case AniLibrary.Status.Dropped
                Dim anime = DroppedLVItems.FirstOrDefault(Function(k) CType(k.Tag, AniDBClient.AnimeElement).ID = Item.ID)
                If anime IsNot Nothing Then
                    DroppedLVItems.Remove(anime)
                    Select Case Item.Status
                        Case AniLibrary.Status.Watching
                            WatchingLVItems.Add(anime)
                        Case AniLibrary.Status.Completed
                            CompletedLVItems.Add(anime)
                        Case AniLibrary.Status.Paused
                            PausedLVItems.Add(anime)
                        Case AniLibrary.Status.Dropped
                            DroppedLVItems.Add(anime)
                        Case AniLibrary.Status.Planning
                            PlanningLVItems.Add(anime)
                    End Select
                End If
            Case AniLibrary.Status.Planning
                Dim anime = PlanningLVItems.FirstOrDefault(Function(k) CType(k.Tag, AniDBClient.AnimeElement).ID = Item.ID)
                If anime IsNot Nothing Then
                    PlanningLVItems.Remove(anime)
                    Select Case Item.Status
                        Case AniLibrary.Status.Watching
                            WatchingLVItems.Add(anime)
                        Case AniLibrary.Status.Completed
                            CompletedLVItems.Add(anime)
                        Case AniLibrary.Status.Paused
                            PausedLVItems.Add(anime)
                        Case AniLibrary.Status.Dropped
                            DroppedLVItems.Add(anime)
                        Case AniLibrary.Status.Planning
                            PlanningLVItems.Add(anime)
                    End Select
                End If
        End Select
    End Sub

    Private Async Sub MainLibrary_OnItemUpdated(Item As AniLibrary.AnimeElement, NewItem As AniLibrary.AnimeElement) Handles MainLibrary.OnItemUpdated
        If NewItem.Status = AniLibrary.Status.Watching Then
            If MainCache.CheckIfAnimeExists(NewItem.ID) Then
                Dim anime = Await MainCache.GetAnime(Item.ID)
                If anime IsNot Nothing Then
                    Home_AnimeInProgress.Children.Add(New AnimeElement(anime, NewItem.EpisodeProgress))
                End If
            Else
                IsLoading = True
                LoadingState = AniResolver.RETRIEVINGANIMEDATA
                Dim anime = Await MainClient.Anime(Item.ID, My.Settings.CLIENT_LOADANIMEPICTURES, My.Settings.CLIENT_LOADCHARACTERS, My.Settings.CLIENT_LOADCHARACTERSPICTURES)
                If anime IsNot Nothing Then
                    Home_AnimeInProgress.Children.Add(New AnimeElement(anime, NewItem.EpisodeProgress))
                End If
                IsLoading = False
            End If
        End If
    End Sub
    Private Sub MainLibrary_OnItemProgressUpdated(Item As AniLibrary.AnimeElement, OldProgress As Integer) Handles MainLibrary.OnItemProgressUpdated
        Dim anime = WatchingLVItems.FirstOrDefault(Function(k) CType(k.Tag, AniDBClient.AnimeElement).ID = Item.ID)
        If anime IsNot Nothing Then
            anime.Progress += Item.EpisodeProgress
        End If
        Dim TBM As New List(Of AniLibrary.AnimeElement)
        For Each element As AnimeElement In Home_AnimeInProgress.Children
            If element.Tag.ID = Item.ID Then
                If Item.EpisodeProgress = element.Tag.EpisodeCount Then
                    TBM.Add(Item)
                Else
                    element.EpProgress = Item.EpisodeProgress
                End If
            End If
        Next
        For Each elem In TBM
            MainLibrary.MoveFromCollection(elem, AniLibrary.Status.Completed)
        Next
        TBM.Clear()
        TBM = Nothing
    End Sub
    Private Sub MainLibrary_OnActivityAdded(Item As AniLibrary.LibraryActivityElement, <Runtime.CompilerServices.CallerMemberName> ByVal Optional propertyName As String = Nothing, <Runtime.CompilerServices.CallerLineNumber> ByVal Optional propertyline As String = Nothing) Handles MainLibrary.OnActivityAdded
        Home_Activity.Children.Insert(0, New ActivityElement(Item, Nothing, My.Settings.CLIENT_LOADANIMEPICTURES, Not My.Settings.CLIENT_LOADANIMEPICTURES))
    End Sub

    Dim MainTabControlSelectedIndex = 0
    Private Sub MainTabControl_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles MainTabControl.SelectionChanged
        If My.Settings.APP_OPTIMIZEMEM Then
            Dim NeedsCollect As Boolean = False 'to collect when neccesary
            Select Case MainTabControlSelectedIndex
                Case 0
                Case 1
                Case 2
                    For Each animeelement As AnimeElementX In Browse_OfflineBrowsing.Children
                        animeelement.Dispose()
                    Next
                    NeedsCollect = True
                Case 3
                Case 4
                    For Each characterelement As CharacterElement In Search_Characters.Children
                        characterelement.Dispose()
                    Next
                    NeedsCollect = True
                Case 5
                    For Each animeelement As AnimeElementX In SearchItems_Items.Children
                        animeelement.Dispose()
                    Next
                    NeedsCollect = True
            End Select
            If NeedsCollect Then GC.Collect()
            Select Case MainTabControl.SelectedIndex
                Case 0
                Case 1
                Case 2
                    For Each animeelement As AnimeElementX In Browse_OfflineBrowsing.Children
                        animeelement.LoadAnimeImages(Not My.Settings.CLIENT_LOADANIMEPICTURES)
                    Next
                Case 3
                Case 4
                    For Each characterelement As CharacterElement In Search_Characters.Children
                        characterelement.LoadCharacterImages(Not My.Settings.CLIENT_LOADCHARACTERSPICTURES)
                    Next
                Case 5
                    For Each animeelement As AnimeElementX In SearchItems_Items.Children
                        animeelement.LoadAnimeImages(Not My.Settings.CLIENT_LOADANIMEPICTURES)
                    Next
            End Select
            MainTabControlSelectedIndex = MainTabControl.SelectedIndex
        End If
    End Sub

    Private Async Sub SET_CLEARCACHE_Checked(sender As Object, e As RoutedEventArgs) Handles SET_CLEARCACHE.Checked
        SET_CLEARCACHE.IsEnabled = False
        Dim animefiles = IO.Directory.GetFiles(IO.Path.Combine(MainCache.CacheLocation, "Animes"), "*.xml")
        Dim thumbfiles = IO.Directory.GetFiles(IO.Path.Combine(MainCache.CacheLocation, "Thumbs"), "*.png")
        Dim YesBtn As New Button With {.Content = "Yes"}
        If AniMessage.ShowMessage(AniResolver.APPNAME, AniResolver.DELETEWARNING & ":" & Environment.NewLine & animefiles.Length & Space(1) & AniResolver.ANIMEFILES & Environment.NewLine & thumbfiles.Length & Space(1) & AniResolver.THUMBFILES, AniMessage.AniImage.WARNING, {YesBtn, New Button With {.Content = AniResolver.NO}}, True, True) Is YesBtn Then
            IsLoading = True
            LoadingState = AniResolver.DELETING & Space(1) & AniResolver.ANIMEFILES
            Dim i = 1
            For Each file In animefiles
                LoadingState = AniResolver.DELETING & Space(1) & AniResolver.ANIMEFILES & " (" & i & ")"
                IO.File.Delete(file)
                i += 1
            Next
            LoadingState = AniResolver.DELETING & Space(1) & AniResolver.THUMBFILES
            For Each file In thumbfiles
                LoadingState = AniResolver.DELETING & Space(1) & AniResolver.THUMBFILES & " (" & i & ")"
                IO.File.Delete(file)
                i += 1
            Next
            IsLoading = False
        End If
        About_CacheSize.Text = AniResolver.CALCULATING & "..."
        Await Task.Run(Async Function()
                           Dim Files = IO.Directory.GetFiles(MainCache.CacheLocation, "*.*", IO.SearchOption.AllDirectories)
                           Dim TotalSize As Long
                           For Each file In Files
                               Dim FI As New IO.FileInfo(file)
                               TotalSize += FI.Length
                               FI = Nothing
                           Next
                           Dispatcher.BeginInvoke(Sub()
                                                      About_CacheSize.Text = AniResolver.CACHESIZE & ": " & Utils.FileSizeConverterSTR(TotalSize)
                                                  End Sub)
                       End Function)
        SET_CLEARCACHE.IsEnabled = True
        SET_CLEARCACHE.IsChecked = False
    End Sub

    Private Async Sub SET_CLEARIMAGECACHE_Checked(sender As Object, e As RoutedEventArgs) Handles SET_CLEARIMAGECACHE.Checked
        SET_CLEARIMAGECACHE.IsEnabled = False
        Dim thumbfiles = IO.Directory.GetFiles(IO.Path.Combine(MainCache.CacheLocation, "Thumbs"), "*.png")
        Dim YesBtn As New Button With {.Content = "Yes"}
        If AniMessage.ShowMessage("AniLife", AniResolver.DELETEWARNING & ":" & Environment.NewLine & thumbfiles.Length & Space(1) & AniResolver.THUMBFILES, AniMessage.AniImage.WARNING, {YesBtn, New Button With {.Content = AniResolver.NO}}, True, True) Is YesBtn Then
            IsLoading = True
            LoadingState = AniResolver.DELETING & Space(1) & AniResolver.THUMBFILES
            Dim i = 0
            For Each file In thumbfiles
                LoadingState = AniResolver.DELETING & Space(1) & AniResolver.THUMBFILES & " (" & i & ")"
                IO.File.Delete(file)
                i += 1
            Next
            IsLoading = False
        End If
        About_CacheSize.Text = AniResolver.CALCULATING & "..."
        Await Task.Run(Async Function()
                           Dim Files = IO.Directory.GetFiles(MainCache.CacheLocation, "*.*", IO.SearchOption.AllDirectories)
                           Dim TotalSize As Long
                           For Each file In Files
                               Dim FI As New IO.FileInfo(file)
                               TotalSize += FI.Length
                               FI = Nothing
                           Next
                           Dispatcher.BeginInvoke(Sub()
                                                      About_CacheSize.Text = AniResolver.CACHESIZE & ": " & Utils.FileSizeConverterSTR(TotalSize)
                                                  End Sub)
                       End Function)
        SET_CLEARIMAGECACHE.IsEnabled = True
        SET_CLEARIMAGECACHE.IsChecked = False
    End Sub

    Private Sub SET_DATALOCATION_Checked(sender As Object, e As RoutedEventArgs) Handles SET_DATALOCATION.Checked
        SET_DATALOCATION.IsEnabled = False
        Dim fbd As New Forms.FolderBrowserDialog
        If fbd.ShowDialog <> Forms.DialogResult.Cancel Then
            Dim YesBtn As New Button With {.Content = AniResolver.YES}
            If AniMessage.ShowMessage(AniResolver.APPNAME, AniResolver.DATAWARNING & ":" & Environment.NewLine & AniResolver.FROM & ": " & My.Settings.DATA_LOCATION & Environment.NewLine & AniResolver.TO & ": " & fbd.SelectedPath, AniMessage.AniImage.WARNING, {YesBtn, New Button With {.Content = AniResolver.NO}}, True, True) Is YesBtn Then
                IsLoading = True
                LoadingState = AniResolver.MOVEWARNING
                Try
                    FileIO.FileSystem.CopyDirectory(My.Settings.DATA_LOCATION, IO.Path.Combine(fbd.SelectedPath, "AniLife"), FileIO.UIOption.AllDialogs, FileIO.UICancelOption.ThrowException)
                    FileIO.FileSystem.DeleteDirectory(My.Settings.DATA_LOCATION, FileIO.DeleteDirectoryOption.DeleteAllContents)
                    My.Settings.DATA_LOCATION = IO.Path.Combine(fbd.SelectedPath, "AniLife")
                    My.Settings.Save()
                    MainCache.CacheLocation = IO.Path.Combine(My.Settings.DATA_LOCATION, "Cache")
                    SET_DATALOCATION.IsEnabled = True
                    SET_DATALOCATION.IsChecked = False
                Catch
                End Try
                IsLoading = False
            End If
        End If
        SET_DATALOCATION.IsEnabled = True
        SET_DATALOCATION.IsChecked = False
    End Sub

    Private Async Sub SET_DISTINCTCACHE_Checked(sender As Object, e As RoutedEventArgs) Handles SET_DISTINCTCACHE.Checked
        SET_DISTINCTCACHE.IsEnabled = False
        IsLoading = True
        LoadingState = AniResolver.DELETING
        Await MainCache.DistinctCache
        IsLoading = False
        About_CacheSize.Text = AniResolver.CALCULATING & "..."
        Await Task.Run(Async Function()
                           Dim Files = IO.Directory.GetFiles(MainCache.CacheLocation, "*.*", IO.SearchOption.AllDirectories)
                           Dim TotalSize As Long
                           For Each file In Files
                               Dim FI As New IO.FileInfo(file)
                               TotalSize += FI.Length
                               FI = Nothing
                           Next
                           Dispatcher.BeginInvoke(Sub()
                                                      About_CacheSize.Text = AniResolver.CACHESIZE & ": " & Utils.FileSizeConverterSTR(TotalSize)
                                                  End Sub)
                       End Function)
        SET_DISTINCTCACHE.IsEnabled = True
        SET_DISTINCTCACHE.IsChecked = False
    End Sub

    Private Sub SET_LOADANIMEPICTURES_Checked(sender As Object, e As RoutedEventArgs) Handles SET_LOADANIMEPICTURES.Checked
        My.Settings.CLIENT_LOADANIMEPICTURES = True
        My.Settings.Save()
    End Sub

    Private Sub SET_LOADANIMEPICTURES_Unchecked(sender As Object, e As RoutedEventArgs) Handles SET_LOADANIMEPICTURES.Unchecked
        My.Settings.CLIENT_LOADANIMEPICTURES = False
        My.Settings.Save()
    End Sub

    Private Sub SET_LOADCHARACTERS_Checked(sender As Object, e As RoutedEventArgs) Handles SET_LOADCHARACTERS.Checked
        My.Settings.CLIENT_LOADCHARACTERS = True
        My.Settings.Save()
    End Sub

    Private Sub SET_LOADCHARACTERS_Unchecked(sender As Object, e As RoutedEventArgs) Handles SET_LOADCHARACTERS.Unchecked
        My.Settings.CLIENT_LOADCHARACTERS = False
        My.Settings.Save()
    End Sub

    Private Sub SET_LOADCHARACTERSPICTURES_Checked(sender As Object, e As RoutedEventArgs) Handles SET_LOADCHARACTERSPICTURES.Checked
        My.Settings.CLIENT_LOADCHARACTERSPICTURES = True
        My.Settings.Save()
    End Sub

    Private Sub SET_LOADCHARACTERSPICTURES_Unchecked(sender As Object, e As RoutedEventArgs) Handles SET_LOADCHARACTERSPICTURES.Unchecked
        My.Settings.CLIENT_LOADCHARACTERSPICTURES = False
        My.Settings.Save()
    End Sub

    Private Sub SET_R18_Checked(sender As Object, e As RoutedEventArgs) Handles SET_R18.Checked
        My.Settings.APP_R18 = True
        My.Settings.Save()
    End Sub

    Private Sub SET_R18_Unchecked(sender As Object, e As RoutedEventArgs) Handles SET_R18.Unchecked
        My.Settings.APP_R18 = False
        My.Settings.Save()
    End Sub

    Private Sub SET_AUDX_Checked(sender As Object, e As RoutedEventArgs) Handles SET_AUDX.Checked
        If SET_AUDX.IsMouseOver Then
            Dim YesBtn As New Button With {.Content = AniResolver.YES}
            Dim NoBtn As New Button With {.Content = AniResolver.NO}
            If AniMessage.ShowMessage("AniLife", AniResolver.AUDXWARNING, AniMessage.AniImage.WARNING, {YesBtn, NoBtn}, True, True) Is YesBtn Then
                If String.IsNullOrEmpty(My.Settings.AUDX_YOUTUBEDL) Or My.Computer.Keyboard.ShiftKeyDown = True Then
                    Dim OFD As New Forms.OpenFileDialog With {.CheckFileExists = True, .Filter = "Executables|*.exe"}
                    If OFD.ShowDialog <> Forms.DialogResult.Cancel Then
                        My.Settings.AUDX_YOUTUBEDL = OFD.FileName
                    Else
                        Exit Sub
                    End If
                End If
                My.Settings.APP_AUDX = True
                My.Settings.Save()
            Else
                SET_AUDX.IsChecked = False
            End If
        End If
    End Sub

    Private Sub SET_AUDX_Unchecked(sender As Object, e As RoutedEventArgs) Handles SET_AUDX.Unchecked
        My.Settings.APP_AUDX = False
        My.Settings.Save()
    End Sub

    Private Sub SET_RAMOPTIMIZATION_Checked(sender As Object, e As RoutedEventArgs) Handles SET_RAMOPTIMIZATION.Checked
        My.Settings.APP_OPTIMIZEMEM = True
        My.Settings.Save()
    End Sub

    Private Sub SET_RAMOPTIMIZATION_Unchecked(sender As Object, e As RoutedEventArgs) Handles SET_RAMOPTIMIZATION.Unchecked
        My.Settings.APP_OPTIMIZEMEM = False
        My.Settings.Save()
        IsLoading = True
        LoadingState = AniResolver.FETCHING

        For Each animeelement As AnimeElementX In Browse_OfflineBrowsing.Children
            animeelement.LoadAnimeImages(Not My.Settings.CLIENT_LOADANIMEPICTURES)
        Next

        For Each characterelement As CharacterElement In Search_Characters.Children
            characterelement.LoadCharacterImages(Not My.Settings.CLIENT_LOADCHARACTERSPICTURES)
        Next
        For Each animeelement As AnimeElementX In SearchItems_Items.Children
            animeelement.LoadAnimeImages(Not My.Settings.CLIENT_LOADANIMEPICTURES)
        Next

        IsLoading = False
    End Sub

    Private Async Sub SET_UPDATECACHE_Checked(sender As Object, e As RoutedEventArgs) Handles SET_UPDATECACHE.Checked
        SET_UPDATECACHE.IsEnabled = False
        IsLoading = True
        LoadingState = AniResolver.UPDATING
        Dim IDList = Await MainCache.GetIDs
        LoadingState = AniResolver.FOUND & Space(1) & IDList.Count & Space(1) & AniResolver.ITEMS & "," & AniResolver.UPDATING
        MainCache.ClearCache()
        Dim i = 1
        Await Task.Run(Async Function()
                           For Each id In IDList
                               Await Dispatcher.InvokeAsync(Sub()
                                                                LoadingState = AniResolver.RETRIEVINGANIMEDATA & Space(1) & AniResolver.FOR & Space(1) & i & "(" & id & ")"
                                                            End Sub)
                               Dim Anime = Await MainClient.Anime(id, My.Settings.CLIENT_LOADANIMEPICTURES, My.Settings.CLIENT_LOADCHARACTERS, My.Settings.CLIENT_LOADCHARACTERSPICTURES)
                               If Anime IsNot Nothing Then
                                   MainCache.AddToCache(Anime)
                               End If
                               i += 1
                           Next
                       End Function)
        About_CacheSize.Text = AniResolver.CALCULATING & "..."
        Await Task.Run(Async Function()
                           Dim Files = IO.Directory.GetFiles(MainCache.CacheLocation, "*.*", IO.SearchOption.AllDirectories)
                           Dim TotalSize As Long
                           For Each file In Files
                               Dim FI As New IO.FileInfo(file)
                               TotalSize += FI.Length
                               FI = Nothing
                           Next
                           Dispatcher.BeginInvoke(Sub()
                                                      About_CacheSize.Text = AniResolver.CACHESIZE & ": " & Utils.FileSizeConverterSTR(TotalSize)
                                                  End Sub)
                       End Function)
        SET_UPDATECACHE.IsEnabled = True
        SET_UPDATECACHE.IsChecked = False
    End Sub

    Private Sub MainWindow_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        MainCache.Save()
        MainLibrary.Save()
        Player.StreamStop()
        Player.Dispose()
    End Sub

    Private Sub SET_THEME_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles SET_THEME.SelectionChanged
        If SET_THEME.SelectedIndex <> -1 Then
            If SET_THEME.SelectedIndex = SET_THEME.Items.Count - 1 Then
                If SET_THEME.IsMouseOver Then
                    Dim THM = ThemeMaker.ShowMaker(Me)
                    If THM IsNot Nothing Then
                        IsLoading = True
                        LoadingState = "Saving Theme"
                        My.Settings.APP_THEME_BG = THM.Background
                        My.Settings.APP_THEME_TOPBAR = THM.TopBar
                        My.Settings.APP_THEME_TOPBARTEXT = THM.TopBarText
                        My.Settings.APP_THEME_CONTENT = THM.Content
                        My.Settings.APP_THEME_ACCENT = THM.Accent
                        My.Settings.APP_THEME_OVERLAY = THM.Overlay
                        My.Settings.APP_THEME_OVERLAYTEXT = THM.OverlayText
                        My.Settings.APP_THEME_TEXT = THM.Text
                        My.Settings.APP_THEME_FONTFAMILY = THM.FontFamily
                        My.Settings.APP_THEME_FONTWEIGHT = THM.FontWeight
                        My.Settings.APP_ISCUSTOMTHEME = True
                        My.Settings.APP_ISEXTERNALTHEME = False
                        My.Settings.Save()
                        With AniResolver.StyleDictionary
                            .Item("BG") = My.Settings.APP_THEME_BG
                            .Item("TOPBAR") = My.Settings.APP_THEME_TOPBAR
                            .Item("TOPBARTEXT") = My.Settings.APP_THEME_TOPBARTEXT
                            .Item("CONTENT") = My.Settings.APP_THEME_CONTENT
                            .Item("ACCENT") = My.Settings.APP_THEME_ACCENT
                            .Item("OVERLAY") = My.Settings.APP_THEME_OVERLAY
                            .Item("OVERLAYTEXT") = My.Settings.APP_THEME_OVERLAYTEXT
                            .Item("TEXT") = My.Settings.APP_THEME_TEXT
                            .Item("FONTFAMILY") = My.Settings.APP_THEME_FONTFAMILY
                            .Item("FONTWEIGHT") = My.Settings.APP_THEME_FONTWEIGHT
                        End With
                        HandyControl.Themes.ThemeManager.Current.AccentColor = AniResolver.ACCENT
                        IsLoading = False
                        AniMessage.ShowMessage(AniResolver.APPNAME, AniResolver.THEMEWARNING, AniMessage.AniImage.WARNING, {New Button With {.Content = AniResolver.OK}}, True, True)
                    End If
                End If
                Exit Sub
            ElseIf SET_THEME.SelectedIndex = SET_THEME.Items.Count - 2 Then
                Dim OFD As New Forms.OpenFileDialog With {.CheckFileExists = True, .Filter = "Resource File|*.xaml"}
                If OFD.ShowDialog <> Forms.DialogResult.Cancel Then
                    My.Settings.APP_ISCUSTOMTHEME = False
                    My.Settings.APP_ISEXTERNALTHEME = True
                    My.Settings.APP_EXTERNALTHEMEPATH = OFD.FileName
                    My.Settings.Save()
                    AniResolver.ClearStyleDictionaries()
                    Application.Current.Resources.MergedDictionaries.Add(New ResourceDictionary() With {.Source = New Uri(My.Settings.APP_EXTERNALTHEMEPATH, UriKind.Absolute)})
                End If
            Else
                AniResolver.ClearStyleDictionaries()
                Application.Current.Resources.MergedDictionaries.Add(New ResourceDictionary() With {.Source = New Uri("Colors/" & CType(SET_THEME.SelectedItem, ComboBoxItem).Tag.ToString & ".xaml", UriKind.Relative)})
                Try
                    If AniResolver.BG_ISPATH Then
                        If AniResolver.BG_ISPATHRELATIVE Then
                            Background = New ImageBrush(New BitmapImage(New Uri(AniResolver.BG_PATH, UriKind.Relative)))
                        ElseIf AniResolver.BG_ISPATHINTERNAL Then
                            Background = New ImageBrush(New BitmapImage(New Uri("pack://application:,,,/AniLife;component/" & AniResolver.BG_PATH)))
                        Else
                            Background = New ImageBrush(New BitmapImage(New Uri(AniResolver.BG_PATH, UriKind.Absolute)))
                        End If
                    Else
                        Background = AniResolver.BG
                    End If
                Catch ex As Exception
                    Console.WriteLine(Utils.ConsoleDebugText & ex.ToString)
                End Try
                With HandyControl.Themes.ThemeManager.Current
                    .ApplicationTheme = AniResolver.APPTHEME
                    .AccentColor = AniResolver.ACCENT
                End With
                My.Settings.APP_ISCUSTOMTHEME = False
                My.Settings.APP_ISEXTERNALTHEME = False
                My.Settings.APP_THEME = CType(SET_THEME.SelectedItem, ComboBoxItem).Tag.ToString
                My.Settings.Save()
            End If
        End If
    End Sub

    Private Sub SET_LANGUAGE_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles SET_LANGUAGE.SelectionChanged
        If SET_LANGUAGE.SelectedIndex <> -1 Then
            If SET_LANGUAGE.SelectedIndex = SET_LANGUAGE.Items.Count - 1 Then
                Dim OFD As New Forms.OpenFileDialog With {.CheckFileExists = True, .Filter = "Resource File|*.xaml"}
                If OFD.ShowDialog <> Forms.DialogResult.Cancel Then
                    My.Settings.APP_ISEXTERNALLANGUAGE = True
                    My.Settings.APP_EXTERNALLANGUAGEPATH = OFD.FileName
                    My.Settings.Save()
                    AniResolver.ClearLocalizationDictionaries()
                    Application.Current.Resources.MergedDictionaries.Add(New ResourceDictionary() With {.Source = New Uri(My.Settings.APP_EXTERNALLANGUAGEPATH, UriKind.Absolute)})
                End If
            Else
                AniResolver.ClearLocalizationDictionaries()
                Application.Current.Resources.MergedDictionaries.Add(New ResourceDictionary() With {.Source = New Uri("Languages/" & CType(SET_LANGUAGE.SelectedItem, ComboBoxItem).Tag.ToString & ".xaml", UriKind.Relative)})
                My.Settings.APP_ISEXTERNALLANGUAGE = False
                My.Settings.APP_LANGUAGE = CType(SET_LANGUAGE.SelectedItem, ComboBoxItem).Tag.ToString
                My.Settings.Save()
            End If
        End If
    End Sub

    Private Async Sub Search_Related_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Search_Related.SelectionChanged
        If Search_Related.SelectedIndex <> -1 Then
            Dim ReAnimeElement = TryCast(RelatedLVItems.Item(Search_Related.SelectedIndex).Tag, AniDB.AniDBClient.RelatedAnimeElement)
            If ReAnimeElement IsNot Nothing Then
                IsLoading = True
                LoadingState = AniResolver.RETRIEVINGANIMEDATA & Space(1) & AniResolver.FOR & Space(1) & ReAnimeElement.Value & "(" & ReAnimeElement.ID & ")"
                Dim Anime As AniDB.AniDBClient.AnimeElement
                If MainCache.CheckIfAnimeExists(ReAnimeElement.ID) Then
                    Anime = Await MainCache.GetAnime(ReAnimeElement.ID)
                Else
                    Anime = Await MainClient.Anime(ReAnimeElement.ID, My.Settings.CLIENT_LOADANIMEPICTURES, My.Settings.CLIENT_LOADCHARACTERS, My.Settings.CLIENT_LOADCHARACTERSPICTURES)
                End If
                IsLoading = False
                If Anime IsNot Nothing Then
                    UpdateSearchTab(Anime)
                End If
            End If
        End If
    End Sub

    Private Async Sub Search_Similar_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles Search_Similar.SelectionChanged
        If Search_Related.SelectedIndex <> -1 Then
            Dim ReAnimeElement = TryCast(SimilarLVItems.Item(Search_Related.SelectedIndex).Tag, AniDB.AniDBClient.SimilarAnimeElement)
            If ReAnimeElement IsNot Nothing Then
                IsLoading = True
                LoadingState = AniResolver.RETRIEVINGANIMEDATA & Space(1) & AniResolver.FOR & Space(1) & ReAnimeElement.Value & "(" & ReAnimeElement.ID & ")"
                Dim Anime As AniDB.AniDBClient.AnimeElement
                If MainCache.CheckIfAnimeExists(ReAnimeElement.ID) Then
                    Anime = Await MainCache.GetAnime(ReAnimeElement.ID)
                Else
                    Anime = Await MainClient.Anime(ReAnimeElement.ID, My.Settings.CLIENT_LOADANIMEPICTURES, My.Settings.CLIENT_LOADCHARACTERS, My.Settings.CLIENT_LOADCHARACTERSPICTURES)
                End If
                IsLoading = False
                If Anime IsNot Nothing Then
                    UpdateSearchTab(Anime)
                End If
            End If
        End If
    End Sub

    Private Sub Collection_Show_All_Checked(sender As Object, e As RoutedEventArgs) Handles Collection_Show_All.Checked
        Collection_Watching.Visibility = Visibility.Visible
        Collection_Completed.Visibility = Visibility.Visible
        Collection_Paused.Visibility = Visibility.Visible
        Collection_Dropped.Visibility = Visibility.Visible
        Collection_Planning.Visibility = Visibility.Visible

        Collection_Show_Watching.IsChecked = False
        Collection_Show_Completed.IsChecked = False
        Collection_Show_Paused.IsChecked = False
        Collection_Show_Dropped.IsChecked = False
        Collection_Show_Planning.IsChecked = False
    End Sub

    Private Sub Collection_Show_Watching_Checked(sender As Object, e As RoutedEventArgs) Handles Collection_Show_Watching.Checked
        Collection_Watching.Visibility = Visibility.Visible
        Collection_Completed.Visibility = Visibility.Collapsed
        Collection_Paused.Visibility = Visibility.Collapsed
        Collection_Dropped.Visibility = Visibility.Collapsed
        Collection_Planning.Visibility = Visibility.Collapsed

        Collection_Show_All.IsChecked = False
        Collection_Show_Completed.IsChecked = False
        Collection_Show_Paused.IsChecked = False
        Collection_Show_Dropped.IsChecked = False
        Collection_Show_Planning.IsChecked = False
    End Sub

    Private Sub Collection_Show_Completed_Checked(sender As Object, e As RoutedEventArgs) Handles Collection_Show_Completed.Checked
        Collection_Watching.Visibility = Visibility.Collapsed
        Collection_Completed.Visibility = Visibility.Visible
        Collection_Paused.Visibility = Visibility.Collapsed
        Collection_Dropped.Visibility = Visibility.Collapsed
        Collection_Planning.Visibility = Visibility.Collapsed

        Collection_Show_All.IsChecked = False
        Collection_Show_Watching.IsChecked = False
        Collection_Show_Paused.IsChecked = False
        Collection_Show_Dropped.IsChecked = False
        Collection_Show_Planning.IsChecked = False
    End Sub

    Private Sub Collection_Show_Paused_Checked(sender As Object, e As RoutedEventArgs) Handles Collection_Show_Paused.Checked
        Collection_Watching.Visibility = Visibility.Collapsed
        Collection_Completed.Visibility = Visibility.Collapsed
        Collection_Paused.Visibility = Visibility.Visible
        Collection_Dropped.Visibility = Visibility.Collapsed
        Collection_Planning.Visibility = Visibility.Collapsed

        Collection_Show_All.IsChecked = False
        Collection_Show_Watching.IsChecked = False
        Collection_Show_Completed.IsChecked = False
        Collection_Show_Dropped.IsChecked = False
        Collection_Show_Planning.IsChecked = False
    End Sub

    Private Sub Collection_Show_Dropped_Checked(sender As Object, e As RoutedEventArgs) Handles Collection_Show_Dropped.Checked
        Collection_Watching.Visibility = Visibility.Collapsed
        Collection_Completed.Visibility = Visibility.Collapsed
        Collection_Paused.Visibility = Visibility.Collapsed
        Collection_Dropped.Visibility = Visibility.Visible
        Collection_Planning.Visibility = Visibility.Collapsed

        Collection_Show_All.IsChecked = False
        Collection_Show_Watching.IsChecked = False
        Collection_Show_Completed.IsChecked = False
        Collection_Show_Paused.IsChecked = False
        Collection_Show_Planning.IsChecked = False
    End Sub

    Private Sub Collection_Show_Planning_Checked(sender As Object, e As RoutedEventArgs) Handles Collection_Show_Planning.Checked
        Collection_Watching.Visibility = Visibility.Collapsed
        Collection_Completed.Visibility = Visibility.Collapsed
        Collection_Paused.Visibility = Visibility.Collapsed
        Collection_Dropped.Visibility = Visibility.Collapsed
        Collection_Planning.Visibility = Visibility.Visible

        Collection_Show_All.IsChecked = False
        Collection_Show_Watching.IsChecked = False
        Collection_Show_Completed.IsChecked = False
        Collection_Show_Paused.IsChecked = False
        Collection_Show_Dropped.IsChecked = False
    End Sub

    Private Sub TopBar_Apps_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles TopBar_Apps.MouseLeftButtonUp
        If TopBar_Apps_Container.Width = 0 Then
            TopBar_Apps_Container.BeginAnimation(WidthProperty, New Animation.DoubleAnimation(155, New Duration(TimeSpan.FromMilliseconds(250))))
        Else
            TopBar_Apps_Container.BeginAnimation(WidthProperty, New Animation.DoubleAnimation(0, New Duration(TimeSpan.FromMilliseconds(250))))
        End If
    End Sub

    Private Sub TopBar_Apps_AUDX_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles TopBar_Apps_AUDX.MouseLeftButtonUp
        If TopBar_Apps_AUDX_Controls.Visibility = Visibility.Visible Then
            TopBar_Apps_AUDX_Controls.Visibility = Visibility.Collapsed
        Else
            TopBar_Apps_AUDX_Controls.Visibility = Visibility.Visible
        End If
    End Sub

    Private Sub TopBar_Apps_ImageViewer_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles TopBar_Apps_ImageViewer.MouseLeftButtonUp
        Dim OFD As New Forms.OpenFileDialog With {.CheckFileExists = True, .Filter = "Image Files(*.BMP;*.JPG;*.GIF;*.PNG;*.JPEG)|*.BMP;*.JPG;*.GIF;*.PNG;*.JPEG|All Files(*.*)|*.*"}
        If OFD.ShowDialog <> Forms.DialogResult.Cancel Then
            Dim IgViewer As New ImageViewer(OFD.FileName) With {.Owner = Me}
            IgViewer.Show()
        End If
    End Sub
    Private Sub TopBar_Apps_VideoViewer_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles TopBar_Apps_VideoViewer.MouseLeftButtonUp
        MainTabControl.SelectedIndex = 7
    End Sub
    Private Sub Search_SeeIn_ImageViewer_Click(sender As Object, e As RoutedEventArgs) Handles Search_SeeIn_ImageViewer.Click
        Dim pic = MainCache.GetPicture(CType(Search_TAB.Tag, AniDB.AniDBClient.AnimeElement).ID, 1)
        If pic IsNot Nothing Then
            Dim IgViewer As New ImageViewer(Utils.ImageSourceFromBitmap(pic)) With {.Owner = Me}
            IgViewer.Show()
        Else
            Dim IgViewer As New ImageViewer(Search_Cover.Source) With {.Owner = Me}
            IgViewer.Show()
        End If
    End Sub

    Private Async Sub Search_SeeIn_AUDX_OP_Click(sender As Object, e As RoutedEventArgs) Handles Search_SeeIn_AUDX_OP.Click
        Dim anime = TryCast(Search_TAB.Tag, AniDB.AniDBClient.AnimeElement)
        If anime IsNot Nothing Then
            IsLoading = True
            LoadingState = "Searching For Opening"
            Dim DURL = Await Task.Run(Async Function()
                                          Dim YTDL As New YoutubeDL(My.Settings.AUDX_YOUTUBEDL)
                                          Dim YTID = Await YTDL.SearchVideo(anime.Title & " Opening 1")
                                          Dim YTVID = Await YTDL.GetVideo(YTID, My.Settings.AUDX_REQUESTQUALITY)
                                          Return YTVID.DirectURL
                                      End Function)
            LoadSong(DURL, True)
            IsLoading = False
        End If
    End Sub

    Private Async Sub Search_SeeIn_AUDX_ED_Click(sender As Object, e As RoutedEventArgs) Handles Search_SeeIn_AUDX_ED.Click
        Dim anime = TryCast(Search_TAB.Tag, AniDB.AniDBClient.AnimeElement)
        If anime IsNot Nothing Then
            IsLoading = True
            LoadingState = "Searching For Ending"
            Dim DURL = Await Task.Run(Async Function()
                                          Dim YTDL As New YoutubeDL(My.Settings.AUDX_YOUTUBEDL)
                                          Dim YTID = Await YTDL.SearchVideo(anime.Title & " Ending 1")
                                          Dim YTVID = Await YTDL.GetVideo(YTID, My.Settings.AUDX_REQUESTQUALITY)
                                          Return YTVID.DirectURL
                                      End Function)
            LoadSong(DURL, True)
            IsLoading = False
        End If
    End Sub

    Private Async Sub TopBar_NONET_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles TopBar_NONET.MouseLeftButtonUp
        TopBar.IsEnabled = False
        TopBar_NONET.BeginAnimation(OpacityProperty, New Animation.DoubleAnimation(0, New Duration(TimeSpan.FromMilliseconds(150))) With {.AutoReverse = True, .RepeatBehavior = Animation.RepeatBehavior.Forever})
        Dim result = Await Task.Run(Function()
                                        Try
                                            Using client = New Net.WebClient()
                                                Using stream = client.OpenRead("http://www.google.com")
                                                    Return True
                                                End Using
                                            End Using
                                        Catch
                                            Return False
                                        End Try
                                    End Function)
        TopBar_NONET.BeginAnimation(OpacityProperty, New Animation.DoubleAnimation(1, New Duration(TimeSpan.FromMilliseconds(10))))
        If result Then TopBar_NONET.Visibility = Visibility.Collapsed
        TopBar.IsEnabled = True
    End Sub

#Region "AUDX"
    Public WithEvents Player As New Player(Me, AddressOf MediaEnded) With {.AutoPlay = True}
    Public WithEvents UIManager As New Forms.Timer With {.Interval = 500}

    Public Sub LoadSong(File As String, Optional IsURL As Boolean = False)
        If IsURL Then
            Player.LoadSong(Nothing, True, True, File)
        Else
            Player.LoadSong(File)
        End If
    End Sub

    Private Sub AUDX_Activated(sender As Object, e As EventArgs) Handles Me.Activated
        UIManager.Start()
    End Sub

    Private Sub AUDX_Deactivated(sender As Object, e As EventArgs) Handles Me.Deactivated
        UIManager.Start()
    End Sub

    Private Sub Media_PlayPause_Click(sender As Object, e As RoutedEventArgs) Handles Media_Play.Click, Media_Pause.Click
        If My.Computer.Keyboard.ShiftKeyDown Then
            Dim OFD As New Forms.OpenFileDialog With {.CheckFileExists = True}
            If OFD.ShowDialog <> Forms.DialogResult.Cancel Then
                Player.LoadSong(OFD.FileName)
            End If
        Else
            Player.StreamToggle()
        End If
    End Sub

    Private Sub Media_Track_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles Media_Track.ValueChanged
        If Media_Track.IsMouseOver = True Then
            Player.SetPosition(Media_Track.Value)
        End If
    End Sub
    Private Sub Media_Volume_OnValueChanged(value As Integer) Handles Media_Volume.OnValueChanged
        Player.SetVolume(value / 100)
    End Sub
    Private Sub MediaEnded()
        UIManager.Stop()
    End Sub

    Private Sub Player_MediaLoaded(Title As String, Artist As String, Cover As Interop.InteropBitmap, Thumb As Interop.InteropBitmap, LyricsAvailable As Boolean, Lyrics As String) Handles Player.MediaLoaded
        Media_Track.Maximum = Player.GetLength
        Media_Volume.Value = Player.Volume * 100
        UIManager.Start()
    End Sub

    Private Sub Player_OnMediaError(ErrorCode As BASSError) Handles Player.OnMediaError
        Utils.NotificationBucket.AddToBucket({New Utils.NotificationBucket.NotificationItem(AniResolver.APPNAME, AniResolver.ERROR & Space(1) & ErrorCode.ToString, HandyControl.Data.NotifyIconInfoType.Error)})
    End Sub

    Private Sub UIManager_Tick(sender As Object, e As EventArgs) Handles UIManager.Tick
        If Media_Track.IsMouseOver = False Then
            Media_Track.Value = Player.GetPosition
        End If
        Media_Track.ToolTip = Utils.SecsToMins(Player.GetPosition) & "/" & Utils.SecsToMins(Media_Track.Maximum)
    End Sub

    Private Sub Player_PlayerStateChanged(State As Player.State) Handles Player.PlayerStateChanged
        Select Case State
            Case Player.State.Playing
                Media_Play.Visibility = Visibility.Collapsed
                Media_Pause.Visibility = Visibility.Visible
            Case Else
                Media_Pause.Visibility = Visibility.Collapsed
                Media_Play.Visibility = Visibility.Visible
        End Select
    End Sub
#End Region

    Private Async Sub MainWindow_KeyUp(sender As Object, e As KeyEventArgs) Handles Me.KeyUp
        If IsFullScreenMode AndAlso e.Key = Key.Escape Then
            ToggleFullscreen()
        End If
        If My.Computer.Keyboard.CtrlKeyDown AndAlso e.Key = Key.C Then
            My.Windows.DeveloperConsole.Show()
        End If
    End Sub

    Private Async Sub SET_CLIENT_Checked(sender As Object, e As RoutedEventArgs) Handles SET_CLIENT.Checked
        Dim Client = InputBox(AniResolver.CLIENT, AniResolver.APPNAME)
        Dim Version = InputBox(AniResolver.VERSION, AniResolver.APPNAME)
        If Not String.IsNullOrEmpty(Client) Then
            If System.Text.RegularExpressions.Regex.IsMatch(Version, "^[0-9 ]+$") Then
                IsLoading = True
                LoadingState = AniResolver.CREDENTIALSWARNING
                If Await AniDB.AniDBClient.SharedFunctions.CheckClient(Client, Version) Then
                    My.Settings.APP_CLIENT = Client
                    My.Settings.APP_CLIENTVER = Version
                    My.Settings.Save()
                Else
                    AniMessage.ShowMessage(AniResolver.APPNAME, AniResolver.ERROR, AniMessage.AniImage.WARNING, {New Button With {.Content = AniResolver.OK}}, True, True)
                End If
            End If
        End If
        IsLoading = False
        SET_CLIENT.IsChecked = False
    End Sub

    Private Async Sub SET_LIBRARY_ADD_Click(sender As Object, e As RoutedEventArgs) Handles SET_LIBRARY_ADD.Click
        Dim FBD As New Forms.FolderBrowserDialog
        If FBD.ShowDialog Then
            If Not My.Settings.LIBRARY_PATHS.Contains(FBD.SelectedPath) Then
                My.Settings.LIBRARY_PATHS.Add(FBD.SelectedPath)
                My.Settings.Save()
                SET_LIBRARY.Items.Add(FBD.SelectedPath)
                IsLoading = True
                LoadingState = AniResolver.SEARCHING
                Await Task.Run(Async Function()
                                   Dim i = 0
                                   For Each path In My.Settings.LIBRARY_PATHS
                                       Dim files = IO.Directory.GetDirectories(path, "*", IO.SearchOption.AllDirectories)
                                       For Each file In files
                                           i += 1
                                           Dim info = New IO.DirectoryInfo(file)
                                           Dim Xanime = Await MainClient.SearchByName(MainClient.SearchCache, info.Name)
                                           If Xanime.Count <> 0 Then
                                               Dim anime = Await MainCache.GetAnime(MainClient.IDFromSearch(Xanime(0)), False)
                                               If anime IsNot Nothing Then
                                                   Dispatcher.BeginInvoke(Sub()
                                                                              LoadingState = AniResolver.RETRIEVINGANIMEDATA & " (" & i & ")"
                                                                          End Sub)
                                                   Dispatcher.BeginInvoke(Sub()
                                                                              If anime.Restricted AndAlso My.Settings.APP_R18 = False Then Exit Sub
                                                                              'Browse_Library.Children.Add(New AnimeElementX(anime, False) With {.Margin = New Thickness(10, 10, 0, 0)})
                                                                              Dim CVI As New HandyControl.Controls.CoverViewItem
                                                                              CVI.Header = New AnimeElement(anime, 0) With {.IncrementVisibility = Visibility.Collapsed, .EpProVisibility = Visibility.Collapsed, .RedirectToUpdateSearchTab = False}
                                                                              Dim CVICW As New WrapPanel With {.Orientation = Orientation.Horizontal, .Margin = New Thickness(10)}
                                                                              Dim CVIC As New ScrollViewer With {.VerticalScrollBarVisibility = ScrollBarVisibility.Auto, .Content = CVICW}
                                                                              For Each ep In anime.Episodes
                                                                                  CVICW.Children.Add(New EpisodeElementS(ep, anime.ID, True, file) With {.Width = 400, .Margin = New Thickness(10, 10, 0, 0)})
                                                                              Next
                                                                              CVI.Content = CVIC
                                                                              Browse_Library.Items.Add(CVI)
                                                                          End Sub, System.Windows.Threading.DispatcherPriority.Render)
                                               Else
                                                   Dispatcher.BeginInvoke(Sub()
                                                                              LoadingState = AniResolver.RETRIEVINGANIMEDATA & " (" & i & ")"
                                                                          End Sub)
                                                   anime = Await MainClient.Anime(MainClient.IDFromSearch(Xanime(0)), False, My.Settings.CLIENT_LOADCHARACTERS, My.Settings.CLIENT_LOADCHARACTERSPICTURES)
                                                   If anime IsNot Nothing Then
                                                       Dispatcher.BeginInvoke(Sub()
                                                                                  If anime.Restricted AndAlso My.Settings.APP_R18 = False Then Exit Sub
                                                                                  'Browse_Library.Children.Add(New AnimeElementX(anime, False) With {.Margin = New Thickness(10, 10, 0, 0)})
                                                                                  Dim CVI As New HandyControl.Controls.CoverViewItem
                                                                                  CVI.Header = New AnimeElement(anime, 0) With {.IncrementVisibility = Visibility.Collapsed, .EpProVisibility = Visibility.Collapsed, .RedirectToUpdateSearchTab = False}
                                                                                  Dim CVICW As New WrapPanel With {.Orientation = Orientation.Horizontal, .Margin = New Thickness(10)}
                                                                                  Dim CVIC As New ScrollViewer With {.VerticalScrollBarVisibility = ScrollBarVisibility.Auto, .Content = CVICW}
                                                                                  For Each ep In anime.Episodes
                                                                                      CVICW.Children.Add(New EpisodeElementS(ep, anime.ID, True, file) With {.Width = 400, .Margin = New Thickness(10, 10, 0, 0)})
                                                                                  Next
                                                                                  CVI.Content = CVIC
                                                                                  Browse_Library.Items.Add(CVI)
                                                                              End Sub, System.Windows.Threading.DispatcherPriority.Render)
                                                   End If
                                               End If
                                           End If
                                       Next
                                   Next
                               End Function)
                IsLoading = False
            End If
        End If
    End Sub

    Private Sub SET_LIBRARY_REMOVE_Click(sender As Object, e As RoutedEventArgs) Handles SET_LIBRARY_REMOVE.Click
        If SET_LIBRARY.SelectedIndex <> -1 Then
            My.Settings.LIBRARY_PATHS.RemoveAt(SET_LIBRARY.SelectedIndex)
            My.Settings.Save()
            SET_LIBRARY.Items.RemoveAt(SET_LIBRARY.SelectedIndex)
        End If
    End Sub

    Private Sub SET_BACKGROUNDLOADING_Checked(sender As Object, e As RoutedEventArgs) Handles SET_BACKGROUNDLOADING.Checked
        My.Settings.APP_USEBACKGROUNDLOADING = True
        My.Settings.Save()
        IsBackgroundLoading = True
    End Sub

    Private Sub SET_BACKGROUNDLOADING_Unchecked(sender As Object, e As RoutedEventArgs) Handles SET_BACKGROUNDLOADING.Unchecked
        My.Settings.APP_USEBACKGROUNDLOADING = False
        My.Settings.Save()
        IsBackgroundLoading = False
    End Sub

    Private Sub SET_OFFLINEBROWSING_Checked(sender As Object, e As RoutedEventArgs) Handles SET_OFFLINEBROWSING.Checked
        My.Settings.CLIENT_OFFLINEBROWSING = True
        My.Settings.Save()
    End Sub

    Private Sub SET_OFFLINEBROWSING_Unchecked(sender As Object, e As RoutedEventArgs) Handles SET_OFFLINEBROWSING.Unchecked
        My.Settings.CLIENT_OFFLINEBROWSING = False
        My.Settings.Save()
    End Sub

    Private Async Sub TopBar_UPDATES_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles TopBar_UPDATES.MouseLeftButtonUp
        If Utils.CheckInternetConnection Then
            Notify(AniResolver.UPDATESCHECKC)
            TopBar_UPDATES.IsEnabled = False
            TopBar_UPDATES.RenderTransform = New RotateTransform()
            TopBar_UPDATES.RenderTransform.BeginAnimation(RotateTransform.AngleProperty, New Animation.DoubleAnimation(0, 360, New Duration(TimeSpan.FromMilliseconds(1000))) With {.AccelerationRatio = 0.5, .DecelerationRatio = 0.5, .RepeatBehavior = Animation.RepeatBehavior.Forever})
            Dim Updates = Await AniServer.Updates.CheckForUpdates(True)
            If Updates IsNot Nothing Then
                If Updates.Count > 0 Then
                    Dim CUpd = Updates.FirstOrDefault(Function(k) k.Type = AniServer.Updates.Update.UpdateType.Required)
                    If CUpd Is Nothing Then
                        Utils.NotificationBucket.AddToBucket({New Utils.NotificationBucket.NotificationItem(AniResolver.APPNAME, AniResolver.UPDATES, HandyControl.Data.NotifyIconInfoType.Info)}, True)
                    Else
                        Utils.NotificationBucket.AddToBucket({New Utils.NotificationBucket.NotificationItem(AniResolver.APPNAME, AniResolver.UPDATESCRITICAL, HandyControl.Data.NotifyIconInfoType.Info)}, True)
                        Dim UPDBtn = New Button With {.Content = AniResolver.UPDATE}
                        If AniMessage.ShowMessage(AniResolver.APPNAME, AniResolver.UPDATESCRITICALWARNING, AniMessage.AniImage.WARNING, {New Button With {.Content = AniResolver.OK}, UPDBtn}, True, True) Is UPDBtn Then
                            Process.Start(AniServer.Updates.ReleaseLink)
                            Dim ExitBtn As New Button With {.Content = AniResolver.CLOSE}
                            If AniMessage.ShowMessage(AniResolver.APPNAME, AniResolver.CONTINUE, AniMessage.AniImage.WARNING, {New Button With {.Content = AniResolver.YES}, ExitBtn}, True, True) Is ExitBtn Then
                                Close()
                            End If
                        End If
                    End If
                End If
                End If
                Dim RAnime As New Animation.DoubleAnimation(360, New Duration(TimeSpan.FromMilliseconds(500)))
                AddHandler RAnime.Completed, Sub()
                                                 TopBar_UPDATES.RenderTransform = Nothing
                                                 TopBar_UPDATES.IsEnabled = True
                                             End Sub
                TopBar_UPDATES.RenderTransform.BeginAnimation(RotateTransform.AngleProperty, RAnime)
            Else
                Notify(AniResolver.UPDATESERROR)
        End If
    End Sub

    Private Async Sub Home_Quote_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles Home_Quote.MouseLeftButtonUp
        Home_Quote.IsEnabled = False
        IsLoading = True
        Console.WriteLine(Utils.ConsoleInfoText & "Requesting Random Quote from AnimechanClient")
        Dim str = Await MainQuoteClient.GetRandomQuote
        IsLoading = False
        If str IsNot Nothing Then
            Await Utils.FillTextBlock(Home_Quote, str.Character & " From " & str.Anime & " Said: " & Environment.NewLine & str.Quote, 5, True)
        End If
        Home_Quote.IsEnabled = True
    End Sub

    Private Sub SOCIAL_DISC_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles SOCIAL_DISC.MouseLeftButtonUp
        Process.Start("https://discord.gg/cdN8x99h8X")
    End Sub

    Private Sub SOCIAL_GITHUB_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles SOCIAL_GITHUB.MouseLeftButtonUp
        Process.Start("https://github.com/AnesHamdani08")
    End Sub

    Private Sub SOCIAL_SOURCEFORGE_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles SOCIAL_SOURCEFORGE.MouseLeftButtonUp
        Process.Start("https://sourceforge.net/u/anes08/profile")
    End Sub

    Private Async Sub Search_SeeIn_Wallpapers_Click(sender As Object, e As RoutedEventArgs) Handles Search_SeeIn_Wallpapers.Click
        Dim MTClient As New API.MinitokyoClient()
        IsLoading = True
        LoadingState = AniResolver.SEARCHING
        If TryCast(Search_TAB.Tag, AniDBClient.AnimeElement) IsNot Nothing Then
            Dim WLL = Await MTClient.GetWallpapers(TryCast(Search_TAB.Tag, AniDBClient.AnimeElement).Title)
            If WLL IsNot Nothing Then
                If WLL.Count > 0 Then
                    For Each child In SearchItems_Items.Children
                        If TypeOf child Is IDisposable Then
                            TryCast(child, IDisposable)?.Dispose()
                        End If
                    Next
                    SearchItems_Items.Children.Clear()
                    For Each wallpaper In WLL
                        SearchItems_Items.Children.Add(New WallpaperElementX(wallpaper) With {.Margin = New Thickness(10, 10, 0, 0)})
                    Next
                    MainTabControl.SelectedIndex = 6
                End If
            End If
        End If
        IsLoading = False
    End Sub

#Region "Video"
    WithEvents VideoUIManager As New Forms.Timer With {.Interval = 500}
    Private Sub Watch_MediaElement_MediaOpened(sender As Object, e As RoutedEventArgs) Handles Watch_MediaElement.MediaOpened
        VideoUIManager.Start()
        Watch_Track.Maximum = Watch_MediaElement.NaturalDuration.TimeSpan.TotalSeconds
        Watch_MediaElement.Play()
        Watch_Play.Visibility = Visibility.Collapsed
        Watch_Pause.Visibility = Visibility.Visible
        TopBar_Apps_VideoViewer.Visibility = Visibility.Visible
    End Sub

    Private Sub Watch_MediaElement_MediaEnded(sender As Object, e As RoutedEventArgs) Handles Watch_MediaElement.MediaEnded
        VideoUIManager.Stop()
        TopBar_Apps_VideoViewer.Visibility = Visibility.Collapsed        
    End Sub

    Private Sub Watch_MediaElement_MediaFailed(sender As Object, e As ExceptionRoutedEventArgs) Handles Watch_MediaElement.MediaFailed
        VideoUIManager.Stop()
    End Sub

    Private Sub Watch_Play_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles Watch_Play.MouseLeftButtonUp
        If My.Computer.Keyboard.ShiftKeyDown Then
            Dim OFD As New Forms.OpenFileDialog With {.CheckFileExists = True}
            If OFD.ShowDialog <> Forms.DialogResult.Cancel Then
                Watch_MediaElement.Source = New Uri(OFD.FileName, UriKind.Absolute)
                Watch_MediaElement.Play()
            End If
        Else
            Watch_MediaElement.Play()
            Watch_Play.Visibility = Visibility.Collapsed
            Watch_Pause.Visibility = Visibility.Visible
        End If
    End Sub

    Private Sub Watch_Pause_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles Watch_Pause.MouseLeftButtonUp
        Watch_MediaElement.Pause()
        Watch_Play.Visibility = Visibility.Visible
        Watch_Pause.Visibility = Visibility.Collapsed
    End Sub

    Private Sub Watch_VolumeControl_OnValueChanged(value As Double) Handles Watch_VolumeControl.OnValueChanged
        Watch_MediaElement.Volume = value / 100
    End Sub

    Private Sub Watch_FullScreen_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles Watch_FullScreen.MouseLeftButtonUp

    End Sub

    Private Sub VideoUIManager_Tick(sender As Object, e As EventArgs) Handles VideoUIManager.Tick
        Try
            Watch_PosnDur.Text = Utils.SecsToMins(Watch_MediaElement.Position) & "/" & Utils.SecsToMins(Watch_MediaElement.NaturalDuration.TimeSpan)
            If Not Watch_Track.IsMouseOver Then Watch_Track.Value = Watch_MediaElement.Position.TotalSeconds
        Catch
        End Try
    End Sub

    Private Sub Watch_Track_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles Watch_Track.ValueChanged
        Try
            If Watch_Track.IsMouseOver Then Watch_MediaElement.Position = TimeSpan.FromSeconds(e.NewValue)
        Catch
        End Try
    End Sub

    Private Sub Watch_SP_MouseEnter(sender As Object, e As MouseEventArgs) Handles Watch_SP.MouseEnter
        Watch_SP.BeginAnimation(OpacityProperty, New Animation.DoubleAnimation(1, New Duration(TimeSpan.FromMilliseconds(100))) With {.DecelerationRatio = 0.8})
    End Sub

    Private Sub Watch_SP_MouseLeave(sender As Object, e As MouseEventArgs) Handles Watch_SP.MouseLeave
        Watch_SP.BeginAnimation(OpacityProperty, New Animation.DoubleAnimation(0, New Duration(TimeSpan.FromMilliseconds(100))) With {.AccelerationRatio = 0.8})
    End Sub

    Private Async Sub SET_CLEARCUSTOMLOCKWALL_Checked(sender As Object, e As RoutedEventArgs) Handles SET_CLEARCUSTOMLOCKWALL.Checked
        SET_CLEARCUSTOMLOCKWALL.IsEnabled = False
        IsLoading = True
        LoadingState = AniResolver.CLEARCUSTOMLOCKWALL
        Await Task.Run(Sub()
                           Dim PSI As New ProcessStartInfo With {.FileName = IO.Path.Combine(My.Application.Info.DirectoryPath, "AniLife.exe"), .UseShellExecute = True, .Verb = "runas", .Arguments = "-CLEAR_LOCKWALL"}
                           Dim Proc = Process.Start(PSI)
                           Proc.WaitForExit(2000)
                           Try
                               Proc.Kill()
                           Catch
                           End Try
                       End Sub)
        IsLoading = False
        SET_CLEARCUSTOMLOCKWALL.IsEnabled = True
        SET_CLEARCUSTOMLOCKWALL.IsChecked = False
    End Sub


#End Region
End Class
