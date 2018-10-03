Imports ABCAnalysis.Pages

Namespace AbcCalculator
    Public Class ResultCalculation

        Public Sub New(temp As Template, pickQtyTasks As IEnumerable(Of AbcValue))
            Interval = temp.RunInterval
            Period = temp.BillingPeriod
            UpAB = temp.UpThresholdAB.Value
            LowAB = temp.LowThresholdAB.Value
            UpBC = temp.UpThresholdBC.Value
            LowBC = temp.LowThresholdBC.Value

            Dim Sum = pickQtyTasks.Sum(Function(i) i.A + i.B + i.C + i.X + i.NA)
            If Sum = 0 Then Return

            A = pickQtyTasks.Sum(Function(i) i.A) / Sum
            B = pickQtyTasks.Sum(Function(i) i.B) / Sum
            C = pickQtyTasks.Sum(Function(i) i.C) / Sum
            X = pickQtyTasks.Sum(Function(i) i.X) / Sum
            NA = pickQtyTasks.Sum(Function(i) i.NA) / Sum
        End Sub


        Public Property Interval As Byte
        Public Property Period As Byte
        Public Property UpAB As Double
        Public Property LowAB As Double
        Public Property UpBC As Double
        Public Property LowBC As Double
        Public Property A As Double
        Public Property B As Double
        Public Property C As Double
        Public Property X As Double
        Public Property NA As Double

    End Class
End Namespace