﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class District : MonoBehaviour
{

    private const int MAX_HOUSES = 7;
    private const int MIN_HOUSES = 3;

    private int m_nbHouses;

    private List<GameObject> m_houses;

    [SerializeField] GameObject m_house;

    public void Start()
    {
        m_houses = new List<GameObject>();
        m_nbHouses = Random.Range(MIN_HOUSES, MAX_HOUSES);
        CreateDistrict();
    }

    
    // Calcule la distance à laquelle le centre de la deuxième maison doit être placé
    private Vector3 ComputeNewCenterOfHouse(GameObject house, GameObject addedHouse)
    {
        Vector3 fieldHouse = house.GetComponent<House>().Field;
        Vector3 fieldAddedHouse = addedHouse.GetComponent<House>().Field;

        return new Vector3(house.transform.position.x, 0f, house.transform.position.z + (fieldHouse.z / 2f) + (fieldAddedHouse.z / 2f));
    }

    // Créer le quartier en y ajoutant les maisons
    private void AddHouse()
    {
        if(m_houses.Count == 0)
        {
            // On créer la première maison sur la position du quartier
            GameObject addedHouse = Instantiate(m_house, transform.position, Quaternion.identity);
            addedHouse.GetComponent<House>().Start();
            m_houses.Add(addedHouse);
        }
        else
        {
            GameObject addedHouse = Instantiate(m_house, transform.position, Quaternion.identity);
            addedHouse.GetComponent<House>().Start();

            Vector3 newPos = ComputeNewCenterOfHouse(m_houses[m_houses.Count - 1], addedHouse);

            addedHouse.transform.position = newPos;
        }
    }

    // Créer le quartier
    private void CreateDistrict()
    {
        for(int i = 0 ; i < m_nbHouses; i++)
        {
            AddHouse();
        }
    }
}
