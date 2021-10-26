Imports System.IO
Public Class MultiTextWriter
    Inherits TextWriter

    Private writers As IEnumerable(Of TextWriter)

    Public Sub New(ByVal writers As IEnumerable(Of TextWriter))
        Me.writers = writers.ToList()
    End Sub

    Public Sub New(ParamArray writers As TextWriter())
        Me.writers = writers
    End Sub

    Public Overrides Sub Write(ByVal value As Char)
        For Each writer In writers
            writer.Write(value)
        Next
    End Sub

    Public Overrides Sub Write(ByVal value As String)
        For Each writer In writers
            writer.Write(value)
        Next
    End Sub

    Public Overrides Sub Flush()
        For Each writer In writers
            writer.Flush()
        Next
    End Sub

    Public Overrides Sub Close()
        For Each writer In writers
            writer.Close()
        Next
    End Sub

    Public Overrides ReadOnly Property Encoding As Text.Encoding
        Get
            Return Text.Encoding.ASCII
        End Get
    End Property

    Public Class ControlWriter
        Inherits TextWriter

        Public Property Dispatcher As System.Windows.Threading.Dispatcher
        Private textbox As TextBox

        Public Sub New(ByVal textbox As TextBox, dsp As Threading.Dispatcher)
            Dispatcher = dsp
            Me.textbox = textbox
        End Sub

        Public Overrides Sub Write(ByVal value As Char) ', <Runtime.CompilerServices.CallerMemberName> Optional ByVal MemberName As String = Nothing, <Runtime.CompilerServices.CallerLineNumber> Optional ByVal MemberNumber As String = Nothing)            
            Dispatcher.InvokeAsync(Sub()
                                       textbox.Text += value
                                   End Sub)
        End Sub

        Public Overrides Sub Write(ByVal value As String) ', <Runtime.CompilerServices.CallerMemberName> Optional ByVal MemberName As String = Nothing, <Runtime.CompilerServices.CallerLineNumber> Optional ByVal MemberNumber As String = Nothing)            
            Dispatcher.InvokeAsync(Sub()
                                       textbox.Text += value
                                   End Sub)
        End Sub

        Public Overrides ReadOnly Property Encoding As Text.Encoding
            Get
                Return Text.Encoding.ASCII
            End Get
        End Property
    End Class

End Class
