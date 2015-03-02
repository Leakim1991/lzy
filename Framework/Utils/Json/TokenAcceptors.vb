Namespace Utils.Json
    Public Class TokenAcceptors
        Public Const ObjectStart = "{"c
        Public Const ObjectEnd = "}"c
        Public Const Qualifier = ":"c
        Public Const Separator = ","c

        Public Shared BuilderFactory As Type = GetType(ObjectBuilder(Of ))

        Public Shared Sub EatUntil(c As Char, nextChar As IReader)
            WhiteSpace(nextChar)
            If nextChar.Current <> c Then
                Throw New MissingTokenException(c)
            End If
            nextChar.Read()
        End Sub

        Public Shared Sub EatUntil(c As String, nextChar As IReader)
            While nextChar.BufferPeek.Length < c.Length
                nextChar.PeekToBuffer()
            End While
            While nextChar.BufferPeek <> c
                nextChar.Read()
                nextChar.PeekToBuffer()
            End While
            nextChar.ClearBuffer()
            nextChar.Read() 'Dump end of buffer
        End Sub

        Public Shared Sub WhiteSpace(nextchar As IReader)
            If AscW(nextchar.Current) > 32 Then
                Return
            End If
            While AscW(nextchar.Peek) <= 32
                nextchar.Read()
            End While
            ConsumeComment(nextchar)
        End Sub

        Private Shared Sub ConsumeComment(ByVal nextchar As IReader)
            nextchar.PeekToBuffer()
            If nextchar.Current = "/" Then 'Start of single or multiline comment
                nextchar.PeekToBuffer()
                If nextchar.BufferPeek = "//" Then
                    nextchar.ClearBuffer()
                    EatUntil(vbCrLf, nextchar)
                End If
                If nextchar.BufferPeek = "/*" Then
                    nextchar.ClearBuffer()
                    EatUntil("*/", nextchar)
                End If
                WhiteSpace(nextchar)
            End If
        End Sub

        Public Shared Sub Quote(nextChar As IReader)
            If nextChar.Current <> Chr(34) Then
                Throw New MissingTokenException(Chr(34))
            End If
            nextChar.Read() 'Dump quote
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
            If fInfo Is Nothing Then
                Throw New UnknownAttributeException(name)
            End If
            If fInfo.FieldType.IsValueType Or fInfo.FieldType Is GetType(String) Then
                Dim builder = TypeParserMapper(fInfo.FieldType)

                fInfo.SetValue(result, builder.Parse(nextChar))
            Else
                Dim b As Builder = CType(Activator.CreateInstance(BuilderFactory.MakeGenericType(fInfo.FieldType)), Builder)
                fInfo.SetValue(result, b.Parse(nextChar))
            End If
        End Sub

        Public Shared Sub BufferLegalCharacters(nextChar As IReader, leagal As String)
            Dim toArray = leagal.ToArray
            While toArray.Contains(nextChar.Peek)
                nextChar.PeekToBuffer()
            End While
        End Sub


        Public Shared TypeParserMapper As New Dictionary(Of Type, Builder) From {
                                                                            {GetType(String), New StringParser},
                                                                            {GetType(Integer), New IntegerParser},
                                                                            {GetType(Int64), New IntegerParser},
                                                                            {GetType(Int16), New IntegerParser},
                                                                            {GetType(Date), New DateParser},
                                                                            {GetType(Double), New DoubleParser}
                                                                        }

        Private Shared Function CanFindValueSeparator(ByVal nextChar As IReader) As Boolean
            WhiteSpace(nextChar)
            If nextChar.Current = "," Then
                nextChar.Read()
                Return True
            End If
            Return False
        End Function
    End Class

    <Serializable> Friend Class UnknownAttributeException
        Inherits Exception
        Public Sub New(ByVal name As String)
            MyBase.New(name)
        End Sub
    End Class

    Public Class NotCompleteException
        Inherits Exception

    End Class
End Namespace