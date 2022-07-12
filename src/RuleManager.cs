using System.Diagnostics;
using System.Net;
using System.Xml.Serialization;

namespace EcpEmuServer
{
    public class RuleManager
    {
        internal HttpClient httpClient = new HttpClient();
        internal RuleList rules = new RuleList();
        public RuleManager()
        {
            Logger.Log(Logger.LogSeverity.info, "RuleManager running, loading rules...");

            XmlSerializer serializer = new XmlSerializer(rules.GetType());

            if (!File.Exists("./rules.xml"))
            {
                Logger.Log(Logger.LogSeverity.warn, "rules.xml was not found, generating blank...");

                using (StreamWriter writer = new StreamWriter(File.Create("./rules.xml")))
                {
                    // Default configuration
                    writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                                     "<ecpemuserver xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\n" +
                                     "  <rules>\n" +
                                     "    <rule>\n" +
                                     "      <Name>New Rule</Name>\n" +
                                     "      <Button>None</Button>\n" +
                                     "      <Action>HttpGET</Action>\n" +
                                     "      <EndPoint>https://www.example.com/</EndPoint>\n" +
                                     "      <ExData> </ExData>\n" +
                                     "    </rule>\n" +
                                     "  </rules>\n" +
                                     "</ecpemuserver>");
                }
                
                Logger.Log(Logger.LogSeverity.info, "Generated new rules.xml, please configure EcpEmuServer rules and restart");
            }

            // Load rules from XML
            using (StreamReader reader = new StreamReader(File.OpenRead("./rules.xml")))
            {
                rules = (RuleList)serializer.Deserialize(reader);
                if (rules == null)
                {
                    Logger.Log(Logger.LogSeverity.error, $"Couldn't load rules from rules.xml, no actions will be taken.");
                    rules = new RuleList();
                }
            }

            if (rules.ruleList.Count > 0)
            {
                foreach (Rule rule in rules.ruleList)
                {
                    Logger.Log(Logger.LogSeverity.success, $"Loaded rule \"{rule.Name}\" from rules.xml for button \"{rule.Button}\"");
                }
            }        
        }

        public void Execute(string button)
        {
            try
            {
                HttpStatusCode statusCode = HttpStatusCode.Unused;

                foreach (Rule rule in rules.ruleList)
                {
                    if (rule.Button == button)
                    {
                        switch (rule.Action)
                        {
                            case RuleAction.HttpGET:
                                statusCode = httpClient.GetAsync(rule.EndPoint).Result.StatusCode;
                                break;
                            case RuleAction.HttpPOST:
                                statusCode = httpClient.PostAsync(rule.EndPoint, new StringContent(rule.ExData)).Result.StatusCode;
                                break;
                            case RuleAction.Execute:
                                ProcessStartInfo startInfo = new();
                                startInfo.UseShellExecute = false;
                                startInfo.FileName = rule.EndPoint;
                                startInfo.WorkingDirectory = Path.GetFullPath(rule.EndPoint);
                                startInfo.Arguments = rule.ExData;
                                startInfo.RedirectStandardOutput = true;

                                using (Process proc = Process.Start(startInfo))
                                {
                                    while (!proc.StandardOutput.EndOfStream) Console.WriteLine(proc.StandardOutput.ReadLine());
                                    proc.WaitForExit();
                                }
                                break;
                            default:
                                break;
                        }

                        switch (statusCode)
                        {
                            case HttpStatusCode.NotFound:
                                throw new BadHttpRequestException($"Rule {rule.Name} failed, got HTTP {HttpStatusCode.NotFound}");
                                break;
                            case HttpStatusCode.OK:
                                Logger.Log(Logger.LogSeverity.success, $"Rule {rule.Name} sent, got HTTP {HttpStatusCode.OK}");
                                break;
                            default:
                                Logger.Log(Logger.LogSeverity.info, $"Rule {rule.Name} ran");
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(Logger.LogSeverity.error, ex.ToString());
            }
        }
    }

    [XmlRoot("ecpemuserver")]
    public class RuleList
    {
        [XmlArray("rules")]

        [XmlArrayItem("rule", typeof(Rule))]
        public List<Rule> ruleList;

        public RuleList()
        {
            ruleList = new List<Rule>();
        }

        public void AddRule(Rule rule)
        {
            ruleList.Add(rule);
        }
    }

    public class Rule
    {
        private string name;
        private string button;
        private RuleAction action;
        private string endPoint;
        private string exData;

        public Rule()
        {
            this.name = "";
            this.button = "";
            this.action = RuleAction.HttpGET;
            this.endPoint = "";
            this.exData = "";
        }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }
        public string Button
        {
            get
            {
                return button;
            }
            set
            {
                button = value;
            }
        }
        public RuleAction Action
        {
            get
            {
                return action;
            }
            set
            {
                action = value;
            }
        }
        public string EndPoint
        {
            get
            {
                return endPoint;
            }
            set
            {
                endPoint = value;
            }
        }
        public string ExData
        {
            get
            {
                return exData;
            }
            set
            {
                exData = value;
            }
        }
    }

    public enum RuleAction
    {
        HttpGET,
        HttpPOST,
        Execute
    }
}
