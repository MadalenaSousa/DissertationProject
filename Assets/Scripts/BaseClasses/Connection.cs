using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Connection
{
    public int paperId;
    public Category category;

    public Connection(int paperId, Category category) {
        this.paperId = paperId;
        this.category = category;
    }
}
