﻿Imports NUnit.Framework

<TestFixture> public Class Reflection

    <Test>Public Sub FindMatchingClassFromNameProperties      
        Assert.AreEqual(1, LazyFramework.Reflection.AllTypes.InNamespace("Test").NameStartsWith("CanThis").NameEndsWith("OrNot").IsAssignableFrom(Of IMarker)().Count)
    End Sub


    <test> Public sub FindFunctionInClass
        
        Assert.AreEqual(1, LazyFramework.Reflection.AllTypes.
                        NameEndsWith("WithFunctions").
                        AllMethods().
                        NameEndsWith("One").
                        SignatureIs(GetType(Integer), GetType(string)).
                        IsFunction.
                        Count)

    End sub

    <Test> Public sub FindClassesThatInheritsGenericTypeFromOpenGenericType

        Dim toSearch As New List(Of Type)
        toSearch.Add(GetType(InheritsGenericType))

        Assert.AreEqual(1, toSearch.FromOpenGeneric(GetType(MyGenericType(Of))).Count)
        

    End sub

    <Test> Public sub FindClassesThatInheritsGenericTypeFromOpenGenericTypeWithObject

        Dim toSearch As New List(Of Type)
        toSearch.Add(GetType(InheritsWithObject))

        Assert.AreEqual(1, toSearch.FromOpenGeneric(GetType(MyGenericType(Of))).Count)
        

    End sub


    Public Class MyGenericType(Of T)

    End Class

    Public Class InheritsGenericType
        Inherits MyGenericType(Of Integer)

    End Class

    Public Class InheritsWithObject
        Inherits MyGenericType(Of Object)
    End Class


    Public Interface IMarker
    End Interface

    Public Class CanThisBeFoundOrNot
        Implements IMarker
    End Class



    Public Class ClassWithFunctions
        Function One(a As Integer, b As String) As Boolean
            Return Nothing
        End Function
    End Class

End Class