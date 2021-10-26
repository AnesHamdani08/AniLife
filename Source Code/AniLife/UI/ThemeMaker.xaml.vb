Imports HandyControl.Data

Public Class ThemeMaker
    Public Class Theme
        Public Property Background As SolidColorBrush
        Public Property TopBar As SolidColorBrush
        Public Property TopBarText As SolidColorBrush
        Public Property Content As SolidColorBrush
        Public Property Accent As SolidColorBrush
        Public Property Overlay As SolidColorBrush
        Public Property OverlayText As SolidColorBrush
        Public Property Text As SolidColorBrush
        Public Property FontFamily As FontFamily
        Public Property FontWeight As FontWeight
        Public Overrides Function ToString() As String
            Return "{BG:" & Background.Color.ToString & ";TOPBAR:" & TopBar.Color.ToString & ";CONTENT" & Content.Color.ToString & ";OVERLAY:" & Overlay.Color.ToString & ";TEXT:" & Text.Color.ToString & ";FONT:" & FontFamily.ToString & ";FONTWEIGHT:" & FontWeight.ToString & "}"
        End Function
    End Class
    Public Property Result As Theme
    Private Property TBDResult As New Theme
    Public Property IsOverlay As Boolean
        Get
            Return If(OV.Visibility = Visibility.Visible, True, False)
        End Get
        Set(value As Boolean)
            If value Then
                OV.Visibility = Visibility.Visible
            Else
                OV.Visibility = Visibility.Collapsed
            End If
        End Set
    End Property
    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub
    Public Sub New(WndO As Window)
        Owner = WndO
        WindowStartupLocation = WindowStartupLocation.CenterOwner

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub
    Public Shared Function ShowMaker() As Theme
        Dim TM As New ThemeMaker
        If TM.ShowDialog Then
            Return TM.Result
        Else
            Return Nothing
        End If
    End Function
    Public Shared Function ShowMaker(WndO As Window) As Theme
        Dim TM As New ThemeMaker(WndO)
        If TM.ShowDialog Then
            Return TM.Result
        Else
            Return Nothing
        End If
    End Function
    Private Async Function WaitForOverlay() As Task
        Await Task.Run(Sub()
                           Do While IsOverlay
                               Threading.Thread.Sleep(10)
                           Loop
                       End Sub)
    End Function
    Private Async Sub SET_BG_Click(sender As Object, e As RoutedEventArgs) Handles SET_BG.Click
        OV_CLRPICKER.SelectedBrush = Resources("BG")
        IsOverlay = True
        Await WaitForOverlay()
        TBDResult.Background = OV_CLRPICKER.SelectedBrush
        Resources("BG") = OV_CLRPICKER.SelectedBrush
    End Sub

    Private Async Sub SET_TOPBAR_Click(sender As Object, e As RoutedEventArgs) Handles SET_TOPBAR.Click
        OV_CLRPICKER.SelectedBrush = Resources("TOPBAR")
        IsOverlay = True
        Await WaitForOverlay()
        TBDResult.TopBar = OV_CLRPICKER.SelectedBrush
        Resources("TOPBAR") = OV_CLRPICKER.SelectedBrush
    End Sub

    Private Async Sub SET_TOPBARTEXT_Click(sender As Object, e As RoutedEventArgs) Handles SET_TOPBARTEXT.Click
        OV_CLRPICKER.SelectedBrush = Resources("TOPBARTEXT")
        IsOverlay = True
        Await WaitForOverlay()
        TBDResult.TopBarText = OV_CLRPICKER.SelectedBrush
        Resources("TOPBARTEXT") = OV_CLRPICKER.SelectedBrush
    End Sub

    Private Async Sub SET_CONTENT_Click(sender As Object, e As RoutedEventArgs) Handles SET_CONTENT.Click
        OV_CLRPICKER.SelectedBrush = Resources("CONTENT")
        IsOverlay = True
        Await WaitForOverlay()
        TBDResult.Content = OV_CLRPICKER.SelectedBrush
        Resources("CONTENT") = OV_CLRPICKER.SelectedBrush
    End Sub

    Private Async Sub SET_ACCENT_Click(sender As Object, e As RoutedEventArgs) Handles SET_ACCENT.Click
        OV_CLRPICKER.SelectedBrush = Resources("ACCENT")
        IsOverlay = True
        Await WaitForOverlay()
        TBDResult.Accent = OV_CLRPICKER.SelectedBrush
        Resources("ACCENT") = OV_CLRPICKER.SelectedBrush
    End Sub

    Private Async Sub SET_OVERLAY_Click(sender As Object, e As RoutedEventArgs) Handles SET_OVERLAY.Click
        OV_CLRPICKER.SelectedBrush = Resources("OVERLAY")
        IsOverlay = True
        Await WaitForOverlay()
        TBDResult.Overlay = OV_CLRPICKER.SelectedBrush
        Resources("OVERLAY") = OV_CLRPICKER.SelectedBrush
    End Sub

    Private Async Sub SET_OVERLAYTEXT_Click(sender As Object, e As RoutedEventArgs) Handles SET_OVERLAYTEXT.Click
        OV_CLRPICKER.SelectedBrush = Resources("OVERLAYTEXT")
        IsOverlay = True
        Await WaitForOverlay()
        TBDResult.OverlayText = OV_CLRPICKER.SelectedBrush
        Resources("OVERLAYTEXT") = OV_CLRPICKER.SelectedBrush
    End Sub

    Private Async Sub SET_TEXT_Click(sender As Object, e As RoutedEventArgs) Handles SET_TEXT.Click
        OV_CLRPICKER.SelectedBrush = Resources("TEXT")
        IsOverlay = True
        Await WaitForOverlay()
        TBDResult.Text = OV_CLRPICKER.SelectedBrush
        Resources("TEXT") = OV_CLRPICKER.SelectedBrush
    End Sub

    Private Sub SET_FONT_Click(sender As Object, e As RoutedEventArgs) Handles SET_FONT.Click
        Dim FDLG As New Forms.FontDialog()
        If FDLG.ShowDialog <> Forms.DialogResult.Cancel Then
            TBDResult.FontFamily = New FontFamily(FDLG.Font.FontFamily.Name)
            TBDResult.FontWeight = If(FDLG.Font.Bold, FontWeights.Bold, FontWeights.Regular)
            Resources("FONT") = TBDResult.FontFamily
            Resources("FONT_WEIGHT") = TBDResult.FontWeight
        End If
    End Sub

    Private Sub ThemeMaker_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized
        Me.Resources.MergedDictionaries.Add(New ResourceDictionary() With {.Source = New Uri("Colors/Default.xaml", UriKind.Relative)})
    End Sub

    Private Sub SET_DONE_Click(sender As Object, e As RoutedEventArgs) Handles SET_DONE.Click
        Result = TBDResult
        DialogResult = True
    End Sub

    Private Sub OV_CLRPICKER_Confirmed(sender As Object, e As FunctionEventArgs(Of Color)) Handles OV_CLRPICKER.Confirmed
        IsOverlay = False
    End Sub

    Private Sub OV_CLRPICKER_Canceled(sender As Object, e As EventArgs) Handles OV_CLRPICKER.Canceled
        IsOverlay = False
    End Sub

    Private Sub Grid_MouseDown(sender As Object, e As MouseButtonEventArgs)
        If e.ChangedButton = MouseButton.Left Then
            I_OV.Visibility = Visibility.Visible
        End If
    End Sub

    Private Sub I_OV_MouseDown(sender As Object, e As MouseButtonEventArgs) Handles I_OV.MouseDown
        If e.ChangedButton = MouseButton.Left Then
            I_OV.Visibility = Visibility.Collapsed
        End If
    End Sub
End Class
