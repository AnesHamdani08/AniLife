Imports System.ComponentModel
Imports System.Runtime.CompilerServices

Public Class LVItem
    Implements INotifyPropertyChanged

    Private _title As String
    Private _score As Double
    Private _progress As String
    Private _type As String

    Public Sub New(ByVal ptitle As String, ByVal pscore As Double, ByVal pprogress As String, ByVal ptype As String, <CallerMemberName> ByVal Optional caller As String = Nothing)
        Title = ptitle
        Score = pscore
        Progress = pprogress
        Type = ptype
    End Sub

    Public Property Title As String
        Get
            Return _title
        End Get
        Set(ByVal value As String)
            If _title = value Then Return
            _title = value
            OnPropertyChanged()
        End Set
    End Property

    Public Property Score As Double
        Get
            Return _score
        End Get
        Set(ByVal value As Double)
            If _score = value Then Return
            _score = value
            OnPropertyChanged()
        End Set
    End Property

    Public Property Progress As String
        Get
            Return _progress
        End Get
        Set(ByVal value As String)
            If _progress = value Then Return
            _progress = value
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

    Public Property Tag As Object
    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    Protected Overridable Sub OnPropertyChanged(<CallerMemberName> ByVal Optional propertyName As String = Nothing)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub
End Class
