using UnityEngine;
using System.Collections.Generic;

public class OgreManager : MonoBehaviour
{
    public GameObject trollPrefab;
    public List<GameObject> ogres;


    void Start()
    {
        SpawnTrolls();
    }
    void Update()
    {
        
    }

    public List<GameObject> SpawnTrolls() {
        List<GameObject> trolls = new List<GameObject>();
        Vector3 pos = new Vector3(Random.Range(-90f,-75f),0,Random.Range(5,40));
        GameObject t = Instantiate(trollPrefab, pos, Quaternion.identity);
        trolls.Add(t);
        t.GetComponent<OgreAgent>().spawnPoint = pos;
        pos = new Vector3(Random.Range(-90f,-75f),0,Random.Range(-40,-5));
        t = Instantiate(trollPrefab, pos, Quaternion.identity);
        trolls.Add(t);
        t.GetComponent<OgreAgent>().spawnPoint = pos;
        return trolls;
    }
}
