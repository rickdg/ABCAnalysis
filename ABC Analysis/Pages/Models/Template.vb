Namespace Pages
    Public Class Template

        Public Property Name As String


#Region "ABC parameters"
        Public Property AbcGroup_id As Integer
        Public Property RunInterval As Byte
        Public Property BillingPeriod As Byte
        Public Overridable Property QuantityAClass As Integer
        Public Overridable Property QuantityBClass As Integer
        Public Property QuantityABClass As Integer
        Public Overridable ReadOnly Property GetValueFunc As Func(Of TaskData, Integer)
#End Region


#Region "Threshold"
        Public Property UpThresholdAB As New Threshold With {.IsUp = True}
        Public Property LowThresholdAB As New Threshold
        Public Property UpThresholdBC As New Threshold With {.IsUp = True}
        Public Property LowThresholdBC As New Threshold
#End Region


#Region "Filters"
        Public Property SalesOrder As Boolean
        Public Overridable ReadOnly Property IsSalesOrderFunc As Func(Of TaskData, Boolean)
        Public Overridable ReadOnly Property Subinventories_id As IEnumerable(Of Integer)
        Public Overridable ReadOnly Property UserPositionTypes_id As IEnumerable(Of Integer)
        Public Overridable ReadOnly Property Categoryes_id As IEnumerable(Of Integer)
#End Region

    End Class
End Namespace