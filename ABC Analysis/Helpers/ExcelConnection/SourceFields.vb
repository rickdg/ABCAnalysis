﻿Imports System.Text

Namespace ExcelConnection
    Module SourceFields

        Public Function GetPickTasksFields() As IEnumerable(Of Column)
            Return {New Column("Дата", AdoEnums.adDate),
                New Column("Зона", AdoEnums.adDouble),
                New Column("Позиция", AdoEnums.adDouble),
                New Column("Категория", AdoEnums.adWChar),
                New Column("ТПП", AdoEnums.adWChar),
                New Column("Заказ на продажу", AdoEnums.adDouble),
                New Column("Кол-во ЗнП", AdoEnums.adDouble),
                New Column("Задачи", AdoEnums.adDouble)}
        End Function


        Public Function CheckColumns(loadType As LoadType, verifiable As IEnumerable(Of Column)) As String
            Dim Result As New StringBuilder
            Dim Original As IEnumerable(Of Column)

            Select Case loadType
                Case LoadType.PickTasks
                    Original = GetPickTasksFields()
                Case Else
                    Throw New ArgumentException("Тип загружаемого файла не определен")
            End Select

            For Each Source In Original
                Dim Target = verifiable.SingleOrDefault(Function(c) c.Name = Source.Name)
                If Target Is Nothing Then
                    Result.AppendLine($"Отсутствует столбец - [b]{Source}[/b]")
                Else
                    If Target.DataType <> Source.DataType Then
                        Result.AppendLine($"Столбец - [b]{Target}[/b] ([url=https://docs.microsoft.com/ru-ru/sql/ado/reference/ado-api/datatypeenum?view=sql-server-2017][b]{Target.DataType}[/b][/url]) несоответствует типу данных [url=https://docs.microsoft.com/ru-ru/sql/ado/reference/ado-api/datatypeenum?view=sql-server-2017][b]{Source.DataType}[/b][/url]")
                    End If
                End If
            Next
            Return Result.ToString
        End Function

    End Module
End Namespace