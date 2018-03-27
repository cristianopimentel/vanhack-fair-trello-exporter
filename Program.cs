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
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string List { get; set; } = "";
        public string Comments { get; set; } = "";

        public string[] ToCsv()
        {
            return new string[] { ("\"" + Name + "\"").Replace('\t', '*'), "\"" + Description + "\"", "\"" + List + "\"", "\"" + Comments + "\"" };
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            //  OUTPUT
            //  CardName, CardDesc, Labels, Comments, List, CardLink

            var ser = new JavaScriptSerializer();

            FileInfo input = new FileInfo(@"C:/Users/cpimentel/Desktop/vanhack_fair_sao_paulo_2018_skip_sunday_QI3Yfn31.json");

            var contents = File.ReadAllText(input.FullName);
            Dictionary<String, Object> root = (Dictionary<String, Object>)ser.DeserializeObject(contents);

            //  LOAD LISTS
            Dictionary<string, string> lists = new Dictionary<string, string>();
            foreach (Dictionary<String, Object> item in (object[])root["lists"])
                lists.Add((string)item["id"], (string)item["name"]);

            //  LOAD CARDS
            Dictionary<long, Card> cards = new Dictionary<long, Card>();
            foreach (Dictionary<String, Object> item in (object[])root["cards"])
            {
                Boolean closed = (Boolean)item["closed"];
                if (!closed)
                {
                    Card newCard = new Card();
                    newCard.Id = (int)item["idShort"];
                    newCard.Name = (string)item["name"];
                    newCard.Description = (string)item["desc"];
                    newCard.List = lists[(string)item["idList"]];

                    cards.Add(newCard.Id, newCard);
                }
            }

            //  LOAD ACTIONS
            //  updateCard
            //  commentCard
            //  updateList
            //Dictionary<long, Card> actions = new Dictionary<long, Card>();
            //foreach (Dictionary<String, Object> item in (object[])root["actions"])
            //{
            //    string type = (string)item["type"];
            //    switch (type)
            //    {
            //        case "commentCard":
                        


            //            Boolean closed = (Boolean)item["closed"];
            //            if (!closed)
            //            {
            //                Card newCard = new Card();
            //                newCard.Id = (int)item["idShort"];
            //                newCard.Name = (string)item["name"];
            //                newCard.Description = (string)item["desc"];
            //                newCard.List = lists[(string)item["idList"]];

            //                cards.Add(newCard.Id, newCard);
            //            }



            //            break;
            //        default:
            //            break;
            //    }
            //}

            string[] header = new string[] { "Name", "Description", "List", "Comments" };

            FileInfo output = new FileInfo(input.FullName + ".csv");
            using (StreamWriter csv = new StreamWriter(File.Open(output.FullName, FileMode.Create), Encoding.UTF8))
            {
                csv.WriteLine(string.Join(",", header));

                foreach (Card card in cards.Values)
                    csv.WriteLine(string.Join(",", card.ToCsv()));
            }

            Console.WriteLine("CSV: " + output.FullName);
        }
    }
}
