﻿Imports ABCAnalysis.Pages

Namespace AbcCalculator
    Public MustInherit Class BaseCalculator

        Public MustOverride Sub Calculate()


#Region "Input"
        Public Property Temp As TemplateBase
#End Region


        Public Property InitialDate As Date
        Public Property FinalDate As Date
        Public Property Data As IEnumerable(Of TaskDataExtend)


#Region "MasterData"
        Public Property Iterations As Integer
        Public Property StartDate As Date
        Public Property CalculationData As IEnumerable(Of DataItem)
        Public Property CodeDict As Dictionary(Of Long, AbcClass())


        Public Sub SetCalculationData()
            CalculationData = (From d In Data
                               Where Temp.UserPositionTypes_id.Contains(d.UserPositionType_Id) AndAlso
                                   Temp.Categoryes_id.Contains(d.Category_Id) AndAlso Temp.IsSalesOrderFunc(d)
                               Select New DataItem With {.XDate = d.XDate, .Code = d.Code, .Value = d.Orders}).ToList
        End Sub


        Public Sub SetMasterData()
            Iterations = CInt(Fix((DateDiff("d", InitialDate, FinalDate) - Temp.BillingPeriodForCalculate) / Temp.RunInterval))
            StartDate = FinalDate.AddDays(-((Iterations * Temp.RunInterval) + Temp.BillingPeriodForCalculate))
            CodeDict = Data.Distinct(New TaskDataComparer).ToDictionary(Function(d) d.Code, Function() New AbcClass(Iterations) {})
        End Sub
#End Region


#Region "Iterations"
        Public Property CurIter As Integer
        Public Property StartBillingPeriod As Date
        Public Property FinalBillingPeriod As Date
        Public Property LastAbcTable As IList(Of AbcItem)


        Public Sub RunIterations()
            Dim AbcTable As IList(Of AbcItem)
            Dim PrevAbcTable1 As IEnumerable(Of AbcItem)
            Dim PrevAbcTable2 As IEnumerable(Of AbcItem)

            For i = 0 To Iterations
                CurIter = i
                StartBillingPeriod = StartDate
                FinalBillingPeriod = StartDate.AddDays(Temp.BillingPeriodForCalculate)
                StartDate = StartDate.AddDays(Temp.RunInterval)

                AbcTable = GetAbcTable()

                If CurIter = 0 Then
                    RestoreABC(AbcTable)
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

                Equalization(AbcTable)

                RecordStatistics(AbcTable)
            Next

            LastAbcTable = AbcTable
        End Sub


        Friend MustOverride Sub RestoreABC(abcTable As IEnumerable(Of AbcItem))


        Friend MustOverride Sub RecordStatistics(abcTable As IEnumerable(Of AbcItem))
#End Region


#Region "Methods"
        Public Function GetAbcTable() As IList(Of AbcItem)
            Dim AbcTable = (From d In CalculationData
                            Where d.XDate >= StartBillingPeriod AndAlso d.XDate <= FinalBillingPeriod
                            Group By d.Code Into Sum(d.Value)
                            Order By Sum Descending, Code Ascending
                            Select New AbcItem With {.Code = Code, .Value = Sum, .AbcClass = AbcClass.C}).ToList

            If CurIter = 0 Then
                For i = 0 To AbcTable.Count - 1
                    Dim Item = AbcTable(i)
                    If i < Temp.QuantityAClass Then
                        Item.AbcClass = AbcClass.A
                        CodeDict(Item.Code)(CurIter) = AbcClass.A
                    Else
                        If i < Temp.QuantityABClass Then
                            Item.AbcClass = AbcClass.B
                            CodeDict(Item.Code)(CurIter) = AbcClass.B
                        Else
                            CodeDict(Item.Code)(CurIter) = AbcClass.C
                        End If
                    End If
                Next
            Else
                For i = 0 To AbcTable.Count - 1
                    Dim Item = AbcTable(i)
                    Dim PrevAbc = CodeDict(Item.Code)(CurIter - 1)
                    If {AbcClass.A, AbcClass.B, AbcClass.C}.Contains(PrevAbc) Then
                        Item.AbcClass = PrevAbc
                        CodeDict(Item.Code)(CurIter) = PrevAbc
                    Else
                        If i < Temp.QuantityAClass Then
                            Item.AbcClass = AbcClass.A
                            CodeDict(Item.Code)(CurIter) = AbcClass.A
                        Else
                            If i < Temp.QuantityABClass Then
                                Item.AbcClass = AbcClass.B
                                CodeDict(Item.Code)(CurIter) = AbcClass.B
                            Else
                                CodeDict(Item.Code)(CurIter) = AbcClass.C
                            End If
                        End If
                    End If
                Next
            End If

            Return AbcTable
        End Function
#End Region


#Region "Algorithms"
        Public Sub TransitionToX(prevAbcTable As IEnumerable(Of AbcItem))
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


        Public Sub Equalization(abcTable As IList(Of AbcItem))
            Dim QtyA = Temp.QuantityAClass
            Dim QtyB = Temp.QuantityBClass
            Dim CurQtyA = abcTable.AsEnumerable.Count(Function(Item) Item.AbcClass = AbcClass.A)

            If QtyA > CurQtyA Then
                For i = 0 To abcTable.Count - 1
                    Dim Item = abcTable(i)
                    If Item.AbcClass <> AbcClass.A Then
                        Item.AbcClass = AbcClass.A
                        CodeDict(Item.Code)(CurIter) = AbcClass.A
                        CurQtyA += 1
                        If QtyA = CurQtyA Then Exit For
                    End If
                Next
            ElseIf QtyA < CurQtyA Then
                For i = abcTable.Count - 1 To 0 Step -1
                    Dim Item = abcTable(i)
                    If Item.AbcClass = AbcClass.A Then
                        If QtyB = 0 Then
                            Item.AbcClass = AbcClass.C
                            CodeDict(Item.Code)(CurIter) = AbcClass.C
                        Else
                            Item.AbcClass = AbcClass.B
                            CodeDict(Item.Code)(CurIter) = AbcClass.B
                        End If
                        CurQtyA -= 1
                        If QtyA = CurQtyA Then Exit For
                    End If
                Next
            End If

            If QtyB = 0 Then Return
            Dim CurQtyB = abcTable.AsEnumerable.Count(Function(Item) Item.AbcClass = AbcClass.B)
            If QtyB > CurQtyB Then
                For i = 0 To abcTable.Count - 1
                    Dim Item = abcTable(i)
                    If Item.AbcClass = AbcClass.C Then
                        Item.AbcClass = AbcClass.B
                        CodeDict(Item.Code)(CurIter) = AbcClass.B
                        CurQtyB += 1
                        If QtyB = CurQtyB Then Exit For
                    End If
                Next
            ElseIf QtyB < CurQtyB Then
                For i = abcTable.Count - 1 To 0 Step -1
                    Dim Item = abcTable(i)
                    If Item.AbcClass = AbcClass.B Then
                        Item.AbcClass = AbcClass.C
                        CodeDict(Item.Code)(CurIter) = AbcClass.C
                        CurQtyB -= 1
                        If QtyB = CurQtyB Then Return
                    End If
                Next
            End If
        End Sub
#End Region

    End Class
End Namespace