using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class AgentController : Agent
{
    [SerializeField] private GameObject sol;
    private Material solMaterial;

    [SerializeField] private GameObject interrupteur;
    private Transform interrupteurTransform;
    private Material interrupteurMaterial;
    private bool interrupteurOff;

    [SerializeField] private List<GameObject> portes;
    private Transform porte1Transform;
    private Transform porte2Transform;
   
    private GameObject porteToReach;
    private Material porteToReachMat;
    private int indexPorteToReach;

    private Transform goalTransform;

    private Color colorOn = Color.yellow;
    private Color colorOff = Color.black;
    private Color colorMurSansPortes = Color.red;
    private Color colorMurAvecPortes = Color.blue;
    private Color colorPorteFerme = Color.magenta;
    private Color colorPorteOuverte = Color.green;

    private string porteOuverteTag = "PorteOuverte";
    private string porteFermeTag = "PorteFerme";
    private string interrupteurTag = "Interrupteur";
    private string murSansPortesTag = "Mur";
    private string murAvecPortesTag = "MurPorte";

    private float murAvecPorteMinX = -2f;
    private float murAvecPorteMaxX = 1.8f;
    private float murAvecPortePosZ = 2.65f;
    private float petitePortePosY = 0.5f;
    private float grandePortePosY = 0.9f;
    private float solMinX = -2f;
    private float solMaxX = 2f;
    private float interrupteurminZ = -1f;
    private float interrupteurmaxZ = 1f;
    private float interrupteurPosY = 0.2f;
    private float agentMinZ = -2f;
    private float agentMaxZ = -1f;
    private float agentPosY = 0.5f;

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
        sensor.AddObservation(goalTransform.localPosition);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(porteOuverteTag))
        {
            GameSuccess();
        }
        else if (other.CompareTag(porteFermeTag))
        {
            GameOver(porteFermeTag);   
        }
        else if (other.CompareTag(interrupteurTag))
        {
            BoutonReached();
        }
        else if (other.CompareTag(murSansPortesTag))
        {
            GameOver(murSansPortesTag);
        }
        else if (other.CompareTag(murAvecPortesTag))
        {
            GameOver(murAvecPortesTag);
        }
    }

    private void GameOver(string collider)
    {
        if (collider == murSansPortesTag)
        {
            AddReward(-1000f);
            solMaterial.color = colorMurSansPortes;
        }
        else if (collider == murAvecPortesTag)
        {
            AddReward(-1000f);
            solMaterial.color = colorMurAvecPortes;
        }
        else if (collider == porteFermeTag)
        {
            AddReward(-100f);
            solMaterial.color = colorPorteFerme;
        }

        EndEpisode();
    }

    private void GameSuccess()
    {
        solMaterial.color = colorPorteOuverte;

        AddReward(100000f);
        EndEpisode();
    }

    private void BoutonReached()
    {
        if (interrupteurOff)
        {
            
            AddReward(1000f);
            interrupteurMaterial.color = colorOff;
            SetPorteToReach();
            goalTransform = porteToReach.transform;

        } else
        {
            interrupteurMaterial.color = colorOn;
            porteToReach.gameObject.tag = porteFermeTag;
            porteToReachMat.color = colorOff;
            goalTransform = interrupteurTransform;
        }

        interrupteurOff = !interrupteurOff;
    }

    private void SetPorteToReach()
    {
        indexPorteToReach = Random.Range(0, portes.Count);
        porteToReach = portes[indexPorteToReach];
        porteToReachMat = porteToReach.GetComponent<MeshRenderer>().material;
        porteToReach.gameObject.tag = porteOuverteTag;
        porteToReachMat.color = colorOn;
    }

    public override void OnEpisodeBegin()
    {
        // Initialisation de l'interrupteur
        interrupteurMaterial = interrupteur.GetComponent<MeshRenderer>().material;
        interrupteurTransform = interrupteur.transform;
        interrupteurMaterial.color = colorOn;
        interrupteurOff = true;
        goalTransform = interrupteurTransform;

        // Initialisation des portes
        foreach (var porte in portes)
        {
            porte.GetComponent<MeshRenderer>().material.color = colorOff;
            porte.gameObject.tag = porteFermeTag;
        }   

        // Initialisation du sol
        solMaterial = sol.GetComponent<MeshRenderer>().material;

        // Position initiale de l'agent et de l'interrupteur
        transform.localPosition = GenerateAgentPosition();
        interrupteurTransform.localPosition = GenerateInterrupteurPosition();

        // Position initiale des portes
        Vector3[] doorPositions = GenerateDoorsPositions();
        portes[0].transform.localPosition = doorPositions[0];
        portes[1].transform.localPosition = doorPositions[1];
    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> contActions = actionsOut.ContinuousActions;
        contActions[0] = Input.GetAxisRaw("Horizontal");
        contActions[1] = Input.GetAxisRaw("Vertical");
    }

    private Vector3 GenerateInterrupteurPosition()
    {
        return new Vector3(Random.Range(solMinX, solMaxX), interrupteurPosY, Random.Range(interrupteurminZ, interrupteurmaxZ));
    }

    private Vector3 GenerateAgentPosition()
    {
        return new Vector3(Random.Range(solMinX, solMaxX), agentPosY, Random.Range(agentMinZ, agentMaxZ));
    }
    
    private Vector3[] GenerateDoorsPositions()
    {
        Vector3[] positions = new Vector3[2];

        float position1X = Random.Range(murAvecPorteMinX, murAvecPorteMaxX);
        float position2X;

        // Génération de la position x pour la deuxième position sans superposition avec la première
        do
        {
            position2X = Random.Range(murAvecPorteMinX, murAvecPorteMaxX);
        } while (Mathf.Abs(position1X - position2X) < 1.2f); 

        positions[0] = new Vector3(position1X, grandePortePosY, murAvecPortePosZ);
        positions[1] = new Vector3(position2X, petitePortePosY, murAvecPortePosZ);

        return positions;
    }
}
