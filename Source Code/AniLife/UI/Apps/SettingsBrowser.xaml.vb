Imports System.ComponentModel
Imports System.Runtime.CompilerServices

Public Class SettingsBrowser
    Public Class SettingItem
        Implements INotifyPropertyChanged

        Private _num As Integer
        Private _name As String
        Private _value As String
        Private _type As String
        Public Sub New(pnum As String, pname As String, pvalue As String, ptype As String, <CallerMemberName> ByVal Optional caller As String = Nothing)
            Num = pnum
            Name = pname
            Value = pvalue
            Type = ptype
        End Sub

        Public Property Num As Integer
            Get
                Return _num
            End Get
            Set(ByVal value As Integer)
                If _num = value Then Return
                _num = value
                OnPropertyChanged()
            End Set
        End Property

        Public Property Name As String
            Get
                Return _name
            End Get
            Set(ByVal value As String)
                If _name = value Then Return
                _name = value
                OnPropertyChanged()
            End Set
        End Property

        Public Property Value As String
            Get
                Return _value
            End Get
            Set(ByVal value As String)
                If _value = value Then Return
                _value = value
                OnPropertyChanged()
            End Set
        End Property

        Public Property Type As String
            Get
                Return _type
            End Get
            Set(ByVal value As String)
                If _type = value Then Return
                _type = value
                OnPropertyChanged()
            End Set
        End Property
        Public Property Tag As System.Configuration.SettingsPropertyValue
        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

        Protected Overridable Sub OnPropertyChanged(<CallerMemberName> ByVal Optional propertyName As String = Nothing)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
        End Sub
    End Class

    Private Settings_Source As New ObjectModel.ObservableCollection(Of SettingItem)

    Private Sub SettingsBrowser_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Main_SettingsView.ItemsSource = Settings_Source
        Dim Dummy = My.Settings.DUMMY
        Dim i = 0
        Dim templist As New List(Of SettingItem)
        For Each value As System.Configuration.SettingsPropertyValue In My.Settings.PropertyValues
            If value.PropertyValue IsNot Nothing Then
                If value.PropertyValue.GetType IsNot GetType(Specialized.StringCollection) Then
                    templist.Add(New SettingItem(i + 1, value.Name, value.PropertyValue.ToString, value.PropertyValue.GetType.Name) With {.Tag = value})
                Else
                    templist.Add(New SettingItem(i + 1, value.Name, TryCast(value.PropertyValue, Specialized.StringCollection).Count & " Item(s)", value.PropertyValue.GetType.Name) With {.Tag = value})
                End If
                i += 1
            Else
                Try
                    templist.Add(New SettingItem(i + 1, value.Name, "Nothing", value.PropertyValue.GetType.Name) With {.Tag = value})
                Catch ex As Exception
                    templist.Add(New SettingItem(i + 1, value.Name, "Nothing", "N/A") With {.Tag = value})
                End Try
                i += 1
            End If
        Next
        templist = templist.OrderBy(Function(k) k.Name).ToList
        For _i As Integer = 0 To templist.Count - 1
            templist(_i).Num = _i + 1
        Next
        For Each si In templist
            Settings_Source.Add(si)
        Next
    End Sub

    Private Sub SettingsBrowser_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        Hide()
        e.Cancel = True
    End Sub

    Private Sub Main_SettingsView_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles Main_SettingsView.MouseDoubleClick
        Dim CurSet = Settings_Source(Main_SettingsView.SelectedIndex)?.Tag
        Dim CurType = CurSet?.PropertyValue?.GetType
        If CurType IsNot Nothing Then
            Select Case CurType
                Case GetType(System.String)
                    Dim IB As New InputDialog(Me, "Value for " & CurSet.Name & " with type " & CurType.Name)
                    If IB.ShowDialog Then
                        CurSet.PropertyValue = IB.Input
                        My.Settings.Save()
                    End If
                Case GetType(System.Boolean)
                    Dim IB As New InputDialog(Me, "Value for " & CurSet.Name & " with type " & CurType.Name, InputDialog.DialogType.BooleanSwitch)
                    If IB.ShowDialog Then
                        CurSet.PropertyValue = IB.Input
                        My.Settings.Save()
                    End If
                Case GetType(System.Double)
                    Dim IB As New InputDialog(Me, "Value for " & CurSet.Name & " with type " & CurType.Name, InputDialog.DialogType.DoubleInput)
                    If IB.ShowDialog Then
                        CurSet.PropertyValue = IB.Input
                        My.Settings.Save()
                    End If
                Case GetType(System.Int32)
                    Dim IB As New InputDialog(Me, "Value for " & CurSet.Name & " with type " & CurType.Name, InputDialog.DialogType.IntegerInput)
                    If IB.ShowDialog Then
                        CurSet.PropertyValue = IB.Input
                        My.Settings.Save()
                    End If
                Case GetType(System.Single)
                    Dim IB As New InputDialog(Me, "Value for " & CurSet.Name & " with type " & CurType.Name, InputDialog.DialogType.DoubleInput)
                    IB.MaximumValue = 1
                    IB.MinimumValue = 0
                    If IB.ShowDialog Then
                        CurSet.PropertyValue = IB.Input
                        My.Settings.Save()
                    End If
                Case GetType(Specialized.StringCollection)
                    Dim IBS As New InputDialog(Me, "0-Edit" & Environment.NewLine & "1-Append" & Environment.NewLine & "2-Remove" & Environment.NewLine & "3-Clear", InputDialog.DialogType.IntegerInput)
                    IBS.MinimumValue = 0
                    IBS.MaximumValue = 3
                    If IBS.ShowDialog Then
                        Select Case IBS.Input
                            Case 0
                                Dim SB As New Text.StringBuilder
                                Dim CCol = CType(CurSet.PropertyValue, Specialized.StringCollection)
                                For i As Integer = 0 To CCol.Count - 1
                                    SB.AppendLine(i & "-" & CCol(i))
                                Next
                                Dim IB As New InputDialog(Me, "Select a value to change..." & Environment.NewLine & SB.ToString, InputDialog.DialogType.IntegerInput)
                                If IB.ShowDialog Then
                                    Dim IB2 As New InputDialog(Me, "Change " & CurSet.Name & "[" & IB.Input & "]" & " = " & CCol(IB.Input) & " To...")
                                    If IB2.ShowDialog Then
                                        CCol(IB.Input) = IB2.Input
                                        My.Settings.Save()
                                    End If
                                End If
                            Case 1
                                Dim CCol = CType(CurSet.PropertyValue, Specialized.StringCollection)
                                Dim IB As New InputDialog(Me, "Value to append...")
                                If IB.ShowDialog Then
                                    CCol.Add(IB.Input)
                                    My.Settings.Save()
                                End If
                            Case 2
                                Dim CCol = CType(CurSet.PropertyValue, Specialized.StringCollection)
                                Dim IB As New InputDialog(Me, "Remove at...", InputDialog.DialogType.IntegerInput)
                                If IB.ShowDialog Then
                                    CCol.Removeat (IB.Input)
                                    My.Settings.Save()
                                End If
                            Case 3
                                CType(CurSet.PropertyValue, Specialized.StringCollection).Clear()
                                My.Settings.Save()
                        End Select
                    End If
                Case GetType(System.Windows.Media.Color)

            End Select
        End If
    End Sub

    Private Sub TitleBar_Refresh_Click(sender As Object, e As RoutedEventArgs) Handles TitleBar_Refresh.Click
        Settings_Source.Clear()
        Dim Dummy = My.Settings.DUMMY
        Dim i = 0
        Dim templist As New List(Of SettingItem)
        For Each value As System.Configuration.SettingsPropertyValue In My.Settings.PropertyValues
            If value.PropertyValue IsNot Nothing Then
                If value.PropertyValue.GetType IsNot GetType(Specialized.StringCollection) Then
                    templist.Add(New SettingItem(i + 1, value.Name, value.PropertyValue.ToString, value.PropertyValue.GetType.Name) With {.Tag = value})
                Else
                    templist.Add(New SettingItem(i + 1, value.Name, TryCast(value.PropertyValue, Specialized.StringCollection).Count & " Item(s)", value.PropertyValue.GetType.Name) With {.Tag = value})
                End If
                i += 1
            Else
                Try
                    templist.Add(New SettingItem(i + 1, value.Name, "Nothing", value.PropertyValue.GetType.Name) With {.Tag = value})
                Catch ex As Exception
                    templist.Add(New SettingItem(i + 1, value.Name, "Nothing", "N/A") With {.Tag = value})
                End Try
                i += 1
            End If
        Next
        templist = templist.OrderBy(Function(k) k.Name).ToList
        For _i As Integer = 0 To templist.Count - 1
            templist(_i).Num = _i + 1
        Next
        For Each si In templist
            Settings_Source.Add(si)
        Next
    End Sub
End Class
