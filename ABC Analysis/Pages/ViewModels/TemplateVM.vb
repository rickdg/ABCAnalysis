Imports System.Collections.ObjectModel
Imports System.Data.Entity.SqlServer
Imports ABCAnalysis.AbcCalculator
Imports FirstFloor.ModernUI.Presentation
Imports Newtonsoft.Json

Namespace Pages
    Public Class TemplateVM
        Inherits Template

        Private _QuantityAClass As Integer
        Private _QuantityBClass As Integer
        Private _ByOrders As Boolean


        Public Sub New()
        End Sub


        Public Sub New(withUpdate As Boolean)
            If withUpdate Then UpdateSubinventories()
        End Sub


        Public Property Activated As Boolean
        Public Property Parent As MainPageVM


#Region "ABC parameters"
        Public Overrides Property QuantityAClass As Integer
            Get
                Return _QuantityAClass
            End Get
            Set
                _QuantityAClass = Math.Abs(Value)
                QuantityABClass = _QuantityAClass + _QuantityBClass
            End Set
        End Property
        Public Overrides Property QuantityBClass As Integer
            Get
                Return _QuantityBClass
            End Get
            Set
                _QuantityBClass = Math.Abs(Value)
                QuantityABClass = _QuantityAClass + _QuantityBClass
            End Set
        End Property
#End Region


#Region "Filters"
        Public Property Subinventories As New ObservableCollection(Of NamedInt)
        Public Property UserPositionTypes As New ObservableCollection(Of Named)
        Public Property Categoryes As New ObservableCollection(Of Named)
        <JsonIgnore>
        Public Overrides ReadOnly Property Subinventories_id As IEnumerable(Of Integer)
            Get
                Return Subinventories.Where(Function(i) i.IsChecked = True).Select(Function(i) i.Name).ToList
            End Get
        End Property
        <JsonIgnore>
        Public Overrides ReadOnly Property UserPositionTypes_id As IEnumerable(Of Integer)
            Get
                Return UserPositionTypes.Where(Function(i) i.IsChecked = True).Select(Function(i) i.Id).ToList
            End Get
        End Property
        <JsonIgnore>
        Public Overrides ReadOnly Property Categoryes_id As IEnumerable(Of Integer)
            Get
                Return Categoryes.Where(Function(i) i.IsChecked = True).Select(Function(i) i.Id).ToList
            End Get
        End Property
#End Region


#Region "Commands"
        <JsonIgnore>
        Public ReadOnly Property CmdRunCalculate As ICommand = New RelayCommand(AddressOf CalculateExecute)
        Private Sub CalculateExecute(parameter As Object)
            Using Context As New AbcAnalysisEntities
                Dim InitialDate = Context.TaskDatas.Min(Function(i) i.XDate)
                Dim FinalDate = Context.TaskDatas.Where(Function(i) CBool(SqlFunctions.DatePart("Weekday", i.XDate) = 6)).Max(Function(i) i.XDate)
                Dim Calculator As New ParallelTestCalculator With {
                    .Temp = Me,
                    .InitialDate = InitialDate,
                    .FinalDate = FinalDate,
                    .Data = Context.TaskDatas.Where(Function(i) i.XDate <= FinalDate AndAlso Subinventories_id.Contains(i.Subinventory)).ToList}
                Calculator.Calculate()
            End Using
        End Sub
        <JsonIgnore>
        Public ReadOnly Property CmdRemove As ICommand = New RelayCommand(Sub() Parent.Templates.Remove(Me))
#End Region


        Public Sub UpdateSubinventories()
            Using Context As New AbcAnalysisEntities
                For Each Item In Context.Subinventories
                    If Not Subinventories.Any(Function(i) i.Name = Item.Name) Then
                        Subinventories.Add(New NamedInt With {.Parent = Me, .Name = Item.Name})
                    End If
                Next
            End Using
        End Sub


        Public Sub UpdateUserPositionTypesAndCategoryes()
            Using Context As New AbcAnalysisEntities
                Dim Data = (From td In Context.TaskDatas
                            Join upt In Context.UserPositionTypes On upt.Id Equals td.UserPositionType_Id
                            Where Subinventories_id.Contains(td.Subinventory)
                            Group By upt.Id, upt.Name Into Sum(td.Orders)
                            Select New DataItem With {.Id = Id, .Name = Name, .Value = Sum}).ToList
                UpdateCollection(Data, UserPositionTypes)

                Data = (From td In Context.TaskDatas
                        Join c In Context.Categories On c.Id Equals td.Category_Id
                        Where Subinventories_id.Contains(td.Subinventory)
                        Group By c.Id, c.Name Into Sum(td.Orders)
                        Select New DataItem With {.Id = Id, .Name = Name, .Value = Sum}).ToList
                UpdateCollection(Data, Categoryes)
            End Using
        End Sub


        Private Sub UpdateCollection(data As IEnumerable(Of DataItem), targetCollection As ObservableCollection(Of Named))
            Dim TempCollection As New List(Of Named)
            For Each Item In data
                Dim Tc = targetCollection.SingleOrDefault(Function(i) i.Name = Item.Name)
                If Tc Is Nothing Then
                    targetCollection.Add(New Named With {.Id = Item.Id, .Name = Item.Name, .Value = Item.Value})
                Else
                    Tc.Value = Item.Value
                End If
            Next
            For Each Item In targetCollection
                If Not data.Any(Function(i) i.Name = Item.Name) Then
                    TempCollection.Add(Item)
                End If
            Next
            For Each Item In TempCollection
                targetCollection.Remove(Item)
            Next
        End Sub

    End Class
End Namespace