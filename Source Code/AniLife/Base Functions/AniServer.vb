Imports AniLife.API
Imports System.Net
''' <summary>
''' Provides methods to contact AniLife Server
''' </summary>
Public Class AniServer
    Public Const Version As String = "0.0.0.1-Beta.3"
    Private Shared ReadOnly Property ConsoleInfoText As String
        Get
            Return "[" & Now.ToString("HH:mm:ss") & "][INFO]: "
        End Get
    End Property
    Private Shared ReadOnly Property ConsoleDebugText As String
        Get
            Return "[" & Now.ToString("HH:mm:ss") & "][DEBUG]: "
        End Get
    End Property
    ''' <summary>
    ''' Provides methods to contact Featured,Waifu,OST
    ''' </summary>
    Public Class Data
        Public Const DataLink = "http://aneshamdani08.github.io/AniLife/Backend/data.xml"
        ''' <summary>
        ''' Returns Featured, Waifu, OST IDs, return nothing if an exception occurs
        ''' </summary>
        ''' <returns></returns>
        Public Shared Async Function GetData() As Task(Of DataItem)
            Dim TBRD As New Data
            Try
                Console.WriteLine(ConsoleInfoText & "Fetching Data From Backend")
                Using HClient As New Http.HttpClient
                    Dim Req = Await HClient.GetStringAsync(DataLink)
                    Dim XReq = XDocument.Parse(Req)
                    Console.WriteLine(ConsoleInfoText & "Parsing Returned Data From Backend")
                    Dim Item = Await ParseData(XReq)
                    Return Item
                End Using
            Catch ex As Exception
                Console.WriteLine(ConsoleDebugText & ex.ToString)
                Return Nothing
            End Try
        End Function
        Public Shared Async Function ParseData(XData As XDocument) As Task(Of DataItem)
            Return Await Task.Run(Function()
                                      Dim Root = XData.Root 'AniLife
                                      If Root.Name = "AniLife" Then
                                          Dim Nodes = Root.Nodes
                                          Dim Featured = Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "Featured")
                                          Dim Waifu = Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "Waifu")
                                          Dim OST = Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "OST")
                                          Dim LFeatured As New List(Of String)
                                          Dim LWaifu As New List(Of DataItem.WaifuElement)
                                          Dim LOST As New List(Of DataItem.OSTElement)
                                          If Featured IsNot Nothing Then
                                              For Each anime As XElement In CType(Featured, XElement).Nodes
                                                  LFeatured.Add(anime.Value)
                                              Next
                                          End If
                                          If Waifu IsNot Nothing Then
                                              For Each character As XElement In CType(Waifu, XElement).Nodes
                                                  LWaifu.Add(New DataItem.WaifuElement With {.ID = character.Value, .Target = character.Attribute("Target").Value, .PictureURL = character.Attribute("Thumb").Value})
                                              Next
                                          End If
                                          If OST IsNot Nothing Then
                                              For Each id As XElement In CType(OST, XElement).Nodes
                                                  LOST.Add(New DataItem.OSTElement With {.ID = id.Value, .PictureURL = id.Attribute("Thumb").Value, .Type = id.Attribute("Source").Value,
                                                           .Title = id.Attribute("Title").Value, .Artist = id.Attribute("Artist").Value, .Anime = id.Attribute("Anime").Value, .AnimeID = id.Attribute("Aid").Value})
                                              Next
                                          End If
                                          Return New DataItem With {.Featured = LFeatured, .Waifu = LWaifu, .OST = LOST}
                                      End If
                                      Return Nothing
                                  End Function)
        End Function
        Public Class DataItem
            Public Property Featured As List(Of String)
            Public Property Waifu As List(Of WaifuElement)
            Public Property OST As List(Of OSTElement)

            Public Class WaifuElement
                Public Property Target As Integer
                Public Property PictureURL As String
                Public Property Picture As System.Drawing.Image
                Public Property ID As Integer
                Public Async Function LoadPicture() As Task(Of Boolean)
                    Console.WriteLine(ConsoleInfoText & "Fetching Waifu Picture for " & ID & " From Anime" & Target & " From " & PictureURL)
                    If String.IsNullOrEmpty(ID) Then
                        Return False
                    End If
                    Return Await Task.Run(Function()
                                              Using WC As New WebClient()
                                                  Try
                                                      Dim ImageData = WC.DownloadData(PictureURL)
                                                      Dim IMG = System.Drawing.Image.FromStream(New IO.MemoryStream(ImageData))
                                                      Picture = IMG
                                                      Return True
                                                  Catch ex As Exception
                                                      Console.WriteLine(ConsoleDebugText & ex.ToString)
                                                      Return False
                                                  End Try
                                              End Using
                                          End Function)
                End Function
            End Class
            Public Class OSTElement
                Public Property ID As String
                Public Property Type As OSTSource
                Public Property Title As String
                Public Property Artist As String
                Public Property AnimeID As String
                Public Property Anime As String
                Public Property PictureURL As String
                Public Property Picture As System.Drawing.Image
                Public Async Function LoadPicture() As Task(Of Boolean)
                    Console.WriteLine(ConsoleInfoText & "Fetching OST Picture for " & ID & " From " & PictureURL)
                    If String.IsNullOrEmpty(ID) Then
                        Return False
                    End If
                    Return Await Task.Run(Function()
                                              Using WC As New WebClient()
                                                  Try
                                                      Dim ImageData = WC.DownloadData(PictureURL)
                                                      Dim IMG = System.Drawing.Image.FromStream(New IO.MemoryStream(ImageData))
                                                      Picture = IMG
                                                      Return True
                                                  Catch ex As Exception
                                                      Console.WriteLine(ConsoleDebugText & ex.ToString)
                                                      Return False
                                                  End Try
                                              End Using
                                          End Function)
                End Function
                Public Enum OSTSource
                    Link
                    Youtube
                End Enum
            End Class
        End Class
    End Class
    ''' <summary>
    ''' Provides methods to check for updates
    ''' </summary>
    Public Class Updates
        Public Const DataLink = "http://aneshamdani08.github.io/AniLife/Backend/updates.xml"
        Public Const ReleaseLink = "https://github.com/AnesHamdani08/AniLife/releases"
        Public Shared Async Function CheckForUpdates(FilterData As Boolean) As Task(Of List(Of Update))
            Try
                Console.WriteLine(ConsoleInfoText & "Fetching Updates Data From Backend")
                Using HClient As New Http.HttpClient
                    Dim Req = Await HClient.GetStringAsync(DataLink)
                    Dim XReq = XDocument.Parse(Req)
                    Console.WriteLine(ConsoleInfoText & "Parsing Returned Updates Data From Backend")
                    Dim Item = Await ParseData(XReq)
                    If FilterData Then
                        Dim TBD As New List(Of Update)
                        For Each upd In Item
                            If upd.Target.ToLower = "anilife" Then
                                If upd.Version = MainWindow.Version Then
                                    TBD.Add(upd)
                                End If
                            ElseIf upd.Target.ToLower = "anicache" Then
                                If upd.Version = AniCache.Version Then
                                    TBD.Add(upd)
                                End If
                            ElseIf upd.Target.ToLower = "anilibrary" Then
                                If upd.Version = AniLibrary.Version Then
                                    TBD.Add(upd)
                                End If
                            ElseIf upd.Target.ToLower = "aniresolver" Then
                                If upd.Version = AniResolver.LinkVersion Then
                                    TBD.Add(upd)
                                End If
                            ElseIf upd.Target.ToLower = "aniserver" Then
                                If upd.Version = AniServer.Version Then
                                    TBD.Add(upd)
                                End If
                            ElseIf upd.Target.ToLower = "anidb" Then
                                If upd.Version = AniDB.AniDBClient.Version Then
                                    TBD.Add(upd)
                                End If
                            ElseIf upd.Target.ToLower = "animechan" Then
                                If upd.Version = AnimechanClient.Version Then
                                    TBD.Add(upd)
                                End If
                            ElseIf upd.Target.ToLower = "minitokyo" Then
                                If upd.Version = MinitokyoClient.Version Then
                                    TBD.Add(upd)
                                End If
                            End If
                        Next
                        For Each upd In TBD
                            Item.Remove(upd)
                        Next
                    End If
                    Return Item
                End Using
            Catch ex As Exception
                Console.WriteLine(ConsoleDebugText & ex.ToString)
                Return Nothing
            End Try
        End Function
        Public Shared Async Function ParseData(XData As XDocument) As Task(Of List(Of Update))
            Return Await Task.Run(Function()
                                      Dim Root = XData.Root 'AniLife
                                      If Root.Name = "AniLife" Then
                                          Dim Updates = Root.Nodes.FirstOrDefault(Function(k) CType(k, XElement).Name = "Updates")
                                          If Updates IsNot Nothing Then
                                              Dim LUpdate As New List(Of Update)
                                              For Each Update As XElement In CType(Updates, XElement).Nodes
                                                  LUpdate.Add(New Update With {.Type = Update.Attribute("Type").Value, .Version = Update.Attribute("Version").Value, .Target = Update.Attribute("Target").Value})
                                              Next
                                              Return LUpdate
                                          End If
                                      End If
                                      Return Nothing
                                  End Function)
        End Function
        Public Class Update
            Public Property Type As UpdateType
            Public Property Version As String
            Public Property Target As String
            Public Enum UpdateType
                [Optional]
                Required
            End Enum
        End Class
    End Class
End Class
