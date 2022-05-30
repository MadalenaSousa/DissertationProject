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

public class APIManager : MonoBehaviour
{
    public static APIManager instance;

    public TextAsset ParsCitTextFile;
    string[] titles;

    public bool getApiData = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        // Save paper titles to use them to call the api
        JObject parscitdata = JObject.Parse(ParsCitTextFile.text);
        JArray parscitpapers = (JArray)parscitdata["citationList"];
        titles = new string[parscitpapers.Count];
        for (int i = 0; i < parscitpapers.Count; i++)
        {        
            titles[i] = (string)parscitpapers[i]["title"];
        }

        // Call API
        List<InitData> temppaperdata = new List<InitData>();

        if(getApiData)
        {
            StartCoroutine(WriteData(titles, temppaperdata));
        }

        //List<int> citationCounts = new List<int>();
        //StartCoroutine(WriteDataCitations(titles, citationCounts));

    }

    IEnumerator WriteData(string[] papertitles, List<InitData> paperlist) // Write Json with API data
    {
        
        for (int i = 0; i < papertitles.Length; i++)
        {
            string encoded = papertitles[i].Replace(",", "%2C");
            encoded = encoded.Replace(".", "%2E");
            encoded = encoded.Replace(" ", "%20");
            encoded = encoded.Replace(":", "%3A");
            encoded = encoded.Replace(";", "%3B");
            
            Debug.Log("Current Call: " + i);
            
            yield return StartCoroutine(GetRequest("https://api.openalex.org/works?mailto=up202003391@up.pt&filter=title.search:" + encoded, paperlist));

            Thread.Sleep(200);
        }

        StreamWriter writer = new StreamWriter("Assets/Resources/apidata.json");

        DataObject totaldata = new DataObject(paperlist);
        JObject jsonpaper = JObject.FromObject(totaldata);

        writer.Write(jsonpaper);
        writer.Close();
    }
  
    IEnumerator GetRequest(string uri, List<InitData> paperlist) // Request API data and format to InitData type object
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
                        paperlist.Add(paper);

                    } else
                    {
                        InitData paper = null;
                        paperlist.Add(paper);
                    }
                  
                    break;
            }
        }
    }
    IEnumerator WriteDataCitations(string[] papertitles, List<int> paperlist) // Write Json with API data
    {

        for (int i = 0; i < papertitles.Length; i++)
        {
            string encoded = papertitles[i].Replace(",", "%2C");
            encoded = encoded.Replace(".", "%2E");
            encoded = encoded.Replace(" ", "%20");
            encoded = encoded.Replace(":", "%3A");
            encoded = encoded.Replace(";", "%3B");

            Debug.Log("Current Call: " + i);

            yield return StartCoroutine(GetCitations("https://api.openalex.org/works?mailto=up202003391@up.pt&filter=title.search:" + encoded, paperlist));

            Thread.Sleep(200);
        }

        StreamWriter writer = new StreamWriter("Assets/Resources/apidatacitations.json");

        DataObjectCitations totaldata = new DataObjectCitations(paperlist);
        JObject jsonpaper = JObject.FromObject(totaldata);

        writer.Write(jsonpaper);
        writer.Close();
    }

    IEnumerator GetCitations(string uri, List<int> paperlist) // Request API data and format to InitData type object
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

                    if (json["results"] != null)
                    {
                        JArray results = (JArray)json["results"];

                        int citationCount = -1;

                        if (results.Count > 0)
                        {
                            if(json["results"][0]["cited_by_count"] != null)
                            {
                                citationCount = (int)json["results"][0]["cited_by_count"];
                            }                            
                        }

                        paperlist.Add(citationCount);

                    }
                    else
                    {
                        paperlist.Add(-1);
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

    public class DataObjectCitations
    {
        public List<int> objects = new List<int>();

        public DataObjectCitations(List<int> objects)
        {
            this.objects = objects;
        }
    }
}
