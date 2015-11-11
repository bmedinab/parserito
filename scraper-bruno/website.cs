using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scraper_bruno
{
    class Website
    {
        public string Url { get; set; }
        public bool wasFound { get; set; }
        public Dictionary<string , string> Languages { get; set; }
        public Dictionary<string, string> Cms { get; set; }
        public Dictionary<string, string> JSLibraries { get; set; }
        public override string ToString()
        {
            String result = "";
            if (!String.IsNullOrEmpty(Url))
            {
                result +=  "Url: " + Url + "\n";
            }
            if (Languages != null && Languages.Count != 0)
            {
                result +=  "Lenguajes: \n";
                foreach (var item in Languages)
                {
                    result += "Lenguaje: " + item.Key + ", Ver: " + item.Value + "\n";
                }
            }
            if (Cms!= null && Cms.Count != 0)
            {
                result +=  "CMS detectados: \n";
                foreach (var item in Cms)
                {
                    result +="Cms: " + item.Key +", Ver: "+ item.Value + "\n";
                }
            }
            if (JSLibraries != null && JSLibraries.Count != 0)
            {
                result +=  "JS libs: \n";
                foreach (var item in JSLibraries)
                {
                    result +=  "Lib: " + item.Key + ", Ver: " + item.Value + "\n";
                }
            }

            return result;
        }
    }
}
