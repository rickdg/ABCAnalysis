Imports System.IO

Namespace DataLoad
    Public Class DLWriter
        Const Delay As String = "0"
        Const Header As String = ""
        Const Window As String = "Oracle Applications"
        Const CommandGroup As String = "NCA"
        Const HourGlass As Boolean = True


        Public Sub New(file As String)
            Me.File = file
        End Sub


        Public Property Description As String
        Public Property File As String
        Public Property Format As String


        Public Sub Write(data As String)
            Using SW As New StreamWriter(File, False, Text.Encoding.Default)
                SW.WriteLine(CreateBegindelays())
                SW.WriteLine($"beginheader{vbLf}{Header}{vbLf}endheader")
                SW.WriteLine(CreateBeginother())
                SW.WriteLine($"begindata{vbLf}{data}enddata")
                SW.WriteLine(CreateBeginoptions())
                SW.WriteLine($"beginformat{vbLf}{Format}{vbLf}endformat")
                SW.WriteLine(CreateBegincommands)
                SW.Close()
            End Using
        End Sub


        Private Function CreateBegindelays() As String
            Return $"begindelays
DataDelay={Delay}
CellDelay={Delay}
FormDelay={Delay}
KeyHold=50
SystemHold=50
HourGlass=1000
TAB=0
ENT=0
*UP=0
*DN=0
*LT=0
*RT=0
*SP=0
*SAVE=0
*NB=0
*PB=0
*NF=0
*PF=0
*NR=0
*PR=0
*FR=0
*LR=0
*ER=0
*DR=0
*SB=0
*ST=0
*BM=0
*QE=0
*QR=0
*FI=0
*FA=0
*IR=0
*CL=0
*FE=0
*AA=0
*AB=0
*AC=0
*AD=0
*AE=0
*AF=0
*AG=0
*AH=0
*AI=0
*AJ=0
*AK=0
*AL=0
*AM=0
*AN=0
*AO=0
*AP=0
*AQ=0
*AR=0
*AS=0
*AT=0
*AU=0
*AV=0
*AW=0
*AX=0
*AY=0
*AZ=0
*CREATE=0
*SPACE=0
enddelays"
        End Function


        Private Function CreateBeginother() As String
            Return $"beginother
Window={Window}
Description={Description}
CommandGroup={CommandGroup}
PasteCtrlV = 1
endother"
        End Function


        Private Function CreateBeginoptions() As String
            Dim HourGlassStr As String = "false"
            If HourGlass Then HourGlassStr = "true"
            Return $"beginoptions
