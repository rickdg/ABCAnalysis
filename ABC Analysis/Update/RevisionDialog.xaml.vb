Partial Public Class RevisionDialog
    Inherits UserControl

    Public Sub New(element As Grid)
        InitializeComponent()

        Dim Parent = DirectCast(element.Parent, Grid)
        If Parent IsNot Nothing Then
            Parent.Children.Remove(element)
        End If

        Grid.Children.Add(element)
    End Sub

End Class