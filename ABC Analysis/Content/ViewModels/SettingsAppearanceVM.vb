Imports ABCAnalysis.Pages
Imports FirstFloor.ModernUI.Presentation
Imports System.ComponentModel

Namespace Content
    Public Class SettingsAppearanceVM
        Inherits NotifyPropertyChanged

        Private _SelectedAccentColor As Color
        Private _SelectedTheme As Link


        Public Sub New()
            Themes.Add(New Link With {
                .DisplayName = "Темная",
                .Source = AppearanceManager.DarkThemeSource})
            Themes.Add(New Link With {
                .DisplayName = "Светлая",
                .Source = AppearanceManager.LightThemeSource})
            SyncThemeAndColor()
            AddHandler AppearanceManager.Current.PropertyChanged, AddressOf OnAppearanceManagerPropertyChanged
        End Sub


        Public ReadOnly Property AccentColors As Color() = New Color() {
            Color.FromRgb(&H33, &H99, &HFF),
            Color.FromRgb(&H0, &HAB, &HA9),
            Color.FromRgb(34, 116, 71),
            Color.FromRgb(&H33, &H99, &H33),
            Color.FromRgb(&H8C, &HBF, &H26),
            Color.FromRgb(&HF0, &H96, &H9),
            Color.FromRgb(&HFF, &H45, &H0),
            Color.FromRgb(&HE5, &H14, &H0),
            Color.FromRgb(&HA2, &H0, &HFF)}
        Public Property SelectedAccentColor As Color
            Get
                Return _SelectedAccentColor
            End Get
            Set
                If _SelectedAccentColor <> Value Then
                    _SelectedAccentColor = Value
                    MainWindow.Model.AccentColor = Value
                    OnPropertyChanged("SelectedAccentColor")
                    DataManager.Model.RefreshColorSeries()
                End If
            End Set
        End Property
        Public ReadOnly Property Themes As New LinkCollection
        Public Property SelectedTheme As Link
            Get
                Return _SelectedTheme
            End Get
            Set
                If _SelectedTheme IsNot Value Then
                    _SelectedTheme = Value
                    MainWindow.Model.ThemeSource = Value.Source
                    OnPropertyChanged("SelectedTheme")
                End If
            End Set
        End Property


        Private Sub SyncThemeAndColor()
            _SelectedTheme = Themes.FirstOrDefault(Function(l) l.Source.Equals(AppearanceManager.Current.ThemeSource))
            _SelectedAccentColor = AppearanceManager.Current.AccentColor
        End Sub


        Private Sub OnAppearanceManagerPropertyChanged(ByVal sender As Object, ByVal e As PropertyChangedEventArgs)
            If e.PropertyName = "ThemeSource" OrElse e.PropertyName = "AccentColor" Then
                SyncThemeAndColor()
            End If
        End Sub

    End Class
End Namespace