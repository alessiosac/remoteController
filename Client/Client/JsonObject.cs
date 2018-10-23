using System.Collections.Generic;

namespace Client
{
    public class JsonObject
    {
        public List<Prog> list { get; set; }
        public string nameF { get; set; }
        public int handle { get; set; }
    }

    public class Prog
    {
        public string title { get; set; }
        public int handle { get; set; }
        public string icon { get; set; }
    }

    public class CommandObject
    {
        public string Application { get; set; }
        public int Tasti { get; set; }
        public List<Input> Lista { get; set; }
    }

    public class Input
    {
        public int vKey { get; set; }
        public int scanCode { get; set; }
    }
}
