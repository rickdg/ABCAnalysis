Imports FirstFloor.ModernUI.Windows.Controls
Imports FirstFloor.ModernUI.Presentation
Imports System.IO
Imports System.Reflection

Partial Public Class MainWindow
    Inherits ModernWindow

    Public Shared Property Model As MainWindowVM
    Private SerializeFileName As String = "MainSettings"


    Public Sub New()
        InitializeComponent()
        BaseDirectory = New DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory)
        MyDocumentsDirectory = New DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                                              "NetApps",
                                                              Assembly.GetExecutingAssembly().GetName.Name))
        If FileExists(SerializeFileName) Then
            Model = Deserialize(Of MainWindowVM)(SerializeFileName)
        Else
            Model = New MainWindowVM With {.Height = 525, .Width = 1012, .Top = 100, .Left = 300}
        End If
        DataContext = Model
    End Sub


    Private Sub ModernWindow_Closing(sender As Object, e As ComponentModel.CancelEventArgs)
        Model.ThemeSource = AppearanceManager.Current.ThemeSource
        Model.AccentColor = AppearanceManager.Current.AccentColor
        Serialize(Model, SerializeFileName)
    End Sub

End Class