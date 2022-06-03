using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using TMPro;
using Random = UnityEngine.Random;

public class GeneticAlgorithm : MonoBehaviour
{
    public TMP_Text GenerationText;
    public TMP_Text HighestFitnessText;
    public TMP_Text AverageFitnessText;
    public TMP_Text TimescaleText;

    public bool FastForward = false;
    
    public List<GameObject> Checkpoints;

    public int PopulationSize = 100;
    public List<GameObject> Agents;
    public int Generation = 1;
    private float alpha = 0.2f;
    bool isRunning = true;

    [Range(1, 100)]
    public float Timescale = 1;
    public int initialGeneAmount = 10;

    // Start is called before the first frame update
    void Start()
    {
        Application.runInBackground = true;

        // Create the population
        for (int i = 0; i < PopulationSize; i++)
        {
            //instantiate a new agent and add it to the list
            Agents.Add(Instantiate(Resources.Load("Prefabs/CarAgent") as GameObject, transform));
            
            Agents[i].GetComponent<AgentCar>().Checkpoints = Checkpoints;
            Agents[i].GetComponent<AgentCar>().InitialiseGenome(initialGeneAmount);
        }

        GenerationText = GameObject.Find("GenerationText").GetComponent<TMP_Text>();
        HighestFitnessText = GameObject.Find("HighestFitnessText").GetComponent<TMP_Text>();
        AverageFitnessText = GameObject.Find("AverageFitnessText").GetComponent<TMP_Text>();
        TimescaleText = GameObject.Find("TimescaleText").GetComponent<TMP_Text>();

    }

    //function that checks if a csv file exists and creates one if it doesn't
    public void CreateCSV()
    {
        if (!System.IO.File.Exists("Assets/Resources/CSV/GeneticAlgorithm.csv"))
        {
            System.IO.File.Create("Assets/Resources/CSV/GeneticAlgorithm.csv");
        }
    }

    //function that appends current generation, average fitness of agents, highest fitness of agents, average gene count and hightest gene count to a csv file
    public void AppendToCSV()
    {
        CreateCSV();
        string path = "Assets/Resources/CSV/GeneticAlgorithm.csv";
        string line = Generation + "," + Agents.Average(x => x.GetComponent<AgentCar>().currentFitness) + "," +
                      Agents.Max(x => x.GetComponent<AgentCar>().currentFitness) + "," +
                      Agents.Average(x => x.GetComponent<AgentCar>().genome.Count) + "," +
                      Agents.Max(x => x.GetComponent<AgentCar>().genome.Count) + "\n";
        System.IO.File.AppendAllText(path, line);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        GenerationText.text = "Generation: " + Generation;
        HighestFitnessText.text = "Highest Fitness: " + Agents.Max(x => x.GetComponent<AgentCar>().currentFitness);
        AverageFitnessText.text = "Average Fitness: " + Agents.Average(x => x.GetComponent<AgentCar>().currentFitness);
        TimescaleText.text = "Timescale: " + Timescale;

        foreach (var car in Agents)
        {
            car.GetComponentInChildren<SpriteRenderer>().enabled = (FastForward) ? false : true;
        }

        Time.timeScale = Timescale;
        
        //check if all agents have died
        bool allDead = true;
        foreach (GameObject agent in Agents)
        {
            if (!agent.GetComponent<AgentCar>().IsDead)
            {
                allDead = false;
            }
        }

        if (allDead && isRunning)
        {
            //create a new generation
            AppendToCSV();
            CreateNewGeneration();
        }
    }

    private void CreateNewGeneration()
    {
        isRunning = false;
        
        foreach (var agent in Agents.ToList())
        {
            agent.GetComponent<AgentCar>().IsDead = false;
        }
        
        List<GameObject> eliteAgents = GetTopFittestAgents(10);
        List<GameObject> winningAgents = TournamentSelection(PopulationSize / 2, PopulationSize / 10);

        //randomise order of winning agents
        winningAgents = winningAgents.OrderBy(x => Random.value).ToList();

        //Children are created and instantiated into the scene, the winning and elites already exist, so must be filtered from the rest
        List<GameObject> childrenAgents = BlendedCrossover(winningAgents);

        //mutate the children
        foreach (var agent in childrenAgents)
        {
            agent.GetComponent<AgentCar>().Mutate();
            agent.GetComponent<AgentCar>().AddGene();
        }

        foreach (var agent in Agents.ToList())    
        {
            if (!eliteAgents.Contains(agent) && !winningAgents.Contains(agent))
            {
                Destroy(agent);
                Agents.Remove(agent);
            }
            else
            {
                agent.GetComponent<AgentCar>().AddGene();
                agent.GetComponent<AgentCar>().ResetCar();
            }
        }

        Agents = Agents.Concat(childrenAgents).ToList();

        int temp = Agents.ToList().Count;
        
        for (int i = 0; i < PopulationSize - temp; i++)
        {
            GameObject agent;
            Agents.Add(agent = Instantiate(Resources.Load("Prefabs/CarAgent") as GameObject, transform));
            agent.GetComponent<AgentCar>().Checkpoints = Checkpoints;
            agent.GetComponent<AgentCar>().InitialiseGenome(initialGeneAmount);
        }

        Generation++;

        isRunning = true;

    }

