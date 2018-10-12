Imports ABCAnalysis.Pages
Imports ABCAnalysis.ExcelConnection
Imports System.Collections.Concurrent

Namespace AbcCalculator
    Public Class Heuristics

#Region "Input"
        Public Property Temp As Template
        Public Property InitialDate As Date
        Public Property FinalDate As Date
        Public Property Data As IEnumerable(Of TaskData)

#End Region


        Public Sub Calculate()
            Dim CalculationData = (From d In Data
                                   Where Temp.UserPositionTypes_id.Contains(d.UserPositionType_Id) AndAlso
                                       Temp.Categoryes_id.Contains(d.Category_Id) AndAlso d.SalesOrder
                                   Select New DataItem With {.XDate = d.XDate, .Code = d.Code, .Value = d.Orders}).ToList

            Parallel.ForEach(GetTemplates,
                             Sub(t)
                                 Dim Calculator As New TestCalculator With {
                                 .Temp = t,
                                 .InitialDate = InitialDate,
                                 .FinalDate = FinalDate,
                                 .Data = Data,
                                 .CalculationData = CalculationData}

                                 Calculator.Calculate()
                                 ResultCalculation.Add(New ResultCalculation(Calculator.Temp, Calculator.PickQtyTasks, Calculator.ClassChange))
                             End Sub)

            'ViewCollection("ResultCalculation", ResultCalculation)

            Dim TargetPickPercent = ResultCalculation.Max(Function(j) j.AvgPickPercent) - Temp.ReductionPickPercent.Value2
            Dim Result = (From Rc In ResultCalculation
                          Where Rc.AvgPickPercent >= TargetPickPercent
                          Order By Rc.Transition Ascending, Rc.AvgPickPercent Descending).First
            Temp.RunInterval = Result.Interval
            Temp.BillingPeriod = Result.Period
            Temp.AvgPickPercent = Result.AvgPickPercent
            Temp.Transition = Result.Transition
        End Sub


        Private Property ResultCalculation As New BlockingCollection(Of ResultCalculation)


        Private Function GetTemplates() As IEnumerable(Of Template)
            Return (From RunInterval In {7, 14, 21, 28, 35, 42}
                    From BillingPeriod In {21, 28, 35, 42, 49, 56, 63, 70, 77, 84, 91, 98, 105, 112, 119, 126, 133, 140, 147, 154, 161, 168, 175, 182, 189, 196}
                    Select New Template With {
                       .RunInterval = CByte(RunInterval),
                       .BillingPeriod = CByte(BillingPeriod),
                       .QuantityAClass = Temp.QuantityAClass,
                       .QuantityBClass = Temp.QuantityBClass,
                       .QuantityABClass = Temp.QuantityABClass}).ToList
        End Function

    End Class
End Namespace