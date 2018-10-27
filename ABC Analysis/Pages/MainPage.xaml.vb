Imports System.ComponentModel

Namespace Pages
    Partial Public Class MainPage
        Inherits UserControl

        Public Shared Property Model As MainPageVM
        Private ReadOnly SerializeFileName As String = "MainPage"


        Public Sub New()
            InitializeComponent()
            If FileExists(SerializeFileName) Then
                Model = Deserialize(Of MainPageVM)(SerializeFileName)
                For Each Temp In Model.Templates
                    AddHandler Temp.AbcChanged, Sub() Model.RaiseAbcChanged()
                Next
            Else
                Model = New MainPageVM With {.SerializeFileName = SerializeFileName}
            End If

            Subinventory.Items.IsLiveSorting = True
            Subinventory.Items.LiveSortingProperties.Add("Name")
            Subinventory.Items.SortDescriptions.Add(New SortDescription("Name", ListSortDirection.Ascending))

            UserPositionType.Items.IsLiveSorting = True
            UserPositionType.Items.LiveSortingProperties.Add("Value")
            UserPositionType.Items.LiveSortingProperties.Add("Name")
            UserPositionType.Items.SortDescriptions.Add(New SortDescription("Value", ListSortDirection.Descending))
            UserPositionType.Items.SortDescriptions.Add(New SortDescription("Name", ListSortDirection.Ascending))

            Category.Items.IsLiveSorting = True
            Category.Items.LiveSortingProperties.Add("Value")
            Category.Items.LiveSortingProperties.Add("Name")
            Category.Items.SortDescriptions.Add(New SortDescription("Value", ListSortDirection.Descending))
            Category.Items.SortDescriptions.Add(New SortDescription("Name", ListSortDirection.Ascending))

            DataContext = Model
        End Sub

    End Class
End Namespace