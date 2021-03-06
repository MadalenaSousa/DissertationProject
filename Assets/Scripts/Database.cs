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
using Cinemachine;

public class Database : MonoBehaviour
{
    public static Database instance;

    private IDbConnection _connection;
    
    [SerializeField]
    private bool FILL_TABLES = false;

    public TextAsset APIDataTextFile;

    private void Awake()
    {
        StartDataSQLite();

        if (instance == null)
        {
            instance = this;
        }    
    }

    void Start()
    {
        JObject apidata = JObject.Parse(APIDataTextFile.text);
        JArray apipapers = (JArray)apidata["objects"];

        if(FILL_TABLES)
        {
            FillTables(apipapers);
        }

        //insertCitations(apipapers);

    }

    void StartDataSQLite()
    {
        string urlDataBase = "URI = file:Assets/Data/dbupdated.db";

        // create Connection to the Database
        this._connection = new SqliteConnection(urlDataBase);

        _connection.Open();
    }

    // DB functions
    public Paper getPaperById(int id)
    {
        IDbCommand _command = _connection.CreateCommand();

        string sqlQuery = "SELECT paper.title, paper.date, paper.pubyear, paper.citationcount FROM paper WHERE id =" + id;
        _command.CommandText = sqlQuery;

        IDataReader reader = _command.ExecuteReader();

        string title = "";
        string date = "";
        int year = -1;
        int citationCount = -1;

        while (reader.Read())
        {
            title = reader.GetString(0);
            date = reader.GetString(1);
            year = reader.GetInt32(2);
            citationCount = reader.GetInt32(3);         
        }

        return new Paper(id, title, date, year, getAuthorAndInstitutionByPaperId(id), getPubOutletByPaperId(id), getPracticeByPaperId(id), getStrategyByPaperId(id), getUseByPaperId(id), citationCount);
    }

    private int getCitationCount(string maxOrMin, string category)
    {
        IDbCommand _command = _connection.CreateCommand();

        string sqlQuery = "";

        string sqlFunc;

        if (maxOrMin == "max")
        {
            sqlFunc = "MAX";
        }
        else
        {
            sqlFunc = "MIN";
        }

        if (category == "practices")
        {
            sqlQuery = "SELECT " + sqlFunc + "(sum) " +
                        "FROM(SELECT practices_account.practices_id, SUM(paper.citationcount) as sum " +
                        "FROM practices_account " +
                        "LEFT JOIN paper_account ON paper_account.account_id = practices_account.account_id " +
                        "LEFT JOIN paper ON paper.id = paper_account.paper_id " +
                        "GROUP BY practices_account.practices_id)";
        } 
        else if(category == "strategies")
        {
            sqlQuery = "SELECT " + sqlFunc + "(sum) " +
                        "FROM(SELECT account_strategies.strategies_id, SUM(paper.citationcount) as sum " +
                        "FROM account_strategies " +
                        "LEFT JOIN paper_account ON paper_account.account_id = account_strategies.account_id " +
                        "LEFT JOIN paper ON paper.id = paper_account.paper_id " +
                        "GROUP BY account_strategies.strategies_id)";
        }

        _command.CommandText = sqlQuery;

        IDataReader reader = _command.ExecuteReader();

        int max = 0;

        while (reader.Read())
        {
            if (!reader.IsDBNull(0))
            {
                max = reader.GetInt32(0);
            }
        }

        return max;
    }

    public int getMaxCitPS()
    {
        return Math.Max(getCitationCount("max", "practices"), getCitationCount("max", "strategies"));
    }

    public int getMinCitPS()
    {
        return Math.Min(getCitationCount("min", "practices"), getCitationCount("min", "strategies"));
    }

    public int getMaxConnPS()
    {
        return Math.Max(getMaxConnPractices(), getMaxConnStrategies());
    }

    private int getMaxConnPractices()
    {
        IDbCommand _command = _connection.CreateCommand();

        string sqlQuery = "SELECT MAX(count) FROM(SELECT practices_id, COUNT(*) as count FROM practices_account LEFT JOIN paper_account WHERE practices_account.account_id = paper_account.account_id GROUP BY practices_id)";

        _command.CommandText = sqlQuery;

        IDataReader reader = _command.ExecuteReader();

        int max = 0;

        while (reader.Read())
        {
            if (!reader.IsDBNull(0))
            {
                max = reader.GetInt32(0);
            }
        }

        return max;
    }

