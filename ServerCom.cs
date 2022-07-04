using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class ServerCom
{

    public static string SERVER_IP = "";
    private const int SERVER_PORT = 2050;

    public static string SendData(string data, out Exception exception, string ip = "")
    {
        try
        {
            if (ip == "") ip = SERVER_IP;
            exception = null;
            byte[] d = Encoding.UTF8.GetBytes(data + "|");
            Console.WriteLine(d.Length);
            TcpClient client = new TcpClient(ip, SERVER_PORT);

            NetworkStream stream = client.GetStream();

            stream.Write(d, 0, d.Length);

            exception = null;
            byte[] ndata = new byte[client.ReceiveBufferSize];

            int a = stream.Read(ndata, 0, ndata.Length);
            Console.WriteLine(":" + Encoding.UTF8.GetString(ndata) + ":");
            client.Close();
            return Encoding.UTF8.GetString(ndata);
        }
        catch (Exception ex)
        {
            exception = ex;
            //Console.WriteLine(ex.ToString());
            return "";
        }
    }

}