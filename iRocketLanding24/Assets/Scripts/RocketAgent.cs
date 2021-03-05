using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using System;
using System.Diagnostics;
using System.Globalization;
using UnityEditor;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class RocketAgent : Agent
{

    private Rigidbody _rBody;
    private float _earthRadius;
    private bool _startedFromEarth;
    public bool enableUi = false;
    public Font font;

    private float _maxSteps = 10000;
    private float _totalSteps = 1;
    private float currentAngularVelocity;
    private float currentMagnitude;
    
    // Start is called before the first frame update
    public void Start()
    {
        _earthRadius = earth.GetComponent<SphereCollider>().radius * earth.localScale.x;
        //Debug.Log("RADIUS: " + _earthRadius);
        _rBody = GetComponent<Rigidbody>();
    }

    public float thrustMultiplier = 200;
    public float rotationMultiplier = 30;

    public Transform moon;
    public Transform earth;

    public float earthMoonDistance = 1000;

    public float oldDistanceToMoon = 999999999999;

    public override void OnActionReceived(float[] actionBuffers)
    {

        //Debug.Log(actionBuffers);
        var space0 = Mathf.Clamp(actionBuffers[0], 0, 1f);
        var space1 = Mathf.Clamp(actionBuffers[1], 0, 1f);
        var space2 = Mathf.Clamp(actionBuffers[2], 0, 1f);
        
        this.currentAngularVelocity = Math.Abs(_rBody.angularVelocity.z);
        this.currentMagnitude = _rBody.velocity.magnitude;

        _rBody.AddForce(transform.up * thrustMultiplier * space0);
        _rBody.AddTorque(Vector3.forward * rotationMultiplier * space1);
        _rBody.AddTorque(Vector3.forward * -rotationMultiplier * space2);

        //Debug.Log("angleVel: " + angleVelocity + " XYVel:  " + xyVelocity);
        
        // --
        // Rewards
        // ---
        
        // Reward agent after leaving earth (once)
        var distanceToEarth = Vector3.Distance(transform.localPosition, earth.localPosition);
        if (!_startedFromEarth && distanceToEarth > _earthRadius * 1.04f)
        {
            AddReward(0.1f);
            _startedFromEarth = true;
        }

        // Otherwise the rockets chils on earth sometimes...
        if (_totalSteps >= 200 && !_startedFromEarth)
        {
            AddReward(-1f);
            EndEpisode();
        }

        var distanceToMoon = Vector3.Distance(transform.localPosition, moon.localPosition);
        if (oldDistanceToMoon == 999999999999)
        {
            oldDistanceToMoon = distanceToMoon;
        }

        // Reward for getting closer to moon
        if (oldDistanceToMoon > distanceToMoon)
        {
            AddReward(0.0002f); //TODO SetReward
        }
        
        // Punish agent for spinning
        if (this.currentAngularVelocity > 3f)
        {
            // Debug.Log("angular velocity: " + this.currentAngularVelocity);
            AddReward(-(this.currentAngularVelocity / 1000));
        }


        // Punish for taking many steps
        AddReward(-(1.0f/_maxSteps));
        _totalSteps++;
        if (_totalSteps >= _maxSteps)
        {
            AddReward(-1f);
            EndEpisode();
        }
    }


    protected void LateUpdate() {
        var transform1 = transform;
        transform1.localEulerAngles = new Vector3(0, 0, transform1.localEulerAngles.z);
        var localPosition = transform1.localPosition;
        localPosition = new Vector3(localPosition.x, localPosition.y, 0);
        transform1.localPosition = localPosition;
    }

    protected void OnDrawGizmos()
    {
        if (!enableUi) return;
        GUI.color = Color.green;
        GUI.skin.font = font;
        var position = transform.localPosition;
        var rootY = -30;
        Handles.Label(position + new Vector3(20,0,0),"STEPS(" + _totalSteps.ToString(CultureInfo.CurrentCulture)+")");
        Handles.Label(position+ new Vector3(20,rootY * 1,0),"MAGNITUDE(" + currentMagnitude.ToString(CultureInfo.CurrentCulture)+")");
        Handles.Label(position+ new Vector3(20,rootY * 2,0),"VELOCITY(" + currentAngularVelocity.ToString(CultureInfo.CurrentCulture)+")");
        Handles.Label(position+ new Vector3(20,rootY * 3,0),"REWARD(" + GetCumulativeReward().ToString(CultureInfo.CurrentCulture)+")");
    }

    void OnCollisionEnter(Collision other)
    {
        // Never go back
        if (_startedFromEarth && other.gameObject.CompareTag("earth"))
        {
            AddReward(-1);
            EndEpisode();
        }

        if (other.gameObject.CompareTag("moon"))
        {
            var trans = this.transform;
            // Direction moon to agent
            var directionMoonToAgent = (trans.localPosition - moon.localPosition).normalized;
        
            // Angle of rocket to moon center
            var angle = Math.Abs(Vector3.Angle(directionMoonToAgent, trans.up));

            // Debug.Log("angle: " + angle + " angleVel: " + this.currentAngularVelocity + " XYVel:  " + this.currentMagnitude);

            // Angle Squared / 1000 -> bigger than zero for angles bigger than ca 62 degrees
            var bonusAngle = Math.Max(0, 4 - angle * angle / 1000);
            var bonusAngleVelocity = Math.Max(0, 4 - this.currentAngularVelocity * 1.5f); // TODO balance
            var bonusXYVelocity = Math.Max(0, 5 - this.currentMagnitude / 50); // TODO balance 
            // Only set reward if angle is between 0 and about 62
            if (bonusAngle > 0)
            {
                // Debug.Log("bonusAngle: " + bonusAngle + " bonusVelocity: " + bonusAngleVelocity + " bonusXYVelocity: " + bonusXYVelocity);
                AddReward(2+bonusAngle+bonusAngleVelocity+bonusXYVelocity);
            }
            // Otherwise only add reward and keep rewards earned before
            else
            {
                AddReward(1);
            }
            //End Episode in any case
            EndEpisode();
        }

    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("bounds"))
        {
            AddReward(-3);
            EndEpisode();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        var transform1 = transform;
        if (transform1 == null) return;
        var localPosition = transform1.localPosition;
        var up = transform1.up;

        //Debug.Log("local: "+localPosition+" global: "+position);

        
        // Distance to moon / earth 
        sensor.AddObservation(Vector3.Distance(localPosition, moon.localPosition)); // 1 obs
        sensor.AddObservation(Vector3.Distance(localPosition, earth.localPosition)); // 1 obs
        
        // Direction moon to agent
        var directionMoonToAgent = (localPosition - moon.localPosition).normalized;
        sensor.AddObservation(directionMoonToAgent); // 3 obs
        
        // Direction earth to agent
        var directionEarthToAgent = (localPosition - earth.localPosition).normalized;
        sensor.AddObservation(directionEarthToAgent); // 3 obs
        
        // Angle of rocket to moon center
        var angle = Vector3.Angle(directionMoonToAgent, up);
        sensor.AddObservation(angle); // 1 obs
        
        // Angle of rocket to earth center
        var angle2 = Vector3.Angle(directionEarthToAgent, up);
        sensor.AddObservation(angle2); // 1 obs
        
        // Agent position / z angle
        sensor.AddObservation(localPosition); // 3 obs
        sensor.AddObservation(transform1.eulerAngles.z); // 1 obs
        
        // Agent body observations
        sensor.AddObservation(_rBody.angularVelocity.z); // 1 obs
        sensor.AddObservation(_rBody.velocity.x); // 1 obs
        sensor.AddObservation(_rBody.velocity.y); // 1 obs
        // Debug.Log(_rBody.angularVelocity.z);

    }

    public override void OnEpisodeBegin()
    {
        _startedFromEarth = false;
        _totalSteps = 1;
        oldDistanceToMoon = 999999999999;
        this.currentAngularVelocity = 0;
        this.currentMagnitude = 0;

        var trans = this.transform;
        var earthPos = earth.localPosition;
        //Debug.Log("pos: "+earthPos + "locPos: "+earth.localPosition);
        //Place Rocket
        var randomPointOnEarth2D = Random.insideUnitCircle.normalized * _earthRadius;
        var randomPointOnEarth3D = new Vector3(randomPointOnEarth2D.x, randomPointOnEarth2D.y, 0);
        trans.localPosition = randomPointOnEarth3D * 1.02f;

        // Reset Rocket Speeds
        _rBody.angularVelocity = Vector3.zero;
        _rBody.velocity = Vector3.zero;

        // Set Rocket rotation
        var v3 = trans.localPosition - earthPos;
        trans.rotation = Quaternion.FromToRotation(trans.up, v3) * this.transform.rotation;

        // Place moon
        var randomMoonPos2D = Random.insideUnitCircle.normalized * earthMoonDistance;
        var randomMoonPos3D = new Vector3(randomMoonPos2D.x, randomMoonPos2D.y, 0);
        moon.localPosition = randomMoonPos3D;
        // TODO add rotation to moon OR let moon move around earth in orbit

        var moonEarthDist = Vector3.Distance(earthPos, moon.localPosition);
        // Debug.Log("rotation: " + trans.rotation);
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