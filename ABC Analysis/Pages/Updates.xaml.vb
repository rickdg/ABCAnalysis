Namespace Pages
    Partial Public Class Updates
        Inherits UserControl

        Public Sub New()
            InitializeComponent()
            For Each Rev In Revisions
                StackPanel.Children.Add(New TextBlock With {.Text = Rev.Value.Version})

                Dim Grid = Rev.Value.GetContent()
                Grid.Margin = New Thickness(24, 0, 0, 0)

                Dim Parent = CType(Grid.Parent, Grid)
                If Parent IsNot Nothing Then
                    Parent.Children.Remove(Grid)
                End If

                StackPanel.Children.Add(Grid)
            Next
        End Sub

    End Class
End Namespace