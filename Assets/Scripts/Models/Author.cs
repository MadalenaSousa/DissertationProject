using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Author
{
    public int id;
    public string name;
    public string openalexid;
    public Institution institution;

    public Author(int id, string name, string authoropenalexid, int instid, string instname, string countrycode, string instopenalexid)
    {
        this.id = id;
        this.name = name;
        this.openalexid = authoropenalexid;
        this.institution = new Institution(instid, instname, countrycode, instopenalexid);
    }
}