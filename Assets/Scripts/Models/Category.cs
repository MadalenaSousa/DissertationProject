using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Category
{
    public int id = 0;
    public string name = "none";
    public enum Type {none, practice, strategy, use};

    public Type catType = Type.none;
}

public class Practice : Category
{
    public Practice(int id, string name)
    {
        this.id = id;
        this.name = name;
        this.catType = Type.practice;
    }
}

public class Strategy : Category
{
    public Strategy(int id, string name)
    {
        this.id = id;
        this.name = name;
        this.catType = Type.strategy;
    }
}

public class Use : Category
{
    public Use(int id, string name)
    {
        this.id = id;
        this.name = name;
        this.catType = Type.use;
    }
}


