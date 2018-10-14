Imports System.Data
Imports System.Data.Entity.SqlServer
Imports OfficeOpenXml

Namespace AbcCalculator
    Public Class StatisticsCalculator
        Inherits BaseCalculator

        Public Overrides Sub Calculate()
            Dim CurrentAbc As IEnumerable(Of AbcCodeItem)
            Using Context As New AbcAnalysisEntities
                If Context.TaskDatas.FirstOrDefault Is Nothing Then Return
                InitialDate = Context.TaskDatas.Min(Function(i) i.XDate)
                FinalDate = Context.TaskDatas.Where(Function(i) CBool(SqlFunctions.DatePart("Weekday", i.XDate) = 6)).Max(Function(i) i.XDate)
                Data = Context.TaskDatas.Where(Function(i) i.XDate <= FinalDate AndAlso Temp.Subinventories_id.Contains(i.Subinventory)).ToList
                CurrentAbc = Context.AbcCodeItems.Where(Function(i) i.AbcGroup_id = Temp.AbcGroup_id).ToList
            End Using

            CalculationData = (From d In Data
                               Where Temp.UserPositionTypes_id.Contains(d.UserPositionType_Id) AndAlso
                                   Temp.Categoryes_id.Contains(d.Category_Id) AndAlso d.SalesOrder
                               Select New DataItem With {.XDate = d.XDate, .Code = d.Code, .Value = d.Orders}).ToList
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


        Public Property Worksheets As ExcelWorksheets


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
            PickQtyCode.Add(New PickValue(FinalInterval, Pick, Function(i) i.QtyCode))
            PickQtyTasks.Add(New PickValue(FinalInterval, Pick, Function(i) i.QtyTasks))
            PickPercentQtyCode.Add(New PickPercent(FinalInterval, Pick, Function(i) i.QtyCode))
            PickPercentQtyTasks.Add(New PickPercent(FinalInterval, Pick, Function(i) i.QtyTasks))
            Me.ClassChange.Add(New Transition(FinalBillingPeriod, ClassChange))
        End Sub


        Public Sub CreateReport()
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


        Public Function AddWorksheet(name As String) As WorksheetHelper
            Return New WorksheetHelper With {.Sheet = Worksheets.Add(name)}
        End Function

    End Class
End Namespace