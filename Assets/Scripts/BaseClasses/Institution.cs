using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Institution
{
    public int id;
    public string name;
    public string countrycode;
    public string openalexid;

    public Institution(int id, string name, string countrycode, string openalexid)
    {
        this.id = id;
        this.name = name;
        this.countrycode = countrycode;
        this.openalexid = openalexid;
    }
}
