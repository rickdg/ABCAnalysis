Imports ABCAnalysis.Pages

Namespace AbcCalculator
    Public MustInherit Class BaseCalculator

        Public MustOverride Sub Calculate()


#Region "Input"
        Public Property Temp As Template
        Public Property InitialDate As Date
        Public Property FinalDate As Date
        Public Property Data As IEnumerable(Of TaskData)
#End Region


#Region "MasterData"
        Private Property Iterations As Integer
        Private Property StartDate As Date
        Public Property CalculationData As IEnumerable(Of DataItem)
        Public Property CodeDict As Dictionary(Of Long, AbcClass())


        Public Sub SetMasterData()
            Iterations = CInt(Fix((DateDiff("d", InitialDate, FinalDate) - Temp.BillingPeriod) / Temp.RunInterval))
            StartDate = FinalDate.AddDays(-((Iterations * Temp.RunInterval) + Temp.BillingPeriod))
            Iterations -= 1
            CodeDict = Data.Distinct(New TaskDataComparer).ToDictionary(Function(d) d.Code, Function() New AbcClass(Iterations) {})
        End Sub


        Public Sub SetCalculationData()
            CalculationData = (From d In Data
                               Where Temp.UserPositionTypes_id.Contains(d.UserPositionType_Id) AndAlso
                                   Temp.Categoryes_id.Contains(d.Category_Id) AndAlso
                                   Temp.IsSalesOrderFunc(d)
                               Select New DataItem With {.XDate = d.XDate, .Code = d.Code, .Value = Temp.GetValueFunc(d)}).ToList
        End Sub
#End Region


#Region "IterationData"
        Public Property CurIter As Integer
        Private Property StartBillingPeriod As Date
        Public Property FinalBillingPeriod As Date


        Public Sub RunIterations()
            Dim PrevAbcTable1 As IEnumerable(Of AbcItem)
            Dim PrevAbcTable2 As IEnumerable(Of AbcItem)

            For i = 0 To Iterations
                CurIter = i
                StartBillingPeriod = StartDate
                FinalBillingPeriod = StartDate.AddDays(Temp.BillingPeriod)
                StartDate = StartDate.AddDays(Temp.RunInterval)

                Dim AbcTable = GetAbcTable()

                If CurIter = 0 Then
                    PrevAbcTable1 = AbcTable
                    Continue For
                End If

                If CurIter Mod 2 = 0 Then
                    PrevAbcTable1 = AbcTable
                    TransitionToX(PrevAbcTable2)
                Else
                    PrevAbcTable2 = AbcTable
                    TransitionToX(PrevAbcTable1)
                End If

                ThresholdAlgorithm(AbcTable)
                Equalization(AbcTable)

                RecordStatistics(AbcTable)
            Next
        End Sub


        Public MustOverride Sub RecordStatistics(abcTable As IEnumerable(Of AbcItem))
#End Region


#Region "Methods"
        Private Function GetAbcTable() As IEnumerable(Of AbcItem)
            Dim AbcTable = (From d In CalculationData
                            Where d.XDate >= StartBillingPeriod AndAlso d.XDate <= FinalBillingPeriod
                            Group By d.Code Into Sum = Sum(d.Value)
                            Order By Sum Descending, Code Ascending
                            Select New AbcItem With {.Code = Code, .Value = Sum, .AbcClass = AbcClass.C}).ToList

            For j = 0 To AbcTable.Count - 1
                Dim Item = AbcTable(j)
                If j < Temp.QuantityAClass Then
                    Item.AbcClass = AbcClass.A
                    CodeDict(Item.Code)(CurIter) = AbcClass.A
                Else
                    If j < Temp.QuantityABClass Then
                        Item.AbcClass = AbcClass.B
                        CodeDict(Item.Code)(CurIter) = AbcClass.B
                    Else
                        CodeDict(Item.Code)(CurIter) = AbcClass.C
                    End If
                End If
            Next

            Return AbcTable
        End Function
#End Region


#Region "Algorithms"
        Private Property UpThresholdAB As Integer
        Private Property LowThresholdAB As Integer
        Private Property UpThresholdBC As Integer
        Private Property LowThresholdBC As Integer


        Private Sub TransitionToX(prevAbcTable As IEnumerable(Of AbcItem))
            For Each Item In prevAbcTable
                If CodeDict(Item.Code)(CurIter) = AbcClass.NA Then
                    CodeDict(Item.Code)(CurIter) = AbcClass.X
                End If
            Next

            For Each Item In CodeDict
                If Item.Value(CurIter - 1) = AbcClass.X AndAlso Item.Value(CurIter) = AbcClass.NA Then
                    Item.Value(CurIter) = AbcClass.X
                End If
            Next
        End Sub


        Private Sub ThresholdAlgorithm(abcTable As IEnumerable(Of AbcItem))
            If Temp.QuantityAClass <= abcTable.Count - 1 Then
                Dim ThresholdAB = abcTable(Temp.QuantityAClass).Value
                UpThresholdAB = Temp.UpThresholdAB.GetThreshold(ThresholdAB)
                LowThresholdAB = Temp.LowThresholdAB.GetThreshold(ThresholdAB)
            End If

            If Temp.QuantityABClass <= abcTable.Count - 1 Then
                Dim ThresholdBC = abcTable(Temp.QuantityABClass).Value
                UpThresholdBC = Temp.UpThresholdBC.GetThreshold(ThresholdBC)
                LowThresholdBC = Temp.LowThresholdBC.GetThreshold(ThresholdBC)
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


        Private Sub Equalization(abcTable As IEnumerable(Of AbcItem))
            Dim QtyA = Temp.QuantityAClass
            Dim QtyB = Temp.QuantityBClass
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
                            If QtyB = CurQtyB Then Return
                        End If
                    Next
                End If
            End If
        End Sub
#End Region

    End Class
End Namespace