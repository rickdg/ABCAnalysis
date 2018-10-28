Imports FirstFloor.ModernUI.Presentation
Imports Newtonsoft.Json
Imports System.IO
Imports Microsoft.SqlServer.Management.Smo
Imports System.Collections.Specialized
Imports System.Data
Imports System.Reflection
Imports System.Collections.ObjectModel

Namespace Pages
    Public Class Project
        Inherits NotifyPropertyChanged

        Private Const srvName = "(LocalDB)\MSSQLLocalDB"
        Private Const dbMdf = "AbcAnalysis.mdf"
        Private Const dbLdf = "AbcAnalysis_log.ldf"
        Private ReadOnly dbName As String = Replace(Assembly.GetExecutingAssembly.GetName.Name, " ", "")
        Private _IsSelected As Boolean


        Public Property Parent As MainWindowModel
        Public Property IsSelected As Boolean
            Get
                Return _IsSelected
            End Get
            Set
                _IsSelected = Value
                OnPropertyChanged("IsSelected")
            End Set
        End Property
        Public Property Index As Integer
        Public Property Name As String
        Public Property Templates As New ObservableCollection(Of TemplateVM)


        <JsonIgnore>
        Private ReadOnly Property TargetDbName As String
            Get
                Return $"{dbName}{Index}"
            End Get
        End Property
        <JsonIgnore>
        Private ReadOnly Property TargetMdf As String
            Get
                Return $"{TargetDbName}.mdf"
            End Get
        End Property
        <JsonIgnore>
        Private ReadOnly Property TargetLdf As String
            Get
                Return $"{TargetDbName}_log.ldf"
            End Get
        End Property
        <JsonIgnore>
        Private ReadOnly Property AttachDbFilename As String
            Get
                Return Path.Combine(MyDocumentsDirectory.FullName, TargetMdf)
            End Get
        End Property
        <JsonIgnore>
        Public ReadOnly Property SqlConnectionString As String
            Get
                Return $"Data Source={srvName};AttachDbFilename={AttachDbFilename};Integrated Security=True"
            End Get
        End Property
        <JsonIgnore>
        Public ReadOnly Property Context As AbcAnalysisEntities
            Get
                Dim NewContext As New AbcAnalysisEntities
                Dim cs = $"data source={srvName};attachdbfilename={AttachDbFilename};integrated security=True;MultipleActiveResultSets=True;App=EntityFramework"
                NewContext.Database.Connection.ConnectionString = cs
                Return NewContext
            End Get
        End Property
        <JsonIgnore>
        Public ReadOnly Property CmdRemove As ICommand = New RelayCommand(Sub()
                                                                              Parent.Projects.Remove(Me)
                                                                              Entity.Database.Delete(TargetDbName)
                                                                              If IsSelected Then
                                                                                  Dim Item = Parent.Projects.FirstOrDefault
                                                                                  If Item IsNot Nothing Then Item.IsSelected = True
                                                                              End If
                                                                          End Sub)


        Public Sub AttachDb()
            Dim Server As New Server(srvName)

            For Each Item As Database In Server.Databases
                If Item.Name = TargetDbName Then
                    If File.Exists(AttachDbFilename) Then
                        Return
                    Else
                        Entity.Database.Delete(TargetDbName)
                    End If
                End If
            Next

            Dim sc As New StringCollection From {
                    MoveFile(dbMdf, TargetMdf),
                    MoveFile(dbLdf, TargetLdf)}

            Server.AttachDatabase(TargetDbName, sc, AttachOptions.None)
        End Sub


        Private Function MoveFile(fromFile As String, toFile As String) As String
            fromFile = Path.Combine(BaseDirectory.FullName, fromFile)
            toFile = Path.Combine(MyDocumentsDirectory.FullName, toFile)

            If Not File.Exists(toFile) Then File.Copy(fromFile, toFile)

            Return toFile
        End Function

    End Class
End Namespace