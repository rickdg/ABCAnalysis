Namespace Pages
    Public Class Threshold

        Public Property IsPercent As Boolean
        Public Property IsUp As Boolean
        Public Property Value As Double
        Public Property Text As String
            Get
                If IsPercent Then
                    Return Value.ToString("P0")
                End If
                Return Value.ToString()
            End Get
            Set(_value As String)
                Dim Result As Double
                If _value.EndsWith("%") Then
                    Double.TryParse(_value.TrimEnd("%"c), Result)
                    Value = Math.Abs(Result) / 100
                    IsPercent = True
                Else
                    Double.TryParse(_value, Result)
                    Value = Math.Abs(Result)
                    IsPercent = False
                End If
            End Set
        End Property


        Public Function GetThreshold(val As Integer) As Integer
            Dim Result As Integer
            If IsPercent Then
                If IsUp Then
                    Result = CInt(val + (val * Value))
                Else
                    Result = CInt(val - (val * Value))
                End If
            Else
                If IsUp Then
                    Result = CInt(val + Value)
                Else
                    Result = CInt(val - Value)
                End If
            End If
            If Result < 0 Then Result = 0
            Return Result
        End Function

    End Class
End Namespace