using UnityEngine;

public class Example : MonoBehaviour
{

    public GameObject BlocktoSpawn;
    public Transform NextBlockSpawnLocation;
    internal GameObject PreviousBlock;

    //Upon collision with another GameObject, this GameObject will reverse direction
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entered " + other.gameObject.name ,other.gameObject );
        if(other.gameObject.tag == "Player"){

            Debug.Log("Catch Player " + other.gameObject.name, other.gameObject  );
            Instantiate(BlocktoSpawn, NextBlockSpawnLocation.position, NextBlockSpawnLocation.rotation);

        }    
    }

}
