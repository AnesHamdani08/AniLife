Public Class Utils
    Public Class BackgroundWorkerWithTag
        Inherits ComponentModel.BackgroundWorker
        Public Property Tag As Object
    End Class
    Public Class PeakItem
        Public Property Master As Single
        Public Property Left As Single
        Public Property Right As Single
        Public Sub New(_Master As Single, _Left As Single, _Right As Single)
            Master = _Master
            Left = _Left
            Right = _Right
        End Sub
    End Class
    Public Class NotificationBucket
        Public Class NotificationArrivedEventArgs
            Public Property Notification As NotificationItem
            Public Property UseSystemNotifications As Boolean
        End Class
        Public Class NotificationItem
            Public Property Title As String
            Public Property Msg As String
            Public Property Icon As HandyControl.Data.NotifyIconInfoType
            Public Sub New(_Title As String, _Msg As String, _Icon As HandyControl.Data.NotifyIconInfoType)
                Msg = _Msg
                Title = _Title
                Icon = _Icon
            End Sub
        End Class
        Public Shared Event OnNotificationArrived(e As NotificationArrivedEventArgs)
        Public Shared Property IsProcessing As Boolean = False
        Public Shared Property Delay As Integer = 10
        Public Shared Property UseSystemNotifications As Boolean = My.Settings.NOTIFICATIONS_USESYSTEMNOTIFICATIONS
        Private Shared Notifications As New List(Of NotificationItem)
        Public Shared Sub AddToBucket(Notification As NotificationItem(), Optional AndProcess As Boolean = False, Optional OverrideRouting As Boolean = False)
            Notifications.AddRange(Notification)
            If AndProcess = True Then
                If IsProcessing = False Then Process(OverrideRouting)
            End If
        End Sub
        Public Shared Async Sub Process(Optional OvRouting As Boolean = False)
            IsProcessing = True
            Dim i As Integer = 0
            Do While i < Notifications.Count
                IsProcessing = True
                If Notifications.Count = 0 Then Exit Do
                If OvRouting = True Then
                    RaiseEvent OnNotificationArrived(New NotificationArrivedEventArgs With {.Notification = Notifications(i), .UseSystemNotifications = True})
                Else
                    RaiseEvent OnNotificationArrived(New NotificationArrivedEventArgs With {.Notification = Notifications(i), .UseSystemNotifications = UseSystemNotifications})
                End If
                Notifications.RemoveAt(i)
                Await Task.Delay(Delay)
            Loop
            IsProcessing = False
        End Sub
    End Class
    <System.Runtime.InteropServices.DllImport("wininet.dll")>
    Private Shared Function InternetGetConnectedState(ByRef Description As Integer, ByVal ReservedValue As Integer) As Boolean
    End Function
    Public Shared Function CheckInternetConnection() As Boolean
        Try
            Dim ConnDesc As Integer
            Return InternetGetConnectedState(ConnDesc, 0)
        Catch
            Return False
        End Try
    End Function
    <Runtime.InteropServices.DllImport("gdi32.dll", EntryPoint:="DeleteObject")>
    Public Shared Function DeleteObject(
<Runtime.InteropServices.[In]> ByVal hObject As IntPtr) As Boolean
    End Function
    Public Shared Function ImageSourceFromBitmap(ByVal bmp As System.Drawing.Bitmap, Optional ChangeRes As Boolean = False, Optional ResX As Integer = 0, Optional ResY As Integer = 0) As ImageSource
        If ChangeRes = False Then
            Try
                Dim handle = bmp.GetHbitmap()
                Try
                    Return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())
                Finally
                    DeleteObject(handle)
                End Try
            Catch ex As Exception
                Return Nothing
            End Try
        Else
            Try
                Dim ResBmp As New System.Drawing.Bitmap(bmp, ResX, ResY)
                Dim handle = ResBmp.GetHbitmap()
                Try
                    Return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())
                Finally
                    DeleteObject(handle)
                End Try
            Catch ex As Exception
                Return Nothing
            End Try
        End If
    End Function
    Public Shared Function ImageSourceFromBitmap(ByVal bmp As System.Drawing.Image, Optional ScalingFactor As Double = 1) As ImageSource
        Try
            Dim ResBmp As New System.Drawing.Bitmap(bmp, bmp.Width / ScalingFactor, bmp.Height / ScalingFactor)
            Dim handle = ResBmp.GetHbitmap()
            Try
                Return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())
            Finally
                DeleteObject(handle)
            End Try
        Catch ex As Exception
            Return Nothing
        End Try
    End Function
    Public Shared Function BitmapFromImageSource(ByVal bitmap As BitmapSource) As System.Drawing.Bitmap
        Dim bmp As System.Drawing.Bitmap = New System.Drawing.Bitmap(bitmap.PixelWidth, bitmap.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppPArgb)
        Dim data As System.Drawing.Imaging.BitmapData = bmp.LockBits(New System.Drawing.Rectangle(System.Drawing.Point.Empty, bmp.Size), System.Drawing.Imaging.ImageLockMode.[WriteOnly], System.Drawing.Imaging.PixelFormat.Format32bppPArgb)
        bitmap.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride)
        bmp.UnlockBits(data)
        Return bmp
    End Function
    Public Shared Function GetAverageColor(ByVal bitmap As BitmapSource, Optional Opacity As Integer = 255) As System.Windows.Media.Color
        If bitmap Is Nothing Then Return Colors.Black
        Dim format = bitmap.Format
        If format <> PixelFormats.Bgr24 AndAlso format <> PixelFormats.Bgr32 AndAlso format <> PixelFormats.Bgra32 AndAlso format <> PixelFormats.Pbgra32 Then
            Throw New InvalidOperationException("BitmapSource must have Bgr24, Bgr32, Bgra32 Or Pbgra32 format")
            Return Nothing
        End If

        Dim width = bitmap.PixelWidth
        Dim height = bitmap.PixelHeight
        Dim numPixels = width * height
        Dim bytesPerPixel = format.BitsPerPixel / 8
        Dim pixelBuffer = New Byte(numPixels * bytesPerPixel - 1) {}
        bitmap.CopyPixels(pixelBuffer, width * bytesPerPixel, 0)
        Dim blue As Long = 0
        Dim green As Long = 0
        Dim red As Long = 0
        Dim i As Integer = 0

        While i < pixelBuffer.Length
            blue += pixelBuffer(i)
            green += pixelBuffer(i + 1)
            red += pixelBuffer(i + 2)
            i += bytesPerPixel
        End While

        Return System.Windows.Media.Color.FromArgb(CByte(Opacity), CByte((red / numPixels)), CByte((green / numPixels)), CByte((blue / numPixels)))
    End Function
    Public Shared Function GetInverseColor(Color As System.Windows.Media.Color, Optional Opacity As Integer = 255) As System.Windows.Media.Color
        Dim R = Color.R
        Dim G = Color.G
        Dim B = Color.B
        Dim newR = 255 - R
        Dim newG = 255 - G
        Dim newB = 255 - B
        Return System.Windows.Media.Color.FromArgb(Opacity, newR, newG, newB)
    End Function
    Public Shared Function GetInverseColor(Color As System.Drawing.Color, Optional Opacity As Integer = 255) As System.Drawing.Color
        Dim R = Color.R
        Dim G = Color.G
        Dim B = Color.B
        Dim newR = 255 - R
        Dim newG = 255 - G
        Dim newB = 255 - B
        Return System.Drawing.Color.FromArgb(Opacity, newR, newG, newB)
    End Function
    Public Shared Function SecsToMins(sec As Double, Optional LongText As Boolean = False) As String
        Dim TS = TimeSpan.FromSeconds(sec)
        Try
            If LongText Then
                If TS.Hours <> 0 Then
                    Return TS.Hours & " hours, " & If(TS.Minutes > 10, TS.Minutes, "0" & TS.Minutes) & " minutes and " & If(TS.Seconds > 10, TS.Seconds, "0" & TS.Seconds) & " seconds"
                Else
                    Return If(TS.Minutes > 10, TS.Minutes, "0" & TS.Minutes) & " minutes and " & If(TS.Seconds > 10, TS.Seconds, "0" & TS.Seconds) & " seconds"
                End If
            Else
                If TS.Hours <> 0 Then
                    Return TS.Hours & ":" & If(TS.Minutes > 10, TS.Minutes, "0" & TS.Minutes) & ":" & If(TS.Seconds > 10, TS.Seconds, "0" & TS.Seconds)
                Else
                    Return If(TS.Minutes > 10, TS.Minutes, "0" & TS.Minutes) & ":" & If(TS.Seconds > 10, TS.Seconds, "0" & TS.Seconds)
                End If
            End If
        Catch ex As Exception
            Return "ERROR"
        End Try
        'Try
        '    If LongText = False Then
        '        Dim Pos = sec / 60
        '        Dim Mins As String
        '        Try
        '            Mins = Pos.ToString.Substring(0, Pos.ToString.IndexOf("."))
        '        Catch ex As Exception
        '            Mins = Pos.ToString
        '        End Try
        '        Dim secs As String
        '        Try
        '            secs = Pos.ToString.Substring(Pos.ToString.IndexOf("."), Pos.ToString.Length - Pos.ToString.IndexOf("."))
        '        Catch ex As Exception
        '            secs = 0
        '        End Try
        '        Dim ConvSecs = secs * 60
        '        If Mins >= 10 AndAlso Math.Round(ConvSecs) >= 10 Then
        '            Return New String(Mins & ":" & Math.Round(ConvSecs))
        '        ElseIf Mins < 10 AndAlso Math.Round(ConvSecs) >= 10 Then
        '            Return New String("0" & Mins & ":" & Math.Round(ConvSecs))
        '        ElseIf Mins >= 10 AndAlso Math.Round(ConvSecs) < 10 Then
        '            Return New String(Mins & ":" & "0" & Math.Round(ConvSecs))
        '        ElseIf Mins < 10 Or Mins = 0 AndAlso Math.Round(ConvSecs) < 10 Then
        '            Return New String("0" & Mins & ":" & "0" & Math.Round(ConvSecs))
        '        ElseIf Mins < 10 AndAlso Math.Round(ConvSecs) > 59 Then
        '            Return New String("0" & Mins & ":" & "00")
        '        ElseIf Mins > 10 AndAlso Math.Round(ConvSecs) > 59 Then
        '            Return New String(Mins & ":" & "00")
        '        End If
        '    ElseIf LongText = True Then
        '        Dim DeciMins = sec / 60
        '        Dim IntMins = DeciMins.ToString.Substring(0, DeciMins.ToString.IndexOf("."))
        '        Dim DeciSecs = DeciMins.ToString.Substring(DeciMins.ToString.IndexOf("."), DeciMins.ToString.Length - DeciMins.ToString.IndexOf("."))
        '        Dim IntSecs = DeciSecs * 60
        '        If IntSecs < 10 Then
        '            Return New String(IntMins & " min 0" & Math.Round(IntSecs) & " sec")
        '        Else
        '            Return New String(IntMins & " min " & Math.Round(IntSecs) & " sec")
        '        End If
        '    End If
        'Catch ex As Exception
        '    Return "ERROR"
        'End Try
    End Function
    Public Shared Function SecsToMins(TS As TimeSpan, Optional LongText As Boolean = False) As String
        Try
            If LongText Then
                If TS.Hours <> 0 Then
                    Return TS.Hours & " hours, " & If(TS.Minutes > 10, TS.Minutes, "0" & TS.Minutes) & " minutes and " & If(TS.Seconds > 10, TS.Seconds, "0" & TS.Seconds) & " seconds"
                Else
                    Return If(TS.Minutes > 10, TS.Minutes, "0" & TS.Minutes) & " minutes and " & If(TS.Seconds > 10, TS.Seconds, "0" & TS.Seconds) & " seconds"
                End If
            Else
                If TS.Hours <> 0 Then
                    Return TS.Hours & ":" & If(TS.Minutes > 10, TS.Minutes, "0" & TS.Minutes) & ":" & If(TS.Seconds > 10, TS.Seconds, "0" & TS.Seconds)
                Else
                    Return If(TS.Minutes > 10, TS.Minutes, "0" & TS.Minutes) & ":" & If(TS.Seconds > 10, TS.Seconds, "0" & TS.Seconds)
                End If
            End If
        Catch ex As Exception
            Return "ERROR"
        End Try
    End Function
    Public Shared Function GetMins(sec As Double)
        Dim mins = sec / 60
        Try
            Return mins.ToString.Substring(0, mins.ToString.IndexOf("."))
        Catch ex As Exception
            Return 0
        End Try
    End Function
    Public Shared Function GetRestSecs(sec As Double)
        Dim mins = sec / 60
        Dim secs = mins.ToString.Substring(mins.ToString.IndexOf(".")) * 60
        Try
            Return secs
        Catch ex As Exception
            Return 0
        End Try
    End Function
    Public Shared Function SecsToMs(sec As Double) As TimeSpan
        Dim Pos = sec.ToString.IndexOf(".")
        Dim Secs As Integer
        Dim Ms As Integer
        If Pos = -1 Then
            Secs = 0
            Ms = 0
        Else
            Secs = sec.ToString.Substring(0, Pos)
            Ms = sec.ToString.Substring(Pos) * 1000
        End If
        Ms += (Secs * 1000)
        Return TimeSpan.FromMilliseconds(Ms)
    End Function

    Public Shared Sub SaveToPng(ByVal visual As ImageSource, ByVal fileName As String)
        Dim encoder = New PngBitmapEncoder()
        SaveUsingEncoder(visual, fileName, encoder)
    End Sub
    Private Shared Sub SaveUsingEncoder(ByVal source As ImageSource, ByVal fileName As String, ByVal encoder As BitmapEncoder)
        Dim frame As BitmapFrame = BitmapFrame.Create(source)
        encoder.Frames.Add(frame)
        Using stream = IO.File.Create(fileName)
            encoder.Save(stream)
        End Using
    End Sub
    Public Shared Sub RemoveAt(Of T)(ByRef arr As T(), ByVal index As Integer)
        Dim uBound = arr.GetUpperBound(0)
        Dim lBound = arr.GetLowerBound(0)
        Dim arrLen = uBound - lBound

        If index < lBound OrElse index > uBound Then
            Throw New ArgumentOutOfRangeException(
        String.Format("Index must be from {0} to {1}.", lBound, uBound))

        Else
            'create an array 1 element less than the input array
            Dim outArr(arrLen - 1) As T
            'copy the first part of the input array
            Array.Copy(arr, 0, outArr, 0, index)
            'then copy the second part of the input array
            Array.Copy(arr, index + 1, outArr, index, uBound - index)

            arr = outArr
        End If
    End Sub
    Public Shared Function ValToPercentage(Val As Double, Min As Double, Max As Double) As Integer
        If Val > Min AndAlso Val < Max Then
            Return ((Val - Min) * 100) / (Max - Min)
        ElseIf Val = Max Then
            Return 100
        Else
            Return 0
        End If
    End Function
    Public Shared Function PercentageToVal(Percentage As Double, Min As Double, Max As Double) As Integer
        If Percentage > 0 AndAlso Percentage < 100 Then
            Return (((Max - Min) * Percentage) + (Min * 100)) / 100
        ElseIf Percentage >= 100 Then
            Return Max
        Else
            Return Min
        End If
    End Function
    Public Shared Function PercentageToFiveMax(Percentage As Double) As Integer
        'Dim PercentageChunk As Integer = 20
        If Percentage = 0 Then
            Return 0
        ElseIf IsInRange(Percentage, 0, 20, True) Then
            Return 1
        ElseIf IsInRange(Percentage, 20, 40, True) Then
            Return 2
        ElseIf IsInRange(Percentage, 40, 60, True) Then
            Return 3
        ElseIf IsInRange(Percentage, 60, 80, True) Then
            Return 4
        ElseIf IsInRange(Percentage, 80, 100, True, True) Then
            Return 5
        Else
            Return 0
        End If
    End Function
    Public Shared Function IsInRange(Val As Integer, Min As Integer, Max As Integer, Optional IsEqualLower As Boolean = False, Optional IsEqualGreater As Boolean = False) As Boolean
        If IsEqualLower = True AndAlso IsEqualLower = False Then
            If Val >= Min AndAlso Val < Max Then
                Return True
            Else
                Return False
            End If
        ElseIf IsEqualLower = True AndAlso IsEqualLower = True Then
            If Val >= Min AndAlso Val <= Max Then
                Return True
            Else
                Return False
            End If
        ElseIf IsEqualLower = False AndAlso IsEqualLower = True Then
            If Val > Min AndAlso Val <= Max Then
                Return True
            Else
                Return False
            End If
        ElseIf IsEqualLower = False AndAlso IsEqualLower = False Then
            If Val > Min AndAlso Val < Max Then
                Return True
            Else
                Return False
            End If
        Else
            If Val > Min AndAlso Val < Max Then
                Return True
            Else
                Return False
            End If
        End If
    End Function
    Public Shared Function GDIColorToColor(GDIColor As System.Drawing.Color) As System.Windows.Media.Color
        Return Color.FromArgb(GDIColor.A, GDIColor.R, GDIColor.G, GDIColor.B)
    End Function
    Public Shared Function ColorToGDIColor(Color As Color) As System.Drawing.Color
        Return System.Drawing.Color.FromArgb(Color.A, Color.R, Color.G, Color.B)
    End Function
