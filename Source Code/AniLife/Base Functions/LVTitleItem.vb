Imports System.ComponentModel
Imports System.Runtime.CompilerServices

Public Class LVTitleItem
    Implements INotifyPropertyChanged

    Private _name As String
    Private _type As String
    Private _language As String

    Public Sub New(ByVal pname As String, ByVal ptype As String, planguage As String, <CallerMemberName> ByVal Optional caller As String = Nothing)
        Name = pname
        Type = ptype
        Language = planguage
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

    Public Property Language As String
        Get
            Return _language
        End Get
        Set(ByVal value As String)
            If _language = value Then Return
            _language = value
            OnPropertyChanged()
        End Set
    End Property

    Public Property Tag As Object
    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    Protected Overridable Sub OnPropertyChanged(<CallerMemberName> ByVal Optional propertyName As String = Nothing)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub
End Class
