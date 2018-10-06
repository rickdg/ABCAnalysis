Imports ABCAnalysis.Pages

Namespace AbcCalculator
    Public Class ResultCalculation

        Public Sub New(temp As Template, pickQtyTasks As IEnumerable(Of AbcValue), genericClassChange As IEnumerable(Of GenericTransition))
            Interval = temp.RunInterval
            Period = temp.BillingPeriod

            Pick = pickQtyTasks.Sum(Function(i) i.A + i.B)
            PickOnePercent = Pick * 0.01
            Dim Sum = pickQtyTasks.Sum(Function(i) i.A + i.B + i.C + i.X + i.NA)
            If Sum <> 0 Then PickPercent = Pick / Sum

            Transition = genericClassChange.Sum(Function(i) i.A_In + i.A_Out + i.B_In + i.B_Out)
        End Sub


        Public Property Interval As Byte
        Public Property Period As Byte
        Public Property PickPercent As Double
        Public Property Pick As Integer
        Public Property PickOnePercent As Double
        Public Property Transition As Integer

    End Class
End Namespace