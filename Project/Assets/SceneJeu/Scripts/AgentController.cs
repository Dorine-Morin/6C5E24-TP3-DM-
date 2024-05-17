using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Xsl;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public class AgentController : Agent
{
    
    [SerializeField] private GameObject sol;
    [SerializeField] private Material solMaterial;

    [SerializeField] private GameObject interrupteur;
    [SerializeField] private Transform interrupteurTransform;
    [SerializeField] private Material interrupteurMaterial;
    [SerializeField] private bool interrupteurAtteint;

    [SerializeField] private List<GameObject> portes;

    [SerializeField] private Transform porte1Transform;
    [SerializeField] private Material porte1Material;
    [SerializeField] private bool porte1Ouverte = false;

    [SerializeField] private Transform porte2Transform;
    [SerializeField] private Material porte2Material;
    [SerializeField] private bool porte2Ouverte = false;

    [SerializeField] private GameObject porteToReach;
    [SerializeField] private Transform porteToReachTransform;
    [SerializeField] private Material porteToReachMat;
    [SerializeField] private int indexPorteToReach;

    [SerializeField] private Color materialOn = Color.yellow;
    [SerializeField] private Color materialOff = Color.black;
    [SerializeField] private Color materialGameOver = Color.red;
    [SerializeField] private Color materialGameSuccess = Color.green;


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

        sensor.AddObservation(porte1Ouverte ? 1: 0);
        sensor.AddObservation(porte2Ouverte ? 1 : 0);

        if (!interrupteurAtteint)
        {
            sensor.AddObservation(interrupteurTransform.localPosition);
        } else
        {
            sensor.AddObservation(porteToReachTransform.localPosition);
        }        
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((other.CompareTag("Porte1") && porte1Ouverte) || (other.CompareTag("Porte2") && porte2Ouverte))
        {
            GameSuccess();
        }
        else if ((other.CompareTag("Porte1") && !porte1Ouverte) || (other.CompareTag("Porte2") && !porte2Ouverte))
        {
            GameOver("PorteFerme");
        }
        else if (other.CompareTag("Bouton"))
        {
            BoutonReached();
        }
        else if (other.CompareTag("Mur"))
        {
            GameOver("Mur");
        }
        else if (other.CompareTag("MurPorte"))
        {
            GameOver("MurPorte");
        }
    }


    private void GameOver(string collider)
    {
        solMaterial.color = materialGameOver;

        if (collider == "Mur")
        {
            AddReward(-100000f);
        } else if(collider == "MurPorte")
        {
            AddReward(-10000f);
            solMaterial.color = Color.blue;
        }
            else if(collider == "PorteFerme")
        {
            AddReward(-100f);
            solMaterial.color = Color.magenta;
        } 

        EndEpisode();
    }

    private void GameSuccess()
    {
        solMaterial.color = materialGameSuccess;

        SetReward(100000f);
        EndEpisode();
    }

    private void BoutonReached()
    {
        if (interrupteurAtteint)
        {
            SetReward(1000f);
            indexPorteToReach = Random.Range(0, portes.Count);
            porteToReach = portes[indexPorteToReach];
            porteToReachMat = porteToReach.GetComponent<MeshRenderer>().material;
            porteToReachTransform = porteToReach.transform;
        } 
        
        interrupteurAtteint = !interrupteurAtteint;
        interrupteurMaterial.color = interrupteurAtteint ? materialOn : materialOff;
        porteToReachMat.color = interrupteurAtteint ? materialOff : materialOn;

        if (indexPorteToReach == 0)
        {
            porte1Ouverte = !porte1Ouverte;
        }
        else
        {
            porte2Ouverte = !porte2Ouverte;
        }
    }


    public override void OnEpisodeBegin()
    {
        // Initialisation de l'interrupteur
        interrupteurMaterial = interrupteur.GetComponent<MeshRenderer>().material;
        interrupteurTransform = interrupteur.transform;
        interrupteurMaterial.color = materialOn;
        interrupteurAtteint = true;

        // Initialisation des portes
        porte1Material = portes[0].GetComponent<MeshRenderer>().material;
        porte1Transform = portes[0].transform;
        porte1Ouverte = false;

        porte2Material = portes[1].GetComponent<MeshRenderer>().material;
        porte2Transform = portes[1].transform;
        porte2Ouverte = false;

        // Choix aléatoire de la porte à atteindre
        porteToReach = portes[0];
        porteToReachMat = porteToReach.GetComponent<MeshRenderer>().material;
        porteToReachTransform = porteToReach.transform;

        // Initialisation du sol
        solMaterial = sol.GetComponent<MeshRenderer>().material;

        // Position initiale de l'agent et de l'interrupteur
        Vector3[] agentAndSwitchPositions = GeneratePositions(0.56f, 0.2f, -3.5f, 1.5f, 2f);
        //transform.localPosition = agentAndSwitchPositions[0];
        //interrupteurTransform.localPosition = agentAndSwitchPositions[1];
        transform.localPosition = new Vector3(-1.5f, 0.56f, -0.45f);
        interrupteurTransform.localPosition = new Vector3(-0.2f, 0.2f, 1.2f);

        // Couleur initiales de l'interrupteur
        interrupteurMaterial.color = materialOn;

        // Position initiale des portes
        Vector3[] doorPositions = GeneratePositions(1f, 0.5f, 4.10f, 4.10f, 2f);
        porte1Transform.localPosition = doorPositions[0];
        porte2Transform.localPosition = doorPositions[1];
        //porte1Transform.localPosition = new Vector3(-1.5f, 1f, 4.1f);
        //porte2Transform.localPosition = new Vector3(1.75f, 0.5f, 4.1f);

        // Initialisation de la couleur des portes
        porte1Material.color = materialOff;
        porte2Material.color = materialOff;
    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> contActions = actionsOut.ContinuousActions;
        contActions[0] = Input.GetAxisRaw("Horizontal");
        contActions[1] = Input.GetAxisRaw("Vertical");
    }

    private Vector3[] GeneratePositions(float minY, float maxY, float minZ, float maxZ, float minDistance)
    {
        Vector3[] positions = new Vector3[2];

        // Génération de la position x pour la première position
        float position1X = Random.Range(-2f, 2f);
        float position2X;

        // Génération de la position x pour la deuxième position sans superposition avec la première
        do
        {
            position2X = Random.Range(-2f, 2f);
        } while (Mathf.Abs(position1X - position2X) < minDistance); // Vérification de la distance minimale entre les positions

        float position1Z = minZ;
        float position2Z = maxZ;

        if (position1Z != position2Z)
        {
            position1Z = Random.Range(minZ, maxZ);
            do
            {
                position2Z = Random.Range(minZ, maxZ);
            } while (Mathf.Abs(position1Z - position2Z) < minDistance);
        }

        // Assignation des positions y et z pour les deux positions
        positions[0] = new Vector3(position1X, minY, position1Z);
        positions[1] = new Vector3(position2X, maxY, position2Z);

        return positions;
    }
}
