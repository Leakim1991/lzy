Imports System.Security.Principal
Namespace ExecutionProfile
    Public Interface IExecutionProfile
        Function User() As IPrincipal
        Function Application() As IApplicationInfo
        Sub Publish(currentUser As IPrincipal, [event] As Object)
        Sub Log(level As Integer, message As String)
        Property Storage As IDictionary(Of String, Object)
        Function Time() As DateTime
    End Interface
End Namespace