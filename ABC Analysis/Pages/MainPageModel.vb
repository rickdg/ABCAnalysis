Imports System.Collections.ObjectModel
Imports FirstFloor.ModernUI.Presentation
Imports Newtonsoft.Json

Namespace Pages
    Public Class MainPageModel
        Inherits NotifyPropertyChanged

        Private _CurrentTemplate As TemplateVM
        Private _Templates As ObservableCollection(Of TemplateVM)


        Public Sub New()
            If ProjectManager.CurrentProject Is Nothing Then Return
            Templates = ProjectManager.CurrentProject.Templates
        End Sub


        <JsonIgnore>
        Public Property AbcGroups As New ObservableCollection(Of AbcGroupModel)
        <JsonIgnore>
        Public Property Templates As ObservableCollection(Of TemplateVM)
            Get
                Return _Templates
            End Get
            Set
                _Templates = Value
                OnPropertyChanged("Templates")
            End Set
        End Property
        <JsonIgnore>
        Public Property CurrentTemplate As TemplateVM
            Get
                Return _CurrentTemplate
            End Get
            Set
                _CurrentTemplate = Value
                OnPropertyChanged("CurrentTemplate")
            End Set
        End Property

        <JsonIgnore>
        Public ReadOnly Property CmdAddNewTemplate As ICommand = New RelayCommand(
            Sub()
                If ProjectManager.CurrentProject Is Nothing Then Return
                If Templates Is Nothing Then
                    Templates = ProjectManager.CurrentProject.Templates
                End If
                Dim NewTemplateVM = New TemplateVM(True) With {.Parent = ProjectManager.CurrentProject}
                NewTemplateVM.ReductionPickPercent.Value2 = 0.01
                Templates.Add(NewTemplateVM)
                CurrentTemplate = NewTemplateVM
            End Sub)
        <JsonIgnore>
        Public ReadOnly Property CmdShowHint As ICommand = New RelayCommand(AddressOf ShowHintExecute)


        Public Sub Update()
            Dim Project = ProjectManager.CurrentProject

            Templates = Project.Templates
            CurrentTemplate = Nothing

            Using Context = Project.Context
                AbcGroups.Clear()
                For Each AbcGroup In Context.AbcGroups
                    AbcGroups.Add(New AbcGroupModel() With {
                                  .ParentCollection = AbcGroups,
                                  .Entity = AbcGroup})
                Next
            End Using
        End Sub


        Public Sub UpdateTemplates()
            For Each Tmp In Templates
                Tmp.UpdateSubinventories()
                Tmp.UpdateUserPositionTypesAndCategoryes()
            Next
        End Sub

    End Class
End Namespace