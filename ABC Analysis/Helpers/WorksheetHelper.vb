Imports OfficeOpenXml
Imports OfficeOpenXml.Drawing.Chart

Public Class WorksheetHelper

    Private _RowPosition As Integer
    Private _ColumnPosition As Integer


    Public Sub New()
        Column = 1
    End Sub


    Public Property Sheet As ExcelWorksheet
    Private Property Column As Integer
    Private Property RowPosition As Integer
        Get
            Return _RowPosition
        End Get
        Set
            If IsEndChartLine Then
                _RowPosition = Value
            End If
        End Set
    End Property
    Private Property ColumnPosition As Integer
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
    Private Property IsEndChartLine As Boolean


    Public Sub AddLineChart(Of T)(collection As IEnumerable(Of T), chartTitle As String, Optional endChartLine As Boolean = False,
                                  Optional width As Integer = 896, Optional height As Integer = 380)
        IsEndChartLine = endChartLine
        Dim DataRange = Sheet.Cells(ExcelAddress.GetAddress(1, Column)).LoadFromCollection(collection)
        Column += GetType(T).GetProperties.Count
        Dim Chart = Sheet.Drawings.AddChart(chartTitle, eChartType.Line)
        Chart.Title.Text = chartTitle
        Chart.SetPosition(RowPosition, 0, ColumnPosition, 0)
        Chart.SetSize(width, height)
        Chart.Border.Fill.Transparancy = 100
        ColumnPosition += CInt(width / 64)
        RowPosition += CInt(height / 20)
        Dim Index = 0
        For Each Prop In GetType(T).GetProperties
            If Index = 0 Then
                Sheet.Column(DataRange.Start.Column).Style.Numberformat.Format = "DD.MM.YYYY"
            Else
                Chart.Series.Add(DataRange.Offset(0, Index, DataRange.End.Row, 1), DataRange.Offset(0, 0, DataRange.End.Row, 1)).Header = Prop.Name
            End If
            Index += 1
        Next
        IsEndChartLine = False
    End Sub

End Class