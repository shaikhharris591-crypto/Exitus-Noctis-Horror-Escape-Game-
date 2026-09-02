using UnityEngine;


public class KeyBehaviour : MonoBehaviour, IItemFunctionality
{
   // public LayerMask doorMask;
    public float offset=0;
    public float radius=1f;
    string mainDoorlayer = "mainDoor";
    
    GameObject target;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        if (radius == 0) radius = 1f;
       
    }

    // Update is called once per frame
    void Update()
    {

        

       
            Vector3 pos = new Vector3(transform.position.x, transform.position.y + offset, transform.position.z);

            // Get all colliders in range
            Collider[] collidersInRange = Physics.OverlapSphere(pos, radius, gameObject.layer);

            if (collidersInRange.Length > 0 && Input.GetKeyDown(PlayerMovement.InputKeys.functionKey))
            {
                // Example: just take the first one
                target = collidersInRange[0].gameObject;
                Debug.Log("Hit object: " + target.name);

               if (LayerMask.LayerToName(gameObject.layer) == mainDoorlayer && Input.GetKeyDown(PlayerMovement.InputKeys.functionKey))
               {
                GameManager.Instance.OnKeyCollected();
               }
               else Unlock();
            }
        
    }

    public void Unlock()
    {
        target.GetComponent<DoorControl>().UnlockDoor(true);
    }

    public void Activate() 
    {
        enabled = true;
    }

    public void Deactivate() 
    {
        enabled = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(
            new Vector3(
                transform.position.x,
                transform.position.y - offset,
                transform.position.z),
            radius);
    }
}
