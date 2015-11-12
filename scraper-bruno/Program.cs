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
        public static bool LimitReached { get; set; }
        public static int Contador { get; set; }
        static List<Website> Lista = new List<Website>();
        static void Main(string[] args)
        {
            //Obtenemos el listado de sitios web del csv y lo hacemos una List
            String inputFile = "input.csv";
            Contador = 0;
            System.Console.WriteLine("Leyendo archivo " + inputFile + "\n");
            //falta validar que exista el archivo
            try
            {
                StreamReader textReader = new StreamReader(@inputFile);
                var csv = new CsvReader(textReader);

                List<Website> inputList = csv.GetRecords<Website>().ToList();
                textReader.Close();
                // *sigh* no procesa whitespaces
                System.Console.WriteLine("Se obtuvieron " + inputList.Count + " url válidos \n");
                if (inputList.Count > 0)
                {
                    //Alteramos o generamos un nuevo listado con la info recabada
                    List<Website> updatedList = new List<Website>();
                    LimitReached = false;
                    StreamWriter textWriter = new StreamWriter(@"output.csv");
                    var csvW = new CsvWriter(textWriter);
                    foreach (Website item in inputList)
                    {
                        //hay que checar limitreached, si ya se tiene y si no es un URL válido
                        if (item.Url.IndexOf(" ") > -1)
                        { //Si no es URL válido lo quitamos de la lista

                            Console.WriteLine("URL inválido, se quita de la lista");

                        }
                        else
                        { //Si es URL válido hay que agregarlo

                            Website aux = new Website();

                            if (item.WasDetected)
                            {//Si ya se tiene se pasa como viene

                                aux = item;
                            }
                            else
                            {//Si no se tiene hay que conseguirlo
                                if (LimitReached)
                                {//si se alcanzó el límite hay que copiar y pegar sin nada más
                                    aux = item;
                                    aux.WasDetected = false;
                                }
                                else
                                {//si no se alcanzó el límite hay que obetener y verificar si se alcanzó el límite
                                    aux = obtainWebsiteInfo(item.Url);
                                }
                            }
                            updatedList.Add(aux);
                            csvW.WriteRecord(aux);


                        }

                    }

                    Console.WriteLine("Se obtuvieron " + Contador + " nuevos registros\n");
                    //Creamos un archivo CSV con la última información

                    textWriter.Close();
                }
            }
            catch (Exception e)
            {
                
                Console.WriteLine("Error al abrir el archivo: "+ inputFile);
                Console.WriteLine("La excepción dice: " + e);
                Console.ReadKey();
                throw;
            }
            if (LimitReached)
            {
                Console.WriteLine("\n---------------------------------------------");
                Console.WriteLine("Límite de llamadas por IP por día alcanzado.");
                Console.WriteLine("Conectate a otra red o cambia tu ip pública para volver a intentarlo");
                Console.WriteLine("No olvides renombrar el archivo 'output.csv' a 'input.csv' para guardar los éxitos");


            }

            Console.ReadKey();
        }

        private static Website obtainWebsiteInfo(string url)
        {
            Website result = new Website();
            result.Url = url;
            result.WasDetected = false;
            if (!String.IsNullOrEmpty(url))
            {
                bool getLanguage, getCms, getJsLibraries;
                getLanguage = getCms = getJsLibraries = false;
                try
                {

                    Console.WriteLine("\n"+ url + "  -- Obteniendo información ...");
                HtmlDocument doc = new HtmlWeb().Load("http://w3techs.com/sites/info/" + url);
                 
                //HtmlDocument doc = new HtmlWeb().Load("http://guess.scritch.org/%2Bguess/?url=skyalert.mx");
                HtmlNode main = doc.DocumentNode.SelectSingleNode("//*[@class='tech_main']");
                    String mainText = main.InnerHtml;
                    if(mainText.Contains("This website is redirected to"))
                    {
                        HtmlNode URLsugerida = main.SelectSingleNode("//*[@class='no_und']");
                        Console.WriteLine("La url es incorrecta se está actualizando a "+ URLsugerida.InnerText);
                        Console.WriteLine("Es necesario volver a correr el programa para volver a intentar obtener la info de este sitio.");
                        result.Url = URLsugerida.InnerText;


                    }else if (mainText.Contains("Site under maintenance"))
                    {
                        LimitReached = true;
                        Console.WriteLine("Llegaste a la cantidad de requests permitidos. ");
                    }
                    else
                    {
                        if (main != null)
                        {
                            Console.WriteLine("Info obtenida.");
                            result.WasDetected = true;
                            Contador++;

                            HtmlNode site = main.SelectSingleNode(".//h1");
                            foreach (HtmlNode node in main.ChildNodes)
                            {
                                if (getLanguage)
                                {
                                    if (node.OuterHtml.Contains("si_tech") && !node.OuterHtml.Contains("si_tech_np"))
                                    {
                                        HtmlNode langName = node.NextSibling;
                                        HtmlNode langVersion = langName.NextSibling;
                                        if (!String.IsNullOrEmpty(langName.InnerText))
                                        {
                                            result.LanguageName = langName.InnerText.Trim();
                                            result.LanguageVersion = (String.IsNullOrEmpty(langVersion.InnerText)) ? "N/A" : langVersion.InnerText.Trim();
                                            Console.WriteLine("Lenguaje detectado: " + result.LanguageName + " v: " + result.LanguageVersion);
                                        }
                                        getLanguage = false;
                                    }

                                }
                                else if (getCms)
                                {
                                    if (node.OuterHtml.Contains("si_tech") && !node.OuterHtml.Contains("si_tech_np"))
                                    {
                                        HtmlNode cmsName = node.NextSibling;
                                        HtmlNode cmsVersion = cmsName.NextSibling;
                                        if (!String.IsNullOrEmpty(cmsName.InnerText))
                                        {
                                            result.CMSName = cmsName.InnerText.Trim();
                                            result.CMSVersion = (String.IsNullOrEmpty(cmsVersion.InnerText)) ? "N/A" : cmsVersion.InnerText.Trim();
                                            Console.WriteLine("CMS detectado: " + result.CMSName + " v: " + result.CMSVersion);

                                        }
                                        getCms = false;
                                    }

                                }
                                else if (getJsLibraries)
                                {
                                    if (node.OuterHtml.Contains("si_tech") && !node.OuterHtml.Contains("si_tech_np"))
                                    {
                                        HtmlNode libraryName = node.NextSibling;
                                        HtmlNode libraryVersion = libraryName.NextSibling;
                                        if (!String.IsNullOrEmpty(libraryName.InnerText))
                                        {
                                            result.JSLibName = libraryName.InnerText.Trim();
                                            result.JSLibVersion = (String.IsNullOrEmpty(libraryVersion.InnerText)) ? "N/A" : libraryVersion.InnerText.Trim();
                                            Console.WriteLine("JS Lib detectado: " + result.JSLibName + " v: " + result.JSLibVersion);

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
                            Console.WriteLine("Scan del sitio terminado");
                        }
                        else
                        {//Falta checar si ya dice under-maintenance
                            Console.WriteLine("El sitio " + url + " no fue encontado en el scanner (probablemente no ha sido indexado), intente después\n");
                            Console.WriteLine("O probablemente ya se acabaron los requests");
                        }
                    }

                
                }
                catch (Exception e)
                {
                    result = new Website();
                    result.Url = url;
                    result.WasDetected = false;
                    Console.WriteLine("Hubo un error al sacar la información del Url: " + url);
                    Console.WriteLine("La excepción dice: \n"+ e);
                    throw;
                }
            }

            return result;

        }

      
       
    }
}
