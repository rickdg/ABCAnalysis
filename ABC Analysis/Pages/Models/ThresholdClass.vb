Namespace Pages
    Public Class ThresholdClass

        Public Property Value As Double
        Public Property IsPercent As Boolean
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

    End Class
End Namespace