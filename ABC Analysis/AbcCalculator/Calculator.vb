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

            Dim x = (From cd In CodeDict
                     From ca In CurrentAbc.Where(Function(i) cd.Key = i.CodeItem).DefaultIfEmpty
                     Where cd.Value(CurIter) <> AbcClass.NA AndAlso (ca Is Nothing OrElse cd.Value(CurIter) <> ca.AbcClass_id)
                     Select CodeItem = cd.Key, AbcClass_id = CInt(cd.Value(CurIter)), AbcClass = cd.Value(CurIter).ToString).ToList

            Dim Table1 As New DataTable

            Table1.Columns.Add("CodeItem", GetType(Long))
            Table1.Columns.Add("AbcClass_id", GetType(Integer))

            For Each Item In x
                Table1.Rows.Add(Item.CodeItem, Item.AbcClass_id)
            Next
            ViewCollection("x", x)
        End Sub


        Public Overrides Sub RecordStatistics(abcTable As IEnumerable(Of AbcItem))
        End Sub

    End Class
End Namespace