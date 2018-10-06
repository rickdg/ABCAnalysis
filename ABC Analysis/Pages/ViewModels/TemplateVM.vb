Imports System.Collections.ObjectModel
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
            If withUpdate Then UpdateLists()
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
        Public Property Subinventories As New ObservableCollection(Of Named(Of Integer))
        Public Property UserPositionTypes As New ObservableCollection(Of Named(Of String))
        Public Property Categoryes As New ObservableCollection(Of Named(Of String))
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
        Public ReadOnly Property CmdRemove As ICommand = New RelayCommand(Sub() Parent.Templates.Remove(Me))
#End Region


        Public Sub UpdateLists()
            Using Context As New AbcAnalysisEntities
                LoadAndSort(Context.Subinventories,
                            Subinventories,
                            Function(i, Item) i.Name = Item.Name,
                            Function(Item) New Named(Of Integer) With {.Name = Item.Name})

                LoadAndSort(Context.UserPositionTypes,
                            UserPositionTypes,
                            Function(i, Item) i.Name = Item.Name,
                            Function(Item) New Named(Of String) With {.Id = Item.Id, .Name = Item.Name})

                LoadAndSort(Context.Categories,
                            Categoryes,
                            Function(i, Item) i.Name = Item.Name,
                            Function(Item) New Named(Of String) With {.Id = Item.Id, .Name = Item.Name})
            End Using
        End Sub


        Private Sub LoadAndSort(Of TData, TKey As IComparable)(sourceCollection As IQueryable(Of TData),
                                                           targetCollection As ObservableCollection(Of Named(Of TKey)),
                                                           existFunc As Func(Of Named(Of TKey), TData, Boolean),
                                                           createFunc As Func(Of TData, Named(Of TKey)))
            For Each Item In sourceCollection
                If Not targetCollection.Any(Function(i) existFunc(i, Item)) Then
                    targetCollection.Add(createFunc(Item))
                End If
            Next
            targetCollection.OrderBy(Function(i) i.Name)
        End Sub


        Public Function GetTemplate() As Template
            Return Me
        End Function

    End Class
End Namespace