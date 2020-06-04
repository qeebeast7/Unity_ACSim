using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pheromone : MonoBehaviour {
    public List<Vector3> foodPos = new List<Vector3>();
    public void AddFood(Vector3 pos)
    {
        if (!foodPos.Contains(pos))
        {
            foodPos.Add(pos);
        }
    }
    public void RemoveFood(Vector3 pos)
    {
        if (foodPos.Contains(pos))
        {
            foodPos.Remove(pos);
        }
    }
    public Vector3 GetFoodPos()
    {
        if (foodPos.Count > 0)
        {
            return foodPos[Random.Range(0, foodPos.Count)];
        }
        else return transform.position;
    }
}
