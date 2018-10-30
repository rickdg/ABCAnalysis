Namespace Pages
    Partial Public Class DataManager
        Inherits UserControl

        Public Shared Property Model As New DataManagerModel


        Public Sub New()
            InitializeComponent()
            AddHandler AxisX.PreviewRangeChanged, AddressOf Model.Axis_PreviewRangeChanged
            DataContext = Model
        End Sub


        Private Sub DataManager_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
            If MainPageModel.IsDataChanged Then
                Model.RefreshSeriesCollection()
                MainPageModel.IsDataChanged = False
            End If
        End Sub

    End Class
End Namespace