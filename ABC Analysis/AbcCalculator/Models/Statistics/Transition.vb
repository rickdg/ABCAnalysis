Namespace AbcCalculator
    Public Class Transition

        Public Sub New(xDate As Date, arr As IEnumerable(Of DirectionItem))
            Дата = xDate
            NAA = GetQtyCode(arr, DirectionTransition.NAA)
            NAB = GetQtyCode(arr, DirectionTransition.NAB)
            NAC = GetQtyCode(arr, DirectionTransition.NAC)
            AA = GetQtyCode(arr, DirectionTransition.AA)
            AB = GetQtyCode(arr, DirectionTransition.AB)
            AC = GetQtyCode(arr, DirectionTransition.AC)
            AX = GetQtyCode(arr, DirectionTransition.AX)
            BA = GetQtyCode(arr, DirectionTransition.BA)
            BB = GetQtyCode(arr, DirectionTransition.BB)
            BC = GetQtyCode(arr, DirectionTransition.BC)
            BX = GetQtyCode(arr, DirectionTransition.BX)
            CA = GetQtyCode(arr, DirectionTransition.CA)
            CB = GetQtyCode(arr, DirectionTransition.CB)
            CC = GetQtyCode(arr, DirectionTransition.CC)
            CX = GetQtyCode(arr, DirectionTransition.CX)
            XA = GetQtyCode(arr, DirectionTransition.XA)
            XB = GetQtyCode(arr, DirectionTransition.XB)
            XC = GetQtyCode(arr, DirectionTransition.XC)
            XX = GetQtyCode(arr, DirectionTransition.XX)
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


        Private Function GetQtyCode(list As IEnumerable(Of DirectionItem), Direction As DirectionTransition) As Integer
            Dim DirecItem = list.Where(Function(i) i.Direction = Direction).FirstOrDefault
            If DirecItem Is Nothing Then Return 0
            Return DirecItem.QtyCode
        End Function

    End Class
End Namespace