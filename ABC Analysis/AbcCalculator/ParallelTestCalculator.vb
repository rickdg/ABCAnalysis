Imports ABCAnalysis.Pages
Imports ABCAnalysis.ExcelConnection
Imports System.Collections.Concurrent

Namespace AbcCalculator
    Public Class ParallelTestCalculator

#Region "Input"
        Public Property Temp As Template
        Public Property InitialDate As Date
        Public Property FinalDate As Date
        Public Property Data As IEnumerable(Of TaskData)
#End Region


        Public Sub Calculate()
            Dim CalculationData = (From d In Data
                                   Where Temp.UserPositionTypes_id.Contains(d.UserPositionType_Id) AndAlso
                                   Temp.Categoryes_id.Contains(d.Category_Id) AndAlso
                                   Temp.IsSalesOrderFunc(d)
                                   Select New DataItem With {.XDate = d.XDate, .Code = d.Code, .Value = Temp.GetValueFunc(d)}).ToList

            Parallel.ForEach(GetTemplates(False),
                             Sub(t)
                                 Dim Calculator As New TestCalculator With {
                                 .Temp = t,
                                 .InitialDate = InitialDate,
                                 .FinalDate = FinalDate,
                                 .Data = Data,
                                 .CalculationData = CalculationData}
                                 Calculator.Calculate()
                                 ResultCalculation.Add(New ResultCalculation(Calculator.Temp, Calculator.PickQtyTasks))
                             End Sub)

            ViewCollection("ResultCalculation", ResultCalculation)
        End Sub


        Private Property ResultCalculation As New BlockingCollection(Of ResultCalculation)


        Private Function GetTemplates(isPercent As Boolean) As IEnumerable(Of Template)
            Return (From RunInterval In {7, 14}
                    From BillingPeriod In {84}
                    From UpThresholdAB In {0}
                    From LowThresholdAB In {0}
                    From UpThresholdBC In {0}
                    From LowThresholdBC In {0}
                    Select New Template With {
                       .RunInterval = CByte(RunInterval),
                       .BillingPeriod = CByte(BillingPeriod),
                       .QuantityAClass = Temp.QuantityAClass,
                       .QuantityBClass = Temp.QuantityBClass,
                       .QuantityABClass = Temp.QuantityABClass,
                       .UpThresholdAB = New Threshold With {.IsUp = True, .IsPercent = isPercent, .Value = UpThresholdAB},
                       .LowThresholdAB = New Threshold With {.IsUp = False, .IsPercent = isPercent, .Value = LowThresholdAB},
                       .UpThresholdBC = New Threshold With {.IsUp = True, .IsPercent = isPercent, .Value = UpThresholdBC},
                       .LowThresholdBC = New Threshold With {.IsUp = False, .IsPercent = isPercent, .Value = LowThresholdBC}}).ToList
        End Function

    End Class
End Namespace