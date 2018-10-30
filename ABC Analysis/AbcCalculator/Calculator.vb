Imports System.Data
Imports System.Text
Imports ABCAnalysis.Connection.SqlServer
Imports ABCAnalysis.DataLoad
Imports ABCAnalysis.Pages
Imports FirstFloor.ModernUI.Windows.Controls
Imports ABCAnalysis.Connection.Excel

Namespace AbcCalculator
    Public Class Calculator
        Inherits BaseCalculator

        Private Property CurrentAbc As IEnumerable(Of AbcCodeItem)


        Public Overrides Sub Calculate()
            Using Context = MainPageModel.CurrentProject.Context
                If Context.TaskDatas.FirstOrDefault Is Nothing Then Throw New Exception("Нет данных для расчета.")

                FinalDate = GetLastFriday()
                Dim TempDate = FinalDate.AddDays(-4)
                Dim CheckTasks = Context.TaskDatas.Where(Function(i) i.XDate >= TempDate AndAlso i.XDate <= FinalDate).ToList.Count
                If CheckTasks = 0 Then Throw New Exception("Недостаточно данных для расчета.")

                CurrentAbc = (From abc In Context.AbcCodeItems
                              Join ci In Context.CodeItems On abc.CodeItem Equals ci.Code
                              Where abc.AbcGroup_id = Temp.AbcGroup_id And Temp.UserPositionTypes_id.Contains(ci.UserPositionType_Id)
                              Select abc).ToList

                If Temp.IsCalculated Then
                    If Temp.FinalDate < FinalDate Then
                        If DateDiff("d", Temp.FinalDate, FinalDate) Mod Temp.RunInterval = 0 Then

                            InitialDate = Temp.FinalDate.AddDays(-Temp.BillingPeriodForCalculate)
                            Data = (From td In Context.TaskDatas
                                    Join ci In Context.CodeItems On ci.Id Equals td.CodeItem_id
                                    Where td.XDate >= InitialDate AndAlso td.XDate <= FinalDate AndAlso Temp.Subinventories_id.Contains(td.Subinventory)
                                    Select New TaskDataExtend With {
                                        .XDate = td.XDate,
                                        .Code = td.Code,
                                        .Category_Id = ci.Category_Id,
                                        .UserPositionType_Id = ci.UserPositionType_Id,
                                        .SalesOrder = td.SalesOrder,
                                        .Orders = td.Orders,
                                        .Tasks = td.Tasks}).ToList

                            AlternativeCalculate()
                            Return
                        Else
                            If Not IsResumeDialog("Текущая дата не совпадает с расчетной, начать новый расчет?") Then Return
                        End If
                    Else
                        If Not IsResumeDialog("АВС уже был рассчитан, начать новый расчет?") Then Return
                    End If
                End If

                InitialDate = Context.TaskDatas.Min(Function(i) i.XDate)
                Data = (From td In Context.TaskDatas
                        Join ci In Context.CodeItems On ci.Id Equals td.CodeItem_id
                        Where td.XDate >= InitialDate AndAlso td.XDate <= FinalDate AndAlso Temp.Subinventories_id.Contains(td.Subinventory)
                        Select New TaskDataExtend With {
                            .XDate = td.XDate,
                            .Code = td.Code,
                            .Category_Id = ci.Category_Id,
                            .UserPositionType_Id = ci.UserPositionType_Id,
                            .SalesOrder = td.SalesOrder,
                            .Orders = td.Orders,
                            .Tasks = td.Tasks}).ToList
            End Using

            SetCalculationData()
            SetMasterData()
            MyBase.RunIterations()
            CalculateResult()

            Temp.FinalDate = FinalBillingPeriod
            Temp.IsCalculated = True
        End Sub


        Private Sub AlternativeCalculate()
            SetCalculationData()
            SetMasterData()
            RunIterations()
            CalculateResult()

            Temp.FinalDate = FinalBillingPeriod
        End Sub


        Private Overloads Sub RunIterations()
            Dim PrevAbcTable1 As IEnumerable(Of AbcItem)
            Dim PrevAbcTable2 As IEnumerable(Of AbcItem)

            For i = 0 To Iterations
                CurIter = i
                StartBillingPeriod = StartDate
                FinalBillingPeriod = StartDate.AddDays(Temp.BillingPeriodForCalculate)
                StartDate = StartDate.AddDays(Temp.RunInterval)

                Dim AbcTable = GetAbcTable()

                If CurIter = 0 Then
                    RestoreABC(AbcTable)
                    PrevAbcTable1 = AbcTable
                    Continue For
                End If

                If CurIter Mod 2 = 0 Then
                    PrevAbcTable1 = AbcTable
                    TransitionToX(PrevAbcTable2)
                Else
                    PrevAbcTable2 = AbcTable
                    TransitionToX(PrevAbcTable1)
                End If

                Equalization(AbcTable)
            Next
        End Sub


        Private Sub RestoreABC(abcTable As IEnumerable(Of AbcItem))
            For Each AbcItem In abcTable
                Dim AbcCodeItem = CurrentAbc.SingleOrDefault(Function(i) i.CodeItem = AbcItem.Code)
                If AbcCodeItem IsNot Nothing Then
                    AbcItem.AbcClass = CType([Enum].ToObject(GetType(AbcClass), AbcCodeItem.AbcClass_id), AbcClass)
                    CodeDict(AbcItem.Code)(CurIter) = AbcItem.AbcClass
                End If
            Next

            For Each Item In CodeDict
                If Item.Value(CurIter) = AbcClass.NA Then
                    Dim AbcCodeItem = CurrentAbc.SingleOrDefault(Function(i) i.CodeItem = Item.Key)
                    If AbcCodeItem IsNot Nothing Then
                        Item.Value(CurIter) = CType([Enum].ToObject(GetType(AbcClass), AbcCodeItem.AbcClass_id), AbcClass)
                    End If
                End If
            Next
        End Sub


        Private Sub CalculateResult()
            ViewCollection("ABC", CodeDict.Select(Function(i) Tuple.Create(i.Key, i.Value(CurIter).ToString)))

            Dim ResultData = (From cd In CodeDict
                              From ca In CurrentAbc.Where(Function(i) cd.Key = i.CodeItem).DefaultIfEmpty
                              Where cd.Value(CurIter) <> AbcClass.NA AndAlso (ca Is Nothing OrElse cd.Value(CurIter) <> ca.AbcClass_id)
                              Select CodeItem = cd.Key, AbcClass_id = CInt(cd.Value(CurIter)), AbcClass = cd.Value(CurIter).ToString, IsNew = ca Is Nothing).
                              Concat(From ca In CurrentAbc
                                     From cd In CodeDict.Where(Function(i) i.Key = ca.CodeItem).DefaultIfEmpty
                                     Where ca.AbcClass_id <> 4 AndAlso (cd.Key = 0 OrElse cd.Value(CurIter) = AbcClass.NA)
                                     Select CodeItem = ca.CodeItem, AbcClass_id = 4, AbcClass = "X", IsNew = False).ToList
            If ResultData.Count = 0 Then Return

            Dim DlData As New StringBuilder
            Dim Table = GetDataTable()

            For Each Item In ResultData
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

            MainPageModel.StoredProcedureExecute({New CommandParameter With {
                                                 .Name = "@AbcGroup_id",
                                                 .SqlDbType = SqlDbType.Int,
                                                 .Value = Temp.AbcGroup_id}},
                                                 New StoredProcedureParameter With {
                                                 .CommandText = "dbo.UpdateAbc",
                                                 .ParameterName = "@Table",
                                                 .TypeName = "AbcTable"}, Table)
        End Sub


        Private Function GetLastFriday() As Date
            Dim StartDate = Today.AddDays(-7)
            While StartDate <= Today
                If Weekday(StartDate, FirstDayOfWeek.Monday) = DayOfWeek.Friday Then Return StartDate
                StartDate = StartDate.AddDays(1)
            End While
        End Function


        Private Function GetDataTable() As DataTable
            Dim Table As New DataTable
            Table.Columns.Add("CodeItem", GetType(Long))
            Table.Columns.Add("AbcClass_id", GetType(Integer))
            Return Table
        End Function


        Private Function IsResumeDialog(content As String) As Boolean?
            Dim Result As Boolean?
            Application.Current.Dispatcher.Invoke(Sub()
                                                      Dim Dlg As New ModernDialog With {
                                                      .Title = "Сообщение",
                                                      .Content = content}
                                                      Dlg.Buttons = {Dlg.YesButton, Dlg.NoButton}
                                                      Result = Dlg.ShowDialog()
                                                  End Sub)
            Return Result
        End Function


        Public Overrides Sub RecordStatistics(abcTable As IEnumerable(Of AbcItem))
        End Sub

    End Class
End Namespace