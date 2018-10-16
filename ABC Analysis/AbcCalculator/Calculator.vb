Imports System.Data
Imports System.Data.Entity.SqlServer
Imports System.Data.SqlClient
Imports System.Text
Imports ABCAnalysis.DataLoad

Namespace AbcCalculator
    Public Class Calculator
        Inherits BaseCalculator

        Public Overrides Sub Calculate()
            Dim CurrentAbc As IEnumerable(Of AbcCodeItem)

            Using Context As New AbcAnalysisEntities
                If Context.TaskDatas.FirstOrDefault Is Nothing Then Throw New Exception("Нет данных для расчета.")

                FinalDate = Context.TaskDatas.Where(Function(i) CBool(SqlFunctions.DatePart("Weekday", i.XDate) = 6)).Max(Function(i) i.XDate)
                Dim LastFriday = GetLastFriday()


                If LastFriday <> FinalDate Then Throw New Exception("")





                If Temp.IsCalculated Then

                Else
                    InitialDate = Context.TaskDatas.Min(Function(i) i.XDate)
                End If
                Data = Context.TaskDatas.Where(Function(i) i.XDate <= FinalDate AndAlso Temp.Subinventories_id.Contains(i.Subinventory)).ToList
                CurrentAbc = Context.AbcCodeItems.Where(Function(i) i.AbcGroup_id = Temp.AbcGroup_id).ToList
            End Using

            SetCalculationData()
            SetMasterData()
            RunIterations()

            Dim Result = (From cd In CodeDict
                          From ca In CurrentAbc.Where(Function(i) cd.Key = i.CodeItem).DefaultIfEmpty
                          Where cd.Value(CurIter) <> AbcClass.NA AndAlso (ca Is Nothing OrElse cd.Value(CurIter) <> ca.AbcClass_id)
                          Select CodeItem = cd.Key, AbcClass_id = CInt(cd.Value(CurIter)), AbcClass = cd.Value(CurIter).ToString, IsNew = ca Is Nothing).ToList

            Dim DlData As New StringBuilder
            Dim Table = GetDataTable()
            For Each Item In Result
                If Item.IsNew Then
                    DlData.AppendLine(AddAbc(Item.CodeItem, Item.AbcClass))
                Else
                    DlData.AppendLine(ChangeAbc(Item.CodeItem, Item.AbcClass))
                End If
                Table.Rows.Add(Item.CodeItem, Item.AbcClass_id)
            Next
            Dim NewFile = GetInBaseFileInfo(GetInBaseDirectoryInfo("DataLoad"), $"{Temp.Name}.dld")
            Dim DLW As New DLWriter(NewFile.FullName) With {.Description = $"АВС {Temp.Name}"}
            DLW.Write(DlData.ToString)
            Process.Start(NewFile.FullName)
            UpdateAbc(Table)

            Temp.IsCalculated = True
        End Sub


        Private Function GetLastFriday() As Date
            Dim StartDate = Today.AddDays(-7)
            While StartDate <= Today
                If Weekday(StartDate, FirstDayOfWeek.Monday) = 5 Then Return StartDate
                StartDate = StartDate.AddDays(1)
            End While
        End Function


        Private Function GetDataTable() As DataTable
            Dim Table As New DataTable
            Table.Columns.Add("CodeItem", GetType(Long))
            Table.Columns.Add("AbcClass_id", GetType(Integer))
            Return Table
        End Function


        Private Sub UpdateAbc(table As DataTable)
            Using Connection As New SqlConnection(My.Settings.AbcAnalysisConnectionString)
                Connection.Open()
                Using Command = Connection.CreateCommand()
                    Command.CommandTimeout = 1800
                    Command.CommandText = "dbo.UpdateAbc"
                    Command.CommandType = CommandType.StoredProcedure
                    Command.Parameters.Add("@AbcGroup_id", SqlDbType.Int).Value = Temp.AbcGroup_id
                    Command.Parameters.Add("@Table", SqlDbType.Structured).TypeName = "AbcTable"
                    Command.Parameters("@Table").Value = table
                    Command.ExecuteReader()
                End Using
            End Using
        End Sub


        Public Overrides Sub RecordStatistics(abcTable As IEnumerable(Of AbcItem))
        End Sub

    End Class
End Namespace