Namespace ExcelConnection
    Public Class Column

        Public Sub New(name As String, dataType_id As Integer)
            Me.Name = name
            Me.DataType = CType([Enum].ToObject(GetType(AdoEnums), dataType_id), AdoEnums)
        End Sub


        Public Sub New(name As String, dataType As AdoEnums)
            Me.Name = name
            Me.DataType = dataType
        End Sub


        Public ReadOnly Property Name As String
        Public ReadOnly Property DataType As AdoEnums


        Public Overrides Function ToString() As String
            Return Name
        End Function

    End Class
End Namespace