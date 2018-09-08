Imports System.Text

Namespace Content
    Partial Public Class ErrorMessage
        Inherits UserControl

        Public Sub New(ex As Exception)
            InitializeComponent()
            Message.BBCode = GetInnerException(ex)
        End Sub

        Private Function GetInnerException(ex As Exception) As String
            Dim Result As New StringBuilder
            Result.Append(ex.Message & vbCrLf & vbCrLf)
            If ex.InnerException IsNot Nothing Then
                Result.Append(GetInnerException(ex.InnerException))
            End If
            Return Result.ToString
        End Function

    End Class
End Namespace