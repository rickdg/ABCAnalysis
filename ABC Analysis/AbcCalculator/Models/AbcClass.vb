Namespace AbcCalculator
    Public Enum AbcClass As Byte
        NA
        A
        B
        C
        X
    End Enum


    Public Enum DirectionTransition As Byte
        NAA = 1
        NAB
        NAC
        AA = 11
        AB
        AC
        AX
        BA = 21
        BB
        BC
        BX
        CA = 31
        CB
        CC
        CX
        XA = 41
        XB
        XC
        XX
    End Enum
End Namespace