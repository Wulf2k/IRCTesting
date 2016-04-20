Imports System
Imports System.Net
Imports System.Net.Sockets
Imports System.IO
Imports System.Threading

Public Class Form1
    Private updTrd As Thread


    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        updTrd = New Thread(AddressOf connect)
        updTrd.IsBackground = True
        updTrd.Start()

    End Sub

    Private Sub connect()
        Dim foo As New My_IRC("irc.freenode.net", "#DSCM_test", "DSCMTest", 6667, False)
        foo.Connect()
    End Sub

    Public Class My_IRC
        Private _sServer As String = String.Empty '-- IRC server name
        Private _sChannel As String = String.Empty '-- the channel you want to join (prefex with #)
        Private _sNickName As String = String.Empty '-- the nick name you want show up in the side bar
        Private _lPort As Int32 = 6667 '-- the port to connect to.  Default is 6667
        Private _bInvisible As Boolean = False '-- shows up as an invisible user.  Still working on this.
        Private _sRealName As String = "DSCMbot" '-- More naming
        Private _sUserName As String = "DSCMbot" '-- Unique name so of the IRC network has a unique handle to you regardless of the nickname.

        Private _tcpclientConnection As TcpClient = Nothing '-- main connection to the IRC network.
        Private _networkStream As NetworkStream = Nothing '-- break that connection down to a network stream.
        Private _streamWriter As StreamWriter = Nothing '-- provide a convenient access to writing commands.

        Private _streamReader As StreamReader = Nothing '-- provide a convenient access to reading commands.

        Public Sub New(ByVal server As String, ByVal channel As String, ByVal nickname As String, ByVal port As Int32, ByVal invisible As Boolean)
            _sServer = server
            _sChannel = channel
            _sNickName = nickname
            _lPort = port
            _bInvisible = invisible
        End Sub

        Public Sub output(str As String)
            Form1.txtOutput.Text = str
            Console.WriteLine(str)
        End Sub

        Public Sub Connect()
            '-- Heads up - when sending a command you need to flush the writer each time.  That's key.
            Dim sIsInvisible As String = String.Empty
            Dim sCommand As String = String.Empty '-- commands to process from the room.

            '-- objects used for the IDENT response.
            Dim identListener As TcpListener = Nothing
            Dim identClient As TcpClient = Nothing
            Dim identNetworkStream As NetworkStream = Nothing
            Dim identStreamReader As StreamReader = Nothing
            Dim identStreamWriter As StreamWriter = Nothing
            Dim identResponseString As String = String.Empty

            Try
                '-- Start the main connection to the IRC server.
                output("**Creating Connection**")
                _tcpclientConnection = New TcpClient(_sServer, _lPort)
                _networkStream = _tcpclientConnection.GetStream
                _streamReader = New StreamReader(_networkStream)
                _streamWriter = New StreamWriter(_networkStream)

                '-- Yeah, questionable if this works all the time.
                If _bInvisible Then
                    sIsInvisible = 8
                Else
                    sIsInvisible = 0
                End If

                '-- Send in your information
                output("**Setting up name**")
                _streamWriter.WriteLine(String.Format("USER {0} {1} * :{2}", _sUserName, sIsInvisible, _sRealName))
                _streamWriter.Flush()

                '-- Create your nickname.
                output("**Setting Nickname**")
                _streamWriter.WriteLine(String.Format(String.Format("NICK {0}", _sNickName)))
                _streamWriter.Flush()

                '-- Tell the server you want to connect to a specific room.
                output("**Joining Room**")
                _streamWriter.WriteLine(String.Format("JOIN {0}", _sChannel))
                _streamWriter.Flush()




                While True
                    sCommand = _streamReader.ReadLine
                    output(sCommand)

                    '-- Not the best method but for the time being it works. 
                    '--
                    '-- Example of a command it picks up
                    ' :nodi123!nodi12312@ipxxx-xx.net PRIVMSG #nodi123_test :? hola!

                    Dim sCommandParts(sCommand.Split(" ").Length) As String
                    sCommandParts = sCommand.Split(" ")

                    If sCommandParts(0) = "PING" Then
                        Dim sPing As String = String.Empty
                        For i As Int32 = 1 To sCommandParts.Length - 1
                            sPing += sCommandParts(i) + " "
                        Next
                        _streamWriter.WriteLine("PONG " + sPing)
                        _streamWriter.Flush()
                        output("PONG " + sPing)
                    End If

                    '-- With my jank split command we want to look for specific commands sent and react to them!
                    '-- In theory this should be dumped to a method, but for this small tutorial you can see them here.
                    '-- Also any user can input this.  If you want to respond to commands from you only you would
                    '-- have to extend the program to look for your non-bot-id in the sCommandParts(0)
                    If sCommandParts.Length >= 4 Then
                        '-- If a statement is proceeded by a question mark (the semi colon's there automatically)
                        '-- then repeat the rest of the string!
                        If sCommandParts(3).StartsWith(":?") Then
                            Dim sVal As String = String.Empty
                            Dim sOut As String = String.Empty
                            '-- the text might have other spaces in them so concatenate the rest of the parts
                            '-- because it's all text.
                            For i As Int32 = 3 To sCommandParts.Length - 1
                                sVal += sCommandParts(i)
                                sVal += " "
                            Next
                            '-- remove the :? part.
                            sVal = sVal.Substring(2, sVal.Length - 2)
                            '-- Trim for good measure.
                            sVal = sVal.Trim
                            '-- Send the text back out.  The format is they command to send the text and the room you are in.
                            sOut = String.Format("PRIVMSG {0} : You said '{1}'", _sChannel, sVal)
                            _streamWriter.WriteLine(sOut)
                            _streamWriter.Flush()
                        End If
                        '-- If you don't quit the bot correctly the connection will be active until a ping/pong is failed. 
                        '-- Even if your programming isn't running!
                        '-- To stop that here's a command to have the bot quit!
                        If sCommandParts(3).Contains(":!Q") Then
                            ' Stop
                            _streamWriter.WriteLine("QUIT")
                            _streamWriter.Flush()
                            Exit Sub
                        End If
                    End If
                End While

            Catch ex As Exception
                '-- Any exception quits the bot gracefully.
                output("Error in Connecting.  " + ex.Message)
                _streamWriter.WriteLine("QUIT")
                _streamWriter.Flush()
            Finally
                '-- close your connections
                _streamReader.Dispose()
                _streamWriter.Dispose()
                _networkStream.Dispose()
            End Try

        End Sub
    End Class


End Class
