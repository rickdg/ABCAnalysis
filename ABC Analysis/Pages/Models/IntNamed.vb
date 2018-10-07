Imports FirstFloor.ModernUI.Presentation
Imports Newtonsoft.Json

Namespace Pages
    Public Class NamedInt

        Public Property Parent As TemplateVM
        <JsonIgnore>
        Public ReadOnly Property Change As ICommand = New RelayCommand(Sub() Parent.UpdateUserPositionTypesAndCategoryes())
        Public Property IsChecked As Boolean
        Public Property Name As Integer

    End Class
End Namespace