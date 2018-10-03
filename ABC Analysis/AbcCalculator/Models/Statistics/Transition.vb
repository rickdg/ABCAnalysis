Namespace AbcCalculator
    Public Class Transition

        Public Sub New(xDate As Date, data As IEnumerable(Of DirectionItem))
            Дата = xDate
            NAA = GetValue(data, DirectionTransition.NAA)
            NAB = GetValue(data, DirectionTransition.NAB)
            NAC = GetValue(data, DirectionTransition.NAC)
            AA = GetValue(data, DirectionTransition.AA)
            AB = GetValue(data, DirectionTransition.AB)
            AC = GetValue(data, DirectionTransition.AC)
            AX = GetValue(data, DirectionTransition.AX)
            BA = GetValue(data, DirectionTransition.BA)
            BB = GetValue(data, DirectionTransition.BB)
            BC = GetValue(data, DirectionTransition.BC)
            BX = GetValue(data, DirectionTransition.BX)
            CA = GetValue(data, DirectionTransition.CA)
            CB = GetValue(data, DirectionTransition.CB)
            CC = GetValue(data, DirectionTransition.CC)
            CX = GetValue(data, DirectionTransition.CX)
            XA = GetValue(data, DirectionTransition.XA)
            XB = GetValue(data, DirectionTransition.XB)
            XC = GetValue(data, DirectionTransition.XC)
            XX = GetValue(data, DirectionTransition.XX)
        End Sub


        Public Property Дата As Date
        Public Property NAA As Integer
        Public Property NAB As Integer
        Public Property NAC As Integer
        Public Property AA As Integer
        Public Property AB As Integer
        Public Property AC As Integer
        Public Property AX As Integer
        Public Property BA As Integer
        Public Property BB As Integer
        Public Property BC As Integer
        Public Property BX As Integer
        Public Property CA As Integer
        Public Property CB As Integer
        Public Property CC As Integer
        Public Property CX As Integer
        Public Property XA As Integer
        Public Property XB As Integer
        Public Property XC As Integer
        Public Property XX As Integer


        Private Function GetValue(data As IEnumerable(Of DirectionItem), Direction As DirectionTransition) As Integer
            Dim Item = data.Where(Function(i) i.Direction = Direction).FirstOrDefault
            If Item Is Nothing Then Return 0
            Return Item.QtyCode
        End Function

    End Class
End Namespace