﻿Namespace Utils.Json
    Friend Class ArrayBuilder
        Inherits Builder


        Public Overrides Function Parse(nextChar As IReader, t As Type) As Object

            TokenAcceptors.WhiteSpace(nextChar)
            TokenAcceptors.BufferLegalCharacters(nextChar, "nul")
            Dim buffer = nextChar.Buffer
            If buffer = "null" Then Return Nothing

            TokenAcceptors.EatUntil(TokenAcceptors.ListStart, nextChar)

            Dim strategy As IParseStrategy
            If t.IsArray Then
                strategy = New ArrayParserStrategy(t)
            Else
                strategy = New ListParseStrategy(t)
            End If

            TokenAcceptors.WhiteSpace(nextChar)

            Do
                If strategy.InnerType.IsValueType Or strategy.InnerType = GetType(String) Then
                    TokenAcceptors.WhiteSpace(nextChar)
                    If nextChar.Peek <> TokenAcceptors.ListEnd Then
                        strategy.ItemList.Add(TokenAcceptors.TypeParserMapper(strategy.InnerType).Parse(nextChar, t))
                    End If
                Else
                    Dim v As Object = Reader.StringToObject(nextChar, strategy.InnerType)
                    If v IsNot Nothing Then
                        strategy.ItemList.Add(v)
                    End If
                End If
            Loop While TokenAcceptors.CanFindValueSeparator(nextChar)

            TokenAcceptors.EatUntil(TokenAcceptors.ListEnd, nextChar)

            Return strategy.Result
        End Function


        Private Interface IParseStrategy
            ReadOnly Property ItemList As IList
            ReadOnly Property InnerType As Type
            ReadOnly Property Result As Object
        End Interface

        Private Class ArrayParserStrategy
            Implements IParseStrategy

            Private _innerType As Type
            Private _t As Type
            Public Sub New(t As Type)
                _t = t
                _innerType = t.GetElementType
                Dim tempType = GetType(List(Of ))
                _itemList = CType(Activator.CreateInstance(tempType.MakeGenericType(InnerType)), IList)
            End Sub


            Private _itemList As IList
            Public ReadOnly Property ItemList As IList Implements IParseStrategy.ItemList
                Get
                    Return _itemList
                End Get
            End Property

            Public ReadOnly Property Result As Object Implements IParseStrategy.Result
                Get
                    Dim newA = Activator.CreateInstance(_t, _itemList.Count)
                    _itemList.CopyTo(CType(newA, Array), 0)
                    Return newA
                End Get
            End Property

            Public ReadOnly Property InnerType As Type Implements IParseStrategy.InnerType
                Get
                    Return _innerType
                End Get
            End Property
        End Class

        Private Class ListParseStrategy
            Implements IParseStrategy

            Private _ItemList As IList
            Private _innertype As Type

            Public Sub New(t As Type)
                If t.IsGenericType Then
                    _innertype = t.GetGenericArguments(0)
                Else
                    Throw New NonGenericListIsNotSupportedException
                End If

                If t.IsInterface Then
                    Dim tempType = GetType(List(Of ))
                    _ItemList = CType(Activator.CreateInstance(tempType.MakeGenericType(InnerType)), IList)
                Else
                    _ItemList = CType(Activator.CreateInstance(t), IList)
                End If


            End Sub

            Public ReadOnly Property InnerType As Type Implements IParseStrategy.InnerType
                Get
                    Return _innertype
                End Get
            End Property

            Public ReadOnly Property ItemList As IList Implements IParseStrategy.ItemList
                Get
                    Return _ItemList
                End Get
            End Property

            Public ReadOnly Property Result As Object Implements IParseStrategy.Result
                Get
                    Return _ItemList
                End Get
            End Property
        End Class

        'Private Class EnumerableParserStrategy
        '    Implements IParseStrategy

        '    Private _innerType As Type
        '    Private _itemList As IList
        '    Private _t As Type

        '    ''' <summary>
        '    ''' In this case when something is supose to become an enumerable, we choose to use a list. 
        '    ''' </summary>
        '    ''' <param name="t"></param>
        '    Public Sub New(t As Type)
        '        _t = t
        '        If t.GetGenericArguments().Length = 0 Then
        '            Throw New CanNotInferNonGenericListException
        '        End If
        '        _innerType = t.GetGenericArguments(0)
        '        Dim tempType = GetType(List(Of ))
        '        _itemList = CType(Activator.CreateInstance(tempType.MakeGenericType(InnerType)), IList)
        '    End Sub

        '    Public ReadOnly Property InnerType As Type Implements IParseStrategy.InnerType
        '        Get
        '            Return _innerType
        '        End Get
        '    End Property

        '    Public ReadOnly Property ItemList As IList Implements IParseStrategy.ItemList
        '        Get
        '            Return _itemList
        '        End Get
        '    End Property

        '    Public ReadOnly Property Result As Object Implements IParseStrategy.Result
        '        Get
        '            Return _itemList
        '        End Get
        '    End Property
        'End Class

    End Class
End Namespace
