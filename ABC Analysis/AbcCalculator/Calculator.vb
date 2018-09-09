Imports ABCAnalysis.Pages

Namespace AbcCalculator
    Public Class Calculator

#Region "Input"
        Public Property InitialDate As Date
        Public Property FinalDate As Date
        Public Property Templates As IEnumerable(Of Template)
        Public Property Data As IEnumerable(Of TaskData)
#End Region


        Public Sub Calculate()
            For Each tmp In Templates
                SetMasterData(tmp)
                RunIterations(tmp)
            Next
        End Sub


#Region "MasterData"
        Private Property Iterations As Integer
        Private Property StartDate As Date
        Private Property CalculationData As IEnumerable(Of DataItem)
        Private Property StatisticsData As IEnumerable(Of DataItem)
        Private Property CodeDict As Dictionary(Of Long, AbcClass())


        Private Sub SetMasterData(tmp As Template)
            Iterations = CInt(Fix((DateDiff("d", InitialDate, FinalDate) - tmp.BillingPeriod) / tmp.RunInterval))

            StartDate = FinalDate.AddDays(-((Iterations * tmp.RunInterval) + tmp.BillingPeriod))

            Iterations -= 1

            CalculationData = (From d In Data
                               Where tmp.Subinventories_id.Contains(d.Subinventory) AndAlso
                                   tmp.UserPositionTypes_id.Contains(d.UserPositionType_Id) AndAlso
                                   tmp.Categoryes_id.Contains(d.Category_Id) AndAlso
                                   tmp.IsSalesOrderFunc(d)
                               Select New DataItem With {.XDate = d.XDate, .Code = d.Code, .Value = tmp.GetValueFunc(d)}).ToList

            StatisticsData = (From d In Data
                              Where tmp.Subinventories_id.Contains(d.Subinventory)
                              Select New DataItem With {.XDate = d.XDate, .Code = d.Code, .Value = d.Tasks}).ToList

            CodeDict = CalculationData.Distinct(New DataItemComparer).ToDictionary(Function(d) d.Code, Function() New AbcClass(Iterations) {})
        End Sub
#End Region


#Region "IterationData"
        Private Property CurIter As Integer
        Private Property StartBillingPeriod As Date
        Private Property FinalBillingPeriod As Date
        Private Property StartInterval As Date
        Private Property FinalInterval As Date


        Private Sub RunIterations(tmp As Template)
            Dim TmpAbcTable As IEnumerable(Of AbcItem)
            Dim AbcTable1 As IEnumerable(Of AbcItem)
            Dim AbcTable2 As IEnumerable(Of AbcItem)

            For i = 0 To Iterations
                CurIter = i
                StartBillingPeriod = StartDate
                FinalBillingPeriod = StartDate.AddDays(tmp.BillingPeriod)
                StartDate = StartDate.AddDays(tmp.RunInterval)

                TmpAbcTable = (From d In CalculationData
                               Where d.XDate >= StartBillingPeriod AndAlso d.XDate <= FinalBillingPeriod
                               Group d By d.Code Into Sum = Sum(d.Value)
                               Order By Sum Descending, Code Ascending
                               Select New AbcItem With {.Code = Code, .Value = Sum}).ToList

                For j = 0 To TmpAbcTable.Count - 1
                    If j < tmp.QuantityAClass Then
                        TmpAbcTable(j).AbcClass = AbcClass.A
                        CodeDict(TmpAbcTable(j).Code)(CurIter) = AbcClass.A
                    Else
                        If j < tmp.QuantityABClass Then
                            TmpAbcTable(j).AbcClass = AbcClass.B
                            CodeDict(TmpAbcTable(j).Code)(CurIter) = AbcClass.B
                        Else
                            TmpAbcTable(j).AbcClass = AbcClass.C
                            CodeDict(TmpAbcTable(j).Code)(CurIter) = AbcClass.C
                        End If
                    End If
                Next

                If CurIter = 0 Then
                    AbcTable1 = TmpAbcTable
                    Continue For
                End If

                If CurIter Mod 2 = 0 Then
                    AbcTable1 = TmpAbcTable
                    CompareAbcTable(tmp, AbcTable2, AbcTable1)
                Else
                    AbcTable2 = TmpAbcTable
                    CompareAbcTable(tmp, AbcTable1, AbcTable2)
                End If

                StartInterval = FinalBillingPeriod.AddDays(1)
                FinalInterval = FinalBillingPeriod.AddDays(tmp.RunInterval)

            Next
        End Sub
#End Region


        Private Sub CompareAbcTable(tmp As Template, abcTable1 As IEnumerable(Of AbcItem), abcTable2 As IEnumerable(Of AbcItem))

        End Sub

    End Class
End Namespace