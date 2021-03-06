﻿Imports ABCAnalysis.Pages

Namespace AbcCalculator
    Public Class ResultCalculation

        Public Sub New()
        End Sub


        Public Sub New(temp As TemplateBase, pickQtyTasks As IEnumerable(Of PickValue), classChange As IEnumerable(Of Transition))
            Interval = temp.RunInterval
            Period = temp.BillingPeriod

            Dim Sum = pickQtyTasks.Sum(Function(i) i.A + i.B + i.C + i.X + i.NA)
            If Sum <> 0 Then AvgPickPercent = pickQtyTasks.Sum(Function(i) i.A + i.B) / Sum

            Transition = classChange.Sum(Function(i) i.AB + i.AC + i.AX + i.BA + i.BC + i.BX)
            AllTransition = Transition + classChange.Sum(Function(i) i.CA + i.CB + i.XA + i.XB)
        End Sub


        Public Property Interval As Integer
        Public Property Period As Integer
        Public Property AvgPickPercent As Double
        Public Property Transition As Integer
        Public Property AllTransition As Integer

    End Class
End Namespace