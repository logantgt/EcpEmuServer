using System.Net;
using System.Xml;
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

                Rule blankRule = new Rule();
                blankRule.Name = "New Rule";
                blankRule.Button = "None";
                blankRule.Action = HttpAction.GET;
                blankRule.EndPoint = "https://www.example.com/";

                rules.AddRule(blankRule);

                XmlWriterSettings writerSettings = new XmlWriterSettings();
                writerSettings.Indent = true;

                using (XmlWriter writer = XmlWriter.Create(File.CreateText("./rules.xml"), writerSettings))
                {
                    serializer.Serialize(writer, rules);
                }

                Logger.Log(Logger.LogSeverity.info, "Generated new rules.xml, please configure EcpEmuServer rules and restart");
            }
            else
            {
                // Load rules from XML
                using (XmlReader reader = XmlReader.Create(new StreamReader("./rules.xml")))
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
                        Logger.Log(Logger.LogSeverity.info, $"Loaded rule \"{rule.Name}\" from rules.xml for button \"{rule.Button}\"");
                    }
                }
            }
        }

        public void Execute(string button)
        {
            HttpStatusCode statusCode = HttpStatusCode.NotFound;

            foreach (Rule rule in rules.ruleList)
            {
                if (rule.Button == button)
                {
                    switch (rule.Action)
                    {
                        case HttpAction.GET:
                            try
                            {
                                statusCode = httpClient.GetAsync(rule.EndPoint).Result.StatusCode;
                            }
                            catch
                            {
                                statusCode = HttpStatusCode.NotFound;
                            }
                            break;
                        case HttpAction.POST:
                            try
                            {
                                statusCode = httpClient.PostAsync(rule.EndPoint, new StringContent("")).Result.StatusCode;
                            }
                            catch
                            {
                                statusCode = HttpStatusCode.NotFound;
                            }
                            break;
                        default:
                            break;
                    }

                    switch (statusCode)
                    {
                        case HttpStatusCode.NotFound:
                            Logger.Log(Logger.LogSeverity.error, $"Rule {rule.Name} failed, got HTTP {HttpStatusCode.NotFound}");
                            break;
                        case HttpStatusCode.OK:
                            Logger.Log(Logger.LogSeverity.success, $"Rule {rule.Name} sent, got HTTP {HttpStatusCode.OK}");
                            break;
                        default:
                            Logger.Log(Logger.LogSeverity.info, $"Rule {rule.Name} sent, got HTTP {statusCode}");
                            break;
                    }
                }
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
        private HttpAction action;
        private string endPoint;

        public Rule()
        {
            this.name = "";
            this.button = "";
            this.action = HttpAction.GET;
            this.endPoint = "";
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
        public HttpAction Action
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
    }

    public enum HttpAction
    {
        GET,
        POST
    }
}
