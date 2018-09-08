Imports System.Collections.ObjectModel
Imports ABCAnalysis.Pages
Imports FirstFloor.ModernUI.Presentation

Namespace Content
    Partial Public Class SettingsAbcGroups
        Inherits UserControl

        Public Sub New()
            InitializeComponent()
            DataContext = Me
        End Sub


        Public Property EntitiesCollection As ObservableCollection(Of AbcGroupVM) = MainPage.Model.AbcGroups


        Public ReadOnly Property CmdAddNewEntity As ICommand = New RelayCommand(AddressOf AddNewEntityExecute)
        Private Sub AddNewEntityExecute(parameter As Object)
            Using Context As New AbcAnalysisEntities
                Dim NewEntity = Context.AbcGroups.Add(New AbcGroup)
                Context.SaveChanges()
                EntitiesCollection.Add(New AbcGroupVM With {.ParentCollection = EntitiesCollection, .Entity = NewEntity})
            End Using
        End Sub

    End Class
End Namespace