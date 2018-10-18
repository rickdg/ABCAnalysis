Imports ABCAnalysis.Content
Imports FirstFloor.ModernUI.Windows.Controls

Module HintHelper

    Public Sub ShowHintExecute(resourceKey As Object)
        Dim wnd = New ModernWindow With {
            .WindowStartupLocation = WindowStartupLocation.CenterScreen,
            .Style = CType(Application.Current.Resources("BlankWindow"), Style),
            .Content = New HintMessage(GetContent(resourceKey)),
            .Width = 740}
        wnd.ShowDialog()
    End Sub


    Private Function GetContent(resourceKey As Object) As Grid
        Return CType(Application.Current.FindResource(resourceKey), Grid)
    End Function

End Module