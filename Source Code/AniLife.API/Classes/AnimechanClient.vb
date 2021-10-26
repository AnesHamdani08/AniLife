Imports System.Net
Imports Newtonsoft
''' <summary>
''' Used to access Animechan REST API
''' </summary>
Public Class AnimechanClient
    Public Const Version As String = "0.0.0.1-Beta.3"
    Public Const RateLimit As Integer = 100
    Public Const RandomQuoteLink As String = "https://animechan.vercel.app/api/random"
    Public Const RandomQuotesLink As String = "https://animechan.vercel.app/api/quotes"
    Public Const AvailableAnimes As String = "https://animechan.vercel.app/api/available/anime"
    Public Const QuotesByAnimeTitle As String = "https://animechan.vercel.app/api/quotes/anime?title="
    Public Const QuotesByCharacterName As String = "https://animechan.vercel.app/api/quotes/character?name="
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

    Private _RequestCount As Integer

    Private Property RequestCount As Integer
        Get
            Return _RequestCount
        End Get
        Set(value As Integer)
            If value = RateLimit Then
                RaiseEvent OnRateLimitReached()
            End If
            _RequestCount = value
        End Set
    End Property
    Public ReadOnly Property Requests As Integer
        Get
            Return RequestCount
        End Get
    End Property
    Public Property UseRateLimit As Boolean = True
    Private WithEvents RequestClock As New System.Windows.Forms.Timer With {.Interval = 3600000}

    Public Event OnRateLimitReached()

    'Animechan Requires the following:
    '100 Request per hour
    'Default quote returned count is 10
#Region "Routes"
    ''' <summary>
    ''' Gets a random quote from Animechan
    ''' </summary>
    ''' <returns></returns>
    Public Async Function GetRandomQuote() As Task(Of QuoteElement)
        If Not RequestAccess() Then Return Nothing
        Try
            Console.WriteLine(ConsoleInfoText & "Fetching Data For Random Quote")
            Using HClient As New Http.HttpClient()
                Dim Req = Await HClient.GetStringAsync(RandomQuoteLink)
                Console.WriteLine(ConsoleInfoText & "Parsing Returned Data For Random Quote")
                Dim Item = Await ParseRandomQuote(Req)
                Console.WriteLine(ConsoleInfoText & "Data Parse For Random Quote Completed Successfuly, Returned: " & Item.ToString)
                Return Item
            End Using
        Catch ex As Exception
            Console.WriteLine(ConsoleDebugText & ex.ToString)
            Return Nothing
        End Try
    End Function
    ''' <summary>
    ''' Gets 10 random quotes from Animechan
    ''' </summary>
    ''' <returns></returns>
    Public Async Function GetRandomQuotes() As Task(Of List(Of QuoteElement))
        If Not RequestAccess() Then Return Nothing
        Try
            Console.WriteLine(ConsoleInfoText & "Fetching Data For 10 Random Quote")
            Using HClient As New Http.HttpClient()
                Dim Req = Await HClient.GetStringAsync(RandomQuotesLink)
                Console.WriteLine(ConsoleInfoText & "Parsing Returned Data For 10 Random Quote")
                Dim Item = Await ParseRandomQuotes(Req)
                Console.WriteLine(ConsoleInfoText & "Data Parse For 10 Random Quote Completed Successfuly, Returned: " & Item.ToString)
                Return Item
            End Using
        Catch ex As Exception
            Console.WriteLine(ConsoleDebugText & ex.ToString)
            Return Nothing
        End Try
    End Function
    ''' <summary>
    ''' Get 10 quotes from a specific anime
    ''' </summary>
    ''' <returns></returns>
    Public Async Function GetQuotesByAnimeTitle(title As String) As Task(Of List(Of QuoteElement))
        If Not RequestAccess() Then Return Nothing
        Try
            Console.WriteLine(ConsoleInfoText & "Fetching Data For Quotes By Anime Title")
            Using HClient As New Http.HttpClient()
                Dim Req = Await HClient.GetStringAsync(QuotesByAnimeTitle & title)
                Console.WriteLine(ConsoleInfoText & "Parsing Returned Data For Quotes By Anime Title")
                Dim Item = Await ParseRandomQuotes(Req)
                Console.WriteLine(ConsoleInfoText & "Data Parse For Quotes By Anime Title Completed Successfuly, Returned: " & Item.ToString)
                Return Item
            End Using
        Catch ex As Exception
            Console.WriteLine(ConsoleDebugText & ex.ToString)
            Return Nothing
        End Try
    End Function
    Public Async Function GetQuotesByCharacterName(name As String) As Task(Of List(Of QuoteElement))
        If Not RequestAccess() Then Return Nothing
        Try
            Console.WriteLine(ConsoleInfoText & "Fetching Data For Quotes By Character Name")
            Using HClient As New Http.HttpClient()
                Dim Req = Await HClient.GetStringAsync(QuotesByCharacterName & name)
                Console.WriteLine(ConsoleInfoText & "Parsing Returned Data For Quotes By Character Name")
                Dim Item = Await ParseRandomQuotes(Req)
                Console.WriteLine(ConsoleInfoText & "Data Parse For Quotes By Character Name Completed Successfuly, Returned: " & Item.ToString)
                Return Item
            End Using
        Catch ex As Exception
            Console.WriteLine(ConsoleDebugText & ex.ToString)
            Return Nothing
        End Try
    End Function
    Public Async Function GetAvailableAnimes() As Task(Of List(Of String))
        If Not RequestAccess() Then Return Nothing
        Try
            Console.WriteLine(ConsoleInfoText & "Fetching Data For Quotes By Character Name")
            Using HClient As New Http.HttpClient()
                Dim Req = Await HClient.GetStringAsync(AvailableAnimes)
                Console.WriteLine(ConsoleInfoText & "Parsing Returned Data For Quotes By Character Name")
                Dim Item = Await ParseAvailableAnimes(Req)
                Console.WriteLine(ConsoleInfoText & "Data Parse For Quotes By Character Name Completed Successfuly, Returned: " & Item.ToString)
                Return Item
            End Using
        Catch ex As Exception
            Console.WriteLine(ConsoleDebugText & ex.ToString)
            Return Nothing
        End Try
    End Function
#End Region
#Region "Rate Limit"
    Public Function CheckAccess() As Boolean
        If UseRateLimit AndAlso RequestClock.Enabled = False Then RequestClock.Start()
        Return If(UseRateLimit, If(RequestCount < 100, True, False), True)
    End Function
    Public Function RequestAccess() As Boolean
        If CheckAccess() Then
            RequestCount += 1
            Return True
        Else
            Return False
        End If
    End Function
    Private Sub RequestClock_Tick(sender As Object, e As EventArgs) Handles RequestClock.Tick
        _RequestCount = 0
    End Sub
#End Region
#Region "Helpers"
    Private Async Function ParseRandomQuote(data As String) As Task(Of QuoteElement)
        Return Await Task.Run(Function()
                                  Dim Info = Json.Linq.JObject.Parse(data)
                                  Dim Anime = Info("anime")
                                  Dim Character = Info("character")
                                  Dim Quote = Info("quote")
                                  Return New QuoteElement With {.Anime = Anime, .Character = Character, .Quote = Quote}
                              End Function)
    End Function
    Private Async Function ParseRandomQuotes(data As String) As Task(Of List(Of QuoteElement))
        Return Await Task.Run(Function()
                                  Dim LQuotes As New List(Of QuoteElement)
                                  Dim Info = Json.Linq.JArray.Parse(data)
                                  For Each subquote In Info.Children
                                      Dim Anime = subquote("anime")
                                      Dim Character = subquote("character")
                                      Dim Quote = subquote("quote")
                                      LQuotes.Add(New QuoteElement With {.Anime = Anime, .Character = Character, .Quote = Quote})
                                  Next
                                  Return LQuotes
                              End Function)
    End Function
    Private Async Function ParseAvailableAnimes(data As String) As Task(Of List(Of String))
        Return Await Task.Run(Function()
                                  Dim LQuotes As New List(Of String)
                                  Dim Info = Json.Linq.JArray.Parse(data)
                                  For Each subquote In Info.Children
                                      LQuotes.Add(subquote)
                                  Next
                                  Return LQuotes
                              End Function)
    End Function

#Region "Elements"
    Public Class QuoteElement
        Public Property Anime As String
        Public Property Character As String
        Public Property Quote As String
        Public Overrides Function ToString() As String
            Return "{Anime=" & Anime & ";Character=" & Character & ";Quote=" & Quote & "}"
        End Function
    End Class
#End Region
#End Region
End Class
