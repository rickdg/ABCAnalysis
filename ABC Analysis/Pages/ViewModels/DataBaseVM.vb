Imports FirstFloor.ModernUI.Presentation
Imports Newtonsoft.Json
Imports System.IO
Imports Microsoft.SqlServer.Management.Smo
Imports System.Collections.Specialized
Imports System.Data
Imports System.Reflection

Namespace Connection.SqlServer
    Public Class DataBaseVM
        Inherits NotifyPropertyChanged

        Private Const srvName = "(LocalDB)\MSSQLLocalDB"
        Private Const dbMdf = "AbcAnalysis.mdf"
        Private Const dbLdf = "AbcAnalysis_log.ldf"
        Private ReadOnly targetDbName As String = Replace(Assembly.GetExecutingAssembly.GetName.Name, " ", "")
        Private _IsActive As Boolean


        Public Property Parent As MainWindowVM
        Public Property IsActive As Boolean
            Get
                Return _IsActive
            End Get
            Set
                _IsActive = Value
                OnPropertyChanged("IsActive")
            End Set
        End Property
        Public Property Index As Integer
        Public Property Name As String


        <JsonIgnore>
        Private ReadOnly Property TargetName As String
            Get
                Return $"{targetDbName}{Index}"
            End Get
        End Property
        <JsonIgnore>
        Private ReadOnly Property TargetMdf As String
            Get
                Return $"{TargetName}.mdf"
            End Get
        End Property
        <JsonIgnore>
        Private ReadOnly Property TargetLdf As String
            Get
                Return $"{TargetName}_log.ldf"
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
                Dim Cs = $"data source={srvName};attachdbfilename={AttachDbFilename};integrated security=True;MultipleActiveResultSets=True;App=EntityFramework"
                NewContext.Database.Connection.ConnectionString = Cs
                Return NewContext
            End Get
        End Property
        <JsonIgnore>
        Public ReadOnly Property CmdActivate As ICommand = New RelayCommand(Sub()
                                                                                For Each Item In Parent.Databases
                                                                                    Item.IsActive = False
                                                                                Next
                                                                                IsActive = True
                                                                            End Sub)
        <JsonIgnore>
        Public ReadOnly Property CmdRemove As ICommand = New RelayCommand(Sub()
                                                                              Parent.Databases.Remove(Me)
                                                                              Entity.Database.Delete(TargetName)
                                                                              If IsActive Then
                                                                                  Dim Item = Parent.Databases.FirstOrDefault
                                                                                  If Item IsNot Nothing Then Item.IsActive = True
                                                                              End If
                                                                          End Sub)


        Public Sub Attach()
            Dim Server As New Server(srvName)

            For Each Item As Database In Server.Databases
                If Item.Name = TargetName Then
                    If File.Exists(AttachDbFilename) Then
                        Return
                    Else
                        Entity.Database.Delete(TargetName)
                    End If
                End If
            Next

            Dim sc As New StringCollection From {
                    MoveFile(dbMdf, TargetMdf),
                    MoveFile(dbLdf, TargetLdf)}

            Server.AttachDatabase(TargetName, sc, AttachOptions.None)
        End Sub


        Private Function MoveFile(fromFile As String, toFile As String) As String
            fromFile = Path.Combine(BaseDirectory.FullName, fromFile)
            toFile = Path.Combine(MyDocumentsDirectory.FullName, toFile)

            If Not File.Exists(toFile) Then File.Copy(fromFile, toFile)

            Return toFile
        End Function

    End Class
End Namespace