Imports System.Data
Imports System.Data.OleDb
Imports ABCAnalysis.Content
Imports FirstFloor.ModernUI.Windows.Controls
Imports Microsoft.Win32
Imports OfficeOpenXml
Imports OfficeOpenXml.Table

Namespace ExcelConnection
    Public Module DataValidation

        Public Sub ViewData(loadType As LoadType)
            Dim DialogWindow As New OpenFileDialog With {.Title = "Выбрать файл"}
            If Not DialogWindow.ShowDialog Then Exit Sub
            Try
                Using Connection As New OleDbConnection($"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={DialogWindow.FileName};Extended Properties='Excel 12.0;HDR=YES';")
                    Connection.Open()
                    Dim Table = (From Row In Connection.GetSchema("Columns")
                                 Group New Column(Row.Field(Of String)("COLUMN_NAME"), Row.Field(Of Integer)("DATA_TYPE"))
                                  By TableName = Row.Field(Of String)("TABLE_NAME").Trim("'"c) Into Columns = ToList
                                 Where TableName.EndsWith("$")
                                 Select New Table(TableName) With {.Columns = Columns}).First

                    Dim SQL = GetScript(loadType, Table.Name)
                    Dim CheckResult = CheckColumns(loadType, Table.Columns)
                    If CheckResult <> "" Then Throw New ArgumentException(CheckResult)

                    Dim ExcelTable As New DataTable
                    Using Adapter As New OleDbDataAdapter(SQL, Connection)
                        Dim RecordCount = Adapter.Fill(ExcelTable)
                        If RecordCount = 0 Then Throw New ArgumentException("Запрос вернул пустые строки")
                    End Using

                    Dim NewFile = GetInBaseFileInfo(GetInBaseDirectoryInfo("Validation"), $"{loadType.ToString}.xlsx")
                    Using Package As New ExcelPackage(NewFile)
                        Dim Sheet = Package.Workbook.Worksheets.Add(loadType.ToString)
                        Sheet.Cells("A1").LoadFromDataTable(ExcelTable, True, TableStyles.Light9)
                        Sheet.Cells.AutoFitColumns()
                        Package.Save()
                    End Using
                    Process.Start(NewFile.FullName)
                End Using
            Catch ex As Exception
                Dim Dlg As New ModernDialog With {.Title = "Сообщение", .Content = New ErrorMessage(ex)}
                Dlg.ShowDialog()
            End Try
        End Sub


        Public Sub ViewCollection(Of T)(name As String, collection As IEnumerable(Of T))
            Dim NewFile = GetInBaseFileInfo(GetInBaseDirectoryInfo("Validation"), $"{name}.xlsx")
            Using Package As New ExcelPackage(NewFile)
                Dim Sheet = Package.Workbook.Worksheets.Add(name)
                Sheet.Cells("A1").LoadFromCollection(collection, True, TableStyles.Light9)
                Sheet.Cells.AutoFitColumns()
                Package.Save()
            End Using
            Process.Start(NewFile.FullName)
        End Sub


        Public Sub ViewArray(name As String, arr As IEnumerable(Of Object()))
            Dim NewFile = GetInBaseFileInfo(GetInBaseDirectoryInfo("Validation"), $"{name}.xlsx")
            Using Package As New ExcelPackage(NewFile)
                Dim Sheet = Package.Workbook.Worksheets.Add(name)
                Sheet.Cells("A1").LoadFromArrays(arr)
                Sheet.Cells.AutoFitColumns()
                Package.Save()
            End Using
            Process.Start(NewFile.FullName)
        End Sub

    End Module
End Namespace