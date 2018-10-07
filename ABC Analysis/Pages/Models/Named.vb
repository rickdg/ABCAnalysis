Imports FirstFloor.ModernUI.Presentation

Namespace Pages
    Public Class Named
        Inherits NotifyPropertyChanged

        Private _Value As Integer

        Public Property IsChecked As Boolean
        Public Property Id As Integer
        Public Property Name As String
        Public Property Value As Integer
            Get
                Return _Value
            End Get
            Set
                _Value = Value
                OnPropertyChanged("Value")
            End Set
        End Property

    End Class
End Namespace