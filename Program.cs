using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;


namespace Server
{
    class Program
    {
        const string hostname = "127.0.0.1";
        const int port = 3000;
        static Message[] messages = new Message[5];
        static void Main(string[] args)
        {
            HttpListener http = new HttpListener();
            http.Prefixes.Add($"http://{hostname}:{port}/");
            http.Start();

            while(true)
            {
                var context=http.GetContext();
                switch (context.Request.RawUrl)
                {
                    case "/":
                        BinaryReader reader = new BinaryReader(new FileStream("index.html",FileMode.Open, FileAccess.Read));
                        context.Response.StatusCode = 200;
                        context.Response.ContentType = "text/html";
                        context.Response.AddHeader("Charset", "UTF-8");
                        context.Response.OutputStream.Write(reader.ReadBytes((int)reader.BaseStream.Length));
                        context.Response.Close();
                        reader.Close();
                        break;
                    case "/list":
                        context.Response.StatusCode = 200;
                        context.Response.ContentType = "text/json";
                        context.Response.AddHeader("Charset", "UTF-8");
                        context.Response.OutputStream.Write(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messages)));
                        context.Response.Close();
                        break;
                    case "/add":
                        if (context.Request.HttpMethod != "POST")
                        {
                            context.Response.StatusCode = 400;
                            context.Response.Close();
                            break;
                        }
                        byte[] buffer = new byte[context.Request.ContentLength64];
                        context.Request.InputStream.Read(buffer);
                        for (int i = messages.Length - 1; i > 0; i--)
                            messages[i] = messages[i - 1];
                        messages[0] = JsonConvert.DeserializeObject<Message>(Encoding.UTF8.GetString(buffer),
                            new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                        context.Response.StatusCode = 202;
                        context.Response.Close();
                        break;
                    default:
                        context.Response.StatusCode = 404;
                        context.Response.Close();
                        break;
                }
            }
        }
        class Message
        {
            public string text;
            public string author;

        }

    }
}
