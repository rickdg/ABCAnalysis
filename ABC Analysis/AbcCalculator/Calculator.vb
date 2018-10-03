Imports ABCAnalysis.ExcelConnection

Namespace AbcCalculator
    Public Class Calculator
        Inherits BaseCalculator

        Public Overrides Sub Calculate()
            SetCalculationData()
            SetMasterData()
            RunIterations()

            ViewCollection("PickQtyCode", PickQtyCode)
            ViewCollection("PickQtyTasks", PickQtyTasks)
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