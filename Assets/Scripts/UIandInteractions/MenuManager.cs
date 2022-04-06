using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    void Start()
    {
        
    }

    public void SwitchScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }

}
