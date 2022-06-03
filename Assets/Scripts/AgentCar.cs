using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentCar : MonoBehaviour
{
    public List<GameObject> Checkpoints;
    
    public float TotalDistance = 0f;
    public float DistanceBetweenActiveCheckpoints = 0f;
    public float DistanceFromNextCheckpoint = 0f;

    public int CurrentCheckpoint = 0;

    private GameObject startPosition;
    private GameObject carObject;
    private Car car;
    
    float chanceOfMutation = 1f;

    public bool IsDead = false;

    public List<Gene> genome;
    public int currentGene;
    private float timeOnCurrentGene;
    public float currentFitness;

    // Start is called before the first frame update
    void Start()
    {
        //spawn a car prefab and store it in a variable
        carObject = Instantiate(Resources.Load("Prefabs/Car"), transform) as GameObject;

        //get the car component from the car object
        car = carObject.GetComponent<Car>();

        //get the start position
        startPosition = GameObject.Find("StartPosition");

        //set the car's position to the start position
        car.transform.position = startPosition.transform.position;

    }

    public void InitialiseGenome(int initialGeneCount)
    {
        genome = new List<Gene>();
        for (int i = 0; i < initialGeneCount; i++)
        {
            genome.Add(new Gene());
        }
    }

    public void Mutate()
    {
        //check if a chance has been reached and if so, set one gene in the genome to a new gene
        if (Random.Range(0f, 1f) < chanceOfMutation)
        {
            int randomGene = Random.Range(0, genome.Count);
            genome[randomGene] = new Gene();
        }
    }

    //function that adds new gene to list
    public void AddGene()
    {
        genome.Add(new Gene());
        //print(Gene.NumGenes);
    }

    public void SetCheckpoints(List<GameObject> checkpoints)
    {
        Checkpoints = checkpoints;

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        GetComponentInChildren<SpriteRenderer>().color = (!IsDead) ? Color.Lerp(Color.white, Color.red, currentFitness / 100f) : Color.black;

        //DistanceFromNextCheckpoint = (carObject.transform.position - Checkpoints[CurrentCheckpoint + 1].transform.position).magnitude / ((carObject.transform.position - Checkpoints[CurrentCheckpoint].transform.position).magnitude + (Checkpoints[CurrentCheckpoint + 1].transform.position - carObject.transform.position).magnitude) *
        //                             (Checkpoints[CurrentCheckpoint + 1].transform.position - Checkpoints[CurrentCheckpoint].transform.position).magnitude;

        //Set distance
        DistanceBetweenActiveCheckpoints = Vector3.Distance(Checkpoints[CurrentCheckpoint].transform.position, Checkpoints[CurrentCheckpoint + 1].transform.position);

        DistanceFromNextCheckpoint =
            (carObject.transform.position - Checkpoints[CurrentCheckpoint + 1].transform.position).magnitude;


        timeOnCurrentGene += Time.fixedDeltaTime;
        
        if (currentGene < genome.Count)
        {
            if (timeOnCurrentGene >= genome[currentGene].Time)
            {
                timeOnCurrentGene = 0;
                currentGene++;
            }
            else
            {
                car.Move(genome[currentGene].HorizontalSteering);
                car.Rotate(genome[currentGene].Rotation);
            }
        }
        else
        {
            DestroyCar();
        }

        if (!IsDead)
            currentFitness = CalculateFitness();
    }

    //function that returns the car's genome
    public List<Gene> GetGenome()
    {
        return genome;
    }

    //function that destroys the car
    public void DestroyCar()
    {
        IsDead = true;
    }

    public void OnCarCollision()
    {
        DestroyCar();
    }

    public void CheckIfHitNextCheckpoint(GameObject checkpoint)
    {
        //get last element from checkpoints list
        GameObject lastCheckpoint = Checkpoints[Checkpoints.Count - 1];

        //if the car is at the last checkpoint
        if (checkpoint == lastCheckpoint)
        {
            //reset genetic algorithm
            GetComponentInParent<GeneticAlgorithm>().Reset();
        }

        if (Checkpoints[CurrentCheckpoint + 1] == checkpoint)
        {
            TotalDistance += Vector3.Distance(Checkpoints[CurrentCheckpoint].transform.position, Checkpoints[CurrentCheckpoint + 1].transform.position);
            CurrentCheckpoint++;
            DistanceBetweenActiveCheckpoints = Vector3.Distance(Checkpoints[CurrentCheckpoint].transform.position, Checkpoints[CurrentCheckpoint + 1].transform.position);
        }
        else if (Checkpoints[CurrentCheckpoint - 1] == checkpoint)
        {
            CurrentCheckpoint--;
            TotalDistance -= Vector3.Distance(Checkpoints[CurrentCheckpoint].transform.position, Checkpoints[CurrentCheckpoint + 1].transform.position);
            DistanceBetweenActiveCheckpoints = Vector3.Distance(Checkpoints[CurrentCheckpoint].transform.position, Checkpoints[CurrentCheckpoint + 1].transform.position);
        }
    }
    public void ResetCar()
    {
        print(genome.Count);
        
        Destroy(carObject);

        IsDead = false;

        //Create new car
        carObject = Instantiate(Resources.Load("Prefabs/Car") as GameObject, transform);
        carObject.transform.position = startPosition.transform.position;
        car = carObject.GetComponent<Car>();

        CurrentCheckpoint = 0;
        TotalDistance = 0f;
        DistanceBetweenActiveCheckpoints = 0f;
        DistanceFromNextCheckpoint = 0f;
        currentGene = 0;
    }

    public float CalculateFitness()
    {
        return TotalDistance + (DistanceBetweenActiveCheckpoints - DistanceFromNextCheckpoint);
    }

}
