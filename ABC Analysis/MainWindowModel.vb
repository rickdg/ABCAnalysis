﻿Imports System.Collections.ObjectModel
Imports System.Reflection
Imports ABCAnalysis.Pages
Imports FirstFloor.ModernUI.Presentation
Imports Newtonsoft.Json

Public Class MainWindowModel

    Private ReadOnly AppName As String = Assembly.GetExecutingAssembly.GetName.Name
    Private _AppVersion As Version
    Private _ThemeSource As Uri
    Private _AccentColor As Color


    <JsonIgnore>
    Public ReadOnly Property Title As String
        Get
            Return $"{AppName} {AppVersion}"
        End Get
    End Property
    <JsonIgnore>
    Public Property IsNewVersion As Boolean
    <JsonIgnore>
    Public Property OldRevision As Integer
    <JsonIgnore>
    Public Property NewRevision As Integer


#Region "Serialize"
    Public Property AppVersion As Version
        Get
            Return _AppVersion
        End Get
        Set
            Dim CurrentVersion = Assembly.GetExecutingAssembly.GetName.Version
            If CurrentVersion.Equals(Value) Then
                _AppVersion = Value
                IsNewVersion = False
            Else
                _AppVersion = CurrentVersion
                OldRevision = Value.Revision + 1
                NewRevision = CurrentVersion.Revision
                IsNewVersion = True
                ExecuteUpdate(OldRevision, NewRevision)
            End If
        End Set
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
    Public Property HighlightingDefinition As String
    Public Property Height As Double
    Public Property Width As Double
    Public Property WindowState As WindowState
    Public Property Left As Double
    Public Property Top As Double


    Public Property Projects As New ObservableCollection(Of Project)
    Public Property ProjectCounter As Integer
#End Region

End Class