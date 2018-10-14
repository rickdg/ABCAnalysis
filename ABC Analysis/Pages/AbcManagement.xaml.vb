Imports System.Collections.ObjectModel
Imports System.Collections.Specialized
Imports System.ComponentModel
Imports System.Data
Imports ABCAnalysis.Content
Imports ABCAnalysis.ExcelConnection
Imports FirstFloor.ModernUI.Presentation
Imports FirstFloor.ModernUI.Windows.Controls
Imports LiveCharts
Imports LiveCharts.Configurations

Namespace Pages
    Partial Public Class AbcManagement
        Inherits UserControl
        Implements INotifyPropertyChanged

        Private _Labels As String()


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


        Public Property AbcGroups As ObservableCollection(Of AbcGroupVM) = MainPage.Model.AbcGroups
        Public Property AbcGroup_id As Integer
        Public Property AbcGroupsSeries As New ChartValues(Of AbcGroup_Count)
        Public Property Labels As String()
            Get
                Return _Labels
            End Get
            Set
                _Labels = Value
                RaisePropertyChanged("Labels")
            End Set
        End Property


        Public ReadOnly Property CmdAddNewEntity As ICommand = New RelayCommand(AddressOf AddNewEntityExecute)
        Private Sub AddNewEntityExecute(parameter As Object)
            Using Context As New AbcAnalysisEntities
                Dim NewEntity = Context.AbcGroups.Add(New AbcGroup)
                Context.SaveChanges()
                AbcGroups.Add(New AbcGroupVM With {.ParentCollection = AbcGroups, .Entity = NewEntity})
            End Using
        End Sub
        Public ReadOnly Property CmdLoadAbc As ICommand = New RelayCommand(AddressOf LoadAbcExecute)
        Private Sub LoadAbcExecute(parameter As Object)
            Dim Dlg As New ModernDialog
            Dlg.Content = New DataLoader(Dlg) With {
                .LoadType = LoadType.AbcData,
                .CmdParameters = {New CommandParameter With {
                    .Name = "@AbcGroup_id",
                    .SqlDbType = SqlDbType.Int,
                    .Value = AbcGroup_id}},
                .ProcParameters = New StoredProcedureParameters With {
                    .CommandText = "dbo.LoadAbc",
                    .ParameterName = "@ExcelTable",
                    .TypeName = "ExcelAbcTable"}}
            Dlg.ShowDialog()
            If Dlg.DialogResult Then RefreshAbcGroupsSeries()
        End Sub


        Private Sub CollectionChanged(sender As Object, e As NotifyCollectionChangedEventArgs)
            If e.Action = NotifyCollectionChangedAction.Remove Then RefreshAbcGroupsSeries()
        End Sub


        Private Sub RefreshAbcGroupsSeries()
            AbcGroupsSeries.Clear()
            Dim Data = GetData()
            For Each Item In Data
                AbcGroupsSeries.Add(Item)
            Next
            Labels = Data.Select(Function(i) i.Title).ToArray
        End Sub


        Private Function GetData() As IEnumerable(Of AbcGroup_Count)
            Using Context As New AbcAnalysisEntities
                Return (From Abc In Context.AbcCodeItems
                        Join AbcGroup In Context.AbcGroups On AbcGroup.Id Equals Abc.AbcGroup_id
                        Group By AbcGroup.Name Into Count
                        Select New AbcGroup_Count With {.Title = Name, .CountItem = Count}).ToList
            End Using
        End Function


        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged


        Private Sub RaisePropertyChanged(ByVal propName As String)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propName))
        End Sub

    End Class
End Namespace