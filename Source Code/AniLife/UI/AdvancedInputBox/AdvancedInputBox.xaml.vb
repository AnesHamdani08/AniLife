Namespace AdvancedInputBox
    Public Class AdvancedInputBox
        Public Interface IAdvancedInputBoxElement
            ReadOnly Property Result As InputBoxElementResult
            Property Tag As Object
            Property IsRequired As Boolean
            ReadOnly Property IsNull As Boolean
            ReadOnly Property Name As String
            Sub Notify()
        End Interface
        Public Class InputBoxElementResult
            Private _Value As Object
            ''' <summary>
            ''' Determined from
            ''' <see cref="Type"/>
            ''' </summary>
            ''' <returns></returns>
            Public ReadOnly Property Value As Object
                Get
                    Return _Value
                End Get
            End Property
            Private _IsRequired As Boolean
            Public ReadOnly Property IsRequired As Boolean
                Get
                    Return _IsRequired
                End Get
            End Property
            Private _Type As ReturnType
            Public ReadOnly Property Type As ReturnType
                Get
                    Return _Type
                End Get
            End Property
            Public Property Tag As Object
            Public Sub New(Value As Object, IsReq As Boolean, Type As ReturnType)
                _Value = Value
                _IsRequired = IsReq
                _Type = Type
            End Sub
            Public Sub New(Value As Object, IsReq As Boolean, Type As ReturnType, Tag As Object)
                _Value = Value
                _IsRequired = IsReq
                _Type = Type
                Me.Tag = Tag
            End Sub
            Public Overrides Function ToString() As String
                Return Type.ToString & ";" & Value.GetType.Name
            End Function
            Public Enum ReturnType
                [Object]
                Text
                Number
                Bool
            End Enum
        End Class
        Public Class InputBoxResult
            Public Values As New Dictionary(Of String, InputBoxElementResult)
            Public Overrides Function ToString() As String
                Return String.Join(";", Values)
            End Function
        End Class
        Public Property Result As InputBoxResult
        Public Sub New(Name As String, Header As String, Msg As String, items As IEnumerable(Of IAdvancedInputBoxElement))

            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.
            Title = If(Name, "")
            IB_Header.Text = Header
            IB_Message.Text = Msg
            For Each item In items
                CType(item, FrameworkElement).Margin = New Thickness(0, 0, 0, 10)
                IB_Elements.Children.Insert(IB_Elements.Children.Count - 2, item)
            Next
        End Sub
        Public Sub New(Header As String, Msg As String, items As IEnumerable(Of IAdvancedInputBoxElement))

            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.
            Title = ""
            ExtendViewIntoNonClientArea = True
            CloseButtonBackground = New SolidColorBrush(Color.FromArgb(127, 0, 0, 0))
            IB_Header.Text = Header
            IB_Message.Text = Msg
            If items IsNot Nothing Then
                For Each item In items
                    CType(item, FrameworkElement).Margin = New Thickness(0, 0, 0, 10)
                    IB_Elements.Children.Insert(IB_Elements.Children.Count - 2, item)
                Next
            End If
        End Sub
        Public Sub New(Name As String, Header As String, Msg As String, group As Controls.CommandButtonGroup)

            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.
            Title = If(Name, "")
            IB_Header.Text = Header
            IB_Message.Text = Msg
            If group IsNot Nothing Then
                AddHandler group.OnCommandSelected, Sub()
                                                        Dim R As New InputBoxResult
                                                        R.Values.Add(TryCast(group.Result.Value, IAdvancedInputBoxElement)?.Name, group.Result)
                                                        Result = R
                                                        DialogResult = True
                                                    End Sub
                IB_Elements.Children.Insert(IB_Elements.Children.Count - 2, group)
            End If
        End Sub
        Public Sub New(Header As String, Msg As String, group As Controls.CommandButtonGroup)

            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.
            Title = ""
            ExtendViewIntoNonClientArea = True
            CloseButtonBackground = New SolidColorBrush(Color.FromArgb(127, 0, 0, 0))
            IB_Header.Text = Header
            IB_Message.Text = Msg
            If group IsNot Nothing Then
                AddHandler group.OnCommandSelected, Sub()
                                                        Dim R As New InputBoxResult
                                                        R.Values.Add(TryCast(group.Result.Value, IAdvancedInputBoxElement)?.Name, group.Result)
                                                        Result = R
                                                        DialogResult = True
                                                    End Sub
                IB_Elements.Children.Insert(IB_Elements.Children.Count - 2, group)
            End If
        End Sub
        Public Shared Function ShowQuick(Name As String, Head As String, Msg As String, items As IEnumerable(Of IAdvancedInputBoxElement)) As InputBoxResult
            Dim IB As New AdvancedInputBox(Name, Head, Msg, items)
            IB.ShowDialog()
            Return IB.Result
        End Function
        Public Shared Function ShowQuick(Head As String, Msg As String, items As IEnumerable(Of IAdvancedInputBoxElement)) As InputBoxResult
            Dim IB As New AdvancedInputBox(Head, Msg, items)
            IB.ShowDialog()
            Return IB.Result
        End Function
        Public Shared Function ShowCommand(Name As String, Head As String, Msg As String, content As Controls.CommandButtonGroup) As Controls.CommandButton
            Dim IB As New AdvancedInputBox(Name, Head, Msg, content)
            IB.ShowDialog()
            Try
                Return TryCast(IB.Result.Values.First.Value.Value, Controls.CommandButton)
            Catch
                Return Nothing
            End Try
        End Function
        Public Shared Function ShowCommand(Head As String, Msg As String, content As Controls.CommandButtonGroup) As Controls.CommandButton
            Dim IB As New AdvancedInputBox(Head, Msg, content)
            IB.ShowDialog()
            Try
                Return TryCast(IB.Result.Values.First.Value.Value, Controls.CommandButton)
            Catch
                Return Nothing
            End Try
        End Function
        Private Sub Done_Btn_Click(sender As Object, e As RoutedEventArgs) Handles Done_Btn.Click
            Dim R As New InputBoxResult
            For Each item In IB_Elements.Children
                If TypeOf item Is IAdvancedInputBoxElement Then
                    Dim CItem = CType(item, IAdvancedInputBoxElement)
                    If CItem.IsRequired = True AndAlso CItem.IsNull = True Then
                        CItem.Notify()
                        Exit Sub
                    End If
                    Try
                        R.Values.Add(CItem.Name, CItem.Result)
                    Catch
                    End Try
                End If
            Next
            Result = R
            DialogResult = True
        End Sub
    End Class
End Namespace
