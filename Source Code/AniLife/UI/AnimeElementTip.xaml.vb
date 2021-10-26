Public Class AnimeElementTip
    Public Property AnimeName As String
        Get
            Return TB_AnimeName.Text
        End Get
        Set(value As String)
            TB_AnimeName.Text = value
        End Set
    End Property
    Private _EpProgress As Integer
    Public Property EpisodeProgress As Integer
        Get
            Return _EpProgress
        End Get
        Set(value As Integer)
            _EpProgress = value
            TB_AnimeProg.Text = AniResolver.PROGRESS & ": " & value & "/" & EpisodeCount
        End Set
    End Property
    Private _EpCount As Integer
    ''' <summary>
    ''' Call Episode Progress First
    ''' </summary>
    ''' <returns></returns>
    Public Property EpisodeCount As Integer
        Get
            Return _EpCount
        End Get
        Set(value As Integer)
            _EpCount = value
            TB_AnimeProg.Text = AniResolver.PROGRESS & ": " & EpisodeProgress & "/" & value
        End Set
    End Property
    Public Sub New(AniName As String, EpProgress As Integer, EpCount As Integer)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        AnimeName = AniName
        EpisodeProgress = EpProgress
        EpisodeCount = EpCount
    End Sub
    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub
End Class
