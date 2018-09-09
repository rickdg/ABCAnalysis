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
            If withUpdate Then
                UpdateLists()
                ByOrders = True
            End If
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
        Public Property ByOrders As Boolean
            Get
                Return _ByOrders
            End Get
            Set
                _ByOrders = Value
                ByTasks = Not Value
            End Set
        End Property
        Public Property ByTasks As Boolean
        <JsonIgnore>
        Public Overrides ReadOnly Property GetValueFunc As Func(Of TaskData, Integer)
            Get
                If ByOrders Then
                    Return Function(d) d.Orders
                Else
                    Return Function(d) d.Tasks
                End If
            End Get
        End Property
#End Region


#Region "Filters"
        <JsonIgnore>
        Public Overrides ReadOnly Property IsSalesOrderFunc As Func(Of TaskData, Boolean)
            Get
                If SalesOrder Then
                    Return Function(d) d.SalesOrder
                Else
                    Return Function(d) True
                End If
            End Get
        End Property
        Public Property Subinventories As New List(Of Named(Of Integer))
        Public Property UserPositionTypes As New List(Of Named(Of String))
        Public Property Categoryes As New List(Of Named(Of String))
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
                                                           targetCollection As List(Of Named(Of TKey)),
                                                           existFunc As Func(Of Named(Of TKey), TData, Boolean),
                                                           createFunc As Func(Of TData, Named(Of TKey)))
            For Each Item In sourceCollection
                If Not targetCollection.Any(Function(i) existFunc(i, Item)) Then
                    targetCollection.Add(createFunc(Item))
                End If
            Next
            targetCollection.Sort(New NamedComparer(Of Named(Of TKey), TKey)())
        End Sub


        Public Function GetTemplate() As Template
            Return Me
        End Function

    End Class
End Namespace