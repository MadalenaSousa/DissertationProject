using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System;
using LumenWorks.Framework.IO.Csv;
using UnityEngine.UI;
using System.Data;
using Mono.Data.Sqlite;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;

public class PaperData : MonoBehaviour
{
    // DB variables
    private IDbConnection _connection;
    private const bool FILL_TABLES = true;

    // data storing variables
    public Dictionary<int, string> origin, account_origin, account, uses_category, uses_account, practices_category, practices_account, strategies_category, strategies_account;
      
    //public RelationTable account_origin, uses_account, practices_account, strategies_account;
    //public DoubleColNameTable origin, uses_category, practices_category, strategies_category;
    //public AccountTable account;
    
    
    // instance
    public static PaperData instance;

    public JObject parscitdata;
    public JArray parscitpapers;
    public TextAsset pctext;
    public string[] titles;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        // DB STUFF
        StartDataSQLite();

        FillTables();

        // API STUFF
        parscitdata = JObject.Parse(pctext.text);
        parscitpapers = (JArray)parscitdata["citationList"];
        titles = new string[parscitpapers.Count];

        //for (int i = 0; i < parscitpapers.Count; i++)
        //{
        //    titles[i] = (string)parscitpapers[i]["title"];
        //
        //    string encoded = titles[i].Replace(",", "%2C");
        //    encoded = encoded.Replace(".", "%2E");
        //    encoded = encoded.Replace(" ", "%20");
        //    encoded = encoded.Replace(":", "%3A");
        //    encoded = encoded.Replace(";", "%3B");
        //
        //    StartCoroutine(GetRequest("https://api.openalex.org/works?mailto=up202003391@up.pt&filter=title.search:" + encoded));
        //
        //    Thread.Sleep(200);
        //}
    }

    /* ----------- DATABASE STUFF ----------- */

    void ReadCsv(string filename, Dictionary<int, string> dict)
    {
        // open the file "Assets/Resources/origin.csv" which is a CSV file with headers
        using (CachedCsvReader csv = new CachedCsvReader(new StreamReader("Assets/Resources/" + filename), true, ';'))
        {
            while (csv.ReadNextRecord())
            {
                string column1 = csv[0];
                string column2 = csv[1];
                dict.Add(int.Parse(column1), column2);
            }
        }
    }

    void StartDataSQLite()
    {
        string urlDataBase = "URI = file:Assets/Resources/database.db";

        // create Connection to the Database
        this._connection = new SqliteConnection(urlDataBase);

        _connection.Open();
    }

    void FillTables()
    {
        if (FILL_TABLES == false)
        {
            return;
        }

        // ORIGIN + ACCOUNT
        Dictionary<int, string> origin = new Dictionary<int, string>();
        ReadCsv("origin.csv", origin);
        
        foreach (KeyValuePair<int, string> pair in origin)
        {
            insertDataSQLite("paper", "id, title, paperyear, publishin", String.Format("{0}, '{1}', {2}, '{3}'", pair.Key, pair.Value, 0000, "placeholder"));
        }

        //Dictionary<int, string> account_origin = new Dictionary<int, string>();
        //ReadCsv("account_origin.csv", account_origin);
        //
        //foreach (KeyValuePair<int, string> pair in account_origin)
        //{
        //    insertDataSQLite("paper_account", "paper_id, account_id", String.Format("{0}, {1}", int.Parse(pair.Value), pair.Key));
        //}
        //
        //Dictionary<int, string> account = new Dictionary<int, string>();
        //ReadCsv("account.csv", account);
        //
        //// ??????????? DIFERENT ?????????
        //
        //// USES
        //Dictionary<int, string> uses_category = new Dictionary<int, string>();
        //ReadCsv("uses_category.csv", uses_category);
        //
        //foreach (KeyValuePair<int, string> pair in uses_category)
        //{
        //    insertDataSQLite("uses", "id, name", String.Format("{0}, '{1}'", pair.Key, pair.Value));
        //}
        //
        //Dictionary<int, string> uses_account = new Dictionary<int, string>();
        //ReadCsv("uses_account.csv", uses_account);
        //
        //foreach (KeyValuePair<int, string> pair in uses_account)
        //{
        //    insertDataSQLite("uses_account", "uses_id, account_id", String.Format("{0}, {1}", int.Parse(pair.Value), pair.Key));
        //}
        //
        //// PRACTICES
        //Dictionary<int, string> practices_category = new Dictionary<int, string>();
        //ReadCsv("practices_category.csv", practices_category);
        //
        //foreach (KeyValuePair<int, string> pair in practices_category)
        //{
        //    insertDataSQLite("practices", "id, name", String.Format("{0}, '{1}'", pair.Key, pair.Value));
        //}
        //
        //Dictionary<int, string> practices_account = new Dictionary<int, string>();
        //ReadCsv("practices_account.csv", practices_account);
        //
        //foreach (KeyValuePair<int, string> pair in practices_account)
        //{
        //    insertDataSQLite("practices_account", "practices_id, account_id", String.Format("{0}, {1}", int.Parse(pair.Value), pair.Key));
        //}
        //
        //// STRATEGIES
        //Dictionary<int, string> strategies_category = new Dictionary<int, string>();
        //ReadCsv("strategies_category.csv", strategies_category);
        //
        //foreach (KeyValuePair<int, string> pair in strategies_category)
        //{
        //    insertDataSQLite("strategies", "id, name", String.Format("{0}, '{1}'", pair.Key, pair.Value));
        //}
        //
        //Dictionary<int, string> strategies_account = new Dictionary<int, string>();
        //ReadCsv("strategies_account.csv", strategies_account);
        //
        //foreach (KeyValuePair<int, string> pair in strategies_account)
        //{
        //    insertDataSQLite("strategies_account", "strategies_id, account_id", String.Format("{0}, {1}", int.Parse(pair.Value), pair.Key));
        //}
    }

    void readDataSQLiteExample()
    {

        IDbCommand _command = _connection.CreateCommand();

        string sqlQuery = "SELECT id, text, survey_id FROM account";

        _command.CommandText = sqlQuery;

        IDataReader reader = _command.ExecuteReader();

        while (reader.Read())

        {
            int value = reader.GetInt32(0);

            string name = reader.GetString(1);

            int rand = reader.GetInt32(2);

            Debug.Log("id = " + value + " text =" + name + " survey_id =" + rand);
        }
    }

    void insertDataSQLite(string insertTable, string insertFields, string insertValues)
    {

        IDbCommand _command = _connection.CreateCommand();

        string sql = String.Format("INSERT INTO {0}({1}) VALUES({2})", insertTable, insertFields, insertValues);

        _command.CommandText = sql;

        _command.ExecuteNonQuery();
    }

    // data storing classes
    public class RelationTable { int[] col1, col2; }
    public class DoubleColNameTable { int[] col1; string[] col2; }
    public class AccountTable { int[] col1, col3; string[] col2; }


    /* ----------- API STUFF ----------- */
    IEnumerator GetRequest(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    //Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text)

                    JObject json = JObject.Parse(webRequest.downloadHandler.text);
                    JArray authorships = (JArray)json["results"][0]["authorships"]; //need to add an IF EXISTS and IS NOT FORBIDDEN

                    string[] authors = new string[authorships.Count];
                    string[] affiliations = new string[authorships.Count];

                    for (int i = 0; i < authors.Length; i++)
                    {
                        authors[i] = (string)authorships[i]["author"]["display_name"];
                        affiliations[i] = (string)authorships[i]["raw_affiliation_string"];
                    }

                    InitData paper = new InitData(authors, affiliations);
                    paper.title = (string)json["results"][0]["title"];
                    paper.date = (string)json["results"][0]["publication_date"];
                    paper.journal.name = (string)json["results"][0]["host_venue"]["display_name"];
                    paper.journal.publisher = (string)json["results"][0]["host_venue"]["publisher"];
                    paper.journal.url = (string)json["results"][0]["host_venue"]["url"];

                    JObject jsonpaper = JObject.FromObject(paper);
                    //Debug.Log(jsonpaper);

                    StreamWriter writer = new StreamWriter("Assets/Data/articledata.json");
                    writer.Write(jsonpaper);
                    writer.Close();

                    break;
            }
        }
    }

    public class InitData
    {
        public string title;
        public string date;
        public HostVenue journal = new HostVenue();
        public List<AuthorInstitution> authorinst = new List<AuthorInstitution>();

        public InitData(string[] authorname, string[] rawaffiliation)
        {
            for (int i = 0; i < authorname.Length; i++)
            {
                authorinst.Add(new AuthorInstitution(authorname[i], rawaffiliation[i]));
            }
        }

        public class HostVenue
        {
            public string name;
            public string publisher;
            public string url;
        }

        public class AuthorInstitution
        {
            public string authorname;
            //public string institutionname;
            //public string countrycode;
            public string rawaffiliation;

            public AuthorInstitution(string authorname, string rawaffiliation)
            {
                this.authorname = authorname;
                this.rawaffiliation = rawaffiliation;
            }
        }
    }
}
