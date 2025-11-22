using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.ComponentModel;

public class RandomPlacement : MonoBehaviour {
    [Header("Prefabs")]
    public GameObject Boulder;
    public GameObject Shroom;
    public List<Bounds> obstacleBounds = new List<Bounds>();
    public List<Bounds> shroomBounds = new List<Bounds>();

    void Start() {
        PlaceObjects(Boulder, 4.5f, Random.Range(0, 360f), Random.Range(10,21), obstacleBounds);
        PlaceObjects(Shroom, 0f, 0f, Random.Range(5,11), shroomBounds);
    }
    
    //places obstacles in unobstructed parts of the board based on the detection using the colliders of the obstacles. 
    public void PlaceObjects(GameObject prefab, float y, float rotation, int num, List<Bounds> bounds) {
        int tries = 0;
        bounds.Clear();

        while (bounds.Count < num && tries < 200) {
            
            tries++;
            Vector3 pos = new Vector3(
                Random.Range(-70, 85), y,
                Random.Range(-35, 35)
            );
            Quaternion rot = Quaternion.Euler(0, rotation, 0);

            GameObject obstacle = Instantiate(prefab, pos, rot);

            Collider[] colliders = obstacle.GetComponentsInChildren<Collider>();
            if (colliders.Length == 0) return;

            Bounds newBounds = colliders[0].bounds;
            for (int i = 1; i < colliders.Length; i++)
            {
                newBounds.Encapsulate(colliders[i].bounds);
            }

            // Optional buffer to prevent tight packing
            newBounds.Expand(2.0f);

            bool overlaps = false;
            foreach (Bounds b in obstacleBounds) {
                if (b.Intersects(newBounds)) {
                    overlaps = true;
                    break;
                }
            }
            
            foreach (Bounds b in shroomBounds) {
                if (b.Intersects(newBounds)) {
                    overlaps = true;
                    break;
                }
            }

            if (overlaps)
            {
                Destroy(obstacle);
                continue;
            }

            //newBounds.Expand(-1.0f);
            bounds.Add(newBounds);

        }
    }
}