using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour, IItem
{
    private void OnTriggerEnter(Collider other)
    {
        IItem item = other.GetComponent<IItem>();

        //if (item != null)
        //    item.Use();
    }

    public void Use(GameObject target)
    {

    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
