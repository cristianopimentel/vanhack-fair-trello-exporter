using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace fair
{
    public class Card
    {
        public int Id { get; set; }
        private string _CardName = "";
        public string Description { get; set; } = "";
        public string List { get; set; } = "";
        public string Comments { get; set; } = "";
        public string ShortUrl { get; set; } = "";

        public string Time { get; private set; }
        public string Number { get; private set; }
        public string Name { get; private set; }
        public string Email { get; private set; }
        public string Profile { get; private set; }

        public string CardName
        {
            get
            {
                return _CardName;
            }
            set
            {
                _CardName = value.Replace('\t', ' ');

                string[] parts = value.Split('\t');
                if (parts.Length == 1)
                    parts = value.Split(' ');

                //  Find email. This "sets" the position for the other data
                //  This is the simples implementation possible, using "human" algoritm
                //  Perhaps using RegEx here would extract everything we need
                int emailPosition;
                for (emailPosition = 0; emailPosition < parts.Length; emailPosition++)
                {
                    if (parts[emailPosition].Contains("@"))
                    {
                        this.Email = parts[emailPosition].Trim();
                        break;
                    }
                }

                //  Gets TIME, NUMBER and NAME if they exists. They come in this order.
                for (int i = emailPosition - 1; i >= 0; i--)
                {
                    if (parts[i].Contains(":"))
                    {
                        this.Time = parts[i].Trim();
                    }
                    else
                    {
                        int number = 0;
                        if (int.TryParse(parts[i].Trim(), out number))
                            this.Number = number.ToString();
                        else
                            this.Name = parts[i] + " " + this.Name;
                    }
                }

                //  Gets ROLE
                for (int i = emailPosition + 1; i < parts.Length; i++)
                    this.Profile += " " + parts[i];
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {

            var ser = new JavaScriptSerializer();

            FileInfo input = new FileInfo(@"C:/Users/cpimentel/Desktop/vanhack_fair_sao_paulo_2018_skip_sunday_QI3Yfn31.json");

            var contents = File.ReadAllText(input.FullName);
            Dictionary<String, Object> root = (Dictionary<String, Object>)ser.DeserializeObject(contents);

            //  LOAD LISTS
            Dictionary<string, string> lists = new Dictionary<string, string>();
            foreach (Dictionary<String, Object> item in (object[])root["lists"]) {
                Boolean closed = (Boolean)item["closed"];
                if (!closed)
                    lists.Add((string)item["id"], (string)item["name"]);
            }

            //  LOAD CARDS
            Dictionary<long, Card> cards = new Dictionary<long, Card>();
            foreach (Dictionary<String, Object> item in (object[])root["cards"])
            {
                Boolean closed = (Boolean)item["closed"];
                if (!closed && lists.ContainsKey((string)item["idList"]))
                {
                    Card newCard = new Card();
                    newCard.Id = (int)item["idShort"];
                    newCard.CardName = (string)item["name"];
                    newCard.Description = (string)item["desc"];
                    newCard.List = lists[(string)item["idList"]];
                    newCard.ShortUrl = (string)item["shortUrl"];

                    cards.Add(newCard.Id, newCard);
                }
            }

            //  LOAD ACTIONS
            //  updateCard
            //  commentCard
            //  updateList
            Dictionary<long, Card> actions = new Dictionary<long, Card>();
            foreach (Dictionary<String, Object> item in (object[])root["actions"])
            {
                string type = (string)item["type"];
                switch (type)
                {
                    case "commentCard":
                        Dictionary<String, Object> data = (Dictionary<String, Object>)item["data"];
                        Dictionary<String, Object> dataCard = (Dictionary<String, Object>)data["card"];

                        if (!cards.ContainsKey((int)dataCard["idShort"]))
                            continue;

                        Card card = cards[(int)dataCard["idShort"]];
                        if (card != null) {
                            Dictionary<String, Object> memberCreator = (Dictionary<String, Object>)item["memberCreator"];

                            string comment = (string)memberCreator["fullName"] + ": " + (string)data["text"];
                            comment = comment.Replace("\"", "\"\"").Replace("\n", "").Replace("\r", "");

                            if (card.Comments.Length == 0)
                                card.Comments += comment;
                            else
                                card.Comments += "\n" + comment;
                        } 

                        break;
                    default:
                        break;
                }
            }

            //  OUTPUT
            //  CardName, CardDesc, Labels, Comments, List, CardLink
            string[] header = new string[] { "Time", "Number", "Name", "Email", "Profile", "Card Name", "Description", "List", "Comments", "Card Url" };

            FileInfo output = new FileInfo(input.FullName + ".csv");
            using (StreamWriter csv = new StreamWriter(File.Open(output.FullName, FileMode.Create), Encoding.UTF8))
            {
                csv.WriteLine(string.Join(",", header));

                foreach (Card card in cards.Values)
                {
                    string[] record = new string[] {
                        "\"" + card.Time + "\"",
                        "\"" + card.Number + "\"",
                        "\"" + card.Name + "\"",
                        "\"" + card.Email + "\"",
                        "\"" + card.Profile + "\"",
                        ("\"" + card.CardName + "\"").Replace('\t', ' '),
                        "\"" + card.Description + "\"",
                        "\"" + card.List + "\"",
                        "\"" + card.Comments + "\"",
                        card.ShortUrl };

                    csv.WriteLine(string.Join(",", record));
                }
            }

            Console.WriteLine("CSV: " + output.FullName);
        }
    }
}
