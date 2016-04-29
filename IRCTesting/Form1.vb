Imports System
Imports System.Net
Imports System.Net.Sockets
Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Collections.Concurrent

Public Class Form1

    Private ircTrd As Thread

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        ircTrd = New Thread(AddressOf Main)
        ircTrd.IsBackground = True
        ircTrd.Start()
    End Sub

    Private WithEvents ircTimer As New System.Windows.Forms.Timer()
    Private Shared ircQueue As New List(Of String)

    Private _tcpclientConnection As TcpClient = Nothing
    Private _networkStream As NetworkStream = Nothing
    Private _streamWriter As StreamWriter = Nothing
    Private _streamReader As StreamReader = Nothing

    Private Shared ircOnline As Boolean = False
    Private Shared input
    Private Shared output

    Private Sub ircTimer_Tick() Handles ircTimer.Tick
        If ircQueue.Count > 0 Then
            dgvInput.Rows.Add(ircQueue(0))
            ircQueue.Remove(ircQueue(0))
            If dgvInput.Rows.Count > 20 Then
                dgvInput.Rows.Remove(dgvInput.Rows(0))
            End If
        End If

    End Sub

    Private Sub btnSubmit_Click(sender As Object, e As EventArgs) Handles btnSubmit.Click
        output.Write(txOutput.Text & vbCr & vbLf)
        output.Flush()
    End Sub

    Public Shared Sub Main(args As String())
        Dim port As Integer
        Dim buf As String, nick As String, owner As String, server As String, chan As String
        Dim sock As New System.Net.Sockets.TcpClient()

        nick = "DSCM-0110test"
        owner = "DSCM"
        server = "dscm.wulf2k.ca"
        port = 8123
        chan = "#DSCM_Test"

        'Connect to irc server and get input and output text streams from TcpClient.
        sock.Connect(server, port)
        If Not sock.Connected Then
            Console.WriteLine("Failed to connect!")
            Return
        End If
        input = New System.IO.StreamReader(sock.GetStream())
        output = New System.IO.StreamWriter(sock.GetStream())

        'USER and NICK login commands 
        output.Write("USER " & nick & " 0 * :" & owner & vbCr & vbLf & "NICK " & nick & vbCr & vbLf)
        output.Flush()

        output.Write("MODE " & nick & " +B" & vbCr & vbLf)
        output.Flush()

        ircOnline = False

        'Join channel on start
        While Not ircOnline
            buf = input.ReadLine()
            If Not buf Is Nothing Then
                Console.WriteLine(buf)
                ircQueue.Add(buf)

                If buf.StartsWith("PING ") Then
                    output.Write(buf.Replace("PING", "PONG") & vbCr & vbLf)
                    output.Flush()
                End If


                If buf.Contains(":MOTD") Then
                    output.write("JOIN " & chan & vbCr & vbLf)
                    output.flush()
                    ircOnline = True
                End If
            End If
        End While


        'Process each line received from irc server
        While ircOnline
            buf = input.ReadLine()
            'Display received irc message

            If Not buf Is Nothing Then
                Console.WriteLine(buf)
                ircQueue.Add(buf)


                'Send pong reply to any ping messages
                If buf.StartsWith("PING ") Then
                    output.Write(buf.Replace("PING", "PONG") & vbCr & vbLf)
                    output.Flush()
                End If


                'Parse report commands
                If buf.Contains("REPORT|") Then
                    Dim tmpName As String
                    Dim tmpSteamID As String
                    Dim tmpSL As Integer
                    Dim tmpPhantom As String
                    Dim tmpMPZone As Integer
                    Dim tmpWorld As String

                    Dim tmpFields()

                    Dim tmpReport As String
                    tmpReport = buf.Split("|")(1)

                    tmpFields = tmpReport.Split(",")



                    tmpName = tmpFields(0)
                    tmpSteamID = tmpFields(1)
                    tmpSL = tmpFields(2)
                    tmpPhantom = tmpFields(3)
                    tmpMPZone = tmpFields(4)
                    tmpWorld = tmpFields(5)
                End If
            End If
        End While
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        dgvInput.Columns.Add("text", "Text")
        dgvInput.Columns(0).Width = 700
        dgvInput.Font = New Font("Consolas", 10)


        ircTimer = New System.Windows.Forms.Timer
        ircTimer.Interval = 200
        ircTimer.Enabled = True
        ircTimer.Start()
    End Sub
End Class