#Region "Flash Window Ex"
    Private Declare Function FlashWindowEx Lib "User32" (ByRef fwInfo As FLASHWINFO) As Boolean

    ' As defined by: http://msdn.microsoft.com/en-us/library/ms679347(v=vs.85).aspx
    Public Enum FlashWindowFlags As UInt32
        ' Stop flashing. The system restores the window to its original state.
        FLASHW_STOP = 0
        ' Flash the window caption.
        FLASHW_CAPTION = 1
        ' Flash the taskbar button.
        FLASHW_TRAY = 2
        ' Flash both the window caption and taskbar button.
        ' This is equivalent to setting the FLASHW_CAPTION | FLASHW_TRAY flags.
        FLASHW_ALL = 3
        ' Flash continuously, until the FLASHW_STOP flag is set.
        FLASHW_TIMER = 4
        ' Flash continuously until the window comes to the foreground.
        FLASHW_TIMERNOFG = 12
    End Enum

    Public Structure FLASHWINFO
        Public cbSize As UInt32
        Public hwnd As IntPtr
        Public dwFlags As FlashWindowFlags
        Public uCount As UInt32
        Public dwTimeout As UInt32
    End Structure

    Public Shared Function FlashWindow(ByRef handle As IntPtr, ByVal FlashTitleBar As Boolean, ByVal FlashTray As Boolean, ByVal FlashCount As Integer) As Boolean
        If handle = Nothing Then Return False

        Try
            Dim fwi As New FLASHWINFO
            With fwi
                .hwnd = handle
                If FlashTitleBar Then .dwFlags = .dwFlags Or FlashWindowFlags.FLASHW_CAPTION
                If FlashTray Then .dwFlags = .dwFlags Or FlashWindowFlags.FLASHW_TRAY
                .uCount = CUInt(FlashCount)
                If FlashCount = 0 Then .dwFlags = .dwFlags Or FlashWindowFlags.FLASHW_TIMERNOFG
                .dwTimeout = 0 ' Use the default cursor blink rate.
                .cbSize = CUInt(System.Runtime.InteropServices.Marshal.SizeOf(fwi))
            End With

            Return FlashWindowEx(fwi)
        Catch
            Return False
        End Try
    End Function
