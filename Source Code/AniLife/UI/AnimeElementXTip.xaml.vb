Imports AniLife.API
Public Class AnimeElementXTip
    Public Shadows Property Tag As Object
    Public Sub New(AnimeElement As AniDB.AniDBClient.RecommendationElement)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Tag = AnimeElement
        If AnimeElement IsNot Nothing Then
            AirDate.Text = Utils.DateToSeason(AnimeElement.StartDate) & Space(1) & AnimeElement.StartDate.Year
            Score.Text = AnimeElement.Ratings.FirstOrDefault(Function(k) k.Type = AniDB.AniDBClient.RatingElement.RatingType.Parmanent)?.Value
            StudioName.Text = AniResolver.NOTAVAILABLE
            TypexEpisodes.Text = AnimeElement.Type.ToString & " . " & AnimeElement.EpisodeCount & Space(1) & AniResolver.EPISODE
        End If
    End Sub
    Public Sub New(AnimeElement As AniDB.AniDBClient.AnimeElement)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Tag = AnimeElement
        If AnimeElement IsNot Nothing Then
            AirDate.Text = Utils.DateToSeason(AnimeElement.StartDate) & Space(1) & AnimeElement.StartDate.Year
            Dim XScore = AnimeElement.Ratings.FirstOrDefault(Function(k) k.Type = AniDB.AniDBClient.RatingElement.RatingType.Parmanent)
            Score.Text = If(XScore IsNot Nothing, XScore.Value, Nothing)
            Dim XStudio = AnimeElement.Creators.FirstOrDefault(Function(k) k.Type.ToLower = "animation work")
            StudioName.Text = If(XStudio IsNot Nothing, XStudio.Value, Nothing)
            TypexEpisodes.Text = AnimeElement.Type.ToString & " . " & AnimeElement.EpisodeCount & Space(1) & AniResolver.EPISODE
            For Each animetag In AnimeElement.Tags
                Tags.Children.Add(New Button With {.Content = animetag.Name, .Margin = New Thickness(10, 10, 0, 0)})
            Next
        End If
    End Sub
    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub
End Class
