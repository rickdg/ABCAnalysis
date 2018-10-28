Imports FirstFloor.ModernUI.Presentation
Imports ABCAnalysis.Connection.SqlServer
Imports System.Collections.ObjectModel
Imports System.Data
Imports System.Data.SqlClient

Namespace Pages
    Partial Public Class ProjectManager
        Inherits UserControl

        Public Sub New()
            InitializeComponent()
            DataContext = Me
            MainPage.Model.Update()
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
        Public ReadOnly Property AddNewProject As ICommand = New RelayCommand(
            Sub()
                Dim NewProject As New Project With {
                .Parent = MainWindow.Model,
                .Index = MainWindow.Model.ProjectCounter}
                NewProject.AttachDb()
                Projects.Add(NewProject)
                MainWindow.Model.ProjectCounter += 1
            End Sub)


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
                                                 procParameters As StoredProcedureParameter,
                                                 table As DataTable)
            Using Connection As New SqlConnection(CurrentProject.SqlConnectionString)
                Connection.Open()
                Using Command = Connection.CreateCommand()
                    Command.CommandTimeout = 1800
                    Command.CommandText = procParameters.CommandText
                    Command.CommandType = CommandType.StoredProcedure
                    For Each Item In cmdParameters
                        Command.Parameters.Add(Item.Name, Item.SqlDbType).Value = Item.Value
                    Next
                    Command.Parameters.Add(procParameters.ParameterName, SqlDbType.Structured).TypeName = procParameters.TypeName
                    Command.Parameters(procParameters.ParameterName).Value = table
                    Command.ExecuteReader()
                End Using
            End Using
        End Sub

    End Class
End Namespace