    private int getMaxConnStrategies()
    {
        IDbCommand _command = _connection.CreateCommand();

        string sqlQuery = "SELECT MAX(count) FROM(SELECT strategies_id, COUNT(*) as count FROM account_strategies LEFT JOIN paper_account WHERE account_strategies.account_id = paper_account.account_id GROUP BY strategies_id)";

        _command.CommandText = sqlQuery;

        IDataReader reader = _command.ExecuteReader();

        int max = 0;

        while (reader.Read())
        {
            if (!reader.IsDBNull(0))
            {
                max = reader.GetInt32(0);
            }
        }

        return max;
    }

    public int getMinConnPS()
    {
        return Math.Min(getMinConnPractices(), getMinConnStrategies());
    }

    private int getMinConnPractices()
    {
        IDbCommand _command = _connection.CreateCommand();

        string sqlQuery = "SELECT MIN(count) " +
                        "FROM(" +
                            "SELECT practices_id, COUNT(*) as count " +
                            "FROM practices_account " +
                            "LEFT JOIN paper_account WHERE practices_account.account_id = paper_account.account_id " +
                            "GROUP BY practices_id)";

        _command.CommandText = sqlQuery;

        IDataReader reader = _command.ExecuteReader();

        int max = 0;

        while (reader.Read())
        {
            if (!reader.IsDBNull(0))
            {
                max = reader.GetInt32(0);
            }
        }

        return max;
    }

    private int getMinConnStrategies()
    {
        IDbCommand _command = _connection.CreateCommand();

        string sqlQuery = "SELECT MIN(count) FROM(SELECT strategies_id, COUNT(*) as count FROM account_strategies LEFT JOIN paper_account WHERE account_strategies.account_id = paper_account.account_id GROUP BY strategies_id)";

        _command.CommandText = sqlQuery;

        IDataReader reader = _command.ExecuteReader();

        int max = 0;

        while (reader.Read())
        {
            if (!reader.IsDBNull(0))
            {
                max = reader.GetInt32(0);
            }
        }

        return max;
    }

    public List<int> filter(string author, string journal, string institution, int yearMin, int yearMax)
    {
        IDbCommand _command = _connection.CreateCommand();

        string sqlQuery = "SELECT DISTINCT paper.id " +
                            "FROM paper " +
                            "LEFT JOIN puboutlet_paper ON paper.id = puboutlet_paper.paper_id " +
                            "LEFT JOIN puboutlet ON puboutlet_paper.puboutlet_id = puboutlet.id " +
                            "LEFT JOIN author_paper ON paper.id = author_paper.paper_id " +
                            "LEFT JOIN author ON author.id = author_paper.author_id " +
                            "LEFT JOIN author_institution ON author_institution.author_id = author_paper.author_id " +
                            "LEFT JOIN institution ON institution.id = author_institution.institution_id " +
                            "WHERE(author.name LIKE '%" + (author ?? "") + "%'" + (author == null ? " OR author.name IS NULL" : "") + ")" +
                            "AND(puboutlet.name LIKE '%" + (journal ?? "") + "%'" + (journal == null ? " OR puboutlet.name IS NULL" : "") + ")" +
                            "AND(institution.name LIKE '%" + (institution ?? "") + "%'" + (institution == null ? " OR institution.name IS NULL" : "") + ")" +
                            "AND paper.pubyear >=" + yearMin + " AND paper.pubyear <=" + yearMax;

        _command.CommandText = sqlQuery;

        IDataReader reader = _command.ExecuteReader();

        List<int> results = new List<int>();

        while (reader.Read())
        {
            if (!reader.IsDBNull(0))
            {
                results.Add(reader.GetInt32(0));
            }
        }

        return results;
    }

    public Practice getPracticeById(int id)
    {
        IDbCommand _command = _connection.CreateCommand();

        string sqlQuery = "SELECT practices.id, practices.name FROM practices WHERE practices.id=" + id;

        _command.CommandText = sqlQuery;

        IDataReader reader = _command.ExecuteReader();

        Practice practice = null;

        while (reader.Read())
        {
            if (!reader.IsDBNull(0))
            {
                practice = new Practice(reader.GetInt32(0), reader.GetString(1));
            }
        }

        return practice;
    }

