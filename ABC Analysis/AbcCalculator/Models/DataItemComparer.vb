Namespace AbcCalculator
    Public Class DataItemComparer
        Implements IEqualityComparer(Of DataItem)

        Public Function Equals1(x As DataItem, y As DataItem) As Boolean Implements IEqualityComparer(Of DataItem).Equals
            Return x.Code.Equals(y.Code)
        End Function


        Public Function GetHashCode1(obj As DataItem) As Integer Implements IEqualityComparer(Of DataItem).GetHashCode
            Return obj.Code.GetHashCode
        End Function

    End Class
End Namespace