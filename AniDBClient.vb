'--------------------------------------------------------------------------
'COPYRIGHT 2021 Anes08(@Aneshamdani8) All Rights Reserved
'You may use this library for your needs
'You are required to add credits to your app/library
'Credits:
'Copyright 2021 Anes08 All Rights Reserved
'Show some love to AniDB :)
'--------------------------------------------------------------------------
Imports System.Net

Namespace AniDB
    ''' <summary>
    ''' Used to access AniDB HTTP XML API
    ''' </summary>
    Public Class AniDBClient
        Public Const Version As String = "0.0.0.1-Beta.3"
        Public Const DataCommandsVersion As String = "0.0.0.1-Beta.3"
        Public Const SearchCommandsVersion As String = "0.0.0.1-Beta.2"
        Public Const HelpersVersion As String = "0.0.0.1-Beta.3"

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

        'AniDB Requires the following:
        'Local Caching
        'Request One Page Every 2 Seconds
        'Parameters :
        'client={string} [required]
        'client identification String, needs To be a registered client identifier under the HTTP API (this Is Not your project name).
        'clientver={integer} [required]
        'client version number, needs To be a valid client version number, For the given client identification String, registered under the HTTP API.
        'protover={integer} [required]
        'protocol version In use. Possible values: 1
        'request={string} [required]
        'HTTP XML datapage requested. See following sections.
        '----------------------------------------------------------------------
        'This project was made possible by following the following article:
        'https://wiki.anidb.net/HTTP_API_Definition
        'Thanks for the Wiki !

        Public Event OnError(Code As Integer, Value As String)
        Public Event OnImageDownloaded(ID As Integer, Image As System.Drawing.Image)
        Public Event OnAnimeElementDownloaded(ID As Integer, Data As AnimeElement)
        Public Event OnNetworkError()

#Region "Settings"
        Private _Client As String
        ''' <summary>
        ''' Client name used to contact AniDB
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Client As String
            Get
                Return _Client
            End Get
        End Property
        Private _ClientVer As Integer
        ''' <summary>
        ''' Client version used to contact AniDB
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ClientVer As Integer
            Get
                Return _ClientVer
            End Get
        End Property
        ''' <summary>
        ''' Ban Prevention, protects the client from getting banned by AniDB by limiting number of requests per AniDBRequestDelay property
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property UseBanPrevention As Boolean = True
        ''' <summary>
        ''' Gets or Sets the Request rate limiting delay
        ''' </summary>
        ''' <returns></returns>
        Public Property AniDBRequestDelay As Integer = 2500
        ''' <summary>
        ''' Gets or Sets the character fetching cap, Set to -1 to fetch all available characters
        ''' </summary>
        ''' <returns></returns>
        Public Property CharacterCap As Integer = 10
        ''' <summary>
        ''' Used to show only relevant tags
        ''' </summary>
        ''' <returns></returns>
        Public Property FilterTags As Boolean = True
        ''' <summary>
        ''' Gets or Sets whether to parse date string using Date.ParseExact (False) or String.Split (True)
        ''' </summary>
        ''' <returns></returns>
        Public Property UseLegacyDateParsing As Boolean = False
        ''' <summary>
        ''' Gets or Sets whether to directly use anidb cdn , or use anidb api for images
        ''' </summary>
        ''' <returns></returns>
        Public Property UseImageAPI As Boolean = False
        ''' <summary>
        ''' Gets or Sets whether to check for internet connection before any API Call
        ''' </summary>
        ''' <returns></returns>
        Public Property UseInternetCheck As Boolean = True
        Private Property IsAbleToRequest As Boolean = True
#End Region
        ''' <summary>
        ''' Initialize a new AniDBClient
        ''' </summary>
        ''' <param name="Client">Client name you registered for your app at AniDB</param>
        ''' <param name="ClientVersion">Client version you registered for your app at AniDB</param>
        Public Sub New(Client As String, ClientVersion As Integer)
            Console.WriteLine(ConsoleInfoText & "Initializing AniDBClient With " & Client & "/" & ClientVersion)
            _Client = Client
            _ClientVer = ClientVersion
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 'AniDB Requires Tls1.2
            Console.WriteLine(ConsoleInfoText & "Now Using TLS 1.2")
        End Sub
#Region "Data Commands"
        ''' <summary>
        ''' Allows retrieval of non-file or episode related information for a specific anime by AID (AniDB anime id).
        ''' </summary>
        ''' <param name="aid">AniDB anime id of the anime you want to retrieve data for.</param>
        ''' <returns></returns>
        Public Async Function Anime(aid As Integer, Optional PreloadPicture As Boolean = True, Optional FetchCharacters As Boolean = True, Optional PreloadCharacterPicture As Boolean = True) As Task(Of AnimeElement)
            'Base URL: http://api.anidb.net:9001/httpapi?request=anime                        
            If UseInternetCheck Then
                If Not UniSharedFunctions.CheckInternetConnection Then
                    Return Nothing
                End If
            End If
                If UseBanPrevention Then
                Await WaitForRequest()
                ResetRequest()
            End If
            Try
                Console.WriteLine(ConsoleInfoText & "Fetching Anime Data For " & aid)
                Using HClient As New Http.HttpClient(New Http.HttpClientHandler With {.AutomaticDecompression = DecompressionMethods.GZip Or DecompressionMethods.Deflate})
                    Dim Req = Await HClient.GetStringAsync(BuildAniDBRequest(AniDBRequestType.anime, Client, ClientVer, aid))
                    Dim XReq = XDocument.Parse(Req)
                    If XReq.Root.Name = "error" Then
                        Dim ErrCode = XReq.Root.FirstAttribute?.Value
                        Dim Value = XReq.Root.Value
                        Console.WriteLine(ConsoleInfoText & "ERROR With Code: " & ErrCode & ", Value: " & Value)
                        RaiseEvent OnError(ErrCode, Value)
                        Return Nothing
                    End If
                    Console.WriteLine(ConsoleInfoText & "Parsing Returned Data For " & aid)
                    Dim Item = Await ParseAnimeAniDBXML(XReq, PreloadPicture, FetchCharacters, PreloadCharacterPicture)
                    Console.WriteLine(ConsoleInfoText & "Data Parse For " & aid & " Completed Successfuly, Returned: " & Item.ToString)
                    RaiseEvent OnAnimeElementDownloaded(aid, Item)
                    Return Item
                End Using
            Catch ex As Exception
                Console.WriteLine(ConsoleDebugText & ex.ToString)
                RaiseEvent OnNetworkError()
                Return Nothing
            End Try
        End Function
        ''' <summary>
        ''' Allow retrieval of a specific character by ID from anime
        ''' </summary>
        ''' <param name="ID">Character ID</param>
        ''' <param name="AID">Anime ID</param>
        ''' <param name="PreloadPicture">Whether to preload character picture</param>
        ''' <returns></returns>
        Public Async Function Character(ID As Integer, AID As Integer, Optional PreloadPicture As Boolean = True) As Task(Of CharacterElement)
            'Base URL: http://api.anidb.net:9001/httpapi?request=anime         
            If UseInternetCheck Then
                If Not UniSharedFunctions.CheckInternetConnection Then
                    Return Nothing
                End If
            End If
            If UseBanPrevention Then
                Await WaitForRequest()
                ResetRequest()
            End If
            Try
                Console.WriteLine(ConsoleInfoText & "Fetching Character Data For " & ID & " From " & AID)
                Using HClient As New Http.HttpClient(New Http.HttpClientHandler With {.AutomaticDecompression = DecompressionMethods.GZip Or DecompressionMethods.Deflate})
                    Dim Req = Await HClient.GetStringAsync(BuildAniDBRequest(AniDBRequestType.anime, Client, ClientVer, AID))
                    Dim XReq = XDocument.Parse(Req)
                    If XReq.Root.Name = "error" Then
                        Dim ErrCode = XReq.Root.FirstAttribute.Value
                        Dim Value = XReq.Root.Value
                        Console.WriteLine(ConsoleInfoText & "ERROR With Code: " & ErrCode & ", Value: " & Value)
                        RaiseEvent OnError(ErrCode, Value)
                        Return Nothing
                    End If
                    Console.WriteLine(ConsoleInfoText & "Parsing Returned Data For " & ID & " From " & AID)
                    Dim Item = Await ParseExactCharacterFromAnimeAniDBXML(XReq, ID, PreloadPicture)
                    Console.WriteLine(ConsoleInfoText & "Data Parse For " & ID & " From " & AID & " Completed Successfuly, Returned: " & Item.ToString)
                    Return Item
                End Using
            Catch ex As Exception
                Console.WriteLine(ConsoleDebugText & ex.ToString)
                RaiseEvent OnNetworkError()
                Return Nothing
            End Try
        End Function
        ''' <summary>
        ''' This command mirrors the type of data provided on the main web page. Use this instead of scraping the HTML. Please note, however, that the 'random recommendations' are, in fact, random. Please do not expect random results here to match random results there.
        ''' </summary>
        ''' <param name="PreloadPicture">Pre-download Anime Picture</param>
        ''' <returns></returns>
        Public Async Function RandomRecommendation(Optional PreloadPicture As Boolean = True) As Task(Of List(Of RecommendationElement))
            'Base URL: http://api.anidb.net:9001/httpapi?request=randomrecommendation         
            If UseInternetCheck Then
                If Not UniSharedFunctions.CheckInternetConnection Then
                    Return Nothing
                End If
            End If
            If UseBanPrevention Then
                Await WaitForRequest()
                ResetRequest()
            End If
            Try
                Console.WriteLine(ConsoleInfoText & "Fetching Data For RandomRecommendation")
                Using HClient As New Http.HttpClient(New Http.HttpClientHandler With {.AutomaticDecompression = DecompressionMethods.GZip Or DecompressionMethods.Deflate})
                    Dim Req = Await HClient.GetStringAsync(BuildAniDBRequest(AniDBRequestType.randomrecommendation, Client, ClientVer, 0))
                    Dim XReq = XDocument.Parse(Req)
                    If XReq.Root.Name = "error" Then
                        Dim ErrCode = XReq.Root.FirstAttribute.Value
                        Dim Value = XReq.Root.Value
                        Console.WriteLine(ConsoleInfoText & "ERROR With Code: " & ErrCode & ", Value: " & Value)
                        RaiseEvent OnError(ErrCode, Value)
                        Return Nothing
                    End If
                    Console.WriteLine(ConsoleInfoText & "Parsing Returned Data For RandomRecommendation")
                    Return Await ParseRandomRecommendationAniDBXML(XReq, PreloadPicture)
                    Console.WriteLine(ConsoleInfoText & "Data Parse For RandomRecommendation Completed Successfuly")
                End Using
            Catch ex As Exception
                Console.WriteLine(ConsoleDebugText & ex.ToString)
                RaiseEvent OnNetworkError()
                Return Nothing
            End Try
        End Function
        ''' <summary>
        ''' This command mirrors the type of data provided on the main web page. Use this instead of scraping the HTML. Please note, however, that the 'random similar' are, in fact, random. Please do not expect random results here to match random results there.
        ''' </summary>
        ''' <param name="PreloadPicture">Pre-download Anime Picture</param>
        ''' <returns></returns>        
        Public Async Function RandomSimilar(Optional PreloadPicture As Boolean = True) As Task(Of List(Of SimilarElement))
            'Base URL: http://api.anidb.net:9001/httpapi?request=randomsimilar         
            If UseInternetCheck Then
                If Not UniSharedFunctions.CheckInternetConnection Then
                    Return Nothing
                End If
            End If
            If UseBanPrevention Then
                Await WaitForRequest()
                ResetRequest()
            End If
            Try
                Console.WriteLine(ConsoleInfoText & "Fetching Data For RandomSimilar")
                Using HClient As New Http.HttpClient(New Http.HttpClientHandler With {.AutomaticDecompression = DecompressionMethods.GZip Or DecompressionMethods.Deflate})
                    Dim Req = Await HClient.GetStringAsync(BuildAniDBRequest(AniDBRequestType.randomsimilar, Client, ClientVer, 0))
                    Dim XReq = XDocument.Parse(Req)
                    If XReq.Root.Name = "error" Then
                        Dim ErrCode = XReq.Root.FirstAttribute.Value
                        Dim Value = XReq.Root.Value
                        Console.WriteLine(ConsoleInfoText & "ERROR With Code: " & ErrCode & ", Value: " & Value)
                        RaiseEvent OnError(ErrCode, Value)
                        Return Nothing
                    End If
                    Console.WriteLine(ConsoleInfoText & "Parsing Returned Data For RandomSimilar")
                    Return Await ParseRandomSimilarAniDBXML(XReq, PreloadPicture)
                    Console.WriteLine(ConsoleInfoText & "Data Parse For RandomSimilar Completed Successfuly")
                End Using
            Catch ex As Exception
                Console.WriteLine(ConsoleDebugText & ex.ToString)
                RaiseEvent OnNetworkError()
                Return Nothing
            End Try
        End Function
        ''' <summary>
        ''' This command mirrors the type of data provided on the main web page. Use this instead of scraping the HTML. Unlike the two random result commands, the results here will match the results as supplied by the main web page (with some possible variance of a few hours, depending on cache life.)
        ''' </summary>
        ''' <param name="PreloadPicture">Pre-download Anime Picture</param>
        ''' <returns></returns>
        Public Async Function HotAnime(Optional PreloadPicture As Boolean = True) As Task(Of List(Of RecommendationElement))
            'Base URL: http://api.anidb.net:9001/httpapi?request=hotanime         
            If UseInternetCheck Then
                If Not UniSharedFunctions.CheckInternetConnection Then
                    Return Nothing
                End If
            End If
            If UseBanPrevention Then
                Await WaitForRequest()
                ResetRequest()
            End If
            Try
                Console.WriteLine(ConsoleInfoText & "Fetching Data For HotAnime")
                Using HClient As New Http.HttpClient(New Http.HttpClientHandler With {.AutomaticDecompression = DecompressionMethods.GZip Or DecompressionMethods.Deflate})
                    Dim Req = Await HClient.GetStringAsync(BuildAniDBRequest(AniDBRequestType.hotanime, Client, ClientVer, 0))
                    Dim XReq = XDocument.Parse(Req)
                    If XReq.Root.Name = "error" Then
                        Dim ErrCode = XReq.Root.FirstAttribute.Value
                        Dim Value = XReq.Root.Value
                        Console.WriteLine(ConsoleInfoText & "ERROR With Code: " & ErrCode & ", Value: " & Value)
                        RaiseEvent OnError(ErrCode, Value)
                        Return Nothing
                    End If
                    Console.WriteLine(ConsoleInfoText & "Parsing Returned Data For HotAnime")
                    Return Await ParseHotAnimeAniDBXML(XReq, PreloadPicture)
                    Console.WriteLine(ConsoleInfoText & "Data Parse For HotAnime Completed Successfuly")
                End Using
            Catch ex As Exception
                Console.WriteLine(ConsoleDebugText & ex.ToString)
                RaiseEvent OnNetworkError()
                Return Nothing
            End Try
        End Function
        ''' <summary>
        ''' A one-stop command returning the combined results of random recommendation, random similar, and hot anime. Use this command instead of scraping the HTML, and if you need more than one of the individual replies.
        ''' </summary>
        ''' <param name="PreloadPicture">Pre-download Anime Picture</param>
        ''' <returns></returns>
        Public Async Function Main(Optional PreloadPicture As Boolean = True, Optional SkipHotAnime As Boolean = False, Optional SkipRandomSimilar As Boolean = False, Optional SkipRandomRecommendation As Boolean = False) As Task(Of MainElement)
            'Base URL: http://api.anidb.net:9001/httpapi?request=main         
            If UseInternetCheck Then
                If Not UniSharedFunctions.CheckInternetConnection Then
                    Return Nothing
                End If
            End If
            If UseBanPrevention Then
                Await WaitForRequest()
                ResetRequest()
            End If
            Try
                Console.WriteLine(ConsoleInfoText & "Fetching Data For Main")
                Using HClient As New Http.HttpClient(New Http.HttpClientHandler With {.AutomaticDecompression = DecompressionMethods.GZip Or DecompressionMethods.Deflate})
                    Dim Req = Await HClient.GetStringAsync(BuildAniDBRequest(AniDBRequestType.main, Client, ClientVer, 0))
                    Dim XReq = XDocument.Parse(Req)
                    If XReq.Root.Name = "error" Then
                        Dim ErrCode = XReq.Root.FirstAttribute.Value
                        Dim Value = XReq.Root.Value
                        Console.WriteLine(ConsoleInfoText & "ERROR With Code: " & ErrCode & ", Value: " & Value)
                        RaiseEvent OnError(ErrCode, Value)
                        Return Nothing
                    End If
                    Console.WriteLine(ConsoleInfoText & "Parsing Returned Data For Main")
                    Return Await ParseMainAniDBXML(XReq, PreloadPicture, SkipHotAnime, SkipRandomSimilar, SkipRandomRecommendation)
                    Console.WriteLine(ConsoleInfoText & "Data Parse For Main Completed Successfuly")
                End Using
            Catch ex As Exception
                Console.WriteLine(ConsoleDebugText & ex.ToString)
                RaiseEvent OnNetworkError()
                Return Nothing
            End Try
        End Function
