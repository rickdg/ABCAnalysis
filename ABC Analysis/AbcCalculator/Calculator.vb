Imports ABCAnalysis.Pages
Imports ABCAnalysis.ExcelConnection

Namespace AbcCalculator
    Public Class Calculator

#Region "Input"
        Public Property Temp As Template
        Public Property InitialDate As Date
        Public Property FinalDate As Date
        Public Property Data As IEnumerable(Of TaskData)
#End Region


        Public Sub Calculate()
            SetMasterData()
            RunIterations()

            ViewCollection("ClassChangeList", ClassChangeList)
        End Sub


#Region "MasterData"
        Private Property Iterations As Integer
        Private Property StartDate As Date
        Private Property CalculationData As IEnumerable(Of DataItem)
        Private Property CodeDict As Dictionary(Of Long, AbcClass())


        Private Sub SetMasterData()
            Iterations = CInt(Fix((DateDiff("d", InitialDate, FinalDate) - Temp.BillingPeriod) / Temp.RunInterval))

            StartDate = FinalDate.AddDays(-((Iterations * Temp.RunInterval) + Temp.BillingPeriod))

            CalculationData = (From d In Data
                               Where Temp.UserPositionTypes_id.Contains(d.UserPositionType_Id) AndAlso
                                   Temp.Categoryes_id.Contains(d.Category_Id) AndAlso
                                   Temp.IsSalesOrderFunc(d)
                               Select New DataItem With {.XDate = d.XDate, .Code = d.Code, .Value = Temp.GetValueFunc(d)}).ToList
            Iterations -= 1
            CodeDict = Data.Distinct(New TaskDataComparer).ToDictionary(Function(d) d.Code, Function() New AbcClass(Iterations) {})
        End Sub
#End Region


#Region "IterationData"
        Private Property CurIter As Integer
        Private Property StartBillingPeriod As Date
        Private Property FinalBillingPeriod As Date


        Private Sub RunIterations()
            Dim PrevAbcTable1 As IEnumerable(Of AbcItem)
            Dim PrevAbcTable2 As IEnumerable(Of AbcItem)

            For i = 0 To Iterations
                CurIter = i
                StartBillingPeriod = StartDate
                FinalBillingPeriod = StartDate.AddDays(Temp.BillingPeriod)
                StartDate = StartDate.AddDays(Temp.RunInterval)

                Dim AbcTable = CreateAbcTable()

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
#End Region


        Private Function CreateAbcTable() As IEnumerable(Of AbcItem)
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


#Region "Algorithms"
        Private Sub ThresholdAlgorithm(abcTable As IEnumerable(Of AbcItem))
            Dim UpThresholdAB As Integer
            Dim LowThresholdAB As Integer
            Dim UpThresholdBC As Integer
            Dim LowThresholdBC As Integer

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
                            If QtyB = CurQtyB Then Exit For
                        End If
                    Next
                End If
            End If
        End Sub
#End Region


#Region "Statistics"
        Private Property AbcList As New Dictionary(Of Date, IEnumerable(Of StatItem))
        Private Property PickList As New Dictionary(Of Date, IEnumerable(Of StatItem))
        Private Property ClassChangeList As New List(Of Transition)

        Private Sub RecordStatistics(abcTable As IEnumerable(Of AbcItem))
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

            AbcList.Add(FinalBillingPeriod, Abc)
            PickList.Add(FinalInterval, Pick)
            ClassChangeList.Add(New Transition(FinalBillingPeriod, ClassChange))
        End Sub
#End Region

    End Class
End Namespace