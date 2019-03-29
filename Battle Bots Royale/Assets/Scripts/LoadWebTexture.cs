using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LoadWebTexture : MonoBehaviour
{
    [Header("Post your funny cat meme here")]
    public string url = "https://imgur.com/r/funnycats/ii98X";

    // Start is called before the first frame update
    void Start()
    {
        UnityWebRequest request = new UnityWebRequest(url);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
