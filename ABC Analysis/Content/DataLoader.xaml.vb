﻿Imports System.Data
Imports System.Data.OleDb
Imports System.Data.SqlClient
Imports System.Text
Imports ABCAnalysis.ExcelConnection
Imports FirstFloor.ModernUI.Windows.Controls
Imports Microsoft.Win32

Namespace Content
    Partial Public Class DataLoader
        Inherits UserControl

        Private Dialog As ModernDialog


        Public Sub New(dlg As ModernDialog)
            InitializeComponent()
            Dialog = dlg

            Dim DialogWindow As New OpenFileDialog With {.Title = "Выбрать файл"}
            If DialogWindow.ShowDialog Then
                Dim LoadTask As New Task(Sub() LoadTasks(DialogWindow.FileName))
                LoadTask.Start()
                Dialog.Title = "Запрос"
                Dialog.Buttons.First.Visibility = Visibility.Collapsed
            Else
                Dialog.Title = "Отменено"
                ProgressRing.IsActive = False
            End If
        End Sub


        Public Sub LoadTasks(fileName As String)
            Try
                Dim ExcelTable As New DataTable

                Using Connection As New OleDbConnection($"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={fileName};Extended Properties='Excel 12.0;HDR=YES';")
                    Connection.Open()
                    Dim Table = (From Row In Connection.GetSchema("Columns")
                                 Group New Column(Row.Field(Of String)("COLUMN_NAME"), Row.Field(Of Integer)("DATA_TYPE"))
                                      By TableName = Row.Field(Of String)("TABLE_NAME").Trim("'"c) Into Columns = ToList
                                 Where TableName.EndsWith("$")
                                 Select New Table(TableName) With {.Columns = Columns}).First

                    Dim LoadType = ExcelConnection.LoadType.PickTasks
                    Dim SQL = GetScript(LoadType, Table.Name)
                    Dim CheckResult = CheckColumns(LoadType, Table.Columns)
                    If CheckResult <> "" Then Throw New ArgumentException(CheckResult)

                    Using Adapter As New OleDbDataAdapter(SQL, Connection)
                        Dim RecordCount = Adapter.Fill(ExcelTable)
                        If RecordCount = 0 Then Throw New ArgumentException("Нет данных для загрузки")

                        Dispatcher.Invoke(Sub()
                                              Dialog.Title = "Загрузка"
                                              Message.BBCode = $"Количество строк [b]{RecordCount}[/b]"
                                          End Sub)
                    End Using
                End Using

                ExecuteStoredProcedure("dbo.LoadTasks", "@ExcelTasks", "TaskExcelTable", ExcelTable)

                Dispatcher.Invoke(Sub()
                                      Dialog.Title = "Завершено"
                                      Dialog.Buttons = {Dialog.OkButton}
                                      Complete.Visibility = Visibility.Visible
                                  End Sub)
            Catch ex As Exception
                Dispatcher.Invoke(Sub()
                                      Dialog.Title = "Ошибка"
                                      Message.BBCode = GetInnerException(ex)
                                      Warning.Visibility = Visibility.Visible
                                  End Sub)
            Finally
                Dispatcher.Invoke(Sub()
                                      Dialog.Buttons.First.Visibility = Visibility.Visible
                                      ProgressRing.IsActive = False
                                      ProgressRing.Visibility = Visibility.Collapsed
                                  End Sub)
            End Try
        End Sub


        Private Sub ExecuteStoredProcedure(commandText As String, parameterName As String, typeName As String, parameterValue As DataTable)
            Using Connection As New SqlConnection(My.Settings.AbcAnalysisConnectionString)
                Connection.Open()
                Using Command = Connection.CreateCommand()
                    Command.CommandTimeout = 1800
                    Command.CommandText = commandText
                    Command.CommandType = CommandType.StoredProcedure
                    Command.Parameters.Add(parameterName, SqlDbType.Structured).TypeName = typeName
                    Command.Parameters(parameterName).Value = parameterValue
                    Command.ExecuteReader()
                End Using
            End Using
        End Sub


        Private Function GetInnerException(ex As Exception) As String
            Dim Result As New StringBuilder
            Result.Append(ex.Message & vbCrLf & vbCrLf)
            If ex.InnerException IsNot Nothing Then
                Result.Append(GetInnerException(ex.InnerException))
            End If
            Return Result.ToString
        End Function

    End Class
End Namespace