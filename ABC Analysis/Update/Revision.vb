Imports FirstFloor.ModernUI.Windows.Controls

Public Class Revision

    Public Property VersionPart As String
    Public ReadOnly Property Version As String
        Get
            Return $"{VersionPart}.{Number} ({XDate.ToShortDateString})"
        End Get
    End Property
    Public Property Number As Integer
    Public Property IsUpdate As Boolean
    Public Property XDate As Date


    Public Sub UpdateDateBase()
        If IsUpdate Then ExecuteSqlCommand(GetSqlCommand)
    End Sub


    Public Sub Show()
        Dim Dlg As New ModernDialog With {
            .WindowStartupLocation = WindowStartupLocation.CenterScreen,
            .Title = Version,
            .Content = New RevisionDialog(GetContent)}
        Dlg.ShowDialog()
    End Sub


    Public Function GetContent() As Grid
        Return CType(Application.Current.FindResource($"Content-{Number}"), Grid)
    End Function


    Private Function GetSqlCommand() As String
        Return CStr(Application.Current.FindResource($"SqlCommand-{Number}"))
    End Function

End Class