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
        public bool WasDetected { get; set; }
        public string CMSName { get; set;}
        public string CMSVersion { get; set; }
        public string LanguageName { get; set; }
        public string LanguageVersion { get; set; }
        public string JSLibName { get; set; }
        public string JSLibVersion { get; set; }


    }
}