    public Strategy getStrategyById(int id)
    {
        IDbCommand _command = _connection.CreateCommand();

        string sqlQuery = "SELECT strategies.id, strategies.name FROM strategies WHERE strategies.id=" + id;

        _command.CommandText = sqlQuery;

        IDataReader reader = _command.ExecuteReader();

        Strategy strategy = null;

        while (reader.Read())
        {
            if (!reader.IsDBNull(0))
            {
                strategy = new Strategy(reader.GetInt32(0), reader.GetString(1));
            }
        }

        return strategy;
    }

    public List<int> getStrategies()
    {
        IDbCommand _command = _connection.CreateCommand();

        string sqlQuery = "SELECT strategies.id FROM strategies";

        _command.CommandText = sqlQuery;

        IDataReader reader = _command.ExecuteReader();

        List<int> strategies = new List<int>();

        while (reader.Read())
        {
            if (!reader.IsDBNull(0))
            {
                strategies.Add(reader.GetInt32(0));
            }
        }

        return strategies;
    }

    public List<int> getPractices()
    {
        IDbCommand _command = _connection.CreateCommand();

        string sqlQuery = "SELECT practices.id FROM practices";

        _command.CommandText = sqlQuery;

        IDataReader reader = _command.ExecuteReader();

        List<int> practices = new List<int>();

        while (reader.Read())
        {
            if (!reader.IsDBNull(0))
            {
                practices.Add(reader.GetInt32(0));
            }
        }

        return practices;
    }

    public List<Paper> getPapersForPracticeById(int practiceId)
    {
        IDbCommand _command = _connection.CreateCommand();

        string sqlQuery = "SELECT DISTINCT paper_id"
                        + " FROM paper"
                        + " LEFT JOIN paper_account ON paper.id = paper_account.paper_id"
                        + " LEFT JOIN practices_account ON paper_account.account_id = practices_account.account_id"
                        + " WHERE practices_account.practices_id = " + practiceId;

        _command.CommandText = sqlQuery;

        IDataReader reader = _command.ExecuteReader();

        List<Paper> papers = new List<Paper>();

        while (reader.Read())
        {
            if (!reader.IsDBNull(0))
            {
                papers.Add(getPaperById(reader.GetInt32(0)));
            }
        }

        return papers;
    }

    public List<Paper> getPapersForStrategyById(int strategyId)
    {
        IDbCommand _command = _connection.CreateCommand();

        string sqlQuery = "SELECT DISTINCT paper_id"
                + " FROM paper"
                + " LEFT JOIN paper_account ON paper.id = paper_account.paper_id"
                + " LEFT JOIN account_strategies ON paper_account.account_id = account_strategies.account_id"
                + " WHERE account_strategies.strategies_id = " + strategyId;

        _command.CommandText = sqlQuery;

        IDataReader reader = _command.ExecuteReader();

        List<Paper> papers = new List<Paper>();

        while (reader.Read())
        {
            if (!reader.IsDBNull(0))
            {
                papers.Add(getPaperById(reader.GetInt32(0)));
            }
        }

        return papers;
    }

    public List<int> getPapersWithPracticesOrStrategies()
    {
        IDbCommand _command = _connection.CreateCommand();

        string sqlQuery = "SELECT paper.id " +
            "FROM paper " +
            "WHERE paper.id IN " +
                "(SELECT paper_account.paper_id " +
                "FROM paper_account " +
                "INNER JOIN practices_account ON practices_account.account_id = paper_account.account_id) " +
            "OR paper.id IN " +
                "(SELECT paper_account.paper_id " +
                "FROM paper_account " +
                "INNER JOIN account_strategies ON account_strategies.account_id = paper_account.account_id)";

        _command.CommandText = sqlQuery;

        IDataReader reader = _command.ExecuteReader();

        List<int> papers = new List<int>();

        while (reader.Read())
        {
            if (!reader.IsDBNull(0))
            {
                papers.Add(reader.GetInt32(0));
            }
        }

        return papers;
    }

