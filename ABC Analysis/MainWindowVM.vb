Imports System.Reflection
Imports FirstFloor.ModernUI.Presentation
Imports Newtonsoft.Json

Public Class MainWindowVM

    Private _ThemeSource As Uri
    Private _AccentColor As Color
    Private ReadOnly AppName As String = Assembly.GetExecutingAssembly.GetName.Name
    Private ReadOnly AppVersion As String = Assembly.GetExecutingAssembly.GetName.Version.ToString

    <JsonIgnore>
    Public ReadOnly Property Title As String
        Get
            Return $"{AppName} {AppVersion}"
        End Get
    End Property
    Public Property ThemeSource As Uri
        Get
            Return _ThemeSource
        End Get
        Set
            _ThemeSource = Value
            AppearanceManager.Current.ThemeSource = Value
        End Set
    End Property
    Public Property AccentColor As Color
        Get
            Return _AccentColor
        End Get
        Set
            _AccentColor = Value
            AppearanceManager.Current.AccentColor = Value
        End Set
    End Property
    Public Property Height As Double
    Public Property Width As Double
    Public Property WindowState As WindowState
    Public Property Left As Double
    Public Property Top As Double

End Class