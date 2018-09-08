Imports FirstFloor.ModernUI.Presentation
Imports Newtonsoft.Json

Namespace Pages
    Public Class TemplateVM

        Private _QuantityAClass As UShort
        Private _QuantityBClass As UShort
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
        Public Property Name As String
        Public Property Parent As MainPageVM


#Region "ABC parameters"
        Public Property AbcGroup_id As Integer
        Public Property RunInterval As Byte
        Public Property BillingPeriod As Byte
        Public Property QuantityAClass As UShort
            Get
                Return _QuantityAClass
            End Get
            Set
                _QuantityAClass = Value
                QuantityABClass = _QuantityAClass + _QuantityBClass
            End Set
        End Property
        Public Property QuantityBClass As UShort
            Get
                Return _QuantityBClass
            End Get
            Set
                _QuantityBClass = Value
                QuantityABClass = _QuantityAClass + _QuantityBClass
            End Set
        End Property
        Public Property QuantityABClass As Integer
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
#End Region


#Region "Threshold"
        Public Property UpperThresholdAB As New ThresholdClass
        Public Property LowerThresholdAB As New ThresholdClass
        Public Property UpperThresholdBC As New ThresholdClass
        Public Property LowerThresholdBC As New ThresholdClass
#End Region


#Region "Filters"
        Public Property SalesOrder As Boolean
        Public Property Subinventories As New List(Of Named(Of Integer))
        Public Property UserPositionTypes As New List(Of Named(Of String))
        Public Property Categoryes As New List(Of Named(Of String))
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
            targetCollection.Sort(New Comparer(Of Named(Of TKey), TKey)())
        End Sub
    End Class
End Namespace