#End Region
    Public Shared Function IsUserAdministrator() As Boolean
        Dim isAdmin As Boolean
        Try
            Dim user As Security.Principal.WindowsIdentity = Security.Principal.WindowsIdentity.GetCurrent()
            Dim principal As Security.Principal.WindowsPrincipal = New Security.Principal.WindowsPrincipal(user)
            isAdmin = principal.IsInRole(Security.Principal.WindowsBuiltInRole.Administrator)
        Catch
            isAdmin = False
        End Try

        Return isAdmin
    End Function
    Public Shared Function DateToSeason(_date As Date) As String
        'Seasons :
        'Winter : December 1 > March 1
        'Spring : March 1 > June 1
        'Summer : June 1 > September 1
        'Fall : September 1 > December 1
        Dim December As New Date(_date.Year, 12, 1)
        Dim March As New Date(_date.Year, 3, 1)
        Dim June As New Date(_date.Year, 6, 1)
        Dim September As New Date(_date.Year, 9, 1)
        If _date >= December AndAlso _date < March Then
            Return AniResolver.WINTER
        ElseIf _date >= March AndAlso _date < June Then
            Return AniResolver.SPRING
        ElseIf _date >= June AndAlso _date < September Then
            Return AniResolver.SUMMER
        ElseIf _date >= September AndAlso _date < December Then
            Return AniResolver.FALL
        Else
            Return String.Empty
        End If
    End Function
