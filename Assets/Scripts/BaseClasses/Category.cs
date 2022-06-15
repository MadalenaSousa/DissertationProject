using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class Category
{
    public int id = 0;
    public string name = "none";
    public float radius = 0;

    protected Category(int id, string name)
    {
        this.id = id;
        this.name = name;
    }
}

public class Practice : Category
{
    public Practice(int id, string name) : base(id, name) { }
}

public class Strategy : Category
{
    public Strategy(int id, string name) : base(id, name) { }
}

public class Use : Category
{
    public Use(int id, string name) : base(id, name) { }
}