AutoTab=false
HourGlass={HourGlassStr}
AutoInsert=true
ProgressBar=true
TitleSize=false
Feedback=false
DataWait=false
CommandWait=true
DataColour=8454143
CommandColour=8454016
KeystrokeColour=16777088
DataSource=0
ImportLoad=0
FeedbackFile=
endoptions"
        End Function


        Private Function CreateBegincommands() As String
            Return "begincommands" & vbLf &
               "Command" & vbTab & "NCA" & vbTab & "10SC" & vbTab & "Other" & vbLf &
               "TAB" & vbTab & "\{TAB}" & vbTab & "\{TAB}" & vbTab & "\{TAB}" & vbLf &
               "ENT" & vbTab & "\{ENTER}" & vbTab & "\{ENTER}" & vbTab & "\{ENTER}" & vbLf &
               "*UP" & vbTab & "\{UP}" & vbTab & "\{UP}" & vbTab & "\{UP}" & vbLf &
               "*DN" & vbTab & "\{DOWN}" & vbTab & "\{DOWN}" & vbTab & "\{DOWN}" & vbLf &
               "*LT" & vbTab & "\{LEFT}" & vbTab & "\{LEFT}" & vbTab & "\{LEFT}" & vbLf &
               "*RT" & vbTab & "\{RIGHT}" & vbTab & "\{RIGHT}" & vbTab & "\{RIGHT}" & vbLf &
               "*SP" & vbTab & "\%A{DOWN 4}{ENTER}" & vbTab & "\%A%A" & vbTab & "" & vbLf &
               "*SAVE" & vbTab & "\^S" & vbTab & "\{F10}" & vbTab & "\^S" & vbLf &
               "*NB" & vbTab & "\^{PGDN}" & vbTab & "\^{PGDN}" & vbTab & "" & vbLf &
               "*PB" & vbTab & "\^{PGUP}" & vbTab & "\^{PGUP}" & vbTab & "" & vbLf &
               "*NF" & vbTab & "\%G{DOWN}{ENTER}" & vbTab & "\{TAB}" & vbTab & "" & vbLf &
               "*PF" & vbTab & "\%G{DOWN 2}{ENTER}" & vbTab & "\+{TAB}" & vbTab & "" & vbLf &
               "*NR" & vbTab & "\%G{DOWN 3}{ENTER}" & vbTab & "\+{DOWN}" & vbTab & "" & vbLf &
               "*PR" & vbTab & "\%G{DOWN 4}{ENTER}" & vbTab & "\+{UP}" & vbTab & "" & vbLf &
               "*FR" & vbTab & "\%G{DOWN 5}{ENTER}" & vbTab & "\%G%F" & vbTab & "" & vbLf &
               "*LR" & vbTab & "\%G{DOWN 6}{ENTER}" & vbTab & "\%G%vbLf" & vbTab & "" & vbLf &
               "*ER" & vbTab & "\{F6}" & vbTab & "\+{F4}" & vbTab & "" & vbLf &
               "*DR" & vbTab & "\^{UP}" & vbTab & "\+{F6}" & vbTab & "" & vbLf &
               "*SB" & vbTab & "\ " & vbTab & "\ " & vbTab & "\ " & vbLf &
               "*ST" & vbTab & "\{HOME}+{END}" & vbTab & "\{HOME}+{END}" & vbTab & "" & vbLf &
               "*BM" & vbTab & "\^B" & vbTab & "\{F5}" & vbTab & "" & vbLf &
               "*QE" & vbTab & "\{F11}" & vbTab & "\{F7}" & vbTab & "" & vbLf &
               "*QR" & vbTab & "\^{F11}" & vbTab & "\{F8}" & vbTab & "" & vbLf &
               "*FI" & vbTab & "\%Q{DOWN}{ENTER}" & vbTab & "" & vbTab & "" & vbLf &
               "*FA" & vbTab & "\%Q{DOWN 2}{ENTER}" & vbTab & "" & vbTab & "" & vbLf &
               "*IR" & vbTab & "\^{DOWN}" & vbTab & "\{F6}" & vbTab & "" & vbLf &
               "*CL" & vbTab & "\{F5}" & vbTab & "\^U" & vbTab & "" & vbLf &
               "*FE" & vbTab & "\^E" & vbTab & "\^E" & vbTab & "" & vbLf &
               "*AA" & vbTab & "\%A" & vbTab & "\%A" & vbTab & "\%A" & vbLf &
               "*AB" & vbTab & "\%B" & vbTab & "\%B" & vbTab & "\%B" & vbLf &
               "*AC" & vbTab & "\%C" & vbTab & "\%C" & vbTab & "\%C" & vbLf &
               "*AD" & vbTab & "\%D" & vbTab & "\%D" & vbTab & "\%D" & vbLf &
               "*AE" & vbTab & "\%E" & vbTab & "\%E" & vbTab & "\%E" & vbLf &
               "*AF" & vbTab & "\%F" & vbTab & "\%F" & vbTab & "\%F" & vbLf &
               "*AG" & vbTab & "\%G" & vbTab & "\%G" & vbTab & "\%G" & vbLf &
               "*AH" & vbTab & "\%H" & vbTab & "\%H" & vbTab & "\%H" & vbLf &
               "*AI" & vbTab & "\%I" & vbTab & "\%I" & vbTab & "\%I" & vbLf &
               "*AJ" & vbTab & "\%J" & vbTab & "\%J" & vbTab & "\%J" & vbLf &
               "*AK" & vbTab & "\%K" & vbTab & "\%K" & vbTab & "\%K" & vbLf &
               "*AL" & vbTab & "\%vbLf" & vbTab & "\%vbLf" & vbTab & "\%vbLf" & vbLf &
               "*AM" & vbTab & "\%M" & vbTab & "\%M" & vbTab & "\%M" & vbLf &
               "*AN" & vbTab & "\%N" & vbTab & "\%N" & vbTab & "\%N" & vbLf &
               "*AO" & vbTab & "\%O" & vbTab & "\%O" & vbTab & "\%O" & vbLf &
               "*AP" & vbTab & "\%P" & vbTab & "\%P" & vbTab & "\%P" & vbLf &
               "*AQ" & vbTab & "\%Q" & vbTab & "\%Q" & vbTab & "\%Q" & vbLf &
               "*AR" & vbTab & "\%R" & vbTab & "\%R" & vbTab & "\%R" & vbLf &
               "*AS" & vbTab & "\%S" & vbTab & "\%S" & vbTab & "\%S" & vbLf &
               "*AT" & vbTab & "\%vbTab" & vbTab & "\%vbTab" & vbTab & "\%vbTab" & vbLf &
               "*AU" & vbTab & "\%U" & vbTab & "\%U" & vbTab & "\%U" & vbLf &
               "*AV" & vbTab & "\%V" & vbTab & "\%V" & vbTab & "\%V" & vbLf &
               "*AW" & vbTab & "\%W" & vbTab & "\%W" & vbTab & "\%W" & vbLf &
               "*AX" & vbTab & "\%X" & vbTab & "\%X" & vbTab & "\%X" & vbLf &
               "*AY" & vbTab & "\%Y" & vbTab & "\%Y" & vbTab & "\%Y" & vbLf &
               "*AZ" & vbTab & "\%Z" & vbTab & "\%Z" & vbTab & "\%Z" & vbLf &
               "*CREATE" & vbTab & "" & vbTab & "" & vbTab & "\{F8}" & vbLf &
               "*SPACE" & vbTab & "" & vbTab & "" & vbTab & "\ " & vbLf &
               "endcommands"
        End Function
    End Class
End Namespace