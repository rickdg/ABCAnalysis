Imports FirstFloor.ModernUI.Presentation
Imports ABCAnalysis.Connection.SqlServer
Imports Newtonsoft.Json
Imports System.Collections.ObjectModel
Imports System.Data
Imports System.Data.SqlClient

Namespace Pages
    Partial Public Class DatabaseManager
        Inherits UserControl

        Public Sub New()
            InitializeComponent()
            DataContext = Me
        End Sub


        Public Shared Property Databases As ObservableCollection(Of DataBaseVM) = MainWindow.Model.Databases
        Public Shared ReadOnly Property CurrentDatabase As DataBaseVM
            Get
                Return Databases.SingleOrDefault(Function(i) i.IsActive)
            End Get
        End Property


        <JsonIgnore>
        Public ReadOnly Property AddDatabase As ICommand = New RelayCommand(AddressOf AddDatabaseExecute)
        Private Sub AddDatabaseExecute(parameter As Object)
            Dim NotExist = CurrentDatabase Is Nothing
            If NotExist Then
                For Each Item In Databases
                    Item.IsActive = False
                Next
            End If
            Dim NewDatabase As New DataBaseVM With {
                .Parent = MainWindow.Model,
                .Index = MainWindow.Model.DatabaseCount,
                .IsActive = NotExist}
            NewDatabase.Attach()
            Databases.Add(NewDatabase)
            MainWindow.Model.DatabaseCount += 1
        End Sub


        Public Shared Sub ExecuteSqlCommand(commandText As String, connectionString As String)
            Using Connection As New SqlConnection(connectionString)
                Connection.Open()
                Using Command = Connection.CreateCommand()
                    Command.CommandTimeout = 1800
                    Command.CommandText = commandText
                    Command.ExecuteNonQuery()
                End Using
            End Using
        End Sub


        Public Shared Sub ExecuteStoredProcedure(cmdParameters As IEnumerable(Of CommandParameter),
                                           procParameters As StoredProcedureParameter,
                                           parameterValue As DataTable)
            Using Connection As New SqlConnection(CurrentDatabase.SqlConnectionString)
                Connection.Open()
                Using Command = Connection.CreateCommand()
                    Command.CommandTimeout = 1800
                    Command.CommandText = procParameters.CommandText
                    Command.CommandType = CommandType.StoredProcedure
                    For Each Item In cmdParameters
                        Command.Parameters.Add(Item.Name, Item.SqlDbType).Value = Item.Value
                    Next
                    Command.Parameters.Add(procParameters.ParameterName, SqlDbType.Structured).TypeName = procParameters.TypeName
                    Command.Parameters(procParameters.ParameterName).Value = parameterValue
                    Command.ExecuteReader()
                End Using
            End Using
        End Sub

    End Class
End Namespace