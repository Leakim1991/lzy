Imports System.Reflection

Namespace Utils.Json
    Public Class TokenAcceptors
        Public Const ObjectStart = "{"c
        Public Const ObjectEnd = "}"c
        Public Const Qualifier = ":"c
        Public Const Separator = ","c

        Public Shared BuilderFactory As Type = GetType(ObjectBuilder(Of ))

        Public Shared Sub ConsumeComments(nextChar As IReader)
            WhiteSpace(nextChar)

        End Sub

        Public Shared Sub EatUntil(c As Char, nextChar As IReader)
            WhiteSpace(nextChar)
            If nextChar.Read <> c Then
                Throw New MissingTokenException(c)
            End If
        End Sub

        Public Shared Sub WhiteSpace(nextchar As IReader)
            While AscW(nextchar.PeekToBuffer) <= 32
                nextchar.Read()
            End While
        End Sub

        Public Shared Sub Quote(nextChar As IReader)
            If nextChar.Read <> Chr(34) Then
                Throw New MissingTokenException(Chr(34))
            End If
        End Sub

        Public Shared Function Attribute(nextChar As IReader) As String
            'Dim buffer As New StringBuilder
            WhiteSpace(nextChar)
            Quote(nextChar)
            Dim w = AscW(nextChar.PeekToBuffer)

            While (w > 64 AndAlso w < 91) OrElse (w > 96 AndAlso w < 123) 'This is A-Z a-z  The only characters allowed in attribute names.
                w = AscW(nextChar.PeekToBuffer)
            End While
            Dim ret = nextChar.Buffer
            Quote(nextChar)
            Return ret
        End Function

        Public Shared Sub Attributes(ByVal result As Object, ByVal nextChar As IReader)
            Do
                Dim name = Attribute(nextChar)
                EatUntil(Qualifier, nextChar)
                CreateAttributeValue(nextChar, result, name)
            Loop While CanFindValueSeparator(nextChar)
        End Sub

        Private Shared Sub CreateAttributeValue(ByVal nextChar As IReader, ByVal result As Object, ByVal name As String)
            Dim fInfo = result.GetType().GetField(name)

            If fInfo.FieldType.IsValueType Or fInfo.FieldType Is GetType(String) Then
                CreateSimpleValue(result, fInfo, nextChar)
            Else
                CreateComplexValue(result, fInfo, nextChar)
            End If
        End Sub

        Private Shared Sub CreateComplexValue(ByVal result As Object, ByVal fInfo As FieldInfo, ByVal nextChar As IReader)

            Dim b As Builder = CType(Activator.CreateInstance(BuilderFactory.MakeGenericType(fInfo.FieldType)), Builder)
            b.Parse(nextChar)
            If b.Complete Then
                fInfo.SetValue(result, b.InnerResult)
            Else
                Throw New Utils.Json.NotCompleteException
            End If
        End Sub

        Private Shared Sub CreateSimpleValue(ByVal result As Object, ByVal fInfo As FieldInfo, ByVal nextChar As IReader)

            Dim builder = TypeParserMapper(fInfo.FieldType)
            builder.Parse(nextChar)
            fInfo.SetValue(result, builder.InnerResult)
        End Sub

        Public Shared TypeParserMapper As New Dictionary(Of Type, Builder) From {
                                                                            {GetType(String), New StringParser},
                                                                            {GetType(Integer), New IntegerParser},
                                                                            {GetType(Int64), New IntegerParser},
                                                                            {GetType(Int16), New IntegerParser}
                                                                        }



        Private Shared Function CanFindValueSeparator(ByVal nextChar As IReader) As Boolean
            WhiteSpace(nextChar)
            If nextChar.Peek = "," Then
                nextChar.Read()
                Return True
            End If
            Return False
        End Function

    End Class

    Public Class NotCompleteException
        Inherits Exception

    End Class
End Namespace