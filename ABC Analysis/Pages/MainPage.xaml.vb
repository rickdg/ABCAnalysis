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


#Region "Select Today"
        Private Sub CommandBinding_CanExecute(sender As Object, e As CanExecuteRoutedEventArgs)
            If e.Command Is MyCommands.SelectToday Then
                e.CanExecute = True
            End If
        End Sub


        Private Sub CommandBinding_Executed(sender As Object, e As ExecutedRoutedEventArgs)
            If e.Command Is MyCommands.SelectToday Then
                CType(e.Parameter, Calendar).SelectedDate = Now
            End If
        End Sub
#End Region

    End Class


    Public Class MyCommands
        Public Shared SelectToday As New RoutedCommand("Today", GetType(MyCommands))
    End Class

End Namespace