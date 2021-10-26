Public Class NotificationElement
    Public Property Message
        Get
            Return Message_TB.Text
        End Get
        Set(value)
            Message_TB.Text = value
        End Set
    End Property

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub
    Public Sub New(Message As String)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Message_TB.Text = Message
    End Sub
End Class
