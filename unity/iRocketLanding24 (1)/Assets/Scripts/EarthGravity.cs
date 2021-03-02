using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthGravity : MonoBehaviour
{
    //set default gravity strength
    public float earthGravity = 9.81f;
    public float moonGravity = 3f;
    public Transform earth;
    public Transform moon;

    public float moonEarthDist;
    Rigidbody _rBody;
    
    
    // Start is called before the first frame update
    void Start()
    {
        moonEarthDist = Vector3.Distance(earth.position, moon.position);
        _rBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
        var position = this.transform.position;
        var earthPos = earth.position;
        var moonPos = moon.position;
        
        var rocketEarthDist = Vector3.Distance(position, earthPos);
        var rocketMoonDist = Vector3.Distance(position, moonPos);
        

        if (rocketEarthDist < moonEarthDist * 0.6)
        {
            _rBody.AddForce((earthPos - position).normalized * earthGravity);
        }
        
        if (rocketMoonDist < moonEarthDist * 0.4)
        {
            _rBody.AddForce((moonPos - position).normalized * moonGravity);
        }
    }
}
