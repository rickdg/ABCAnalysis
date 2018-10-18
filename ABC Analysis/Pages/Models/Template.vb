Imports ABCAnalysis.AbcCalculator
Imports FirstFloor.ModernUI.Presentation

Namespace Pages
    Public Class Template
        Inherits NotifyPropertyChanged

        Private _FinalDate As Date


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
                Return FinalDate.AddDays(RunInterval)
            End Get
        End Property
        Public Property IsCalculated As Boolean
        Public Property RunInterval As Integer
        Public Property BillingPeriod As Integer
        Public Overridable Property QuantityAClass As Integer
        Public Overridable Property QuantityBClass As Integer
        Public Property QuantityABClass As Integer
        Public Property ReductionPickPercentText As String
            Get
                Return ReductionPickPercent.Text
            End Get
            Set
                ReductionPickPercent.Text = Value
                UpdateSettings()
            End Set
        End Property
        Public Property ReductionPickPercent As New Percentage
#End Region


#Region "Result"
        Public Property AvgPickPercent As Double
        Public Property Transition As Integer
        Public Property HeuristicsResult As IEnumerable(Of ResultCalculation)
#End Region


#Region "Filters"
        Public Overridable ReadOnly Property Subinventories_id As IEnumerable(Of Integer)
        Public Overridable ReadOnly Property UserPositionTypes_id As IEnumerable(Of Integer)
        Public Overridable ReadOnly Property Categoryes_id As IEnumerable(Of Integer)
#End Region


        Public Sub UpdateSettings()
            If HeuristicsResult Is Nothing Then Return
            Dim TargetPickPercent = HeuristicsResult.Max(Function(j) j.AvgPickPercent) - ReductionPickPercent.Value2
            Dim Result = (From Rc In HeuristicsResult
                          Where Rc.AvgPickPercent >= TargetPickPercent
                          Order By Rc.Transition Ascending, Rc.AvgPickPercent Descending).First
            RunInterval = Result.Interval
            BillingPeriod = Result.Period
            AvgPickPercent = Result.AvgPickPercent
            Transition = Result.Transition
            OnPropertyChanged("RunInterval")
            OnPropertyChanged("BillingPeriod")
            OnPropertyChanged("AvgPickPercent")
            OnPropertyChanged("Transition")
            OnPropertyChanged("NextFinalDate")
        End Sub

    End Class
End Namespace