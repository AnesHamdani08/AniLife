Imports Newtonsoft
Public Class YoutubeDL
        Public Property YoutubeDLLocation As String
        Private Property UniversalSplitter As Char() = New Char() {"\r", "\n", "\r\n"}
        Public Class YoutubeVideo
            Public Property Title As String
            Public Property FullTitle As String
            Public Property Author As String
            Public Property URL As String
            Public Property DirectURL As String
            Public Property RequestQuality As Quality
            Public Property AlternateTitle As String
            Public Property Duration As TimeSpan
            Public Property AuthorID As String
            Public Property VideoCodec As String
            Public Property ViewCount As Integer
            Public Property VideoExtension As String
            Public Property ThumbnailURL As String
            Public Property UploadDate As Date
            Public Property AuthorURL As String
            Public Property AudioCodec As String
            Public Property Album As String
            Public Property Tags As New List(Of String)
            Public Property Track As String
            Public Property Artist As String
            Public Property Artists As String()
            Public Property Thumbnails As New List(Of Thumbnail)
            Public Property HQThumbnail As Thumbnail
            Public Property LQThumbnail As Thumbnail
            Public Property Categories As String()
            Public Property Description As String
            Public Property DislikeCount As Integer
            Public Property FileName As String
            Public Property LikeCount As Integer
            Public Property Creator As String
            Public Property FPS As Integer
            Public Property Subtitles As New List(Of List(Of Subtitle))
            Public Property Videos As New List(Of Video)
            Public Property HQVideo As Video
            Public Property LQVideo As Video
            Public Property HQAudio As Video
            Public Property LQAudio As Video
            Public Property HQMixed As Video
            Public Property LQMixed As Video
            Public Property AudioOnly As New List(Of Video)
            Public Property VideoOnly As New List(Of Video)
            Public Property MixedOnly As New List(Of Video)
            Public Class Thumbnail
                Public Property ID As Integer
                Public Property URL As String
                Public Property Width As Integer
                Public Property Height As Integer
                Public Property Resolution As String
            End Class
            Public Class Subtitle
                Public Property Language As String
                Public Property RawData As String
                Public Property Extension As String
            End Class
            Public Class Video
                Public Property VBR As String
                Public Property ASR As String
                Public Property TBR As String
                Public Property ABR As String
                Public Property Height As String
                Public Property Quality As String
                Public Property Extension As String
                Public Property FileSize As String
                Public Property Container As String
                Public Property FPS As String
                Public Property Format As String
                Public Property HTTPHeaders As HTTPHeader
                Public Property Width As String
                Public Property Protocol As String
                Public Property FormatID As String
                Public Property FormatNote As String
                Public Property AudioCodec As String
                Public Property VideoCodec As String
                Public Property DirectURL As String
                Public Property Type As FileType
                Public Enum FileType
                    Audio
                    Video
                    Mixed
                End Enum
                Public Class HTTPHeader
                    Public Property AcceptLanguage As String
                    Public Property AcceptCharset As String
                    Public Property UserAgent As String
                    Public Property AcceptEncoding As String
                    Public Property Accept As String
                End Class
            End Class
            Public Enum Quality
                best
                worst
                bestvideo
                worstvideo
                bestaudio
                worstaudio
            End Enum
            Public Shadows Function ToString() As String
                Return Title & Environment.NewLine & Author & Environment.NewLine & URL & Environment.NewLine & DirectURL & Environment.NewLine & RequestQuality.ToString
            End Function
        End Class
        Public Class Resolution
            Public Property Height As Integer
            Public Property Width As Integer
            Public Sub New(H As Integer, W As Integer)
                Height = H
                Width = W
            End Sub
        End Class
        Public Sub New(YTDLPath As String)
            If IO.File.Exists(YTDLPath) Then
                YoutubeDLLocation = YTDLPath
            Else
                Throw New ArgumentNullException("YoutubeDL doesn't exists in the given path.")
            End If
        End Sub
        Public Function IsYoutubeLink(URL As String) As Boolean
            '?v=
            If URL.Contains("?v=") Then Return True
            'youtu.be
            If URL.ToLower.Contains("youtu.be") Then Return True 'https://youtu.be/PKfxmFU3lWY?list=RDPKfxmFU3lWY
            Return False
        End Function
    Public Async Function GetVideo(URL As String, Q As YoutubeVideo.Quality) As Task(Of YoutubeVideo)
        If Utils.CheckInternetConnection Then
            Return Await Task.Run(Function()
                                      Dim YoutubeDL As Process = Process.Start(New ProcessStartInfo(YoutubeDLLocation, "-s -e -g -f " & Q.ToString & Space(1) & URL) With {.RedirectStandardOutput = True, .UseShellExecute = False, .WindowStyle = ProcessWindowStyle.Hidden, .CreateNoWindow = True})
                                      Dim Info = YoutubeDL.StandardOutput.ReadToEnd.Split(New Char() {vbCr, vbCrLf, vbLf})
                                      Dim Vid As New YoutubeVideo With {.Title = Info(0), .URL = URL, .DirectURL = Info(1), .RequestQuality = Q}
                                      Return Vid
                                  End Function)
        Else
            Return Nothing
        End If
    End Function
    Public Async Function SearchVideo(Query As String) As Task(Of String)
        If Utils.CheckInternetConnection Then
            Return Await Task.Run(Function()
                                      Dim YoutubeDL As Process = Process.Start(New ProcessStartInfo(YoutubeDLLocation, """ytsearch:" & Query & """ --get-id") With {.RedirectStandardOutput = True, .UseShellExecute = False, .WindowStyle = ProcessWindowStyle.Hidden, .CreateNoWindow = True})
                                      Return YoutubeDL.StandardOutput.ReadToEnd
                                  End Function)
        Else
            Return Nothing
        End If
    End Function
    Public Async Function RequestAndDumpInfo(URL As String, Optional SkipURLCheck As Boolean = False) As Task(Of YoutubeVideo)
        If Utils.CheckInternetConnection Then
            If SkipURLCheck = False Then
                If IsYoutubeLink(URL) = False Then Return Nothing
            End If
            Dim YoutubeDL As Process = Process.Start(New ProcessStartInfo(YoutubeDLLocation, "-j " & URL) With {.RedirectStandardOutput = True, .UseShellExecute = False, .WindowStyle = ProcessWindowStyle.Hidden, .CreateNoWindow = True})
            Dim Info = YoutubeDL.StandardOutput.ReadToEnd
            If String.IsNullOrEmpty(Info.Trim) Then Throw New InvalidOperationException
            'Acquiring Info        
            Dim ParsedInfo = Json.Linq.JObject.Parse(Info)
            Dim FullTitle = ParsedInfo("fulltitle")
            Dim Album = ParsedInfo("album")
            Dim AltTitle = ParsedInfo("alt_title")
            Dim Artist = ParsedInfo("artist")
            Dim Artists As String()
            Try
                Artists = ParsedInfo("artist").ToString.Split(",")
            Catch ex As Exception
                Artists = {}
            End Try
            Dim AudioCodec = ParsedInfo("acodec")
            Dim Author = ParsedInfo("channel")
            Dim AuhorID = ParsedInfo("channel_id")
            Dim AuthorURL = ParsedInfo("channel_url")
            Dim RawCategories = ParsedInfo("categories")
            Dim Categories = String.Join(Environment.NewLine, RawCategories).Split(UniversalSplitter)
            Dim Creator = ParsedInfo("creator")
            Dim Description = ParsedInfo("description")
            Dim RawDirectURLS = ParsedInfo("formats")
            'Dim RawDirectURLS = ParsedInfo("requested_formats")
            Dim DirectURLS As New List(Of YoutubeVideo.Video)
            Dim AudioOnlyURLS As New List(Of YoutubeVideo.Video)
            Dim VideoOnlyURLS As New List(Of YoutubeVideo.Video)
            Dim MixedOnlyURLS As New List(Of YoutubeVideo.Video)
            For Each RURL As Json.Linq.JToken In RawDirectURLS
                Dim CVideo As New YoutubeVideo.Video
                CVideo.AudioCodec = RURL("acodec")
                CVideo.Container = RURL("container")
                CVideo.DirectURL = RURL("url")
                CVideo.Extension = RURL("ext")
                CVideo.FileSize = RURL("filesize")
                CVideo.Format = RURL("format")
                CVideo.FormatID = RURL("format_id")
                CVideo.FormatNote = RURL("format_note")
                CVideo.FPS = RURL("fps")
                CVideo.Height = RURL("height")
                Dim RawHTTPHeader = RURL("http_headers")
                Dim HTTPHeader As New YoutubeVideo.Video.HTTPHeader
                HTTPHeader.Accept = RawHTTPHeader("Accept")
                HTTPHeader.AcceptCharset = RawHTTPHeader("Accept-Charset")
                HTTPHeader.AcceptEncoding = RawHTTPHeader("Accept-Encoding")
                HTTPHeader.AcceptLanguage = RawHTTPHeader("Accept-Language")
                HTTPHeader.UserAgent = RawHTTPHeader("User-Agent")
                CVideo.HTTPHeaders = HTTPHeader
                CVideo.Protocol = RURL("protocol")
                CVideo.Quality = RURL("quality")
                CVideo.VideoCodec = RURL("vcodec")
                CVideo.Width = RURL("width")
                If CVideo.AudioCodec <> "none" AndAlso CVideo.VideoCodec <> "none" Then
                    CVideo.Type = YoutubeVideo.Video.FileType.Mixed
                ElseIf CVideo.AudioCodec <> "none" AndAlso CVideo.VideoCodec = "none" Then
                    CVideo.Type = YoutubeVideo.Video.FileType.Audio
                ElseIf CVideo.AudioCodec = "none" AndAlso CVideo.VideoCodec <> "none" Then
                    CVideo.Type = YoutubeVideo.Video.FileType.Video
                End If
                Select Case CVideo.Type
                    Case YoutubeVideo.Video.FileType.Mixed
                        CVideo.VBR = RURL("vbr")
                        CVideo.ASR = RURL("asr")
                        CVideo.TBR = RURL("tbr")
                        CVideo.ABR = RURL("abr")
                        MixedOnlyURLS.Add(CVideo)
                    Case YoutubeVideo.Video.FileType.Audio
                        CVideo.ASR = RURL("asr")
                        CVideo.TBR = RURL("tbr")
                        CVideo.ABR = RURL("abr")
                        AudioOnlyURLS.Add(CVideo)
                    Case YoutubeVideo.Video.FileType.Video
                        CVideo.VBR = RURL("vbr")
                        CVideo.ASR = RURL("asr")
                        CVideo.TBR = RURL("tbr")
                        VideoOnlyURLS.Add(CVideo)
                End Select
                DirectURLS.Add(CVideo)
            Next
            AudioOnlyURLS.OrderBy(Function(k) k.FileSize)
            VideoOnlyURLS.OrderBy(Function(k) k.Width)
            MixedOnlyURLS.OrderBy(Function(k) k.Width)
            Dim HQAudio = AudioOnlyURLS.Last
            Dim LQAudio = AudioOnlyURLS.First
            Dim HQVideo = VideoOnlyURLS.Last
            Dim LQVideo = VideoOnlyURLS.First
            Dim HQMixed = MixedOnlyURLS.Last
            Dim LQMixed = MixedOnlyURLS.First
            Dim DislikeCount = ParsedInfo("dislike_count")
            Dim Duration = TimeSpan.FromSeconds(ParsedInfo("duration"))
            Dim FileName = ParsedInfo("_filename")
            Dim FPS = ParsedInfo("fps")
            Dim LikeCount = ParsedInfo("like_count")
            Dim RawSubtitles = ParsedInfo("subtitles")
            Dim Subtitles As New List(Of List(Of YoutubeVideo.Subtitle))
            If RawSubtitles IsNot Nothing Then
                For Each Subtitle As Json.Linq.JToken In RawSubtitles
                    Dim CSubtitle As New List(Of YoutubeVideo.Subtitle)
                    For Each _Subtitle As Json.Linq.JToken In Subtitle
                        For Each __Subtitle As Json.Linq.JToken In _Subtitle
                            Dim CCSubtitle As New YoutubeVideo.Subtitle
                            CCSubtitle.Language = CType(Subtitle, Json.Linq.JProperty).Name
                            CCSubtitle.Extension = __Subtitle("ext") 'String.Join(Environment.NewLine, _Subtitle("ext")).Split(UniversalSplitter) '_Subtitle("ext")
                            CCSubtitle.RawData = __Subtitle("url")
                            CSubtitle.Add(CCSubtitle)
                        Next
                    Next
                    Subtitles.Add(CSubtitle)
                Next
            End If
            Dim RawTags = ParsedInfo("tags")
            Dim Tags As New List(Of String)
            For Each Tag In RawTags
                Tags.Add(CType(Tag, Json.Linq.JValue).Value)
            Next
            Dim RawThumbnails = ParsedInfo("thumbnails")
            Dim Thumbnails As New List(Of YoutubeVideo.Thumbnail)
            For Each Thumbnail As Json.Linq.JToken In RawThumbnails
                Dim CThumbnail As New YoutubeVideo.Thumbnail
                CThumbnail.ID = Thumbnail("id")
                CThumbnail.Resolution = Thumbnail("resolution")
                CThumbnail.URL = Thumbnail("url")
                CThumbnail.Height = Thumbnail("height")
                CThumbnail.Width = Thumbnail("width")
                Thumbnails.Add(CThumbnail)
            Next
            Thumbnails.OrderBy(Function(k) k.Width)
            Dim HQThumbnail = Thumbnails.Last
            Dim LQThumbnail = Thumbnails.First
            Dim ThumbnailURL = ParsedInfo("thumbnail")
            Dim Track = ParsedInfo("track")
            Dim Title = ParsedInfo("title")
            Dim RawUploadDate = ParsedInfo("upload_date")
            Dim UploadDate As New Date(RawUploadDate.ToString.Substring(0, 4), RawUploadDate.ToString.Substring(4, 2), RawUploadDate.ToString.Substring(6, 2))
            Dim VideoCodec = ParsedInfo("vcodec")
            Dim VideoExtension = ParsedInfo("ext")
            Dim ViewCount = ParsedInfo("view_count")
            'Returning Info
            Return Await Task.FromResult(New YoutubeVideo With {.Album = Album, .AlternateTitle = AltTitle, .ThumbnailURL = ThumbnailURL, .Artist = Artist, .Artists = Artists, .AudioCodec = AudioCodec, .Author = Author, .AuthorID = AuhorID, .AuthorURL = AuthorURL, .Categories = Categories, .Creator = Creator, .Description = Description, .DislikeCount = DislikeCount, .Duration = Duration, .FileName = FileName, .FPS = FPS, .FullTitle = FullTitle, .LikeCount = LikeCount, .Subtitles = Subtitles, .Tags = Tags, .Thumbnails = Thumbnails, .Title = Title, .Track = Track, .UploadDate = UploadDate, .URL = URL, .VideoCodec = VideoCodec, .VideoExtension = VideoExtension, .ViewCount = ViewCount, .Videos = DirectURLS, .HQAudio = HQAudio, .HQMixed = HQMixed, .HQThumbnail = HQThumbnail, .HQVideo = HQVideo, .LQAudio = LQAudio, .LQMixed = LQMixed, .LQThumbnail = LQThumbnail, .LQVideo = LQVideo, .AudioOnly = AudioOnlyURLS, .VideoOnly = VideoOnlyURLS, .MixedOnly = MixedOnlyURLS})
        Else
            Return Nothing
        End If
    End Function
End Class