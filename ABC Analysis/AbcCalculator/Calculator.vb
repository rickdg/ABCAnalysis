Imports ABCAnalysis.Pages

Namespace AbcCalculator
    Public Class Calculator

#Region "Input"
        Public Property InitialDate As Date
        Public Property FinalDate As Date
        Public Property Templates As IEnumerable(Of Template)
        Public Property Data As IEnumerable(Of TaskData)
#End Region


#Region "Data"
        Private Property Iterations As Integer
        Private Property StartDate As Date
        Private Property CalculationData As IEnumerable(Of DataItem)
        Private Property StatisticsData As IEnumerable(Of DataItem)
        Private Property CodeAbcDict As Dictionary(Of Long, Byte())


        Private Sub SetData(tmp As Template)
            Iterations = CInt(Fix((DateDiff("d", InitialDate, FinalDate) - tmp.BillingPeriod) / tmp.RunInterval))

            StartDate = FinalDate.AddDays(-((Iterations * tmp.RunInterval) + tmp.BillingPeriod))

            CalculationData = (From d In Data
                               Where tmp.Subinventories_id.Contains(d.Subinventory) AndAlso
                                   tmp.UserPositionTypes_id.Contains(d.UserPositionType_Id) AndAlso
                                   tmp.Categoryes_id.Contains(d.Category_Id) AndAlso
                                   tmp.IsSalesOrderFunc(d)
                               Select New DataItem With {.XDate = d.XDate, .Code = d.Code, .Value = tmp.GetValueFunc(d)}).ToList

            StatisticsData = (From d In Data
                              Where tmp.Subinventories_id.Contains(d.Subinventory)
                              Select New DataItem With {.XDate = d.XDate, .Code = d.Code, .Value = d.Tasks}).ToList

            CodeAbcDict = CalculationData.Distinct(New DataItemComparer).ToDictionary(Function(d) d.Code, Function() New Byte(Iterations) {})
        End Sub
#End Region





        'Private Property StartBillingPeriodDate As Date
        'Private Property FinalBillingPeriodDate As Date


        'Private Property AbcTable As List(Of AbcTableItem)
        'Private Property StartIntervalDate As Date
        'Private Property FinalIntervalDate As Date


        Public Sub Calculate()
            For Each Tmp In Templates
                SetData(Tmp)




            Next
        End Sub




    End Class
End Namespace