using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cluster
{
    public int min, max;
    public List<Category> categories = new List<Category>();
    public Vector3 center;
    public int avg;

    public Cluster(List<Category> clusterCategories, int min, int max)
    {
        this.categories = clusterCategories;
        this.min = min;
        this.max = max;

        center = Random.insideUnitSphere * 250;
        avg = (min + max) / 2;
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