Imports ABCAnalysis.Connection.Excel
Imports ABCAnalysis.Connection.SqlServer
Imports ABCAnalysis.Content
Imports FirstFloor.ModernUI.Presentation
Imports FirstFloor.ModernUI.Windows.Controls
Imports LiveCharts
Imports LiveCharts.Configurations
Imports LiveCharts.Wpf

Namespace Pages
    Public Class DataManagerVM
        Inherits NotifyPropertyChanged

        Private ReadOnly MonthFormatter As Func(Of Double, String) = Function(v) New DateTime(CLng(Math.Abs(v) * TimeSpan.FromDays(1).Ticks * 30.44)).ToString("MMM yyyy")
        Private ReadOnly NFormatter As Func(Of Double, String) = Function(v) v.ToString("N0")
        Private ReadOnly PFormatter As Func(Of Double, String) = Function(v) v.ToString("P0")


        Private _YFormatter As Func(Of Double, String)
        Private _XFormatter As Func(Of Double, String)


        Public Sub New()
            Mapper.X(Function(m) m.XDate.Ticks / (TimeSpan.FromDays(1).Ticks * 30.44))
            Mapper.Y(Function(m) m.Value)
            YFormatter = NFormatter
            XFormatter = MonthFormatter
            StackMode = StackMode.Values
            RefreshSeriesCollection()
        End Sub


        Public Property SeriesCollection As New SeriesCollection
        Public Property XFormatter As Func(Of Double, String)
            Get
                Return _XFormatter
            End Get
            Set
                _XFormatter = Value
                OnPropertyChanged("XFormatter")
            End Set
        End Property
        Public Property YFormatter As Func(Of Double, String)
            Get
                Return _YFormatter
            End Get
            Set(ByVal value As Func(Of Double, String))
                _YFormatter = value
                OnPropertyChanged("YFormatter")
            End Set
        End Property
        Public Property StackMode As StackMode
        Public Property Mapper As New CartesianMapper(Of MeasureModel)


        Public ReadOnly Property CmdLoadTasks As ICommand = New RelayCommand(AddressOf LoadTasksExecute)
        Private Sub LoadTasksExecute(parameter As Object)
            Dim Dlg As New ModernDialog
            Dlg.Content = New LoadData(Dlg) With {
                .LoadType = LoadType.PickTasks,
                .CmdParameters = {},
                .ProcParameter = New StoredProcedureParameter With {
                    .CommandText = "dbo.LoadTasks",
                    .ParameterName = "@ExcelTasks",
                    .TypeName = "TaskExcelTable"}}
            Dlg.ShowDialog()
            If Dlg.DialogResult Then
                RefreshSeriesCollection()
                MainPage.Model.UpdateTemplates()
            End If
        End Sub
        Public ReadOnly Property CmdDeleteTasks As ICommand = New RelayCommand(AddressOf DeleteTasksExecute)
        Private Sub DeleteTasksExecute(parameter As Object)
            Dim Dlg As New ModernDialog With {.Title = "Удаление данных"}
            Dlg.Buttons = {Dlg.YesButton, Dlg.CancelButton}
            Dlg.Content = New RemoveData(Dlg)
            If Dlg.ShowDialog() Then
                RefreshSeriesCollection()
                MainPage.Model.UpdateTemplates()
            End If
        End Sub
        Public ReadOnly Property CmdChangeStackMode As ICommand = New RelayCommand(AddressOf ChangeStackModeExecute)
        Private Sub ChangeStackModeExecute(ByVal parameter As Object)
            If StackMode = StackMode.Values Then
                YFormatter = PFormatter
                StackMode = StackMode.Percentage
            Else
                YFormatter = NFormatter
                StackMode = StackMode.Values
            End If
            For Each Series In SeriesCollection.Cast(Of StackedColumnSeries)
                Series.StackMode = StackMode
            Next
        End Sub


        Private Sub RefreshSeriesCollection()
            SeriesCollection.Clear()
            SeriesCollection.AddRange(GetStackedColumnSeriesMonth())
        End Sub


        Private Function GetData() As IEnumerable(Of Month_Tasks_Orders)
            Using Context = DatabaseManager.CurrentDatabase.Context
                Return (From Task In Context.TaskDatas
                        Group By Task.YearNum, Task.MonthNum Into SumTasks = Sum(Task.Tasks), SumOrders = Sum(Task.Orders)
                        Select New Month_Tasks_Orders With {.YearNum = YearNum, .MonthNum = MonthNum, .Tasks = SumTasks, .Orders = SumOrders}).ToList
            End Using
        End Function


        Private Function GetStackedColumnSeriesMonth() As IEnumerable(Of StackedColumnSeries)
            Dim TmpData = GetData()
            Return (From t In TmpData
                    Group New MeasureModel(New DateTime(t.YearNum, t.MonthNum, 1), t.Tasks) By t Into ToList
                    Select New StackedColumnSeries With {
                        .Tag = 1,
                        .Fill = ConvertIntToBrush(1),
                        .StackMode = StackMode,
                        .Configuration = Mapper,
                        .Title = "Задачи",
                        .Values = New ChartValues(Of MeasureModel)(ToList.OrderBy(Function(m) m.XDate))}).Concat(
                        From t In TmpData
                        Group New MeasureModel(New DateTime(t.YearNum, t.MonthNum, 1), t.Orders) By t Into ToList
                        Select New StackedColumnSeries With {
                            .Tag = 2,
                            .Fill = ConvertIntToBrush(2),
                            .StackMode = StackMode,
                            .Configuration = Mapper,
                            .Title = "ЗнП",
                            .Values = New ChartValues(Of MeasureModel)(ToList.OrderBy(Function(m) m.XDate))}).ToList
        End Function


        Public Sub Axis_PreviewRangeChanged(e As Events.PreviewRangeChangedEventArgs)
            Dim Range = e.PreviewMaxValue - e.PreviewMinValue
            If Range = e.Range Then Return
            Select Case Range
                Case Is > 120
                    e.Cancel = True
            End Select
        End Sub


        Public Sub RefreshColorSeries()
            For Each Series In SeriesCollection.Cast(Of StackedColumnSeries)
                Series.Fill = ConvertIntToBrush(CInt(Series.Tag))
            Next
        End Sub


        Private Function ConvertIntToBrush(int As Integer) As Brush
            Dim Color = AppearanceManager.Current.AccentColor
            Select Case int
                Case 1
                    Color = Color.Multiply(Color, 0.2)
                Case 2

                Case Else
                    Return Brushes.Black
            End Select
            Color.A = 200
            Return New SolidColorBrush(Color)
        End Function

    End Class
End Namespace