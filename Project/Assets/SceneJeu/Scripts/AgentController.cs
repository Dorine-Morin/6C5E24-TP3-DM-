using System.Collections;
using System.Collections.Generic;
using System.Xml.Xsl;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class AgentController : Agent
{
    [SerializeField] private Transform boutonTransform;
    [SerializeField] private Transform porteTransform;
    [SerializeField] private GameObject sol;
    private MeshRenderer solRenderer;

    private void Start()
    {
        solRenderer = sol.GetComponent<MeshRenderer>();
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveY = actions.ContinuousActions[1];
        float speed = 10;
        transform.Translate(new Vector3(moveX, 0, moveY) * Time.deltaTime * speed);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(boutonTransform.localPosition);
        sensor.AddObservation(porteTransform.localPosition);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Porte"))
        {
            Debug.Log("Porte");
            AddReward(2f);
        } 
        else if (other.CompareTag("Bouton"))
        {
            Debug.Log("Mur");
            AddReward(1f);
        }
        else if (other.CompareTag("Mur"))
        {
            
            solRenderer.material.color = Color.red;
            Debug.Log("Mur");
            AddReward(-1f);
        }      
    }

    public override void OnEpisodeBegin()
    {
        solRenderer.material.color = Color.black;
        Vector3 move = new Vector3(Random.Range(-3f, 3f), 0.34f, Random.Range(-3f, -2f));
        transform.Translate(move);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> contActions = actionsOut.ContinuousActions;
        contActions[0] = Input.GetAxisRaw("Horizontal");
        contActions[1] = Input.GetAxisRaw("Vertical");
    }
}
