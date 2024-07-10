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
var startIndex = portions[0].IndexOf("/");
string substring = portions[0].Substring(startIndex);
var endIndex = substring.IndexOf(" ");
string uri = substring.Substring(0, endIndex);

var response = (uri != "/") ? 
    "HTTP/1.1 404 Not Found\r\n\r\n" : 
    "HTTP/1.1 200 OK\r\n\r\n";
socket.Send(Encoding.UTF8.GetBytes(response));
socket.Close();
