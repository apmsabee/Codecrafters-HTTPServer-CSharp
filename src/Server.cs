using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Collections;

internal class Program
{
    private static void Main(string[] args)
    {
        TcpListener server = new TcpListener(IPAddress.Any, 4221);
        server.Start();

        while (true)
        {
            var socket = server.AcceptSocket(); // wait for client
            _ = Task.Run(() => HandleConnection(socket)); //Handle an individual connection(can run multiple times)
        }

        void HandleConnection(Socket socket)
        {
            try
            {
                byte[] buffer = new byte[4096];
                int received = socket.Receive(buffer); //receive the request text

                string[] portions = ASCIIEncoding.UTF8.GetString(buffer).Split("\r\n"); //split it into its portions (Request line, headers, body)
                var reqParts = portions[0].Split(" "); //split request into its portions(method, uri, httptype)
                var (method, path, httpVer) = (reqParts[0], reqParts[1], reqParts[2]);

                var response = "";
                var content = "";

                Console.WriteLine("Path Substring: " + path.Substring(7));
                if (path == "/")
                {
                    Console.WriteLine("Empty path return");
                    response = "HTTP/1.1 200 OK\r\n\r\n";
                }
                else if (path.StartsWith("/echo/"))
                {
                    Console.WriteLine("Echo path return");
                    content = path.Substring(6);
                    response = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {content.Length}\r\n\r\n{content}";
                }
                else if (path.StartsWith("/user-agent"))
                {
                    Console.WriteLine("User-agent path return");
                    content = portions[2].Split(" ")[1];
                    response = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {content.Length}\r\n\r\n{content}";
                    Console.WriteLine(response);
                }
                else if (path.StartsWith("/files/")){
                    Console.WriteLine("In files branch");
                    Console.WriteLine(args[1]);
                    string dir = Path.Combine(args[0], path.Substring(7));
                    Console.WriteLine(File.Exists(dir));
                    if (File.Exists(dir))
                    {
                        //long length = new System.IO.FileInfo(path.Substring(7)).Length;
                        //var f = File.ReadAllBytes(path.Substring(7));
                        //response = $"HTTP/1.1 200 OK\r\nContent-Type: application/octet-stream\r\nContent-Length: {length}\r\n\r\n{f}";
                    }
                    else
                    {
                        response = "HTTP/1.1 404 Not Found\r\n\r\n";
                    }
                }
                else
                {
                    Console.WriteLine("404 path return");
                    response = "HTTP/1.1 404 Not Found\r\n\r\n";
                }
                Console.WriteLine("Response: " + response);
                socket.Send(Encoding.UTF8.GetBytes(response));
            }
            catch(Exception e) 
            {
                Console.WriteLine("In catch statement");
                Console.WriteLine(e.ToString());
                socket.Send(Encoding.UTF8.GetBytes("HTTP/1.1 500 Internal Server Error\r\n\r\n"));
            }
            finally
            {
                socket.Close();
            }
        }
    }
}

