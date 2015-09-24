using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



using Windows.Data.Json;


namespace SysInfo
{
    /// <summary>
    /// JSON returns Name-Value pairs
    /// When show results of queries, create a NameValue obkect for each JSON name-value object
    /// The List binds to the Name and Vlaue property of NameValue object list (NameValue.NameValues).
    /// </summary>
    public class NameValue
    {
        //Properties
        public string Name { get; set;}
        public string Value { get; set; }

        /// <summary>
        /// Standard constructor
        /// </summary>
        /// <param name="name">JSON name of properfty</param>
        /// <param name="value">JSON value of property</param>
        public NameValue(string name, string value)
        {
            Name = name;
            Value = value;
            //Objects are automatically addded to list.
            NameValues.Add(this);
        }

        /// <summary>
        /// Create obvject from JSON obkect
        /// </summary>
        /// <param name="oJsn">The JSON obkect</param>
        public NameValue(JsonObject oJsn)
        {
            string name;
            name = oJsn.Keys.ToArray<string>()[0];
            IJsonValue val;
            bool res = oJsn.TryGetValue(name, out val);
            Name = name;
            String Type = oJsn.GetNamedString("Type");
            Value = val.ToString();

            NameValues.Add(this);
        }

        /// <summary>
        /// Create object from JSON KeyValuePair
        /// This is what is used below
        /// </summary>
        /// <param name="preName">Higher level info about this branch</param>
        /// <param name="index">If an array item apply index to prepend of name</param>
        /// <param name="oJsn">The </param>
        public NameValue(string preName, int index, KeyValuePair<string, IJsonValue> oJson_KVP)
        {
            Name = oJson_KVP.Key;
            //Display a property so indicate with ,
            if (preName != "")
                Name = preName +"."  + oJson_KVP.Key;
            IJsonValue val = oJson_KVP.Value;
            Value = val.ToString();
            //For string values remove leading and trailing "
            //ToDo: Could make this an option
            Value = Value.Replace("\"", "");
            NameValues.Add(this);
        }

        /// <summary>
        /// This is the "Powerhouse"
        /// Get all name-value pairs in the object
        /// - For each value get its value if simple type
        /// - If value is an array get objects in array an recursively call this for each item in the array
        /// - If value is an object call this recursiveky.
        /// </summary>
        /// <param name="preName">Text to prepend to name in list</param>
        /// <param name="oJson">The JSON object</param>
        public static void GetNameValues(string preName, JsonObject oJson)
        {
            foreach (KeyValuePair<string, IJsonValue> oJson_KVP in oJson)
            {
                string prefix = oJson_KVP.Key;
                if (preName != "")
                    //Drilling into an object to indicate using ->
                    prefix = preName + "->" + oJson_KVP.Key;

                //A bit of house keeping: List names are in plural.
               // int n = "address".Length;
               // if (prefix.Substring(prefix.Length - n, n).ToLower() != "address")
               // {
               //     if (prefix.Substring(prefix.Length - 2, 2) == "es")
               //         prefix = prefix.Substring(0, prefix.Length - 2);
               //     else if (prefix.Substring(prefix.Length - 1, 1) == "s")
               //         prefix = prefix.Substring(0, prefix.Length - 1);
               //}

                string typ = oJson_KVP.Value.ValueType.ToString().ToLower();

                if (typ == "object")
                {
                    //Recursively call this for the value
                    GetNameValues(prefix , oJson.GetNamedObject(oJson_KVP.Key));
                }
                else if (typ == "array")
                {            
                    JsonArray ja =  oJson.GetNamedArray(oJson_KVP.Key);
                    //foreach (JsonObject jajo in ja.)
                    for(uint index = 0; index < ja.Count; index++)
                    {
                        string prfx = prefix;
                        if (ja.Count > 1)
                        {
                            //If more than one item in the array, prepend the array index of the item  to the name,
                            NameValues_IsFrom_Array = true;
                            prfx = index.ToString() + ". " + prfx;
                        }
                        JsonObject joja = ja.GetObjectAt(index).GetObject();
                        //Recursively call this for the values in teh array
                        GetNameValues(prfx, joja);
                    }
                }
                else
                {
                    //Got a value to add to list
                    NameValue nv = new NameValue(preName,-1, oJson_KVP);
                }

            }
        }

