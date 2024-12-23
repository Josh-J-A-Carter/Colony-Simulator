using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {
    // Start is called before the first frame update
    void Start() {
        Material mat = GetComponent<Renderer>().material;

        mat.EnableKeyword("_OUTLINE_ON");
        mat.EnableKeyword("_REDTINT_ON");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
