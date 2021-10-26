Public Class UniSharedFunctions
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
End Class
