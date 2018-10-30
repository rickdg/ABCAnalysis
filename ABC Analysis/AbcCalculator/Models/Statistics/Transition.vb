Namespace AbcCalculator
    Public Class Transition

        Public Sub New(xDate As Date, data As IEnumerable(Of DirectionItem))
            Дата = xDate
            AB = GetValue(data, DirectionTransition.AB)
            AC = GetValue(data, DirectionTransition.AC)
            AX = GetValue(data, DirectionTransition.AX)
            BA = GetValue(data, DirectionTransition.BA)
            BC = GetValue(data, DirectionTransition.BC)
            BX = GetValue(data, DirectionTransition.BX)
            CA = GetValue(data, DirectionTransition.CA)
            CB = GetValue(data, DirectionTransition.CB)
            XA = GetValue(data, DirectionTransition.XA)
            XB = GetValue(data, DirectionTransition.XB)
        End Sub


        Public Property Дата As Date
        Public Property AB As Integer
        Public Property AC As Integer
        Public Property AX As Integer
        Public Property BA As Integer
        Public Property BC As Integer
        Public Property BX As Integer
        Public Property CA As Integer
        Public Property CB As Integer
        Public Property XA As Integer
        Public Property XB As Integer


        Private Function GetValue(data As IEnumerable(Of DirectionItem), Direction As DirectionTransition) As Integer
            Dim Item = data.Where(Function(i) i.Direction = Direction).FirstOrDefault
            If Item Is Nothing Then Return 0
            Return Item.QtyCode
        End Function

    End Class
End Namespace