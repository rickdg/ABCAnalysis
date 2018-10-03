Namespace AbcCalculator
    Public Class GenericTransition

        Public Sub New(xDate As Date, arr As IEnumerable(Of DirectionItem))
            Дата = xDate
            Dim NAA = GetValue(arr, DirectionTransition.NAA)
            Dim NAB = GetValue(arr, DirectionTransition.NAB)
            Dim NAC = GetValue(arr, DirectionTransition.NAC)
            Dim AB = GetValue(arr, DirectionTransition.AB)
            Dim AC = GetValue(arr, DirectionTransition.AC)
            Dim AX = GetValue(arr, DirectionTransition.AX)
            Dim BA = GetValue(arr, DirectionTransition.BA)
            Dim BC = GetValue(arr, DirectionTransition.BC)
            Dim BX = GetValue(arr, DirectionTransition.BX)
            Dim CA = GetValue(arr, DirectionTransition.CA)
            Dim CB = GetValue(arr, DirectionTransition.CB)
            Dim CX = GetValue(arr, DirectionTransition.CX)
            Dim XA = GetValue(arr, DirectionTransition.XA)
            Dim XB = GetValue(arr, DirectionTransition.XB)
            Dim XC = GetValue(arr, DirectionTransition.XC)
            A_In = NAA + BA + CA + XA
            B_In = NAB + AB + CB + XB
            C_In = NAC + AC + BC + XC
            X_In = AX + BX + CX
            A_Out = AB + AC + AX
            B_Out = BA + BC + BX
            C_Out = CA + CB + CX
            X_Out = XA + XB + XC
        End Sub


        Public Property Дата As Date
        Public Property A_In As Integer
        Public Property A_Out As Integer
        Public Property B_In As Integer
        Public Property B_Out As Integer
        Public Property C_In As Integer
        Public Property C_Out As Integer
        Public Property X_In As Integer
        Public Property X_Out As Integer


        Private Function GetValue(data As IEnumerable(Of DirectionItem), Direction As DirectionTransition) As Integer
            Dim Item = data.Where(Function(i) i.Direction = Direction).FirstOrDefault
            If Item Is Nothing Then Return 0
            Return Item.QtyCode
        End Function

    End Class
End Namespace