Namespace Connection.Excel
    Module SqlScript

        Public Function GetScript(loadType As LoadType, table As String) As String
            Select Case loadType
                Case LoadType.PickTasks
                    Return GetPickTasksScript(table)
                Case LoadType.AbcData
                    Return GetAbcDataScript(table)
            End Select
            Return ""
        End Function


        Private Function GetAbcDataScript(table As String) As String
            Return $"SELECT [Позиция] AS CodeItem,
                   [Класс ABC] AS ClassABC
                   FROM [{table}]"
        End Function


        Public Function GetPickTasksScript(table As String) As String
            Return $"SELECT [Дата] AS TaskDate,
                   [Зона] AS Subinventory,
                   [Позиция] AS Code,
                   [Категория] AS Category,
                   [ТПП] AS UserPositionType,
                   [Заказ на продажу] AS SalesOrder,
                   [Кол-во ЗнП] AS Orders,
                   [Задачи] AS Tasks
                   FROM [{table}]
                   WHERE [Позиция] IS NOT NULL"
        End Function

    End Module
End Namespace