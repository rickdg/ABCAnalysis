Namespace Pages
    Partial Public Class MainPage
        Inherits UserControl

        Public Shared Property Model As MainPageVM
        Private ReadOnly SerializeFileName As String = "MainPage"


        Public Sub New()
            InitializeComponent()
            If FileExists(SerializeFileName) Then
                Model = Deserialize(Of MainPageVM)(SerializeFileName)
            Else
                Model = New MainPageVM With {.SerializeFileName = SerializeFileName}
            End If
            DataContext = Model
        End Sub

    End Class
End Namespace