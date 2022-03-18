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
       
    // instance
    public static PaperData instance;

    public JObject parscitdata;
    public JArray parscitpapers;
    public TextAsset pctext;
    //public string[] titles;
    
    public TextAsset apitext;
    public JObject apidata;
    public JArray apipapers;

    public List<InitData> fullpapers;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        // API STUFF
        parscitdata = JObject.Parse(pctext.text);
        parscitpapers = (JArray)parscitdata["citationList"];

        apidata = JObject.Parse(apitext.text);
        apipapers = (JArray)apidata["objects"];
        
        Debug.Log("API JSON Article Count: " + apipapers.Count);

        fullpapers = new List<InitData>();

        //StartCoroutine(WriteData(parscitpapers));

        //for(int i = 0; i < parscitpapers.Count; i++)
        //{
        //    InitData temp = new InitData();
        //    if(parscitpapers[i]["date"] != null)
        //    {
        //        temp.date = (string)parscitpapers[i]["date"];
        //    }
        //    
        //    temp.title = (string)parscitpapers[i]["title"];
        //    temp.practice_id = getPracticeByOrigin(i);
        //
        //    fullpapers.Add(temp);
        //}

        // DB STUFF
        StartDataSQLite();

        FillTables();  
    }

    IEnumerator WriteData(JArray papers)
    {
        for (int i = 0; i < papers.Count; i++)
        {
            string[] titles = new string[papers.Count];
            titles[i] = (string)papers[i]["title"];

            string encoded = titles[i].Replace(",", "%2C");
            encoded = encoded.Replace(".", "%2E");
            encoded = encoded.Replace(" ", "%20");
            encoded = encoded.Replace(":", "%3A");
            encoded = encoded.Replace(";", "%3B");

            yield return StartCoroutine(GetRequest("https://api.openalex.org/works?mailto=up202003391@up.pt&filter=title.search:" + encoded));

            Thread.Sleep(200);
        }

        StreamWriter writer = new StreamWriter("Assets/Data/articledata.json");

        DataObject totaldata = new DataObject(fullpapers);

        Debug.Log(totaldata.objects[0].title);

        JObject jsonpaper = JObject.FromObject(totaldata);

        writer.Write(jsonpaper);
        writer.Close();
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
        string urlDataBase = "URI = file:Assets/Resources/dbupdated.db";

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
        
        // PAPERS
        for (int i = 0; i < apipapers.Count; i++)
        {
            var date = apipapers[i]["date"];
            string year = null;

            if (date != null && date.ToString().Length > 0)
            {
                year = date.ToString().Substring(0, 4);
            }
            
            int paperid = i;
            JArray authors = (JArray)apipapers[i]["authors"];
            
            insertDataSQLite("paper", "id, title, pubyear, date", String.Format("{0}, '{1}', {2}, '{3}'", paperid, apipapers[i]["title"], year == null ? 0 : Int32.Parse(year), date == null ? "" : date));

            // AUTHORS
            for(int j = 0; j < authors.Count; j++)
            {
                int authorid = getAuthorByOpenAlexId((string)authors[j]["openalexid"], (string)authors[j]["name"]);

                if(authorid == -1) // se não existir author já com id com o mesmo open alex id que o autor j nem com o mesmo nome
                {
                    if((string)authors[j]["openalexid"] == null) // se ele não tiver open alex id
                    {
                        insertDataSQLite("author", "name", String.Format("'{0}'", authors[j]["name"])); // crio e dá uma auto id
                    } 
                    else // se tiver open alex id
                    {
                        insertDataSQLite("author", "name, openalexid", String.Format("'{0}', '{1}'", authors[j]["name"], authors[j]["openalexid"])); // crio e dá uma auto id e guarda a nova openalexid
                    }
                    
                    authorid = getAuthorByOpenAlexId((string)authors[j]["openalexid"], (string)authors[j]["name"]); // guardo o id novo do autor para meter na tabela de relação
                }
                
                insertDataSQLite("author_paper", "author_id, paper_id", String.Format("{0}, {1}", authorid, paperid)); // insiro dados na tabela de relação

                // INSTITUTIONS
                if(authors[j]["institution"] != null && authors[j]["institution"].ToString().Length > 0)
                {
                    int institutionid = getInstitutionByOpenAlexId((string)authors[j]["institution"]["openalexid"], (string)authors[j]["institution"]["name"]);
                    
                    if (institutionid == -1) // se não existir uma instituição com o mesmo nome nem com o mesmo openalex eu crio e crio a relação autor instituição
                    {
                        if ((string)authors[j]["institution"]["openalexid"] == null)
                        {
                            string cc = (string)authors[j]["institution"]["name"];

                            insertDataSQLite("institution", "name, countrycode", String.Format("'{0}', '{1}'", authors[j]["institution"]["name"], cc == null ? "" : cc));
                        }
                        else
                        {
                            string cc = (string)authors[j]["institution"]["name"];

                            insertDataSQLite("institution", "name, countrycode, openalexid", String.Format("'{0}', '{1}', '{2}'", authors[j]["institution"]["name"], cc == null ? "" : cc, authors[j]["institution"]["openalexid"]));
                        }

                        institutionid = getInstitutionByOpenAlexId((string)authors[j]["institution"]["openalexid"], (string)authors[j]["institution"]["name"]);

                        insertDataSQLite("author_institution", "author_id, institution_id", String.Format("{0}, {1}", authorid, institutionid));
                    }
                    else // se a instituição já existe
                    {
                        if (!checkAuthorInstRelation(authorid, institutionid)) // se o autor não tiver uma relação com essa instituição eu crio
                        {
                            insertDataSQLite("author_institution", "author_id, institution_id", String.Format("{0}, {1}", authorid, institutionid));
                        }
                    }
                } 
            }

            // PUB OUTLET  
            if (apipapers[i]["journal"] != null && apipapers[i]["journal"].ToString().Length > 0)
            {
                string journalname = (string)apipapers[i]["journal"]["name"];
                if (journalname == null && (string)apipapers[i]["journal"]["publisher"] != null)
                {
                    journalname = (string)apipapers[i]["journal"]["publisher"];
                }

                int puboutletid = getPubOutletByOpenAlexId((string)apipapers[i]["journal"]["openalexid"], journalname);

                if (puboutletid == -1)
                {
                    string url = (string)apipapers[i]["journal"]["url"];
                    string issn = (string)apipapers[i]["journal"]["issn"];

                    if ((string)apipapers[i]["journal"]["openalexid"] == null)
                    {
                        insertDataSQLite("puboutlet", "name, url, issn", String.Format("'{0}', '{1}', '{2}'", apipapers[i]["journal"]["name"], (url == null ? "" : url), (issn == null ? "" : issn)));
                    }
                    else
                    {
                        insertDataSQLite("puboutlet", "name, url, issn, openalexid", String.Format("'{0}', '{1}', '{2}', '{3}'", apipapers[i]["journal"]["name"], (url == null ? "" : url), (issn == null ? "" : issn), apipapers[i]["journal"]["openalexid"]));
                    }

                    puboutletid = getPubOutletByOpenAlexId((string)apipapers[i]["journal"]["openalexid"], journalname);
                }

                insertDataSQLite("puboutlet_paper", "puboutlet_id, paper_id", String.Format("{0}, {1}", puboutletid, paperid));
            }
        }


        // ACCOUNT
        List<RelationTable> origin_account = new List<RelationTable>();
        ReadCsvRelationTable("origin_account.csv", origin_account);

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

    int getInstitutionByOpenAlexId(string openalexid, string instname)
    {
        IDbCommand _command = _connection.CreateCommand();

        string sqlQuery = "SELECT id FROM institution WHERE openalexid LIKE '" + (openalexid == null ? "dummyid" : openalexid) + "' OR name LIKE '" + instname + "'";

        _command.CommandText = sqlQuery;

        IDataReader reader = _command.ExecuteReader();

        int instid = -1;
        while (reader.Read())
        {
            instid = reader.GetInt32(0);
        }

        return instid;
    }

    bool checkAuthorInstRelation(int authorid, int institutionid)
    {
        IDbCommand _command = _connection.CreateCommand();

        string sqlQuery = "SELECT institution_id FROM author_institution WHERE author_id =" + authorid + " AND institution_id=" + institutionid;

        _command.CommandText = sqlQuery;

        IDataReader reader = _command.ExecuteReader();

        int value = -1;
        bool exists = false;
        while (reader.Read())
        {
            value = reader.GetInt32(0);
        }

        if(value != -1)
        {
            exists = true;
        }

        return exists;
    }

    int getAuthorByOpenAlexId(string openalexid, string authorname)
    {
        IDbCommand _command = _connection.CreateCommand();

        string sqlQuery = "SELECT id FROM author WHERE openalexid LIKE '" + (openalexid == null ? "dummyid" : openalexid) + "' OR name LIKE '" + authorname + "'";

        _command.CommandText = sqlQuery;

        IDataReader reader = _command.ExecuteReader();

        int authorid = -1;
        while (reader.Read())
        {
            authorid = reader.GetInt32(0);
        }

        return authorid;
    }

    int getPubOutletByOpenAlexId(string openalexid, string pubname)
    {
        IDbCommand _command = _connection.CreateCommand();

        string sqlQuery = "SELECT id FROM puboutlet WHERE openalexid LIKE '" + (openalexid == null ? "dummyid" : openalexid) + "' OR name LIKE '" + pubname + "'";

        _command.CommandText = sqlQuery;

        IDataReader reader = _command.ExecuteReader();

        int pubid = -1;
        while (reader.Read())
        {
            pubid = reader.GetInt32(0);
        }

        return pubid;
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
                    //InitData paper;

                    if (json["results"] != null)
                    {
                        JArray results = (JArray)json["results"];

                        string title = null;
                        string date = null;
                        string openalexid = null;

                        int totalauthors = 0;
                        string[] authorname = null;
                        string[] authoropenalexid = null;
                        string[] rawaffiliation = null;
                        string[] instname = null;
                        string[] countrycode = null;
                        string[] instopenalexid = null;

                        string pubname = null;
                        string publisher = null;
                        string puburl = null;
                        string pubopenalexid = null;
                        string issn = null;

                        if (results.Count > 0)
                        {
                            title = (string)json["results"][0]["title"];
                            date = (string)json["results"][0]["publication_date"];
                            openalexid = (string)json["results"][0]["id"];

                            if (json["results"][0]["authorships"] != null)
                            {
                                JArray authorships = (JArray)json["results"][0]["authorships"];
                                totalauthors = authorships.Count;

                                if (authorships.Count > 0)
                                {
                                    authorname = new string[authorships.Count];
                                    authoropenalexid = new string[authorships.Count];
                                    rawaffiliation = new string[authorships.Count];
                                    instname = new string[authorships.Count];
                                    countrycode = new string[authorships.Count];
                                    instopenalexid = new string[authorships.Count];

                                    for (int i = 0; i < totalauthors; i++)
                                    {
                                        authorname[i] = (string)authorships[i]["author"]["display_name"];
                                        authoropenalexid[i] = (string)authorships[i]["author"]["id"];
                                        rawaffiliation[i] = (string)authorships[i]["raw_affiliation_string"];

                                        JArray institutions = (JArray)authorships[i]["institutions"];
                                        int totalinstitutions = 0;
                                        
                                        if (institutions != null)
                                        {
                                            totalinstitutions = institutions.Count;
                                        }
                                        
                                        for (int j = 0; j < totalinstitutions; j++)
                                        {
                                            instname[i] = (string)institutions[j]["display_name"];
                                            countrycode[i] = (string)institutions[j]["country_code"];
                                            instopenalexid[i] = (string)institutions[j]["id"];
                                        }
                                    }
                                }

                                pubname = (string)json["results"][0]["host_venue"]["display_name"];
                                publisher = (string)json["results"][0]["host_venue"]["publisher"];
                                puburl = (string)json["results"][0]["host_venue"]["url"];
                                pubopenalexid = (string)json["results"][0]["host_venue"]["id"];
                                issn = (string)json["results"][0]["host_venue"]["issn_l"];
                            }
                        }
 
                        InitData paper = new InitData(openalexid, title, date, totalauthors, authorname, authoropenalexid, rawaffiliation, instname, countrycode, instopenalexid, pubname, publisher, puburl, pubopenalexid, issn);
                        fullpapers.Add(paper);

                    } else
                    {
                        InitData paper = null;
                        fullpapers.Add(paper);
                    }
                  
                    break;
            }
        }
    }

    public class InitData
    {
        public string openalexid;
        public string title;
        public string date; 
        int totalauthors;

        public PubOutlet journal;
        public List<Author> authors = new List<Author>();
        //public int id, use_id, practice_id, strategy_id;

        public InitData(string openalexid, string title, string date, int totalauthors, string[] authorname, string[] authoropenalexid, string[] rawaffiliation, string[] instname, string[] countrycode, string[] instopenalexid, string pubname, string publisher, string puburl, string pubopenalexid, string issn)
        {
            this.openalexid = openalexid;
            this.title = title;;
            this.date = date;
            this.totalauthors = totalauthors;
           
            if(authorname != null && rawaffiliation != null)
            {
                this.totalauthors = authorname.Length;
                
                if (this.totalauthors > 0)
                {
                    for (int i = 0; i < totalauthors; i++)
                    {
                        authors.Add(new Author(authorname[i], authoropenalexid[i], rawaffiliation[i], instname[i], countrycode[i], instopenalexid[i]));
                    }
                }
            }

            journal = new PubOutlet(pubname, publisher, puburl, pubopenalexid, issn);
        }

        public class PubOutlet
        {
            public string name;
            public string publisher;
            public string url;
            public string openalexid;
            public string issn;
          
            public PubOutlet(string name, string publisher, string url, string openalexid, string issn)
            {
                this.name = name;
                this.publisher = publisher;
                this.url = url;
                this.openalexid = openalexid;
                this.issn = issn;
            }
        }

        public class Author
        {
            public string name;
            public string openalexid;
            public string rawaffiliation;
            public Institution institution;

            public Author(string name, string authoropenalexid, string rawaffiliation, string instname, string countrycode, string instopenalexid)
            {
                this.name = name;
                this.openalexid = authoropenalexid;
                this.rawaffiliation = rawaffiliation;
                this.institution = new Institution(instname, countrycode, instopenalexid);
            }

            public class Institution
            {
                public string name;
                public string countrycode;
                public string openalexid;

                public Institution(string name, string countrycode, string openalexid)
                {
                    this.name = name;
                    this.countrycode = countrycode;
                    this.openalexid = openalexid;
                }
            }
        }
    }

    public class DataObject
    {
        public List<InitData> objects = new List<InitData>();

        public DataObject(List<InitData> objects)
        {
            this.objects = objects;
        }
    }
}
