Imports FirstFloor.ModernUI.Presentation

Namespace Pages
    Public Class Template
        Inherits NotifyPropertyChanged

        Private _FinalDate As Date
        Private _RunInterval As Integer
        Private _BillingPeriod As Integer
        Private _AvgPickPercent As Double
        Private _Transition As Integer


        Public Property Name As String


#Region "ABC parameters"
        Public Property AbcGroup_id As Integer
        Public Property FinalDate As Date
            Get
                Return _FinalDate
            End Get
            Set
                _FinalDate = Value
                OnPropertyChanged("NextFinalDate")
            End Set
        End Property
        Public ReadOnly Property NextFinalDate As Date
            Get
                Return FinalDate.AddDays(_RunInterval)
            End Get
        End Property
        Public Property IsCalculated As Boolean
        Public Property RunInterval As Integer
            Get
                Return _RunInterval
            End Get
            Set
                _RunInterval = Value
                OnPropertyChanged("RunInterval")
                OnPropertyChanged("NextFinalDate")
            End Set
        End Property
        Public Property BillingPeriod As Integer
            Get
                Return _BillingPeriod
            End Get
            Set
                _BillingPeriod = Value
                OnPropertyChanged("BillingPeriod")
            End Set
        End Property
        Public Overridable Property QuantityAClass As Integer
        Public Overridable Property QuantityBClass As Integer
        Public Property QuantityABClass As Integer
        Public Property ReductionPickPercent As New Percentage
#End Region


#Region "Result"
        Public Property AvgPickPercent As Double
            Get
                Return _AvgPickPercent
            End Get
            Set
                _AvgPickPercent = Value
                OnPropertyChanged("AvgPickPercent")
            End Set
        End Property
        Public Property Transition As Integer
            Get
                Return _Transition
            End Get
            Set
                _Transition = Value
                OnPropertyChanged("Transition")
            End Set
        End Property
#End Region


#Region "Filters"
        Public Overridable ReadOnly Property Subinventories_id As IEnumerable(Of Integer)
        Public Overridable ReadOnly Property UserPositionTypes_id As IEnumerable(Of Integer)
        Public Overridable ReadOnly Property Categoryes_id As IEnumerable(Of Integer)
#End Region

    End Class
End Namespace