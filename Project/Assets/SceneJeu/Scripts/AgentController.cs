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
    private Material porte1Material;
    private bool porte1Ouverte = false;

    private Transform porte2Transform;
    private Material porte2Material;
    private bool porte2Ouverte = false;

    private GameObject porteToReach;
    private Transform porteToReachTransform;
    private Material porteToReachMat;
    private int indexPorteToReach;

    private Transform goalTransform;

    private Color materialOn = Color.yellow;
    private Color materialOff = Color.black;
    private Color materialGameOver = Color.red;
    private Color materialGameSuccess = Color.green;


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
        sensor.AddObservation(porteToReachTransform.localPosition);
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
            AddReward(-1000f);
        }
        else if (collider == "MurPorte")
        {
            AddReward(-1000f);
            solMaterial.color = Color.blue;
        }
        else if (collider == "PorteFerme")
        {
            AddReward(-100f);
            solMaterial.color = Color.magenta;
        }

        EndEpisode();
    }

    private void GameSuccess()
    {
        solMaterial.color = materialGameSuccess;

        AddReward(100000f);
        EndEpisode();
    }

    private void BoutonReached()
    {
        if (!interrupteurOff)
        {
            AddReward(1000f);
            indexPorteToReach = Random.Range(0, portes.Count);
            porteToReach = portes[indexPorteToReach];
            porteToReachMat = porteToReach.GetComponent<MeshRenderer>().material;
            porteToReachTransform = porteToReach.transform;

            goalTransform = porteToReachTransform;

        } else
        {
            goalTransform = interrupteurTransform;
        }

        interrupteurOff = !interrupteurOff;
        interrupteurMaterial.color = interrupteurOff ? materialOff : materialOn;
        porteToReachMat.color = interrupteurOff ? materialOn : materialOff;
        

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
        interrupteurOff = false;
        goalTransform = interrupteurTransform;

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
        transform.localPosition = GenerateFloorPosition(0.5f, -2f, 2f, -1f, -2f);
        interrupteurTransform.localPosition = GenerateFloorPosition(0.2f, -2f, 2f, 0f, 1f);

        // Position initiale des portes
        Vector3[] doorPositions = GenerateDoorsPositions(0.9f, 0.5f, 2.65f, 2f);
        porte1Transform.localPosition = doorPositions[0];
        porte2Transform.localPosition = doorPositions[1];

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

    private Vector3 GenerateFloorPosition(float posY, float minX, float maxX, float minZ, float maxZ)
    {
        return new Vector3(Random.Range(minX, maxX), posY, Random.Range(minZ, maxZ));
    }
    
    private Vector3[] GenerateDoorsPositions(float minY, float maxY, float posZ, float minDistance)
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

        // Assignation des positions y et z pour les deux positions
        positions[0] = new Vector3(position1X, minY, posZ);
        positions[1] = new Vector3(position2X, maxY, posZ);

        return positions;
    }
}