        public static void ClearList()
        {
            NameValues = new List<NameValue>();
            NameValues_IsFrom_Array = false;
        }

        /// <summary>
        /// This is the list that is bound to the listbox
        /// </summary>
        public static List<NameValue> NameValues = new List<NameValue>();

        //Where the items are an array, collapse to one entry per item but flag that so it can be interogated.
        public static bool NameValues_IsFrom_Array { get; set; } = false;
        //Stack used top preserve list that are collapsed. Used when "drilling into" an item's properties.
        public static Stack<List<NameValue>> NameValuesStack = new Stack<List<NameValue>>();
    }

    /// <summary>
    /// Performs queries to Win 10 IoT device's web interface
    /// Uses REST Interface
    /// Makes use of NameValue class to do treturned JSON processing
    /// </summary>
    public static class SysInfo {

        //Device credentials
        public static string Device { get; set; } = "localhost";
        public static string Port { get; set; } = "8080";
        public static string Admin { get; set; } = "Administrator";
        public static string Pwd { get; set; } = "p@ssw0rd";
        public static string RelAppId { get; set; } = "";
        public static string FullAppName { get; set; } = "";
        public static bool ForceStop { get; set; } = false;
        /*
                //Query strings
                public static string IpConfigURL { get; set; } = "api/networking/ipconfig";
                public static string SysInfoURLRL { get; set; } = "api/iot/deviceinformation";
                public static string APIURL { get; set; } = "api/networking/ipconfig";
                public static string OSAPILRL { get; set; } = "api/os/info";
                public static string DevicesURL { get; set; } = "api/devicemanager/devices";
                public static string ProcessesURL { get; set; } = "api/resourcemanager/processes";
                public static string DefaulAppURL { get; set; } = "api/iot/appx/getdefault";
                public static string ProvidersURL { get; set; } = "/api/etw/providers";
                public static string PackagesURL { get; set; } = "/api/appx/installed";
                /* [6] For if a new query is added
                public static string NewqueryURL { get; set; } = "/api/XXX/YYYYY";
                */

        public static string APIURL { get; set; } = "api/networking/ipconfig";
        public static string API_Params { get; set; } = "";



        public async static Task<bool>  DoQuery(Commands cmd)
        {
            
            NameValue.ClearList();



            StreamReader SR = null;
            HttpStatusCode response;
            //The REST call for the command
            if (cmd.url[cmd.url.Length-1]=='*')
            {
                //POST
                response = await PostRequest(cmd);
                NameValue nv = new NameValue("Result:",response.ToString());
            }
            else
            {
                //GET
                SR = await GetJsonStreamData(cmd);

                if (SR == null)
                {
                    return false;
                }

                //Process the JSON stream data
                JsonObject ResultData = null;
                try
                {
                    String JSONData;
                    //Get the stream as text.
                    JSONData = SR.ReadToEnd();

                    //Convert to JSON object
                    ResultData = (JsonObject)JsonObject.Parse(JSONData);

                    //Process the JSON data
                    NameValue.GetNameValues("", ResultData);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    return false;
                }
            }
            return true;
        }

        public static CancellationTokenSource cts { get; set; } = new CancellationTokenSource();
        public static int Timeout { get; set; } = 10000;

