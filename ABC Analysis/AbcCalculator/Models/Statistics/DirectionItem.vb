Namespace AbcCalculator
    Public Class DirectionItem

        Public Sub New(prevAbc As AbcClass, curAbc As AbcClass)
            Direction = CType([Enum].ToObject(GetType(DirectionTransition), prevAbc * 10 + curAbc), DirectionTransition)
        End Sub


        Public Property Direction As DirectionTransition
        Public Property QtyCode As Integer

    End Class
End Namespace