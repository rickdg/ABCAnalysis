Imports ABCAnalysis.Pages
Imports FirstFloor.ModernUI.Windows.Controls

Namespace Content
    Partial Public Class RemoveData
        Inherits UserControl

        Private _StartDate As Date = Now
        Private _EndDate As Date = Now


        Public Sub New(dlg As ModernDialog)
            InitializeComponent()
            DataContext = Me
            AddHandler dlg.Buttons.First.Click, AddressOf ClickYes
        End Sub


        Private Sub ClickYes(s As Object, e As RoutedEventArgs)
            Using Context = DatabaseManager.CurrentDatabase.Context
                Context.DeleteTasks(StartDate, EndDate)
            End Using
        End Sub


        Public Property StartDate As Date
            Get
                Return _StartDate.Date
            End Get
            Set
                _StartDate = Value
            End Set
        End Property
        Public Property EndDate As Date
            Get
                Return _EndDate.Date
            End Get
            Set
                _EndDate = Value
            End Set
        End Property

    End Class
End Namespace