    //function that returns the top 10 fittest agents
    public List<GameObject> GetTopFittestAgents(int NumOfAgents)
    {
        //sort the agents by fitness
        Agents.Sort(delegate (GameObject a, GameObject b)
        {
            return b.GetComponent<AgentCar>().currentFitness.CompareTo(a.GetComponent<AgentCar>().currentFitness);
        });

        //return the top 10 agents
        List<GameObject> topAgents = new List<GameObject>();
        for (int i = 0; i < NumOfAgents; i++)
        {
            topAgents.Add(Agents[i]);
        }

        return topAgents;
    }

    //function that performs tournament selection on the agents and returns a list of the winners
    public List<GameObject> TournamentSelection(int NumOfWinners, int NumOfAgents)
    {
        //create a list of the winners
        List<GameObject> winners = new List<GameObject>();

        //for each of the winners
        for (int i = 0; i < NumOfWinners; i++)
        {
            //create a list of the agents that will be used in the tournament
            List<GameObject> tournamentAgents = new List<GameObject>();

            //for each of the agents in the tournament
            for (int j = 0; j < NumOfAgents; j++)
            {
                //add a random agent to the tournament
                tournamentAgents.Add(Agents[Random.Range(0, Agents.Count)]);
            }

            //sort the tournament agents by fitness
            tournamentAgents.Sort(delegate (GameObject a, GameObject b)
            {
                return b.GetComponent<AgentCar>().currentFitness.CompareTo(a.GetComponent<AgentCar>().currentFitness);
            });

            //add the first agent in the tournament to the winners list
            winners.Add(tournamentAgents[0]);
        }

        return winners;
    }

    public List<GameObject> BlendedCrossover(List<GameObject> parents)
    {
        //create a list of the children
        List<GameObject> children = new List<GameObject>();

        print("Parents: " + parents.Count);

        GameObject child = null;
        //for each of the parents
        for (int i = 0; i < parents.Count - 1; i++)
        {
            //create a child
            child = Instantiate(Resources.Load("Prefabs/CarAgent") as GameObject, transform);

            child.GetComponent<AgentCar>().Checkpoints = Checkpoints;

            //get the genes from the parents
            List<Gene> parent1Genes = parents[i].GetComponent<AgentCar>().genome;
            List<Gene> parent2Genes = parents[i + 1].GetComponent<AgentCar>().genome;

            List<Gene> childGenes = new List<Gene>();

            for (int j = 0; j < Mathf.Min(parent1Genes.Count, parent2Genes.Count); j++)
            {
                Gene newGene = new Gene();
                newGene.HorizontalSteering = parent1Genes[j].HorizontalSteering + alpha * (parent2Genes[j].HorizontalSteering - parent1Genes[j].HorizontalSteering);
                newGene.Rotation = parent1Genes[j].Rotation + alpha * (parent2Genes[j].Rotation - parent1Genes[j].Rotation);
                newGene.Time = parent1Genes[j].Time + alpha * (parent2Genes[j].Time - parent1Genes[j].Time);

                childGenes.Add(newGene);
            }

            child.GetComponent<AgentCar>().genome = childGenes;
            children.Add(child);
        }

        return children;
    }

    //function that performs single point crossover on the agents and returns a list of the children
    public List<GameObject> SinglePointCrossover(List<GameObject> parents)
    {
        //create a list of the children
        List<GameObject> children = new List<GameObject>();

        print("Parents: " + parents.Count);
        
        GameObject child = null;
        //for each of the parents
        for (int i = 0; i < parents.Count; i += 2)
        {
            for (int j = 0; j < 2; j++)
            {
                //create a child
                child = Instantiate(Resources.Load("Prefabs/CarAgent") as GameObject, transform);

                child.GetComponent<AgentCar>().Checkpoints = Checkpoints;

                //get the genes from the parents
                List<Gene> parent1Genes = parents[i].GetComponent<AgentCar>().genome;
                List<Gene> parent2Genes = parents[i + 1].GetComponent<AgentCar>().genome;

                //create a list of the child genes
                List<Gene> childGenes = new List<Gene>();

                //for each of the genes in the parents
                for (int k = 0; k < Mathf.Min(parent1Genes.Count, parent2Genes.Count); k++)
                {
                    if (Random.Range(0, 1) == 0)
                    {
                        //add the gene to the child
                        childGenes.Add(parent1Genes[j]);
                    }
                    else
                    {
                        //add the gene to the child
                        childGenes.Add(parent2Genes[j]);
                    }
                }

                //set the child genes
                child.GetComponent<AgentCar>().genome = childGenes;
                children.Add(child);
            }
            
        }

        return children;
    }
    public void IncreaseTimescale()
    {
        Timescale++;
    }

    public void DecreaseTimescale()
    {
        if (Timescale > 1)
        {
            Timescale--;
        }
    }

    //function that resets genetic algorithm variables
    public void Reset()
    {
        AppendToCSV();
        
        Generation = 0;
        //delete all objects in agent list
        foreach (GameObject agent in Agents)
        {
            Destroy(agent);
        }
        Agents.Clear();
        isRunning = true;
        Start();
    }

}
