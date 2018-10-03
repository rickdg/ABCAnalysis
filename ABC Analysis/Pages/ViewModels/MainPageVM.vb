Imports System.Collections.ObjectModel
Imports System.Data.Entity.SqlServer
Imports ABCAnalysis.AbcCalculator
Imports ABCAnalysis.Content
Imports FirstFloor.ModernUI.Presentation
Imports Newtonsoft.Json

Namespace Pages
    Public Class MainPageVM
        Inherits NotifyPropertyChanged

        Private _CurrentTemplate As TemplateVM


        Public Sub New()
            Using Context As New AbcAnalysisEntities
                For Each AbcGroup In Context.AbcGroups
                    AbcGroups.Add(New AbcGroupVM() With {.Entity = AbcGroup})
                Next
            End Using
        End Sub


#Region "ViewModel"
        <JsonIgnore>
        Public Property AbcGroups As New ObservableCollection(Of AbcGroupVM)


#Region "Serialize"
        Public Property SerializeFileName As String
        Public Property Templates As New ObservableCollection(Of TemplateVM)
        Public Property CurrentTemplate As TemplateVM
            Get
                Return _CurrentTemplate
            End Get
            Set
                _CurrentTemplate = Value
                OnPropertyChanged("CurrentTemplate")
            End Set
        End Property
#End Region


#Region "Commands"
        <JsonIgnore>
        Public ReadOnly Property CmdSave As ICommand = New RelayCommand(Sub() Serialize(Me, SerializeFileName))
        <JsonIgnore>
        Public ReadOnly Property CmdRunCalculate As ICommand = New RelayCommand(AddressOf CalculateExecute)
        Private Sub CalculateExecute(parameter As Object)
            For Each Tmp In Templates.Where(Function(i) i.Activated = True).Select(Function(i) i.GetTemplate).ToList
                Task.Factory.StartNew(Sub() RunCalculate(Tmp))
            Next
        End Sub
        Private Sub RunCalculate(tmp As Template)
            Dim Stopwatch As New Stopwatch
            Stopwatch.Start()
            Using Context As New AbcAnalysisEntities
                Dim InitialDate = Context.TaskDatas.Min(Function(i) i.XDate)
                Dim FinalDate = Context.TaskDatas.Where(Function(i) CBool(SqlFunctions.DatePart("Weekday", i.XDate) = 6)).Max(Function(i) i.XDate)
                Dim Calculator As New ParallelTestCalculator With {
                    .Temp = tmp,
                    .InitialDate = InitialDate,
                    .FinalDate = FinalDate,
                    .Data = Context.TaskDatas.Where(Function(i) i.XDate <= FinalDate AndAlso tmp.Subinventories_id.Contains(i.Subinventory)).ToList}
                Calculator.Calculate()
            End Using
            Stopwatch.Stop()
            MsgBox(Stopwatch.Elapsed.TotalSeconds)
        End Sub
        <JsonIgnore>
        Public ReadOnly Property CmdAddNewTemplate As ICommand = New RelayCommand(Sub() Templates.Add(New TemplateVM(True) With {.Parent = Me}))
#End Region
#End Region


        Public Sub UpdateTemplates()
            For Each Tmp In Templates
                Tmp.UpdateLists()
            Next
        End Sub

    End Class
End Namespace