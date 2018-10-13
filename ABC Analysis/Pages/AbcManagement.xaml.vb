Imports System.Collections.ObjectModel
Imports System.Data
Imports ABCAnalysis.Content
Imports ABCAnalysis.ExcelConnection
Imports FirstFloor.ModernUI.Presentation
Imports FirstFloor.ModernUI.Windows.Controls
Imports LiveCharts
Imports LiveCharts.Wpf

Namespace Pages
    Partial Public Class AbcManagement
        Inherits UserControl

        Public Sub New()
            InitializeComponent()
            DataContext = Me
        End Sub


        Public Property EntitiesCollection As ObservableCollection(Of AbcGroupVM) = MainPage.Model.AbcGroups
        Public Property AbcGroup_id As Integer
        Public Property SeriesCollection As New SeriesCollection
        Public Property StackMode As StackMode


        Public ReadOnly Property CmdAddNewEntity As ICommand = New RelayCommand(AddressOf AddNewEntityExecute)
        Private Sub AddNewEntityExecute(parameter As Object)
            Using Context As New AbcAnalysisEntities
                Dim NewEntity = Context.AbcGroups.Add(New AbcGroup)
                Context.SaveChanges()
                EntitiesCollection.Add(New AbcGroupVM With {.ParentCollection = EntitiesCollection, .Entity = NewEntity})
            End Using
        End Sub
        Public ReadOnly Property CmdLoadTasks As ICommand = New RelayCommand(AddressOf LoadTasksExecute)
        Private Sub LoadTasksExecute(parameter As Object)
            Dim Dlg As New ModernDialog
            Dlg.Content = New DataLoader(Dlg) With {
                .LoadType = CType(parameter, LoadType),
                .CmdParameters = {New CommandParameter With {
                    .Name = "@AbcGroup_id",
                    .SqlDbType = SqlDbType.Int,
                    .Value = AbcGroup_id}},
                .ProcParameters = New StoredProcedureParameters With {
                    .CommandText = "dbo.LoadAbc",
                    .ParameterName = "@ExcelTable",
                    .TypeName = "ExcelAbcTable"}}
            Dlg.ShowDialog()
            If Dlg.DialogResult Then RefreshSeriesCollection()
        End Sub


        Private Sub RefreshSeriesCollection()
            SeriesCollection.Clear()
            SeriesCollection.AddRange(GetStackedColumnSeriesMonth())
        End Sub


        Private Function GetDataByMonth() As IEnumerable(Of Month_Tasks_Orders)
            Using Context As New AbcAnalysisEntities
                Return (From Task In Context.TaskDatas
                        Group Task By Task.YearNum, Task.MonthNum Into SumTasks = Sum(Task.Tasks), SumOrders = Sum(Task.Orders)
                        Select New Month_Tasks_Orders With {.YearNum = YearNum, .MonthNum = MonthNum, .Tasks = SumTasks, .Orders = SumOrders}).ToList
            End Using
        End Function


        Private Function GetStackedColumnSeriesMonth() As IEnumerable(Of StackedColumnSeries)
            Dim TmpData = GetDataByMonth()
            Return (From t In TmpData
                    Group New MeasureModel(New DateTime(t.YearNum, t.MonthNum, 1), t.Tasks) By t Into ToList
                    Select New StackedColumnSeries With {
                        .Tag = 1,
                        .Fill = ConvertIntToBrush(1),
                        .StackMode = StackMode,
                        .Title = "Задачи",
                        .Values = New ChartValues(Of MeasureModel)(ToList.OrderBy(Function(m) m.XDate))}).Concat(
                        From t In TmpData
                        Group New MeasureModel(New DateTime(t.YearNum, t.MonthNum, 1), t.Orders) By t Into ToList
                        Select New StackedColumnSeries With {
                            .Tag = 2,
                            .Fill = ConvertIntToBrush(2),
                            .StackMode = StackMode,
                            .Title = "ЗнП",
                            .Values = New ChartValues(Of MeasureModel)(ToList.OrderBy(Function(m) m.XDate))}).ToList
        End Function


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