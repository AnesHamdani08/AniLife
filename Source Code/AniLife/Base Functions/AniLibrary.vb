''' <summary>
''' Provides method to load and store anime informations in an XML Databse , this version is improved for AniDB Caching, Requires AniLife.AniDB
''' </summary>
Public Class AniLibrary
    Public Const Version = "0.0.0.1-Beta.2"

    Public Event OnLoaded()
    Public Event OnItemAdded(Item As AnimeElement)
    Public Event OnItemRemoved(Item As AnimeElement)
    Public Event OnItemUpdated(Item As AnimeElement, NewItem As AnimeElement)
    Public Event OnItemMoved(Item As AnimeElement, OldStatus As Status)
    Public Event OnItemProgressUpdated(Item As AnimeElement, OldProgress As Integer)
    Public Event OnActivityAdded(Item As LibraryActivityElement)
    Public Event OnDuplicatedEntry(Item As AnimeElement)
    Public Event OnEntryNotExists(Item As AnimeElement)

    Public Property AutoLoad As Boolean = True
    Public Property AutoActivityLogging As Boolean = True

    Private Property _BaseDocument As XDocument
    Public ReadOnly Property BaseDocument As XDocument
        Get
            Return _BaseDocument
        End Get
    End Property
    Private Property _Collection As New List(Of AnimeElement)
    Public ReadOnly Property Collection As List(Of AnimeElement)
        Get
            Return _Collection
        End Get
    End Property
    Private Property _Watching As New List(Of AnimeElement)
    Public ReadOnly Property Watching As List(Of AnimeElement)
        Get
            Return _Watching
        End Get
    End Property
    Private Property _Completed As New List(Of AnimeElement)
    Public ReadOnly Property Completed As List(Of AnimeElement)
        Get
            Return _Completed
        End Get
    End Property
    Private Property _Paused As New List(Of AnimeElement)
    Public ReadOnly Property Paused As List(Of AnimeElement)
        Get
            Return _Paused
        End Get
    End Property
    Private Property _Dropped As New List(Of AnimeElement)
    Public ReadOnly Property Dropped As List(Of AnimeElement)
        Get
            Return _Dropped
        End Get
    End Property
    Private Property _Planning As New List(Of AnimeElement)
    Public ReadOnly Property Planning As List(Of AnimeElement)
        Get
            Return _Planning
        End Get
    End Property
    Private Property _Activities As New List(Of LibraryActivityElement)
    Public ReadOnly Property Activities As List(Of LibraryActivityElement)
        Get
            Return _Activities
        End Get
    End Property
    Public Property AutoSave As Boolean = True
    Private FileLocation As String
    Public Shared Function MakeLibrary() As AniLibrary
        Dim XLibray As New XDocument()
        Dim Root As New XElement("AniLife")
        Dim Collection As New XElement("Collection")
        Dim Watching As New XElement("Watching")
        Dim Completed As New XElement("Completed")
        Dim Paused As New XElement("Paused")
        Dim Dropped As New XElement("Dropped")
        Dim Planning As New XElement("Planning")
        Dim Activity As New XElement("Activities")
        Root.Add({Collection, Watching, Completed, Paused, Dropped, Planning, Activity})
        XLibray.Add(Root)
        Return New AniLibrary(XLibray)
    End Function
    Public Sub New(XLibrary As XDocument)
        If XLibrary.Root.Name = "AniLife" Then
            _BaseDocument = XLibrary
            If AutoLoad Then Load(BaseDocument)
        End If
    End Sub
    Public Sub New(XFile As String)
        If IO.File.Exists(XFile) Then
            Dim Xlibrary = XDocument.Load(XFile)
            If Xlibrary.Root.Name = "AniLife" Then
                FileLocation = XFile
                _BaseDocument = Xlibrary
                If AutoLoad Then Load(BaseDocument)
            End If
        Else
            Throw New ArgumentNullException("File: " & XFile & " doesn't exist.")
        End If
    End Sub
    Public Sub New()
    End Sub
    Public Sub Load(XLibrary As XDocument)
        If XLibrary.Root.Name = "AniLife" Then
            _BaseDocument = XLibrary
            Collection.Clear()
            For Each anime As XElement In CType(BaseDocument.Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "Collection"), XElement)?.Nodes
                _Collection.Add(New AnimeElement(anime.Value, anime.Attribute("Score").Value, anime.Attribute("EpisodeProgress").Value, anime.Attribute("Note").Value, Status.None))
            Next
            Watching.Clear()
            For Each anime As XElement In CType(BaseDocument.Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "Watching"), XElement)?.Nodes
                Watching.Add(New AnimeElement(anime.Value, anime.Attribute("Score").Value, anime.Attribute("EpisodeProgress").Value, anime.Attribute("Note").Value, Status.Watching))
            Next
            Completed.Clear()
            For Each anime As XElement In CType(BaseDocument.Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "Completed"), XElement)?.Nodes
                Completed.Add(New AnimeElement(anime.Value, anime.Attribute("Score").Value, anime.Attribute("EpisodeProgress").Value, anime.Attribute("Note").Value, Status.Completed))
            Next
            Paused.Clear()
            For Each anime As XElement In CType(BaseDocument.Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "Paused"), XElement)?.Nodes
                Paused.Add(New AnimeElement(anime.Value, anime.Attribute("Score").Value, anime.Attribute("EpisodeProgress").Value, anime.Attribute("Note").Value, Status.Paused))
            Next
            Dropped.Clear()
            For Each anime As XElement In CType(BaseDocument.Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "Dropped"), XElement)?.Nodes
                Dropped.Add(New AnimeElement(anime.Value, anime.Attribute("Score").Value, anime.Attribute("EpisodeProgress").Value, anime.Attribute("Note").Value, Status.Dropped))
            Next
            Planning.Clear()
            For Each anime As XElement In CType(BaseDocument.Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "Planning"), XElement)?.Nodes
                Planning.Add(New AnimeElement(anime.Value, anime.Attribute("Score").Value, anime.Attribute("EpisodeProgress").Value, anime.Attribute("Note").Value, Status.Planning))
            Next
            Activities.Clear()
            For Each activity As XElement In CType(BaseDocument.Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "Activities"), XElement)?.Nodes
                Activities.Add(New LibraryActivityElement(activity.Value, activity.Attribute("Value").Value))
            Next
            RaiseEvent OnLoaded()
        End If
    End Sub
    Public Sub Load(XFile As String)
        If IO.File.Exists(XFile) Then
            Dim Xlibrary = XDocument.Load(XFile)
            If Xlibrary.Root.Name = "AniLife" Then
                FileLocation = XFile
                _BaseDocument = Xlibrary
                Collection.Clear()
                For Each anime As XElement In CType(BaseDocument.Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "Collection"), XElement)?.Nodes
                    _Collection.Add(New AnimeElement(anime.Value, anime.Attribute("Score").Value, anime.Attribute("EpisodeProgress").Value, anime.Attribute("Note").Value, Status.None))
                Next
                Watching.Clear()
                For Each anime As XElement In CType(BaseDocument.Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "Watching"), XElement)?.Nodes
                    Watching.Add(New AnimeElement(anime.Value, anime.Attribute("Score").Value, anime.Attribute("EpisodeProgress").Value, anime.Attribute("Note").Value, Status.Watching))
                Next
                Completed.Clear()
                For Each anime As XElement In CType(BaseDocument.Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "Completed"), XElement)?.Nodes
                    Completed.Add(New AnimeElement(anime.Value, anime.Attribute("Score").Value, anime.Attribute("EpisodeProgress").Value, anime.Attribute("Note").Value, Status.Completed))
                Next
                Paused.Clear()
                For Each anime As XElement In CType(BaseDocument.Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "Paused"), XElement)?.Nodes
                    Paused.Add(New AnimeElement(anime.Value, anime.Attribute("Score").Value, anime.Attribute("EpisodeProgress").Value, anime.Attribute("Note").Value, Status.Paused))
                Next
                Dropped.Clear()
                For Each anime As XElement In CType(BaseDocument.Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "Dropped"), XElement)?.Nodes
                    Dropped.Add(New AnimeElement(anime.Value, anime.Attribute("Score").Value, anime.Attribute("EpisodeProgress").Value, anime.Attribute("Note").Value, Status.Dropped))
                Next
                Planning.Clear()
                For Each anime As XElement In CType(BaseDocument.Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "Planning"), XElement)?.Nodes
                    Planning.Add(New AnimeElement(anime.Value, anime.Attribute("Score").Value, anime.Attribute("EpisodeProgress").Value, anime.Attribute("Note").Value, Status.Planning))
                Next
                Activities.Clear()
                For Each activity As XElement In CType(BaseDocument.Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "Activities"), XElement)?.Nodes
                    Activities.Add(New LibraryActivityElement(activity.Value, activity.Attribute("Value").Value))
                Next
                RaiseEvent OnLoaded()
            End If
        Else
            Throw New ArgumentNullException("File: " & XFile & " doesn't exist.")
        End If
    End Sub
    Public Sub Save(Path As String)
        If BaseDocument.Root.Name = "AniLife" Then
            BaseDocument.Save(Path)
        End If
    End Sub
    Public Sub Save()
        If BaseDocument.Root.Name = "AniLife" Then
            BaseDocument.Save(FileLocation)
        End If
    End Sub
    Public Function CheckIfExists(_AnimeElement As AnimeElement) As Boolean
        If _AnimeElement Is Nothing Then
            Return False
        End If
        Return If(Collection.FirstOrDefault(Function(k) k.ID = _AnimeElement.ID) Is Nothing, False, True)
    End Function
    Public Function CheckIfExists(ID As Integer) As Boolean
        Return If(Collection.FirstOrDefault(Function(k) k.ID = ID) Is Nothing, False, True)
    End Function
    Public Function GetByID(ID As Integer) As AnimeElement
        If CheckIfExists(ID) Then
            Dim Anime As XElement = CType(BaseDocument.Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "Collection"), XElement).Nodes.FirstOrDefault(Function(l) CType(l, XElement).Value = ID)
            Dim WAnime As XElement = CType(BaseDocument.Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "Watching"), XElement).Nodes.FirstOrDefault(Function(l) CType(l, XElement).Value = ID)
            Dim CAnime As XElement = CType(BaseDocument.Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "Completed"), XElement).Nodes.FirstOrDefault(Function(l) CType(l, XElement).Value = ID)
            Dim PAnime As XElement = CType(BaseDocument.Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "Paused"), XElement).Nodes.FirstOrDefault(Function(l) CType(l, XElement).Value = ID)
            Dim DAnime As XElement = CType(BaseDocument.Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "Dropped"), XElement).Nodes.FirstOrDefault(Function(l) CType(l, XElement).Value = ID)
            Dim PLAnime As XElement = CType(BaseDocument.Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "Planning"), XElement).Nodes.FirstOrDefault(Function(l) CType(l, XElement).Value = ID)
            If WAnime IsNot Nothing Then
                Return New AnimeElement(WAnime.Value, WAnime.Attribute("Score").Value, WAnime.Attribute("EpisodeProgress").Value, WAnime.Attribute("Note").Value, Status.Watching)
            End If
            If CAnime IsNot Nothing Then
                Return New AnimeElement(CAnime.Value, CAnime.Attribute("Score").Value, CAnime.Attribute("EpisodeProgress").Value, CAnime.Attribute("Note").Value, Status.Completed)
            End If
            If PAnime IsNot Nothing Then
                Return New AnimeElement(PAnime.Value, PAnime.Attribute("Score").Value, PAnime.Attribute("EpisodeProgress").Value, PAnime.Attribute("Note").Value, Status.Paused)
            End If
            If DAnime IsNot Nothing Then
                Return New AnimeElement(DAnime.Value, DAnime.Attribute("Score").Value, DAnime.Attribute("EpisodeProgress").Value, DAnime.Attribute("Note").Value, Status.Dropped)
            End If
            If PLAnime IsNot Nothing Then
                Return New AnimeElement(PLAnime.Value, PLAnime.Attribute("Score").Value, PLAnime.Attribute("EpisodeProgress").Value, PLAnime.Attribute("Note").Value, Status.Planning)
            End If
            Return New AnimeElement(Anime.Value, Anime.Attribute("Score").Value, Anime.Attribute("EpisodeProgress").Value, Anime.Attribute("Note").Value, Status.None)
        End If
        Return Nothing
    End Function
    Public Sub AddToCollection(_AnimeElement As AnimeElement, Optional RaiseEvents As Boolean = True)
        If CheckIfExists(_AnimeElement) Then
            RaiseEvent OnDuplicatedEntry(_AnimeElement)
            Exit Sub
        End If
        Collection.Add(_AnimeElement)
        CType(BaseDocument.Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "Collection"), XElement)?.Add(BuildXElement(_AnimeElement))
        Select Case _AnimeElement.Status
            Case Status.Watching
                CType(BaseDocument.Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "Watching"), XElement)?.Add(BuildXElement(_AnimeElement))
                Watching.Add(_AnimeElement)
            Case Status.Completed
                CType(BaseDocument.Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "Completed"), XElement)?.Add(BuildXElement(_AnimeElement))
                Completed.Add(_AnimeElement)
            Case Status.Paused
                CType(BaseDocument.Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "Paused"), XElement)?.Add(BuildXElement(_AnimeElement))
                Paused.Add(_AnimeElement)
            Case Status.Dropped
                CType(BaseDocument.Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "Dropped"), XElement)?.Add(BuildXElement(_AnimeElement))
                Dropped.Add(_AnimeElement)
            Case Status.Planning
                CType(BaseDocument.Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "Planning"), XElement)?.Add(BuildXElement(_AnimeElement))
                Planning.Add(_AnimeElement)
        End Select
        If AutoSave Then
            If Not String.IsNullOrEmpty(FileLocation) Then
                Save(FileLocation)
            End If
        End If
        If RaiseEvents Then RaiseEvent OnItemAdded(_AnimeElement)
        If AutoActivityLogging AndAlso RaiseEvents Then
            Dim ActElement As New LibraryActivityElement(_AnimeElement.ID, AniResolver.ADDED & Space(1) & _AnimeElement.ID & Space(1) & AniResolver.TO & Space(1) & _AnimeElement.Status.ToString)
            AddActivity(ActElement, RaiseEvents)
        End If
    End Sub
    Public Sub RemoveFromCollection(_AnimeElement As AnimeElement, Optional RaiseEvents As Boolean = True)
        If Not CheckIfExists(_AnimeElement) Then
            RaiseEvent OnEntryNotExists(_AnimeElement)
            Exit Sub
        End If
        Collection.Remove(Collection.FirstOrDefault(Function(k) k.ID = _AnimeElement.ID))
        CType(BaseDocument.Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "Collection"), XElement).Nodes.FirstOrDefault(Function(l) CType(l, XElement).Value = _AnimeElement.ID)?.Remove()
        Select Case _AnimeElement.Status
            Case Status.Watching
                CType(BaseDocument.Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "Watching"), XElement).Nodes.FirstOrDefault(Function(l) CType(l, XElement).Value = _AnimeElement.ID)?.Remove()
                Watching.Remove(_AnimeElement)
            Case Status.Completed
                CType(BaseDocument.Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "Completed"), XElement).Nodes.FirstOrDefault(Function(l) CType(l, XElement).Value = _AnimeElement.ID)?.Remove()
                Completed.Remove(_AnimeElement)
            Case Status.Paused
                CType(BaseDocument.Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "Paused"), XElement).Nodes.FirstOrDefault(Function(l) CType(l, XElement).Value = _AnimeElement.ID)?.Remove()
                Paused.Remove(_AnimeElement)
            Case Status.Dropped
                CType(BaseDocument.Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "Dropped"), XElement).Nodes.FirstOrDefault(Function(l) CType(l, XElement).Value = _AnimeElement.ID)?.Remove()
                Dropped.Remove(_AnimeElement)
            Case Status.Planning
                CType(BaseDocument.Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "Planning"), XElement).Nodes.FirstOrDefault(Function(l) CType(l, XElement).Value = _AnimeElement.ID)?.Remove()
                Planning.Remove(_AnimeElement)
        End Select
        If AutoSave Then
            If Not String.IsNullOrEmpty(FileLocation) Then
                Save(FileLocation)
            End If
        End If
        If RaiseEvents Then RaiseEvent OnItemRemoved(_AnimeElement)
        If AutoActivityLogging AndAlso RaiseEvents Then
            Dim ActElement As New LibraryActivityElement(_AnimeElement.ID, AniResolver.REMOVED & Space(1) & _AnimeElement.ID & Space(1) & AniResolver.FROM & Space(1) & AniResolver.COLLECTION)
            AddActivity(ActElement)
        End If
    End Sub
    Public Sub MoveFromCollection(_AnimeElement As AnimeElement, NewStatus As Status, Optional RaiseEvents As Boolean = True)
        If CheckIfExists(_AnimeElement) Then
            Dim OldStatus = _AnimeElement.Status
            RemoveFromCollection(_AnimeElement, False)
            _AnimeElement.Status = NewStatus
            AddToCollection(_AnimeElement, False)
            If AutoSave Then
                If Not String.IsNullOrEmpty(FileLocation) Then
                    Save(FileLocation)
                End If
            End If
            If RaiseEvents Then RaiseEvent OnItemMoved(_AnimeElement, OldStatus)
            If AutoActivityLogging AndAlso RaiseEvents Then
                Dim ActElement As New LibraryActivityElement(_AnimeElement.ID, AniResolver.MOVED & Space(1) & _AnimeElement.ID & Space(1) & AniResolver.FROM & Space(1) & OldStatus.ToString & Space(1) & AniResolver.TO & Space(1) & NewStatus.ToString)
                AddActivity(ActElement)
            End If
        Else
            RaiseEvent OnEntryNotExists(_AnimeElement)
        End If
    End Sub
    Public Sub UpdateItem(_AnimeElement As AnimeElement, _NewAnimeElement As AnimeElement, Optional RaiseEvents As Boolean = True)
        If CheckIfExists(_AnimeElement) Then
            RemoveFromCollection(_AnimeElement, False)
            AddToCollection(_NewAnimeElement, False)
            If AutoSave Then
                If Not String.IsNullOrEmpty(FileLocation) Then
                    Save(FileLocation)
                End If
            End If
            If RaiseEvents Then RaiseEvent OnItemUpdated(_AnimeElement, _NewAnimeElement)
            If AutoActivityLogging AndAlso RaiseEvents Then
                Dim ActElement As New LibraryActivityElement(_AnimeElement.ID, AniResolver.UPDATED & Space(1) & _AnimeElement.ID)
                AddActivity(ActElement, RaiseEvents)
            End If
        Else
            RaiseEvent OnEntryNotExists(_AnimeElement)
        End If
    End Sub
    Public Sub UpdateItemProgress(ID As Integer, Progress As Integer, Optional Increase As Boolean = False, Optional RaiseEvents As Boolean = True)
        Dim Item = Watching.FirstOrDefault(Function(k) k.ID = ID)
        If Item IsNot Nothing Then
            Dim OldProgress = CInt(Item.EpisodeProgress.ToString)
            If Increase Then
                Watching.Item(Watching.IndexOf(Item)).EpisodeProgress += Progress
                Dim xitem As XElement = CType(BaseDocument.Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "Watching"), XElement)?.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Value = ID)
                If xitem IsNot Nothing Then
                    xitem.Attribute("EpisodeProgress").Value += Progress
                End If
            Else
                Watching.Item(Watching.IndexOf(Item)).EpisodeProgress = Progress
                Dim xitem As XElement = CType(BaseDocument.Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "Watching"), XElement)?.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Value = ID)
                If xitem IsNot Nothing Then
                    xitem.Attribute("EpisodeProgress").Value = Progress
                End If
            End If
            If RaiseEvents Then RaiseEvent OnItemProgressUpdated(Item, OldProgress)
            If AutoActivityLogging AndAlso RaiseEvents Then
                If Increase Then
                    If Progress = 1 Then
                        Dim ActElement As New LibraryActivityElement(ID, AniResolver.WATCHED & Space(1) & Progress & Space(1) & AniResolver.EPISODE & Space(1) & AniResolver.OF & Space(1) & ID)
                        AddActivity(ActElement)
                    Else
                        Dim ActElement As New LibraryActivityElement(ID, AniResolver.WATCHED & Space(1) & OldProgress & Space(1) & AniResolver.TO & Progress & Space(1) & AniResolver.EPISODE & Space(1) & AniResolver.OF & Space(1) & ID)
                        AddActivity(ActElement)
                    End If
                Else
                    Dim ActElement As New LibraryActivityElement(ID, AniResolver.WATCHED & Space(1) & AniResolver.EPISODE & Space(1) & OldProgress & Space(1) & AniResolver.TO & Space(1) & Progress & Space(1) & AniResolver.OF & Space(1) & ID)
                    AddActivity(ActElement)
                End If
            End If
        End If
        If AutoSave Then Save(FileLocation)
    End Sub
    Public Sub AddActivity(_ActivityElement As LibraryActivityElement, Optional RaiseEvents As Boolean = True, <Runtime.CompilerServices.CallerMemberName> ByVal Optional propertyName As String = Nothing, <Runtime.CompilerServices.CallerLineNumber> ByVal Optional propertyline As String = Nothing)
        If _ActivityElement IsNot Nothing Then
            CType(BaseDocument.Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "Activities"), XElement)?.Add(BuildXElement(_ActivityElement))
            If RaiseEvents Then RaiseEvent OnActivityAdded(_ActivityElement)
            If AutoSave Then Save(FileLocation)
        End If
    End Sub
    Public Enum Status
        None
        Watching
        Completed
        Paused
        Dropped
        Planning
    End Enum
    Public Class AnimeElement
        Public Property ID As Integer
        Public Property Score As Double
        Public Property EpisodeProgress As Integer
        Public Property Note As String
        Public Property Status As Status
        Public Sub New(_ID As Integer, _Score As Double, _EpisodeProgress As Integer, _Note As String, _Status As Status)
            ID = _ID
            Score = _Score
            EpisodeProgress = _EpisodeProgress
            Note = _Note
            Status = _Status
        End Sub
        Public Function Clone() As AnimeElement
            Return New AnimeElement(ID, Score, EpisodeProgress, Note, Status)
        End Function
        Public Overrides Function ToString() As String
            Return "{ID=" & ID & ";Score=" & Score & ";EpProg=" & EpisodeProgress & ";Note=" & Note.Length & ";Status=" & Status.ToString & "}"
        End Function
    End Class
    Public Class LibraryActivityElement
        Public Property ID As Integer
        Public Property Value As String
        Public Sub New(_ID As Integer, _Value As String)
            ID = _ID
            Value = _Value
        End Sub
        Public Overrides Function ToString() As String
            Return "{ID:" & ID & ";Value:" & Value & "}"
        End Function
    End Class
#Region "Helpers"
    Public Function BuildXElement(_AnimeElement As AnimeElement) As XElement
        Try
            Dim Item As New XElement("Anime") With {.Value = _AnimeElement.ID}
            Item.Add(New XAttribute("Score", _AnimeElement.Score))
            Item.Add(New XAttribute("EpisodeProgress", _AnimeElement.EpisodeProgress))
            Item.Add(New XAttribute("Note", If(String.IsNullOrEmpty(_AnimeElement.Note), "", _AnimeElement.Note)))
            Return Item
        Catch ex As Exception
            Return Nothing
        End Try
    End Function
    Public Function BuildXElement(_ActivityElement As LibraryActivityElement) As XElement
        Try
            Dim Item As New XElement("Activity") With {.Value = _ActivityElement.ID}
            Item.Add(New XAttribute("Value", _ActivityElement.Value))
            Return Item
        Catch ex As Exception
            Return Nothing
        End Try
    End Function
#End Region
End Class
