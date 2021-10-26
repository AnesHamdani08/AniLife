Imports AniLife.API
Public Class EntryEditor
    Public Shadows Property Tag As AniDB.AniDBClient.AnimeElement
    Public Property OriginalResult As AniLibrary.AnimeElement
    Public Property Result As AniLibrary.AnimeElement
    Public Sub New(WndO As Window, AnimeElement As AniDB.AniDBClient.AnimeElement, LibraryElement As AniLibrary.AnimeElement)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        WindowStartupLocation = WindowStartupLocation.CenterOwner
        Owner = WndO
        Tag = AnimeElement
        OriginalResult = LibraryElement.Clone
        Result = LibraryElement.Clone
        If AnimeElement.Picture IsNot Nothing Then
            Entry_Image.Source = Utils.ImageSourceFromBitmap(AnimeElement.Picture)
        End If
        Entry_Title.Text = AnimeElement.Titles.FirstOrDefault(Function(k) k.Type.ToLower = "main")?.Value
        Entry_Score.Value = LibraryElement.Score
        Entry_EpisodeProgress.Maximum = AnimeElement.EpisodeCount
        Entry_EpisodeProgress.Value = LibraryElement.EpisodeProgress
        Entry_Note.Text = LibraryElement.Note
        Entry_Status.Text = AniResolver.STATUS & Space(1) & LibraryElement.Status.ToString
    End Sub
    Public Sub New(AnimeElement As AniDB.AniDBClient.AnimeElement, LibraryElement As AniLibrary.AnimeElement)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Tag = AnimeElement
        Result = LibraryElement
        If AnimeElement.Picture IsNot Nothing Then
            Entry_Image.Source = Utils.ImageSourceFromBitmap(AnimeElement.Picture)
        End If
        Entry_Title.Text = AnimeElement.Titles.FirstOrDefault(Function(k) k.Type.ToLower = "main")?.Value
        Entry_Score.Value = LibraryElement.Score
        Entry_EpisodeProgress.Maximum = AnimeElement.EpisodeCount
        Entry_EpisodeProgress.Value = LibraryElement.EpisodeProgress
        Entry_Note.Text = LibraryElement.Note
        Entry_Status.Text = AniResolver.STATUS & Space(1) & LibraryElement.Status.ToString
    End Sub

    Private Sub Entry_Save_Click(sender As Object, e As RoutedEventArgs) Handles Entry_Save.Click
        Result.Score = Entry_Score.Value
        Result.EpisodeProgress = Entry_EpisodeProgress.Value
        Result.Note = Entry_Note.Text
        DialogResult = True
    End Sub

    Private Sub Entry_SetAs_Completed_Click(sender As Object, e As RoutedEventArgs) Handles Entry_SetAs_Completed.Click
        Result.Status = AniLibrary.Status.Completed
        Entry_Status.Text = AniResolver.STATUS & Space(1) & AniLibrary.Status.Completed.ToString
    End Sub

    Private Sub Entry_SetAs_Planning_Click(sender As Object, e As RoutedEventArgs) Handles Entry_SetAs_Planning.Click
        Result.Status = AniLibrary.Status.Planning
        Entry_Status.Text = AniResolver.STATUS & Space(1) & AniLibrary.Status.Planning.ToString
    End Sub

    Private Sub Entry_SetAs_Watching_Click(sender As Object, e As RoutedEventArgs) Handles Entry_SetAs_Watching.Click
        Result.Status = AniLibrary.Status.Watching
        Entry_Status.Text = AniResolver.STATUS & Space(1) & AniLibrary.Status.Watching.ToString
    End Sub

    Private Sub Search_SetAs_Dropped_Click(sender As Object, e As RoutedEventArgs) Handles Entry_SetAs_Dropped.Click
        Result.Status = AniLibrary.Status.Dropped
        Entry_Status.Text = AniResolver.STATUS & Space(1) & AniLibrary.Status.Dropped.ToString
    End Sub

    Private Sub Search_SetAs_Paused_Click(sender As Object, e As RoutedEventArgs) Handles Entry_SetAs_Paused.Click
        Result.Status = AniLibrary.Status.Paused
        Entry_Status.Text = AniResolver.STATUS & Space(1) & AniLibrary.Status.Paused.ToString
    End Sub

    Private Sub Entry_Close_Click(sender As Object, e As RoutedEventArgs) Handles Entry_Close.Click
        DialogResult = False
    End Sub
End Class
