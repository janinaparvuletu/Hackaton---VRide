using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class GameDirector : MonoBehaviour
{
    public string urlServer = "http://127.0.0.1:5000/bpm";
    public float pulsCurent = 70f;

    void Start() { StartCoroutine(CitesteServer()); }

    IEnumerator CitesteServer()
    {
        while (true)
        {
            using (UnityWebRequest req = UnityWebRequest.Get(urlServer))
            {
                yield return req.SendWebRequest();
                if (req.result == UnityWebRequest.Result.Success) 
                    float.TryParse(req.downloadHandler.text, out pulsCurent);
            }
            yield return new WaitForSeconds(1f);
        }
    }
}