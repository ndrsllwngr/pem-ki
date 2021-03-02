using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using System;
using System.Diagnostics;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class RocketAgent : Agent
{

    private Rigidbody _rBody;
    private float _earthRadius;
    private bool _startedFromEarth;

    private float _maxSteps = 3000;
    private float _totalSteps = 1;
    
    // Start is called before the first frame update
    public void Start()
    {
        _earthRadius = earth.GetComponent<SphereCollider>().radius * earth.localScale.x;
        // Debug.Log("RADIUS: " + _earthRadius);
        _rBody = GetComponent<Rigidbody>();
    }

    public float thrustMultiplier = 100;
    public float rotationMultiplier = 30;


    public Transform moon;
    public Transform earth;

    public float earthMoonDistance = 500;

    public float oldDistanceToMoon = 999;

    public override void OnActionReceived(float[] actionBuffers)
    {

        //Debug.Log(actionBuffers);
        var space0 = Mathf.Clamp(actionBuffers[0], 0, 1f);
        var space1 = Mathf.Clamp(actionBuffers[1], 0, 1f);
        var space2 = Mathf.Clamp(actionBuffers[2], 0, 1f);
        
        _rBody.AddForce(transform.up * thrustMultiplier * space0);
        _rBody.AddTorque(Vector3.forward * rotationMultiplier * space1);
        _rBody.AddTorque(Vector3.forward * -rotationMultiplier * space2);

        // ---
        // Rewards
        // ---
        
        // Reward agent after leaving earth (once)
        var distanceToEarth = Vector3.Distance(transform.localPosition, earth.localPosition);
        if (!_startedFromEarth && distanceToEarth > _earthRadius + 20)
        {
            AddReward(0.1f);
            _startedFromEarth = true;
        }

        var distanceToMoon = Vector3.Distance(transform.localPosition, moon.localPosition);
        if (oldDistanceToMoon == 999)
        {
            oldDistanceToMoon = distanceToMoon;
        }

        // Reward for getting closer to moon / punish if distance increases
        if (oldDistanceToMoon > distanceToMoon)
        {
            AddReward(0.0001f);
        }
        else
        {
            AddReward(-0.00005f);
        }
        
        // Punish agent for spinning
        var angularVelocity = Math.Abs(_rBody.angularVelocity.z);
        // Debug.Log("angular velocity: " + angularVelocity);
        if (angularVelocity > 2)
        {
            AddReward(-(angularVelocity / 1000));
        }


        // Punish for taking many steps
        AddReward(-(0.5f/_maxSteps));
        _totalSteps++;
        if (_totalSteps >= _maxSteps)
        {
            EndEpisode();
        }
        /*
        float angle = this.transform.eulerAngles.z;
        if (angle < 320 && angle > 40)
        {
            float diff = 180 - Math.Abs(180 - angle);
            SetReward(-diff/900);
        }
        else if (angle < 40)
        {
            SetReward((40 - angle) / 1000);
        }
        else if (angle > 320)
        {
            SetReward(Math.Abs(320 - angle) / 1000);
        }
        
        if (angle < 280 && angle > 80)
        {
            //Debug.Log(angle);
            SetReward(-1);
            EndEpisode();
        }
 
        float angularVelocity = rBody.angularVelocity.z;
 
        //Debug.Log("angular velocity: " + angularVelocity);
 
        if (Math.Abs(angularVelocity) > 0.3)
        {
            SetReward(-Math.Abs(angularVelocity / 5));
        }
 
        // Debug.Log("angles: " + this.transform.eulerAngles + " dist: " + distanceDelta);
 
        oldDistanceX = distanceToTargetX;
        
        // to far away
        if (distanceToTargetX > 200)
        {
            SetReward(-1.0f);
            EndEpisode();
        }
        
        // Fell off platform
        if (this.transform.localPosition.y < -50.0f)
        {
            SetReward(-1.0f);
            EndEpisode();
        }*/
    }
    

    void OnCollisionEnter(Collision other)
    {
        // Never go back
        if (_startedFromEarth && other.gameObject.CompareTag("earth"))
        {
            SetReward(-1);
            EndEpisode();
        }

        if (other.gameObject.CompareTag("moon"))
        {
            var trans = this.transform;
            // Direction moon to agent
            var directionMoonToAgent = (trans.position - moon.position).normalized;
        
            // Angle of rocket to moon center
            var angle = Math.Abs(Vector3.Angle(directionMoonToAgent, trans.up));

            Debug.Log("Landed on Moon with "+angle+"°");
            
            var bonus = Math.Max(0, 4 - angle * angle / 1000);
            SetReward(1 + bonus);
            EndEpisode();
        }

    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("bounds"))
        {
            SetReward(-1);
            EndEpisode();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        var transform1 = transform;
        if (transform1 == null) return;
        var localPosition = transform1.localPosition;
        
        // Distance to moon / earth 
        sensor.AddObservation(Vector3.Distance(localPosition, moon.localPosition)); // 1 obs
        sensor.AddObservation(Vector3.Distance(localPosition, earth.localPosition)); // 1 obs
        
        // Direction moon to agent
        var directionMoonToAgent = (transform1.position - moon.position).normalized;
        sensor.AddObservation(directionMoonToAgent); // 3 obs
        
        // Angle of rocket to moon center
        var angle = Vector3.Angle(directionMoonToAgent, transform1.up);
        sensor.AddObservation(angle); // 1 obs
        
        // Agent position / z angle
        sensor.AddObservation(localPosition); // 3 obs
        sensor.AddObservation(transform1.eulerAngles.z); // 1 obs
        
        // Agent body observations
        sensor.AddObservation(_rBody.angularVelocity.z); // 1 obs
        sensor.AddObservation(_rBody.velocity.x); // 1 obs
        sensor.AddObservation(_rBody.velocity.y); // 1 obs

    }

    public override void OnEpisodeBegin()
    {
        _startedFromEarth = false;
        _totalSteps = 1;

        var trans = this.transform;
        var earthPos = earth.position;
        //Place Rocket
        var randomPointOnEarth2D = Random.insideUnitCircle.normalized * _earthRadius;
        var randomPointOnEarth3D = new Vector3(randomPointOnEarth2D.x, randomPointOnEarth2D.y, 0);
        trans.localPosition = randomPointOnEarth3D;

        // Reset Rocket Speeds
        _rBody.angularVelocity = Vector3.zero;
        _rBody.velocity = Vector3.zero;

        // Set Rocket rotation
        var v3 = trans.position - earthPos;
        trans.rotation = Quaternion.FromToRotation(trans.up, v3) * this.transform.rotation;

        // Place moon
        var randomMoonPos2D = Random.insideUnitCircle.normalized * earthMoonDistance;
        var randomMoonPos3D = new Vector3(randomMoonPos2D.x, randomMoonPos2D.y, 0);
        moon.localPosition = randomMoonPos3D;
        // TODO add rotation to moon OR let moon move around earth in orbit

        var moonEarthDist = Vector3.Distance(earthPos, moon.position);
        // Debug.Log("Moon-Earth dist: " + moonEarthDist);
    }

    public override void Heuristic(float[] continuousActionsOut)
    {
        float space0 = 0;
        float space1 = 0;
        float space2 = 0;

        if (Input.GetKey(KeyCode.Space))
        {
            space0 = 1;
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            space1 = 1;
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            space2 = 1;
        }
        
        continuousActionsOut[0] = space0;
        continuousActionsOut[1] = space1;
        continuousActionsOut[2] = space2;
    }
}