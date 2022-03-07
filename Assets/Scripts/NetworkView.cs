using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System;
using LumenWorks.Framework.IO.Csv;
using UnityEngine.UI;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class NetworkView : MonoBehaviour
{
    public static NetworkView instance;
    public List<GameObject> papers;
    PaperData data;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        data = PaperData.instance;
    }

    void Start()
    {
        papers = new List<GameObject>();

        foreach (KeyValuePair<int, string> pair in data.origin)
        {
            papers.Add(GameObject.CreatePrimitive(PrimitiveType.Sphere));
        }

        for (int i = 0; i < papers.Count; i++)
        {
            papers[i].AddComponent<Paper>();
            papers[i].GetComponent<Paper>().title = data.origin[i + 1];
            papers[i].GetComponent<Paper>().x = UnityEngine.Random.Range(-50f, 50f);
            papers[i].GetComponent<Paper>().y = UnityEngine.Random.Range(-50f, 50f);
            papers[i].GetComponent<Paper>().z = UnityEngine.Random.Range(-50f, 50f);
            papers[i].GetComponent<Paper>().radius = UnityEngine.Random.Range(-50f, 50f);
        }

        StartCoroutine(GetRequest("https://api.openalex.org/works?filter=title.search:coffee"));
    }

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
                    JArray authorships = (JArray)json["results"][0]["authorships"];

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
                    Debug.Log(jsonpaper);

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
            for(int i = 0; i < authorname.Length; i++)
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


