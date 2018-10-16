Namespace DataLoad
    Public Module DLStringBuilder

        Private Const StartFind = "*QE"
        Private Const EndFind = "*QR"
        Private Const Save = "*SAVE"
        Private Const Down = "*DN"
        Private Const Tab = "TAB"


        Public Function AddAbc(codeItem As Long, abcClass As String) As String
            Return Join({Down, codeItem, "", Tab, abcClass, Save}, vbTab)
        End Function


        Public Function ChangeAbc(codeItem As Long, abcClass As String) As String
            Return Join({StartFind, codeItem, EndFind, Tab, abcClass, Save}, vbTab)
        End Function

    End Module
End Namespace