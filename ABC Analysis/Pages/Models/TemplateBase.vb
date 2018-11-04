Imports ABCAnalysis.AbcCalculator
Imports FirstFloor.ModernUI.Presentation
Imports Newtonsoft.Json

Namespace Pages
    Public Class TemplateBase
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
                OnPropertyChanged("NextCalculationData")
            End Set
        End Property
        <JsonIgnore>
        Public ReadOnly Property NextCalculationData As Date
            Get
                Return FinalDate.AddDays(RunInterval + 3)
            End Get
        End Property
        Public Property IsCalculated As Boolean
        Public Property RunInterval As Integer
        Public Property BillingPeriod As Integer
        <JsonIgnore>
        Public ReadOnly Property BillingPeriodForCalculate As Integer
            Get
                Return BillingPeriod - 1
            End Get
        End Property
        Public Overridable Property QuantityAClass As Integer
        Public Overridable Property QuantityBClass As Integer
        Public Property QuantityABClass As Integer
        <JsonIgnore>
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
        Public Property AllTransition As Integer
        Public Property CalculateResult As IEnumerable(Of ResultCalculation)
#End Region


#Region "Filters"
        Public Overridable ReadOnly Property Subinventories_id As IEnumerable(Of Integer)
        Public Overridable ReadOnly Property UserPositionTypes_id As IEnumerable(Of Integer)
        Public Overridable ReadOnly Property Categoryes_id As IEnumerable(Of Integer)
#End Region


        Public Sub UpdateSettings()
            If CalculateResult Is Nothing Then Return
            Dim TargetPickPercent = CalculateResult.Max(Function(j) j.AvgPickPercent) - ReductionPickPercent.Value2

            Dim Result = (From Rc In CalculateResult
                          Where Rc.AvgPickPercent >= TargetPickPercent
                          Order By Rc.Transition Ascending, Rc.AvgPickPercent Descending).First

            RunInterval = Result.Interval
            BillingPeriod = Result.Period
            AvgPickPercent = Result.AvgPickPercent
            Transition = Result.Transition
            AllTransition = Result.AllTransition

            OnPropertyChanged("RunInterval")
            OnPropertyChanged("BillingPeriod")
            OnPropertyChanged("AvgPickPercent")
            OnPropertyChanged("Transition")
            OnPropertyChanged("AllTransition")
            OnPropertyChanged("NextFinalDate")
        End Sub

    End Class
End Namespace