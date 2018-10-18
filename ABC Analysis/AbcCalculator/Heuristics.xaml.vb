﻿Imports System.Collections.Concurrent
Imports System.ComponentModel
Imports System.Data.Entity.SqlServer
Imports System.Text
Imports ABCAnalysis.Pages
Imports FirstFloor.ModernUI.Windows.Controls

Namespace AbcCalculator
    Partial Public Class Heuristics
        Inherits UserControl
        Implements INotifyPropertyChanged

        Private Dialog As ModernDialog
        Private _ProgressValue As Integer


        Public Sub New(dlg As ModernDialog)
            InitializeComponent()
            DataContext = Me
            Dialog = dlg
            Dialog.Title = "Эвристический анализ"
            Dialog.Buttons.First.Visibility = Visibility.Collapsed
            Task.Factory.StartNew(Sub() Calculate())
        End Sub


        Public Property Temp As Template
        Public Property ProgressValue As Integer
            Get
                Return _ProgressValue
            End Get
            Set
                _ProgressValue = Value
                RaisePropertyChanged("ProgressValue")
            End Set
        End Property


        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged


        Private Sub RaisePropertyChanged(ByVal propName As String)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propName))
        End Sub


        Public Sub Calculate()
            Dim InitialDate As Date
            Dim FinalDate As Date
            Dim Data As IEnumerable(Of TaskDataExtend)
            Dim HeuristicsResult As New BlockingCollection(Of ResultCalculation)

            Try
                Using Context As New AbcAnalysisEntities
                    If Context.TaskDatas.FirstOrDefault Is Nothing Then Throw New Exception("Нет данных для расчета.")
                    InitialDate = Context.TaskDatas.Min(Function(i) i.XDate)
                    FinalDate = Context.TaskDatas.Where(Function(i) CBool(SqlFunctions.DatePart("Weekday", i.XDate) = 6)).Max(Function(i) i.XDate)
                    Data = (From td In Context.TaskDatas
                            Join ci In Context.CodeItems On ci.Id Equals td.CodeItem_id
                            Where td.XDate <= FinalDate AndAlso Temp.Subinventories_id.Contains(td.Subinventory)
                            Select New TaskDataExtend With {
                                .XDate = td.XDate,
                                .Code = td.Code,
                                .Category_Id = ci.Category_Id,
                                .UserPositionType_Id = ci.UserPositionType_Id,
                                .SalesOrder = td.SalesOrder,
                                .Orders = td.Orders,
                                .Tasks = td.Tasks}).ToList
                End Using

                Dim CalculationData = (From d In Data
                                       Where Temp.UserPositionTypes_id.Contains(d.UserPositionType_Id) AndAlso
                                           Temp.Categoryes_id.Contains(d.Category_Id) AndAlso d.SalesOrder
                                       Select New DataItem With {.XDate = d.XDate, .Code = d.Code, .Value = d.Orders}).ToList

                Parallel.ForEach(GetTemplates,
                                 Sub(t)
                                     Dim Calculator As New TestCalculator With {
                                     .Temp = t,
                                     .InitialDate = InitialDate,
                                     .FinalDate = FinalDate,
                                     .Data = Data,
                                     .CalculationData = CalculationData}

                                     Calculator.Calculate()
                                     HeuristicsResult.Add(New ResultCalculation(Calculator.Temp, Calculator.PickQtyTasks, Calculator.ClassChange))
                                     ProgressValue += 1
                                 End Sub)

                Temp.HeuristicsResult = HeuristicsResult.ToList
                Temp.UpdateSettings()
            Catch ex As Exception
                Dispatcher.Invoke(Sub()
                                      Dialog.Title = "Сообщение"
                                      Message.BBCode = GetInnerException(ex)
                                      Warning.Visibility = Visibility.Visible
                                      Indicator.Visibility = Visibility.Collapsed
                                  End Sub)
            Finally
                Dispatcher.Invoke(Sub() Dialog.Buttons.First.Visibility = Visibility.Visible)
            End Try
        End Sub


        Private Function GetTemplates() As IEnumerable(Of Template)
            Return (From RunInterval In {7, 14, 21, 28, 35, 42}
                    From BillingPeriod In {21, 28, 35, 42, 49, 56, 63, 70, 77, 84, 91, 98, 105, 112, 119, 126, 133, 140, 147, 154, 161, 168, 175, 182, 189, 196, 203}
                    Select New Template With {
                       .RunInterval = RunInterval,
                       .BillingPeriod = BillingPeriod,
                       .QuantityAClass = Temp.QuantityAClass,
                       .QuantityBClass = Temp.QuantityBClass,
                       .QuantityABClass = Temp.QuantityABClass}).ToList
        End Function


        Private Function GetInnerException(ex As Exception) As String
            Dim Result As New StringBuilder
            Result.Append(ex.Message & vbCrLf & vbCrLf)
            If ex.InnerException IsNot Nothing Then
                Result.Append(GetInnerException(ex.InnerException))
            End If
            Return Result.ToString
        End Function

    End Class
End Namespace