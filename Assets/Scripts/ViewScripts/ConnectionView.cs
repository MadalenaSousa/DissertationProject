using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionView : MonoBehaviour
{
    private PaperView paperView;
    private CategoryView categoryView;

    public void setConnection(PaperView paperView, CategoryView categoryView)
    {
        this.paperView = paperView;
        this.categoryView = categoryView;
    }

    public void updateConnection()
    {
        gameObject.SetActive(paperView.gameObject.activeInHierarchy);
        gameObject.GetComponent<LineRenderer>().SetPosition(0, paperView.getPosition());
        gameObject.GetComponent<LineRenderer>().SetPosition(1, categoryView.getPosition());
    }
}
