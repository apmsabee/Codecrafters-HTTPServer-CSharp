using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();
var socket = server.AcceptSocket(); // wait for client

byte[] buffer = new byte[1024];
int received = socket.Receive(buffer); //receive the request text
string[] portions = ASCIIEncoding.UTF8.GetString(buffer).Split("\r\n"); //split it into its portions (Request line, headers, body)
Console.WriteLine(portions[0]);
var response = (portions[0].Contains("abcdefg")) ? 
    "HTTP/1.1 400 Not Found\r\n\r\n" : 
    "HTTP/1.1 200 OK\r\n\r\n";
socket.Send(Encoding.UTF8.GetBytes(response));
socket.Close();
