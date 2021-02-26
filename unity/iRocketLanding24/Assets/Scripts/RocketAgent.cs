using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class RocketAgent : Agent
{
    // Start is called before the first frame update
    Rigidbody rBody;
    void Start () {
        rBody = GetComponent<Rigidbody>();
    }

    public float forceMultiplier = 10; 
    
    

    
    public override void OnActionReceived(float[] actionBuffers)
{
    // Actions, size = 2
    Vector3 controlSignal = Vector3.zero;
    controlSignal.x = actionBuffers[0];
    controlSignal.y = actionBuffers[1];
    rBody.AddForce(controlSignal * forceMultiplier);

   //// Rewards
   //float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);

   //// Reached target
   //if (distanceToTarget < 1.42f)
   //{
   //    SetReward(1.0f);
   //    EndEpisode();
   //}

   //// Fell off platform
   //else if (this.transform.localPosition.y < 0)
   //{
   //    EndEpisode();
   //}
}
    
    public override void CollectObservations(VectorSensor sensor)
{
    // Target and Agent positions
    // sensor.AddObservation(Target.localPosition);
    sensor.AddObservation(this.transform.localPosition);

    // Agent velocity
    sensor.AddObservation(rBody.velocity.x);
    sensor.AddObservation(rBody.velocity.y);
}
    
    
    public override void OnEpisodeBegin()
    {
       // If the Agent fell, zero its momentum
        if (this.transform.localPosition.y < 0)
        {
            this.rBody.angularVelocity = Vector3.zero;
            this.rBody.velocity = Vector3.zero;
            this.transform.localPosition = new Vector3( 0, 0.5f, 0);
        }

        // Move the target to a new spot
        //Target.localPosition = new Vector3(Random.value * 8 - 4,
    }
    
    public override void Heuristic(float[] actionsOut)
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Translate(0.1f, 0f, 0f);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.Translate(-0.1f, 0f, 0f);
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.Translate(0.0f, 0.1f, 0f);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            transform.Translate(0.0f, -0.1f, 0f);
        }
    }
}
