Imports System.IO
Imports System.Reflection
Imports FirstFloor.ModernUI.Windows.Controls

Partial Public Class MainWindow
    Inherits ModernWindow

    Public Shared Property Model As MainWindowVM
    Private SerializeFileName As String = "MainSettings"


    Public Sub New()
        InitializeComponent()
        BaseDirectory = New DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory)
        Dim Company = DirectCast(Assembly.GetExecutingAssembly.GetCustomAttribute(GetType(AssemblyCompanyAttribute)), AssemblyCompanyAttribute).Company
        MyDocumentsDirectory = New DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                                              Company,
                                                              Assembly.GetExecutingAssembly.GetName.Name))
        'DbStartCheck()

        If FileExists(SerializeFileName) Then
            Model = Deserialize(Of MainWindowVM)(SerializeFileName)
        Else
            Model = New MainWindowVM With {
                .Height = 525, .Width = 1012, .Top = 100, .Left = 300,
                .AppVersion = Assembly.GetExecutingAssembly.GetName.Version}
        End If
        DataContext = Model
    End Sub


    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        If Model.IsNewVersion Then
            For r = Model.OldRevision To Model.NewRevision
                Revisions(r).Show()
            Next
        End If
    End Sub


    Private Sub ModernWindow_Closing(sender As Object, e As ComponentModel.CancelEventArgs)
        Serialize(Model, SerializeFileName)
    End Sub

End Class