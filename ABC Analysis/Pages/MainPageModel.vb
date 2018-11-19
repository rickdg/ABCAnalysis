Imports System.Collections.ObjectModel
Imports System.Data
Imports System.Data.SqlClient
Imports ABCAnalysis.Connection.SqlServer
Imports FirstFloor.ModernUI.Presentation

Namespace Pages
    Public Class MainPageModel
        Inherits NotifyPropertyChanged

        Private _CurrentTemplate As TemplateVM
        Private _Templates As ObservableCollection(Of TemplateVM)


        Public Sub New()
            Update()
        End Sub


        Public Shared Property ProjectExist As Func(Of Object, Boolean) = Function(parameter) CurrentProject IsNot Nothing
        Public Shared Property IsAbcDataChanged As Boolean
        Public Shared Property IsDataChanged As Boolean
        Public Shared Property Projects As ObservableCollection(Of Project) = MainWindow.Model.Projects
        Public Shared Property CurrentProject As Project
            Get
                Return Projects.SingleOrDefault(Function(i) i.IsSelected)
            End Get
            Set
                If Value Is Nothing Then
                    MainPage.Model.Templates = Nothing
                    Return
                End If
                For Each Item In Projects
                    Item.IsSelected = False
                Next
                Value.IsSelected = True
                MainPage.Model.Update()
                IsAbcDataChanged = True
                IsDataChanged = True
            End Set
        End Property
        Public Property AbcGroups As New ObservableCollection(Of AbcGroupModel)
        Public Property Templates As ObservableCollection(Of TemplateVM)
            Get
                Return _Templates
            End Get
            Set
                _Templates = Value
                OnPropertyChanged("Templates")
            End Set
        End Property
        Public Property CurrentTemplate As TemplateVM
            Get
                Return _CurrentTemplate
            End Get
            Set
                _CurrentTemplate = Value
                OnPropertyChanged("CurrentTemplate")
            End Set
        End Property
        Public ReadOnly Property AddNewProject As ICommand = New RelayCommand(
            Sub()
                Dim NewProject As New Project With {
                .Parent = MainWindow.Model,
                .Index = MainWindow.Model.ProjectCounter}
                NewProject.AttachDb()
                Projects.Add(NewProject)
                MainWindow.Model.ProjectCounter += 1
            End Sub)
        Public ReadOnly Property CmdAddNewTemplate As ICommand = New RelayCommand(
            Sub()
                If CurrentProject Is Nothing Then Return
                If Templates Is Nothing Then
                    Templates = CurrentProject.Templates
                End If
                Dim NewTemplate = New TemplateVM(True) With {.Parent = CurrentProject}
                NewTemplate.ReductionPickPercent.Value2 = 0.01
                Templates.Add(NewTemplate)
                CurrentTemplate = NewTemplate
            End Sub)
        Public ReadOnly Property CmdShowHint As ICommand = New RelayCommand(AddressOf ShowHintExecute)


        Public Sub Update()
            If CurrentProject Is Nothing Then Return
            Templates = CurrentProject.Templates
            CurrentTemplate = Nothing

            Using Context = CurrentProject.Context
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


        Public Shared Sub SqlCommandExecute(commandText As String, connectionString As String)
            Using Connection As New SqlConnection(connectionString)
                Connection.Open()
                Using Command = Connection.CreateCommand()
                    Command.CommandTimeout = 1800
                    Command.CommandText = commandText
                    Command.ExecuteNonQuery()
                End Using
            End Using
        End Sub


        Public Shared Sub StoredProcedureExecute(cmdParameters As IEnumerable(Of CommandParameter),
                                                 procParameter As StoredProcedureParameter,
                                                 table As DataTable)
            Using Connection As New SqlConnection(CurrentProject.SqlConnectionString)
                Connection.Open()
                Using Command = Connection.CreateCommand()
                    Command.CommandTimeout = 1800
                    Command.CommandText = procParameter.CommandText
                    Command.CommandType = CommandType.StoredProcedure
                    For Each Item In cmdParameters
                        Command.Parameters.Add(Item.Name, Item.SqlDbType).Value = Item.Value
                    Next
                    Command.Parameters.Add(procParameter.ParameterName, SqlDbType.Structured).TypeName = procParameter.TypeName
                    Command.Parameters(procParameter.ParameterName).Value = table
                    Command.ExecuteReader()
                End Using
            End Using
        End Sub

    End Class
End Namespace