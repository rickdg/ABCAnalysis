Imports System.Collections.ObjectModel
Imports ABCAnalysis.Content
Imports FirstFloor.ModernUI.Presentation
Imports Newtonsoft.Json

Namespace Pages
    Public Class MainPageVM
        Inherits NotifyPropertyChanged

        Private _CurrentTemplate As TemplateVM


        Public Sub New()
            Using Context As New AbcAnalysisEntities
                For Each AbcGroup In Context.AbcGroups
                    AbcGroups.Add(New AbcGroupVM() With {.Entity = AbcGroup})
                Next
            End Using
        End Sub


#Region "ViewModel"
        <JsonIgnore>
        Public Property AbcGroups As New ObservableCollection(Of AbcGroupVM)


#Region "Serialize"
        Public Property SerializeFileName As String
        Public Property Templates As New ObservableCollection(Of TemplateVM)
        Public Property CurrentTemplate As TemplateVM
            Get
                Return _CurrentTemplate
            End Get
            Set
                _CurrentTemplate = Value
                OnPropertyChanged("CurrentTemplate")
            End Set
        End Property
#End Region


#Region "Commands"
        <JsonIgnore>
        Public ReadOnly Property CmdSave As ICommand = New RelayCommand(Sub() Serialize(Me, SerializeFileName))
        <JsonIgnore>
        Public ReadOnly Property CmdAddNewTemplate As ICommand = New RelayCommand(Sub()
                                                                                      Dim NewTemplateVM = New TemplateVM(True) With {.Parent = Me}
                                                                                      NewTemplateVM.ReductionPickPercent.Value2 = 0.01
                                                                                      Templates.Add(NewTemplateVM)
                                                                                      CurrentTemplate = NewTemplateVM
                                                                                  End Sub)
#End Region
#End Region


        Public Sub UpdateTemplates()
            For Each Tmp In Templates
                Tmp.UpdateSubinventories()
                Tmp.UpdateUserPositionTypesAndCategoryes()
            Next
        End Sub

    End Class
End Namespace