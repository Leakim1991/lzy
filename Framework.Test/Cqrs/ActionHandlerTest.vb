﻿Imports LazyFramework.CQRS.ExecutionProfile
Imports LazyFramework.CQRS.Security
Imports NUnit.Framework

Namespace Cqrs
    <TestFixture> Public Class ActionHandlerTest

        <SetUp> Public Sub SetupFixture()
            Runtime.Context.Current = New Runtime.WinThread
            LazyFramework.ClassFactory.Clear()
            LazyFramework.ClassFactory.SetTypeInstance(Of IActionSecurity)(New TestSecurity)
            LazyFramework.ClassFactory.SetTypeInstance(Of IExecutionProfileProvider)(New TestExecutionProfileProvider)
        End Sub

        <TearDown> Public Sub TearDown()
            LazyFramework.ClassFactory.Clear()
        End Sub

        <Test> Public Sub ActionHandlerWithInterfaceParamterIsFound()

            Dim p As New ImplementedQuery
            p.Id = 1000


            Dim executeQuery As Object = Nothing
            Assert.DoesNotThrow(Sub() executeQuery = LazyFramework.CQRS.Query.Handling.ExecuteQuery(New TestExecutionProfileProvider().GetExecutionProfile,p))

            Assert.IsNotNull(executeQuery)

        End Sub


        Public Interface IInterfacedQuery
            Property Id As Integer
        End Interface

        Public Class ImplementedQuery
            Inherits LazyFramework.CQRS.Query.QueryBase(Of Object)
            Implements IInterfacedQuery
            Public Property Id As Integer Implements IInterfacedQuery.Id    
        End Class

        Public Class QueryHandler
            Implements LazyFramework.CQRS.Query.IHandleQuery

            Public Shared IsCalled As Boolean

            Public shared Function HandleQueryHandler(param As IInterfacedQuery) As Object
                
                Return New Object

            End Function


        End Class

    End Class



End Namespace