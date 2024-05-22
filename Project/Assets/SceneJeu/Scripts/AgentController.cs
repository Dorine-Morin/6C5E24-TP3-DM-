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
   
    private GameObject porteOuverte;
    private Material porteOuverteMaterial;
    private int indexPorteOuverte;

    private Transform butTransform;

    private Color couleurOn = Color.yellow;
    private Color couleurOff = Color.black;
    private Color couleurMurSansPortes = Color.red;
    private Color couleurMurAvecPortes = Color.blue;
    private Color couleurPorteFerme = Color.magenta;
    private Color couleurPorteOuverte = Color.green;

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
    private float interrupteurminZ = -2f;
    private float interrupteurmaxZ = 1f;
    private float interrupteurPosY = 0.2f;
    private float agentMinZ = -2f;
    private float agentMaxZ = 1f;
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
        sensor.AddObservation(butTransform.localPosition);
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
            BoutonAtteint();
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
            solMaterial.color = couleurMurSansPortes;
        }
        else if (collider == murAvecPortesTag)
        {
            AddReward(-1000f);
            solMaterial.color = couleurMurAvecPortes;
        }
        else if (collider == porteFermeTag)
        {
            AddReward(-100f);
            solMaterial.color = couleurPorteFerme;
        }

        EndEpisode();
    }

    private void GameSuccess()
    {
        solMaterial.color = couleurPorteOuverte;

        AddReward(100000f);
        EndEpisode();
    }

    private void BoutonAtteint()
    {
        if (interrupteurOff)
        {  
            AddReward(1000f);
            interrupteurMaterial.color = couleurOff;
            SetPorteOuverte();
            butTransform = porteOuverte.transform;

        } else
        {
            interrupteurMaterial.color = couleurOn;
            porteOuverte.gameObject.tag = porteFermeTag;
            porteOuverteMaterial.color = couleurOff;
            butTransform = interrupteurTransform;
        }

        interrupteurOff = !interrupteurOff;
    }

    private void SetPorteOuverte()
    {
        indexPorteOuverte = Random.Range(0, portes.Count);
        porteOuverte = portes[indexPorteOuverte];
        porteOuverteMaterial = porteOuverte.GetComponent<MeshRenderer>().material;
        porteOuverte.gameObject.tag = porteOuverteTag;
        porteOuverteMaterial.color = couleurOn;
    }

    public override void OnEpisodeBegin()
    {
        // Initialisation de l'interrupteur
        interrupteurMaterial = interrupteur.GetComponent<MeshRenderer>().material;
        interrupteurTransform = interrupteur.transform;
        interrupteurMaterial.color = couleurOn;
        interrupteurOff = true;
        butTransform = interrupteurTransform;

        // Initialisation des portes
        foreach (var porte in portes)
        {
            porte.GetComponent<MeshRenderer>().material.color = couleurOff;
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

        do
        {
            position2X = Random.Range(murAvecPorteMinX, murAvecPorteMaxX);
        } while (Mathf.Abs(position1X - position2X) < 1.2f); 

        positions[0] = new Vector3(position1X, grandePortePosY, murAvecPortePosZ);
        positions[1] = new Vector3(position2X, petitePortePosY, murAvecPortePosZ);

        return positions;
    }
}
