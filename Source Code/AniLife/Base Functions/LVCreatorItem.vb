Imports System.ComponentModel
Imports System.Runtime.CompilerServices

Public Class LVCreatorItem
    Implements INotifyPropertyChanged

    Private _name As String
    Private _credit As String

    Public Sub New(ByVal pname As String, ByVal pcredit As String, <CallerMemberName> ByVal Optional caller As String = Nothing)
        Name = pname
        Credit = pcredit
    End Sub

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

    Public Property Credit As String
        Get
            Return _credit
        End Get
        Set(ByVal value As String)
            If _credit = value Then Return
            _credit = value
            OnPropertyChanged()
        End Set
    End Property

    Public Property Tag As Object
    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    Protected Overridable Sub OnPropertyChanged(<CallerMemberName> ByVal Optional propertyName As String = Nothing)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub
End Class
