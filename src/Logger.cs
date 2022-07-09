namespace EcpEmuServer
{
    public static class Logger
    {
        public static readonly string logFile = $"logs/EcpEmuServer-{DateTime.Now.ToString().Replace(':', '-').Replace('/', '-').Replace(' ', '-')}.log";

        public static bool LoggerInitiated = false;

        public static void Init()
        {
            LoggerInitiated = true;
            //Directory.CreateDirectory("logs");
            //File.CreateText(logFile).Close();
        }
        public static void Log(LogSeverity severity, string text)
        {
            if (!LoggerInitiated) { Init(); }

            Console.Write("[");

            string severitySymbol = GetSeveritySymbol(severity);
            ConsoleColor severityColor = Console.ForegroundColor;
            Console.Write(severitySymbol);
            Console.ForegroundColor = ConsoleColor.Gray;

            Console.Write("] ");

            Console.ForegroundColor = severityColor;
            Console.Write(severity);
            Console.ForegroundColor = ConsoleColor.Gray;

            Console.Write($": {text}\n");

            /*
            using (StreamWriter logWriter = File.AppendText(logFile))
            {
                logWriter.WriteLine($"[{severitySymbol}] {severity}: {text}");
            }
            */
        }

        internal static string GetSeveritySymbol(LogSeverity severity)
        {
            switch (severity)
            {
                case LogSeverity.info:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    return "i";
                case LogSeverity.warn:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    return "!";
                case LogSeverity.error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    return "x";
                case LogSeverity.success:
                    Console.ForegroundColor = ConsoleColor.Green;
                    return "*";
                case LogSeverity.debug:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    return "x";
                default:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    return " ";
            }
        }

        public enum LogSeverity
        {
            info, // [i] info: <text>
            warn, // [!] warn: <text>
            error, // [x] error: <text>
            success, // [*] success: <text>
            debug // [~] debug: <text>
        }
    }
}
