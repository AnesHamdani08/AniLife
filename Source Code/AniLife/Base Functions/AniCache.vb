Imports AniLife.API
''' <summary>
''' Provides a set of methods and function to manager local anime chache, This class requires AniDB
''' </summary>
Public Class AniCache
    Public Const Version As String = "0.0.0.1-Beta.3"
    Private Property _BaseAnimeDocument As XDocument
    Public ReadOnly Property BaseAnimeDocument As XDocument
        Get
            Return _BaseAnimeDocument
        End Get
    End Property
    Private Property _CacheLocation As String
    ''' <summary>
    ''' Directory that contains the cache files
    ''' </summary>
    ''' <returns></returns>
    Public Property CacheLocation As String
        Get
            Return _CacheLocation
        End Get
        Set(value As String)
            BaseAnimeDocument.Root.FirstAttribute.Value = value
            _CacheLocation = value
            If AutoSave Then Save()
        End Set
    End Property
    Public Property AutoSave As Boolean = True
    Public ReadOnly Property AnimeCount As Integer
        Get
            Return If(BaseAnimeDocument IsNot Nothing, TryCast(BaseAnimeDocument.Root.FirstNode, XElement)?.Nodes.Count, 0)
        End Get
    End Property
    Public ReadOnly Property ThumbCount As Integer
        Get
            Return IO.Directory.GetFiles(IO.Path.Combine(CacheLocation, "Thumbs")).Length
        End Get
    End Property
    Public Shared Async Function MakeCache(Location As String) As Task(Of XDocument)
        If IO.Directory.Exists(Location) Then
            Return Await Task.Run(Function()
                                      Dim XCache As New XDocument
                                      Dim Root As New XElement("AniLife", New XAttribute("Location", Location))
                                      Root.Add(New XElement("Animes"))
                                      XCache.Add(Root)
                                      Return XCache
                                  End Function)
        Else
            Return Await Task.Run(Function()
                                      Dim XCache As New XDocument
                                      Dim Root As New XElement("AniLife", New XAttribute("Location", IO.Directory.CreateDirectory(Location).FullName))
                                      Root.Add(New XElement("Animes"))
                                      XCache.Add(Root)
                                      Return XCache
                                  End Function)
        End If
    End Function
    Public Sub New()

    End Sub
    Public Sub New(Cache As XDocument)
        If Cache.Root.Name = "AniLife" Then
            _CacheLocation = Cache.Root.FirstAttribute?.Value
            If Not IO.Directory.Exists(CacheLocation) Then
                IO.Directory.CreateDirectory(CacheLocation)
            End If
            If Not IO.Directory.Exists(IO.Path.Combine(CacheLocation, "Animes")) Then
                IO.Directory.CreateDirectory(IO.Path.Combine(CacheLocation, "Animes"))
            End If
            If Not IO.Directory.Exists(IO.Path.Combine(CacheLocation, "Thumbs")) Then
                IO.Directory.CreateDirectory(IO.Path.Combine(CacheLocation, "Thumbs"))
            End If
            _BaseAnimeDocument = Cache
        End If
    End Sub
    Public Sub New(File As String)
        If IO.File.Exists(File) Then
            Dim XCache = XDocument.Load(File)
            If XCache.Root.Name = "AniLife" Then
                _CacheLocation = XCache.Root.FirstAttribute?.Value
                If Not IO.Directory.Exists(CacheLocation) Then
                    IO.Directory.CreateDirectory(CacheLocation)
                End If
                If Not IO.Directory.Exists(IO.Path.Combine(CacheLocation, "Animes")) Then
                    IO.Directory.CreateDirectory(IO.Path.Combine(CacheLocation, "Animes"))
                End If
                If Not IO.Directory.Exists(IO.Path.Combine(CacheLocation, "Thumbs")) Then
                    IO.Directory.CreateDirectory(IO.Path.Combine(CacheLocation, "Thumbs"))
                End If
                _BaseAnimeDocument = XCache
            End If
        End If
    End Sub
    Public Sub Load(Cache As XDocument)
        If Cache.Root.Name = "AniLife" Then
            _CacheLocation = Cache.Root.FirstAttribute?.Value
            If Not IO.Directory.Exists(CacheLocation) Then
                IO.Directory.CreateDirectory(CacheLocation)
            End If
            If Not IO.Directory.Exists(IO.Path.Combine(CacheLocation, "Animes")) Then
                IO.Directory.CreateDirectory(IO.Path.Combine(CacheLocation, "Animes"))
            End If
            If Not IO.Directory.Exists(IO.Path.Combine(CacheLocation, "Thumbs")) Then
                IO.Directory.CreateDirectory(IO.Path.Combine(CacheLocation, "Thumbs"))
            End If
            _BaseAnimeDocument = Cache
        End If
    End Sub
    Public Sub Load(File As String)
        If IO.File.Exists(File) Then
            Dim XCache = XDocument.Load(File)
            If XCache.Root.Name = "AniLife" Then
                _CacheLocation = XCache.Root.FirstAttribute?.Value
                If Not IO.Directory.Exists(CacheLocation) Then
                    IO.Directory.CreateDirectory(CacheLocation)
                End If
                If Not IO.Directory.Exists(IO.Path.Combine(CacheLocation, "Animes")) Then
                    IO.Directory.CreateDirectory(IO.Path.Combine(CacheLocation, "Animes"))
                End If
                If Not IO.Directory.Exists(IO.Path.Combine(CacheLocation, "Thumbs")) Then
                    IO.Directory.CreateDirectory(IO.Path.Combine(CacheLocation, "Thumbs"))
                End If
                _BaseAnimeDocument = XCache
            End If
        End If
    End Sub
    Public Function Save() As String
        BaseAnimeDocument.Save(IO.Path.Combine(CacheLocation, "Cache.xml"))
        Return IO.Path.Combine(CacheLocation, "Cache.xml")
    End Function
    Public Function Size() As Long
        Dim CacheInfo As New IO.FileInfo(IO.Path.Combine(CacheLocation, "Cache.xml"))
        Return CacheInfo.Length
    End Function
    Public Async Sub AddToCache(AnimeElement As AniDB.AniDBClient.AnimeElement)
        If AnimeElement IsNot Nothing Then
            If CType(BaseAnimeDocument.Root.FirstNode, XElement).Nodes.FirstOrDefault(Function(k) CType(k, XElement).Value = AnimeElement.ID) Is Nothing Then
                CType(BaseAnimeDocument.Root.FirstNode, XElement).Add(New XElement("Anime") With {.Value = AnimeElement.ID})
                If IO.Directory.Exists(CacheLocation) Then
                    If Not IO.Directory.Exists(IO.Path.Combine(CacheLocation, "Animes")) Then IO.Directory.CreateDirectory(IO.Path.Combine(CacheLocation, "Animes"))
                    If Not IO.Directory.Exists(IO.Path.Combine(CacheLocation, "Thumbs")) Then IO.Directory.CreateDirectory(IO.Path.Combine(CacheLocation, "Thumbs"))
                    AnimeElement.SaveToFile(IO.Path.Combine(CacheLocation, "Animes", AnimeElement.ID & ".xml"))
                    If AnimeElement.Picture IsNot Nothing Then
                        AnimeElement.Picture.Save(IO.Path.Combine(CacheLocation, "Thumbs", AnimeElement.ID & ".png"), System.Drawing.Imaging.ImageFormat.Png)
                    End If
                    For Each Character In AnimeElement.Characters
                        Try
                            If Character.Picture Is Nothing Then
                                Dim Image = Await AniDB.AniDBClient.SharedFunctions.GetAniDBImageAsync(Character.ID)
                                Image.Save(IO.Path.Combine(CacheLocation, "Thumbs", Character.ID & ".png"), System.Drawing.Imaging.ImageFormat.Png)
                            Else
                                Character.Picture.Save(IO.Path.Combine(CacheLocation, "Thumbs", Character.ID & ".png"), System.Drawing.Imaging.ImageFormat.Png)
                            End If
                        Catch
                        End Try
                    Next
                End If
                If AutoSave Then Save()
            End If
        End If
    End Sub
    Public Sub RemoveFromCache(AnimeElement As AniDB.AniDBClient.AnimeElement)
        If AnimeElement IsNot Nothing Then
            Dim Item = CType(BaseAnimeDocument.Root.FirstNode, XElement).Nodes.FirstOrDefault(Function(k) CType(k, XElement).Value = AnimeElement.ID)
            If Item IsNot Nothing Then
                Item.Remove()
                If IO.File.Exists(IO.Path.Combine(CacheLocation, "Animes", AnimeElement.ID & ".xml")) Then IO.File.Delete(IO.Path.Combine(CacheLocation, "Animes", AnimeElement.ID & ".xml"))
                If IO.File.Exists(IO.Path.Combine(CacheLocation, "Thumbs", AnimeElement.ID & ".png")) Then IO.File.Delete(IO.Path.Combine(CacheLocation, "Thumbs", AnimeElement.ID & ".png"))
                For Each Character In AnimeElement.Characters
                    If IO.File.Exists(IO.Path.Combine(CacheLocation, "Thumbs", Character.ID & ".png")) Then IO.File.Delete(IO.Path.Combine(CacheLocation, "Thumbs", Character.ID & ".png"))
                Next
                If AutoSave Then Save()
            End If
        End If
    End Sub
    Public Sub ClearCache()
        CType(BaseAnimeDocument.Root.FirstNode, XElement).RemoveNodes()
        If AutoSave Then Save()
    End Sub
    Public Sub UpdateCache(AnimeElement As AniDB.AniDBClient.AnimeElement)
        If AnimeElement IsNot Nothing Then
            If CheckIfAnimeExists(AnimeElement.ID) Then
                RemoveFromCache(AnimeElement)
            End If
            AddToCache(AnimeElement)
        End If
    End Sub
    ''' <summary>
    ''' Checks if Anime exists then saves or overwrites the picture
    ''' </summary>
    ''' <param name="ID"></param>
    ''' <param name="Picture"></param>
    Public Sub UpdateCachePicture(ID As Integer, Picture As System.Drawing.Image)
        If Picture IsNot Nothing Then
            If CheckIfAnimeExists(ID) Then
                Picture.Save(IO.Path.Combine(CacheLocation, "Thumbs", ID & ".png"), System.Drawing.Imaging.ImageFormat.Png)
            End If
        End If
    End Sub
    ''' <summary>
    ''' Saves or overwrites the picture
    ''' </summary>
    ''' <param name="ID"></param>
    ''' <param name="Picture"></param>
    Public Sub AddPictureToCache(ID As String, Picture As System.Drawing.Image)
        If Picture IsNot Nothing Then
            Try
                Picture.Save(IO.Path.Combine(CacheLocation, "Thumbs", ID & ".png"), System.Drawing.Imaging.ImageFormat.Png)
            Catch
            End Try
        End If
    End Sub
    Public Function CheckIfAnimeExists(ID As Integer) As Boolean
        Return If(CType(BaseAnimeDocument.Root.FirstNode, XElement).Nodes.FirstOrDefault(Function(k) CType(k, XElement).Value = ID) IsNot Nothing, True, False)
    End Function
    Public Async Function GetAnime(ID As Integer, Optional PreloadPicture As Boolean = True, Optional ImageScalingFactor As Integer = 1, Optional LoadCharacters As Boolean = True, Optional LoadCharactersImages As Boolean = True) As Task(Of AniDB.AniDBClient.AnimeElement)
        Return Await Task.Run(Async Function()
                                  Dim Item = CType(BaseAnimeDocument.Root.FirstNode, XElement).Nodes.FirstOrDefault(Function(k) CType(k, XElement).Value = ID)
                                  If Item IsNot Nothing Then
                                      Dim Anime = Await AniDB.AniDBClient.SharedFunctions.ParseAnimeAniDBXML(XDocument.Load(IO.Path.Combine(CacheLocation, "Animes", ID & ".xml")), False, LoadCharacters, False)
                                      If PreloadPicture Then
                                          If PreloadPicture Then
                                              Anime.Picture = GetPicture(ID, ImageScalingFactor)
                                          End If
                                          If LoadCharactersImages Then
                                              For Each character In Anime.Characters
                                                  character.Picture = GetPicture(character.ID, ImageScalingFactor)
                                              Next
                                          End If
                                      End If
                                      Return Anime
                                  End If
                                  Return Nothing
                              End Function)
    End Function
    Public Function GetXAnime(ID As Integer) As XDocument
        Dim Item = CType(BaseAnimeDocument.Root.FirstNode, XElement).Nodes.FirstOrDefault(Function(k) CType(k, XElement).Value = ID)
        If Item IsNot Nothing Then
            Return XDocument.Load(IO.Path.Combine(CacheLocation, "Animes", ID & ".xml"))
        Else
            Return Nothing
        End If
    End Function
    Public Function GetPicture(ID As Integer, Optional ScalingFactor As Integer = 1) As System.Drawing.Image
        Try
            Return Utils.ResizeImage(System.Drawing.Image.FromFile(IO.Path.Combine(CacheLocation, "Thumbs", ID & ".png")), ScalingFactor)
        Catch
        End Try
        Return Nothing
    End Function
    Public Async Function GetAnimes(Optional PreloadPicture As Boolean = True, Optional ImageScalingFactor As Integer = 1, Optional LoadCharacters As Boolean = True, Optional LoadCharactersImages As Boolean = True) As Task(Of IEnumerable(Of AniDB.AniDBClient.AnimeElement))
        Dim AniList As New List(Of AniDB.AniDBClient.AnimeElement) 'XD
        Await Task.Run(Async Function()
                           For Each anime As XElement In CType(BaseAnimeDocument.Root.FirstNode, XElement).Nodes
                               Dim _Anime = Await GetAnime(anime.Value, PreloadPicture, ImageScalingFactor, LoadCharacters, LoadCharactersImages)
                               AniList.Add(_Anime)
                           Next
                       End Function)
        Return AniList
    End Function
    Public Async Function GetAnimes(Start As Integer, [End] As Integer, Optional PreloadPicture As Boolean = True, Optional ImageScalingFactor As Integer = 1, Optional LoadCharacters As Boolean = True, Optional LoadCharactersImages As Boolean = True) As Task(Of IEnumerable(Of AniDB.AniDBClient.AnimeElement))
        If CType(BaseAnimeDocument.Root.FirstNode, XElement).Nodes.Count > 0 Then
            Dim AniList As New List(Of AniDB.AniDBClient.AnimeElement) 'XD
            Await Task.Run(Async Function()
                               Dim XNodes = CType(BaseAnimeDocument.Root.FirstNode, XElement).Nodes
                               If Utils.IsInRange(Start, 0, AnimeCount, True, True) AndAlso Utils.IsInRange([End], 0, AnimeCount, True, True) AndAlso Start < [End] Then
                                   For i As Integer = Start To [End]
                                       Dim _Anime = Await GetAnime(CType(XNodes(i), XElement).Value, PreloadPicture, ImageScalingFactor, LoadCharacters, LoadCharactersImages)
                                       AniList.Add(_Anime)
                                   Next
                               ElseIf Utils.IsInRange(Start, 0, AnimeCount, True, True) AndAlso Not Utils.IsInRange([End], 0, AnimeCount, True, True) Then
                                   For i As Integer = Start To XNodes.Count - 1
                                       Dim _Anime = Await GetAnime(CType(XNodes(i), XElement).Value, PreloadPicture, ImageScalingFactor, LoadCharacters, LoadCharactersImages)
                                       AniList.Add(_Anime)
                                   Next
                               End If
                           End Function)
            Return AniList
        Else
            Return New List
        End If
    End Function
    Public Async Function GetIDs() As Task(Of List(Of Integer))
        Return Await Task.Run(Function()
                                  Dim IDList As New List(Of Integer)
                                  For Each anime As XElement In CType(BaseAnimeDocument.Root.FirstNode, XElement).Nodes
                                      IDList.Add(anime.Value)
                                  Next
                                  Return IDList
                              End Function)
    End Function
    Public Async Function DistinctCache() As Task
        Await Task.Run(Async Function()
                           Dim AnimeFiles = Await GetIDs()
                           Dim DupelessAnimeFiles = AnimeFiles.Distinct.ToList
                           If (AnimeFiles.Count - DupelessAnimeFiles.Count) > 0 Then
                               CType(BaseAnimeDocument.Root.FirstNode, XElement).RemoveNodes()
                               For Each anime In DupelessAnimeFiles
                                   CType(BaseAnimeDocument.Root.FirstNode, XElement).Add(New XElement("Anime") With {.Value = anime})
                               Next
                           End If
                           If AutoSave Then Save()
                       End Function)
    End Function
End Class