    public List<string> getPSAuthors()
    {
        IDbCommand _command = _connection.CreateCommand();

        string sqlQuery = "SELECT DISTINCT author.name " +
                        "FROM author " +
                        "LEFT JOIN author_paper ON author.id = author_paper.author_id " +
                        "WHERE author.id = author_paper.author_id AND author_paper.paper_id IN " +
                            "(SELECT paper.id " +
                            "FROM paper " +
                            "WHERE paper.id IN " +
                                "(SELECT paper_account.paper_id " +
                                "FROM paper_account " +
                                "INNER JOIN practices_account ON practices_account.account_id = paper_account.account_id) " +
                            "OR paper.id IN " +
                                "(SELECT paper_account.paper_id " +
                                "FROM paper_account " +
                                "INNER JOIN account_strategies ON account_strategies.account_id = paper_account.account_id))";

        _command.CommandText = sqlQuery;

        IDataReader reader = _command.ExecuteReader();

        List<string> elementsInTable = new List<string>();

        while (reader.Read())
        {
            if (!reader.IsDBNull(0))
            {
                elementsInTable.Add(reader.GetString(0));
            }
        }

        return elementsInTable;
    }

    public List<string> getPSJournals()
    {
        IDbCommand _command = _connection.CreateCommand();

        string sqlQuery = "SELECT DISTINCT puboutlet.name " +
                        "FROM puboutlet " +
                        "LEFT JOIN puboutlet_paper ON puboutlet.id = puboutlet_paper.puboutlet_id " +
                        "WHERE puboutlet.id = puboutlet_paper.puboutlet_id AND puboutlet_paper.paper_id IN " +
                            "(SELECT paper.id " +
                            "FROM paper " +
                            "WHERE paper.id IN " +
                                "(SELECT paper_account.paper_id " +
                                "FROM paper_account " +
                                "INNER JOIN practices_account ON practices_account.account_id = paper_account.account_id) " +
                            "OR paper.id IN " +
                                "(SELECT paper_account.paper_id " +
                                "FROM paper_account " +
                                "INNER JOIN account_strategies ON account_strategies.account_id = paper_account.account_id))";

        _command.CommandText = sqlQuery;

        IDataReader reader = _command.ExecuteReader();

        List<string> elementsInTable = new List<string>();

        while (reader.Read())
        {
            if (!reader.IsDBNull(0))
            {
                elementsInTable.Add(reader.GetString(0));
            }
        }

        return elementsInTable;
    }

    public List<string> getPSInstitutions()
    {
        IDbCommand _command = _connection.CreateCommand();

        string sqlQuery = "SELECT DISTINCT institution.name " +
                        "FROM institution " +
                        "LEFT JOIN author_institution ON institution.id = author_institution.institution_id " +
                        "LEFT JOIN author_paper ON author_institution.author_id = author_paper.author_id " +
                        "WHERE author_institution.author_id = author_paper.author_id AND author_paper.paper_id IN " +
                            "(SELECT paper.id FROM paper " +
                            "WHERE paper.id IN " +
                                "(SELECT paper_account.paper_id " +
                                "FROM paper_account " +
                                "INNER JOIN practices_account ON practices_account.account_id = paper_account.account_id) " +
                            "OR paper.id IN " +
                                "(SELECT paper_account.paper_id " +
                                "FROM paper_account " +
                                "INNER JOIN account_strategies ON account_strategies.account_id = paper_account.account_id))";

        _command.CommandText = sqlQuery;

        IDataReader reader = _command.ExecuteReader();

        List<string> elementsInTable = new List<string>();

        while (reader.Read())
        {
            if (!reader.IsDBNull(0))
            {
                elementsInTable.Add(reader.GetString(0));
            }
        }

        return elementsInTable;
    }

    public List<Author> getAuthorAndInstitutionByPaperId(int id)
    {
        IDbCommand _command = _connection.CreateCommand();
    
        string sqlQuery = "SELECT author.id, author.name, author.openalexid, author_institution.institution_id " +
            "FROM author_paper " +
            "LEFT JOIN author ON author_paper.author_id = author.id " +
            "LEFT JOIN author_institution ON author.id = author_institution.author_id W" +
            "HERE author_paper.paper_id =" + id;

        _command.CommandText = sqlQuery;

        IDataReader reader = _command.ExecuteReader();

        List<Author> authors = new List<Author>();
        while (reader.Read())
        {
            string authoropenalexid = !reader.IsDBNull(2) ? reader.GetString(2) : "";

            Institution authorInst = !reader.IsDBNull(3) ? getInstitutionById(reader.GetInt32(3)) : new Institution(-1, "", "", "");

            authors.Add(new Author(reader.GetInt32(0), reader.GetString(1), authoropenalexid, authorInst.id, authorInst.name, authorInst.countrycode, authorInst.openalexid));         
        }
    
        return authors;
    }

