﻿using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

#pragma warning disable CS8618 // Although the compiler warns that it could be null, it actually will never be
#pragma warning disable CS8622 // Same thing.

namespace TwitchPlaysGames
{
    public class ChatBot
    {
        const string ip = "irc.chat.twitch.tv";
        const int port = 6697; // Use the more secure SSL

        private string nick;
        private string password;
        private StreamReader streamReader;
        private StreamWriter streamWriter;
        private TaskCompletionSource<int> connected = new TaskCompletionSource<int>();

        public event TwitchChatEventHandler OnMessage = delegate { };
        public delegate void TwitchChatEventHandler(object sender, TwitchChatMessage e);

        public class TwitchChatMessage : EventArgs
        {
            public string Sender { get; set; }
            public string Message { get; set; }
            public string Channel { get; set; }
        }

        public ChatBot(string nick, string password)
        {
            this.nick = nick;
            this.password = password;
        }

        public async Task Start()
        {
            var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(ip, port);
            SslStream sslStream = new SslStream(
                tcpClient.GetStream(),
                false,
                ValidateServerCertificate,
                null
            );
            await sslStream.AuthenticateAsClientAsync(ip);
            streamReader = new StreamReader(sslStream);
            streamWriter = new StreamWriter(sslStream) { NewLine = "\r\n", AutoFlush = true };

            await streamWriter.WriteLineAsync($"PASS {password}");
            await streamWriter.WriteLineAsync($"NICK {nick}");
            connected.SetResult(0);

            while (true)
            {
                string? line = await streamReader.ReadLineAsync();
                Console.WriteLine(line);

                if (line != null)
                {
                    string[] split = line.Split(" ");
                    //PING :tmi.twitch.tv
                    //Respond with PONG :tmi.twitch.tv
                    if (line.StartsWith("PING"))
                    {
                        Console.WriteLine("PONG");
                        await streamWriter.WriteLineAsync($"PONG {split[1]}");
                    }

                    if (split.Length > 2 && split[1] == "PRIVMSG")
                    {
                        //:mytwitchchannel!mytwitchchannel@mytwitchchannel.tmi.twitch.tv 
                        // ^^^^^^^^
                        //Grab this name here
                        int exclamationPointPosition = split[0].IndexOf("!");
                        string username = split[0].Substring(1, exclamationPointPosition - 1);
                        //Skip the first character, the first colon, then find the next colon
                        int secondColonPosition = line.IndexOf(':', 1);//the 1 here is what skips the first character
                        string message = line.Substring(secondColonPosition + 1);//Everything past the second colon
                         string channel = split[2].TrimStart('#');

                        OnMessage(this, new TwitchChatMessage
                        {
                            Message = message,
                            Sender = username,
                            Channel = channel
                        });
                    }
                }
            }
        }

        public async Task SendMessage(string channel, string message)
        {
            await connected.Task;
            await streamWriter.WriteLineAsync($"PRIVMSG #{channel} :{message}");
        }

        public async Task JoinChannel(string channel)
        {
            await connected.Task;
            await streamWriter.WriteLineAsync($"JOIN #{channel}");
        }

        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return sslPolicyErrors == SslPolicyErrors.None;
        }
    }
}