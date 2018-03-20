using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Example : MonoBehaviour {


    public int life = 10;

    [Toolbar]
    public void Foo(RaycastHit hit)
     {
            Debug.Log("Foo");
     }

    [Toolbar]
    public void Bar(RaycastHit hit)
    {
            Debug.Log("Bar");
    }

}
