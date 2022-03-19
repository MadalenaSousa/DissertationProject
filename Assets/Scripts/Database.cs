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

public class Database : MonoBehaviour
{
    public static Database instance;

    private IDbConnection _connection;
    private const bool FILL_TABLES = true;

    public TextAsset APIDataTextFile;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        JObject apidata = JObject.Parse(APIDataTextFile.text);
        JArray apipapers = (JArray)apidata["objects"];

        StartDataSQLite();
        FillTables(apipapers);
    }

    void StartDataSQLite()
    {
        string urlDataBase = "URI = file:Assets/Data/dbupdated.db";

        // create Connection to the Database
        this._connection = new SqliteConnection(urlDataBase);

        _connection.Open();
    }
  
    // DB functions
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

        if (value != -1)
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

    // run one time only method to fill DB tables
    void FillTables(JArray apipaperlist)
    {
        if (FILL_TABLES == false)
        {
            return;
        }

        // PAPERS
        for (int i = 0; i < apipaperlist.Count; i++)
        {
            var date = apipaperlist[i]["date"];
            string year = null;

            if (date != null && date.ToString().Length > 0)
            {
                year = date.ToString().Substring(0, 4);
            }

            int paperid = i;
            JArray authors = (JArray)apipaperlist[i]["authors"];

            insertDataSQLite("paper", "id, title, pubyear, date", String.Format("{0}, '{1}', {2}, '{3}'", paperid, apipaperlist[i]["title"], year == null ? 0 : Int32.Parse(year), date == null ? "" : date));

            // AUTHORS
            for (int j = 0; j < authors.Count; j++)
            {
                int authorid = getAuthorByOpenAlexId((string)authors[j]["openalexid"], (string)authors[j]["name"]);

                if (authorid == -1) // se não existir author já com id com o mesmo open alex id que o autor j nem com o mesmo nome
                {
                    if ((string)authors[j]["openalexid"] == null) // se ele não tiver open alex id
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
                if (authors[j]["institution"] != null && authors[j]["institution"].ToString().Length > 0)
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
            if (apipaperlist[i]["journal"] != null && apipaperlist[i]["journal"].ToString().Length > 0)
            {
                string journalname = (string)apipaperlist[i]["journal"]["name"];
                if (journalname == null && (string)apipaperlist[i]["journal"]["publisher"] != null)
                {
                    journalname = (string)apipaperlist[i]["journal"]["publisher"];
                }

                int puboutletid = getPubOutletByOpenAlexId((string)apipaperlist[i]["journal"]["openalexid"], journalname);

                if (puboutletid == -1)
                {
                    string url = (string)apipaperlist[i]["journal"]["url"];
                    string issn = (string)apipaperlist[i]["journal"]["issn"];

                    if ((string)apipaperlist[i]["journal"]["openalexid"] == null)
                    {
                        insertDataSQLite("puboutlet", "name, url, issn", String.Format("'{0}', '{1}', '{2}'", apipaperlist[i]["journal"]["name"], (url == null ? "" : url), (issn == null ? "" : issn)));
                    }
                    else
                    {
                        insertDataSQLite("puboutlet", "name, url, issn, openalexid", String.Format("'{0}', '{1}', '{2}', '{3}'", apipaperlist[i]["journal"]["name"], (url == null ? "" : url), (issn == null ? "" : issn), apipaperlist[i]["journal"]["openalexid"]));
                    }

                    puboutletid = getPubOutletByOpenAlexId((string)apipaperlist[i]["journal"]["openalexid"], journalname);
                }

                insertDataSQLite("puboutlet_paper", "puboutlet_id, paper_id", String.Format("{0}, {1}", puboutletid, paperid));
            }
        }


        // ACCOUNT
        List<RelationTable> origin_account = new List<RelationTable>();
        ReadCsvRelationTable("origin_account.csv", origin_account);

        for (int i = 0; i < origin_account.Count; i++)
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

    // data storing classes
    public class RelationTable { public int col1, col2; }
    public class DoubleColNameTable { public int col1; public string col2; }
    public class AccountTable { public int col1, col2; public string col3; }

    // read CSV methods
    void ReadCsv(string filename, Dictionary<int, string> dict)
    {
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
            }
        }
    }

    void ReadCsvAccount(string filename, List<AccountTable> dict)
    {
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
            }
        }
    }

}
