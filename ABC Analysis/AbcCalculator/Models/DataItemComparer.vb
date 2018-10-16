Namespace AbcCalculator
    Public Class TaskDataComparer
        Implements IEqualityComparer(Of TaskDataExtend)

        Public Function Equals1(x As TaskDataExtend, y As TaskDataExtend) As Boolean Implements IEqualityComparer(Of TaskDataExtend).Equals
            Return x.Code.Equals(y.Code)
        End Function


        Public Function GetHashCode1(obj As TaskDataExtend) As Integer Implements IEqualityComparer(Of TaskDataExtend).GetHashCode
            Return obj.Code.GetHashCode
        End Function

    End Class
End Namespace