using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System;
using LumenWorks.Framework.IO.Csv;
using UnityEngine.UI;

public class Paper : MonoBehaviour
{
    Database db;

    // DB based variables
    string title, date;
    int year;
    List<Author> author;
    PubOutlet publication_outlet;

    // externaly defined variables
    int id;

    public Paper(int id)
    {
        this.id = id;
        this.db = Database.instance;
        db.StartDataSQLite();

        this.title = db.getTitleById(id);
        this.date = db.getDateById(id);
        this.year = db.getYearById(id);

        this.author = db.getAuthorAndInstitutionByPaperId(id);
        this.publication_outlet = db.getPubOutletByPaperId(id);
    }
}

public class Author
{
    int id;
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

public class Practice : Category
{
    public Practice()
    {
        this.id = 1;
        this.name = "Alface";
    }
}

public class Strategy : Category
{
    public Strategy()
    {
        this.id = 1;
        this.name = "Cebola";
    }
}

public class Use : Category
{
    public Use()
    {
        this.id = 1;
        this.name = "Cacau";
    }
}

public class Category
{
    public int id;
    public string name;
}

