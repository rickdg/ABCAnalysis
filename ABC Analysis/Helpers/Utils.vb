Imports System.IO
Imports Newtonsoft.Json

Public Module Utils

    Private _BaseDirectory As DirectoryInfo = Nothing
    Private _MyDocumentsDirectory As DirectoryInfo = Nothing


    Public Property BaseDirectory As DirectoryInfo
        Get
            Return _BaseDirectory
        End Get
        Set(ByVal value As DirectoryInfo)
            _BaseDirectory = value
            If Not _BaseDirectory.Exists Then
                _BaseDirectory.Create()
            End If
        End Set
    End Property
    Public Property MyDocumentsDirectory As DirectoryInfo
        Get
            Return _MyDocumentsDirectory
        End Get
        Set(ByVal value As DirectoryInfo)
            _MyDocumentsDirectory = value
            If Not _MyDocumentsDirectory.Exists Then
                _MyDocumentsDirectory.Create()
            End If
        End Set
    End Property


    Public Function GetInBaseDirectoryInfo(directory As String) As DirectoryInfo
        Dim di = New DirectoryInfo(Path.Combine(_BaseDirectory.FullName, directory))
        If Not di.Exists Then di.Create()
        Return di
    End Function


    Public Function GetInBaseFileInfo(file As String, Optional deleteIfExists As Boolean = True) As FileInfo
        Dim fi = New FileInfo(Path.Combine(BaseDirectory.FullName, file))
        If deleteIfExists AndAlso fi.Exists Then fi.Delete()
        Return fi
    End Function


    Public Function GetInBaseFileInfo(altOutputDir As DirectoryInfo, file As String, Optional deleteIfExists As Boolean = True) As FileInfo
        Dim fi = New FileInfo(Path.Combine(altOutputDir.FullName, file))
        If deleteIfExists AndAlso fi.Exists Then fi.Delete()
        Return fi
    End Function


    Public Function ReadInBaseTextFile(codeDir As DirectoryInfo, fileName As String) As String
        Return File.ReadAllText(GetInBaseFileInfo(codeDir, fileName, False).FullName)
    End Function


    Public Sub Serialize(value As Object, name As String)
        Dim FullPath = Path.Combine(MyDocumentsDirectory.FullName, $"{name}.json")
        Dim Serializer As New JsonSerializer With {
            .TypeNameHandling = TypeNameHandling.Auto,
            .NullValueHandling = NullValueHandling.Ignore,
            .Formatting = Formatting.Indented,
            .PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            .ReferenceLoopHandling = ReferenceLoopHandling.Serialize}
        Serializer.Converters.Add(New Converters.JavaScriptDateTimeConverter())
        Using StreamWriter As New StreamWriter(FullPath)
            Using Writer As New JsonTextWriter(StreamWriter)
                Serializer.Serialize(Writer, value)
            End Using
        End Using
    End Sub


    Public Function Deserialize(Of T)(name As String) As T
        Dim FullPath = Path.Combine(MyDocumentsDirectory.FullName, $"{name}.json")
        Return JsonConvert.DeserializeObject(Of T)(File.ReadAllText(FullPath), New JsonSerializerSettings With {
                                                   .TypeNameHandling = TypeNameHandling.Auto,
                                                   .NullValueHandling = NullValueHandling.Ignore})
    End Function


    Public Function FileExists(name As String) As Boolean
        Dim FullPath = Path.Combine(MyDocumentsDirectory.FullName, $"{name}.json")
        Return File.Exists(FullPath)
    End Function

End Module