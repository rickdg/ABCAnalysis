﻿Imports System.Collections.ObjectModel
Imports ABCAnalysis.AbcCalculator
Imports FirstFloor.ModernUI.Presentation
Imports FirstFloor.ModernUI.Windows.Controls
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
        Public Property Subinventories As New ObservableCollection(Of SubinventoryItem)
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
        Public ReadOnly Property CmdRunHeuristics As ICommand = New RelayCommand(AddressOf HeuristicsExecute)
        Private Sub HeuristicsExecute(parameter As Object)
            Dim Dlg As New ModernDialog
            Dlg.Content = New Heuristics(Dlg) With {.Temp = Me}
            Dlg.ShowDialog()
        End Sub
        <JsonIgnore>
        Public ReadOnly Property CmdRunCalculateAbc As ICommand = New RelayCommand(AddressOf CalculateAbcExecute)
        Private Sub CalculateAbcExecute(parameter As Object)
            Dim c As New Calculator With {.Temp = Me}
            c.Calculate()
        End Sub
        <JsonIgnore>
        Public ReadOnly Property CmdRemove As ICommand = New RelayCommand(Sub() Parent.Templates.Remove(Me))
#End Region


        Public Sub UpdateSubinventories()
            Using Context As New AbcAnalysisEntities
                Dim TempCollection As New List(Of SubinventoryItem)
                Dim Data = Context.Subinventories.ToList
                For Each Item In Data
                    If Not Subinventories.Any(Function(i) i.Name = Item.Name) Then
                        Subinventories.Add(New SubinventoryItem With {.Parent = Me, .Name = Item.Name})
                    End If
                Next
                For Each Item In Subinventories
                    If Not Data.Any(Function(i) i.Name = Item.Name) Then
                        TempCollection.Add(Item)
                    End If
                Next
                For Each Item In TempCollection
                    Subinventories.Remove(Item)
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

                UpdateCategoryes()
            End Using
        End Sub


        Public Sub UpdateCategoryes()
            Using Context As New AbcAnalysisEntities
                Dim Data = (From td In Context.TaskDatas
                            Join c In Context.Categories On c.Id Equals td.Category_Id
                            Where Subinventories_id.Contains(td.Subinventory) AndAlso UserPositionTypes_id.Contains(td.UserPositionType_Id)
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
                    targetCollection.Add(New Named With {.Parent = Me, .Id = Item.Id, .Name = Item.Name, .Value = Item.Value})
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