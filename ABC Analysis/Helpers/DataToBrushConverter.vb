Imports System.Globalization

Public Class DataToBrushConverter
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        If targetType <> GetType(Brush) Then
            Throw New InvalidOperationException()
        End If

        Dim TargetDate = CDate(value)
        Dim Color As Color
        Select Case True
            Case TargetDate <= Today
                Color = Color.FromRgb(&H8C, &HBF, &H26)
            Case TargetDate <= Today.AddDays(7)
                Color = Color.FromRgb(&HF0, &H96, &H9)
            Case TargetDate > Today
                Color = Color.FromRgb(&H33, &H99, &HFF)
        End Select

        Return New SolidColorBrush(Color)
    End Function


    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Return DependencyProperty.UnsetValue
    End Function
End Class
