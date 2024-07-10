using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;

TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();
var socket = server.AcceptSocket(); // wait for client

byte[] buffer = new byte[1024];
int received = socket.Receive(buffer); //receive the request text
string[] portions = ASCIIEncoding.UTF8.GetString(buffer).Split("\r\n"); //split it into its portions (Request line, headers, body)

var reqParts = portions[0].Split(" "); //split request into its portions(method, uri, httptype)
var (method, path, httpVer) = (reqParts[0], reqParts[1], reqParts[2]);

var response = "";
if(path == "/")
{
    response = "HTTP/1.1 200 OK\r\n\r\n";
}
else if (path.StartsWith("/echo/")){
    string content = path.Substring(6);
    response = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {content.Length}\r\n\r\n{content}";
}
//else if (path.StartsWith("/user-agent"))
//{
//    string content = portions[1].Split("\r\n")[1];
    
//    response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: ";
//}
else
{
    response = "HTTP/1.1 404 Not Found\r\n\r\n";
}

socket.Send(Encoding.UTF8.GetBytes(response));

socket.Close();
