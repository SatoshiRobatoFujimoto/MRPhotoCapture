using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveSelfManager : MonoBehaviour {

    public void Remove()
    {
        Destroy(gameObject);
    }

}
