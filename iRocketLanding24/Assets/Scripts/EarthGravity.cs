﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Globalization;
using UnityEngine.UI;

// using static UnityEditor.Handles;

public class EarthGravity : MonoBehaviour
{
    //set default gravity strength
    public float earthGravity = 300;
    public float moonGravity = 250;
    public bool enableUi;
    public Transform earth;
    public Transform moon;
    public Text txtEarthDst;
    public Text txtMoonDst;

    // private float _moonEarthDist;
    private Vector3 _gravityToEarth;
    private Vector3 _gravityToMoon;
    private Rigidbody _rBody;
    
    
    // Start is called before the first frame update
    private void Start()
    {
        // _moonEarthDist = Vector3.Distance(earth.localPosition, moon.localPosition);
        _rBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {

        var position = this.transform.localPosition;
        var earthPos = earth.localPosition;
        var moonPos = moon.localPosition;
        
        var rocketEarthDist = Vector3.Distance(position, earthPos);
        var rocketMoonDist = Vector3.Distance(position, moonPos);

        _gravityToEarth = (earthPos - position).normalized * (float) (earthGravity / Math.Sqrt(rocketEarthDist));
        _gravityToMoon = (moonPos - position).normalized * (float) (moonGravity / Math.Sqrt(rocketMoonDist));
        _rBody.AddForce(_gravityToEarth);
        _rBody.AddForce(_gravityToMoon);
        
        if (!enableUi) return;
        txtEarthDst.text = _gravityToEarth.magnitude.ToString(CultureInfo.CurrentCulture);
        txtMoonDst.text = _gravityToMoon.magnitude.ToString(CultureInfo.CurrentCulture);
        var position1 = transform.position;
        Debug.DrawLine(position1, earth.transform.position, Color.green);
        Debug.DrawLine(position1, moon.transform.position, Color.green);
    }
    

    private void OnDrawGizmos()
    {
        if (!enableUi) return;
        // GUI.color = Color.green;
        // var position = transform.position;
        // Handles.Label(position - (position - 
        //                           earth.transform.position)/2, "g("+_gravityToEarth.magnitude.ToString(CultureInfo.CurrentCulture)+")");
        // Handles.Label(position - (position - 
        //                           moon.transform.position)/2, "g("+_gravityToMoon.magnitude.ToString(CultureInfo.CurrentCulture)+")");
    }
}
