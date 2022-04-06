using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Category
{
    public int id = 0;
    public string name = "none";
}

public class Practice : Category
{
    public Practice(int id, string name)
    {
        this.id = id;
        this.name = name;
    }
}

public class Strategy : Category
{
    public Strategy(int id, string name)
    {
        this.id = id;
        this.name = name;
    }
}

public class Use : Category
{
    public Use(int id, string name)
    {
        this.id = id;
        this.name = name;
    }
}


