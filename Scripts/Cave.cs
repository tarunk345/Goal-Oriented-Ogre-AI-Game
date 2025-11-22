using UnityEngine;


public class Cave : MonoBehaviour
{
    public bool treasureStolen = false;
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
        treasureStolen = true;
        }
    }
}