#Region "File Assosciation"
    <System.Runtime.InteropServices.DllImport("shell32.dll")> Shared Sub _
    SHChangeNotify(ByVal wEventId As Integer, ByVal uFlags As Integer,
    ByVal dwItem1 As Integer, ByVal dwItem2 As Integer)
    End Sub


    ' Create the new file association
    '
    ' Extension is the extension to be registered (eg ".cad"
    ' ClassName is the name of the associated class (eg "CADDoc")
    ' Description is the textual description (eg "CAD Document"
    ' ExeProgram is the app that manages that extension (eg "c:\Cad\MyCad.exe")

    Public Shared Function CreateFileAssociation(ByVal extension As String,
    ByVal className As String, ByVal description As String,
    ByVal exeProgram As String) As Boolean
        Const SHCNE_ASSOCCHANGED = &H8000000
        Const SHCNF_IDLIST = 0

        ' ensure that there is a leading dot
        If extension.Substring(0, 1) <> "." Then
            extension = "." & extension
        End If

        Dim key1, key2, key3 As Microsoft.Win32.RegistryKey
        Try
            ' create a value for this key that contains the classname
            key1 = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey(extension)
            key1.SetValue("", className)
            ' create a new key for the Class name
            key2 = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey(className)
            key2.SetValue("", description)
            ' associate the program to open the files with this extension
            key3 = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey(className &
            "\Shell\Open\Command")
            key3.SetValue("", exeProgram & " ""%1""")
        Catch
            Return False
        Finally
            If Not key1 Is Nothing Then key1.Close()
            If Not key2 Is Nothing Then key2.Close()
            If Not key3 Is Nothing Then key3.Close()
        End Try

        ' notify Windows that file associations have changed
        SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, 0, 0)
        Return True
    End Function
