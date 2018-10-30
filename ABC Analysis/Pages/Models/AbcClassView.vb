Imports ABCAnalysis.AbcCalculator

Namespace Pages
    Public Class AbcClassView

        Public AbcClass_id As Integer


        Public Property Позиция As Long
        Public ReadOnly Property Класс As String
            Get
                Return CType([Enum].ToObject(GetType(AbcClass), AbcClass_id), AbcClass).ToString
            End Get
        End Property

    End Class
End Namespace