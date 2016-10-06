Imports System.Reflection

Namespace Utils.Json
    Public Class SetterCache

        Private Shared _setterlist As New Dictionary(Of String, NameInfo)

        Private Shared padLock As New Object

        Public Shared Function GetInfo(mInfo As MemberInfo) As NameInfo
            Dim key = mInfo.DeclaringType.FullName & "-" & mInfo.Name

            If Not _setterlist.ContainsKey(key) Then
                SyncLock padLock
                    If Not _setterlist.ContainsKey(key) Then
                        _setterlist(key) = New NameInfo(mInfo.DeclaringType.Name, mInfo.Name, Reflection.CreateSetter(mInfo))
                    End If
                End SyncLock
            End If
            Return _setterlist(key)

        End Function

        Public Function SetInfo() As NameInfo
            Return Nothing 'Not implemented
        End Function


        Public Class NameInfo
            Private ReadOnly _typename As String
            Private ReadOnly _name As String
            Private ReadOnly _setter As Func(Of Object, Object, Object)


            Public Sub New(typename As String, name As String, setter As Func(Of Object, Object, Object))
                _typename = typename
                _name = name
                _setter = setter
            End Sub

            Public ReadOnly Property Setter As Func(Of Object, Object, Object)
                Get
                    Return _setter
                End Get
            End Property

            Public ReadOnly Property Name As String
                Get
                    Return _name
                End Get
            End Property

            Public ReadOnly Property Typename As String
                Get
                    Return _typename
                End Get
            End Property
        End Class
    End Class



End Namespace