    public Institution getInstitutionById(int id)
    {
        IDbCommand _command = _connection.CreateCommand();

        string sqlQuery = "SELECT institution.id, institution.name, institution.countrycode, institution.openalexid FROM institution WHERE institution.id =" + id;
        _command.CommandText = sqlQuery;

        IDataReader reader = _command.ExecuteReader();

        int instId = -1;
        string name = "";
        string cc = "";
        string oaId = "";
            
        while (reader.Read())
        {
            instId = reader.GetInt32(0);
            if (!reader.IsDBNull(1)) { name = reader.GetString(1); }
            if (!reader.IsDBNull(2)) { cc = reader.GetString(2); }
            if (!reader.IsDBNull(3)) { oaId = reader.GetString(3); }            
        }

        Institution newInstitution = new Institution(instId, name, cc, oaId);

        return newInstitution;
    }

    public PubOutlet getPubOutletByPaperId(int id)
    {
        IDbCommand _command = _connection.CreateCommand();

        string sqlQuery = "SELECT puboutlet.id, puboutlet.name, puboutlet.url, puboutlet.openalexid, puboutlet.issn FROM puboutlet, puboutlet_paper WHERE puboutlet_paper.puboutlet_id = puboutlet.id AND puboutlet_paper.paper_id =" + id;
        _command.CommandText = sqlQuery;

        IDataReader reader = _command.ExecuteReader();

        int pubid = -1;
        string pubname = "";
        string puburl = "";
        string puboalexid = "";
        string pubissn = "";

        while (reader.Read())
        {
            pubid = reader.GetInt32(0);
            pubname = reader.GetString(1);
            if (!reader.IsDBNull(2)) { puburl = reader.GetString(2); }
            if (!reader.IsDBNull(3)) { puboalexid = reader.GetString(3); }
            if (!reader.IsDBNull(4)) { pubissn = reader.GetString(4); }
        }
        
        PubOutlet publicationoutlet = new PubOutlet(pubid, pubname, puburl, puboalexid, pubissn);
        
        return publicationoutlet;
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------//

    //Functions for the FillTables() (one run only)

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

    public List<Practice> getPracticeByPaperId(int id)
    {
        IDbCommand _command = _connection.CreateCommand();

        string sqlQuery = "SELECT DISTINCT practices.id, practices.name  FROM practices, practices_account, paper_account WHERE practices_account.account_id = paper_account.account_id AND practices.id = practices_account.practices_id AND paper_account.paper_id =" + id;

        _command.CommandText = sqlQuery;

        IDataReader reader = _command.ExecuteReader();

        List<Practice> practice = new List<Practice>();
        
        while (reader.Read())
        {
            practice.Add(new Practice(reader.GetInt32(0), reader.GetString(1)));
        }

        return practice;
    }

    public List<Strategy> getStrategyByPaperId(int id)
    {
        IDbCommand _command = _connection.CreateCommand();

        string sqlQuery = "SELECT DISTINCT strategies.id, strategies.name  FROM strategies, account_strategies, paper_account WHERE account_strategies.account_id = paper_account.account_id AND strategies.id = account_strategies.strategies_id AND paper_account.paper_id =" + id;

        _command.CommandText = sqlQuery;

        IDataReader reader = _command.ExecuteReader();

        List<Strategy> strategy = new List<Strategy>();

        while (reader.Read())
        {
            strategy.Add(new Strategy(reader.GetInt32(0), reader.GetString(1)));
        }

        return strategy;
    }

    public List<Use> getUseByPaperId(int id)
    {
        IDbCommand _command = _connection.CreateCommand();

        string sqlQuery = "SELECT uses.id, uses.name  FROM uses, uses_account, paper_account WHERE uses_account.account_id = paper_account.account_id AND uses.id = uses_account.uses_id AND paper_account.paper_id =" + id;

        _command.CommandText = sqlQuery;

        IDataReader reader = _command.ExecuteReader();

        List<Use> use = new List<Use>();

        while (reader.Read())
        {
            if(!reader.IsDBNull(0))
            {
                use.Add(new Use(reader.GetInt32(0), reader.GetString(1)));
            }       
        }

        return use;
    }

    void insertDataSQLite(string insertTable, string insertFields, string insertValues)
    {
        IDbCommand _command = _connection.CreateCommand();
        string sql = String.Format("INSERT INTO {0}({1}) VALUES({2})", insertTable, insertFields, insertValues);

        _command.CommandText = sql;
        _command.ExecuteNonQuery();
    }

    void insertCitations(JArray citationCounts)
    {
        for(int i = 0; i < citationCounts.Count; i++)
        {
            IDbCommand _command = _connection.CreateCommand();
            string sql = "UPDATE paper SET citationcount = " + citationCounts[i] + " WHERE paper.id = " + i;

            _command.CommandText = sql;
            _command.ExecuteNonQuery();
        }
    }

    // Run one time only method to fill DB tables
    void FillTables(JArray apipaperlist)
    {
        // PAPERS
        for (int i = 0; i < apipaperlist.Count; i++)
        {
            var date = apipaperlist[i]["date"];
            string year = null;

            if (date != null && date.ToString().Length > 0)
            {
                year = date.ToString().Substring(0, 4);
            }

            string oaid = (string)apipaperlist[i]["openalexid"];

            int paperid = i;
            JArray authors = (JArray)apipaperlist[i]["authors"];

            insertDataSQLite("paper", "id, title, pubyear, date, openalexid", String.Format("{0}, '{1}', {2}, '{3}', '{4}'", paperid, apipaperlist[i]["title"], year == null ? 0 : Int32.Parse(year), date == null ? "" : date, oaid == null ? null : oaid));

            // AUTHORS
            for (int j = 0; j < authors.Count; j++)
            {
                int authorid = getAuthorByOpenAlexId((string)authors[j]["openalexid"], (string)authors[j]["name"]);

                if (authorid == -1) // se n?o existir author j? com id com o mesmo open alex id que o autor j nem com o mesmo nome
                {
                    if ((string)authors[j]["openalexid"] == null) // se ele n?o tiver open alex id
                    {
                        insertDataSQLite("author", "name", String.Format("'{0}'", authors[j]["name"])); // crio e d? uma auto id
                    }
                    else // se tiver open alex id
                    {
                        insertDataSQLite("author", "name, openalexid", String.Format("'{0}', '{1}'", authors[j]["name"], authors[j]["openalexid"])); // crio e d? uma auto id e guarda a nova openalexid
                    }

                    authorid = getAuthorByOpenAlexId((string)authors[j]["openalexid"], (string)authors[j]["name"]); // guardo o id novo do autor para meter na tabela de rela??o
                }

                insertDataSQLite("author_paper", "author_id, paper_id", String.Format("{0}, {1}", authorid, paperid)); // insiro dados na tabela de rela??o

                // INSTITUTIONS
                if (authors[j]["institution"] != null && authors[j]["institution"].ToString().Length > 0)
                {
                    int institutionid = getInstitutionByOpenAlexId((string)authors[j]["institution"]["openalexid"], (string)authors[j]["institution"]["name"]);

                    if (institutionid == -1) // se n?o existir uma institui??o com o mesmo nome nem com o mesmo openalex eu crio e crio a rela??o autor institui??o
                    {
                        if ((string)authors[j]["institution"]["countrycode"] == null)
                        {
                            string cc = (string)authors[j]["institution"]["name"];

                            insertDataSQLite("institution", "name, countrycode", String.Format("'{0}', '{1}'", authors[j]["institution"]["name"], cc == null ? "" : cc));
                        }
                        else
                        {
                            string cc = (string)authors[j]["institution"]["countrycode"];

                            insertDataSQLite("institution", "name, countrycode, openalexid", String.Format("'{0}', '{1}', '{2}'", authors[j]["institution"]["name"], cc == null ? "" : cc, authors[j]["institution"]["openalexid"]));
                        }

                        institutionid = getInstitutionByOpenAlexId((string)authors[j]["institution"]["openalexid"], (string)authors[j]["institution"]["name"]);

                        insertDataSQLite("author_institution", "author_id, institution_id", String.Format("{0}, {1}", authorid, institutionid));
                    }
                    else // se a institui??o j? existe
                    {
                        if (!checkAuthorInstRelation(authorid, institutionid)) // se o autor n?o tiver uma rela??o com essa institui??o eu crio
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

    // Data storing classes
    public class RelationTable { public int col1, col2; }
    public class DoubleColNameTable { public int col1; public string col2; }
    public class AccountTable { public int col1, col2; public string col3; }

    // Read CSV methods
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
