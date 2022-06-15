using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cluster
{
    public int min, max;
    public List<Category> categories = new List<Category>();
    public Vector3 center;
    public int avg;
    public int offsetMulti = 20;

    public Cluster(int min, int max)
    {
        this.min = min;
        this.max = max;

        center = Random.insideUnitSphere * PracticesAndStrategies.instance.globalSphereRadius;
        avg = (max - min) / 2 + min;
    }

    public Vector3 getOffsetVector(CategoryView category)
    {
        int offsetRadius = Mathf.Abs(category.clusterCriteria - avg) * offsetMulti;
        
        return (Random.insideUnitSphere * offsetRadius);
    }

    public int getRandom(int first, int second)
    {
        if (Random.value < 0.5f)
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