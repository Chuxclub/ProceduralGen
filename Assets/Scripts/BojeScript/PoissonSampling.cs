﻿
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Security.Cryptography;
using System.Diagnostics;
using UnityEngine;

public class PoissonSampling : MonoBehaviour
{
    [Header("Basic Settings")]

    [SerializeField]
    [Range(0f, 100f)]
    [Tooltip("Distance minimale entre chaque points")]
    private float Rayon = 10f;

    [SerializeField]
    [Tooltip("Nombre d'essais par nouveau point")]
    private int Iterations = 100;

    [SerializeField]
    [Tooltip("Nombre d'essais de point")]
    private int Precision = 10000;

    [SerializeField]
    [Range(2, 2)]
    [Tooltip("WIP - NE PAS MODIFIER")]
    private int Dimension = 2;

    [SerializeField]
    [Range(0f, 2000f)]
    [Tooltip("Taille sur X")]
    public int Range_x = 500;

    [SerializeField]
    [Tooltip("Active le positionnement sur Y")]
    private bool scale_yEnable = true;

    [SerializeField]
    [Range(0f, 100f)]
    [Tooltip("Taille sur Y")]
    public float scale_y = 2f;

    [SerializeField]
    [Range(0f, 10f)]
    [Tooltip("Taille du masque de bruit de Perlin")]
    public float perlinScale = 2f;

    [SerializeField]
    [Range(0f, 2000f)]
    [Tooltip("Taille sur Z")]
    public int Range_z = 500;

    [Header("Display Settings")]

    [SerializeField]
    [Tooltip("Active l'instanciation")]
    private bool instanciateEnable = true;

    [SerializeField]
    [Tooltip("Objet instancié a chaque point calculé")]
    private GameObject objetInstance;

    [Header("Activity Settings")]

    [SerializeField] private bool activityEnable = true;
    [SerializeField] private int activityConcentration;


    private Vector3[,] grid;
    private List<GameObject> instanciatedPoints = new List<GameObject>();
    private List<List<Vector3>> resultGrid;
    private List<Vector3> active = new List<Vector3>();
    private List<Vector3> activityPoints;
    private Vector3 newPos;
    private Vector3 randomPos;
    private Vector3 activePos;
    private Vector3 neighborPos;
    private Vector3Int randomPosFloored;
    private Vector3Int newPosFloored;
    private Stopwatch stopwatchTimer;
    private float Cell_size;
    private float randomMagnitude;
    private float distance;
    private float activityRange;
    private int Cols_size;
    private int Rows_size;
    private int namingCount;
    private int randomIndex;
    private int debugCount;
    private bool isFound;
    private bool isCorrectDistance;