        /// <summary>
        /// The command REST call to the Win 10 IoT device
        /// </summary>
        /// <param name="URL">The REST URL less the host information</param>
        /// <returns>The REST stream. Returns null if any errors etc.</returns>
        private static async Task<StreamReader> GetJsonStreamData(Commands cmd)
        {
            String URL = cmd.url;
            HttpWebRequest wrGETURL = null;
            Stream objStream = null;
            StreamReader objReader = null; //If any errors etc this is returned as null.
            string url = "http://" + Device + ":" + Port +"/"+ URL;

            //Can add parameters:
            //if (cmd.url[cmd.url.Length-1] == '?') 
            //{
            //    System.Diagnostics.Debug.WriteLine(API_Params);

            //     API_Params = API_Params.Trim();

            //    byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(API_Params);
            //    string appName64 = System.Convert.ToBase64String(toEncodeAsBytes);
            //    url += appName64;
            //    return await PostJsonStreamData(url);
            //}

            try
            {
                //Set up REST call
                wrGETURL = (HttpWebRequest)WebRequest.Create(url);
                wrGETURL.Credentials = new NetworkCredential(Admin,Pwd);

                //Use ability to cancel/timeout/capture error
                HttpWebResponse Response = null; // = (HttpWebResponse)(await wrGETURL.GetResponseAsync());
                bool completed = false;
                cts = new CancellationTokenSource();
                try
                {
                    cts.CancelAfter(Timeout);
                    //Make the REST call:
                    Response = (HttpWebResponse)(await wrGETURL.GetResponseAsync());
                    completed = true;
                }
                catch (TaskCanceledException ex)
                {
                    completed = false;
                }
                catch (Exception exception)
                {
                    completed = false;
                }

                if ( (!cts.IsCancellationRequested) && (completed))
                {
                    if (Response.StatusCode == HttpStatusCode.OK)
                    {
                        //If OK then get reader to return.
                        objStream = Response.GetResponseStream();
                        objReader = new StreamReader(objStream);
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("GetData " + e.Message);
            }
            return objReader;
        }
        private static async Task<HttpStatusCode> PostRequest(Commands cmd)
        {
            
            string queryString = API_Params;
            if (cmd.name == "startapp")
                queryString = RelAppId;
            else if (cmd.name == "stopapp")
                queryString = FullAppName;
            else if (cmd.name == "shutdown")
                queryString = "";
            else if (cmd.name == "reboot")
                queryString = "";

 

            //Post is used if url has * on end so remove it. 
            string url = cmd.url;
            url = url.Substring( 0, url.Length-1 );

            System.Diagnostics.Debug.WriteLine(queryString);
            byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(queryString);
            string appName64 = System.Convert.ToBase64String(toEncodeAsBytes);
            
            
            if (cmd.name == "stopapp")
            {
                url = "http://" + Device + ":" + Port + "/" + url + (ForceStop ? "?forcestop=true&" : "") + "package=" + appName64;
            }
            else
                url = "http://" + Device + ":" + Port + "/" + url + appName64;


            System.Diagnostics.Debug.WriteLine(url);


            HttpStatusCode response = await PostJsonStreamData(url);

            return response;
        }

        private static async Task<HttpStatusCode> PostJsonStreamData(String URL)
        {
            HttpWebRequest wrGETURL = null;
            Stream objStream = null;
            StreamReader objReader = null;

            System.Diagnostics.Debug.WriteLine(URL);
            HttpStatusCode PostResponse = HttpStatusCode.BadRequest;

            try
            {
                wrGETURL = (HttpWebRequest)WebRequest.Create(URL);
                wrGETURL.Method = "POST";
                wrGETURL.Credentials = new NetworkCredential("Administrator", "p@ssw0rd");
               
                HttpWebResponse Response = (HttpWebResponse)(await wrGETURL.GetResponseAsync());
                PostResponse = Response.StatusCode;
                if (Response.StatusCode == HttpStatusCode.OK)
                {
                    objStream = Response.GetResponseStream();
                    objReader = new StreamReader(objStream);
                    string strn = objReader.ReadToEnd();             
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("GetData " + e.Message);
               
            }
            return PostResponse;
        }


    }
}
