﻿Namespace AbcCalculator
    Public Class TestCalculator
        Inherits BaseCalculator

        Public Overrides Sub Calculate()
            SetMasterData()
            RunIterations()
        End Sub


        Public Property PickQtyTasks As New List(Of AbcValue)
        Public Property GenericClassChange As New List(Of GenericTransition)


        Public Overrides Sub RecordStatistics(abcTable As IEnumerable(Of AbcItem))
            Dim StartInterval = FinalBillingPeriod.AddDays(1)
            Dim FinalInterval = FinalBillingPeriod.AddDays(Temp.RunInterval)

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

            PickQtyTasks.Add(New AbcValue(FinalInterval, Pick, Function(i) i.QtyTasks))
            GenericClassChange.Add(New GenericTransition(FinalBillingPeriod, ClassChange))
        End Sub

    End Class
End Namespace