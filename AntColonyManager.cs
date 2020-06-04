using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class AntColonyManager : MonoBehaviour
{
    public static AntColonyManager Instance;

    public GameObject antPrefab;
    public GameObject net;
    private float netRadius;

    private Slider[] sliders;

    public float rate;
    public float decay;
    public int count;
    public float speed;
    public float carry;
    public float radius;

    private int maxAntCount;//最多蚂蚁数
    public List<Ant> ants = new List<Ant>();//所有蚂蚁列表
    public List<Ant> curAnts = new List<Ant>();//现有蚂蚁列表
    public List<GameObject> foods = new List<GameObject>();//所有食物列表
    private float[] etas;

    private int alpha=5;
    private int beta=5;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        netRadius = net.transform.localScale.x;
        //Bind
        sliders = GetComponentsInChildren<Slider>();
        for (int i = 0; i < sliders.Length; i++)
        {
            Slider slider = sliders[i];
            if (i != 2) slider.onValueChanged.AddListener((float value) => OnSliderChanged(value, slider));
            //数量改变
            else slider.onValueChanged.AddListener((float value) => OnCountChanged(value, slider));
        }
        //Get Value
        rate = sliders[0].value;
        decay = sliders[1].value;
        count = (int)sliders[2].value;
        speed = sliders[3].value;
        carry = sliders[4].value;
        radius = sliders[5].value;
        maxAntCount = (int)sliders[2].maxValue;
        //produce ants
        for (int i = 0; i < maxAntCount; i++)
        {
            Vector3 pos = Random.insideUnitSphere * netRadius;
            pos = pos.normalized * (netRadius / 2 + pos.magnitude);
            pos = new Vector3(pos.x + net.transform.position.x, 0.5f, pos.z + net.transform.position.z);
            GameObject antObj = Instantiate(antPrefab, pos, Quaternion.identity);
            Ant ant = antObj.GetComponent<Ant>();
            ants.Add(ant);
            antObj.SetActive(false);
        }
        ProduceAnts();
        //get foods
        foods = GameObject.FindGameObjectsWithTag("Food").ToList();
        etas = new float[ants.Count];
    }
    void ProduceAnts()
    {
        if (curAnts.Count < count)
        {
            for (int i = 0; i < count; i++)
            {
                if (!ants[i].gameObject.activeInHierarchy)
                {
                    ants[i].gameObject.SetActive(true);
                    curAnts.Add(ants[i]);
                }
            }
        }
        else if (curAnts.Count > count)
        {
            for (int i = count; i < curAnts.Count; i++)
            {
                if (ants[i].gameObject.activeInHierarchy)
                {
                    ants[i].gameObject.SetActive(false);
                    curAnts.Remove(ants[i]);
                }
            }
        }
    }
    void OnSliderChanged(float value, Slider slider)
    {
        rate = sliders[0].value;
        decay = sliders[1].value;
        speed = sliders[3].value;
        carry = sliders[4].value;
        radius = sliders[5].value;
        for (int i = 0; i < ants.Count; i++)
        {
            ants[i].SetInfo();
        }
    }
    void OnCountChanged(float value, Slider slider)
    {
        count = (int)sliders[2].value;
        ProduceAnts();
    }
    public bool IsFoodEmpty()
    {
        return foods.Count <= 0;
    }
    public void RemoveFood(GameObject food)
    {
        foods.Remove(food);
        for (int i = 0; i < ants.Count; i++)
        {
            ants[i].RemoveFoodPos(food.transform.position);
        }
    }
    public void CalP()
    {
        foreach (Ant ant in ants)
        {
            for (int i = 0; i < ant.tense.Length; i++)
            {
                ant.tense[i] = ant.curPhero;                
                ant.tense[i] = (1 - decay) * ant.tense[i] +GetTense(i);
                print(ant.tense[i]);
            }
        }
        float sum = 0;
        for (int i = 0; i < ants.Count; i++)
        {
            Ant ant = ants[i];
            for (int j = 0; j < foods.Count; j++)
            {
                etas[i] = 1 / (Vector3.Distance(ant.gameObject.transform.position,
                    foods[j].transform.position));
                for (int k = 0; k < ant.tense.Length; k++)
                {
                    sum += Mathf.Pow(ant.tense[k], alpha) * Mathf.Pow(etas[i], beta);
                }
            }
        }
        foreach (Ant ant in ants)
        {
            for (int i = 0; i < ant.p.Length; i++)
            {
                ant.p[i] = (Mathf.Pow(ant.tense[i], alpha) * Mathf.Pow(etas[i], beta)) / sum;
            }
        }
    }
    float GetTense(int k)
    {
        float sum = 0;
        foreach (Ant ant in ants)
        {
            for (int i = 0; i < ant.tense.Length-1; i++)
            {
                sum += Mathf.Pow(ant.tense[i + 1] - ant.tense[i],k);
            }
        }
        return sum;
    }
}
