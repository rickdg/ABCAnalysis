Imports System.IO
Imports System.Reflection
Imports FirstFloor.ModernUI.Windows.Controls

Partial Public Class MainWindow
    Inherits ModernWindow

    Public Shared Property Model As MainWindowModel
    Private SerializeFileName As String = "MainSettings"


    Public Sub New()
        InitializeComponent()
        SetDirectory()
        DataContext = DeserializeModel()
    End Sub


    Private Sub SetDirectory()
        BaseDirectory = New DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory)
        Dim Company = DirectCast(Assembly.GetExecutingAssembly.GetCustomAttribute(GetType(AssemblyCompanyAttribute)), AssemblyCompanyAttribute).Company
        MyDocumentsDirectory = New DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                                              Company,
                                                              Assembly.GetExecutingAssembly.GetName.Name))
    End Sub


    Private Function DeserializeModel() As MainWindowModel
        If FileExists(SerializeFileName) Then
            Model = Deserialize(Of MainWindowModel)(SerializeFileName)
        Else
            Model = New MainWindowModel With {
                .Height = 525, .Width = 1155, .Top = 100, .Left = 300,
                .AppVersion = Assembly.GetExecutingAssembly.GetName.Version}
        End If
        Return Model
    End Function


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