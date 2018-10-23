using Newtonsoft.Json;

namespace Client
{
    
    /// <summary>
    /// JSON Serialization and Deserialization Assistant Class
    /// </summary>
    public class JsonHelper
    {
        public static string JSONSerilaize<T>(T obj)
        {
            // Convert object to JOSN string format   
            return JsonConvert.SerializeObject(obj);
            ///string cityJson = JsonConvert.SerializeObject(c, Formatting.Indented);
            /// {
            ///   "Name": "Oslo",
            ///   "Population": 650000
            /// }  
                            
            ///json = JsonConvert.SerializeObject(red, new JsonSerializerSettings
            ///{
            ///    Formatting = Formatting.Indented,
            ///    Converters = { new HtmlColorConverter() }
            ///});
        }

        public static T JSONDeserialize<T>(string jsonString)
        {
            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        //example
        /*JsonObject obj = new JsonObject();
            List<Prog> l = new List<Prog>();
            Prog p1 = new Prog();
            p1.icon = "ciao icona";
            p1.title = "ciao titolo";
            l.Add(p1);
            Prog p2 = new Prog();
            p2.icon = "ciao icona2";
            p2.title = "ciao titolo 2";
            l.Add(p2);
            obj.list = l;
            obj.nameF = "word";
            string s = JsonHelper.JSONSerilaize<JsonObject>(obj);*/

        /*string json = @"{  
            'list': '',  
            'nameF': 'Manas',   
            }";
        JsonObject obj = JsonHelper.JSONDeserialize<JsonObject>(json);*/
    }
}
