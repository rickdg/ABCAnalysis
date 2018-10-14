Imports OfficeOpenXml
Imports OfficeOpenXml.Drawing.Chart

Public Class WorksheetHelper

    Private _RowPosition As Integer
    Private _ColumnPosition As Integer


    Public Sub New()
        Column = 1
    End Sub


    Public Property Sheet As ExcelWorksheet
    Public Property Column As Integer
    Public Property RowPosition As Integer
        Get
            Return _RowPosition
        End Get
        Set
            If IsEndChartLine Then
                _RowPosition = Value
            End If
        End Set
    End Property
    Public Property ColumnPosition As Integer
        Get
            Return _ColumnPosition
        End Get
        Set
            If IsEndChartLine Then
                _ColumnPosition = 0
            Else
                _ColumnPosition = Value
            End If
        End Set
    End Property
    Public Property IsEndChartLine As Boolean
    Public ReadOnly Property CurrentAddress As String
        Get
            Return ExcelAddress.GetAddress(1, Column)
        End Get
    End Property


    Public Sub AddLineChart(Of T)(collection As IEnumerable(Of T), chartTitle As String, Optional endChartLine As Boolean = False,
                                  Optional width As Integer = 896, Optional height As Integer = 380)
        IsEndChartLine = endChartLine
        Dim DataRange = LoadFromCollection(collection)
        Dim Chart = Sheet.Drawings.AddChart(chartTitle, eChartType.Line)
        Chart.Title.Text = chartTitle
        Chart.SetPosition(RowPosition, 0, ColumnPosition, 0)
        Chart.SetSize(width, height)
        ColumnPosition += CInt(width / 64)
        RowPosition += cint(height / 20)
        Dim Index = 0
        For Each Prop In GetType(T).GetProperties
            If Index = 0 Then
                Sheet.Column(DataRange.Start.Column).Style.Numberformat.Format = "DD.MM.YYYY"
            Else
                Chart.Series.Add(DataRange.Offset(1, Index, DataRange.End.Row - 1, 1), DataRange.Offset(1, 0, DataRange.End.Row - 1, 1)).Header = Prop.Name
            End If
            Index += 1
        Next
        Chart.Border.Fill.Transparancy = 100
        IsEndChartLine = False
    End Sub


    Public Function LoadFromCollection(Of T)(Collection As IEnumerable(Of T)) As ExcelRangeBase
        Dim Result = Sheet.Cells(CurrentAddress).LoadFromCollection(Collection)
        Column += GetType(T).GetProperties.Count
        Return Result
    End Function

End Class