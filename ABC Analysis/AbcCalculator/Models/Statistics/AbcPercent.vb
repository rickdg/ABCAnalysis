Namespace AbcCalculator
    Friend Class AbcPercent

        Private ReadOnly Selector As Func(Of StatItem, Integer)


        Public Sub New(xDate As Date, data As IEnumerable(Of StatItem), selector As Func(Of StatItem, Integer))
            Me.Selector = selector
            Дата = xDate
            Dim Sum = data.Where(Function(i) {AbcClass.A, AbcClass.B, AbcClass.C}.Contains(i.AbcClass)).Sum(Function(i) selector(i))
            If Sum = 0 Then Return
            A = GetValue(data, AbcClass.A) / Sum
            B = GetValue(data, AbcClass.B) / Sum
            C = GetValue(data, AbcClass.C) / Sum
        End Sub


        Public Property Дата As Date
        Public Property A As Double
        Public Property B As Double
        Public Property C As Double


        Private Function GetValue(data As IEnumerable(Of StatItem), abcClass As AbcClass) As Integer
            Dim Item = data.SingleOrDefault(Function(i) i.AbcClass = abcClass)
            If Item Is Nothing Then Return 0
            Return Selector(Item)
        End Function

    End Class
End Namespace
