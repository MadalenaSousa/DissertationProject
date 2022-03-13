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
    private const bool FILL_TABLES = false;
       
    // instance
    public static PaperData instance;

    public JObject parscitdata;
    public JArray parscitpapers;
    public TextAsset pctext;
    public string[] titles;


    public List<InitData> fullpapers;

    StreamWriter writer;

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
        Debug.Log("ParsCit JSON Article Count: " + parscitpapers.Count);

        //writer = new StreamWriter("Assets/Data/articledata.json", true);
        //for (int i = 1; i < parscitpapers.Count; i++)
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
        //writer.Close();

        fullpapers = new List<InitData>();

        for(int i = 0; i < parscitpapers.Count; i++)
        {
            InitData temp = new InitData();
            if(parscitpapers[i]["date"] != null)
            {
                temp.date = (string)parscitpapers[i]["date"];
            }
            
            temp.title = (string)parscitpapers[i]["title"];
            temp.practice_id = getPracticeByOrigin(i);

            fullpapers.Add(temp);
        }
    }

    /* ----------- DATABASE STUFF ----------- */

    void ReadCsv(string filename, Dictionary<int, string> dict)
    {
        // open the file "Assets/Resources/origin.csv" which is a CSV file with headers
        using (CachedCsvReader csv = new CachedCsvReader(new StreamReader("Assets/Resources/" + filename), true, ';'))
        {
            while (csv.ReadNextRecord())
            {               
                int column1 = int.Parse(csv[0]);
                string column2 = csv[1];
                dict.Add(column1, column2);
            }
        }
    }

    void ReadCsvRelationTable(string filename, List<RelationTable> dict)
    {
        // open the file "Assets/Resources/origin.csv" which is a CSV file with headers
        using (CachedCsvReader csv = new CachedCsvReader(new StreamReader("Assets/Resources/" + filename), true, ';'))
        {
            while (csv.ReadNextRecord())
            {
                int column1 = int.Parse(csv[0]);
                int column2 = int.Parse(csv[1]);
                
                RelationTable temp = new RelationTable();
                temp.col1 = column1;
                temp.col2 = column2;

                dict.Add(temp);

                //dict.Add(column1, column2);
            }
        }
    }

    void ReadCsvAccount(string filename, List<AccountTable> dict)
    {
        // open the file "Assets/Resources/origin.csv" which is a CSV file with headers
        using (CachedCsvReader csv = new CachedCsvReader(new StreamReader("Assets/Resources/" + filename), true, ';'))
        {
            while (csv.ReadNextRecord())
            {
                int column1 = int.Parse(csv[0]);
                int column2 = int.Parse(csv[1]);
                string column3 = csv[2];

                AccountTable temp = new AccountTable();
                temp.col1 = column1;
                temp.col2 = column2;
                temp.col3 = column3;

                dict.Add(temp);

                //dict.Add(column1, column2);
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
            string encoded = pair.Value.Replace(",", "%2C");
            encoded = encoded.Replace(".", "%2E");
            encoded = encoded.Replace(" ", "%20");
            encoded = encoded.Replace(":", "%3A");
            encoded = encoded.Replace(";", "%3B");
            encoded = encoded.Replace("'", "%27");
            insertDataSQLite("paper", "id, title, paperyear, publishin", String.Format("{0}, '{1}', {2}, '{3}'", pair.Key, encoded, 0000, "placeholder"));
        }

        List<RelationTable> origin_account = new List<RelationTable>();
        ReadCsvRelationTable("account_origin.csv", origin_account);

        for(int i = 0; i < origin_account.Count; i++)
        {
            insertDataSQLite("paper_account", "paper_id, account_id", String.Format("{0}, {1}", origin_account[i].col1, origin_account[i].col2));
        }

        List<AccountTable> account = new List<AccountTable>();
        ReadCsvAccount("account.csv", account);

        for (int i = 0; i < account.Count; i++)
        {
            string encoded = account[i].col3.Replace(",", "%2C");
            encoded = encoded.Replace(".", "%2E");
            encoded = encoded.Replace(" ", "%20");
            encoded = encoded.Replace(":", "%3A");
            encoded = encoded.Replace(";", "%3B");
            encoded = encoded.Replace("'", "%27");
            insertDataSQLite("account", "survey_id, id, text", String.Format("{0}, {1}, '{2}'", account[i].col1, account[i].col2, encoded));
        }

        // USES
        Dictionary<int, string> uses_category = new Dictionary<int, string>();
        ReadCsv("uses_category.csv", uses_category);
        
        foreach (KeyValuePair<int, string> pair in uses_category)
        {
            insertDataSQLite("uses", "id, name", String.Format("{0}, '{1}'", pair.Key, pair.Value));
        }

        List<RelationTable> uses_account = new List<RelationTable>();
        ReadCsvRelationTable("uses_account.csv", uses_account);

        for (int i = 0; i < uses_account.Count; i++)
        {
            insertDataSQLite("uses_account", "uses_id, account_id", String.Format("{0}, {1}", uses_account[i].col1, uses_account[i].col2));
        }
        
        // PRACTICES
        Dictionary<int, string> practices_category = new Dictionary<int, string>();
        ReadCsv("practices_category.csv", practices_category);
        
        foreach (KeyValuePair<int, string> pair in practices_category)
        {
            insertDataSQLite("practices", "id, name", String.Format("{0}, '{1}'", pair.Key, pair.Value));
        }

        List<RelationTable> practices_account = new List<RelationTable>();
        ReadCsvRelationTable("practices_account.csv", practices_account);

        for (int i = 0; i < practices_account.Count; i++)
        {
            insertDataSQLite("practices_account", "practices_id, account_id", String.Format("{0}, {1}", practices_account[i].col1, practices_account[i].col2));
        }
        
        // STRATEGIES
        Dictionary<int, string> strategies_category = new Dictionary<int, string>();
        ReadCsv("strategies_category.csv", strategies_category);
        
        foreach (KeyValuePair<int, string> pair in strategies_category)
        {
            insertDataSQLite("strategies", "id, name", String.Format("{0}, '{1}'", pair.Key, pair.Value));
        }

        List<RelationTable> strategies_account = new List<RelationTable>();
        ReadCsvRelationTable("strategies_account.csv", strategies_account);

        for (int i = 0; i < strategies_account.Count; i++)
        {
            insertDataSQLite("account_strategies", "strategies_id, account_id", String.Format("{0}, {1}", strategies_account[i].col1, strategies_account[i].col2));
        }
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

    int getAccountByOrigin(int id)
    {
        IDbCommand _command = _connection.CreateCommand();

        string sqlQuery = "SELECT account_id FROM paper_account WHERE paper_id=" + id;

        _command.CommandText = sqlQuery;

        IDataReader reader = _command.ExecuteReader();

        int accountid = 0;
        while (reader.Read())
        {
            accountid = reader.GetInt32(0);
        }

        return accountid;
    }

    int getPracticeByOrigin(int id)
    {
        int accountid = getAccountByOrigin(id);

        IDbCommand _command = _connection.CreateCommand();

        string sqlQuery = "SELECT practices_id FROM practices_account WHERE account_id=" + accountid;

        _command.CommandText = sqlQuery;

        IDataReader reader = _command.ExecuteReader();

        int practiceid = 0;
        while (reader.Read())
        {
            practiceid = reader.GetInt32(0);
        }

        return practiceid;
    }

    void insertDataSQLite(string insertTable, string insertFields, string insertValues)
    {

        IDbCommand _command = _connection.CreateCommand();

        string sql = String.Format("INSERT INTO {0}({1}) VALUES({2})", insertTable, insertFields, insertValues);

        _command.CommandText = sql;

        _command.ExecuteNonQuery();
    }

    // data storing classes
    public class RelationTable { public int col1, col2; }
    public class DoubleColNameTable { public int col1; public string col2; }
    public class AccountTable { public int col1, col2; public string col3; }


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

                    Debug.Log("API Authorships Count Per Article: " + authorships.Count);

                    string[] authors = new string[authorships.Count];
                    string[] affiliations = new string[authorships.Count];

                    for (int i = 0; i < authors.Length; i++)
                    {
                        authors[i] = (string)authorships[i]["author"]["display_name"];
                        affiliations[i] = (string)authorships[i]["raw_affiliation_string"];
                    }
                    
                  
                    InitData paper = new InitData();
                    paper.title = (string)json["results"][0]["title"];
                    paper.date = (string)json["results"][0]["publication_date"];
                    paper.journal.name = (string)json["results"][0]["host_venue"]["display_name"];
                    paper.journal.publisher = (string)json["results"][0]["host_venue"]["publisher"];
                    paper.journal.url = (string)json["results"][0]["host_venue"]["url"];

                    fullpapers.Add(paper);

                    JObject jsonpaper = JObject.FromObject(paper);
                    //Debug.Log(jsonpaper);
                    
                    writer.Write(jsonpaper);                   

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
        public int id, use_id, practice_id, strategy_id;

        public InitData(/*string[] authorname, string[] rawaffiliation*/)
        {
            //for (int i = 0; i < authorname.Length; i++)
            //{
            //    authorinst.Add(new AuthorInstitution(authorname[i], rawaffiliation[i]));
            //}
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
