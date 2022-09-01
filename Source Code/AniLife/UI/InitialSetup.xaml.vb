Imports AniLife.API
Imports System.ComponentModel

Public Class InitialSetup
    Dim IsOfflineMode As Boolean = False
    Private Sub InitialSetup_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        My.Settings.Upgrade()
        My.Settings.Save()
        If Not My.Settings.ISFIRSTRUN Then
            DoKill = False
            DialogResult = True
        End If
        If Not Utils.CheckInternetConnection Then
            Dim YesBTN As New Button With {.Content = AniResolver.YES}
            If AniMessage.ShowMessage(AniResolver.APPNAME, AniResolver.INIT_NOINTERNET, AniMessage.AniImage.WARNING, {YesBTN, New Button With {.Content = AniResolver.NO}}, True, True) Is YesBTN Then
                IsOfflineMode = True
            End If
        End If
        Dim OAnime As New Animation.DoubleAnimation(1, New Duration(TimeSpan.FromMilliseconds(1000))) With {.AccelerationRatio = 0.9}
        Dim MAnim As New Animation.ThicknessAnimation(New Thickness(250, 200, 250, 300), New Duration(TimeSpan.FromMilliseconds(250))) With {.AccelerationRatio = 0.9}
        Dim SOAnime As New Animation.DoubleAnimation(1, New Duration(TimeSpan.FromMilliseconds(500))) With {.AccelerationRatio = 0.9}
        Dim TOAnime As New Animation.DoubleAnimation(1, New Duration(TimeSpan.FromMilliseconds(500))) With {.AccelerationRatio = 0.9}
        Dim SMAnime As New Animation.ThicknessAnimation(New Thickness(420, 40, 0, 0), New Duration(TimeSpan.FromMilliseconds(500))) With {.AutoReverse = True, .RepeatBehavior = Animation.RepeatBehavior.Forever}
        AddHandler OAnime.Completed, Sub()
                                         Main_Logo.BeginAnimation(MarginProperty, MAnim)
                                         Background_Image.Effect.BeginAnimation(Effects.BlurEffect.RadiusProperty, New Animation.DoubleAnimation(10, New Duration(TimeSpan.FromMilliseconds(250))) With {.AccelerationRatio = 0.9})
                                     End Sub
        AddHandler MAnim.Completed, Sub()
                                        Main_Text.BeginAnimation(OpacityProperty, SOAnime)
                                    End Sub
        AddHandler SOAnime.Completed, Sub()
                                          Main_Start.BeginAnimation(OpacityProperty, TOAnime)
                                      End Sub
        AddHandler TOAnime.Completed, Sub()
                                          Main_Start.BeginAnimation(MarginProperty, SMAnime)
                                      End Sub
        Main_Logo.BeginAnimation(OpacityProperty, OAnime)
    End Sub

    Private Async Sub Main_Start_Click(sender As Object, e As RoutedEventArgs) Handles Main_Start.Click
        Dim MAnim As New Animation.ThicknessAnimation(New Thickness(ActualWidth + 32, 40, 0, 0), New Duration(TimeSpan.FromMilliseconds(500)))
        Dim SAnim As New Animation.ThicknessAnimation(New Thickness(-FirstPage.ActualWidth, 0, FirstPage.ActualWidth, 0), New Duration(TimeSpan.FromMilliseconds(500))) With {.AccelerationRatio = 0.9}
        AddHandler MAnim.Completed, Sub()
                                        Main_Start.Visibility = Visibility.Collapsed
                                        FirstPage.BeginAnimation(MarginProperty, SAnim)
                                        SecondPage.BeginAnimation(MarginProperty, New Animation.ThicknessAnimation(New Thickness(0, 0, 0, 0), New Duration(TimeSpan.FromMilliseconds(500))) With {.AccelerationRatio = 0.9})
                                    End Sub
        If Not IsOfflineMode Then
            Do While True
                MessageBox.Show(AniResolver.ANIDBCLIENTINSTRUCTIONS, AniResolver.APPNAME, MessageBoxButton.OK, MessageBoxImage.Information)
                Dim Result = AdvancedInputBox.AdvancedInputBox.ShowQuick(AniResolver.APPNAME, AniResolver.CREADENTIALSFILL, {New AdvancedInputBox.Controls.TextField(AniResolver.NAME, True), New AdvancedInputBox.Controls.NumberField(AniResolver.VERSION, 0, Double.MaxValue, True)})
                If Result IsNot Nothing Then
                    Try
                        If Await AniDB.AniDBClient.SharedFunctions.CheckClient(Result.Values(AniResolver.NAME).Value, Result.Values(AniResolver.VERSION).Value) Then
                            My.Settings.APP_CLIENT = Result.Values(AniResolver.NAME).Value
                            My.Settings.APP_CLIENTVER = Result.Values(AniResolver.VERSION).Value
                            My.Settings.Save()
                            Exit Do
                        Else
                            AniMessage.ShowMessage(AniResolver.APPNAME, AniResolver.ERROR, AniMessage.AniImage.WARNING, {New Button With {.Content = AniResolver.OK}}, True, True)
                        End If
                    Catch
                    End Try
                End If
                'Dim Client = InputBox(AniResolver.CLIENT, AniResolver.APPNAME)
                'Dim Version = InputBox(AniResolver.VERSION, AniResolver.APPNAME)
                'Dim NVersion As Integer
                'If Integer.TryParse(Version, NVersion) AndAlso Not String.IsNullOrEmpty(Client) Then
                '    Try
                '        If Await AniDB.AniDBClient.SharedFunctions.CheckClient(Client, NVersion) Then
                '            My.Settings.APP_CLIENT = Client
                '            My.Settings.APP_CLIENTVER = Version
                '            My.Settings.Save()
                '            Exit Do
                '        Else
                '            AniMessage.ShowMessage(AniResolver.APPNAME, AniResolver.ERROR, AniMessage.AniImage.WARNING, {New Button With {.Content = AniResolver.OK}}, True, True)
                '        End If
                '    Catch
                '    End Try
                'End If
            Loop
        End If
        Main_Start.BeginAnimation(MarginProperty, MAnim)
    End Sub

    Private Sub Second_Next_Click(sender As Object, e As RoutedEventArgs) Handles Second_Next.Click
        Dim SAnim As New Animation.ThicknessAnimation(New Thickness(-SecondPage.ActualWidth, 0, SecondPage.ActualWidth, 0), New Duration(TimeSpan.FromMilliseconds(500))) With {.AccelerationRatio = 0.9}
        SecondPage_First.BeginAnimation(MarginProperty, SAnim)
        SecondPage_Second.BeginAnimation(MarginProperty, New Animation.ThicknessAnimation(New Thickness(0, 0, 0, 0), New Duration(TimeSpan.FromMilliseconds(500))) With {.AccelerationRatio = 0.9})
    End Sub

    Private Async Sub Second_Second_Next_Click(sender As Object, e As RoutedEventArgs) Handles Second_Second_Next.Click
        SecondPage.BeginAnimation(OpacityProperty, New Animation.DoubleAnimation(0, New Duration(TimeSpan.FromMilliseconds(500))))
        LastPage.Visibility = Visibility.Visible
        Await Task.Delay(500)
        LastPage.BeginAnimation(OpacityProperty, New Animation.DoubleAnimation(1, New Duration(TimeSpan.FromMilliseconds(500))))
        Await FirstRunPrepare()
        DoKill = False
        DialogResult = True
    End Sub

    Private Sub Second_Second_Previous_Click(sender As Object, e As RoutedEventArgs) Handles Second_Second_Previous.Click
        Dim SAnim As New Animation.ThicknessAnimation(New Thickness(SecondPage.ActualWidth, 0, -SecondPage.ActualWidth, 0), New Duration(TimeSpan.FromMilliseconds(500))) With {.AccelerationRatio = 0.9}
        SecondPage_Second.BeginAnimation(MarginProperty, SAnim)
        SecondPage_First.BeginAnimation(MarginProperty, New Animation.ThicknessAnimation(New Thickness(0, 0, 0, 0), New Duration(TimeSpan.FromMilliseconds(500))) With {.AccelerationRatio = 0.9})
    End Sub

    Private Async Function FirstRunPrepare() As Task
        Await Task.Run(Async Function()

                           Dim MainLibrary As New AniLibrary()
                           Dim MainCache As New AniCache()
                           Dim MainClient As AniDB.AniDBClient = If(IsOfflineMode, New AniDB.AniDBClient("AniLife", 1), New AniDB.AniDBClient(My.Settings.APP_CLIENT, My.Settings.APP_CLIENTVER))

                           MainLibrary = AniLibrary.MakeLibrary()
                           IO.Directory.CreateDirectory(IO.Path.Combine(My.Settings.DATA_LOCATION, "Library"))
                           If IO.File.Exists(IO.Path.Combine(My.Settings.DATA_LOCATION, "Library", "Library.xml")) Then
                               Dispatcher.Invoke(Sub()
                                                     Dim OVBtn As New Button With {.Content = AniResolver.OVERWRITE}
                                                     If AniMessage.ShowMessage(AniResolver.APPNAME, AniResolver.LIBRARY & Space(1) & AniResolver.FILEEXISTS, AniMessage.AniImage.WARNING, {New Button With {.Content = AniResolver.OK}, OVBtn}, True, True) Is OVBtn Then
                                                         MainLibrary.Save(IO.Path.Combine(My.Settings.DATA_LOCATION, "Library", "Library.xml"))
                                                     End If
                                                 End Sub)
                           Else
                               MainLibrary.Save(IO.Path.Combine(My.Settings.DATA_LOCATION, "Library", "Library.xml"))
                           End If
                           If IO.File.Exists(IO.Path.Combine(My.Settings.DATA_LOCATION, "Cache", "Cache.xml")) Then
                               Dispatcher.Invoke(Async Function()
                                                     Dim OVBtn As New Button With {.Content = AniResolver.OVERWRITE}
                                                     If AniMessage.ShowMessage(AniResolver.APPNAME, AniResolver.CACHE & Space(1) & AniResolver.FILEEXISTS, AniMessage.AniImage.WARNING, {New Button With {.Content = "OK"}, OVBtn}, True, True) Is OVBtn Then
                                                         MainCache.Load(Await AniCache.MakeCache(IO.Path.Combine(My.Settings.DATA_LOCATION, "Cache")))
                                                         MainCache.Save()
                                                     Else
                                                         MainCache.Load(IO.Path.Combine(My.Settings.DATA_LOCATION, "Cache", "Cache.xml"))
                                                     End If
                                                 End Function)
                           Else
                               MainCache.Load(Await AniCache.MakeCache(IO.Path.Combine(My.Settings.DATA_LOCATION, "Cache")))
                               MainCache.Save()
                           End If

                           Try
                               Dim Data = Await MainClient.GetSearchData
                               If MainClient.LoadSearchCacheFromDocument(Data) Then
                                   Await MainClient.SaveSearchCache(Data, IO.Path.Combine(My.Settings.DATA_LOCATION, "Cache", "SearchCache.xml"))
                               End If
                           Catch ex As Exception
                               Dispatcher.Invoke(Sub()
                                                     Dim DownloadManualBTN As New Button With {.Content = AniResolver.DOWNLOADMANUALLY}
                                                     Dim LoadFromFileBTN As New Button With {.Content = AniResolver.LOADFROMFILE}
                                                     Dim Result As Button = Nothing
                                                     If Not IsOfflineMode Then
                                                         Result = AniMessage.ShowMessage(AniResolver.APPNAME, ex.Message, AniMessage.AniImage.WARNING, {New Button With {.Content = AniResolver.OK}, DownloadManualBTN, LoadFromFileBTN}, True, True)
                                                     Else
                                                         Result = AniMessage.ShowMessage(AniResolver.APPNAME, ex.Message, AniMessage.AniImage.WARNING, {New Button With {.Content = AniResolver.OK}, LoadFromFileBTN}, True, True)
                                                     End If
                                                     If Result Is DownloadManualBTN Then
                                                         Process.Start("http://anidb.net/api/anime-titles.xml.gz")
                                                         Do While MainClient.SearchCache Is Nothing
                                                             Dim OFD As New Forms.OpenFileDialog With {.CheckFileExists = True, .Filter = "Search Cache|*.gz"}
                                                             If OFD.ShowDialog Then
                                                                 MainClient.LoadSearchCacheFromDocument(MainClient.PrepareSearchData(OFD.FileName))
                                                             Else
                                                                 AniMessage.ShowMessage(AniResolver.APPNAME, AniResolver.SEARCHDISABLED, AniMessage.AniImage.WARNING, {New Button With {.Content = AniResolver.OK}}, True, True)
                                                                 Exit Do
                                                             End If
                                                         Loop
                                                     ElseIf Result Is LoadFromFileBTN Then
                                                         Do While MainClient.SearchCache Is Nothing
                                                             Dim OFD As New Forms.OpenFileDialog With {.CheckFileExists = True, .Filter = "Search Cache|*.gz"}
                                                             If OFD.ShowDialog Then
                                                                 MainClient.LoadSearchCacheFromDocument(MainClient.PrepareSearchData(OFD.FileName))
                                                             Else
                                                                 AniMessage.ShowMessage(AniResolver.APPNAME, AniResolver.SEARCHDISABLED, AniMessage.AniImage.WARNING, {New Button With {.Content = AniResolver.OK}}, True, True)
                                                                 Exit Do
                                                             End If
                                                         Loop
                                                     End If
                                                 End Sub)

                           End Try

                           If MainClient.SearchCache Is Nothing Then
                               Await MainClient.LoadSearchCache(IO.Path.Combine(My.Settings.DATA_LOCATION, "Cache", "SearchCache.xml"))
                           End If

                           Dim i = 0
                           For Each path In My.Settings.LIBRARY_PATHS
                               Dim files = IO.Directory.GetDirectories(path)
                               For Each file In files
                                   i += 1
                                   Dim info = New IO.DirectoryInfo(file)
                                   Dim Xanime = Await MainClient.SearchExactByName(MainClient.SearchCache, info.Name)
                                   If Xanime IsNot Nothing Then
                                       Dim anime = Await MainCache.GetAnime(MainClient.IDFromSearch(Xanime), False)
                                       If anime IsNot Nothing Then
                                           Dispatcher.BeginInvoke(Sub()
                                                                      Title = AniResolver.RETRIEVINGANIMEDATA & " (" & i & ")"
                                                                  End Sub)
                                       Else
                                           If IsOfflineMode Then Continue For
                                           Dispatcher.BeginInvoke(Sub()
                                                                      Title = AniResolver.RETRIEVINGANIMEDATA & " (" & i & ")"
                                                                  End Sub)
                                           anime = Await MainClient.Anime(MainClient.IDFromSearch(Xanime), False, My.Settings.CLIENT_LOADCHARACTERS, My.Settings.CLIENT_LOADCHARACTERSPICTURES)
                                           If anime IsNot Nothing Then
                                               Dispatcher.BeginInvoke(Sub()
                                                                          MainCache.AddToCache(anime)
                                                                      End Sub, System.Windows.Threading.DispatcherPriority.Render)
                                           End If
                                       End If
                                   End If
                               Next
                           Next

                           My.Settings.ISFIRSTRUN = False
                           My.Settings.Save()
                       End Function)
    End Function

    Private DoKill As Boolean = True
    Private Sub InitialSetup_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        If DoKill Then
            e.Cancel = True
            Process.GetCurrentProcess.Kill()
        End If
    End Sub

    Private Sub Second_AddLibrayPath_Click(sender As Object, e As RoutedEventArgs) Handles Second_AddLibrayPath.Click
        Dim FBD As New Forms.FolderBrowserDialog
        If FBD.ShowDialog Then
            If Not My.Settings.LIBRARY_PATHS.Contains(FBD.SelectedPath) Then
                My.Settings.LIBRARY_PATHS.Add(FBD.SelectedPath)
                My.Settings.Save()
            End If
        End If
    End Sub

    Private Sub Second_Second_DataLocation_Click(sender As Object, e As RoutedEventArgs) Handles Second_Second_DataLocation.Click
        Dim FBD As New Forms.FolderBrowserDialog
        If FBD.ShowDialog Then
            My.Settings.DATA_LOCATION = IO.Path.Combine(FBD.SelectedPath, "AniLife")
            My.Settings.Save()
        End If
    End Sub
End Class
