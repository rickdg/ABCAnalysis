﻿Imports ABCAnalysis.Pages

Namespace AbcCalculator
    Public Class ResultCalculation

        Public Sub New(temp As Template, pickQtyTasks As IEnumerable(Of AbcValue), genericClassChange As IEnumerable(Of Transition))
            Interval = temp.RunInterval
            Period = temp.BillingPeriod

            Dim Sum = pickQtyTasks.Sum(Function(i) i.A + i.B + i.C + i.X + i.NA)
            If Sum <> 0 Then AvgPickPercent = pickQtyTasks.Sum(Function(i) i.A + i.B) / Sum

            Transition = genericClassChange.Sum(Function(i) i.AB + i.AC + i.AX + i.BA + i.BC + i.BX)
        End Sub


        Public Property Interval As Byte
        Public Property Period As Byte
        Public Property AvgPickPercent As Double
        Public Property Transition As Integer

    End Class
End Namespace