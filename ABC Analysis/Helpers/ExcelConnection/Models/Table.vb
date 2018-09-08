Namespace ExcelConnection
    Public Class Table

        Public Sub New(name As String)
            Me.Name = name
        End Sub


        Public ReadOnly Property Name As String
        Public Property Columns As IEnumerable(Of Column)


        Public Overrides Function ToString() As String
            Return Name
        End Function

    End Class
End Namespace