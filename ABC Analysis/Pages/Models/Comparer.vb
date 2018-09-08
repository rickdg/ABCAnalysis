Namespace Pages
    Public Class Comparer(Of T As Named(Of TName), TName As IComparable)
        Implements IComparer(Of T)

        Public Function Compare(x As T, y As T) As Integer Implements IComparer(Of T).Compare
            Return x.Name.CompareTo(y.Name)
        End Function

    End Class
End Namespace