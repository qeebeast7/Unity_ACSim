using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Ant : MonoBehaviour
{
    public enum AntState
    {
        Explore,//寻路
        Transport,//运输食物
        Follow//跟随信息素
    }
    private AntState state;
    private Transform net;
    private float exploreTimer = 2;//寻路timer
    //private float produceTimer = 0;//释放信息素timer

    private NavMeshAgent agent;
    private GameObject pheromone;//信息素
    public float curPhero;//剩余信息素

    private float rate;
    private float decay;
    private float speed;
    private float carry;
    private float radius;

    public float[] tense;//信息素强度
    public float[] p;//概率
    public List<Vector3> foodPos = new List<Vector3>();//找到的食物位置

    void Start()
    {
        pheromone = transform.GetChild(0).gameObject;
        net = GameObject.FindGameObjectWithTag("Net").transform;
        agent = GetComponent<NavMeshAgent>();
        state = AntState.Explore;
        curPhero = 2;
        SetInfo();
    }
    public void SetInfo()
    {
        tense = new float[AntColonyManager.Instance.foods.Count];
        p = new float[AntColonyManager.Instance.foods.Count];

        rate = AntColonyManager.Instance.rate;
        decay = AntColonyManager.Instance.decay;
        speed = AntColonyManager.Instance.speed;
        carry = AntColonyManager.Instance.carry;
        radius = AntColonyManager.Instance.radius;
    }
    // Update is called once per frame
    void Update()
    {
        agent.speed = speed;
        switch (state)
        {
            case AntState.Explore:
                RandomExplore();
                break;
            case AntState.Transport:
                TransportFood();
                break;
            case AntState.Follow:
                FollowPheromone();
                break;
            default:
                break;
        }
    }
    void SetPheromone()
    {
        ParticleSystem ps = pheromone.GetComponent<ParticleSystem>();
        BoxCollider bc = pheromone.GetComponent<BoxCollider>();
        var main = ps.main;
        main.startLifetime = curPhero;
        bc.size = new Vector3(bc.size.x, bc.size.y, curPhero);
        bc.center = new Vector3(bc.center.x, bc.center.y, curPhero / 2f);
        //AntColonyManager.Instance.CalP();
    }
    //释放信息素
    void ProducePheromone()
    {
        curPhero = 2;
        SetPheromone();
        InvokeRepeating("DecayPheromone", rate, rate);
    }
    //信息素衰减
    void DecayPheromone()
    {
        curPhero *= (1 - decay);
        SetPheromone();
    }
    //信息素增强
    void EnhancePheromone()
    {
        curPhero *= (1 + decay);
        SetPheromone();
    }
    //随机寻路
    void RandomExplore()
    {
        exploreTimer += Time.deltaTime;
        if (exploreTimer >= 2)
        {
            Vector3 randomPos = Random.insideUnitSphere * radius;
            randomPos += transform.position;
            NavMeshHit hit;
            NavMesh.SamplePosition(randomPos, out hit, radius, 1);
            agent.SetDestination(hit.position);
            exploreTimer = 0;
        }
    }
    //运输食物
    void TransportFood()
    {
        agent.SetDestination(net.transform.position);
        ProducePheromone();
    }
    //跟随信息素
    void FollowPheromone()
    {
        if (foodPos.Count > 0)
        {
            int j = Random.Range(0, foodPos.Count);
            float pj = p[j];
            if (true)//p
            {
                agent.SetDestination(foodPos[j]);
            }
        }
        else
        {
            state = AntState.Explore;
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Pheromone"))
        {
            BoxCollider bc = other.GetComponent<BoxCollider>();
            if (bc.size.z > 0.1f)
            {
                if (state != AntState.Transport)
                {
                    state = AntState.Follow;
                }
                foodPos = other.GetComponent<Pheromone>().foodPos;
            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Food"))
        {
            state = AntState.Transport;
            EnhancePheromone();
            GameObject food = collision.gameObject;
            food.transform.localScale -= Vector3.one * carry;
            pheromone.GetComponent<Pheromone>().AddFood(food.transform.position);
        }
        if (collision.gameObject.CompareTag("Net"))
        {
            if (AntColonyManager.Instance.IsFoodEmpty())
            {
                state = AntState.Follow;
            }
            else
            {
                state = AntState.Explore;
            }
        }
    }
    public void RemoveFoodPos(Vector3 pos)
    {
        pheromone.GetComponent<Pheromone>().RemoveFood(pos);
        if (foodPos.Contains(pos))
        {
            foodPos.Remove(pos);
        }
    }
}
