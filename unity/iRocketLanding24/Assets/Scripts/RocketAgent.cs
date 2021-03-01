using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using System;

public class RocketAgent : Agent
{
    Rigidbody rBody;
    
    // Start is called before the first frame update
    void Start () {
        rBody = GetComponent<Rigidbody>();
    }
    
    public float thrustMultipler = 20;
    public float rotationMultiplier = 3;
    public Transform Target;
    public float oldDistanceX = 999;
    public override void OnActionReceived(float[] actionBuffers)
    {
        //Debug.Log(actionBuffers);
        if (actionBuffers[0] == 1)
        {
            rBody.AddForce(transform.up * thrustMultipler);
        }

        if (actionBuffers[1] == 1)
        {
            //rBody.AddTorque(Vector3.down * rotationMultiplier);
            //rBody.AddTorque(Vector3.left * rotationMultiplier);
            rBody.AddTorque(Vector3.forward * rotationMultiplier);
        }
        
        if (actionBuffers[2] == 1)
        {
            rBody.AddTorque(Vector3.forward * -rotationMultiplier);
        }
        
       // Rewards
       float distanceToTargetX = Math.Abs(this.transform.localPosition.x - Target.localPosition.x);

       if (oldDistanceX == 999)
       {
           oldDistanceX = distanceToTargetX;
       }

       
       float distanceDeltaX = oldDistanceX - distanceToTargetX;

       if (Math.Abs(distanceDeltaX) <= 0.001)
       {
           //Debug.Log("didnt move, " + distanceDeltaX);
           SetReward(-0.001f);
       }
       
       if (oldDistanceX > distanceToTargetX)
       {
            SetReward(0.1f);
       }
       else
       {
           SetReward(-0.001f);
       }

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
       }
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("TargetPad"))
        {
            SetReward(10);
            EndEpisode();
        }
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        // Target and Agent positions
        sensor.AddObservation(Target.localPosition);
        sensor.AddObservation(this.transform.localPosition);
        sensor.AddObservation(this.transform.eulerAngles.z);

        // Agent velocity
        sensor.AddObservation(rBody.angularVelocity.z);
        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.y);
    }
    
    public override void OnEpisodeBegin()
    {
        this.rBody.angularVelocity = Vector3.zero;
        this.rBody.velocity = Vector3.zero;
        this.transform.localPosition = new Vector3( 7, 10, 5);
        this.transform.rotation = Quaternion.identity;
    }
    
    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = 0;
        actionsOut[1] = 0;
        actionsOut[2] = 0;
            
        if (Input.GetKey(KeyCode.Space))
        {
            actionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            actionsOut[1] = 1;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            actionsOut[2] = 1;
        }
    }
}
