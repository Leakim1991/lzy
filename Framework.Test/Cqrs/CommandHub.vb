﻿
Imports System.Security.Principal
Imports LazyFramework.CQRS
Imports LazyFramework.CQRS.Availability
Imports LazyFramework.CQRS.Command
Imports LazyFramework.CQRS.ExecutionProfile
Imports LazyFramework.CQRS.Security
Imports NUnit.Framework

Namespace Cqrs

    <TestFixture> Public Class CommandHub

        <SetUp> Public Sub SetupFixture()
            Runtime.Context.Current = New Runtime.WinThread
            LazyFramework.ClassFactory.Clear()
            LazyFramework.ClassFactory.SetTypeInstance(Of IActionSecurity)(New TestSecurity)
            LazyFramework.ClassFactory.SetTypeInstance(Of IExecutionProfileProvider)(New TestExecutionProfileProvider)
        End Sub

        <TearDown> Public Sub TearDown()
            LazyFramework.ClassFactory.Clear()
        End Sub

        <Test> Public Sub CommandIsLogged()

            Handling.ExecuteCommand(New TestExecutionProfileProvider().GetExecutionProfile, New TestCommand)
            'Assert.IsTrue(_TestLogger.LogIsCalled)

        End Sub


        <Test> Public Sub CommandIsRunned()

            Dim testExecutionProfileProvider = New TestExecutionProfileProvider()
            Handling.ExecuteCommand(testExecutionProfileProvider.GetExecutionProfile, New TestCommand)
            Handling.ExecuteCommand(testExecutionProfileProvider.GetExecutionProfile, New AnotherCommand)
            Assert.IsTrue(CommandHandler.Found)

        End Sub


        <Test> Public Sub ExecptionIsHandledCorrectly()

            Assert.Throws(Of InnerException)(Sub() Handling.ExecuteCommand(New TestExecutionProfileProvider().GetExecutionProfile, New ExceptionIsThrownCommand))

        End Sub

        <Test> Public Sub ByRefparemSubIsCalled()
            Dim byrefCommand As ByrefCommand = New ByrefCommand
            Handling.ExecuteCommand(New TestExecutionProfileProvider().GetExecutionProfile, byrefCommand)

            Assert.IsTrue(byrefCommand.Called)

        End Sub

        <Test> Public Sub CommandIsMappedToName()
            Dim toTest As New CommandForA

            Assert.AreEqual(toTest.ActionName, Handling.CommandList(toTest.ActionName).FullName.Replace("."c, ""))

        End Sub


        <Test> Public Sub ReadOnlyPropertiesIsNotSerialized()

            Dim a As New CommandForA

            Dim res = Newtonsoft.Json.JsonConvert.SerializeObject(a)

            Debug.Print(res)

        End Sub

        <Test> Public Sub NotAvailableCommandIsStopped()

            Assert.Throws(Of ActionIsNotAvailableException)(Sub() Handling.ExecuteCommand(New TestExecutionProfileProvider().GetExecutionProfile, New ThisCommandIsNotAvailableIfIdIs0 With {.Id = 0}))
            
        End Sub

        <Test> Public Sub CommandAvailabilityIsCalled

            Assert.DoesNotThrow(Sub() Handling.ExecuteCommand(New TestExecutionProfileProvider().GetExecutionProfile, New ThisCommandIsNotAvailableIfIdIs0 With {.Id= 1}))
            
        End Sub


        <Test> Public sub AddHandlerManually
            Dim cmdHandler As New CommandHandler
            
            LazyFramework.CQRS.Command.Handling.Add(Of ThisCommandIsNotAvailableIfIdIs0)(addressof cmdHandler.CommandHandler)
        End sub

    End Class


    Public Class CommandAvaialability
        Inherits LazyFramework.CQRS.Availability.CommandAvailability(Of ThisCommandIsNotAvailableIfIdIs0, Entity)

        Public Overrides Function IsAvailable(profile As IExecutionProfile, commad As ThisCommandIsNotAvailableIfIdIs0) As Boolean
            Return commad.Id <> 0
        End Function

        Public Overrides Function IsAvailable(profile As IExecutionProfile, commad As ThisCommandIsNotAvailableIfIdIs0, entity As Entity) As Boolean
            Return False
        End Function
    End Class

    Public Class ThisCommandIsNotAvailableIfIdIs0
        Inherits BaseCommand(Of Entity)
        
        Public Property Id As Integer


    End Class

    

    Public Class Entity
        Public Property A As Integer
    End Class


    Public MustInherit Class BaseCommand(Of T)
        Inherits CommandBase(Of T)


    End Class

    Public Class CommandForA
        Inherits BaseCommand(Of Entity)

        Public MyParam As Integer = 1


    End Class

    Public Class AnotherCommandForA
        Inherits CommandForA

    End Class





    Public Class CalculateKm
        Inherits CommandBase

        Public KmDrive As Integer
        Public DateDriven As DateTime

        Public Overrides Function ActionName() As String

            Return "Kalkuler km sats"

        End Function

        
    End Class

    Public Class TestSecurity
        Implements IActionSecurity

        Public Function EntityIsAvailableForUser(profile As IExecutionProfile, action As IAmAnAction, entity As Object) As Boolean Implements IActionSecurity.EntityIsAvailableForUser
            Return True
        End Function

        Public Function GetActionList(profile As IExecutionProfile, action As IActionBase, entity As Object) As List(Of IActionDescriptor) Implements IActionSecurity.GetActionList
            Return New List(Of IActionDescriptor)
        End Function

        Public Function UserCanRunThisAction(profile As IExecutionProfile, c As IActionBase) As Boolean Implements IActionSecurity.UserCanRunThisAction
            Return True
        End Function

        Public Function UserCanRunThisAction(profile As IExecutionProfile, action As IActionBase, entity As Object) As Boolean Implements IActionSecurity.UserCanRunThisAction
            Return True
        End Function
    End Class


    Public Class CommandHandler
        Implements IHandleCommand

        Public Shared Found As Boolean = False

        Public  sub CommandHandler(cmd As ThisCommandIsNotAvailableIfIdIs0)

        End sub

        Public Shared Sub CommandHandler(cmd As CalculateKm)

            Dim res As Single

            res = CType((cmd.KmDrive * 4.14), Single)

            'EventHandling.EventHub.Publish()


        End Sub

        Public Shared Sub CommandHandler(command As TestCommand)
            Found = True
        End Sub

    End Class

    Public Class InnerException
        Inherits Exception

    End Class

    Public Class ExceptionIsThrownCommand
        Inherits CommandBase

        Public Overrides Function ActionName() As String

            Return "Exception"

        End Function

  
    End Class

    Public Class AnotherCommand
        Inherits CommandBase

        Public Overrides Function ActionName() As String
            Return "jbjkbkjb"
        End Function

  
    End Class

    Public Class ByrefCommand
        Inherits CommandBase

        Public Called As Boolean = False

        Public Overrides Function ActionName() As String

            Return ""

        End Function
        
    End Class

    Public Class TestCommand
        Inherits CommandBase

        Public Overrides Function ActionName() As String

            Return "Name"

        End Function

        
    End Class


    Public Class Another
        Implements IHandleCommand

        Public Shared Sub HandleSomethingElseCommandHandler(cmd As AnotherCommand)
            Dim a = cmd.ActionName
        End Sub


        Public Shared IsCalled As Boolean = False

        Public Shared Sub ParamIsByRefCommandHandler(ByRef cmd As ByrefCommand)
            cmd.Called = True
        End Sub

        Public Shared Sub ExceptionIsThrownCommandHandler(cmd As ExceptionIsThrownCommand)

            Throw New InnerException

        End Sub

    End Class
End Namespace