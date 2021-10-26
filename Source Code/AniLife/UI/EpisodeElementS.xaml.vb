Public Class EpisodeElementS
    Public Shadows Property Tag As API.AniDB.AniDBClient.EpisodeElement
    Public Property AID As Integer
    Public Property FilePath
    Public ReadOnly Property IsFileExists As Boolean
        Get
            Return IO.File.Exists(FilePath)
        End Get
    End Property
    Public Sub New(ee As API.AniDB.AniDBClient.EpisodeElement, _AID As Integer, Optional Local As Boolean = False, Optional AnimePath As String = Nothing)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Tag = ee
        AID = _AID
        If ee IsNot Nothing Then
            Episode_Title.Text = ee.Title
            Episode_Num.Text = ee.Number
            Episode_Length.Text = Utils.TimeToString(ee.Length * 60)
            Episode_Rating.Text = ee.Rating.Value
            Episode_Rating.ToolTip = ee.Rating.Count & Space(1) & AniResolver.USER
            For Each res In ee.Resources
                For Each url In res.URL.URLs
                    Dim ResIG As New Image With {.Height = 32, .Stretch = Stretch.Uniform, .Source = Utils.ResourceTypeToInternalImage(res.Type), .ToolTip = res.Type.ToString, .Margin = New Thickness(0, 0, 10, 0)}
                    AddHandler ResIG.MouseLeftButtonDown, Sub(sender As Object, e As MouseButtonEventArgs)
                                                              If e.ClickCount >= 2 Then
                                                                  Process.Start(url)
                                                              End If
                                                          End Sub
                    Episode_Resources.Children.Add(ResIG)
                Next
            Next
        End If
        If Local Then
            If IO.Directory.Exists(AnimePath) Then
                Dim files = IO.Directory.GetFiles(AnimePath, ee.Number & ".*")
                If files.Length = 0 Then
                    files = IO.Directory.GetFiles(AnimePath, "0" & ee.Number & ".*")
                    If files.Length = 0 Then
                        files = IO.Directory.GetFiles(AnimePath, "00" & ee.Number & ".*")
                        If files.Length = 0 Then
                            files = IO.Directory.GetFiles(AnimePath, "000" & ee.Number & ".*")
                        End If
                    End If
                End If
                If files.Length > 0 Then
                    FilePath = files(0)
                    Episode_Resources_Local.Visibility = Visibility.Visible
                End If
            End If
        End If
    End Sub
    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub

    Private Sub Episode_Resources_Local_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles Episode_Resources_Local.MouseLeftButtonDown
        If e.ClickCount >= 2 Then
            If IsFileExists Then MainWindow.LoadingStateHelper.OpenVideo(FilePath)
        End If
    End Sub
    Private Sub OpenWith(ByVal filePath As String)
        Dim psi As New ProcessStartInfo("rundll32.exe")
        With psi
            .Arguments = String.Format("shell32.dll,OpenAs_RunDLL {0}", filePath)
        End With
        Process.Start(psi)
    End Sub

    Private Sub Episode_Resources_Local_OpenWith_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles Episode_Resources_Local_OpenWith.MouseLeftButtonDown
        If e.ClickCount >= 2 Then
            If IsFileExists Then OpenWith(FilePath)
        End If
    End Sub
End Class
