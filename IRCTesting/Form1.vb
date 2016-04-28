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

    Private WithEvents refTimer As New System.Windows.Forms.Timer()
    Private Shared ircQueue As New List(Of String)
    Private Shared syncObj = New Object

    Private _tcpclientConnection As TcpClient = Nothing '-- main connection to the IRC network.
    Private _networkStream As NetworkStream = Nothing '-- break that connection down to a network stream.
    Private _streamWriter As StreamWriter = Nothing '-- provide a convenient access to writing commands.

    Private _streamReader As StreamReader = Nothing '-- provide a convenient access to reading commands.

    Private Shared input
    Private Shared output

    Private Sub refTimer_Tick() Handles refTimer.Tick
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
        'Dim input As System.IO.TextReader
        'Dim output As System.IO.TextWriter

        'Get nick, owner, server, port, and channel from user
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

        'Starting USER and NICK login commands 
        output.Write("USER " & nick & " 0 * :" & owner & vbCr & vbLf & "NICK " & nick & vbCr & vbLf)
        output.Flush()

        output.Write("MODE " & nick & " +B" & vbCr & vbLf)
        output.Flush()


        output.Write("JOIN #DSCM_Test" & vbCr & vbLf)
        output.Flush()

        'Process each line received from irc server
        While True
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

                If buf(0) <> ":"c Then
                    'Continue While
                End If
            End If
        End While
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        dgvInput.Columns.Add("text", "Text")
        dgvInput.Columns(0).Width = 700
        dgvInput.Font = New Font("Consolas", 10)


        refTimer = New System.Windows.Forms.Timer
        refTimer.Interval = 200
        refTimer.Enabled = True
        refTimer.Start()
    End Sub
End Class


