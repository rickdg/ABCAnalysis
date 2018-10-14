Namespace AbcCalculator
    Friend Class PickPercent

        Private ReadOnly Selector As Func(Of StatItem, Integer)


        Public Sub New(xDate As Date, data As IEnumerable(Of StatItem), selector As Func(Of StatItem, Integer))
            Me.Selector = selector
            Дата = xDate
            Dim Sum = data.Sum(Function(i) selector(i))
            If Sum = 0 Then Return
            A = GetValue(data, AbcClass.A) / Sum
            B = GetValue(data, AbcClass.B) / Sum
            C = GetValue(data, AbcClass.C) / Sum
            X = GetValue(data, AbcClass.X) / Sum
            NA = GetValue(data, AbcClass.NA) / Sum
        End Sub


        Public Property Дата As Date
        Public Property A As Double
        Public Property B As Double
        Public Property C As Double
        Public Property X As Double
        Public Property NA As Double


        Private Function GetValue(data As IEnumerable(Of StatItem), abcClass As AbcClass) As Integer
            Dim Item = data.SingleOrDefault(Function(i) i.AbcClass = abcClass)
            If Item Is Nothing Then Return 0
            Return Selector(Item)
        End Function

    End Class
End Namespace
