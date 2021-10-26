Imports System.ComponentModel

Public Class DeveloperConsole
    Public Shared CommandList As New List(Of String())({New String() {"api -get delay", "Gets the current API request delay"},
                                                      New String() {"api -get charcap", "Gets the current characters cap"},
                                                      New String() {"api -get client", "Gets the current client name"},
                                                      New String() {"api -get clientver", "Gets the current client version"},
                                                      New String() {"api -set delay", "Sets the current API request delay"},
                                                      New String() {"api -set charcap", "Sets the current characters cap"},
                                                      New String() {"api -set client", "Sets the current client name"},
                                                      New String() {"api -set clientver", "Sets the current client version"},
                                                      New String() {"api -notify client", "Notify the app to change the client credentials"},
                                                      New String() {"client -view settings", "View all the app settings in read/write mode, don't change values if you don't know what you're doing"},
                                                      New String() {"client -clear lockscreen", "Resets the lockscreen picture to the default one if you changed it from the app"},
                                                      New String() {"client -library namefromapi", "Names the library episodes based on their names from the api"}})
    Private MRE As New Threading.ManualResetEvent(False)
    Private IsWaiting As Boolean = False
    Private Sub ConsoleIn_TB_KeyUp(sender As Object, e As KeyEventArgs) Handles ConsoleIn_TB.KeyUp
        If e.Key = Key.Enter Then
            If IsWaiting Then
                MRE.Set()
            Else
                ResolveCommand(ConsoleIn_TB.Text)
                ConsoleIn_TB.Text = String.Empty
            End If
        End If
    End Sub

    Private Sub ConsoleOut_TB_TextChanged(sender As Object, e As TextChangedEventArgs) Handles ConsoleOut_TB.TextChanged
        ConsoleOut_TB.ScrollToEnd()

    End Sub

    Private Sub DeveloperConsole_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        Hide()
        e.Cancel = True
    End Sub
    Private Async Function ReadLine() As Task(Of String)
        IsWaiting = True
        Dispatcher.InvokeAsync(Async Sub()
                                   Do While IsWaiting
                                       ConsoleOut_TB.Visibility = If(ConsoleOut_TB.Visibility = Visibility.Visible, Visibility.Hidden, Visibility.Visible)
                                       Await Task.Delay(250)
                                   Loop
                                   ConsoleOut_TB.Visibility = Visibility.Visible
                               End Sub)
        Return Await Task.Run(Function()
                                  MRE.WaitOne()
                                  MRE.Reset()
                                  IsWaiting = False
                                  Dim TBR As String = Nothing
                                  Dispatcher.Invoke(Sub()
                                                        TBR = ConsoleIn_TB.GetValue(TextBox.TextProperty)
                                                        ConsoleIn_TB.ClearValue(TextBox.TextProperty)
                                                    End Sub)
                                  Return TBR
                              End Function)
    End Function
    Public Async Sub ResolveCommand(command As String)
        Select Case command.ToLower
            Case "help"
                Dim SB As New Text.StringBuilder
                For Each com In CommandList
                    SB.AppendLine(com(0) & ": " & com(1))
                Next
                Console.WriteLine(Utils.ConsoleInfoText & SB.ToString)
                SB = Nothing
            Case "api -get delay"
                Console.WriteLine(Utils.ConsoleInfoText & My.Settings.CLIENT_REQUESTDELAY)
            Case "api -get charcap"
                Console.WriteLine(Utils.ConsoleInfoText & My.Settings.CLIENT_CHARACTERCAP)
            Case "api -set delay"
                Console.WriteLine(Utils.ConsoleWarningText & "Settings API Delay lower than 2000ms could get you banned")
                Dim NewVal = InputBox("API Delay in ms", "AniLife", "2500")
                If System.Text.RegularExpressions.Regex.IsMatch(NewVal, "^[0-9 ]+$") Then
                    My.Settings.CLIENT_REQUESTDELAY = NewVal
                Else
                    Console.WriteLine(Utils.ConsoleWarningText & "Must be a number")
                End If
            Case "api -set charcap"
                Console.WriteLine(Utils.ConsoleWarningText & "Settings characters cap more than 10 can harm the app's performance")
                Dim NewVal = InputBox("Characters cap", "AniLife", "10")
                If System.Text.RegularExpressions.Regex.IsMatch(NewVal, "^[0-9 ]+$") Then
                    My.Settings.CLIENT_CHARACTERCAP = NewVal
                Else
                    Console.WriteLine(Utils.ConsoleWarningText & "Must be a number")
                End If
            Case "client -view settings"
                Dim SB As New SettingsBrowser With {.Owner = Application.Current.MainWindow}
                SB.ShowDialog()
                SB = Nothing
            Case "client -clear lockscreen"
                Await Task.Run(Sub()
                                   Dim PSI As New ProcessStartInfo With {.FileName = IO.Path.Combine(My.Application.Info.DirectoryPath, "AniLife.exe"), .UseShellExecute = True, .Verb = "runas", .Arguments = "-CLEAR_LOCKWALL"}
                                   Dim Proc = Process.Start(PSI)
                                   Proc.WaitForExit(2000)
                                   Try
                                       Proc.Kill()
                                   Catch
                                   End Try
                               End Sub)
            Case "client -library namefromapi"
                Console.WriteLine(Utils.ConsoleInfoText & "Beta feature, coming soon")
            Case "api -get client"
                Console.WriteLine(My.Settings.APP_CLIENT)
            Case "api -set client"
                My.Settings.APP_CLIENT = Await ReadLine()
                My.Settings.Save()
            Case "api -get clientver"
                Console.WriteLine(Utils.ConsoleInfoText & My.Settings.APP_CLIENTVER)
            Case "api -set clientver"
                Dim Num = Await ReadLine()
                If System.Text.RegularExpressions.Regex.IsMatch(Num, "^[0-9 ]+$") Then
                    My.Settings.APP_CLIENTVER = Num
                    My.Settings.Save()
                Else
                    Console.WriteLine(Utils.ConsoleWarningText & "Must be a number")
                End If
            Case "api -notify client"
                MainWindow.LoadingStateHelper.MainWindow.ChangeClient(My.Settings.APP_CLIENT, My.Settings.APP_CLIENTVER)
            Case Else
                Console.WriteLine(Utils.ConsoleInfoText & "Command """ & command & """ Was Found")
        End Select
    End Sub
End Class
