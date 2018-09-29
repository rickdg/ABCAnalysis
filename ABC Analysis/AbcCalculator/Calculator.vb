Imports ABCAnalysis.Pages
Imports ABCAnalysis.ExcelConnection

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

            CalculationData = (From d In Data
                               Where tmp.Subinventories_id.Contains(d.Subinventory) AndAlso
                                   tmp.UserPositionTypes_id.Contains(d.UserPositionType_Id) AndAlso
                                   tmp.Categoryes_id.Contains(d.Category_Id) AndAlso
                                   tmp.IsSalesOrderFunc(d)
                               Select New DataItem With {.XDate = d.XDate, .Code = d.Code, .Value = tmp.GetValueFunc(d)}).ToList

            StatisticsData = (From d In Data
                              Where tmp.Subinventories_id.Contains(d.Subinventory)
                              Select New DataItem With {.XDate = d.XDate, .Code = d.Code, .Value = d.Tasks}).ToList
            Iterations -= 1
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
            For i = 0 To Iterations
                CurIter = i
                StartBillingPeriod = StartDate
                FinalBillingPeriod = StartDate.AddDays(tmp.BillingPeriod)
                StartDate = StartDate.AddDays(tmp.RunInterval)

                Dim AbcTable = CreateAbcTable(tmp)

                If CurIter = 0 Then Continue For

                ThresholdAlgorithm(tmp, AbcTable)

                Equalization(tmp, AbcTable)

                StartInterval = FinalBillingPeriod.AddDays(1)
                FinalInterval = FinalBillingPeriod.AddDays(tmp.RunInterval)
            Next
        End Sub
