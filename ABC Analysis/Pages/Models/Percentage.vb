Imports Newtonsoft.Json

Namespace Pages
    Public Class Percentage

        Public Property Value2 As Double
        <JsonIgnore>
        Public Property Text As String
            Get
                Return Value2.ToString("P0")
            End Get
            Set
                Dim Result As Double
                Double.TryParse(Value.TrimEnd("%"c).Replace(".", ","), Result)
                Value2 = Math.Abs(Result / 100)
            End Set
        End Property

    End Class
End Namespace