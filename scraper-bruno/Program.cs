using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.Threading.Tasks;
using scraper_bruno;
using System.IO;
using CsvHelper;

namespace GetLinks
{
    class Program
    {
        static List<Website> Lista = new List<Website>();
        static void Main(string[] args)
        {
            //List<string> list = new List<string>();
            //list.Add("edu.mx");
            //list.Add("skyalert.mx");
            //list.Add("andrea.com");
            //list.Add("iberopuebla.mx");
            //Obtaining URLs from csv file
            String inputFile = "Ranking.csv";
            System.Console.WriteLine("Leyendo archivo " + inputFile +"\n");
            List<string> list = GetWebsitesFromCSV(inputFile);
            System.Console.WriteLine("Se obtuvieron " + list.Count + " url válidos \n");
            //if the list is not empty we get their info
            if (list.Count >0)
            {
                ParallelOptions ops = new ParallelOptions();
                ops.MaxDegreeOfParallelism = -1;

                if (Parallel.ForEach(list, ops, (url) => {
                    System.Console.WriteLine("inicio: " + url);
                    Execute(url);
                }).IsCompleted)
                {
                    System.Console.WriteLine("terminé!");
                }
                int count = 1;
                foreach (var item in Lista)
                {

                    System.Console.WriteLine("\nElemento " + count + " :\n");
                    System.Console.WriteLine(item);
                    count++;

                }
                System.Console.WriteLine(count + " websites fueron escaneados");
                //Here we print the values found on a CSV
                GenerateCSVFileFromResults(Lista);

            }
            Console.ReadKey();
        }
        private static void GenerateCSVFileFromResults(List<Website> list)
        {
            StreamWriter sw = new StreamWriter(@"resultados.csv");
            var csv = new CsvWriter(sw);
            csv.WriteField("URL");
            csv.WriteField("Detected");
            csv.WriteField("Detected CMS Name");
            csv.WriteField("Detected CMS Version");
            csv.WriteField("Detected Language Name");
            csv.WriteField("Detected Language Version");
            csv.WriteField("Detected JS Lib Name");
            csv.WriteField("Detected JS Lib Version");
            csv.NextRecord();
            foreach (var item in list)
            {
                csv.WriteField(item.Url);
                if(item.Cms!= null)
                {

                }
                else
                {
                    csv.WriteField("");
                    csv.WriteField("");
                }

                csv.NextRecord();
            }
        }
        private static List<string> GetWebsitesFromCSV(String file)
        {
            List<string> result = new List<string>();
            if (!String.IsNullOrEmpty(file))
            {
                try
                {
                    StreamReader sr = new StreamReader(@file);
                    var csv = new CsvReader(sr);
                    while (csv.Read())
                    {
                        //En esta línea es donde se define la columna de donde se toman las urls
                        string stringField = csv.GetField<string>(2);
                        //TODO: falra la validaciónde stringField
                        if (!String.IsNullOrEmpty(stringField))
                        {
                            result.Add(stringField);
                            Console.WriteLine(stringField + "\n");
                        }
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine("No se encontró el archivo: " + file +" y la excepción dice: \n");
                    Console.WriteLine(e);
                }


            }else
            {
                Console.WriteLine("ERRROR: El nombre del archivo es vacío\n");
            }
            return result;
        }
        private static void Execute(string url)
        {

            if (String.IsNullOrEmpty(url))
            {
                return;
            }
            Website result = new Website();
            result.Url = url;
            result.wasFound = false;
            bool getLanguage, getCms, getJsLibraries;
            getLanguage = getCms = getJsLibraries = false;
            //HtmlDocument doc = new HtmlWeb().Load("http://w3techs.com/sites/info/" + url);
            HtmlDocument doc = new HtmlWeb().Load("http://guess.scritch.org/%2Bguess/?url=skyalert.mx");
            HtmlNode main = doc.DocumentNode.SelectSingleNode("//*[@class='tech_main']");
            if (main != null)
            {
                result.wasFound = true;
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
            }
            else
            {
                Console.WriteLine("El sitio "+ url + " no fue encontado en el scanner (probablemente no ha sido indexado), intente después\n");
            }

            Lista.Add(result);
        } 
    }
}
