using System;
using System.IO;
using System.Linq;
using System.Threading;

public enum GPIODirection
{
    In,
    Out
}

public class TinyGPIO : IDisposable
{
    public class Debug
    {
        public static string DebugPath { get { return AppDomain.CurrentDomain.BaseDirectory; } }
    }

    private static string BasePath { get { return "/sys/class/gpio"; } }

    private static string ConvertPath(string path)
    {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            var debugDevPath = Debug.DebugPath;
            path = path.TrimStart('/').Replace('/', '\\');
            var convertedPath = Path.Combine(debugDevPath, path);
            var dir = Path.GetDirectoryName(convertedPath);
            if (Directory.Exists(dir) == false) Directory.CreateDirectory(dir);
            return convertedPath;
        }
        else
        {
            return path;
        }
    }

    private static void RawWrite(string path, object value)
    {
        File.WriteAllText(ConvertPath(path), value.ToString());
    }

    private static string RawRead(string path)
    {
        return File.ReadAllLines(ConvertPath(path)).FirstOrDefault();
    }

    public static TinyGPIO Export(int port, GPIODirection? direction = null)
    {
        var portPath = GetGPIOPortPath(port);
        if (!Directory.Exists(portPath))
        {
            RawWrite(BasePath + "/export", port.ToString());
            
            // Why need 100msec wait? > http://qiita.com/aquahika/items/8911739147fa9620da6e
            Thread.Sleep(100);
        }
        var gpio = new TinyGPIO(port);
        if (direction.HasValue) gpio.Direction = direction.Value;
        return gpio;
    }

    public static bool Unexport(int port)
    {
        var portPath = GetGPIOPortPath(port);
        if (Directory.Exists(portPath))
        {
            RawWrite(BasePath + "/unexport", port.ToString());
            return true;
        }
        else return false;
    }

    private string PortPath { get; set; }

    public int Port { get; private set; }

    public int Value
    {
        get { return int.Parse(this.Read("value")); }
        set { this.Write("value", value); }
    }

    public GPIODirection Direction
    {
        get { return (GPIODirection)Enum.Parse(typeof(GPIODirection), this.Read("direction"), ignoreCase: true); }
        set { this.Write("direction", value.ToString().ToLower()); }
    }

    private TinyGPIO(int port)
    {
        this.Port = port;
        this.PortPath = GetGPIOPortPath(port);
    }

    private static string GetGPIOPortPath(int port)
    {
        return BasePath + "/gpio" + port.ToString();
    }

    private void Write(string name, object value)
    {
        name = name.TrimStart('/');
        RawWrite(this.PortPath + "/" + name, value);
    }

    private string Read(string name)
    {
        name = name.TrimStart('/');
        return RawRead(this.PortPath + "/" + name);
    }

    public void Dispose()
    {
        TinyGPIO.Unexport(this.Port);
    }
}