    public void initPoisson()
    {
        deleteComputed();
        Cell_size = (float)(Rayon / Math.Sqrt(Dimension));
        Rows_size = (int)Math.Ceiling(Range_x / Cell_size);
        Cols_size = (int)Math.Ceiling(Range_z / Cell_size);
        grid = new Vector3[Cols_size, Rows_size];
        namingCount = 0;
        debugCount = 0;
        stopwatchTimer = new Stopwatch();
        stopwatchTimer.Start();
        randomPos = new Vector3(UnityEngine.Random.Range(0.0f, (float)Range_x), 0f, UnityEngine.Random.Range(0.0f, (float)Range_z));
        randomPosFloored = floorVector3(randomPos);
        grid[randomPosFloored.x, randomPosFloored.z] = randomPos;
        active.Add(randomPos);
    }
    public void computePoints()
    {
        initPoisson();
        for (int l = 0; l < Precision; l++)
        {
            if (active.Count <= 0 && l != 0) { break; } // Safety check
            isFound = false;
            randomIndex = UnityEngine.Random.Range(0, active.Count);
            activePos = active[randomIndex];
            for (int n = 0; n < Iterations; n++)
            {
                newPos = new Vector3(UnityEngine.Random.Range(-1.0f, 1.0f), 0f, UnityEngine.Random.Range(-1.0f, 1.0f)).normalized;
                randomMagnitude = UnityEngine.Random.Range(0.0f, (float)(2 * Rayon));
                newPos = newPos * randomMagnitude;
                //newPos.y = UnityEngine.Random.Range(-(float)(Range_y / 2f), (float)(Range_y / 2f));
                newPos += activePos;
                newPosFloored = floorVector3(newPos);
                if (0 <= newPos.x && newPos.x < Range_x && 0 <= newPos.z && newPos.z < Range_z && 0 <= newPosFloored.x && newPosFloored.x < Cols_size && 0 <= newPosFloored.z && newPosFloored.z < Rows_size && grid[newPosFloored.x, newPosFloored.z] == Vector3.zero)
                {
                    isCorrectDistance = true;
                    for (int i = -1; i < 2; i++)
                    {
                        for (int j = -1; j < 2; j++)
                        {
                            if (newPosFloored.x + i >= 0 && newPosFloored.x + i < Cols_size && newPosFloored.z + j >= 0 && newPosFloored.z + j < Rows_size)
                            {
                                neighborPos = grid[newPosFloored.x + i, newPosFloored.z + j];
                                if (neighborPos != Vector3.zero)
                                {
                                    Vector2 pt1 = new Vector2(newPos.x, newPos.z);
                                    Vector2 pt2 = new Vector2(neighborPos.x, neighborPos.z);
                                    distance = Vector2.Distance(pt1, pt2);
                                    if (distance < 2 * Rayon)
                                    {
                                        isCorrectDistance = false;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (isCorrectDistance)
                    {
                        grid[newPosFloored.x, newPosFloored.z] = newPos;
                        debugCount += 1;
                        active.Add(newPos);
                        isFound = true;
                    }
                }
            }
            if (!isFound)
            {
                active.Remove(active[randomIndex]);
            }
        }
        if (scale_yEnable)
        {
            computePointsHeight();
        }
        displayGrid();
        stopwatchTimer.Stop();
        UnityEngine.Debug.Log("Exec time to create " + debugCount.ToString() + " took " + (stopwatchTimer.ElapsedMilliseconds).ToString() + " ms ");
    }
    public void displayGrid()
    {
        if (instanciateEnable)
        {
            for (int i = 0; i < Cols_size; i++)
            {
                for (int j = 0; j < Rows_size; j++)
                {
                    if (grid[i, j] != Vector3.zero)
                    {
                        GameObject resultInstance = Instantiate(objetInstance, grid[i, j], Quaternion.identity);
                        resultInstance.name = namingCount.ToString();
                        namingCount++;
                        instanciatedPoints.Add(resultInstance);
                    }
                }
            }
        }
    }
    public void displayPoint()
    {
        if (instanciateEnable)
        {
            Vector3 pt = newPos;
            GameObject resultInstance = Instantiate(objetInstance, pt, Quaternion.identity);
            resultInstance.name = namingCount.ToString();
            namingCount++;
            instanciatedPoints.Add(resultInstance);
        }
    }
    public void deleteComputed()
    {
        if (instanciatedPoints.Count > 0)
        {
            foreach (GameObject item in instanciatedPoints)
            {
                Destroy(item);
            }
            instanciatedPoints.Clear();
        }
    }
    public void computePointsHeight()
    {
        for (int i = 0; i < Cols_size; i++)
        {
            for (int j = 0; j < Rows_size; j++)
            {
                if (grid[i, j] != Vector3.zero)
                {
                    Vector3 temp = grid[i, j];
                    temp.y = GetComponent<PerlinNoiseGenerator>().perlinNoiseGeneratePoint(temp.x, temp.z, Range_x, Range_z, perlinScale) * scale_y;
                    grid[i, j] = temp;
                }
            }
        }
    }
    public Vector3Int floorVector3(Vector3 vec)
    {
        Vector3Int result;
        result = new Vector3Int((int)Math.Floor(vec.x / Cell_size), 0, (int)Math.Floor(vec.z / Cell_size));
        return result;
    }
    public Vector3[,] poissonGrid
    {
        get { return grid; }
    }
    public int getRowSize
    {
        get { return Rows_size; }
    }
    public int getColSize
    {
        get { return Cols_size; }
    }
}
