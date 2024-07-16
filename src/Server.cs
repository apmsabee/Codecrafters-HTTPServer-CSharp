using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Collections;
using System.IO.Compression;

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
                
                bool encoding = portions[2].StartsWith("Accept-Encoding");
                bool validEncoding = (encoding) ? 
                    portions[2].Contains("gzip")
                    : false;

                var reqParts = portions[0].Split(" ");
                var (method, path, httpVer) = (reqParts[0], reqParts[1], reqParts[2]);

                var response = "";
                var content = "";

                if (path == "/")
                {
                    response = "HTTP/1.1 200 OK\r\n\r\n";
                }
                else if (path.StartsWith("/echo/"))
                {
                    content = path.Substring(6);
                    var compressedContent = Zip(content);
                    Console.WriteLine(Encoding.UTF8.GetString(compressedContent));
                    Console.WriteLine(Convert.ToBase64String(compressedContent));
                    response = (validEncoding) ?
                        $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Encoding: gzip\r\nContent-Length: {compressedContent.Length}\r\n\r\n{Convert.ToBase64String(compressedContent)}"
                        : $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {content.Length}\r\n\r\n{content}";
                }
                else if (path.StartsWith("/user-agent"))
                {
                    content = portions[2].Split(" ")[1];
                    response = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {content.Length}\r\n\r\n{content}";
                }
                else if (path.StartsWith("/files/")){
                    if (method == "POST")
                    {
                        try
                        {
                            string newFilePath = Path.Combine(args[1], path.Substring(7));
                            using (StreamWriter output = new StreamWriter(newFilePath))
                            {
                                byte[] dataAsBytes = Encoding.ASCII.GetBytes(portions[5]);
                                int numBytes = Int32.Parse(portions[2].Split(' ')[1]);
                                output.BaseStream.Write(dataAsBytes, 0, numBytes);
                                response = "HTTP/1.1 201 Created\r\n\r\n";
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                            response = "HTTP/1.1 500 Internal Server Error\r\n\r\n";
                        }
                    }
                    else
                    {
                        try
                        {
                            long length = new System.IO.FileInfo(Path.Combine(args[1], path.Substring(7))).Length;
                            var f = File.ReadAllText(Path.Combine(args[1], path.Substring(7)));
                            response = $"HTTP/1.1 200 OK\r\nContent-Type: application/octet-stream\r\nContent-Length: {length}\r\n\r\n{f}";
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                            response = "HTTP/1.1 404 Not Found\r\n\r\n";
                        }
                    }
                    
                }
                else
                {
                    response = "HTTP/1.1 404 Not Found\r\n\r\n";
                }
                //Console.WriteLine("Response: " + response);
                socket.Send(Encoding.UTF8.GetBytes(response));
            }
            catch(Exception e) 
            {
                Console.WriteLine("In overall catch statement");
                Console.WriteLine(e.ToString());
                socket.Send(Encoding.UTF8.GetBytes("HTTP/1.1 500 Internal Server Error\r\n\r\n"));
            }
            finally
            {
                socket.Close();
            }
        }
        static byte[] Zip(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            using var mso = new MemoryStream();

            using var gs = new GZipStream(mso, CompressionMode.Compress, true);
            gs.Write(bytes, 0, bytes.Length);
            gs.Flush();
            gs.Close();
            return  mso.ToArray();


               
        }
    }
}

