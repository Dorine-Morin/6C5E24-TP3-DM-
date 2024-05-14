using System.Collections;
using System.Collections.Generic;
using System.Xml.Xsl;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class AgentController : Agent
{
    [SerializeField] private GameObject bouton;
    [SerializeField] private List<GameObject> portes;
    [SerializeField] private GameObject sol;

    private Transform boutonTransform;
    private Transform porte1Transform;
    private Transform porte2Transform;

    private Material boutonMaterial;
    private Material porte1Material;
    private Material porte2Material;
    private Material solMaterial;

    private bool porte1Ouverte = false;
    private bool porte2Ouverte = false;
    private bool boutonActif = true;

    private GameObject porteToReach;
    private Material porteToReachMat;
    private int indexPorteToReach;


    private void Start()
    {
        boutonMaterial = bouton.GetComponent<MeshRenderer>().material;
        porte1Material = portes[0].GetComponent<MeshRenderer>().material;
        porte2Material = portes[1].GetComponent<MeshRenderer>().material;
        solMaterial = sol.GetComponent<MeshRenderer>().material;        

        boutonTransform = bouton.GetComponent<Transform>();
        porte1Transform = portes[0].GetComponent<Transform>();
        porte2Transform = portes[1].GetComponent<Transform>();
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveY = actions.ContinuousActions[1];
        float speed = 5;
        transform.Translate(new Vector3(moveX, 0, moveY) * Time.deltaTime * speed);
        
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(boutonTransform.localPosition);
        sensor.AddObservation(porte1Transform.localPosition);
        sensor.AddObservation(porte2Transform.localPosition);
        sensor.AddObservation(boutonActif ? 1 : 0);
        sensor.AddObservation(porte1Ouverte ? 1 : 0);
        sensor.AddObservation(porte2Ouverte ? 1 : 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Porte1"))
        {
            if (porte1Ouverte)
            {
                GameSuccess();
            } else
            {
                GameOver("Porte");
            }
            
        } 
        else if (other.CompareTag("Porte2"))
        {
            if (porte2Ouverte)
            {
                GameSuccess();
            }
            else
            {
                GameOver("Porte");
            }

        }
        else if (other.CompareTag("Bouton"))
        {
            BoutonReached();
        }
        else if (other.CompareTag("Mur"))
        {
            GameOver("Mur");
        }      
    }

    private void GameOver(string collider)
    {
        solMaterial.color = Color.red;
        float reward = -1f;

        if (collider == "Mur")
        {
            reward = -3f;
        }

        AddReward(reward);
        EndEpisode();
    }

    private void GameSuccess()
    {
        solMaterial.color = Color.green;

        AddReward(1000f);
        EndEpisode();
    }

    private void BoutonReached()
    {
       
                    

            if (indexPorteToReach == 0)
            {
                if (porte1Ouverte)
                {
                SetReward(-50f);
                porte1Ouverte = false;
                    porte1Material.color = Color.black;
                } else
                {
                SetReward(100f);
                porte1Ouverte = true;
                    porte1Material.color = Color.yellow;
                }
                
                
            }
            else
            {
            if (porte2Ouverte)
            {
                SetReward(-50f);
                porte2Ouverte = false;
                porte2Material.color = Color.black;
            }
            else
            {
                SetReward(100f);
                porte2Ouverte = true;
                porte2Material.color = Color.yellow;
            }
        }

                    
    }

    public override void OnEpisodeBegin()
    {
        indexPorteToReach = Random.Range(0, 2);
        porteToReach = portes[indexPorteToReach];
        porteToReachMat = porteToReach.GetComponent<MeshRenderer>().material;

        boutonMaterial.color = Color.yellow;
        porte1Material.color = Color.black;
        porte2Material.color = Color.black;

        porte1Ouverte = false;
        porte2Ouverte = false;

        transform.localPosition = new Vector3(1, 0.34f, -3f);
        //Vector3 move = new Vector3(Random.Range(-2f, 3f), 0, Random.Range(-3f, -2f));
        //transform.Translate(move);

        porte1Transform.localPosition = new Vector3(-1.5f, 1f, 3.5f);

        boutonTransform.localPosition = new Vector3(-0.5f, 0.2f, -2f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> contActions = actionsOut.ContinuousActions;
        contActions[0] = Input.GetAxisRaw("Horizontal");
        contActions[1] = Input.GetAxisRaw("Vertical");
    }
}