#End Region


        Private Function CreateAbcTable(tmp As Template) As IEnumerable(Of AbcItem)
            Dim AbcTable = (From d In CalculationData
                            Where d.XDate >= StartBillingPeriod AndAlso d.XDate <= FinalBillingPeriod
                            Group d By d.Code Into Sum = Sum(d.Value)
                            Order By Sum Descending, Code Ascending
                            Select New AbcItem With {.Code = Code, .Value = Sum, .AbcClass = AbcClass.C}).ToList

            For j = 0 To AbcTable.Count - 1
                Dim Item = AbcTable(j)
                If j < tmp.QuantityAClass Then
                    Item.AbcClass = AbcClass.A
                    CodeDict(Item.Code)(CurIter) = AbcClass.A
                Else
                    If j < tmp.QuantityABClass Then
                        Item.AbcClass = AbcClass.B
                        CodeDict(Item.Code)(CurIter) = AbcClass.B
                    Else
                        CodeDict(Item.Code)(CurIter) = AbcClass.C
                    End If
                End If
            Next

            Return AbcTable
        End Function


        Private Sub ThresholdAlgorithm(tmp As Template, abcTable As IEnumerable(Of AbcItem))
            Dim ThresholdAB As Integer
            Dim ThresholdBC As Integer
            Dim UpThresholdAB As Integer
            Dim LowThresholdAB As Integer
            Dim UpThresholdBC As Integer
            Dim LowThresholdBC As Integer

            If tmp.QuantityAClass <= abcTable.Count - 1 Then
                ThresholdAB = abcTable(tmp.QuantityAClass).Value
                UpThresholdAB = tmp.UpThresholdAB.GetThreshold(ThresholdAB)
                LowThresholdAB = tmp.LowThresholdAB.GetThreshold(ThresholdAB)
            End If

            If tmp.QuantityABClass <= abcTable.Count - 1 Then
                ThresholdBC = abcTable(tmp.QuantityABClass).Value
                UpThresholdBC = tmp.UpThresholdBC.GetThreshold(ThresholdBC)
                LowThresholdBC = tmp.LowThresholdBC.GetThreshold(ThresholdBC)
            End If

            For i = 0 To abcTable.Count - 1
                Dim Item = abcTable(i)
                Dim PrevAbc = CodeDict(Item.Code)(CurIter - 1)
                Dim CurAbc = CodeDict(Item.Code)(CurIter)

                Select Case PrevAbc
                    Case AbcClass.A
                        Select Case CurAbc
                            Case AbcClass.B
                                If Item.Value >= LowThresholdAB Then
                                    Item.AbcClass = AbcClass.A
                                    CodeDict(Item.Code)(CurIter) = AbcClass.A
                                End If
                            Case AbcClass.C
                                If Item.Value >= LowThresholdBC Then
                                    If Item.Value >= LowThresholdAB Then
                                        Item.AbcClass = AbcClass.A
                                        CodeDict(Item.Code)(CurIter) = AbcClass.A
                                    Else
                                        Item.AbcClass = AbcClass.B
                                        CodeDict(Item.Code)(CurIter) = AbcClass.B
                                    End If
                                End If
                        End Select
                    Case AbcClass.B
                        Select Case CurAbc
                            Case AbcClass.C
                                If Item.Value >= LowThresholdBC Then
                                    Item.AbcClass = AbcClass.B
                                    CodeDict(Item.Code)(CurIter) = AbcClass.B
                                End If
                            Case AbcClass.A
                                If Item.Value <= UpThresholdAB Then
                                    Item.AbcClass = AbcClass.B
                                    CodeDict(Item.Code)(CurIter) = AbcClass.B
                                End If
                        End Select
                    Case AbcClass.C
                        Select Case CurAbc
                            Case AbcClass.B
                                If Item.Value <= UpThresholdBC Then
                                    Item.AbcClass = AbcClass.C
                                    CodeDict(Item.Code)(CurIter) = AbcClass.C
                                End If
                            Case AbcClass.A
                                If Item.Value <= UpThresholdAB Then
                                    If Item.Value <= UpThresholdBC Then
                                        Item.AbcClass = AbcClass.C
                                        CodeDict(Item.Code)(CurIter) = AbcClass.C
                                    Else
                                        Item.AbcClass = AbcClass.B
                                        CodeDict(Item.Code)(CurIter) = AbcClass.B
                                    End If
                                End If
                        End Select
                End Select
            Next
        End Sub


        Private Sub Equalization(tmp As Template, abcTable As IEnumerable(Of AbcItem))
            Dim QtyA = tmp.QuantityAClass
            Dim QtyB = tmp.QuantityBClass
            Dim CurQtyA = abcTable.Count(Function(Item) Item.AbcClass = AbcClass.A)
            If QtyA > CurQtyA Then
                For i = 0 To abcTable.Count - 1
                    If abcTable(i).AbcClass <> AbcClass.A Then
                        abcTable(i).AbcClass = AbcClass.A
                        CodeDict(abcTable(i).Code)(CurIter) = AbcClass.A
                        CurQtyA += 1
                        If QtyA = CurQtyA Then Exit For
                    End If
                Next
            Else
                If QtyA < CurQtyA Then
                    For i = abcTable.Count - 1 To 0 Step -1
                        If abcTable(i).AbcClass = AbcClass.A Then
                            If QtyB = 0 Then
                                abcTable(i).AbcClass = AbcClass.C
                                CodeDict(abcTable(i).Code)(CurIter) = AbcClass.C
                            Else
                                abcTable(i).AbcClass = AbcClass.B
                                CodeDict(abcTable(i).Code)(CurIter) = AbcClass.B
                            End If
                            CurQtyA -= 1
                            If QtyA = CurQtyA Then Exit For
                        End If
                    Next
                End If
            End If

            If QtyB = 0 Then Return
            Dim CurQtyB = abcTable.Count(Function(Item) Item.AbcClass = AbcClass.B)
            If QtyB > CurQtyB Then
                For i = 0 To abcTable.Count - 1
                    If abcTable(i).AbcClass = AbcClass.C Then
                        abcTable(i).AbcClass = AbcClass.B
                        CodeDict(abcTable(i).Code)(CurIter) = AbcClass.B
                        CurQtyB += 1
                        If QtyB = CurQtyB Then Exit For
                    End If
                Next
            Else
                If QtyB < CurQtyB Then
                    For i = abcTable.Count - 1 To 0 Step -1
                        If abcTable(i).AbcClass = AbcClass.B Then
                            abcTable(i).AbcClass = AbcClass.C
                            CodeDict(abcTable(i).Code)(CurIter) = AbcClass.C
                            CurQtyB -= 1
                            If QtyB = CurQtyB Then Exit For
                        End If
                    Next
                End If
            End If
        End Sub

    End Class
End Namespace