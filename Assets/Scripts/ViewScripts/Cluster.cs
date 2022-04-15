using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cluster
{
    public int min, max;
    public List<Category> categories = new List<Category>();
    public Vector3 center;
    public int avg;

    public Cluster(int min, int max)
    {
        //this.categories = clusterCategories;
        this.min = min;
        this.max = max;

        center = Random.insideUnitSphere * 500;
        avg = (min + max) / 2;
    }

    public Vector3 getOffsetVector(CategoryView category)
    {
        int xOffset = Mathf.Abs(category.totalConnections - avg) * getRandom(-1, 1);
        int yOffset = Mathf.Abs(category.totalConnections - avg) * getRandom(-1, 1);
        int zOffset = Mathf.Abs(category.totalConnections - avg) * getRandom(-1, 1);

        return new Vector3(xOffset, yOffset, zOffset);
    }

    public int getRandom(int first, int second)
    {
        if (UnityEngine.Random.value < 0.5f)
        {
            return first;
        }
        else
        {
            return second;
        }
    }
}


public class ClusterThreshold
{
    public int minValue, maxValue;
    public Vector3 clusterCenter;

    public ClusterThreshold(int min, int max)
    {
        this.minValue = min;
        this.maxValue = max;
    }
}