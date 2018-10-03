Namespace AbcCalculator
    Public Class TestCalculator
        Inherits BaseCalculator

        Public Overrides Sub Calculate()
            SetMasterData()
            RunIterations()
        End Sub


        Public Property PickQtyTasks As New List(Of AbcValue)
        Public Property ClassChange As New List(Of Transition)
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

            PickQtyTasks.Add(New AbcValue(FinalInterval, Pick, Function(i) i.QtyTasks))
        End Sub

    End Class
End Namespace