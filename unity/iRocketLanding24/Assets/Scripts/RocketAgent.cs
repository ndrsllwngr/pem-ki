using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using System;

public class RocketAgent : Agent
{
    // Start is called before the first frame update
    Rigidbody rBody;
    void Start () {
        rBody = GetComponent<Rigidbody>();
    }

    public float forceMultiplier = 10;
    public float thrustMultipler = 30;
    public float rotationMultiplier = 3;
    public Transform Target;
    public float oldDistance = 999;
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
        
       //// Rewards
       float distanceToTarget = Math.Abs(this.transform.localPosition.x - Target.localPosition.x);

       if (oldDistance == 999)
       {
           oldDistance = distanceToTarget;
       }

       float distanceDelta = oldDistance - distanceToTarget;

       float angle = this.transform.eulerAngles.z;

       if (angle < 330 && angle > 30)
       {
           float diff = 180 - Math.Abs(180 - angle);
           SetReward(-diff/90);
       }
       
       // Debug.Log("angles: " + this.transform.eulerAngles + " dist: " + distanceDelta);
       
       SetReward(distanceDelta);

       oldDistance = distanceToTarget;
       
       // Fell off platform
       if (this.transform.localPosition.y < -5)
       {
           SetReward(-1.0f);
           EndEpisode();
       }
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("TargetPad"))
        {
            SetReward(4);
            EndEpisode();
        }
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        // Target and Agent positions
        sensor.AddObservation(Target.localPosition);
        sensor.AddObservation(this.transform.localPosition);

        // Agent velocity
        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.y);
    }
    
    
    public override void OnEpisodeBegin()
    {
       // If the Agent fell, zero its momentum
        if (this.transform.localPosition.y < -5)
        {
            this.rBody.angularVelocity = Vector3.zero;
            this.rBody.velocity = Vector3.zero;
            this.transform.localPosition = new Vector3( 7, 10, 5);
            this.transform.rotation = Quaternion.identity;
        }

        // Move the target to a new spot
        //Target.localPosition = new Vector3(Random.value * 8 - 4,
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