#End Region
    Public Shared Function ResizeImage(ByRef Image As System.Drawing.Image, W As Integer, H As Integer, Optional Dispose As Boolean = True) As System.Drawing.Image
        Dim Img = New System.Drawing.Bitmap(Image, W, H)
        Image.Dispose()
        Return Img
    End Function
    Public Shared Function ResizeImage(ByRef Image As System.Drawing.Image, Factor As Integer, Optional Dispose As Boolean = True) As System.Drawing.Image
        Dim Img = New System.Drawing.Bitmap(Image, Image.Width / Factor, Image.Height / Factor)
        Image.Dispose()
        Return Img
    End Function
    Public Shared Function FileSizeConverter(Size As ULong) As Double
        Try
            Dim DoubleBytes As Double
            Select Case Size
                Case Is >= 1099511627776
                    DoubleBytes = CDbl(Size / 1099511627776) 'TB
                    Return FormatNumber(DoubleBytes, 2)
                Case 1073741824 To 1099511627775
                    DoubleBytes = CDbl(Size / 1073741824) 'GB
                    Return FormatNumber(DoubleBytes, 2)
                Case 1048576 To 1073741823
                    DoubleBytes = CDbl(Size / 1048576) 'MB
                    Return FormatNumber(DoubleBytes, 2)
                Case 1024 To 1048575
                    DoubleBytes = CDbl(Size / 1024) 'KB
                    Return FormatNumber(DoubleBytes, 2)
                Case 0 To 1023
                    DoubleBytes = Size ' bytes
                    Return FormatNumber(DoubleBytes, 2)
                Case Else
            End Select
        Catch
            Return 0
        End Try
    End Function
    Public Shared Function FileSizeConverterSTR(Size As ULong) As String
        Try
            Dim DoubleBytes As Double
            Select Case Size
                Case Is >= 1099511627776
                    DoubleBytes = CDbl(Size / 1099511627776) 'TB
                    Return FormatNumber(DoubleBytes, 2) & "TB"
                Case 1073741824 To 1099511627775
                    DoubleBytes = CDbl(Size / 1073741824) 'GB
                    Return FormatNumber(DoubleBytes, 2) & "GB"
                Case 1048576 To 1073741823
                    DoubleBytes = CDbl(Size / 1048576) 'MB
                    Return FormatNumber(DoubleBytes, 2) & "MB"
                Case 1024 To 1048575
                    DoubleBytes = CDbl(Size / 1024) 'KB
                    Return FormatNumber(DoubleBytes, 2) & "KB"
                Case 0 To 1023
                    DoubleBytes = Size ' bytes
                    Return FormatNumber(DoubleBytes, 2) & "Bytes"
                Case Else
            End Select
        Catch
            Return 0
        End Try
    End Function
    ''' <summary>
    ''' Return true if all values are true
    ''' </summary>
    ''' <param name="lst"></param>
    ''' <returns></returns>
    Public Shared Function BooleanSum(ByRef lst As IEnumerable(Of Boolean)) As Boolean
        For Each bol In lst
            If bol = False Then
                Return False
            End If
        Next
        Return True
    End Function
    Public Shared Async Function FillTextBlock(TB As TextBlock, fill As String, Optional Delay As Integer = 50, Optional DoubleBuffer As Boolean = False) As Task
        Await EmptyTextBlock(TB, Delay, DoubleBuffer)
        Do While TB.Text <> fill
            Try
                TB.Text &= fill.Substring(TB.Text.Length, If(DoubleBuffer, 2, 1))
            Catch
                TB.Text &= fill.Substring(TB.Text.Length, 1)
            End Try
            Await Task.Delay(Delay)
        Loop
    End Function
    Public Shared Async Function EmptyTextBlock(TB As TextBlock, Optional delay As Integer = 50, Optional DoubleBuffer As Boolean = False) As Task
        Do While Not String.IsNullOrEmpty(TB.Text)
            Try
                TB.Text = TB.Text.Remove(TB.Text.Length - If(DoubleBuffer, 2, 1), If(DoubleBuffer, 2, 1))
            Catch
                TB.Text = TB.Text.Remove(TB.Text.Length - 1, 1)
            End Try
            Await Task.Delay(delay)
        Loop
    End Function
    ''' <summary>
    ''' Converts time in seconds to it's equalivent biggest hour value
    ''' </summary>
    ''' <param name="time">Time in Seconds</param>
    ''' <returns></returns>
    Public Shared Function TimeToString(time As Integer) As String
        Dim TS = TimeSpan.FromSeconds(time)
        If time < 60 Then
            Return TS.Seconds & "s"
        ElseIf time >= 60 AndAlso time < 3600 Then
            Return TS.Minutes & "m"
        ElseIf time >= 3600 Then
            Return TS.Hours & ":" & TS.Minutes
        Else
            Return TS.Minutes & "m"
        End If
    End Function
    Public Shared Function ResourceTypeToInternalImage(type As API.AniDB.AniDBClient.ResourceType) As DrawingImage
        Select Case type
            Case API.AniDB.AniDBClient.ResourceType.AllCinema
                Return AniResolver.ALLCINEMA
            Case API.AniDB.AniDBClient.ResourceType.Amazon
                Return AniResolver.AMAZON
            Case API.AniDB.AniDBClient.ResourceType.AnimeNewsNetwork
                Return AniResolver.ANN
            Case API.AniDB.AniDBClient.ResourceType.Anison
                Return AniResolver.ANISON
            Case API.AniDB.AniDBClient.ResourceType.Crunchyroll
                Return AniResolver.CRUNCHYROLL
            Case API.AniDB.AniDBClient.ResourceType.DotLain
                Return AniResolver.DOTLAIN
            Case API.AniDB.AniDBClient.ResourceType.Facebook
                Return AniResolver.FACEBOOK
            Case API.AniDB.AniDBClient.ResourceType.MAL
                Return AniResolver.MAL
            Case API.AniDB.AniDBClient.ResourceType.Marumegane
                Return AniResolver.MARUMEGANE
            Case API.AniDB.AniDBClient.ResourceType.Netflix
                Return AniResolver.NETFLIX
            Case API.AniDB.AniDBClient.ResourceType.OfficialStreams
                Return AniResolver.STREAM
            Case API.AniDB.AniDBClient.ResourceType.Syoboi
                Return AniResolver.SYOBOI
            Case API.AniDB.AniDBClient.ResourceType.Twitter
                Return AniResolver.TWITTER
            Case API.AniDB.AniDBClient.ResourceType.VNDB
                Return AniResolver.VNDB
            Case API.AniDB.AniDBClient.ResourceType.Website, API.AniDB.AniDBClient.ResourceType.WebsiteOrArcs
                Return AniResolver.WEBSITE
            Case API.AniDB.AniDBClient.ResourceType.WikipediaEN, API.AniDB.AniDBClient.ResourceType.WikipediaJP, API.AniDB.AniDBClient.ResourceType.WikipediaKO, API.AniDB.AniDBClient.ResourceType.WikipediaZH
                Return AniResolver.WIKIPEDIA
            Case API.AniDB.AniDBClient.ResourceType.Youtube
                Return AniResolver.YOUTUBE
            Case Else
                Return AniResolver.WARNING
        End Select
    End Function
    Public Shared ReadOnly Property ConsoleInfoText As String
        Get
            Return "[" & Now.ToString("HH:mm:ss") & "][INFO]: "
        End Get
    End Property
    Public Shared ReadOnly Property ConsoleDebugText As String
        Get
            Return "[" & Now.ToString("HH:mm:ss") & "][DEBUG]: "
        End Get
    End Property
    Public Shared ReadOnly Property ConsoleWarningText As String
        Get
            Return "[" & Now.ToString("HH:mm:ss") & "][WARNING]: "
        End Get
    End Property
End Class