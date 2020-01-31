using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogSceneLoader : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SceneManager.LoadScene("Scenes/DialogUI", LoadSceneMode.Additive);
    }

    // Update is called once per frame
    void Update()
    {
    }
}
