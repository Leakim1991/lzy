﻿Imports NUnit.Framework
Imports System.Security.Principal
Imports LazyFramework.CQRS
Imports LazyFramework.CQRS.ExecutionProfile

<TestFixture> Public Class ValidatorTest

    <Test> Public Sub ValidatorsIsCalled()
        Assert.Throws(Of LazyFramework.CQRS.Validation.ValidationException)(Sub() LazyFramework.CQRS.Validation.Handling.ValidateAction(nothing,New ToValidate))

        Try
            LazyFramework.CQRS.Validation.Handling.ValidateAction(Nothing, New ToValidate)
        Catch ex As LazyFramework.CQRS.Validation.ValidationException
            Assert.AreEqual(2, ex.ExceptionList.Count)
        End Try

    End Sub

End Class

Public Class ToValidate
    Inherits LazyFramework.CQRS.ActionBase
    Property Id As Integer

    Public Overrides Function IsAvailable() As Boolean
        Throw New NotImplementedException()
    End Function

    Public Overrides Function IsAvailable(user As IPrincipal) As Boolean
        Throw New NotImplementedException()
    End Function

    Public Overrides Function IsAvailable(user As IPrincipal, o As Object) As Boolean
        Throw New NotImplementedException()
    End Function
End Class


Public Class ToValidateValidator
    Inherits LazyFramework.CQRS.Validation.ValidateActionBase(Of ToValidate)

    Public Sub CheckId(action As ToValidate)
        If action.Id = 0 Then
            Throw New MissingFieldException
        End If
    End Sub

    Public Sub IdIsLessThan1000(action As ToValidate)
        If action.Id < 1000 Then
            Throw New ArgumentOutOfRangeException
        End If
    End Sub

    Private Sub DoNotCall()
        Throw New NotImplementedException
    End Sub

End Class
