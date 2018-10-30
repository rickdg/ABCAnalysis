Imports System.Collections.ObjectModel
Imports System.Data.Entity
Imports FirstFloor.ModernUI.Presentation
Imports ABCAnalysis.Connection.Excel

Namespace Pages
    Public Class AbcGroupModel

        Public Property ParentCollection As ObservableCollection(Of AbcGroupModel)
        Public Property Entity As AbcGroup


        Public Property Name As String
            Get
                Return Entity.Name
            End Get
            Set
                Entity.Name = Value
                EntityModifed("Name")
            End Set
        End Property


        Public ReadOnly Property CmdView As ICommand = New RelayCommand(
            Sub()
                Using Context = MainPageModel.CurrentProject.Context
                    Dim Collection = (From abc In Context.AbcCodeItems
                                      Where abc.AbcGroup_id = Entity.Id
                                      Select New AbcClassView With {.Позиция = abc.CodeItem, .AbcClass_id = abc.AbcClass_id}).ToList
                    ViewCollection(Name, Collection)
                End Using
            End Sub)
        Public ReadOnly Property CmdRemove As ICommand = New RelayCommand(
            Sub()
                Using Context = MainPageModel.CurrentProject.Context
                    Context.DeleteAbc(Entity.Id)
                    Context.Entry(Entity).State = EntityState.Deleted
                    Context.SaveChanges()
                    ParentCollection.Remove(Me)
                End Using
            End Sub)


        Private Sub EntityModifed(propertyName As String)
            Using Context = MainPageModel.CurrentProject.Context
                Context.AbcGroups.Attach(Entity)
                Context.Entry(Entity).Property(propertyName).IsModified = True
                Context.SaveChanges()
            End Using
        End Sub

    End Class
End Namespace