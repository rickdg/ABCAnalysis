Namespace AbcCalculator
    Public Class TaskDataComparer
        Implements IEqualityComparer(Of TaskData)

        Public Function Equals1(x As TaskData, y As TaskData) As Boolean Implements IEqualityComparer(Of TaskData).Equals
            Return x.Code.Equals(y.Code)
        End Function


        Public Function GetHashCode1(obj As TaskData) As Integer Implements IEqualityComparer(Of TaskData).GetHashCode
            Return obj.Code.GetHashCode
        End Function

    End Class
End Namespace