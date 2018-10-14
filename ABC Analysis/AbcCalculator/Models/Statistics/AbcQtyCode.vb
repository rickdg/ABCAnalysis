Namespace AbcCalculator
    Friend Class AbcQtyCode

        Public Sub New(xDate As Date, data As IEnumerable(Of StatItem))
            Дата = xDate
            A = GetValue(data, AbcClass.A)
            B = GetValue(data, AbcClass.B)
            C = GetValue(data, AbcClass.C)
            X = GetValue(data, AbcClass.X)
        End Sub


        Public Property Дата As Date
        Public Property A As Integer
        Public Property B As Integer
        Public Property C As Integer
        Public Property X As Integer


        Private Function GetValue(data As IEnumerable(Of StatItem), abcClass As AbcClass) As Integer
            Dim Item = data.SingleOrDefault(Function(i) i.AbcClass = abcClass)
            If Item Is Nothing Then Return 0
            Return Item.QtyCode
        End Function

    End Class
End Namespace
