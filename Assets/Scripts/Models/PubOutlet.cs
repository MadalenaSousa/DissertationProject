using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PubOutlet
{
    public int id;
    public string name;
    public string url;
    public string openalexid;
    public string issn;

    public PubOutlet(int id, string name, string url, string openalexid, string issn)
    {
        this.id = id;
        this.name = name;
        this.url = url;
        this.openalexid = openalexid;
        this.issn = issn;
    }
}
