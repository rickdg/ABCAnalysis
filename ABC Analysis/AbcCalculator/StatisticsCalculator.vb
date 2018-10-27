Imports System.Data
Imports System.Data.Entity.SqlServer
Imports ABCAnalysis.Pages
Imports OfficeOpenXml

Namespace AbcCalculator
    Public Class StatisticsCalculator
        Inherits BaseCalculator

        Public Overrides Sub Calculate()
            Using Context = DatabaseManager.CurrentDatabase.Context
                If Context.TaskDatas.FirstOrDefault Is Nothing Then Throw New Exception("Нет данных для расчета.")
                InitialDate = Context.TaskDatas.Min(Function(i) i.XDate)
                FinalDate = Context.TaskDatas.Where(Function(i) CBool(SqlFunctions.DatePart("Weekday", i.XDate) = 6)).Max(Function(i) i.XDate)
                Data = (From td In Context.TaskDatas
                        Join ci In Context.CodeItems On ci.Id Equals td.CodeItem_id
                        Where td.XDate <= FinalDate AndAlso Temp.Subinventories_id.Contains(td.Subinventory)
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
            RunIterations()
            CreateReport()
        End Sub


        Private Property AbcQtyCode As New List(Of AbcQtyCode)
        Private Property AbcQtyTasks As New List(Of AbcQtyTasks)
        Private Property AbcPercentQtyCode As New List(Of AbcPercent)
        Private Property AbcPercentQtyTasks As New List(Of AbcPercent)
        Private Property PickQtyCode As New List(Of PickValue)
        Private Property PickQtyTasks As New List(Of PickValue)
        Private Property PickPercentQtyCode As New List(Of PickPercent)
        Private Property PickPercentQtyTasks As New List(Of PickPercent)
        Private Property ClassChange As New List(Of Transition)
        Private Property Worksheets As ExcelWorksheets


        Public Overrides Sub RecordStatistics(abcTable As IEnumerable(Of AbcItem))
            Dim StartInterval = FinalBillingPeriod.AddDays(1)
            Dim FinalInterval = FinalBillingPeriod.AddDays(Temp.RunInterval)

            Dim Abc = (From at In abcTable
                       Group By at.AbcClass Into Count, Sum(at.Value)
                       Select New StatItem With {.AbcClass = AbcClass, .QtyCode = Count, .QtyTasks = Sum}).
                       Concat(From cd In CodeDict
                              Where cd.Value(CurIter) = AbcClass.X
                              Group By AbcClass = cd.Value(CurIter) Into Count
                              Select New StatItem With {.AbcClass = AbcClass, .QtyCode = Count, .QtyTasks = 0}).ToList

            Dim Pick = (From td In (From d In Data
                                    Join cd In CodeDict On cd.Key Equals d.Code
                                    Where d.XDate >= StartInterval AndAlso d.XDate <= FinalInterval
                                    Group By d.Code, AbcClass = cd.Value(CurIter) Into Sum(d.Tasks))
                        Group By td.AbcClass Into Count, Sum(td.Sum)
                        Select New StatItem With {.AbcClass = AbcClass, .QtyCode = Count, .QtyTasks = Sum}).ToList

            Dim ClassChange = (From cd In CodeDict
                               Where cd.Value(CurIter) <> AbcClass.NA
                               Group By prev = cd.Value(CurIter - 1), cur = cd.Value(CurIter) Into Count
                               Select New DirectionItem(prev, cur) With {.QtyCode = Count}).ToList

            AbcQtyCode.Add(New AbcQtyCode(FinalBillingPeriod, Abc))
            AbcQtyTasks.Add(New AbcQtyTasks(FinalBillingPeriod, Abc))
            AbcPercentQtyCode.Add(New AbcPercent(FinalBillingPeriod, Abc, Function(i) i.QtyCode))
            AbcPercentQtyTasks.Add(New AbcPercent(FinalBillingPeriod, Abc, Function(i) i.QtyTasks))
            Me.ClassChange.Add(New Transition(FinalBillingPeriod, ClassChange))

            If CurIter = Iterations Then Return
            PickQtyCode.Add(New PickValue(FinalInterval, Pick, Function(i) i.QtyCode))
            PickQtyTasks.Add(New PickValue(FinalInterval, Pick, Function(i) i.QtyTasks))
            PickPercentQtyCode.Add(New PickPercent(FinalInterval, Pick, Function(i) i.QtyCode))
            PickPercentQtyTasks.Add(New PickPercent(FinalInterval, Pick, Function(i) i.QtyTasks))
        End Sub


        Private Sub CreateReport()
            Dim NewFile = GetInBaseFileInfo(GetInBaseDirectoryInfo("Reports"), "Статистика.xlsx")
            Using Package As New ExcelPackage(NewFile)
                Worksheets = Package.Workbook.Worksheets
                With AddWorksheet("АВС таблица")
                    .AddLineChart(AbcQtyCode, "Кол-во позиций")
                    .AddLineChart(AbcQtyTasks, "Кол-во задач", endChartLine:=True)
                    .AddLineChart(AbcPercentQtyCode, "Процент позиций")
                    .AddLineChart(AbcPercentQtyTasks, "Процент задач")
                End With
                With AddWorksheet("Отбор")
                    .AddLineChart(PickQtyCode, "Кол-во позиций")
                    .AddLineChart(PickQtyTasks, "Кол-во задач", endChartLine:=True)
                    .AddLineChart(PickPercentQtyCode, "Процент позиций")
                    .AddLineChart(PickPercentQtyTasks, "Процент задач")
                End With
                AddWorksheet("Изменения классов").AddLineChart(ClassChange, "")
                Package.Save()
            End Using
            Process.Start(NewFile.FullName)
        End Sub


        Private Function AddWorksheet(name As String) As WorksheetHelper
            Return New WorksheetHelper With {.Sheet = Worksheets.Add(name)}
        End Function

    End Class
End Namespace