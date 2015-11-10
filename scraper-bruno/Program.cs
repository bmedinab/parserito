using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.Threading.Tasks;
using scraper_bruno;

namespace GetLinks
{
    class Program
    {
        static List<Website> Lista = new List<Website>();
        static void Main(string[] args)
        {
            List<string> list = new List<string>();
            list.Add("edu.mx");
            list.Add("skyalert.mx");
            list.Add("andrea.com");
            list.Add("iberopuebla.mx");

            ParallelOptions ops = new ParallelOptions();
            ops.MaxDegreeOfParallelism = -1;

            if(Parallel.ForEach(list, ops, (url) => {
                System.Console.WriteLine("inicio: " + url);
                Execute(url);
            }).IsCompleted)
            {
                System.Console.WriteLine("terminé!");
            }
            int count = 1;
            foreach (var item in Lista)
            {
                
                System.Console.WriteLine("\nElemento " + count+ " :\n");
                System.Console.WriteLine(item);
                count++;

            }
            System.Console.WriteLine(count + " websites fueron escaneados");

            Console.ReadKey();
        }

        private static void Execute(string url)
        {

            if (String.IsNullOrEmpty(url))
            {
                return;
            }
            Website result = new Website();
            result.Url = url;
            bool getLanguage, getCms, getJsLibraries;
            getLanguage = getCms = getJsLibraries = false;
            HtmlDocument doc = new HtmlWeb().Load("http://w3techs.com/sites/info/" + url);
            HtmlNode main = doc.DocumentNode.SelectSingleNode("//*[@class='tech_main']");
            HtmlNode site = main.SelectSingleNode(".//h1");
            foreach (HtmlNode node in main.ChildNodes)
            {
                if (getLanguage)
                {
                    if (node.OuterHtml.Contains("si_tech") && !node.OuterHtml.Contains("si_tech_np"))
                    {
                        result.Languages = new Dictionary<string, string>();
                        HtmlNode langName = node.NextSibling;
                        HtmlNode langVersion = langName.NextSibling;
                        if (!String.IsNullOrEmpty(langName.InnerText))
                        {
                            result.Languages.Add(langName.InnerText.Trim(), (String.IsNullOrEmpty(langVersion.InnerText)) ? "N/A" : langVersion.InnerText.Trim());
                        }
                        getLanguage = false;
                    }

                }
                else if (getCms)
                {
                    if (node.OuterHtml.Contains("si_tech") && !node.OuterHtml.Contains("si_tech_np"))
                    {
                        result.Cms = new Dictionary<string, string>();
                        HtmlNode cmsName = node.NextSibling;
                        HtmlNode cmsVersion = cmsName.NextSibling;
                        if (!String.IsNullOrEmpty(cmsName.InnerText))
                        {
                            result.Cms.Add(cmsName.InnerText.Trim(), (String.IsNullOrEmpty(cmsVersion.InnerText)) ? "N/A" : cmsVersion.InnerText.Trim());
                        }
                        getCms = false;
                    }

                }
                else if (getJsLibraries)
                {
                    if (node.OuterHtml.Contains("si_tech") && !node.OuterHtml.Contains("si_tech_np"))
                    {
                        result.JSLibraries = new Dictionary<string, string>();
                        HtmlNode libraryName = node.NextSibling;
                        HtmlNode libraryVersion = libraryName.NextSibling;
                        if (!String.IsNullOrEmpty(libraryName.InnerText))
                        {
                            result.JSLibraries.Add(libraryName.InnerText.Trim(), (String.IsNullOrEmpty(libraryVersion.InnerText)) ? "N/A" : libraryVersion.InnerText.Trim());
                        }
                        getJsLibraries = false;
                    }
                }
                if (node.InnerText.Equals("Server-side Programming Language"))
                {
                    getLanguage = true;
                    //Console.WriteLine(node.InnerText);
                }
                else if (node.InnerText.Equals("Content Management System"))
                {
                    getCms = true;
                    //Console.WriteLine(node.InnerText);
                }
                else if (node.InnerText.Equals("JavaScript Libraries"))
                {
                    getJsLibraries = true;
                    //Console.WriteLine(node.InnerText);
                }


            }

            Lista.Add(result);
        } 
    }
}
