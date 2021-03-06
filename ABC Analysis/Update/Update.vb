﻿Module Update

    Public ReadOnly Property Revisions As New Dictionary(Of Integer, Revision) From {
        {34, New Revision With {.VersionPart = "4.0.1", .Number = 34, .XDate = New DateTime(2018, 11, 19)}}}


    Public Sub ExecuteUpdate(oldRevision As Integer, newRevision As Integer)
        For r = oldRevision To newRevision
            Revisions(r).UpdateDateBase()
        Next
    End Sub


    Public Sub ShowUpdate()
        'Revisions(88).UpdateDateBase()
        'Revisions(88).Show()
        If MainWindow.Model.IsNewVersion Then
            For r = MainWindow.Model.OldRevision To MainWindow.Model.NewRevision
                Revisions(r).Show()
            Next
        End If
    End Sub

End Module