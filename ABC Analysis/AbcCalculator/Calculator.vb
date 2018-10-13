Imports System.Data
Imports System.Data.Entity.SqlServer
Imports ABCAnalysis.ExcelConnection

Namespace AbcCalculator
    Public Class Calculator
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

            'ViewCollection("PickQtyCode", PickQtyCode)
            'ViewCollection("PickQtyTasks", PickQtyTasks)

            Dim x = (From ca In CurrentAbc
                     From cd In CodeDict
                     Where cd.Value(CurIter) <> AbcClass.NA AndAlso cd.Key = ca.CodeItem AndAlso cd.Value(CurIter) <> ca.AbcClass_id
                     Select CodeItem = cd.Key, AbcClass_id = CInt(cd.Value(CurIter))).ToList

            Dim Table1 As New DataTable

            Table1.Columns.Add("CodeItem", GetType(Long))
            Table1.Columns.Add("AbcClass_id", GetType(Integer))

            For Each Item In x
                Table1.Rows.Add(Item.CodeItem, Item.AbcClass_id)
            Next
            Stop
        End Sub


        Private Property AbcQtyCode As New List(Of AbcValue)
        Private Property AbcQtyTasks As New List(Of AbcValue)
        Private Property PickQtyCode As New List(Of AbcValue)
        Private Property PickQtyTasks As New List(Of AbcValue)
        Private Property ClassChange As New List(Of Transition)
        Private Property GenericClassChange As New List(Of GenericTransition)


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

            AbcQtyCode.Add(New AbcValue(FinalBillingPeriod, Abc, Function(i) i.QtyCode))
            AbcQtyTasks.Add(New AbcValue(FinalBillingPeriod, Abc, Function(i) i.QtyTasks))
            PickQtyCode.Add(New AbcValue(FinalInterval, Pick, Function(i) i.QtyCode))
            PickQtyTasks.Add(New AbcValue(FinalInterval, Pick, Function(i) i.QtyTasks))
            Me.ClassChange.Add(New Transition(FinalBillingPeriod, ClassChange))
            GenericClassChange.Add(New GenericTransition(FinalBillingPeriod, ClassChange))
        End Sub

    End Class
End Namespace