#End Region
#Region "Search Commands"
        Private Property _SearchCache As XDocument
        Public ReadOnly Property SearchCache As XDocument
            Get
                Return _SearchCache
            End Get
        End Property
        ''' <summary>
        ''' Loads Cache from a file
        ''' </summary>
        ''' <param name="Path">Cache Location</param>
        ''' <returns></returns>
        Public Async Function LoadSearchCache(Path As String, Optional UpdateSearchCache As Boolean = True) As Task(Of XDocument)
            If IO.File.Exists(Path) Then
                Return Await Task.Run(Function()
                                          Console.WriteLine(ConsoleInfoText & "Loading SearchCache From " & Path)
                                          If UpdateSearchCache Then
                                              Dim XCache = XDocument.Load(Path)
                                              If XCache.Root.Name = "animetitles" Then
                                                  Console.WriteLine(ConsoleInfoText & "Successfuly Loaded SearchCache")
                                                  _SearchCache = XCache
                                                  Return SearchCache
                                              End If
                                              Console.WriteLine(ConsoleInfoText & "Failed To Load SearchCache Reason: Mismatched Document Root")
                                              Return Nothing
                                          Else
                                              Dim XCache = XDocument.Load(Path)
                                              If XCache.Root.Name = "animetitles" Then
                                                  Console.WriteLine(ConsoleInfoText & "Successfuly Loaded SearchCache")
                                                  Return XCache
                                              End If
                                              Console.WriteLine(ConsoleInfoText & "Failed To Load SearchCache Reason: Mismatched Document Root")
                                              Return Nothing
                                          End If
                                      End Function)
            End If
            Console.WriteLine(ConsoleInfoText & "Failed To Load SearchCache Reason: File Doesn't Exist")
            Return Nothing
        End Function
        Public Function LoadSearchCacheFromDocument(XCache As XDocument) As Boolean
            Console.WriteLine(ConsoleInfoText & "Loading SearchCache From Document")
            If XCache.Root.Name = "animetitles" Then
                Console.WriteLine(ConsoleInfoText & "Successfuly Loaded SearchCache")
                _SearchCache = XCache
                Return True
            End If
            Console.WriteLine(ConsoleInfoText & "Failed To Load SearchCache Reason: Mismatched Document Root")
            Return False
        End Function
        ''' <summary>
        ''' Saves Cache to a file
        ''' </summary>
        ''' <param name="XCache"></param>
        ''' <param name="Path">Cache Location</param>
        ''' <returns></returns>
        Public Async Function SaveSearchCache(XCache As XDocument, Path As String) As Task
            Await Task.Run(Sub()
                               XCache.Save(Path)
                           End Sub)
        End Function
        ''' <summary>
        ''' Returns TimeSpan.Zero if file doesn't exist
        ''' </summary>
        ''' <param name="Path">Cache location</param>
        ''' <returns></returns>
        Public Function CheckCacheLife(Path As String) As TimeSpan
            If IO.File.Exists(Path) Then
                Console.WriteLine(ConsoleInfoText & "Checking Cache Life For " & Path)
                Return Now.Subtract(IO.File.GetCreationTime(Path))
            End If
            Return TimeSpan.Zero
        End Function
        ''' <summary>
        ''' Warning! Don't Not Request More Than Once Per Day !
        ''' Alternatively Use LoadSearchCache and SaveSearchCache Functions
        ''' </summary>
        ''' <returns></returns>
        Public Async Function GetSearchData() As Task(Of XDocument)
            'Base URL: http://anidb.net/api/anime-titles.xml.gz        
            Console.WriteLine(ConsoleInfoText & "Fetching SearchCache From DB")
            Using HClient As New Http.HttpClient(New Http.HttpClientHandler With {.AutomaticDecompression = DecompressionMethods.GZip Or DecompressionMethods.Deflate})
                Dim Req = Await HClient.GetStreamAsync("http://anidb.net/api/anime-titles.xml.gz")
                Console.WriteLine(ConsoleInfoText & "Successfuly Retrieved SearchCache From DB")
                Using GZ As New System.IO.Compression.GZipStream(Req, IO.Compression.CompressionMode.Decompress)
                    _SearchCache = XDocument.Load(GZ)
                    Console.WriteLine(ConsoleInfoText & "Successfuly Loaded SearchCache From DB")
                    Return SearchCache
                End Using
            End Using
        End Function
        ''' <summary>
        ''' Decompresses Downloaded Search Cache File from AniDB
        ''' </summary>
        ''' <param name="File">Search Cache File GZ</param>
        ''' <returns></returns>
        Public Async Function PrepareSearchDataAsync(File As String) As Task(Of XDocument)
            'Base URL: http://anidb.net/api/anime-titles.xml.gz        
            Return Await Task.Run(Function()
                                      Console.WriteLine(ConsoleInfoText & "Preparing SearchCache File With Args: " & File & ", Compression: GZ")
                                      If IO.File.Exists(File) Then
                                          Dim Req = New IO.FileStream(File, IO.FileMode.Open, IO.FileAccess.Read)
                                          Using GZ As New System.IO.Compression.GZipStream(Req, IO.Compression.CompressionMode.Decompress)
                                              _SearchCache = XDocument.Load(GZ)
                                              Console.WriteLine(ConsoleInfoText & "Successfuly Loaded SearchCache")
                                              Return SearchCache
                                          End Using
                                      Else
                                          Return Nothing
                                      End If
                                  End Function)
        End Function
        ''' <summary>
        ''' Decompresses Downloaded Search Cache File from AniDB
        ''' </summary>
        ''' <param name="File">Search Cache File GZ</param>
        ''' <returns></returns>
        Public Function PrepareSearchData(File As String) As XDocument
            'Base URL: http://anidb.net/api/anime-titles.xml.gz                
            Console.WriteLine(ConsoleInfoText & "Preparing SearchCache File With Args: " & File & ", Compression: GZ")
            If IO.File.Exists(File) Then
                Dim Req = New IO.FileStream(File, IO.FileMode.Open, IO.FileAccess.Read)
                Using GZ As New System.IO.Compression.GZipStream(Req, IO.Compression.CompressionMode.Decompress)
                    _SearchCache = XDocument.Load(GZ)
                    Console.WriteLine(ConsoleInfoText & "Successfuly Loaded SearchCache")
                    Return SearchCache
                End Using
            Else
                Return Nothing
            End If
        End Function
        ''' <summary>
        ''' Search Anime By ID
        ''' </summary>
        ''' <param name="XCache"></param>
        ''' <param name="id">Anime ID</param>
        ''' <returns></returns>
        Public Async Function SearchExactByID(XCache As XDocument, id As Integer) As Task(Of XElement)
            Dim root = XCache.Root 'animetitles
            Console.WriteLine(ConsoleInfoText & "Searching For Anime By ID: " & id)
            If root?.Name = "animetitles" Then
                Return Await Task.Run(Function()
                                          Return CType(root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).FirstAttribute.Value = id), XElement)
                                      End Function)
            End If
            Return Nothing
        End Function
        ''' <summary>
        ''' Return a single anime, that anime name matches the requested name
        ''' </summary>
        ''' <param name="XCache"></param>
        ''' <param name="name">Anime Name</param>
        ''' <returns></returns>
        Public Async Function SearchExactByName(XCache As XDocument, name As String) As Task(Of XElement)
            Dim root = XCache.Root 'animetitles
            Console.WriteLine(ConsoleInfoText & "Searching For Anime By Name: " & name)
            If root?.Name = "animetitles" Then
                Return Await Task.Run(Function()
                                          For Each item As XElement In root.Nodes
                                              Dim fitem = item.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Value.ToLower = name.ToLower)
                                              If fitem IsNot Nothing Then
                                                  Return item
                                              End If
                                          Next
                                          Return Nothing
                                      End Function)
            End If
            Return Nothing
        End Function
        ''' <summary>
        ''' Returns a list of animes, that matches the whole or portion of the requested name
        ''' </summary>
        ''' <param name="XCache"></param>
        ''' <param name="name">Anime Name</param>
        ''' <returns></returns>
        Public Async Function SearchByName(XCache As XDocument, name As String) As Task(Of List(Of XElement))
            Dim root = XCache.Root 'animetitles
            Console.WriteLine(ConsoleInfoText & "Finding Anime By Name: " & name)
            If root?.Name = "animetitles" Then
                Return Await Task.Run(Function()
                                          Dim ReturnList As New List(Of XElement)
                                          For Each item As XElement In root.Nodes
                                              Dim fitem = item.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Value.ToLower.Contains(name.ToLower))
                                              If fitem IsNot Nothing Then
                                                  ReturnList.Add(item)
                                              End If
                                          Next
                                          Return ReturnList
                                      End Function)
            End If
            Return Nothing
        End Function

        Public Function IDFromSearch(SearchData As XElement) As Integer
            Console.WriteLine(ConsoleInfoText & "Search For ID From Search Data")
            If SearchData.Name.LocalName = "anime" Then
                Return SearchData?.FirstAttribute?.Value
            End If
            Return 0
        End Function
#End Region
#Region "Helpers"
        ''' <summary>
        ''' Builds a URL to request the data from AniDB API
        ''' </summary>
        ''' <param name="RequestType">The data to request</param>
        ''' <param name="Client">Client name</param>
        ''' <param name="ClientVer">Client version</param>
        ''' <param name="Aid">Anime ID</param>
        ''' <param name="Protover">Protocol version, possible values: 1 </param>
        ''' <returns></returns>
        Private Function BuildAniDBRequest(RequestType As AniDBRequestType, Client As String, ClientVer As Integer, Aid As Integer, Optional Protover As Integer = 1) As String
            Select Case RequestType
                Case AniDBRequestType.anime
                    'Example: URL: http://api.anidb.net:9001/httpapi?request=anime&client={str}&clientver={int}&protover=1&aid={int}
                    Return "http://api.anidb.net:9001/httpapi?request=" & RequestType.ToString & "&client=" & Client.ToLower & "&clientver=" & ClientVer & "&protover=" & Protover & "&aid=" & Aid
                Case AniDBRequestType.randomrecommendation
                    'Example: URL: http://api.anidb.net:9001/httpapi?client={str}&clientver={int}&protover=1&request=randomrecommendation
                    Return "http://api.anidb.net:9001/httpapi?client=" & Client & "&clientver=" & ClientVer & "&protover=" & Protover & "&request=" & RequestType.ToString
                Case AniDBRequestType.randomsimilar
                    'Example: URL: http://api.anidb.net:9001/httpapi?client={str}&clientver={int}&protover=1&request=randomsimilar
                    Return "http://api.anidb.net:9001/httpapi?client=" & Client & "&clientver=" & ClientVer & "&protover=" & Protover & "&request=" & RequestType.ToString
                Case AniDBRequestType.hotanime
                    'Example: URL: http://api.anidb.net:9001/httpapi?client={str}&clientver={int}&protover=1&request=hotanime
                    Return "http://api.anidb.net:9001/httpapi?client=" & Client & "&clientver=" & ClientVer & "&protover=" & Protover & "&request=" & RequestType.ToString
                Case AniDBRequestType.main
                    'Example: URL: http://api.anidb.net:9001/httpapi?client={str}&clientver={int}&protover=1&request=main
                    Return "http://api.anidb.net:9001/httpapi?client=" & Client & "&clientver=" & ClientVer & "&protover=" & Protover & "&request=" & RequestType.ToString
            End Select
        End Function
        Public Enum AniDBRequestType
            anime
            randomrecommendation
            randomsimilar
            hotanime
            main
        End Enum
        ''' <summary>
        ''' Returns managed data for AniDB API XML Response
        ''' </summary>
        ''' <param name="xml">XML data</param>
        ''' <param name="PreloadPicture">Pre-download Anime Picture</param>
        ''' <returns></returns>
        Public Async Function ParseAnimeAniDBXML(xml As String, Optional PreloadPicture As Boolean = True, Optional FetchCharacters As Boolean = True, Optional PreloadCharacterPicture As Boolean = True) As Task(Of AnimeElement)
            Return Await Task.Run(Async Function()
                                      Dim XDoc = XDocument.Parse(xml)
                                      Dim Root = XDoc.Root 'anime
                                      Dim RootNodes = Root.Nodes
                                      Dim ID As Integer = Root.FirstAttribute.Value 'xxxx        
                                      Dim Restricted As Boolean = Root.LastAttribute.Value
                                      Dim XType = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "type"), XElement)
                                      Dim Type = If(XType IsNot Nothing, XType.Value, Nothing)
                                      Dim XEpisodeCount = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "episodecount"), XElement)
                                      Dim EpisodeCount As Integer = If(XEpisodeCount IsNot Nothing, XEpisodeCount.Value, 0)
                                      Dim XStartDate = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "startdate"), XElement)?.Value 'YYYY-MM-DD
                                      Dim SXStartDate = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "startdate"), XElement)?.Value.Split("-") 'YYYY-MM-DD
                                      Dim StartDate As Date = If(UseLegacyDateParsing, If(SXStartDate IsNot Nothing, If(SXStartDate.Length = 3, New Date(SXStartDate(0), SXStartDate(1), SXStartDate(2)), Date.MinValue), Date.MinValue), Date.ParseExact(If(String.IsNullOrEmpty(XStartDate), "0001-01-01", XStartDate), "yyyy-MM-dd", Globalization.DateTimeFormatInfo.InvariantInfo, Globalization.DateTimeStyles.None))
                                      Dim XEndDate = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "enddate"), XElement)?.Value 'YYYY-MM-DD
                                      Dim SXEndDate = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "enddate"), XElement)?.Value.Split("-") 'YYYY-MM-DD
                                      Dim EndDate As Date = If(UseLegacyDateParsing, If(SXEndDate IsNot Nothing, If(SXEndDate.Length = 3, New Date(SXEndDate(0), SXEndDate(1), SXEndDate(2)), Date.MinValue), Date.MinValue), Date.ParseExact(If(String.IsNullOrEmpty(XEndDate), "0001-01-01", XEndDate), "yyyy-MM-dd", Globalization.DateTimeFormatInfo.InvariantInfo, Globalization.DateTimeStyles.None))
                                      Dim Titles As New List(Of TitleElement)
                                      Dim XTitles = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "titles"), XElement)
                                      If XTitles IsNot Nothing Then
                                          For Each title As XElement In XTitles.Nodes
                                              Titles.Add(New TitleElement(title.FirstAttribute.Value, title.LastAttribute.Value, title.Value))
                                          Next
                                      End If
                                      Dim RelatedAnime As New List(Of RelatedAnimeElement)
                                      Dim XRelatedAnime = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "relatedanime"), XElement)
                                      If XRelatedAnime IsNot Nothing Then
                                          For Each anime As XElement In XRelatedAnime.Nodes
                                              RelatedAnime.Add(New RelatedAnimeElement(CInt(anime.FirstAttribute.Value), anime.LastAttribute.Value, anime.Value))
                                          Next
                                      End If
                                      Dim SimilarAnime As New List(Of SimilarAnimeElement)
                                      Dim XSimilarAnime = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "similaranime"), XElement)
                                      If XSimilarAnime IsNot Nothing Then
                                          For Each anime As XElement In XSimilarAnime.Nodes
                                              SimilarAnime.Add(New SimilarAnimeElement(anime.FirstAttribute.Value, anime.FirstAttribute.NextAttribute.Value, anime.LastAttribute.Value, anime.Value))
                                          Next
                                      End If
                                      Dim Xrecommendations = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "recommendations"), XElement) 'Needs Work !!!
                                      Dim Recommendations As String = If(Xrecommendations IsNot Nothing, Xrecommendations.ToString, Nothing)
                                      Dim XURL = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "url"), XElement)
                                      Dim URL As String = If(XURL IsNot Nothing, XURL.Value, Nothing)
                                      Dim Creators As New List(Of CreatorsElement)
                                      Dim XCreators = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "creators"), XElement)
                                      If XCreators IsNot Nothing Then
                                          For Each creator As XElement In XCreators.Nodes
                                              Creators.Add(New CreatorsElement(creator.FirstAttribute.Value, creator.LastAttribute.Value, creator.Value))
                                          Next
                                      End If
                                      Dim XDescription = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "description"), XElement)
                                      Dim Description As String = If(XDescription IsNot Nothing, XDescription.Value, Nothing)
                                      Dim Ratings As New List(Of RatingElement)
                                      Dim XRatings = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "ratings"), XElement)
                                      If XRatings IsNot Nothing Then
                                          For Each rating As XElement In XRatings.Nodes
                                              Ratings.Add(New RatingElement(rating.Name.LocalName, CInt(rating.FirstAttribute.Value), CDbl(rating.Value)))
                                          Next
                                      End If
                                      Dim XPictureURL = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "picture"), XElement)
                                      Dim PictureURL As String = If(XPictureURL IsNot Nothing, XPictureURL.Value, Nothing)
                                      Dim Picture As System.Drawing.Image = If(PreloadPicture, If(String.IsNullOrEmpty(PictureURL), Nothing, Await GetAniDBImageAsync(PictureURL)), Nothing)
                                      Dim XResources = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "resources"), XElement)
                                      Dim Resources As New List(Of ResourceElement)
                                      If XResources IsNot Nothing Then
                                          For Each resource As XElement In XResources.Nodes
                                              Dim ResType = resource.Attribute("type")?.Value
                                              Dim ExtEnt As New List(Of ResourceElement.ExternalEntityElement)
                                              For Each XExtEnt As XElement In resource.Nodes
                                                  Dim XEEE As New ResourceElement.ExternalEntityElement
                                                  For Each ExtEntID As XElement In XExtEnt.Nodes
                                                      XEEE.Identifiers.Add(ExtEntID.Value)
                                                  Next
                                                  ExtEnt.Add(XEEE)
                                              Next
                                              Resources.Add(New ResourceElement With {.Type = ResType, .ExternalEntities = ExtEnt})
                                          Next
                                      End If
                                      Dim Tags As New List(Of TagElement)
                                      Dim XTags = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "tags"), XElement)
                                      If XTags IsNot Nothing Then
                                          For Each tag As XElement In XTags.Nodes
                                              If FilterTags Then
                                                  Dim IsInfoBox = tag.Attribute("infobox")
                                                  If IsInfoBox IsNot Nothing Then
                                                      Tags.Add(New TagElement(CType(tag.FirstNode, XElement)?.Value, CType(tag.FirstNode.NextNode, XElement)?.Value))
                                                  End If
                                              Else
                                                  Tags.Add(New TagElement(CType(tag.FirstNode, XElement)?.Value, CType(tag.FirstNode.NextNode, XElement)?.Value))
                                              End If
                                          Next
                                      End If
                                      Dim Characters As New List(Of CharacterElement)
                                      If FetchCharacters Then
                                          Dim XCharacters = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "characters"), XElement)
                                          If XCharacters IsNot Nothing Then
                                              For Each character As XElement In XCharacters.Nodes
                                                  Characters.Add(New CharacterElement(CInt(character.FirstAttribute.Value),
                                                        character.FirstAttribute.NextAttribute.Value,
                                                        New RatingElement(RatingElement.RatingType.Parmanent, CInt(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "rating"), XElement)?.FirstAttribute.Value), CDbl(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "rating"), XElement)?.Value)),
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "name"), XElement)?.Value,
                                                       CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "gender"), XElement)?.Value,
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "description"), XElement)?.Value,
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "picture"), XElement)?.Value,
                                                         If(PreloadCharacterPicture, Await GetAniDBImageAsync(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "picture"), XElement)?.Value), Nothing),
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "seiyuu"), XElement)?.Value,
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "seiyuu"), XElement)?.LastAttribute.Value,
                                                       CInt(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "seiyuu"), XElement)?.FirstAttribute.Value)))
                                                  If CharacterCap <> -1 Then
                                                      If Characters.Count >= CharacterCap Then Exit For
                                                  End If
                                              Next
                                          End If
                                      End If
                                      Dim XEpisodes = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "episodes"), XElement)
                                      Dim Episodes As New List(Of EpisodeElement)
                                      Dim UnEpisodes As New List(Of EpisodeElement)
                                      If XEpisodes IsNot Nothing Then
                                          For Each Episode As XElement In XEpisodes.Nodes
                                              Dim EID = Episode.Attribute("id")?.Value
                                              Dim XUDate = Episode.Attribute("update")?.Value
                                              Dim SXUDate = XUDate?.Split("-")
                                              Dim UDate As Date = If(UseLegacyDateParsing, If(SXUDate IsNot Nothing, If(SXUDate.Length = 3, New Date(SXUDate(0), SXUDate(1), SXUDate(2)), Date.MinValue), Date.MinValue), Date.ParseExact(If(String.IsNullOrEmpty(XUDate), "0001-01-01", XUDate), "yyyy-MM-dd", Globalization.DateTimeFormatInfo.InvariantInfo, Globalization.DateTimeStyles.None))
                                              Dim XEpNo = TryCast(Episode.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "epno"), XElement)
                                              Dim EpNo = If(XEpNo IsNot Nothing, XEpNo.Value, Nothing)
                                              Dim EType = If(XEpNo IsNot Nothing, XEpNo.Attribute("type")?.Value, Nothing)
                                              Dim Length = TryCast(Episode.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "length"), XElement)?.Value
                                              Dim XADate = TryCast(Episode.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "airdate"), XElement)?.Value
                                              Dim SXADate = XADate?.Split("-")
                                              Dim ADate As Date = If(UseLegacyDateParsing, If(SXADate IsNot Nothing, If(SXADate.Length = 3, New Date(SXADate(0), SXADate(1), SXADate(2)), Date.MinValue), Date.MinValue), Date.ParseExact(If(String.IsNullOrEmpty(XADate), "0001-01-01", XADate), "yyyy-MM-dd", Globalization.DateTimeFormatInfo.InvariantInfo, Globalization.DateTimeStyles.None))
                                              Dim XRating = TryCast(Episode.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "rating"), XElement)
                                              Dim Rating As New RatingElement(RatingElement.RatingType.Parmanent, CInt(XRating?.Attribute("votes")?.Value), CDbl(XRating?.Value))
                                              Dim ETitles As New List(Of TitleElement)
                                              For Each XTitleN As XElement In Episode.Nodes
                                                  If XTitleN.Name = "title" Then
                                                      ETitles.Add(New TitleElement(XTitleN.FirstAttribute?.Value, "main", XTitleN.Value))
                                                  End If
                                              Next
                                              Dim Summary As String = TryCast(Episode.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "summary"), XElement)?.Value
                                              Dim XEResources = TryCast(Episode.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "resources"), XElement)
                                              Dim EResources As New List(Of ResourceElement)
                                              If XEResources IsNot Nothing Then
                                                  For Each resource As XElement In XEResources.Nodes
                                                      Dim ResType = resource.Attribute("type")?.Value
                                                      Dim ExtEnt As New List(Of ResourceElement.ExternalEntityElement)
                                                      For Each XExtEnt As XElement In resource.Nodes
                                                          Dim XEEE As New ResourceElement.ExternalEntityElement
                                                          For Each ExtEntID As XElement In XExtEnt.Nodes
                                                              XEEE.Identifiers.Add(ExtEntID.Value)
                                                          Next
                                                          ExtEnt.Add(XEEE)
                                                      Next
                                                      EResources.Add(New ResourceElement With {.Type = ResType, .ExternalEntities = ExtEnt})
                                                  Next
                                              End If
                                              Dim REpNo As String
                                              If Integer.TryParse(EpNo, REpNo) Then
                                                  Episodes.Add(New EpisodeElement With {.ID = EID, .EditDate = UDate, .Number = EpNo, .Type = EType, .Length = Length, .AirDate = ADate, .Rating = Rating, .Titles = ETitles, .Summary = Summary, .Resources = EResources})
                                              Else
                                                  UnEpisodes.Add(New EpisodeElement With {.ID = EID, .EditDate = UDate, .Number = EpNo, .Type = EType, .Length = Length, .AirDate = ADate, .Rating = Rating, .Titles = ETitles, .Summary = Summary, .Resources = EResources})
                                              End If
                                          Next
                                          Episodes = Episodes.OrderBy(Function(k) CInt(k.Number)).ToList
                                      End If
                                      Return New AnimeElement(ID, Restricted, Type, EpisodeCount, StartDate, EndDate, Titles, RelatedAnime, SimilarAnime, Recommendations, Creators, Description, Ratings, PictureURL, Picture, Resources, Tags, Characters, Episodes) With {.UnmanagedXMLData = xml, .UnRelatedEpisodes = UnEpisodes}
                                  End Function)
        End Function
        ''' <summary>
        ''' Returns managed data for AniDB API XML Response
        ''' </summary>
        ''' <param name="xml">XML data</param>
        ''' <param name="PreloadPicture">Pre-download Anime Picture</param>
        ''' <returns></returns>
        Public Async Function ParseAnimeAniDBXML(xml As XDocument, Optional PreloadPicture As Boolean = True, Optional FetchCharacters As Boolean = True, Optional PreloadCharacterPicture As Boolean = True) As Task(Of AnimeElement)
            Return Await Task.Run(Async Function()
                                      Dim XDoc = xml
                                      Dim Root = XDoc.Root 'anime
                                      Dim RootNodes = Root.Nodes
                                      Dim ID As Integer = Root.FirstAttribute.Value 'xxxx        
                                      Dim Restricted As Boolean = Root.LastAttribute.Value
                                      Dim XType = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "type"), XElement)
                                      Dim Type = If(XType IsNot Nothing, XType.Value, Nothing)
                                      Dim XEpisodeCount = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "episodecount"), XElement)
                                      Dim EpisodeCount As Integer = If(XEpisodeCount IsNot Nothing, XEpisodeCount.Value, 0)
                                      Dim XStartDate = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "startdate"), XElement)?.Value 'YYYY-MM-DD
                                      Dim SXStartDate = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "startdate"), XElement)?.Value.Split("-") 'YYYY-MM-DD
                                      Dim StartDate As Date = If(UseLegacyDateParsing, If(SXStartDate IsNot Nothing, If(SXStartDate.Length = 3, New Date(SXStartDate(0), SXStartDate(1), SXStartDate(2)), Date.MinValue), Date.MinValue), Date.ParseExact(If(String.IsNullOrEmpty(XStartDate), "0001-01-01", XStartDate), "yyyy-MM-dd", Globalization.DateTimeFormatInfo.InvariantInfo, Globalization.DateTimeStyles.None))
                                      Dim XEndDate = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "enddate"), XElement)?.Value 'YYYY-MM-DD
                                      Dim SXEndDate = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "enddate"), XElement)?.Value.Split("-") 'YYYY-MM-DD
                                      Dim EndDate As Date = If(UseLegacyDateParsing, If(SXEndDate IsNot Nothing, If(SXEndDate.Length = 3, New Date(SXEndDate(0), SXEndDate(1), SXEndDate(2)), Date.MinValue), Date.MinValue), Date.ParseExact(If(String.IsNullOrEmpty(XEndDate), "0001-01-01", XEndDate), "yyyy-MM-dd", Globalization.DateTimeFormatInfo.InvariantInfo, Globalization.DateTimeStyles.None))
                                      Dim Titles As New List(Of TitleElement)
                                      Dim XTitles = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "titles"), XElement)
                                      If XTitles IsNot Nothing Then
                                          For Each title As XElement In XTitles.Nodes
                                              Titles.Add(New TitleElement(title.FirstAttribute.Value, title.LastAttribute.Value, title.Value))
                                          Next
                                      End If
                                      Dim RelatedAnime As New List(Of RelatedAnimeElement)
                                      Dim XRelatedAnime = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "relatedanime"), XElement)
                                      If XRelatedAnime IsNot Nothing Then
                                          For Each anime As XElement In XRelatedAnime.Nodes
                                              RelatedAnime.Add(New RelatedAnimeElement(CInt(anime.FirstAttribute.Value), anime.LastAttribute.Value, anime.Value))
                                          Next
                                      End If
                                      Dim SimilarAnime As New List(Of SimilarAnimeElement)
                                      Dim XSimilarAnime = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "similaranime"), XElement)
                                      If XSimilarAnime IsNot Nothing Then
                                          For Each anime As XElement In XSimilarAnime.Nodes
                                              SimilarAnime.Add(New SimilarAnimeElement(anime.FirstAttribute.Value, anime.FirstAttribute.NextAttribute.Value, anime.LastAttribute.Value, anime.Value))
                                          Next
                                      End If
                                      Dim Xrecommendations = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "recommendations"), XElement) 'Needs Work !!!
                                      Dim Recommendations As String = If(Xrecommendations IsNot Nothing, Xrecommendations.ToString, Nothing)
                                      Dim XURL = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "url"), XElement)
                                      Dim URL As String = If(XURL IsNot Nothing, XURL.Value, Nothing)
                                      Dim Creators As New List(Of CreatorsElement)
                                      Dim XCreators = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "creators"), XElement)
                                      If XCreators IsNot Nothing Then
                                          For Each creator As XElement In XCreators.Nodes
                                              Creators.Add(New CreatorsElement(creator.FirstAttribute.Value, creator.LastAttribute.Value, creator.Value))
                                          Next
                                      End If
                                      Dim XDescription = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "description"), XElement)
                                      Dim Description As String = If(XDescription IsNot Nothing, XDescription.Value, Nothing)
                                      Dim Ratings As New List(Of RatingElement)
                                      Dim XRatings = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "ratings"), XElement)
                                      If XRatings IsNot Nothing Then
                                          For Each rating As XElement In XRatings.Nodes
                                              Ratings.Add(New RatingElement(rating.Name.LocalName, CInt(rating.FirstAttribute.Value), CDbl(rating.Value)))
                                          Next
                                      End If
                                      Dim XPictureURL = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "picture"), XElement)
                                      Dim PictureURL As String = If(XPictureURL IsNot Nothing, XPictureURL.Value, Nothing)
                                      Dim Picture As System.Drawing.Image = If(PreloadPicture, If(String.IsNullOrEmpty(PictureURL), Nothing, Await GetAniDBImageAsync(PictureURL)), Nothing)
                                      Dim XResources = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "resources"), XElement)
                                      Dim Resources As New List(Of ResourceElement)
                                      If XResources IsNot Nothing Then
                                          For Each resource As XElement In XResources.Nodes
                                              Dim ResType = resource.Attribute("type")?.Value
                                              Dim ExtEnt As New List(Of ResourceElement.ExternalEntityElement)
                                              For Each XExtEnt As XElement In resource.Nodes
                                                  Dim XEEE As New ResourceElement.ExternalEntityElement
                                                  For Each ExtEntID As XElement In XExtEnt.Nodes
                                                      XEEE.Identifiers.Add(ExtEntID.Value)
                                                  Next
                                                  ExtEnt.Add(XEEE)
                                              Next
                                              Resources.Add(New ResourceElement With {.Type = ResType, .ExternalEntities = ExtEnt})
                                          Next
                                      End If
                                      Dim Tags As New List(Of TagElement)
                                      Dim XTags = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "tags"), XElement)
                                      If XTags IsNot Nothing Then
                                          For Each tag As XElement In XTags.Nodes
                                              If FilterTags Then
                                                  Dim IsInfoBox = tag.Attribute("infobox")
                                                  If IsInfoBox IsNot Nothing Then
                                                      Tags.Add(New TagElement(CType(tag.FirstNode, XElement)?.Value, CType(tag.FirstNode.NextNode, XElement)?.Value))
                                                  End If
                                              Else
                                                  Tags.Add(New TagElement(CType(tag.FirstNode, XElement)?.Value, CType(tag.FirstNode.NextNode, XElement)?.Value))
                                              End If
                                          Next
                                      End If
                                      Dim Characters As New List(Of CharacterElement)
                                      If FetchCharacters Then
                                          Dim XCharacters = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "characters"), XElement)
                                          If XCharacters IsNot Nothing Then
                                              For Each character As XElement In XCharacters.Nodes
                                                  Characters.Add(New CharacterElement(CInt(character.FirstAttribute.Value),
                                                        character.FirstAttribute.NextAttribute.Value,
                                                        New RatingElement(RatingElement.RatingType.Parmanent, CInt(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "rating"), XElement)?.FirstAttribute.Value), CDbl(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "rating"), XElement)?.Value)),
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "name"), XElement)?.Value,
                                                       CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "gender"), XElement)?.Value,
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "description"), XElement)?.Value,
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "picture"), XElement)?.Value,
                                                         If(PreloadCharacterPicture, Await GetAniDBImageAsync(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "picture"), XElement)?.Value), Nothing),
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "seiyuu"), XElement)?.Value,
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "seiyuu"), XElement)?.LastAttribute.Value,
                                                       CInt(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "seiyuu"), XElement)?.FirstAttribute.Value)))
                                                  If CharacterCap <> -1 Then
                                                      If Characters.Count >= CharacterCap Then Exit For
                                                  End If
                                              Next
                                          End If
                                      End If
                                      Dim XEpisodes = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "episodes"), XElement)
                                      Dim Episodes As New List(Of EpisodeElement)
                                      Dim UnEpisodes As New List(Of EpisodeElement)
                                      If XEpisodes IsNot Nothing Then
                                          For Each Episode As XElement In XEpisodes.Nodes
                                              Dim EID = Episode.Attribute("id")?.Value
                                              Dim XUDate = Episode.Attribute("update")?.Value
                                              Dim SXUDate = XUDate?.Split("-")
                                              Dim UDate As Date = If(UseLegacyDateParsing, If(SXUDate IsNot Nothing, If(SXUDate.Length = 3, New Date(SXUDate(0), SXUDate(1), SXUDate(2)), Date.MinValue), Date.MinValue), Date.ParseExact(If(String.IsNullOrEmpty(XUDate), "0001-01-01", XUDate), "yyyy-MM-dd", Globalization.DateTimeFormatInfo.InvariantInfo, Globalization.DateTimeStyles.None))
                                              Dim XEpNo = TryCast(Episode.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "epno"), XElement)
                                              Dim EpNo = If(XEpNo IsNot Nothing, XEpNo.Value, Nothing)
                                              Dim EType = If(XEpNo IsNot Nothing, XEpNo.Attribute("type")?.Value, Nothing)
                                              Dim Length = TryCast(Episode.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "length"), XElement)?.Value
                                              Dim XADate = TryCast(Episode.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "airdate"), XElement)?.Value
                                              Dim SXADate = XADate?.Split("-")
                                              Dim ADate As Date = If(UseLegacyDateParsing, If(SXADate IsNot Nothing, If(SXADate.Length = 3, New Date(SXADate(0), SXADate(1), SXADate(2)), Date.MinValue), Date.MinValue), Date.ParseExact(If(String.IsNullOrEmpty(XADate), "0001-01-01", XADate), "yyyy-MM-dd", Globalization.DateTimeFormatInfo.InvariantInfo, Globalization.DateTimeStyles.None))
                                              Dim XRating = TryCast(Episode.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "rating"), XElement)
                                              Dim Rating As New RatingElement(RatingElement.RatingType.Parmanent, CInt(XRating?.Attribute("votes")?.Value), CDbl(XRating?.Value))
                                              Dim ETitles As New List(Of TitleElement)
                                              For Each XTitleN As XElement In Episode.Nodes
                                                  If XTitleN.Name = "title" Then
                                                      ETitles.Add(New TitleElement(XTitleN.FirstAttribute?.Value, "main", XTitleN.Value))
                                                  End If
                                              Next
                                              Dim Summary As String = TryCast(Episode.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "summary"), XElement)?.Value
                                              Dim XEResources = TryCast(Episode.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "resources"), XElement)
                                              Dim EResources As New List(Of ResourceElement)
                                              If XEResources IsNot Nothing Then
                                                  For Each resource As XElement In XEResources.Nodes
                                                      Dim ResType = resource.Attribute("type")?.Value
                                                      Dim ExtEnt As New List(Of ResourceElement.ExternalEntityElement)
                                                      For Each XExtEnt As XElement In resource.Nodes
                                                          Dim XEEE As New ResourceElement.ExternalEntityElement
                                                          For Each ExtEntID As XElement In XExtEnt.Nodes
                                                              XEEE.Identifiers.Add(ExtEntID.Value)
                                                          Next
                                                          ExtEnt.Add(XEEE)
                                                      Next
                                                      EResources.Add(New ResourceElement With {.Type = ResType, .ExternalEntities = ExtEnt})
                                                  Next
                                              End If
                                              Dim REpNo As String
                                              If Integer.TryParse(EpNo, REpNo) Then
                                                  Episodes.Add(New EpisodeElement With {.ID = EID, .EditDate = UDate, .Number = EpNo, .Type = EType, .Length = Length, .AirDate = ADate, .Rating = Rating, .Titles = ETitles, .Summary = Summary, .Resources = EResources})
                                              Else
                                                  UnEpisodes.Add(New EpisodeElement With {.ID = EID, .EditDate = UDate, .Number = EpNo, .Type = EType, .Length = Length, .AirDate = ADate, .Rating = Rating, .Titles = ETitles, .Summary = Summary, .Resources = EResources})
                                              End If
                                          Next
                                          Episodes = Episodes.OrderBy(Function(k) CInt(k.Number)).ToList
                                      End If

                                      Return New AnimeElement(ID, Restricted, Type, EpisodeCount, StartDate, EndDate, Titles, RelatedAnime, SimilarAnime, Recommendations, Creators, Description, Ratings, PictureURL, Picture, Resources, Tags, Characters, Episodes) With {.UnmanagedXMLData = xml.ToString, .UnRelatedEpisodes = UnEpisodes}
                                  End Function)
        End Function
        ''' <summary>
        ''' Returns character only managed data for AniDB API XML Response
        ''' </summary>
        ''' <param name="xml">String XML Data</param>
        ''' <param name="PreloadPicture"></param>
        ''' <returns></returns>
        Public Async Function ParseCharacterFromAnimeAniDBXML(xml As String, Optional PreloadPicture As Boolean = True) As Task(Of List(Of CharacterElement))
            Return Await Task.Run(Async Function()
                                      Dim XDoc = XDocument.Parse(xml)
                                      Dim Root = XDoc.Root 'anime
                                      Dim RootNodes = Root.Nodes
                                      Dim Characters As New List(Of CharacterElement)
                                      Dim XCharacters = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "characters"), XElement)
                                      If XCharacters IsNot Nothing Then
                                          For Each character As XElement In XCharacters.Nodes
                                              Characters.Add(New CharacterElement(CInt(character.FirstAttribute.Value),
                                                        character.FirstAttribute.NextAttribute.Value,
                                                        New RatingElement(RatingElement.RatingType.Parmanent, CInt(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "rating"), XElement)?.FirstAttribute.Value), CDbl(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "rating"), XElement)?.Value)),
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "name"), XElement)?.Value,
                                                       CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "gender"), XElement)?.Value,
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "description"), XElement)?.Value,
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "picture"), XElement)?.Value,
                                                         If(PreloadPicture, Await GetAniDBImageAsync(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "picture"), XElement)?.Value), Nothing),
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "seiyuu"), XElement)?.Value,
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "seiyuu"), XElement)?.LastAttribute.Value,
                                                       CInt(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "seiyuu"), XElement)?.FirstAttribute.Value)))
                                              If CharacterCap <> -1 Then
                                                  If Characters.Count >= CharacterCap Then Exit For
                                              End If
                                          Next
                                      End If
                                      Return Characters
                                  End Function)
        End Function
        ''' <summary>
        ''' Returns character only managed data for AniDB API XML Response
        ''' </summary>
        ''' <param name="xml">String XML Data</param>
        ''' <param name="PreloadPicture"></param>
        ''' <returns></returns>
        Public Async Function ParseCharacterFromAnimeAniDBXML(xml As XDocument, Optional PreloadPicture As Boolean = True) As Task(Of List(Of CharacterElement))
            Return Await Task.Run(Async Function()
                                      Dim XDoc = xml
                                      Dim Root = XDoc.Root 'anime
                                      Dim RootNodes = Root.Nodes
                                      Dim Characters As New List(Of CharacterElement)
                                      Dim XCharacters = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "characters"), XElement)
                                      If XCharacters IsNot Nothing Then
                                          For Each character As XElement In XCharacters.Nodes
                                              Characters.Add(New CharacterElement(CInt(character.FirstAttribute.Value),
                                                        character.FirstAttribute.NextAttribute.Value,
                                                        New RatingElement(RatingElement.RatingType.Parmanent, CInt(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "rating"), XElement)?.FirstAttribute.Value), CDbl(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "rating"), XElement)?.Value)),
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "name"), XElement)?.Value,
                                                       CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "gender"), XElement)?.Value,
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "description"), XElement)?.Value,
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "picture"), XElement)?.Value,
                                                         If(PreloadPicture, Await GetAniDBImageAsync(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "picture"), XElement)?.Value), Nothing),
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "seiyuu"), XElement)?.Value,
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "seiyuu"), XElement)?.LastAttribute.Value,
                                                       CInt(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "seiyuu"), XElement)?.FirstAttribute.Value)))
                                              If CharacterCap <> -1 Then
                                                  If Characters.Count >= CharacterCap Then Exit For
                                              End If
                                          Next
                                      End If
                                      Return Characters
                                  End Function)
        End Function
        ''' <summary>
        ''' Returns character only managed data for AniDB API XML Response
        ''' </summary>
        ''' <param name="xml">String XML Data</param>
        ''' <param name="PreloadPicture"></param>
        ''' <returns></returns>
        Public Async Function ParseExactCharacterFromAnimeAniDBXML(xml As String, ID As Integer, Optional PreloadPicture As Boolean = True) As Task(Of CharacterElement)
            Return Await Task.Run(Async Function()
                                      Dim XDoc = XDocument.Parse(xml)
                                      Dim Root = XDoc.Root 'anime
                                      Dim RootNodes = Root.Nodes
                                      Dim SCharacter As CharacterElement = Nothing
                                      Dim XCharacters = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "characters"), XElement)
                                      If XCharacters IsNot Nothing Then
                                          For Each character As XElement In XCharacters.Nodes
                                              If CInt(character.FirstAttribute.Value) = ID Then
                                                  SCharacter = New CharacterElement(CInt(character.FirstAttribute.Value),
                                                        character.FirstAttribute.NextAttribute.Value,
                                                        New RatingElement(RatingElement.RatingType.Parmanent, CInt(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "rating"), XElement)?.FirstAttribute.Value), CDbl(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "rating"), XElement)?.Value)),
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "name"), XElement)?.Value,
                                                       CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "gender"), XElement)?.Value,
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "description"), XElement)?.Value,
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "picture"), XElement)?.Value,
                                                         If(PreloadPicture, Await GetAniDBImageAsync(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "picture"), XElement)?.Value), Nothing),
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "seiyuu"), XElement)?.Value,
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "seiyuu"), XElement)?.LastAttribute.Value,
                                                       CInt(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "seiyuu"), XElement)?.FirstAttribute.Value))
                                                  Exit For
                                              End If
                                          Next
                                      End If
                                      Return SCharacter
                                  End Function)
        End Function
        ''' <summary>
        ''' Returns character only managed data for AniDB API XML Response
        ''' </summary>
        ''' <param name="xml">String XML Data</param>
        ''' <param name="PreloadPicture"></param>
        ''' <returns></returns>
        Public Async Function ParseExactCharacterFromAnimeAniDBXML(xml As XDocument, ID As Integer, Optional PreloadPicture As Boolean = True) As Task(Of CharacterElement)
            Return Await Task.Run(Async Function()
                                      Dim XDoc = xml
                                      Dim Root = XDoc.Root 'anime
                                      Dim RootNodes = Root.Nodes
                                      Dim SCharacter As CharacterElement = Nothing
                                      Dim XCharacters = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "characters"), XElement)
                                      If XCharacters IsNot Nothing Then
                                          For Each character As XElement In XCharacters.Nodes
                                              If CInt(character.FirstAttribute.Value) = ID Then
                                                  SCharacter = New CharacterElement(CInt(character.FirstAttribute.Value),
                                                        character.FirstAttribute.NextAttribute.Value,
                                                        New RatingElement(RatingElement.RatingType.Parmanent, CInt(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "rating"), XElement)?.FirstAttribute.Value), CDbl(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "rating"), XElement)?.Value)),
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "name"), XElement)?.Value,
                                                       CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "gender"), XElement)?.Value,
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "description"), XElement)?.Value,
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "picture"), XElement)?.Value,
                                                         If(PreloadPicture, Await GetAniDBImageAsync(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "picture"), XElement)?.Value), Nothing),
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "seiyuu"), XElement)?.Value,
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "seiyuu"), XElement)?.LastAttribute.Value,
                                                       CInt(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "seiyuu"), XElement)?.FirstAttribute.Value))
                                                  Exit For
                                              End If
                                          Next
                                      End If
                                      Return SCharacter
                                  End Function)
        End Function
        ''' <summary>
        ''' Returns managed data for AniDB API XML Response
        ''' </summary>
        ''' <param name="xml">XML data</param>
        ''' <param name="PreloadPicture">Pre-download Anime Picture</param>
        ''' <returns></returns>
        Public Async Function ParseRandomRecommendationAniDBXML(xml As String, Optional PreloadPicture As Boolean = True) As Task(Of List(Of RecommendationElement))
            Return Await Task.Run(Async Function()
                                      Dim XDoc = XDocument.Parse(xml)
                                      Dim Root = XDoc.Root 'randomrecommendation
                                      Dim Recs As New List(Of RecommendationElement)
                                      For Each recommendation As XElement In Root.Nodes
                                          Dim anime = CType(recommendation.FirstNode, XElement)
                                          Dim ID As Integer = anime.FirstAttribute.Value
                                          Dim Restricted As Boolean = anime.LastAttribute.Value
                                          Dim Type = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "type"), XElement)?.Value
                                          Dim EpisodeCount As Integer = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "episodecount"), XElement)?.Value
                                          Dim XStartDate = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "startdate"), XElement)?.Value.Split("-") 'YYYY-MM-DD
                                          Dim StartDate As Date
                                          Try
                                              StartDate = If(XStartDate IsNot Nothing, New Date(XStartDate(0), XStartDate(1), XStartDate(2)), Nothing)
                                          Catch
                                              Try
                                                  StartDate = Date.Parse(TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "startdate"), XElement)?.Value)
                                              Catch
                                              End Try
                                          End Try
                                          Dim XEndDate = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "enddate"), XElement)?.Value.Split("-") 'YYYY-MM-DD
                                          Dim EndDate As Date
                                          Try
                                              EndDate = If(XEndDate IsNot Nothing, New Date(XEndDate(0), XEndDate(1), XEndDate(2)), Nothing)
                                          Catch
                                              Try
                                                  EndDate = Date.Parse(TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "enddate"), XElement)?.Value)
                                              Catch
                                              End Try
                                          End Try
                                          Dim XTitle = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "title"), XElement)
                                          Dim Title As TitleElement = If(XTitle IsNot Nothing, New TitleElement(XTitle.FirstAttribute.Value, XTitle.LastAttribute.Value, XTitle.Value), Nothing)
                                          Dim PictureURL = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "picture"), XElement)?.Value
                                          Dim Picture As System.Drawing.Image = If(PreloadPicture, Await GetAniDBImageAsync(PictureURL), Nothing)
                                          Dim XRatings = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "ratings"), XElement)
                                          Dim Ratings As New List(Of RatingElement)
                                          If XRatings IsNot Nothing Then
                                              For Each rating As XElement In XRatings.Nodes
                                                  Ratings.Add(New RatingElement(rating.Name.LocalName, If(rating.FirstAttribute IsNot Nothing, CInt(rating.FirstAttribute.Value), 0), CDbl(rating.Value.Insert(1, "."))))
                                              Next
                                          End If
                                          Recs.Add(New RecommendationElement(ID, Restricted, Type, EpisodeCount, StartDate, EndDate, Title, PictureURL, Picture, Ratings))
                                      Next
                                      Return Recs
                                  End Function)
        End Function
        ''' <summary>
        ''' Returns managed data for AniDB API XML Response
        ''' </summary>
        ''' <param name="xml">XML data</param>
        ''' <param name="PreloadPicture">Pre-download Anime Picture</param>
        ''' <returns></returns>
        Public Async Function ParseRandomRecommendationAniDBXML(xml As XDocument, Optional PreloadPicture As Boolean = True) As Task(Of List(Of RecommendationElement))
            Return Await Task.Run(Async Function()
                                      Dim XDoc = xml
                                      Dim Root = XDoc.Root 'randomrecommendation
                                      Dim Recs As New List(Of RecommendationElement)
                                      For Each recommendation As XElement In Root.Nodes
                                          Dim anime = CType(recommendation.FirstNode, XElement)
                                          Dim ID As Integer = anime.FirstAttribute.Value
                                          Dim Restricted As Boolean = anime.LastAttribute.Value
                                          Dim Type = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "type"), XElement)?.Value
                                          Dim EpisodeCount As Integer = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "episodecount"), XElement)?.Value
                                          Dim XStartDate = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "startdate"), XElement)?.Value.Split("-") 'YYYY-MM-DD
                                          Dim StartDate As Date = If(XStartDate IsNot Nothing, New Date(XStartDate(0), XStartDate(1), XStartDate(2)), Nothing)
                                          Dim XEndDate = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "enddate"), XElement)?.Value.Split("-") 'YYYY-MM-DD
                                          Dim EndDate As Date = If(XEndDate IsNot Nothing, New Date(XEndDate(0), XEndDate(1), XEndDate(2)), Nothing)
                                          Dim XTitle = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "title"), XElement)
                                          Dim Title As TitleElement = If(XTitle IsNot Nothing, New TitleElement(XTitle.FirstAttribute.Value, XTitle.LastAttribute.Value, XTitle.Value), Nothing)
                                          Dim PictureURL = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "picture"), XElement)?.Value
                                          Dim Picture As System.Drawing.Image = If(PreloadPicture, Await GetAniDBImageAsync(PictureURL), Nothing)
                                          Dim XRatings = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "ratings"), XElement)
                                          Dim Ratings As New List(Of RatingElement)
                                          If XRatings IsNot Nothing Then
                                              For Each rating As XElement In XRatings.Nodes
                                                  Ratings.Add(New RatingElement(rating.Name.LocalName, If(rating.FirstAttribute IsNot Nothing, CInt(rating.FirstAttribute.Value), 0), CDbl(rating.Value.Insert(1, "."))))
                                              Next
                                          End If
                                          Recs.Add(New RecommendationElement(ID, Restricted, Type, EpisodeCount, StartDate, EndDate, Title, PictureURL, Picture, Ratings))
                                      Next
                                      Return Recs
                                  End Function)
        End Function
        ''' <summary>
        ''' Returns managed data for AniDB API XML Response
        ''' </summary>
        ''' <param name="xml">XML data</param>
        ''' <param name="PreloadPicture">Pre-download Anime Picture</param>
        ''' <returns></returns>
        Public Async Function ParseRandomSimilarAniDBXML(xml As String, Optional PreloadPicture As Boolean = True) As Task(Of List(Of SimilarElement))
            Return Await Task.Run(Async Function()
                                      Dim XDoc = XDocument.Parse(xml)
                                      Dim Root = XDoc.Root 'randomsimilar
                                      Dim Similars As New List(Of SimilarElement)
                                      For Each similar As XElement In Root.Nodes
                                          Dim XSource = CType(similar.FirstNode, XElement)
                                          Dim ID As Integer = XSource.FirstAttribute.Value
                                          Dim Restricted As Boolean = XSource.LastAttribute.Value
                                          Dim XTitle = TryCast(XSource.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "title"), XElement)
                                          Dim Title As TitleElement = If(XTitle IsNot Nothing, New TitleElement(XTitle.FirstAttribute.Value, XTitle.LastAttribute.Value, XTitle.Value), Nothing)
                                          Dim PictureURL = TryCast(XSource.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "picture"), XElement)?.Value
                                          Dim Picture As System.Drawing.Image = If(PreloadPicture, Await GetAniDBImageAsync(PictureURL), Nothing)
                                          Dim Source As New SimilarElement.SimilarItem(ID, Restricted, Title, PictureURL, Picture)
                                          Dim XTarget = CType(similar.LastNode, XElement)
                                          Dim _ID As Integer = XTarget.FirstAttribute.Value
                                          Dim _Restricted As Boolean = XTarget.LastAttribute.Value
                                          Dim _XTitle = TryCast(XTarget.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "title"), XElement)
                                          Dim _Title As TitleElement = If(_XTitle IsNot Nothing, New TitleElement(_XTitle.FirstAttribute.Value, _XTitle.LastAttribute.Value, _XTitle.Value), Nothing)
                                          Dim _PictureURL = TryCast(XTarget.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "picture"), XElement)?.Value
                                          Dim _Picture As System.Drawing.Image = If(PreloadPicture, Await GetAniDBImageAsync(_PictureURL), Nothing)
                                          Dim Target As New SimilarElement.SimilarItem(_ID, _Restricted, _Title, _PictureURL, _Picture)
                                          Similars.Add(New SimilarElement(Source, Target))
                                      Next
                                      Return Similars
                                  End Function)
        End Function
        ''' <summary>
        ''' Returns managed data for AniDB API XML Response
        ''' </summary>
        ''' <param name="xml">XML data</param>
        ''' <param name="PreloadPicture">Pre-download Anime Picture</param>
        ''' <returns></returns>
        Public Async Function ParseRandomSimilarAniDBXML(xml As XDocument, Optional PreloadPicture As Boolean = True) As Task(Of List(Of SimilarElement))
            Return Await Task.Run(Async Function()
                                      Dim XDoc = xml
                                      Dim Root = XDoc.Root 'randomsimilar
                                      Dim Similars As New List(Of SimilarElement)
                                      For Each similar As XElement In Root.Nodes
                                          Dim XSource = CType(similar.FirstNode, XElement)
                                          Dim ID As Integer = XSource.FirstAttribute.Value
                                          Dim Restricted As Boolean = XSource.LastAttribute.Value
                                          Dim XTitle = TryCast(XSource.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "title"), XElement)
                                          Dim Title As TitleElement = If(XTitle IsNot Nothing, New TitleElement(XTitle.FirstAttribute.Value, XTitle.LastAttribute.Value, XTitle.Value), Nothing)
                                          Dim PictureURL = TryCast(XSource.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "picture"), XElement)?.Value
                                          Dim Picture As System.Drawing.Image = If(PreloadPicture, Await GetAniDBImageAsync(PictureURL), Nothing)
                                          Dim Source As New SimilarElement.SimilarItem(ID, Restricted, Title, PictureURL, Picture)
                                          Dim XTarget = CType(similar.LastNode, XElement)
                                          Dim _ID As Integer = XTarget.FirstAttribute.Value
                                          Dim _Restricted As Boolean = XTarget.LastAttribute.Value
                                          Dim _XTitle = TryCast(XTarget.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "title"), XElement)
                                          Dim _Title As TitleElement = If(_XTitle IsNot Nothing, New TitleElement(_XTitle.FirstAttribute.Value, _XTitle.LastAttribute.Value, _XTitle.Value), Nothing)
                                          Dim _PictureURL = TryCast(XTarget.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "picture"), XElement)?.Value
                                          Dim _Picture As System.Drawing.Image = If(PreloadPicture, Await GetAniDBImageAsync(_PictureURL), Nothing)
                                          Dim Target As New SimilarElement.SimilarItem(_ID, _Restricted, _Title, _PictureURL, _Picture)
                                          Similars.Add(New SimilarElement(Source, Target))
                                      Next
                                      Return Similars
                                  End Function)
        End Function
        ''' <summary>
        ''' Return managed data for AniDB API XML Response
        ''' </summary>
        ''' <param name="xml">XML Data</param>
        ''' <param name="PreloadPicture">Pre-download Anime Picture</param>
        ''' <returns></returns>
        Public Async Function ParseHotAnimeAniDBXML(xml As String, Optional PreloadPicture As Boolean = True) As Task(Of List(Of RecommendationElement))
            Return Await Task.Run(Async Function()
                                      Dim XDoc = XDocument.Parse(xml)
                                      Dim Root = XDoc.Root 'hotanime
                                      Dim Recs As New List(Of RecommendationElement)
                                      For Each anime As XElement In Root.Nodes
                                          Dim ID As Integer = anime.FirstAttribute.Value
                                          Dim Restricted As Boolean = anime.LastAttribute.Value
                                          Dim Type = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "type"), XElement)?.Value
                                          Dim EpisodeCount As Integer = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "episodecount"), XElement)?.Value
                                          Dim XStartDate = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "startdate"), XElement)?.Value.Split("-") 'YYYY-MM-DD
                                          Dim StartDate As Date = If(XStartDate IsNot Nothing, New Date(XStartDate(0), XStartDate(1), XStartDate(2)), Nothing)
                                          Dim XEndDate = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "enddate"), XElement)?.Value.Split("-") 'YYYY-MM-DD
                                          Dim EndDate As Date = If(XEndDate IsNot Nothing, New Date(XEndDate(0), XEndDate(1), XEndDate(2)), Nothing)
                                          Dim XTitle = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "title"), XElement)
                                          Dim Title As TitleElement = If(XTitle IsNot Nothing, New TitleElement(XTitle.FirstAttribute.Value, XTitle.LastAttribute.Value, XTitle.Value), Nothing)
                                          Dim PictureURL = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "picture"), XElement)?.Value
                                          Dim Picture As System.Drawing.Image = If(PreloadPicture, Await GetAniDBImageAsync(PictureURL), Nothing)
                                          Dim XRatings = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "ratings"), XElement)
                                          Dim Ratings As New List(Of RatingElement)
                                          If XRatings IsNot Nothing Then
                                              For Each rating As XElement In XRatings.Nodes
                                                  Ratings.Add(New RatingElement(rating.Name.LocalName, If(rating.FirstAttribute IsNot Nothing, CInt(rating.FirstAttribute.Value), 0), CDbl(rating.Value)))
                                              Next
                                          End If
                                          Recs.Add(New RecommendationElement(ID, Restricted, If(Type IsNot Nothing, Type, "TV Series"), EpisodeCount, StartDate, EndDate, Title, PictureURL, Picture, Ratings))
                                      Next
                                      Return Recs
                                  End Function)
        End Function
        ''' <summary>
        ''' Return managed data for AniDB API XML Response
        ''' </summary>
        ''' <param name="xml">XML Data</param>
        ''' <param name="PreloadPicture">Pre-download Anime Picture</param>
        ''' <returns></returns>
        Public Async Function ParseHotAnimeAniDBXML(xml As XDocument, Optional PreloadPicture As Boolean = True) As Task(Of List(Of RecommendationElement))
            Return Await Task.Run(Async Function()
                                      Dim XDoc = xml
                                      Dim Root = XDoc.Root 'hotanime
                                      Dim Recs As New List(Of RecommendationElement)
                                      For Each anime As XElement In Root.Nodes
                                          Dim ID As Integer = anime.FirstAttribute.Value
                                          Dim Restricted As Boolean = anime.LastAttribute.Value
                                          Dim Type = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "type"), XElement)?.Value
                                          Dim EpisodeCount As Integer = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "episodecount"), XElement)?.Value
                                          Dim XStartDate = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "startdate"), XElement)?.Value.Split("-") 'YYYY-MM-DD
                                          Dim StartDate As Date = If(XStartDate IsNot Nothing, New Date(XStartDate(0), XStartDate(1), XStartDate(2)), Nothing)
                                          Dim XEndDate = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "enddate"), XElement)?.Value.Split("-") 'YYYY-MM-DD
                                          Dim EndDate As Date = If(XEndDate IsNot Nothing, New Date(XEndDate(0), XEndDate(1), XEndDate(2)), Nothing)
                                          Dim XTitle = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "title"), XElement)
                                          Dim Title As TitleElement = If(XTitle IsNot Nothing, New TitleElement(XTitle.FirstAttribute.Value, XTitle.LastAttribute.Value, XTitle.Value), Nothing)
                                          Dim PictureURL = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "picture"), XElement)?.Value
                                          Dim Picture As System.Drawing.Image = If(PreloadPicture, Await GetAniDBImageAsync(PictureURL), Nothing)
                                          Dim XRatings = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "ratings"), XElement)
                                          Dim Ratings As New List(Of RatingElement)
                                          If XRatings IsNot Nothing Then
                                              For Each rating As XElement In XRatings.Nodes
                                                  Ratings.Add(New RatingElement(rating.Name.LocalName, If(rating.FirstAttribute IsNot Nothing, CInt(rating.FirstAttribute.Value), 0), CDbl(rating.Value)))
                                              Next
                                          End If
                                          Recs.Add(New RecommendationElement(ID, Restricted, If(Type IsNot Nothing, Type, "TV Series"), EpisodeCount, StartDate, EndDate, Title, PictureURL, Picture, Ratings))
                                      Next
                                      Return Recs
                                  End Function)
        End Function
        ''' <summary>
        ''' Return managed data for AniDB API XML Response
        ''' </summary>
        ''' <param name="xml">XML Data</param>
        ''' <param name="PreloadPicture">Pre-download Anime Picture</param>
        ''' <returns></returns>
        Public Async Function ParseMainAniDBXML(xml As String, Optional PreloadPicture As Boolean = True, Optional SkipHotAnime As Boolean = False, Optional SkipRandomSimilar As Boolean = False, Optional SkipRandomRecommendation As Boolean = False) As Task(Of MainElement)
            Return Await Task.Run(Async Function()
                                      Dim XDoc = XDocument.Parse(xml)
                                      Dim Root = XDoc.Root 'main       

                                      Dim XHot = Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "hotanime")
                                      Dim HotAnime = If(SkipHotAnime, Nothing, Await ParseHotAnimeAniDBXML(XHot.ToString, PreloadPicture))

                                      Dim XRandomSimilar = Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "randomsimilar")
                                      Dim RandomSimilar = If(SkipRandomSimilar, Nothing, Await ParseRandomSimilarAniDBXML(XRandomSimilar.ToString, PreloadPicture))

                                      Dim XRandomRecommendation = Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "randomrecommendation")
                                      Dim RandomRecommendation = If(SkipRandomRecommendation, Nothing, Await ParseRandomRecommendationAniDBXML(XRandomRecommendation.ToString, PreloadPicture))

                                      Return New MainElement(HotAnime, RandomSimilar, RandomRecommendation)
                                  End Function)
        End Function
        ''' <summary>
        ''' Return managed data for AniDB API XML Response
        ''' </summary>
        ''' <param name="xml">XML Data</param>
        ''' <param name="PreloadPicture">Pre-download Anime Picture</param>
        ''' <returns></returns>
        Public Async Function ParseMainAniDBXML(xml As XDocument, Optional PreloadPicture As Boolean = True, Optional SkipHotAnime As Boolean = False, Optional SkipRandomSimilar As Boolean = False, Optional SkipRandomRecommendation As Boolean = False) As Task(Of MainElement)
            Return Await Task.Run(Async Function()
                                      Dim XDoc = xml
                                      Dim Root = XDoc.Root 'main       

                                      Dim XHot = Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "hotanime")
                                      Dim HotAnime = If(SkipHotAnime, Nothing, Await ParseHotAnimeAniDBXML(XHot.ToString, PreloadPicture))

                                      Dim XRandomSimilar = Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "randomsimilar")
                                      Dim RandomSimilar = If(SkipRandomSimilar, Nothing, Await ParseRandomSimilarAniDBXML(XRandomSimilar.ToString, PreloadPicture))

                                      Dim XRandomRecommendation = Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "randomrecommendation")
                                      Dim RandomRecommendation = If(SkipRandomRecommendation, Nothing, Await ParseRandomRecommendationAniDBXML(XRandomRecommendation.ToString, PreloadPicture))

                                      Return New MainElement(HotAnime, RandomSimilar, RandomRecommendation)
                                  End Function)
        End Function
        ''' <summary>
        ''' Return the requested picture from AniDB CDN
        ''' </summary>
        ''' <param name="id">picture id : xxxx.xxx</param>
        ''' <returns></returns>
        Public Async Function GetAniDBImageAsync(id As String) As Task(Of System.Drawing.Image)
            'Base URL: https://cdn.anidb.net/images/main/
            'Base API URL: http://img7.anidb.net/pics/anime/         
            If UseInternetCheck Then
                If Not UniSharedFunctions.CheckInternetConnection Then
                    Return Nothing
                End If
            End If
            Console.WriteLine(ConsoleInfoText & "Fetching Anime Image From DB[API:" & UseImageAPI & "]: " & id)
            If String.IsNullOrEmpty(id) Then
                Return Nothing
            End If
            Return Await Task.Run(Function()
                                      Using WC As New WebClient()
                                          If UseImageAPI Then
                                              If ServicePointManager.SecurityProtocol <> SecurityProtocolType.Tls12 Then
                                                  ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 'AniDB Requires Tls1.2
                                                  Console.WriteLine("[" & Now.ToString("HH:mm:ss") & "][INFO]: " & "TLS 1.2 Is Required for Image API, Now Using TLS 1.2")
                                              End If
                                              Dim RURL As String
                                              Dim req As HttpWebRequest = HttpWebRequest.Create("http://img7.anidb.net/pics/anime/" & id)
                                              Try
                                                  Dim response As HttpWebResponse = req.GetResponse
                                                  RURL = response.ResponseUri.AbsoluteUri
                                              Catch ex As Exception
                                                  Console.WriteLine("[" & Now.ToString("HH:mm:ss") & "][DEBUG]: " & ex.ToString)
                                                  Console.WriteLine("[" & Now.ToString("HH:mm:ss") & "][INFO]: Now Trying With .jpg")
                                                  Try
                                                      req = HttpWebRequest.Create("http://img7.anidb.net/pics/anime/" & id & ".jpg")
                                                      Dim response As HttpWebResponse = req.GetResponse
                                                      RURL = response.ResponseUri.AbsoluteUri
                                                  Catch _ex As Exception
                                                      Console.WriteLine("[" & Now.ToString("HH:mm:ss") & "][INFO]: No Luck!")
                                                      Return Nothing
                                                  End Try
                                              End Try
                                              If Not String.IsNullOrEmpty(RURL) Then
                                                  Try
                                                      Dim ImageData = WC.DownloadData(RURL)
                                                      Dim IMG = System.Drawing.Image.FromStream(New IO.MemoryStream(ImageData))
                                                      Return IMG
                                                  Catch ex As Exception
                                                      Console.WriteLine("[" & Now.ToString("HH:mm:ss") & "][DEBUG]: " & ex.ToString)
                                                      Return Nothing
                                                  End Try
                                              Else
                                                  Return Nothing
                                              End If
                                          Else
                                              Try
                                                  Dim ImageData = WC.DownloadData("https://cdn.anidb.net/images/main/" & id)
                                                  Dim IMG = System.Drawing.Image.FromStream(New IO.MemoryStream(ImageData))
                                                  Return IMG
                                              Catch ex As Exception
                                                  Console.WriteLine("[" & Now.ToString("HH:mm:ss") & "][DEBUG]: " & ex.ToString)
                                                  Try
                                                      Dim ImageData = WC.DownloadData("https://cdn.anidb.net/images/main/" & id & ".jpg")
                                                      Dim IMG = System.Drawing.Image.FromStream(New IO.MemoryStream(ImageData))
                                                      Return IMG
                                                  Catch _ex As Exception
                                                      Console.WriteLine("[" & Now.ToString("HH:mm:ss") & "][DEBUG]: No Luck!")
                                                      Return Nothing
                                                  End Try
                                              End Try
                                          End If
                                      End Using
                                  End Function)
        End Function
        Public Async Function ResetRequest() As Task
            IsAbleToRequest = False
            Await Task.Delay(AniDBRequestDelay)
            IsAbleToRequest = True
        End Function
        Public Async Function WaitForRequest() As Task
            Await Task.Run(Async Function()
                               Do While IsAbleToRequest = False
                                   Await Task.Delay(50)
                               Loop
                           End Function)
        End Function
        Public Async Function CheckClientStatus() As Task(Of Boolean)
            Return Await SharedFunctions.CheckClient(Client, ClientVer)
        End Function
        Public Enum ResourceType
            AnimeNewsNetwork = 1
            MAL = 2
            WebsiteOrArcs = 4
            Website = 5
            WikipediaEN = 6
            WikipediaJP = 7
            Syoboi = 8
            AllCinema = 9
            Anison = 10
            DotLain = 11
            VNDB = 14
            Marumegane = 15
            WikipediaKO = 19
            WikipediaZH = 20
            Facebook = 22
            Twitter = 23
            Youtube = 26
            Crunchyroll = 28
            Amazon = 32
            OfficialStreams = 34
            Netflix = 41
        End Enum
#Region "Elements"
        Public Class TitleElement
            Public Property Lang As String
            Public Property Type As String
            Public Property Value As String
            Public Sub New(_Lang As String, _Type As String, _Value As String)
                Lang = _Lang
                Type = _Type
                Value = _Value
            End Sub
            Public Overrides Function ToString() As String
                Return "{Lang=" & Lang & ";Type=" & Type & ";Value=" & Value & "}"
            End Function
        End Class
        Public Class RelatedAnimeElement
            Public Property ID As Integer
            Public Property Type As AnimeType
            Public Property Value As String
            Public Sub New(_ID As Integer, _Type As AnimeType, _Value As String)
                ID = _ID
                Type = _Type
                Value = _Value
            End Sub
            Public Sub New(_ID As Integer, _Type As String, _Value As String)
                ID = _ID
                Type = StringToAnimeType(_Type)
                Value = _Value
            End Sub
            Public Function StringToAnimeType(str As String) As AnimeType
                Select Case str.ToLower
                    Case "prequel"
                        Return AnimeType.Prequel
                    Case "sequel"
                        Return AnimeType.Sequel
                    Case "same setting"
                        Return AnimeType.Same_Setting
                    Case "other"
                        Return AnimeType.Other
                    Case Else
                        Return AnimeType.Other
                End Select
            End Function
            Public Overrides Function ToString() As String
                Return "{ID=" & ID & ";Type=" & Type.ToString & ";Value=" & Value & "}"
            End Function
            Public Enum AnimeType
                Prequel
                Sequel
                Same_Setting
                Other
            End Enum
        End Class
        ''' <summary>
        ''' Related To Anime Request in AniDB API
        ''' </summary>
        Public Class SimilarAnimeElement
            Public Property ID As Integer
            Public Property Approval As Integer
            Public Property Total As Integer
            Public Property Value As String
            Public Sub New(_ID As Integer, _Approval As Integer, _Total As Integer, _Value As String)
                ID = _ID
                Approval = _Approval
                Total = _Total
                Value = _Value
            End Sub
            Public Overrides Function ToString() As String
                Return "{ID=" & ID & ";Approval=" & Approval & ";Total=" & Total & ";Value=" & Value & "}"
            End Function
        End Class
        Public Class CreatorsElement
            Public Property ID As Integer
            Public Property Type As String
            Public Property Value As String
            Public Sub New(_ID As Integer, _Type As String, _Value As String)
                ID = _ID
                Type = _Type
                Value = _Value
            End Sub
            Public Overrides Function ToString() As String
                Return "{ID=" & ID & ";Type=" & Type & ";value=" & Value & "}"
            End Function
        End Class
        Public Class RatingElement
            Public Property Type As RatingType
            Public Property Count As Integer
            Public Property Value As Double
            Public Sub New(_Type As RatingType, _Count As Integer, _Value As Double)
                Type = _Type
                Count = _Count
                Value = _Value
            End Sub
            Public Sub New(_Type As String, _Count As Integer, _Value As Double)
                Type = StringToRatingType(_Type)
                Count = _Count
                Value = _Value
            End Sub
            Public Function StringToRatingType(rating As String) As RatingType
                Select Case rating.ToLower
                    Case "permanent"
                        Return RatingType.Parmanent
                    Case "temporary"
                        Return RatingType.Temporary
                    Case "review"
                        Return RatingType.Review
                    Case "recommendations"
                        Return RatingType.Recommendations
                    Case Else
                        Return 0
                End Select
            End Function
            Public Overrides Function ToString() As String
                Return "{Type=" & Type & ";Count=" & Count & ";Value=" & Value & "}"
            End Function
            Public Enum RatingType
                Parmanent
                Temporary
                Review
                Recommendations
            End Enum
        End Class
        Public Class TagElement
            Public Property Name As String
            Public Property Description As String
            Public Sub New(_name As String, _desc As String)
                Name = _name
                Description = _desc
            End Sub
            Public Overrides Function ToString() As String
                Return "{Name=" & Name & ";Description=" & If(Description.Length > 10, Description.Substring(0, 10), Description) & "}"
            End Function
        End Class
        Public Class CharacterElement
            Public Property ID As Integer
            Public Property Type As CharacterType
            Public Property Rating As RatingElement
            Public Property Name As String
            Public Property Gender As String
            Public Property Description As String
            Public Property Picture As System.Drawing.Image
            Public Property PictureURL As String
            Public Property SeiyuuName As String
            Public Property SeiyuuPictureURL As String
            Public Property SeiyuuID As Integer
            Public Sub New(_ID As Integer, _Type As CharacterType, _Rating As RatingElement, _Name As String, _Gender As String, _Desc As String, _PictureURL As String, _Picture As System.Drawing.Image, _SeiyuuName As String, _SeiyuuPictureURL As String, _SeiyuuID As Integer)
                ID = _ID
                Type = _Type
                Rating = _Rating
                Name = _Name
                Gender = _Gender
                Description = _Desc
                PictureURL = _PictureURL
                Picture = _Picture
                SeiyuuName = _SeiyuuName
                SeiyuuID = _SeiyuuID
                SeiyuuPictureURL = _SeiyuuPictureURL
            End Sub
            Public Sub New(_ID As Integer, _Type As String, _Rating As RatingElement, _Name As String, _Gender As String, _Desc As String, _PictureURL As String, _Picture As System.Drawing.Image, _SeiyuuName As String, _SeiyuuPictureURL As String, _SeiyuuID As Integer)
                ID = _ID
                Type = StringToCharacterType(_Type)
                Rating = _Rating
                Name = _Name
                Gender = _Gender
                Description = _Desc
                PictureURL = _PictureURL
                Picture = _Picture
                SeiyuuName = _SeiyuuName
                SeiyuuID = _SeiyuuID
                SeiyuuPictureURL = _SeiyuuPictureURL
            End Sub
            Public Function StringToCharacterType(chartype As String) As CharacterType
                If chartype.ToLower.Contains("main") Then Return CharacterType.Main
                If chartype.ToLower.Contains("secondary") Then Return CharacterType.Secondary
                If chartype.ToLower.Contains("appears") Then Return CharacterType.Appears
                Return CharacterType.Appears
            End Function
            Public Overrides Function ToString() As String
                Return "{ID=" & ID & ";Name=" & Name & ";Type=" & Type.ToString & "}"
            End Function
            Public Enum CharacterType
                Main
                Secondary
                Appears
            End Enum
        End Class
        Public Class AnimeElement
            Public Property ID As Integer
            Public Property Restricted As Boolean
            Public Property Type As AnimeType
            Public Property EpisodeCount As Integer
            Public Property StartDate As Date
            Public Property EndDate As Date
            Public ReadOnly Property Title As String
                Get
                    If Titles IsNot Nothing Then
                        If Titles.Count > 0 Then
                            Dim XTitle = Titles.FirstOrDefault(Function(k) k.Type.ToLower = "main")
                            If XTitle IsNot Nothing Then
                                Return XTitle.Value
                            Else
                                XTitle = Titles.FirstOrDefault(Function(k) k.Type.ToLower = "official")
                                If XTitle IsNot Nothing Then
                                    Return XTitle.Value
                                Else
                                    Return Titles(0).Value
                                End If
                            End If
                        Else
                            Return Nothing
                        End If
                    Else
                        Return Nothing
                    End If
                End Get
            End Property
            Public Property Titles As List(Of TitleElement)
            Public ReadOnly Property Prequel As RelatedAnimeElement
                Get
                    Return RelatedAnimes.FirstOrDefault(Function(k) k.Type = RelatedAnimeElement.AnimeType.Prequel)
                End Get
            End Property
            Public ReadOnly Property Sequel As RelatedAnimeElement
                Get
                    Return RelatedAnimes.FirstOrDefault(Function(k) k.Type = RelatedAnimeElement.AnimeType.Sequel)
                End Get
            End Property
            Public Property RelatedAnimes As List(Of RelatedAnimeElement)
            Public Property SimilarAnimes As List(Of SimilarAnimeElement)
            Public Property Recommendations As String
            Public Property Creators As List(Of CreatorsElement)
            Public Property Description As String
            Public Property Ratings As List(Of RatingElement)
            Public Property PictureURL As String
            Public Property Picture As System.Drawing.Image
            Public Property Resources As List(Of ResourceElement)
            Public Property Tags As List(Of TagElement)
            Public Property Characters As List(Of CharacterElement)
            Public Property Episodes As List(Of EpisodeElement)
            Public Property UnRelatedEpisodes As List(Of EpisodeElement)
            Public Property UnmanagedXMLData As String
            Public Sub New(_ID As Integer, _Restricted As Boolean, _Type As AnimeType, _EpCount As Integer, _StartDate As Date, _EndDate As Date, _Titles As List(Of TitleElement), _RelatedAnimes As List(Of RelatedAnimeElement),
                           _SimilarAnimes As List(Of SimilarAnimeElement), _Recommendations As String, _Creators As List(Of CreatorsElement), _Desc As String, _Ratings As List(Of RatingElement),
                           _PictureURL As String, _Picture As System.Drawing.Image, _Resources As List(Of ResourceElement), _Tags As List(Of TagElement), _Characters As List(Of CharacterElement), _Episodes As List(Of EpisodeElement))
                ID = _ID
                Restricted = _Restricted
                Type = _Type
                EpisodeCount = _EpCount
                StartDate = _StartDate
                EndDate = _EndDate
                Titles = _Titles
                RelatedAnimes = _RelatedAnimes
                SimilarAnimes = _SimilarAnimes
                Recommendations = _Recommendations
                Creators = _Creators
                Description = _Desc
                Ratings = _Ratings
                PictureURL = _PictureURL
                Picture = _Picture
                Resources = _Resources
                Tags = _Tags
                Characters = _Characters
                Episodes = _Episodes
            End Sub
            Public Sub New(_ID As Integer, _Restricted As Boolean, _Type As String, _EpCount As Integer, _StartDate As Date, _EndDate As Date, _Titles As List(Of TitleElement), _RelatedAnimes As List(Of RelatedAnimeElement),
                           _SimilarAnimes As List(Of SimilarAnimeElement), _Recommendations As String, _Creators As List(Of CreatorsElement), _Desc As String, _Ratings As List(Of RatingElement),
                           _PictureURL As String, _Picture As System.Drawing.Image, _Resources As List(Of ResourceElement), _Tags As List(Of TagElement), _Characters As List(Of CharacterElement), _Episodes As List(Of EpisodeElement))
                ID = _ID
                Restricted = _Restricted
                Type = StringToAnimeType(_Type)
                EpisodeCount = _EpCount
                StartDate = _StartDate
                EndDate = _EndDate
                Titles = _Titles
                RelatedAnimes = _RelatedAnimes
                SimilarAnimes = _SimilarAnimes
                Recommendations = _Recommendations
                Creators = _Creators
                Description = _Desc
                Ratings = _Ratings
                PictureURL = _PictureURL
                Picture = _Picture
                Resources = _Resources
                Tags = _Tags
                Characters = _Characters
                Episodes = _Episodes
            End Sub
            Public Function StringToAnimeType(str As String) As AnimeType
                Select Case str.ToLower
                    Case "tv series"
                        Return AnimeType.TV
                    Case "ova"
                        Return AnimeType.OVA
                    Case "web"
                        Return AnimeType.Web
                    Case "movie"
                        Return AnimeType.Movie
                    Case "other"
                        Return AnimeType.Other
                    Case Else
                        Return 0
                End Select
            End Function
            Public Sub SaveToFile(path As String)
                Dim XAnime = XDocument.Parse(UnmanagedXMLData)
                If XAnime IsNot Nothing Then
                    XAnime.Save(path)
                End If
            End Sub
            Public Overrides Function ToString() As String
                Return "{ID=" & ID & ";Title[" & Titles?.Count & "]=" & If(Titles?.Count >= 1, Titles(0).Value, Nothing) & ";Type=" & Type.ToString & "}"
            End Function
            Public Enum AnimeType
                TV
                OVA
                Web
                Movie
                Other
                [Error]
            End Enum
        End Class
        Public Class RecommendationElement
            Public Property ID As Integer
            Public Property Restricted As Boolean
            Public Property Type As AnimeType
            Public Property EpisodeCount As Integer
            Public Property StartDate As Date
            Public Property EndDate As Date
            Public Property Title As TitleElement
            Public Property PictureURL As String
            Public Property Picture As System.Drawing.Image
            Public Property Ratings As List(Of RatingElement)
            Public Sub New(_ID As Integer, _Restricted As Boolean, _Type As AnimeType, _EpCount As Integer, _StartDate As Date, _EndDate As Date, _Title As TitleElement, _PictureURL As String, _Picture As System.Drawing.Image, _Ratings As List(Of RatingElement))
                ID = _ID
                Restricted = _Restricted
                Type = _Type
                EpisodeCount = _EpCount
                StartDate = _StartDate
                EndDate = _EndDate
                Title = _Title
                PictureURL = _PictureURL
                Picture = _Picture
                Ratings = _Ratings
            End Sub
            Public Sub New(_ID As Integer, _Restricted As Boolean, _Type As String, _EpCount As Integer, _StartDate As Date, _EndDate As Date, _Title As TitleElement, _PictureURL As String, _Picture As System.Drawing.Image, _Ratings As List(Of RatingElement))
                ID = _ID
                Restricted = _Restricted
                Type = StringToAnimeType(_Type)
                EpisodeCount = _EpCount
                StartDate = _StartDate
                EndDate = _EndDate
                Title = _Title
                PictureURL = _PictureURL
                Picture = _Picture
                Ratings = _Ratings
            End Sub
            Public Function StringToAnimeType(str As String) As AnimeType
                Select Case str.ToLower
                    Case "tv series"
                        Return AnimeType.TV_Series
                    Case "ova"
                        Return AnimeType.OVA
                    Case "web"
                        Return AnimeType.Web
                    Case "movie"
                        Return AnimeType.Movie
                    Case "other"
                        Return AnimeType.Other
                    Case Else
                        Return 0
                End Select
            End Function
            Public Overrides Function ToString() As String
                Return "{ID=" & ID & ";Type=" & Type.ToString & ";Title=" & Title?.Value & "}"
            End Function
            Public Enum AnimeType
                TV_Series
                OVA
                Web
                Movie
                Other
            End Enum
        End Class
        ''' <summary>
        ''' Related To RandomSimilar Request in AniDB API
        ''' </summary>
        Public Class SimilarElement
            Public Class SimilarItem
                Public Property ID As Integer
                Public Property Restricted As Boolean
                Public Property Title As TitleElement
                Public Property PictureURL As String
                Public Property Picture As System.Drawing.Image
                Public Sub New(_ID As Integer, _Restricted As Boolean, _Title As TitleElement, _PictureURL As String, _Picture As System.Drawing.Image)
                    ID = _ID
                    Restricted = _Restricted
                    Title = _Title
                    PictureURL = _PictureURL
                    Picture = _Picture
                End Sub
            End Class
            Public Property Source As SimilarItem
            Public Property Target As SimilarItem
            Public Sub New(_Source As SimilarItem, _Target As SimilarItem)
                Source = _Source
                Target = _Target
            End Sub
            Public Sub New(SourceID As Integer, SourceRestricted As Boolean, SourceTitle As TitleElement, SourcePicURL As String, SourcePic As System.Drawing.Image,
                           TargetID As Integer, TargetRestricted As Boolean, TargetTitle As TitleElement, TargetPicURL As String, TargetPic As System.Drawing.Image)
                Source = New SimilarItem(SourceID, SourceRestricted, SourceTitle, SourcePicURL, SourcePic)
                Target = New SimilarItem(TargetID, TargetRestricted, TargetTitle, TargetPicURL, TargetPic)
            End Sub
            Public Overrides Function ToString() As String
                Return "{Source={ID=" & Source?.ID & ";Title=" & Source?.Title?.Value & "};Target={ID=" & Target?.ID & ";Title=" & Target?.Title?.Value & "}"
            End Function
        End Class
        Public Class MainElement
            Public Property HotAnime As List(Of RecommendationElement)
            Public Property RandomSimilar As List(Of SimilarElement)
            Public Property RandomRecommendation As List(Of RecommendationElement)
            Public Sub New(_HotAnime As List(Of RecommendationElement), _RandomSimilar As List(Of SimilarElement), _RandomRecommendation As List(Of RecommendationElement))
                HotAnime = _HotAnime
                RandomSimilar = _RandomSimilar
                RandomRecommendation = _RandomRecommendation
            End Sub
        End Class
        Public Class ErrorElement
            Public Property ErrorCode As Integer
            Public Property ErrorValue As String
            Public Sub New(code As Integer, value As String)
                ErrorCode = code
                ErrorValue = value
            End Sub
        End Class
        Public Class ResourceElement
            Public Property Type As ResourceType
            Public Property ExternalEntities As List(Of ExternalEntityElement)
            Public ReadOnly Property URL As ManagedEEEURL
                Get
                    Return ParseURL(Type, ExternalEntities)
                End Get
            End Property
            Public Shared Function ParseURL(Type As ResourceType, ExternalEntities As List(Of ExternalEntityElement)) As ManagedEEEURL
                Dim MEEEU As New ManagedEEEURL
                Select Case Type
                    Case ResourceType.AnimeNewsNetwork
                        MEEEU.URLs.Add("https://www.animenewsnetwork.com/encyclopedia/anime.php?id=" & String.Join("", ExternalEntities(0).Identifiers))
                    Case ResourceType.MAL
                        For Each EEE In ExternalEntities
                            MEEEU.URLs.Add("https://myanimelist.net/anime/" & String.Join("", EEE.Identifiers))
                        Next
                    Case ResourceType.WebsiteOrArcs
                        For Each EEE In ExternalEntities
                            MEEEU.URLs.Add(String.Join("", EEE.Identifiers))
                        Next
                    Case ResourceType.Website
                        For Each EEE In ExternalEntities
                            MEEEU.URLs.Add(String.Join("", EEE.Identifiers))
                        Next
                    Case ResourceType.WikipediaEN
                        For Each EEE In ExternalEntities
                            MEEEU.URLs.Add("https://en.wikipedia.org/wiki/" & String.Join("", EEE.Identifiers))
                        Next
                    Case ResourceType.WikipediaJP
                        For Each EEE In ExternalEntities
                            MEEEU.URLs.Add("https://ja.wikipedia.org/wiki/" & String.Join("", EEE.Identifiers))
                        Next
                    Case ResourceType.Syoboi
                        For Each EEE In ExternalEntities
                            MEEEU.URLs.Add("https://cal.syoboi.jp/tid/" & String.Join("", EEE.Identifiers) & "/time")
                        Next
                    Case ResourceType.AllCinema
                        For Each EEE In ExternalEntities
                            MEEEU.URLs.Add("https://www.allcinema.net/cinema/" & String.Join("", EEE.Identifiers))
                        Next
                    Case ResourceType.Anison
                        For Each EEE In ExternalEntities
                            MEEEU.URLs.Add("http://anison.info/data/program/" & String.Join("", EEE.Identifiers) & ".html")
                        Next
                    Case ResourceType.DotLain
                        For Each EEE In ExternalEntities
                            MEEEU.URLs.Add("http://lain.gr.jp/mediadb/media/" & String.Join("", EEE.Identifiers))
                        Next
                    Case ResourceType.VNDB
                        For Each EEE In ExternalEntities
                            EEE.Identifiers.Reverse()
                            MEEEU.URLs.Add("https://vndb.org/" & String.Join("", EEE.Identifiers))
                        Next
                    Case ResourceType.Marumegane
                        For Each EEE In ExternalEntities
                            MEEEU.URLs.Add("http://www.anime.marumegane.com/" & String.Join("", EEE.Identifiers) & ".html")
                        Next
                    Case ResourceType.WikipediaKO
                        For Each EEE In ExternalEntities
                            MEEEU.URLs.Add("https://ko.wikipedia.org/wiki/" & String.Join("", EEE.Identifiers))
                        Next
                    Case ResourceType.WikipediaZH
                        For Each EEE In ExternalEntities
                            MEEEU.URLs.Add("https://zh.wikipedia.org/wiki/" & String.Join("", EEE.Identifiers))
                        Next
                    Case ResourceType.Facebook
                        For Each EEE In ExternalEntities
                            MEEEU.URLs.Add("https://www.facebook.com/" & String.Join("", EEE.Identifiers))
                        Next
                    Case ResourceType.Twitter
                        For Each EEE In ExternalEntities
                            MEEEU.URLs.Add("https://twitter.com/" & String.Join("", EEE.Identifiers))
                        Next
                    Case ResourceType.Youtube
                        For Each EEE In ExternalEntities
                            MEEEU.URLs.Add("https://www.youtube.com/" & String.Join("", EEE.Identifiers))
                        Next
                    Case ResourceType.Crunchyroll
                        For Each EEE In ExternalEntities
                            EEE.Identifiers.Reverse()
                            MEEEU.URLs.Add("https://www.crunchyroll.com/" & String.Join("/-", EEE.Identifiers))
                        Next
                    Case ResourceType.Amazon
                        For Each EEE In ExternalEntities
                            MEEEU.URLs.Add("https://www.amazon.com/dp/" & String.Join("", EEE.Identifiers))
                        Next
                    Case ResourceType.OfficialStreams
                        For Each EEE In ExternalEntities
                            MEEEU.URLs.Add(String.Join("", EEE.Identifiers))
                        Next
                    Case ResourceType.Netflix
                        For Each EEE In ExternalEntities
                            MEEEU.URLs.Add("https://www.netflix.com/title/" & String.Join("", EEE.Identifiers))
                        Next
                End Select
                Return MEEEU
            End Function
            Public Overrides Function ToString() As String
                Return "{Type=" & Type.ToString & "[" & Type & "];ExtEnt[" & ExternalEntities.Count & "]"
            End Function
            Public Class ExternalEntityElement
                Public Property Identifiers As New List(Of String)
                Public Overrides Function ToString() As String
                    Return "{Identifiers=[" & Identifiers.Count & "]"
                End Function
            End Class
            Public Class ManagedEEEURL
                Public Property URLs As New List(Of String)
                Public Overrides Function ToString() As String
                    Return "{URLs=[" & URLs.Count & "];" & String.Join(";", URLs) & "}"
                End Function
            End Class
        End Class
        Public Class EpisodeElement
            Public Property ID As Integer
            Public Property EditDate As Date
            Public Property Type As Integer
            Public Property Number As String
            Public Property Length As Integer
            Public Property AirDate As Date
            Public Property Rating As RatingElement
            Public Property Resources As List(Of ResourceElement)
            Public ReadOnly Property Title As String
                Get
                    If Titles IsNot Nothing Then
                        If Titles.Count > 0 Then
                            Dim XTitle = Titles.FirstOrDefault(Function(k) k.Lang.ToLower = "en")
                            If XTitle IsNot Nothing Then
                                Return XTitle.Value
                            Else
                                XTitle = Titles.FirstOrDefault(Function(k) k.Type.ToLower = "main")
                                If XTitle IsNot Nothing Then
                                    Return XTitle.Value
                                Else
                                    Return Titles(0).Value
                                End If
                            End If
                        Else
                            Return Nothing
                        End If
                    Else
                        Return Nothing
                    End If
                End Get
            End Property
            Public Property Titles As New List(Of TitleElement)
            Public Property Summary As String
            Public Overrides Function ToString() As String
                Return "{ID=" & ID & "Num=" & Number & ";Title=" & Title & ";SummaryAvailability=" & Not String.IsNullOrEmpty(Summary) & "}"
            End Function
        End Class
#End Region
#Region "Shared"
        Public Class SharedFunctions
            Public Const AniDBRequestDelay As Integer = 2500
            Public Const CharacterCap As Integer = 10
            Public Shared Property FilterTags As Boolean = True
            Public Shared Property UseLegacyDateParsing As Boolean = False
            ''' <summary>
            ''' Gets or Sets whether to directly use anidb cdn , or use anidb api for images
            ''' </summary>
            ''' <returns></returns>
            Public Shared Property UseImageAPI As Boolean = False
            ''' <summary>
            ''' Builds a URL to request the data from AniDB API
            ''' </summary>
            ''' <param name="RequestType">The data to request</param>
            ''' <param name="Client">Client name</param>
            ''' <param name="ClientVer">Client version</param>
            ''' <param name="Aid">Anime ID</param>
            ''' <param name="Protover">Protocol version, possible values: 1 </param>
            ''' <returns></returns>
            Public Shared Function BuildAniDBRequest(RequestType As AniDBRequestType, Client As String, ClientVer As Integer, Aid As Integer, Optional Protover As Integer = 1) As String
                Select Case RequestType
                    Case AniDBRequestType.anime
                        'Example: URL: http://api.anidb.net:9001/httpapi?request=anime&client={str}&clientver={int}&protover=1&aid={int}
                        Return "http://api.anidb.net:9001/httpapi?request=" & RequestType.ToString & "&client=" & Client.ToLower & "&clientver=" & ClientVer & "&protover=" & Protover & "&aid=" & Aid
                    Case AniDBRequestType.randomrecommendation
                        'Example: URL: http://api.anidb.net:9001/httpapi?client={str}&clientver={int}&protover=1&request=randomrecommendation
                        Return "http://api.anidb.net:9001/httpapi?client=" & Client & "&clientver=" & ClientVer & "&protover=" & Protover & "&request=" & RequestType.ToString
                    Case AniDBRequestType.randomsimilar
                        'Example: URL: http://api.anidb.net:9001/httpapi?client={str}&clientver={int}&protover=1&request=randomsimilar
                        Return "http://api.anidb.net:9001/httpapi?client=" & Client & "&clientver=" & ClientVer & "&protover=" & Protover & "&request=" & RequestType.ToString
                    Case AniDBRequestType.hotanime
                        'Example: URL: http://api.anidb.net:9001/httpapi?client={str}&clientver={int}&protover=1&request=hotanime
                        Return "http://api.anidb.net:9001/httpapi?client=" & Client & "&clientver=" & ClientVer & "&protover=" & Protover & "&request=" & RequestType.ToString
                    Case AniDBRequestType.main
                        'Example: URL: http://api.anidb.net:9001/httpapi?client={str}&clientver={int}&protover=1&request=main
                        Return "http://api.anidb.net:9001/httpapi?client=" & Client & "&clientver=" & ClientVer & "&protover=" & Protover & "&request=" & RequestType.ToString
                End Select
            End Function
            ''' <summary>
            ''' Returns managed data for AniDB API XML Response
            ''' </summary>
            ''' <param name="xml">XML data</param>
            ''' <param name="PreloadPicture">Pre-download Anime Picture</param>
            ''' <returns></returns>
            Public Shared Async Function ParseAnimeAniDBXML(xml As String, Optional PreloadPicture As Boolean = True, Optional FetchCharacters As Boolean = True, Optional PreloadCharacterPicture As Boolean = True) As Task(Of AnimeElement)
                Return Await Task.Run(Async Function()
                                          Dim XDoc = XDocument.Parse(xml)
                                          Dim Root = XDoc.Root 'anime
                                          Dim RootNodes = Root.Nodes
                                          Dim ID As Integer = Root.FirstAttribute.Value 'xxxx        
                                          Dim Restricted As Boolean = Root.LastAttribute.Value
                                          Dim XType = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "type"), XElement)
                                          Dim Type = If(XType IsNot Nothing, XType.Value, Nothing)
                                          Dim XEpisodeCount = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "episodecount"), XElement)
                                          Dim EpisodeCount As Integer = If(XEpisodeCount IsNot Nothing, XEpisodeCount.Value, 0)
                                          Dim XStartDate = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "startdate"), XElement)?.Value 'YYYY-MM-DD
                                          Dim SXStartDate = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "startdate"), XElement)?.Value.Split("-") 'YYYY-MM-DD
                                          Dim StartDate As Date = If(UseLegacyDateParsing, If(SXStartDate IsNot Nothing, If(SXStartDate.Length = 3, New Date(SXStartDate(0), SXStartDate(1), SXStartDate(2)), Date.MinValue), Date.MinValue), Date.ParseExact(If(String.IsNullOrEmpty(XStartDate), "0001-01-01", XStartDate), "yyyy-MM-dd", Globalization.DateTimeFormatInfo.InvariantInfo, Globalization.DateTimeStyles.None))
                                          Dim XEndDate = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "enddate"), XElement)?.Value 'YYYY-MM-DD
                                          Dim SXEndDate = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "enddate"), XElement)?.Value.Split("-") 'YYYY-MM-DD
                                          Dim EndDate As Date = If(UseLegacyDateParsing, If(SXEndDate IsNot Nothing, If(SXEndDate.Length = 3, New Date(SXEndDate(0), SXEndDate(1), SXEndDate(2)), Date.MinValue), Date.MinValue), Date.ParseExact(If(String.IsNullOrEmpty(XEndDate), "0001-01-01", XEndDate), "yyyy-MM-dd", Globalization.DateTimeFormatInfo.InvariantInfo, Globalization.DateTimeStyles.None))
                                          Dim Titles As New List(Of TitleElement)
                                          Dim XTitles = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "titles"), XElement)
                                          If XTitles IsNot Nothing Then
                                              For Each title As XElement In XTitles.Nodes
                                                  Titles.Add(New TitleElement(title.FirstAttribute.Value, title.LastAttribute.Value, title.Value))
                                              Next
                                          End If
                                          Dim RelatedAnime As New List(Of RelatedAnimeElement)
                                          Dim XRelatedAnime = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "relatedanime"), XElement)
                                          If XRelatedAnime IsNot Nothing Then
                                              For Each anime As XElement In XRelatedAnime.Nodes
                                                  RelatedAnime.Add(New RelatedAnimeElement(CInt(anime.FirstAttribute.Value), anime.LastAttribute.Value, anime.Value))
                                              Next
                                          End If
                                          Dim SimilarAnime As New List(Of SimilarAnimeElement)
                                          Dim XSimilarAnime = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "similaranime"), XElement)
                                          If XSimilarAnime IsNot Nothing Then
                                              For Each anime As XElement In XSimilarAnime.Nodes
                                                  SimilarAnime.Add(New SimilarAnimeElement(anime.FirstAttribute.Value, anime.FirstAttribute.NextAttribute.Value, anime.LastAttribute.Value, anime.Value))
                                              Next
                                          End If
                                          Dim Xrecommendations = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "recommendations"), XElement) 'Needs Work !!!
                                          Dim Recommendations As String = If(Xrecommendations IsNot Nothing, Xrecommendations.ToString, Nothing)
                                          Dim XURL = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "url"), XElement)
                                          Dim URL As String = If(XURL IsNot Nothing, XURL.Value, Nothing)
                                          Dim Creators As New List(Of CreatorsElement)
                                          Dim XCreators = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "creators"), XElement)
                                          If XCreators IsNot Nothing Then
                                              For Each creator As XElement In XCreators.Nodes
                                                  Creators.Add(New CreatorsElement(creator.FirstAttribute.Value, creator.LastAttribute.Value, creator.Value))
                                              Next
                                          End If
                                          Dim XDescription = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "description"), XElement)
                                          Dim Description As String = If(XDescription IsNot Nothing, XDescription.Value, Nothing)
                                          Dim Ratings As New List(Of RatingElement)
                                          Dim XRatings = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "ratings"), XElement)
                                          If XRatings IsNot Nothing Then
                                              For Each rating As XElement In XRatings.Nodes
                                                  Ratings.Add(New RatingElement(rating.Name.LocalName, CInt(rating.FirstAttribute.Value), CDbl(rating.Value)))
                                              Next
                                          End If
                                          Dim XPictureURL = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "picture"), XElement)
                                          Dim PictureURL As String = If(XPictureURL IsNot Nothing, XPictureURL.Value, Nothing)
                                          Dim Picture As System.Drawing.Image = If(PreloadPicture, If(String.IsNullOrEmpty(PictureURL), Nothing, Await GetAniDBImageAsync(PictureURL)), Nothing)
                                          Dim XResources = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "resources"), XElement)
                                          Dim Resources As New List(Of ResourceElement)
                                          If XResources IsNot Nothing Then
                                              For Each resource As XElement In XResources.Nodes
                                                  Dim ResType = resource.Attribute("type")?.Value
                                                  Dim ExtEnt As New List(Of ResourceElement.ExternalEntityElement)
                                                  For Each XExtEnt As XElement In resource.Nodes
                                                      Dim XEEE As New ResourceElement.ExternalEntityElement
                                                      For Each ExtEntID As XElement In XExtEnt.Nodes
                                                          XEEE.Identifiers.Add(ExtEntID.Value)
                                                      Next
                                                      ExtEnt.Add(XEEE)
                                                  Next
                                                  Resources.Add(New ResourceElement With {.Type = ResType, .ExternalEntities = ExtEnt})
                                              Next
                                          End If
                                          Dim Tags As New List(Of TagElement)
                                          Dim XTags = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "tags"), XElement)
                                          If XTags IsNot Nothing Then
                                              For Each tag As XElement In XTags.Nodes
                                                  If FilterTags Then
                                                      Dim IsInfoBox = tag.Attribute("infobox")
                                                      If IsInfoBox IsNot Nothing Then
                                                          Tags.Add(New TagElement(CType(tag.FirstNode, XElement)?.Value, CType(tag.FirstNode.NextNode, XElement)?.Value))
                                                      End If
                                                  Else
                                                      Tags.Add(New TagElement(CType(tag.FirstNode, XElement)?.Value, CType(tag.FirstNode.NextNode, XElement)?.Value))
                                                  End If
                                              Next
                                          End If
                                          Dim Characters As New List(Of CharacterElement)
                                          If FetchCharacters Then
                                              Dim XCharacters = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "characters"), XElement)
                                              If XCharacters IsNot Nothing Then
                                                  For Each character As XElement In XCharacters.Nodes
                                                      Characters.Add(New CharacterElement(CInt(character.FirstAttribute.Value),
                                                        character.FirstAttribute.NextAttribute.Value,
                                                        New RatingElement(RatingElement.RatingType.Parmanent, CInt(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "rating"), XElement)?.FirstAttribute.Value), CDbl(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "rating"), XElement)?.Value)),
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "name"), XElement)?.Value,
                                                       CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "gender"), XElement)?.Value,
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "description"), XElement)?.Value,
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "picture"), XElement)?.Value,
                                                         If(PreloadCharacterPicture, Await GetAniDBImageAsync(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "picture"), XElement)?.Value), Nothing),
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "seiyuu"), XElement)?.Value,
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "seiyuu"), XElement)?.LastAttribute.Value,
                                                       CInt(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "seiyuu"), XElement)?.FirstAttribute.Value)))
                                                      If Characters.Count >= CharacterCap Then Exit For
                                                  Next
                                              End If
                                          End If
                                          Dim XEpisodes = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "episodes"), XElement)
                                          Dim Episodes As New List(Of EpisodeElement)
                                          Dim UnEpisodes As New List(Of EpisodeElement)
                                          If XEpisodes IsNot Nothing Then
                                              For Each Episode As XElement In XEpisodes.Nodes
                                                  Dim EID = Episode.Attribute("id")?.Value
                                                  Dim XUDate = Episode.Attribute("update")?.Value
                                                  Dim SXUDate = XUDate?.Split("-")
                                                  Dim UDate As Date = If(UseLegacyDateParsing, If(SXUDate IsNot Nothing, If(SXUDate.Length = 3, New Date(SXUDate(0), SXUDate(1), SXUDate(2)), Date.MinValue), Date.MinValue), Date.ParseExact(If(String.IsNullOrEmpty(XUDate), "0001-01-01", XUDate), "yyyy-MM-dd", Globalization.DateTimeFormatInfo.InvariantInfo, Globalization.DateTimeStyles.None))
                                                  Dim XEpNo = TryCast(Episode.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "epno"), XElement)
                                                  Dim EpNo = If(XEpNo IsNot Nothing, XEpNo.Value, Nothing)
                                                  Dim EType = If(XEpNo IsNot Nothing, XEpNo.Attribute("type")?.Value, Nothing)
                                                  Dim Length = TryCast(Episode.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "length"), XElement)?.Value
                                                  Dim XADate = TryCast(Episode.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "airdate"), XElement)?.Value
                                                  Dim SXADate = XADate?.Split("-")
                                                  Dim ADate As Date = If(UseLegacyDateParsing, If(SXADate IsNot Nothing, If(SXADate.Length = 3, New Date(SXADate(0), SXADate(1), SXADate(2)), Date.MinValue), Date.MinValue), Date.ParseExact(If(String.IsNullOrEmpty(XADate), "0001-01-01", XADate), "yyyy-MM-dd", Globalization.DateTimeFormatInfo.InvariantInfo, Globalization.DateTimeStyles.None))
                                                  Dim XRating = TryCast(Episode.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "rating"), XElement)
                                                  Dim Rating As New RatingElement(RatingElement.RatingType.Parmanent, CInt(XRating?.Attribute("votes")?.Value), CDbl(XRating?.Value))
                                                  Dim ETitles As New List(Of TitleElement)
                                                  For Each XTitleN As XElement In Episode.Nodes
                                                      If XTitleN.Name = "title" Then
                                                          ETitles.Add(New TitleElement(XTitleN.FirstAttribute?.Value, "main", XTitleN.Value))
                                                      End If
                                                  Next
                                                  Dim Summary As String = TryCast(Episode.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "summary"), XElement)?.Value
                                                  Dim XEResources = TryCast(Episode.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "resources"), XElement)
                                                  Dim EResources As New List(Of ResourceElement)
                                                  If XEResources IsNot Nothing Then
                                                      For Each resource As XElement In XEResources.Nodes
                                                          Dim ResType = resource.Attribute("type")?.Value
                                                          Dim ExtEnt As New List(Of ResourceElement.ExternalEntityElement)
                                                          For Each XExtEnt As XElement In resource.Nodes
                                                              Dim XEEE As New ResourceElement.ExternalEntityElement
                                                              For Each ExtEntID As XElement In XExtEnt.Nodes
                                                                  XEEE.Identifiers.Add(ExtEntID.Value)
                                                              Next
                                                              ExtEnt.Add(XEEE)
                                                          Next
                                                          EResources.Add(New ResourceElement With {.Type = ResType, .ExternalEntities = ExtEnt})
                                                      Next
                                                  End If
                                                  Dim REpNo As String
                                                  If Integer.TryParse(EpNo, REpNo) Then
                                                      Episodes.Add(New EpisodeElement With {.ID = EID, .EditDate = UDate, .Number = EpNo, .Type = EType, .Length = Length, .AirDate = ADate, .Rating = Rating, .Titles = ETitles, .Summary = Summary, .Resources = EResources})
                                                  Else
                                                      UnEpisodes.Add(New EpisodeElement With {.ID = EID, .EditDate = UDate, .Number = EpNo, .Type = EType, .Length = Length, .AirDate = ADate, .Rating = Rating, .Titles = ETitles, .Summary = Summary, .Resources = EResources})
                                                  End If
                                              Next
                                              Episodes = Episodes.OrderBy(Function(k) CInt(k.Number)).ToList
                                          End If
                                          Return New AnimeElement(ID, Restricted, Type, EpisodeCount, StartDate, EndDate, Titles, RelatedAnime, SimilarAnime, Recommendations, Creators, Description, Ratings, PictureURL, Picture, Resources, Tags, Characters, Episodes) With {.UnmanagedXMLData = xml, .UnRelatedEpisodes = UnEpisodes}
                                      End Function)
            End Function
            ''' <summary>
            ''' Returns managed data for AniDB API XML Response
            ''' </summary>
            ''' <param name="xml">XML data</param>
            ''' <param name="PreloadPicture">Pre-download Anime Picture</param>
            ''' <returns></returns>
            Public Shared Async Function ParseAnimeAniDBXML(xml As XDocument, Optional PreloadPicture As Boolean = True, Optional FetchCharacters As Boolean = True, Optional PreloadCharacterPicture As Boolean = True) As Task(Of AnimeElement)
                Return Await Task.Run(Async Function()
                                          Dim XDoc = xml
                                          Dim Root = XDoc.Root 'anime
                                          Dim RootNodes = Root.Nodes
                                          Dim ID As Integer = Root.FirstAttribute.Value 'xxxx        
                                          Dim Restricted As Boolean = Root.LastAttribute.Value
                                          Dim XType = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "type"), XElement)
                                          Dim Type = If(XType IsNot Nothing, XType.Value, Nothing)
                                          Dim XEpisodeCount = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "episodecount"), XElement)
                                          Dim EpisodeCount As Integer = If(XEpisodeCount IsNot Nothing, XEpisodeCount.Value, 0)
                                          Dim XStartDate = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "startdate"), XElement)?.Value 'YYYY-MM-DD
                                          Dim SXStartDate = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "startdate"), XElement)?.Value.Split("-") 'YYYY-MM-DD
                                          Dim StartDate As Date = If(UseLegacyDateParsing, If(SXStartDate IsNot Nothing, If(SXStartDate.Length = 3, New Date(SXStartDate(0), SXStartDate(1), SXStartDate(2)), Date.MinValue), Date.MinValue), Date.ParseExact(If(String.IsNullOrEmpty(XStartDate), "0001-01-01", XStartDate), "yyyy-MM-dd", Globalization.DateTimeFormatInfo.InvariantInfo, Globalization.DateTimeStyles.None))
                                          Dim XEndDate = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "enddate"), XElement)?.Value 'YYYY-MM-DD
                                          Dim SXEndDate = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "enddate"), XElement)?.Value.Split("-") 'YYYY-MM-DD
                                          Dim EndDate As Date = If(UseLegacyDateParsing, If(SXEndDate IsNot Nothing, If(SXEndDate.Length = 3, New Date(SXEndDate(0), SXEndDate(1), SXEndDate(2)), Date.MinValue), Date.MinValue), Date.ParseExact(If(String.IsNullOrEmpty(XEndDate), "0001-01-01", XEndDate), "yyyy-MM-dd", Globalization.DateTimeFormatInfo.InvariantInfo, Globalization.DateTimeStyles.None))
                                          Dim Titles As New List(Of TitleElement)
                                          Dim XTitles = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "titles"), XElement)
                                          If XTitles IsNot Nothing Then
                                              For Each title As XElement In XTitles.Nodes
                                                  Titles.Add(New TitleElement(title.FirstAttribute.Value, title.LastAttribute.Value, title.Value))
                                              Next
                                          End If
                                          Dim RelatedAnime As New List(Of RelatedAnimeElement)
                                          Dim XRelatedAnime = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "relatedanime"), XElement)
                                          If XRelatedAnime IsNot Nothing Then
                                              For Each anime As XElement In XRelatedAnime.Nodes
                                                  RelatedAnime.Add(New RelatedAnimeElement(CInt(anime.FirstAttribute.Value), anime.LastAttribute.Value, anime.Value))
                                              Next
                                          End If
                                          Dim SimilarAnime As New List(Of SimilarAnimeElement)
                                          Dim XSimilarAnime = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "similaranime"), XElement)
                                          If XSimilarAnime IsNot Nothing Then
                                              For Each anime As XElement In XSimilarAnime.Nodes
                                                  SimilarAnime.Add(New SimilarAnimeElement(anime.FirstAttribute.Value, anime.FirstAttribute.NextAttribute.Value, anime.LastAttribute.Value, anime.Value))
                                              Next
                                          End If
                                          Dim Xrecommendations = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "recommendations"), XElement) 'Needs Work !!!
                                          Dim Recommendations As String = If(Xrecommendations IsNot Nothing, Xrecommendations.ToString, Nothing)
                                          Dim XURL = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "url"), XElement)
                                          Dim URL As String = If(XURL IsNot Nothing, XURL.Value, Nothing)
                                          Dim Creators As New List(Of CreatorsElement)
                                          Dim XCreators = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "creators"), XElement)
                                          If XCreators IsNot Nothing Then
                                              For Each creator As XElement In XCreators.Nodes
                                                  Creators.Add(New CreatorsElement(creator.FirstAttribute.Value, creator.LastAttribute.Value, creator.Value))
                                              Next
                                          End If
                                          Dim XDescription = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "description"), XElement)
                                          Dim Description As String = If(XDescription IsNot Nothing, XDescription.Value, Nothing)
                                          Dim Ratings As New List(Of RatingElement)
                                          Dim XRatings = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "ratings"), XElement)
                                          If XRatings IsNot Nothing Then
                                              For Each rating As XElement In XRatings.Nodes
                                                  Ratings.Add(New RatingElement(rating.Name.LocalName, CInt(rating.FirstAttribute.Value), CDbl(rating.Value)))
                                              Next
                                          End If
                                          Dim XPictureURL = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "picture"), XElement)
                                          Dim PictureURL As String = If(XPictureURL IsNot Nothing, XPictureURL.Value, Nothing)
                                          Dim Picture As System.Drawing.Image = If(PreloadPicture, If(String.IsNullOrEmpty(PictureURL), Nothing, Await GetAniDBImageAsync(PictureURL)), Nothing)
                                          Dim XResources = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "resources"), XElement)
                                          Dim Resources As New List(Of ResourceElement)
                                          If XResources IsNot Nothing Then
                                              For Each resource As XElement In XResources.Nodes
                                                  Dim ResType = resource.Attribute("type")?.Value
                                                  Dim ExtEnt As New List(Of ResourceElement.ExternalEntityElement)
                                                  For Each XExtEnt As XElement In resource.Nodes
                                                      Dim XEEE As New ResourceElement.ExternalEntityElement
                                                      For Each ExtEntID As XElement In XExtEnt.Nodes
                                                          XEEE.Identifiers.Add(ExtEntID.Value)
                                                      Next
                                                      ExtEnt.Add(XEEE)
                                                  Next
                                                  Resources.Add(New ResourceElement With {.Type = ResType, .ExternalEntities = ExtEnt})
                                              Next
                                          End If
                                          Dim Tags As New List(Of TagElement)
                                          Dim XTags = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "tags"), XElement)
                                          If XTags IsNot Nothing Then
                                              For Each tag As XElement In XTags.Nodes
                                                  If FilterTags Then
                                                      Dim IsInfoBox = tag.Attribute("infobox")
                                                      If IsInfoBox IsNot Nothing Then
                                                          Tags.Add(New TagElement(CType(tag.FirstNode, XElement)?.Value, CType(tag.FirstNode.NextNode, XElement)?.Value))
                                                      End If
                                                  Else
                                                      Tags.Add(New TagElement(CType(tag.FirstNode, XElement)?.Value, CType(tag.FirstNode.NextNode, XElement)?.Value))
                                                  End If
                                              Next
                                          End If
                                          Dim Characters As New List(Of CharacterElement)
                                          If FetchCharacters Then
                                              Dim XCharacters = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "characters"), XElement)
                                              If XCharacters IsNot Nothing Then
                                                  For Each character As XElement In XCharacters.Nodes
                                                      Characters.Add(New CharacterElement(CInt(character.FirstAttribute.Value),
                                                            character.FirstAttribute.NextAttribute.Value,
                                                            New RatingElement(RatingElement.RatingType.Parmanent, CInt(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "rating"), XElement)?.FirstAttribute.Value), CDbl(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "rating"), XElement)?.Value)),
                                                            CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "name"), XElement)?.Value,
                                                           CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "gender"), XElement)?.Value,
                                                            CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "description"), XElement)?.Value,
                                                            CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "picture"), XElement)?.Value,
                                                             If(PreloadCharacterPicture, Await GetAniDBImageAsync(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "picture"), XElement)?.Value), Nothing),
                                                            CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "seiyuu"), XElement)?.Value,
                                                            CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "seiyuu"), XElement)?.LastAttribute.Value,
                                                           CInt(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "seiyuu"), XElement)?.FirstAttribute.Value)))
                                                      If Characters.Count >= CharacterCap Then Exit For
                                                  Next
                                              End If
                                          End If
                                          Dim XEpisodes As XElement = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "episodes"), XElement)
                                          Dim Episodes As New List(Of EpisodeElement)
                                          Dim UnEpisodes As New List(Of EpisodeElement)
                                          If XEpisodes IsNot Nothing Then
                                              For Each Episode As XElement In XEpisodes.Nodes
                                                  Dim EID = Episode.Attribute("id")?.Value
                                                  Dim XUDate = Episode.Attribute("update")?.Value
                                                  Dim SXUDate = XUDate?.Split("-")
                                                  Dim UDate As Date = If(UseLegacyDateParsing, If(SXUDate IsNot Nothing, If(SXUDate.Length = 3, New Date(SXUDate(0), SXUDate(1), SXUDate(2)), Date.MinValue), Date.MinValue), Date.ParseExact(If(String.IsNullOrEmpty(XUDate), "0001-01-01", XUDate), "yyyy-MM-dd", Globalization.DateTimeFormatInfo.InvariantInfo, Globalization.DateTimeStyles.None))
                                                  Dim XEpNo = TryCast(Episode.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "epno"), XElement)
                                                  Dim EpNo = If(XEpNo IsNot Nothing, XEpNo.Value, Nothing)
                                                  Dim EType = If(XEpNo IsNot Nothing, XEpNo.Attribute("type")?.Value, Nothing)
                                                  Dim Length = TryCast(Episode.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "length"), XElement)?.Value
                                                  Dim XADate = TryCast(Episode.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "airdate"), XElement)?.Value
                                                  Dim SXADate = XADate?.Split("-")
                                                  Dim ADate As Date = If(UseLegacyDateParsing, If(SXADate IsNot Nothing, If(SXADate.Length = 3, New Date(SXADate(0), SXADate(1), SXADate(2)), Date.MinValue), Date.MinValue), Date.ParseExact(If(String.IsNullOrEmpty(XADate), "0001-01-01", XADate), "yyyy-MM-dd", Globalization.DateTimeFormatInfo.InvariantInfo, Globalization.DateTimeStyles.None))
                                                  Dim XRating = TryCast(Episode.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "rating"), XElement)
                                                  Dim Rating As New RatingElement(RatingElement.RatingType.Parmanent, CInt(XRating?.Attribute("votes")?.Value), CDbl(XRating?.Value))
                                                  Dim ETitles As New List(Of TitleElement)
                                                  For Each XTitleN As XElement In Episode.Nodes
                                                      If XTitleN.Name = "title" Then
                                                          ETitles.Add(New TitleElement(XTitleN.FirstAttribute?.Value, "main", XTitleN.Value))
                                                      End If
                                                  Next
                                                  Dim Summary As String = TryCast(Episode.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "summary"), XElement)?.Value
                                                  Dim XEResources = TryCast(Episode.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "resources"), XElement)
                                                  Dim EResources As New List(Of ResourceElement)
                                                  If XEResources IsNot Nothing Then
                                                      For Each resource As XElement In XEResources.Nodes
                                                          Dim ResType = resource.Attribute("type")?.Value
                                                          Dim ExtEnt As New List(Of ResourceElement.ExternalEntityElement)
                                                          For Each XExtEnt As XElement In resource.Nodes
                                                              Dim XEEE As New ResourceElement.ExternalEntityElement
                                                              For Each ExtEntID As XElement In XExtEnt.Nodes
                                                                  XEEE.Identifiers.Add(ExtEntID.Value)
                                                              Next
                                                              ExtEnt.Add(XEEE)
                                                          Next
                                                          EResources.Add(New ResourceElement With {.Type = ResType, .ExternalEntities = ExtEnt})
                                                      Next
                                                  End If
                                                  Dim REpNo As String
                                                  If Integer.TryParse(EpNo, REpNo) Then
                                                      Episodes.Add(New EpisodeElement With {.ID = EID, .EditDate = UDate, .Number = EpNo, .Type = EType, .Length = Length, .AirDate = ADate, .Rating = Rating, .Titles = ETitles, .Summary = Summary, .Resources = EResources})
                                                  Else
                                                      UnEpisodes.Add(New EpisodeElement With {.ID = EID, .EditDate = UDate, .Number = EpNo, .Type = EType, .Length = Length, .AirDate = ADate, .Rating = Rating, .Titles = ETitles, .Summary = Summary, .Resources = EResources})
                                                  End If
                                              Next
                                              Episodes = Episodes.OrderBy(Function(k) CInt(k.Number)).ToList
                                          End If
                                          Return New AnimeElement(ID, Restricted, Type, EpisodeCount, StartDate, EndDate, Titles, RelatedAnime, SimilarAnime, Recommendations, Creators, Description, Ratings, PictureURL, Picture, Resources, Tags, Characters, Episodes) With {.UnmanagedXMLData = xml.ToString, .UnRelatedEpisodes = UnEpisodes}
                                      End Function)
            End Function
            Public Shared Async Function ParseCharacterFromAnimeAniDBXML(xml As String, Optional PreloadPicture As Boolean = True) As Task(Of List(Of CharacterElement))
                Return Await Task.Run(Async Function()
                                          Dim XDoc = XDocument.Parse(xml)
                                          Dim Root = XDoc.Root 'anime
                                          Dim RootNodes = Root.Nodes
                                          Dim Characters As New List(Of CharacterElement)
                                          Dim XCharacters = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "characters"), XElement)
                                          If XCharacters IsNot Nothing Then
                                              For Each character As XElement In XCharacters.Nodes
                                                  Characters.Add(New CharacterElement(CInt(character.FirstAttribute.Value),
                                                        character.FirstAttribute.NextAttribute.Value,
                                                        New RatingElement(RatingElement.RatingType.Parmanent, CInt(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "rating"), XElement)?.FirstAttribute.Value), CDbl(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "rating"), XElement)?.Value)),
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "name"), XElement)?.Value,
                                                       CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "gender"), XElement)?.Value,
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "description"), XElement)?.Value,
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "picture"), XElement)?.Value,
                                                         If(PreloadPicture, Await GetAniDBImageAsync(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "picture"), XElement)?.Value), Nothing),
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "seiyuu"), XElement)?.Value,
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "seiyuu"), XElement)?.LastAttribute.Value,
                                                       CInt(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "seiyuu"), XElement)?.FirstAttribute.Value)))
                                                  If CharacterCap <> -1 Then
                                                      If Characters.Count >= CharacterCap Then Exit For
                                                  End If
                                              Next
                                          End If
                                          Return Characters
                                      End Function)
            End Function
            ''' <summary>
            ''' Returns character only managed data for AniDB API XML Response
            ''' </summary>
            ''' <param name="xml">String XML Data</param>
            ''' <param name="PreloadPicture"></param>
            ''' <returns></returns>
            Public Shared Async Function ParseCharacterFromAnimeAniDBXML(xml As XDocument, Optional PreloadPicture As Boolean = True) As Task(Of List(Of CharacterElement))
                Return Await Task.Run(Async Function()
                                          Dim XDoc = xml
                                          Dim Root = XDoc.Root 'anime
                                          Dim RootNodes = Root.Nodes
                                          Dim Characters As New List(Of CharacterElement)
                                          Dim XCharacters = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "characters"), XElement)
                                          If XCharacters IsNot Nothing Then
                                              For Each character As XElement In XCharacters.Nodes
                                                  Characters.Add(New CharacterElement(CInt(character.FirstAttribute.Value),
                                                        character.FirstAttribute.NextAttribute.Value,
                                                        New RatingElement(RatingElement.RatingType.Parmanent, CInt(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "rating"), XElement)?.FirstAttribute.Value), CDbl(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "rating"), XElement)?.Value)),
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "name"), XElement)?.Value,
                                                       CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "gender"), XElement)?.Value,
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "description"), XElement)?.Value,
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "picture"), XElement)?.Value,
                                                         If(PreloadPicture, Await GetAniDBImageAsync(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "picture"), XElement)?.Value), Nothing),
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "seiyuu"), XElement)?.Value,
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "seiyuu"), XElement)?.LastAttribute.Value,
                                                       CInt(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "seiyuu"), XElement)?.FirstAttribute.Value)))
                                                  If CharacterCap <> -1 Then
                                                      If Characters.Count >= CharacterCap Then Exit For
                                                  End If
                                              Next
                                          End If
                                          Return Characters
                                      End Function)
            End Function
            ''' <summary>
            ''' Returns character only managed data for AniDB API XML Response
            ''' </summary>
            ''' <param name="xml">String XML Data</param>
            ''' <param name="PreloadPicture"></param>
            ''' <returns></returns>
            Public Shared Async Function ParseExactCharacterFromAnimeAniDBXML(xml As String, ID As Integer, Optional PreloadPicture As Boolean = True) As Task(Of CharacterElement)
                Return Await Task.Run(Async Function()
                                          Dim XDoc = XDocument.Parse(xml)
                                          Dim Root = XDoc.Root 'anime
                                          Dim RootNodes = Root.Nodes
                                          Dim SCharacter As CharacterElement = Nothing
                                          Dim XCharacters = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "characters"), XElement)
                                          If XCharacters IsNot Nothing Then
                                              For Each character As XElement In XCharacters.Nodes
                                                  If CInt(character.FirstAttribute.Value) = ID Then
                                                      SCharacter = New CharacterElement(CInt(character.FirstAttribute.Value),
                                                        character.FirstAttribute.NextAttribute.Value,
                                                        New RatingElement(RatingElement.RatingType.Parmanent, CInt(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "rating"), XElement)?.FirstAttribute.Value), CDbl(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "rating"), XElement)?.Value)),
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "name"), XElement)?.Value,
                                                       CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "gender"), XElement)?.Value,
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "description"), XElement)?.Value,
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "picture"), XElement)?.Value,
                                                         If(PreloadPicture, Await GetAniDBImageAsync(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "picture"), XElement)?.Value), Nothing),
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "seiyuu"), XElement)?.Value,
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "seiyuu"), XElement)?.LastAttribute.Value,
                                                       CInt(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "seiyuu"), XElement)?.FirstAttribute.Value))
                                                      Exit For
                                                  End If
                                              Next
                                          End If
                                          Return SCharacter
                                      End Function)
            End Function
            ''' <summary>
            ''' Returns character only managed data for AniDB API XML Response
            ''' </summary>
            ''' <param name="xml">String XML Data</param>
            ''' <param name="PreloadPicture"></param>
            ''' <returns></returns>
            Public Shared Async Function ParseExactCharacterFromAnimeAniDBXML(xml As XDocument, ID As Integer, Optional PreloadPicture As Boolean = True) As Task(Of CharacterElement)
                Return Await Task.Run(Async Function()
                                          Dim XDoc = xml
                                          Dim Root = XDoc.Root 'anime
                                          Dim RootNodes = Root.Nodes
                                          Dim SCharacter As CharacterElement = Nothing
                                          Dim XCharacters = TryCast(RootNodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "characters"), XElement)
                                          If XCharacters IsNot Nothing Then
                                              For Each character As XElement In XCharacters.Nodes
                                                  If CInt(character.FirstAttribute.Value) = ID Then
                                                      SCharacter = New CharacterElement(CInt(character.FirstAttribute.Value),
                                                        character.FirstAttribute.NextAttribute.Value,
                                                        New RatingElement(RatingElement.RatingType.Parmanent, CInt(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "rating"), XElement)?.FirstAttribute.Value), CDbl(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "rating"), XElement)?.Value)),
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "name"), XElement)?.Value,
                                                       CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "gender"), XElement)?.Value,
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "description"), XElement)?.Value,
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "picture"), XElement)?.Value,
                                                         If(PreloadPicture, Await GetAniDBImageAsync(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "picture"), XElement)?.Value), Nothing),
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "seiyuu"), XElement)?.Value,
                                                        CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "seiyuu"), XElement)?.LastAttribute.Value,
                                                       CInt(CType(character.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "seiyuu"), XElement)?.FirstAttribute.Value))
                                                      Exit For
                                                  End If
                                              Next
                                          End If
                                          Return SCharacter
                                      End Function)
            End Function
            ''' <summary>
            ''' Returns managed data for AniDB API XML Response
            ''' </summary>
            ''' <param name="xml">XML data</param>
            ''' <param name="PreloadPicture">Pre-download Anime Picture</param>
            ''' <returns></returns>
            Public Shared Async Function ParseRandomRecommendationAniDBXML(xml As String, Optional PreloadPicture As Boolean = True) As Task(Of List(Of RecommendationElement))
                Dim XDoc = XDocument.Parse(xml)
                Dim Root = XDoc.Root 'randomrecommendation
                Dim Recs As New List(Of RecommendationElement)
                For Each recommendation As XElement In Root.Nodes
                    Dim anime = CType(recommendation.FirstNode, XElement)
                    Dim ID As Integer = anime.FirstAttribute.Value
                    Dim Restricted As Boolean = anime.LastAttribute.Value
                    Dim Type = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "type"), XElement)?.Value
                    Dim EpisodeCount As Integer = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "episodecount"), XElement)?.Value
                    Dim XStartDate = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "startdate"), XElement)?.Value.Split("-") 'YYYY-MM-DD
                    Dim StartDate As Date = If(XStartDate IsNot Nothing, New Date(XStartDate(0), XStartDate(1), XStartDate(2)), Nothing)
                    Dim XEndDate = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "enddate"), XElement)?.Value.Split("-") 'YYYY-MM-DD
                    Dim EndDate As Date = If(XEndDate IsNot Nothing, New Date(XEndDate(0), XEndDate(1), XEndDate(2)), Nothing)
                    Dim XTitle = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "title"), XElement)
                    Dim Title As TitleElement = If(XTitle IsNot Nothing, New TitleElement(XTitle.FirstAttribute.Value, XTitle.LastAttribute.Value, XTitle.Value), Nothing)
                    Dim PictureURL = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "picture"), XElement)?.Value
                    Dim Picture As System.Drawing.Image = If(PreloadPicture, Await GetAniDBImageAsync(PictureURL), Nothing)
                    Dim XRatings = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "ratings"), XElement)
                    Dim Ratings As New List(Of RatingElement)
                    If XRatings IsNot Nothing Then
                        For Each rating As XElement In XRatings.Nodes
                            Ratings.Add(New RatingElement(rating.Name.LocalName, If(rating.FirstAttribute IsNot Nothing, CInt(rating.FirstAttribute.Value), 0), CDbl(rating.Value.Insert(1, "."))))
                        Next
                    End If
                    Recs.Add(New RecommendationElement(ID, Restricted, Type, EpisodeCount, StartDate, EndDate, Title, PictureURL, Picture, Ratings))
                Next
                Return Recs
            End Function
            ''' <summary>
            ''' Returns managed data for AniDB API XML Response
            ''' </summary>
            ''' <param name="xml">XML data</param>
            ''' <param name="PreloadPicture">Pre-download Anime Picture</param>
            ''' <returns></returns>
            Public Shared Async Function ParseRandomRecommendationAniDBXML(xml As XDocument, Optional PreloadPicture As Boolean = True) As Task(Of List(Of RecommendationElement))
                Dim XDoc = xml
                Dim Root = XDoc.Root 'randomrecommendation
                Dim Recs As New List(Of RecommendationElement)
                For Each recommendation As XElement In Root.Nodes
                    Dim anime = CType(recommendation.FirstNode, XElement)
                    Dim ID As Integer = anime.FirstAttribute.Value
                    Dim Restricted As Boolean = anime.LastAttribute.Value
                    Dim Type = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "type"), XElement)?.Value
                    Dim EpisodeCount As Integer = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "episodecount"), XElement)?.Value
                    Dim XStartDate = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "startdate"), XElement)?.Value.Split("-") 'YYYY-MM-DD
                    Dim StartDate As Date = If(XStartDate IsNot Nothing, New Date(XStartDate(0), XStartDate(1), XStartDate(2)), Nothing)
                    Dim XEndDate = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "enddate"), XElement)?.Value.Split("-") 'YYYY-MM-DD
                    Dim EndDate As Date = If(XEndDate IsNot Nothing, New Date(XEndDate(0), XEndDate(1), XEndDate(2)), Nothing)
                    Dim XTitle = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "title"), XElement)
                    Dim Title As TitleElement = If(XTitle IsNot Nothing, New TitleElement(XTitle.FirstAttribute.Value, XTitle.LastAttribute.Value, XTitle.Value), Nothing)
                    Dim PictureURL = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "picture"), XElement)?.Value
                    Dim Picture As System.Drawing.Image = If(PreloadPicture, Await GetAniDBImageAsync(PictureURL), Nothing)
                    Dim XRatings = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "ratings"), XElement)
                    Dim Ratings As New List(Of RatingElement)
                    If XRatings IsNot Nothing Then
                        For Each rating As XElement In XRatings.Nodes
                            Ratings.Add(New RatingElement(rating.Name.LocalName, If(rating.FirstAttribute IsNot Nothing, CInt(rating.FirstAttribute.Value), 0), CDbl(rating.Value.Insert(1, "."))))
                        Next
                    End If
                    Recs.Add(New RecommendationElement(ID, Restricted, Type, EpisodeCount, StartDate, EndDate, Title, PictureURL, Picture, Ratings))
                Next
                Return Recs
            End Function
            ''' <summary>
            ''' Returns managed data for AniDB API XML Response
            ''' </summary>
            ''' <param name="xml">XML data</param>
            ''' <param name="PreloadPicture">Pre-download Anime Picture</param>
            ''' <returns></returns>
            Public Shared Async Function ParseRandomSimilarAniDBXML(xml As String, Optional PreloadPicture As Boolean = True) As Task(Of List(Of SimilarElement))
                Dim XDoc = XDocument.Parse(xml)
                Dim Root = XDoc.Root 'randomsimilar
                Dim Similars As New List(Of SimilarElement)
                For Each similar As XElement In Root.Nodes
                    Dim XSource = CType(similar.FirstNode, XElement)
                    Dim ID As Integer = XSource.FirstAttribute.Value
                    Dim Restricted As Boolean = XSource.LastAttribute.Value
                    Dim XTitle = TryCast(XSource.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "title"), XElement)
                    Dim Title As TitleElement = If(XTitle IsNot Nothing, New TitleElement(XTitle.FirstAttribute.Value, XTitle.LastAttribute.Value, XTitle.Value), Nothing)
                    Dim PictureURL = TryCast(XSource.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "picture"), XElement)?.Value
                    Dim Picture As System.Drawing.Image = If(PreloadPicture, Await GetAniDBImageAsync(PictureURL), Nothing)
                    Dim Source As New SimilarElement.SimilarItem(ID, Restricted, Title, PictureURL, Picture)
                    Dim XTarget = CType(similar.LastNode, XElement)
                    Dim _ID As Integer = XTarget.FirstAttribute.Value
                    Dim _Restricted As Boolean = XTarget.LastAttribute.Value
                    Dim _XTitle = TryCast(XTarget.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "title"), XElement)
                    Dim _Title As TitleElement = If(_XTitle IsNot Nothing, New TitleElement(_XTitle.FirstAttribute.Value, _XTitle.LastAttribute.Value, _XTitle.Value), Nothing)
                    Dim _PictureURL = TryCast(XTarget.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "picture"), XElement)?.Value
                    Dim _Picture As System.Drawing.Image = If(PreloadPicture, Await GetAniDBImageAsync(_PictureURL), Nothing)
                    Dim Target As New SimilarElement.SimilarItem(_ID, _Restricted, _Title, _PictureURL, _Picture)
                    Similars.Add(New SimilarElement(Source, Target))
                Next
                Return Similars
            End Function
            ''' <summary>
            ''' Returns managed data for AniDB API XML Response
            ''' </summary>
            ''' <param name="xml">XML data</param>
            ''' <param name="PreloadPicture">Pre-download Anime Picture</param>
            ''' <returns></returns>
            Public Shared Async Function ParseRandomSimilarAniDBXML(xml As XDocument, Optional PreloadPicture As Boolean = True) As Task(Of List(Of SimilarElement))
                Dim XDoc = xml
                Dim Root = XDoc.Root 'randomsimilar
                Dim Similars As New List(Of SimilarElement)
                For Each similar As XElement In Root.Nodes
                    Dim XSource = CType(similar.FirstNode, XElement)
                    Dim ID As Integer = XSource.FirstAttribute.Value
                    Dim Restricted As Boolean = XSource.LastAttribute.Value
                    Dim XTitle = TryCast(XSource.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "title"), XElement)
                    Dim Title As TitleElement = If(XTitle IsNot Nothing, New TitleElement(XTitle.FirstAttribute.Value, XTitle.LastAttribute.Value, XTitle.Value), Nothing)
                    Dim PictureURL = TryCast(XSource.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "picture"), XElement)?.Value
                    Dim Picture As System.Drawing.Image = If(PreloadPicture, Await GetAniDBImageAsync(PictureURL), Nothing)
                    Dim Source As New SimilarElement.SimilarItem(ID, Restricted, Title, PictureURL, Picture)
                    Dim XTarget = CType(similar.LastNode, XElement)
                    Dim _ID As Integer = XTarget.FirstAttribute.Value
                    Dim _Restricted As Boolean = XTarget.LastAttribute.Value
                    Dim _XTitle = TryCast(XTarget.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "title"), XElement)
                    Dim _Title As TitleElement = If(_XTitle IsNot Nothing, New TitleElement(_XTitle.FirstAttribute.Value, _XTitle.LastAttribute.Value, _XTitle.Value), Nothing)
                    Dim _PictureURL = TryCast(XTarget.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "picture"), XElement)?.Value
                    Dim _Picture As System.Drawing.Image = If(PreloadPicture, Await GetAniDBImageAsync(_PictureURL), Nothing)
                    Dim Target As New SimilarElement.SimilarItem(_ID, _Restricted, _Title, _PictureURL, _Picture)
                    Similars.Add(New SimilarElement(Source, Target))
                Next
                Return Similars
            End Function
            ''' <summary>
            ''' Return managed data for AniDB API XML Response
            ''' </summary>
            ''' <param name="xml">XML Data</param>
            ''' <param name="PreloadPicture">Pre-download Anime Picture</param>
            ''' <returns></returns>
            Public Shared Async Function ParseHotAnimeAniDBXML(xml As String, Optional PreloadPicture As Boolean = True) As Task(Of List(Of RecommendationElement))
                Dim XDoc = XDocument.Parse(xml)
                Dim Root = XDoc.Root 'hotanime
                Dim Recs As New List(Of RecommendationElement)
                For Each anime As XElement In Root.Nodes
                    Dim ID As Integer = anime.FirstAttribute.Value
                    Dim Restricted As Boolean = anime.LastAttribute.Value
                    Dim Type = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "type"), XElement)?.Value
                    Dim EpisodeCount As Integer = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "episodecount"), XElement)?.Value
                    Dim XStartDate = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "startdate"), XElement)?.Value.Split("-") 'YYYY-MM-DD
                    Dim StartDate As Date = If(XStartDate IsNot Nothing, New Date(XStartDate(0), XStartDate(1), XStartDate(2)), Nothing)
                    Dim XEndDate = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "enddate"), XElement)?.Value.Split("-") 'YYYY-MM-DD
                    Dim EndDate As Date = If(XEndDate IsNot Nothing, New Date(XEndDate(0), XEndDate(1), XEndDate(2)), Nothing)
                    Dim XTitle = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "title"), XElement)
                    Dim Title As TitleElement = If(XTitle IsNot Nothing, New TitleElement(XTitle.FirstAttribute.Value, XTitle.LastAttribute.Value, XTitle.Value), Nothing)
                    Dim PictureURL = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "picture"), XElement)?.Value
                    Dim Picture As System.Drawing.Image = If(PreloadPicture, Await GetAniDBImageAsync(PictureURL), Nothing)
                    Dim XRatings = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "ratings"), XElement)
                    Dim Ratings As New List(Of RatingElement)
                    If XRatings IsNot Nothing Then
                        For Each rating As XElement In XRatings.Nodes
                            Ratings.Add(New RatingElement(rating.Name.LocalName, If(rating.FirstAttribute IsNot Nothing, CInt(rating.FirstAttribute.Value), 0), CDbl(rating.Value)))
                        Next
                    End If
                    Recs.Add(New RecommendationElement(ID, Restricted, If(Type IsNot Nothing, Type, "TV Series"), EpisodeCount, StartDate, EndDate, Title, PictureURL, Picture, Ratings))
                Next
                Return Recs
            End Function
            ''' <summary>
            ''' Return managed data for AniDB API XML Response
            ''' </summary>
            ''' <param name="xml">XML Data</param>
            ''' <param name="PreloadPicture">Pre-download Anime Picture</param>
            ''' <returns></returns>
            Public Shared Async Function ParseHotAnimeAniDBXML(xml As XDocument, Optional PreloadPicture As Boolean = True) As Task(Of List(Of RecommendationElement))
                Dim XDoc = xml
                Dim Root = XDoc.Root 'hotanime
                Dim Recs As New List(Of RecommendationElement)
                For Each anime As XElement In Root.Nodes
                    Dim ID As Integer = anime.FirstAttribute.Value
                    Dim Restricted As Boolean = anime.LastAttribute.Value
                    Dim Type = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "type"), XElement)?.Value
                    Dim EpisodeCount As Integer = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "episodecount"), XElement)?.Value
                    Dim XStartDate = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "startdate"), XElement)?.Value.Split("-") 'YYYY-MM-DD
                    Dim StartDate As Date = If(XStartDate IsNot Nothing, New Date(XStartDate(0), XStartDate(1), XStartDate(2)), Nothing)
                    Dim XEndDate = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "enddate"), XElement)?.Value.Split("-") 'YYYY-MM-DD
                    Dim EndDate As Date = If(XEndDate IsNot Nothing, New Date(XEndDate(0), XEndDate(1), XEndDate(2)), Nothing)
                    Dim XTitle = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "title"), XElement)
                    Dim Title As TitleElement = If(XTitle IsNot Nothing, New TitleElement(XTitle.FirstAttribute.Value, XTitle.LastAttribute.Value, XTitle.Value), Nothing)
                    Dim PictureURL = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "picture"), XElement)?.Value
                    Dim Picture As System.Drawing.Image = If(PreloadPicture, Await GetAniDBImageAsync(PictureURL), Nothing)
                    Dim XRatings = TryCast(anime.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "ratings"), XElement)
                    Dim Ratings As New List(Of RatingElement)
                    If XRatings IsNot Nothing Then
                        For Each rating As XElement In XRatings.Nodes
                            Ratings.Add(New RatingElement(rating.Name.LocalName, If(rating.FirstAttribute IsNot Nothing, CInt(rating.FirstAttribute.Value), 0), CDbl(rating.Value)))
                        Next
                    End If
                    Recs.Add(New RecommendationElement(ID, Restricted, If(Type IsNot Nothing, Type, "TV Series"), EpisodeCount, StartDate, EndDate, Title, PictureURL, Picture, Ratings))
                Next
                Return Recs
            End Function
            ''' <summary>
            ''' Return managed data for AniDB API XML Response
            ''' </summary>
            ''' <param name="xml">XML Data</param>
            ''' <param name="PreloadPicture">Pre-download Anime Picture</param>
            ''' <returns></returns>
            Public Shared Async Function ParseMainAniDBXML(xml As String, Optional PreloadPicture As Boolean = True) As Task(Of MainElement)
                Dim XDoc = XDocument.Parse(xml)
                Dim Root = XDoc.Root 'main       

                Dim XHot = Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "hotanime")
                Dim HotAnime = Await ParseHotAnimeAniDBXML(XHot.ToString, PreloadPicture)

                Dim XRandomSimilar = Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "randomsimilar")
                Dim RandomSimilar = Await ParseRandomSimilarAniDBXML(XRandomSimilar.ToString, PreloadPicture)

                Dim XRandomRecommendation = Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "randomrecommendation")
                Dim RandomRecommendation = Await ParseRandomRecommendationAniDBXML(XRandomRecommendation.ToString, PreloadPicture)

                Return New MainElement(HotAnime, RandomSimilar, RandomRecommendation)
            End Function
            ''' <summary>
            ''' Return managed data for AniDB API XML Response
            ''' </summary>
            ''' <param name="xml">XML Data</param>
            ''' <param name="PreloadPicture">Pre-download Anime Picture</param>
            ''' <returns></returns>
            Public Shared Async Function ParseMainAniDBXML(xml As XDocument, Optional PreloadPicture As Boolean = True) As Task(Of MainElement)
                Dim XDoc = xml
                Dim Root = XDoc.Root 'main       

                Dim XHot = Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "hotanime")
                Dim HotAnime = Await ParseHotAnimeAniDBXML(XHot.ToString, PreloadPicture)

                Dim XRandomSimilar = Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "randomsimilar")
                Dim RandomSimilar = Await ParseRandomSimilarAniDBXML(XRandomSimilar.ToString, PreloadPicture)

                Dim XRandomRecommendation = Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "randomrecommendation")
                Dim RandomRecommendation = Await ParseRandomRecommendationAniDBXML(XRandomRecommendation.ToString, PreloadPicture)

                Return New MainElement(HotAnime, RandomSimilar, RandomRecommendation)
            End Function
            ''' <summary>
            ''' Return the requested picture from AniDB CDN
            ''' </summary>
            ''' <param name="id">picture id : xxxx.xxx</param>
            ''' <returns></returns>
            Public Shared Async Function GetAniDBImageAsync(id As String) As Task(Of System.Drawing.Image)
                'Base URL: https://cdn.anidb.net/images/main/
                'Base API URL: http://img7.anidb.net/pics/anime/
                Console.WriteLine("[" & Now.ToString("HH:mm:ss") & "][INFO]: " & "Fetching Anime Image From DB[API:" & UseImageAPI & "]: " & id)
                If String.IsNullOrEmpty(id) Then
                    Return Nothing
                End If
                Return Await Task.Run(Function()
                                          Using WC As New WebClient()
                                              If UseImageAPI Then
                                                  If ServicePointManager.SecurityProtocol <> SecurityProtocolType.Tls12 Then
                                                      ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 'AniDB Requires Tls1.2
                                                      Console.WriteLine("[" & Now.ToString("HH:mm:ss") & "][INFO]: " & "TLS 1.2 Is Required for Image API, Now Using TLS 1.2")
                                                  End If
                                                  Dim RURL As String
                                                  Dim req As HttpWebRequest = HttpWebRequest.Create("http://img7.anidb.net/pics/anime/" & id)
                                                  Try
                                                      Dim response As HttpWebResponse = req.GetResponse
                                                      RURL = response.ResponseUri.AbsoluteUri
                                                  Catch ex As Exception
                                                      Console.WriteLine("[" & Now.ToString("HH:mm:ss") & "][DEBUG]: " & ex.ToString)
                                                      Console.WriteLine("[" & Now.ToString("HH:mm:ss") & "][INFO]: Now Trying With .jpg")
                                                      Try
                                                          req = HttpWebRequest.Create("http://img7.anidb.net/pics/anime/" & id & ".jpg")
                                                          Dim response As HttpWebResponse = req.GetResponse
                                                          RURL = response.ResponseUri.AbsoluteUri
                                                      Catch _ex As Exception
                                                          Console.WriteLine("[" & Now.ToString("HH:mm:ss") & "][INFO]: No Luck!")
                                                          Return Nothing
                                                      End Try
                                                  End Try
                                                  If Not String.IsNullOrEmpty(RURL) Then
                                                      Try
                                                          Dim ImageData = WC.DownloadData(RURL)
                                                          Dim IMG = System.Drawing.Image.FromStream(New IO.MemoryStream(ImageData))
                                                          Return IMG
                                                      Catch ex As Exception
                                                          Console.WriteLine("[" & Now.ToString("HH:mm:ss") & "][DEBUG]: " & ex.ToString)
                                                          Return Nothing
                                                      End Try
                                                  Else
                                                      Return Nothing
                                                  End If
                                              Else
                                                  Try
                                                      Dim ImageData = WC.DownloadData("https://cdn.anidb.net/images/main/" & id)
                                                      Dim IMG = System.Drawing.Image.FromStream(New IO.MemoryStream(ImageData))
                                                      Return IMG
                                                  Catch ex As Exception
                                                      Console.WriteLine("[" & Now.ToString("HH:mm:ss") & "][DEBUG]: " & ex.ToString)
                                                      Try
                                                          Dim ImageData = WC.DownloadData("https://cdn.anidb.net/images/main/" & id & ".jpg")
                                                          Dim IMG = System.Drawing.Image.FromStream(New IO.MemoryStream(ImageData))
                                                          Return IMG
                                                      Catch _ex As Exception
                                                          Console.WriteLine("[" & Now.ToString("HH:mm:ss") & "][DEBUG]: No Luck!")
                                                          Return Nothing
                                                      End Try
                                                  End Try
                                              End If
                                          End Using
                                      End Function)
            End Function
            Public Shared Async Function CheckClient(Client As String, ClientVer As Integer) As Task(Of Boolean)
                Dim URL = BuildAniDBRequest(AniDBRequestType.anime, Client, ClientVer, 1)
                Try
                    Console.WriteLine("[" & Now.ToString("HH:mm:ss") & "][INFO]: " & "Fetching Anime Data For " & 1)
                    Using HClient As New Http.HttpClient(New Http.HttpClientHandler With {.AutomaticDecompression = DecompressionMethods.GZip Or DecompressionMethods.Deflate})
                        Dim Req = Await HClient.GetStringAsync(BuildAniDBRequest(AniDBRequestType.anime, Client, ClientVer, 1))
                        Dim XReq = XDocument.Parse(Req)
                        If XReq.Root.Name = "error" Then
                            Dim ErrCode = XReq.Root.FirstAttribute.Value
                            Dim Value = XReq.Root.Value
                            Console.WriteLine("[" & Now.ToString("HH:mm:ss") & "][INFO]: " & "ERROR With Code: " & ErrCode & ", Value: " & Value)
                            Return False
                        Else
                            Return True
                        End If
                    End Using
                Catch ex As Exception
                    Console.WriteLine("[" & Now.ToString("HH:mm:ss") & "][DEBUG]: " & ex.ToString)
                    Return False
                End Try
            End Function
        End Class
#End Region
#End Region
    End Class
End Namespace
