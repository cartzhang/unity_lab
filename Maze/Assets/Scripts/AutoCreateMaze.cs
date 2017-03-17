using UnityEngine;
using System.Collections;


using System.Collections.Generic;


public class AutoCreateMaze : MonoBehaviour
{
    List<Wall> SearchList;
    int x = 20;
    int y;
    public GameObject Wall;
    WallNode[,] NodeList;
    Wall[,] WallListi;
    Wall[,] WallListj;
    void Start()
    {
        SearchList = new List<Wall>();
        y = x - 1;
        NodeList = new WallNode[x, x];
        WallListi = new Wall[y, x];
        WallListj = new Wall[x, y];
        for (int i = 0; i < x; i++)
            for (int j = 0; j < x; j++)
            {
                Vector3 position = new Vector3(i, 0, j);
                WallNode node = new WallNode();
                node.WallLsit = new List<Wall>();
                node.Position = position;
                NodeList[i, j] = node;
            }
        for (int i = 0; i < y; i++)
            for (int j = 0; j < x; j++)
            {
                GameObject Walli = Instantiate(Wall) as GameObject;
                Walli.transform.eulerAngles = new Vector3(0, 90, 0);
                Walli.transform.position = NodeList[i, j].Position + new Vector3(0.5f, 0, 0);
                Walli.name = ("i" + i + " " + j);
                WallListi[i, j] = new Wall();
                WallListi[i, j].wall = Walli;
                WallListi[i, j].NodeList = new List<WallNode>();
                WallListi[i, j].NodeList.Add(NodeList[i, j]);
                WallListi[i, j].NodeList.Add(NodeList[i + 1, j]);
            }
        for (int i = 0; i < x; i++)
            for (int j = 0; j < y; j++)
            {
                GameObject Wallj = Instantiate(Wall) as GameObject;
                Wallj.transform.position = NodeList[i, j].Position + new Vector3(0, 0, 0.5f);
                Wallj.name = ("j" + i + " " + j);
                WallListj[i, j] = new Wall();
                WallListj[i, j].wall = Wallj;
                WallListj[i, j].NodeList = new List<WallNode>();
                WallListj[i, j].NodeList.Add(NodeList[i, j]);
                WallListj[i, j].NodeList.Add(NodeList[i, j + 1]);
            }
        for (int i = 0; i < x; i++)
            for (int j = 0; j < x; j++)
            {
                if (i != x - 1)
                {
                    NodeList[i, j].WallLsit.Add(WallListi[i, j]);
                }
                if (j != x - 1)
                {
                    NodeList[i, j].WallLsit.Add(WallListj[i, j]);
                }
                if (i != 0)
                {
                    NodeList[i, j].WallLsit.Add(WallListi[i - 1, j]);
                }
                if (j != 0)
                {
                    NodeList[i, j].WallLsit.Add(WallListj[i, j - 1]);
                }
                //Debug.Log(i + " " + j + " " + NodeList[i, j].WallLsit.Count);
            }
        NodeList[1, 1].isConnect = true;
        for (int i = 0; i < NodeList[1, 1].WallLsit.Count; i++)
        {
            SearchList.Add(NodeList[1, 1].WallLsit[i]);
        }
        MigongCreate();
        for (int i = 0; i < x; i++)
        {
            GameObject WallStartic1 = Instantiate(Wall) as GameObject;
            GameObject WallStartic2 = Instantiate(Wall) as GameObject;
            WallStartic1.name = "WallStartic";
            WallStartic2.name = "WallStartic";
            WallStartic1.transform.position = NodeList[i, 0].Position + new Vector3(0, 0, -0.5f);
            WallStartic2.transform.position = NodeList[i, x - 1].Position + new Vector3(0, 0, 0.5f);
        }
        for (int i = 0; i < x; i++)
        {
            GameObject WallStartic1 = Instantiate(Wall) as GameObject;
            GameObject WallStartic2 = Instantiate(Wall) as GameObject;
            WallStartic1.transform.eulerAngles = new Vector3(0, 90, 0);
            WallStartic2.transform.eulerAngles = new Vector3(0, 90, 0);
            WallStartic1.name = "WallStartic";
            WallStartic2.name = "WallStartic";
            WallStartic1.transform.position = NodeList[0, i].Position + new Vector3(-0.5f, 0, 0);
            WallStartic2.transform.position = NodeList[x - 1, i].Position + new Vector3(0.5f, 0, 0);
        }
    }


    void MigongCreate()
    {
        if (SearchList.Count <= 0)
            return;
        int current = Random.Range(0, SearchList.Count);
        if (SearchList[current].NodeList[0].isConnect && SearchList[current].NodeList[1].isConnect)
        {
            SearchList[current].isCanSearchAgain = false;
            SearchList.RemoveAt(current);
            //Destroy(SearchList[current].wall);
        }


        else
        {
            for (int i = 0; i < 2; i++)
            {
                if (!SearchList[current].NodeList[i].isConnect)
                {
                    SearchList[current].NodeList[i].isConnect = true;
                    SearchList[current].isCanSearchAgain = false;
                    for (int t = 0; t < SearchList[current].NodeList[i].WallLsit.Count; t++)
                    {
                        if (SearchList[current].NodeList[i].WallLsit[t].isCanSearchAgain)
                        {
                            SearchList.Add(SearchList[current].NodeList[i].WallLsit[t]);
                        }
                    }
                    Destroy(SearchList[current].wall);
                    SearchList.RemoveAt(current);
                }
            }
        }
        if (SearchList.Count > 0)
        {
            MigongCreate();
        }
    }


    // Update is called once per frame
    void Update()
    {

    }
}




class WallNode
{
    public Vector3 Position;
    public bool isConnect;
    public List<Wall> WallLsit;
    public WallNode()
    {
        isConnect = false;
    }
}


class Wall
{
    public GameObject wall;
    public List<WallNode> NodeList;
    public bool isCanSearchAgain;
    public Wall()
    {
        isCanSearchAgain = true;
    }
}