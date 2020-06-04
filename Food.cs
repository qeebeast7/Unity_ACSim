using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour {
    bool isExist = true;
	void Update () {
        if (isExist)
        {
            if (transform.localScale.x<=1)
            {
                AntColonyManager.Instance.RemoveFood(gameObject);
                isExist = false;
                gameObject.SetActive(false);
            }
        }
	}
}
