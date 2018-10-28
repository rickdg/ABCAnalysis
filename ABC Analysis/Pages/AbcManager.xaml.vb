Imports System.Collections.ObjectModel
Imports System.Collections.Specialized
Imports System.ComponentModel
Imports System.Data
Imports ABCAnalysis.Connection.Excel
Imports ABCAnalysis.Connection.SqlServer
Imports ABCAnalysis.Content
Imports FirstFloor.ModernUI.Presentation
Imports FirstFloor.ModernUI.Windows.Controls
Imports LiveCharts
Imports LiveCharts.Configurations

Namespace Pages
    Partial Public Class AbcManager
        Inherits UserControl
        Implements INotifyPropertyChanged

        Public Sub New()
            InitializeComponent()
            Dim Mapper = Mappers.Xy(Of AbcGroup_Count).
                X(Function(i, index) index).
                Y(Function(i) i.CountItem)
            Charting.[For](Of AbcGroup_Count)(Mapper)
            DataContext = Me
            RefreshAbcGroupsSeries()
            AddHandler AbcGroups.CollectionChanged, AddressOf CollectionChanged
        End Sub


        Public Property AbcGroups As ObservableCollection(Of AbcGroupModel) = MainPage.Model.AbcGroups
        Public Property AbcGroup_id As Integer
        Public Property AbcGroupsSeries As New ChartValues(Of AbcGroup_Count)
        Public Property Labels As String()


        Public ReadOnly Property CmdAddNewEntity As ICommand = New RelayCommand(
            Sub()
                Using Context = ProjectManager.CurrentProject.Context
                    Dim NewEntity = Context.AbcGroups.Add(New AbcGroup)
                    Context.SaveChanges()
                    AbcGroups.Add(New AbcGroupModel With {.ParentCollection = AbcGroups, .Entity = NewEntity})
                End Using
            End Sub, ProjectManager.ProjectExist)
        Public ReadOnly Property CmdLoadAbc As ICommand = New RelayCommand(
            Sub()
                If AbcGroup_id = 0 Then Return
                Dim Dlg As New ModernDialog
                Dlg.Content = New LoadData(Dlg) With {
                    .LoadType = LoadType.AbcData,
                    .CmdParameters = {New CommandParameter With {
                        .Name = "@AbcGroup_id",
                        .SqlDbType = SqlDbType.Int,
                        .Value = AbcGroup_id}},
                    .ProcParameter = New StoredProcedureParameter With {
                        .CommandText = "dbo.LoadAbc",
                        .ParameterName = "@ExcelTable",
                        .TypeName = "ExcelAbcTable"}}
                Dlg.ShowDialog()
                If Dlg.DialogResult Then RefreshAbcGroupsSeries()
            End Sub, ProjectManager.ProjectExist)


        Private Sub CollectionChanged(sender As Object, e As NotifyCollectionChangedEventArgs)
            If e.Action = NotifyCollectionChangedAction.Remove Then RefreshAbcGroupsSeries()
        End Sub


        Private Sub RefreshAbcGroupsSeries()
            If ProjectManager.CurrentProject Is Nothing Then Return
            AbcGroupsSeries.Clear()
            Dim Data = GetData()
            For Each Item In Data
                AbcGroupsSeries.Add(Item)
            Next
            Labels = Data.Select(Function(i) i.Title).ToArray
            RaisePropertyChanged("Labels")
        End Sub


        Private Function GetData() As IEnumerable(Of AbcGroup_Count)
            Using Context = ProjectManager.CurrentProject.Context
                Return (From Abc In Context.AbcCodeItems
                        Join AbcGroup In Context.AbcGroups On AbcGroup.Id Equals Abc.AbcGroup_id
                        Group By AbcGroup.Name Into Count
                        Select New AbcGroup_Count With {.Title = Name, .CountItem = Count}).ToList
            End Using
        End Function


        Private Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged


        Private Sub RaisePropertyChanged(ByVal propName As String)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propName))
        End Sub


        Private Sub AbcManager_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
            If ProjectManager.IsAbcDataChanged Then
                RefreshAbcGroupsSeries()
                ProjectManager.IsAbcDataChanged = False
            End If
        End Sub

    End Class
End Namespace