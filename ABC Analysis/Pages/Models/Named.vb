Imports FirstFloor.ModernUI.Presentation
Imports Newtonsoft.Json

Namespace Pages
    Public Class Named
        Inherits NotifyPropertyChanged

        Private _Value As Integer


        Public Property Parent As TemplateVM
        <JsonIgnore>
        Public ReadOnly Property Change As ICommand = New RelayCommand(Sub() Parent.UpdateCategoryes())
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