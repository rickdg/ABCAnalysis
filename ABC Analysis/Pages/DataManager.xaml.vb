Namespace Pages
    Partial Public Class DataManager
        Inherits UserControl

        Public Sub New()
            InitializeComponent()
            AddHandler AxisX.PreviewRangeChanged, AddressOf Model.Axis_PreviewRangeChanged
            DataContext = Model
        End Sub


        Public Shared Property Model As New DataManagerVM

    End Class
End Namespace