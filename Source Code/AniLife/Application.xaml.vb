Imports System.Windows.Threading

Class Application
    Private Sub Application_DispatcherUnhandledException(sender As Object, e As DispatcherUnhandledExceptionEventArgs) Handles Me.DispatcherUnhandledException
        Console.WriteLine(Utils.ConsoleDebugText & e.Exception.ToString)
        Dim OkBtn As New Button With {.Content = AniResolver.OK}
        If AniMessage.ShowMessage(AniResolver.ERROR, e.Exception.Message, AniMessage.AniImage.WARNING, {OkBtn}, True, True) Is OkBtn Then
            My.Windows.DeveloperConsole.Show()
        End If
        e.Handled = True
    End Sub

    Private Sub Application_Startup(sender As Object, e As StartupEventArgs) Handles Me.Startup
        If e.Args.Count > 0 Then
            If e.Args(0) = "-SET_LOCKWALL" Then
                If IO.File.Exists(e.Args(1)) Then
                    If API.MinitokyoClient.SetAsLockscreenWallpaper(e.Args(1)) Then
                        Console.WriteLine("OK")
                    Else
                        Console.WriteLine("ERROR")
                    End If
                    Console.WriteLine("ERROR")
                End If
                Process.GetCurrentProcess.Kill()
            ElseIf e.Args(0) = "-CLEAR_LOCKWALL" Then
                If API.MinitokyoClient.ClearLockscreenWallpaper Then
                    Console.WriteLine("OK")
                Else
                    Console.WriteLine("ERROR")
                End If
                Process.GetCurrentProcess.Kill()
            End If
        End If

        Console.SetOut(New MultiTextWriter({New MultiTextWriter.ControlWriter(My.Windows.DeveloperConsole.ConsoleOut_TB, My.Windows.DeveloperConsole.Dispatcher), Console.Out}))

        Console.WriteLine(Utils.ConsoleInfoText & "Connection To Internal Console Established")
        Console.WriteLine(Utils.ConsoleInfoText & "Begin Init At APP Level")

        If My.Settings.ISFIRSTRUN = True Then
            My.Settings.Upgrade()
            My.Settings.Save()
            If My.Settings.ISFIRSTRUN Then
                My.Settings.DATA_LOCATION = IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AniLife")
                My.Settings.Save()
            End If
        End If

        Try
            If My.Settings.APP_ISCUSTOMTHEME Then
                Console.WriteLine(Utils.ConsoleInfoText & "Applying Custom Theme")
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
                Console.WriteLine(Utils.ConsoleInfoText & "Custom Theme Applied Successfuly")
            ElseIf My.Settings.APP_ISEXTERNALTHEME Then
                Console.WriteLine(Utils.ConsoleInfoText & "Applying External Theme")
                AniResolver.ClearStyleDictionaries()
                Application.Current.Resources.MergedDictionaries.Add(New ResourceDictionary() With {.Source = New Uri(My.Settings.APP_EXTERNALTHEMEPATH, UriKind.Absolute)})
                Console.WriteLine(Utils.ConsoleInfoText & "External Theme Applied Successfuly")
            Else
                Console.WriteLine(Utils.ConsoleInfoText & "Applying Internal Theme " & My.Settings.APP_THEME)
                AniResolver.ClearStyleDictionaries()
                Application.Current.Resources.MergedDictionaries.Add(New ResourceDictionary() With {.Source = New Uri("Colors/" & My.Settings.APP_THEME & ".xaml", UriKind.Relative)})
                Console.WriteLine(Utils.ConsoleInfoText & "Internal Theme Applied Successfuly")
            End If
            With HandyControl.Themes.ThemeManager.Current
                .ApplicationTheme = AniResolver.APPTHEME
                .AccentColor = AniResolver.ACCENT
            End With
        Catch ex As Exception
            Console.WriteLine(Utils.ConsoleDebugText & ex.ToString)
        End Try
        Try
            If My.Settings.APP_ISEXTERNALLANGUAGE Then
                Console.WriteLine(Utils.ConsoleInfoText & "Applying External Language")
                AniResolver.ClearLocalizationDictionaries()
                Application.Current.Resources.MergedDictionaries.Add(New ResourceDictionary() With {.Source = New Uri(My.Settings.APP_EXTERNALLANGUAGEPATH, UriKind.Absolute)})
                Console.WriteLine(Utils.ConsoleInfoText & "External Language Applied Successfuly")
            Else
                Console.WriteLine(Utils.ConsoleInfoText & "Applying Internal Language " & My.Settings.APP_LANGUAGE)
                AniResolver.ClearLocalizationDictionaries()
                Application.Current.Resources.MergedDictionaries.Add(New ResourceDictionary() With {.Source = New Uri("Languages/" & My.Settings.APP_LANGUAGE & ".xaml", UriKind.Relative)})
                Console.WriteLine(Utils.ConsoleInfoText & "Internal Language Applied Successfuly")
            End If
        Catch ex As Exception
            Console.WriteLine(Utils.ConsoleDebugText & ex.ToString)
        End Try

        If My.Settings.ISFIRSTRUN Then
            Console.WriteLine(Utils.ConsoleInfoText & "Preparing For First Run")
            My.Windows.InitialSetup.ShowDialog()
        End If
    End Sub

    ' Application-level events, such as Startup, Exit, and DispatcherUnhandledException
    ' can be handled in this file.

    'TODO	  
    'add name local episode by names from db             	

    'CHANGELOG
    'Search is now Anime , Search Item is now Search
    'Added anime links in Anime view (ANN,MAL,Netflix,Crunchyroll...)
    'Added anime episodes
    'Added local anime episode with ability to watch if episode is found
    'Watch local anime episode right now from AniLife , or use your favourite player
    'Added Wallpaper search with: Set as Wallpaper, Set as Lockscreen and View
    'Updated Image API
    'Initial setup now automatically detects previous versions settings and loads them
    'Fixed some issues related to date parsing
    'Initial setup now have an offline setup mode
    'Bugs Fixes
End Class
