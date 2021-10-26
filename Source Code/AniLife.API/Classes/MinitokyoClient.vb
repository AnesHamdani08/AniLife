Imports System.Net
''' <summary>
''' Used to get wallpapers from Minitoyko
''' </summary>
Public Class MinitokyoClient
    Public Const Version = "0.0.0.1-Beta.4"
    Public Const SearchLink As String = "http://www.minitokyo.net/search?q="
    Private ReadOnly Property ConsoleInfoText As String
        Get
            Return "[" & Now.ToString("HH:mm:ss") & "][INFO]: "
        End Get
    End Property
    Private ReadOnly Property ConsoleDebugText As String
        Get
            Return "[" & Now.ToString("HH:mm:ss") & "][DEBUG]: "
        End Get
    End Property
    Public Sub New()
        If ServicePointManager.SecurityProtocol <> SecurityProtocolType.Tls12 Then
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
            Console.WriteLine("[" & Now.ToString("HH:mm:ss") & "][INFO]: " & "TLS 1.2 Is Required for Minitokyo API, Now Using TLS 1.2")
        End If
    End Sub
    Public Async Function GetWallpapers(query As String) As Task(Of List(Of WallpaperElement))
        Using HC As New Http.HttpClient
            Try
                Console.WriteLine(ConsoleInfoText & "Fetching Wallpapers Data for " & query)
                Dim Data = Await HC.GetStringAsync(SearchLink & query)
                Console.WriteLine(ConsoleInfoText & "Parsing Retrieved Data for " & query)
                Dim WB As New System.Windows.Forms.WebBrowser
                WB.ScriptErrorsSuppressed = True
                WB.DocumentText = "0"
                WB.Document.OpenNew(True)
                WB.Document.Write(Data)
                WB.Refresh()
                Console.WriteLine(ConsoleInfoText & "Data filled to WebBrowser, Parsing...")
                Dim WRP = WB.Document.GetElementById("content")
                Dim Name As String
                Dim CoverLink As String
                Console.WriteLine(ConsoleInfoText & "Parsing Phase 1: GetElementById(""content"") Returned: " & If(WRP Is Nothing, "Nothing", "A Valid Element"))
                Dim WLPCNT As New List(Of Windows.Forms.HtmlElement)
                For Each ELM As Windows.Forms.HtmlElement In WRP.Children
                    If ELM.TagName.ToLower = "img" AndAlso ELM.Id.ToLower = "cover" Then
                        Name = ELM.GetAttribute("alt")
                        CoverLink = ELM.GetAttribute("src")
                    End If

                    If ELM.TagName.ToLower = "ul" AndAlso ELM.GetAttribute("className").ToLower = "scans" Then
                        WLPCNT.Add(ELM)
                    End If
                Next
                Console.WriteLine(ConsoleInfoText & "Parsing Phase 2: GetElementByClassName(""ul"" & ""scans"") Returned: " & WLPCNT.Count)
                Dim WPELML As New List(Of WallpaperElement)
                For Each WLP In WLPCNT
                    For Each Child As Windows.Forms.HtmlElement In WLP.Children
                        Dim WPELM As WallpaperElement
                        If Child.TagName.ToLower = "li" Then
                            Dim Link = Child.FirstChild.GetAttribute("href")
                            Dim Resolution As String
                            Dim ThumbLink As String
                            Dim DirectLink As String
                            For Each SubChild As Windows.Forms.HtmlElement In Child.Children
                                If SubChild.TagName.ToLower = "a" Then
                                    For Each SubSubChild As Windows.Forms.HtmlElement In SubChild.Children
                                        If SubSubChild.TagName.ToLower = "img" Then
                                            ThumbLink = SubSubChild.GetAttribute("src")
                                            Resolution = SubSubChild.GetAttribute("title")
                                        End If
                                    Next
                                End If
                                If SubChild.TagName.ToLower = "p" Then
                                    For Each SubSubChild As Windows.Forms.HtmlElement In SubChild.Children
                                        If SubSubChild.TagName.ToLower = "a" Then
                                            DirectLink = SubSubChild.GetAttribute("href")
                                        End If
                                    Next
                                End If
                            Next
                            WPELM = New WallpaperElement With {.Resolution = Resolution, .ThumbLink = ThumbLink, .WallpaperDirectLink = DirectLink, .WallpaperLink = Link, .Name = Name, .CoverLink = CoverLink}
                            Console.WriteLine(ConsoleInfoText & "Parsing Phase 3: Found WallpaperElement: " & WPELM.ToString)
                            WPELML.Add(WPELM)
                        End If
                    Next
                Next
                Console.WriteLine(ConsoleInfoText & "Parsing Phase 3: Completed With Count: " & WPELML.Count)
                Console.WriteLine(ConsoleInfoText & "Returning Wallpaper Results...")
                Return WPELML
            Catch ex As Exception
                Console.WriteLine(ConsoleDebugText & ex.ToString)
                Return Nothing
            End Try
        End Using
    End Function
#Region "Shared"
    Public Shared Async Function GetImageAsync(url As String) As Task(Of System.Drawing.Image)
        'Base URL: https://cdn.anidb.net/images/main/
        'Base API URL: http://img7.anidb.net/pics/anime/
        Console.WriteLine("[" & Now.ToString("HH:mm:ss") & "][INFO]: " & "Fetching Wallpaper From: " & url)
        If String.IsNullOrEmpty(url) Then
            Return Nothing
        End If
        Return Await Task.Run(Function()
                                  Using WC As New WebClient()
                                      Try
                                          Dim ImageData = WC.DownloadData(url)
                                          Dim IMG = System.Drawing.Image.FromStream(New IO.MemoryStream(ImageData))
                                          Return IMG
                                      Catch ex As Exception
                                          Console.WriteLine("[" & Now.ToString("HH:mm:ss") & "][DEBUG]: " & ex.ToString)
                                          Return Nothing
                                      End Try
                                  End Using
                              End Function)
    End Function
    Public Shared SPI_SETDESKTOPWALLPAPER As Integer = 20
    Public Shared SPIF_UPDATEINIFILE As Integer = 1
    Public Shared SPIF_SENDWININICHANGE As Integer = 2

    Public Shared Sub SetAsDesktopWallpaper(path As String)
        Console.WriteLine("[" & Now.ToString("HH:mm:ss") & "][INFO]: Setting """ & path & """ As Desktop Wallpaper...")
        If IO.File.Exists(path) Then SystemParametersInfo(SPI_SETDESKTOPWALLPAPER, 0, path, (SPIF_UPDATEINIFILE Or SPIF_SENDWININICHANGE))
    End Sub

    Private Declare Auto Function SystemParametersInfo Lib "user32.dll" (ByVal uAction As Integer, ByVal uParam As Integer, ByVal lpvParam As String, ByVal fuWinIni As Integer) As Integer
    ''' <summary>
    ''' Note: this requires Admin Access
    ''' Returns True if successeds else, returns False
    ''' </summary>
    ''' <param name="path">If file doesn't exist, the sub won't execute at all</param>
    Public Shared Function SetAsLockscreenWallpaper(path As String) As Boolean
        'Create new Registry key at: Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\Curre‌​ntVersion\Personaliz‌​ationCSP. 
        'Then for this key create strings LockScreenImagePath, LockScreenImageUrl with data set to absolute path of your image. 
        'Then set a DWORD, name to LockScreenImageStatus, its value to 1
        Console.WriteLine("[" & Now.ToString("HH:mm:ss") & "][INFO]: Setting """ & path & """ As Lockscreen Wallpaper...")
        Try
            If IO.File.Exists(path) Then
                Dim registrykey As Microsoft.Win32.RegistryKey
                Console.WriteLine("[" & Now.ToString("HH:mm:ss") & "][INFO]: Retrieving HKEY_LOCAL_MACHINE From Registry")
                If Environment.Is64BitOperatingSystem = True Then
                    registrykey = Microsoft.Win32.RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, Microsoft.Win32.RegistryView.Registry64)
                Else
                    registrykey = Microsoft.Win32.RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, Microsoft.Win32.RegistryView.Registry32)
                End If

                If registrykey Is Nothing Then
                    Console.WriteLine("[" & Now.ToString("HH:mm:ss") & "][WARNING]: Failed To Retrieve HKEY_LOCAL_MACHINE From Registry")
                Else
                    Console.WriteLine("[" & Now.ToString("HH:mm:ss") & "][INFO]: Successfuly Retrieved HKEY_LOCAL_MACHINE From Registry")
                End If

                Console.WriteLine("[" & Now.ToString("HH:mm:ss") & "][INFO]: Opening PersonalizationSCP SubKey")

                Dim PersoCSPKey = registrykey.OpenSubKey("SOFTWARE\Microsoft\Windows\CurrentVersion\PersonalizationCSP", True)

                If PersoCSPKey Is Nothing Then
                    Console.WriteLine("[" & Now.ToString("HH:mm:ss") & "][INFO]: PersonalizationSCP SubKey Doesn't Exist, Creating One...")
                    PersoCSPKey = registrykey.CreateSubKey("SOFTWARE\Microsoft\Windows\CurrentVersion\PersonalizationCSP", Microsoft.Win32.RegistryKeyPermissionCheck.ReadWriteSubTree)
                End If

                If PersoCSPKey IsNot Nothing Then
                    Console.WriteLine("[" & Now.ToString("HH:mm:ss") & "][INFO]: Setting PersonalizationSCP SubKey Values...")
                    PersoCSPKey.SetValue("LockScreenImagePath", path)
                    PersoCSPKey.SetValue("LockScreenImageURL", path)
                    PersoCSPKey.SetValue("LockScreenImageStatus", 1, Microsoft.Win32.RegistryValueKind.DWord)
                End If
                Return True
            Else
                Console.WriteLine("[" & Now.ToString("HH:mm:ss") & "][WARNING]: File Doesn't Exist!")
            End If
            Return False
        Catch ex As Exception
            Console.WriteLine("[" & Now.ToString("HH:mm:ss") & "][DEBUG]: " & ex.ToString)
            Return False
        End Try

    End Function
    Public Shared Function ClearLockscreenWallpaper() As Boolean
        Try
            Dim registrykey As Microsoft.Win32.RegistryKey

            Console.WriteLine("[" & Now.ToString("HH:mm:ss") & "][INFO]: Retrieving HKEY_LOCAL_MACHINE From Registry")
            If Environment.Is64BitOperatingSystem = True Then
                registrykey = Microsoft.Win32.RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, Microsoft.Win32.RegistryView.Registry64)
            Else
                registrykey = Microsoft.Win32.RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, Microsoft.Win32.RegistryView.Registry32)
            End If

            If registrykey Is Nothing Then
                Console.WriteLine("[" & Now.ToString("HH:mm:ss") & "][WARNING]: Failed To Retrieve HKEY_LOCAL_MACHINE From Registry")
            Else
                Console.WriteLine("[" & Now.ToString("HH:mm:ss") & "][INFO]: Successfuly Retrieved HKEY_LOCAL_MACHINE From Registry")
            End If

            Console.WriteLine("[" & Now.ToString("HH:mm:ss") & "][INFO]: Deleting SubKey...")
            registrykey.DeleteSubKey("SOFTWARE\Microsoft\Windows\CurrentVersion\PersonalizationCSP", True)
            Console.WriteLine("[" & Now.ToString("HH:mm:ss") & "][INFO]: Successfuly Deleted SubKey")
            Return True
        Catch ex As Exception
            Console.WriteLine("[" & Now.ToString("HH:mm:ss") & "][DEBUG]: " & ex.ToString)
            Return False
        End Try
    End Function
#End Region
    Public Class WallpaperElement
        Public Property Name As String
        Public Property CoverLink As String
        Public Property WallpaperLink As String
        Public Property ThumbLink As String
        Public Property Resolution As String
        Public Property WallpaperDirectLink As String
        Public Property Wallpaper As System.Drawing.Image
        Public Property Thumb As System.Drawing.Image
        Public Property Cover As System.Drawing.Image
        Public Overrides Function ToString() As String
            Return "{Name:" & Name & ";Resoultion[From Server]:" & Resolution & "}"
        End Function
    End Class
End Class
