﻿Namespace Pages
    Public MustInherit Class Template

        Public Property Name As String


#Region "ABC parameters"
        Public Property AbcGroup_id As Integer
        Public Property RunInterval As Byte
        Public Property BillingPeriod As Byte
        Public MustOverride Property QuantityAClass As Integer
        Public MustOverride Property QuantityBClass As Integer
        Public Property QuantityABClass As Integer
        Public MustOverride ReadOnly Property GetValueFunc As Func(Of TaskData, Integer)
#End Region


#Region "Threshold"
        Public Property UpperThresholdAB As Threshold
        Public Property LowerThresholdAB As Threshold
        Public Property UpperThresholdBC As Threshold
        Public Property LowerThresholdBC As Threshold
#End Region


#Region "Filters"
        Public Property SalesOrder As Boolean
        Public MustOverride ReadOnly Property IsSalesOrderFunc As Func(Of TaskData, Boolean)
        Public MustOverride ReadOnly Property Subinventories_id As IEnumerable(Of Integer)
        Public MustOverride ReadOnly Property UserPositionTypes_id As IEnumerable(Of Integer)
        Public MustOverride ReadOnly Property Categoryes_id As IEnumerable(Of Integer)
#End Region

    End Class
End Namespace