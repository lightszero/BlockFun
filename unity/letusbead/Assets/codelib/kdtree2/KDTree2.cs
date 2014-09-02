using System;
using System.Collections.Generic;

using System.Text;


public class KDTree2D
{
    public KDTree2D()
    {
    }
    public class Node
    {
        public Train point { get; set; }     //节点信息
        public Node leftNode { get; set; }   //左子树
        public Node righNode { get; set; }   //右子树
        public int split { get; set; }       //分割的方向轴序号
        //public Node parent { get; set; }     //父节点
        public List<Train> range { get; set; }   //空间节点
    }

    public class Train
    {
        public float positionX { get; set; }
        public float positionY { get; set; }
        //public float positionZ { get; set; }
        //public Int32 AvgRssi { set; get; }
    }

    public class PriorityList
    {
        public Node node { get; set; }
        public float priority { get; set; }
    }




    private List<PriorityList> priorityList = new List<PriorityList>(); //优先队列

    public Node CreatKDTree(List<Train> train)
    {
        //创建节点
        Node node = new Node();
        node.range = train;

        if (train.Count == 1)
        {
            //只有一个节点时即为叶节点，直接返回叶节点
            node.split = 0;     //默认为X方向轴分割
            node.point = train[0];
            node.leftNode = null;
            node.righNode = null;
            return node;
        }

        int axis = GetAxis(train);
        Train splitNode = GetSplitPoint(train, axis);

        train.Remove(splitNode);    //新的数据空间

        //获取左子树的数据空间,即比splitNode在axis方向上小的数据
        List<Train> leftTreeRange = this.LeftTreeRange(train, splitNode, axis);
        //获取右子树的数据空间,即比splitNode在axis方向上大的数据
        List<Train> rightTreeRange = this.RightTreeRange(train, splitNode, axis);


        node.split = axis;
        node.point = splitNode;

        //子树不为空则进行下一层递归
        if (leftTreeRange.Count == 0)
            node.leftNode = null;
        else
            node.leftNode = this.CreatKDTree(leftTreeRange);
        if (rightTreeRange.Count == 0)
            node.righNode = null;
        else
            node.righNode = this.CreatKDTree(rightTreeRange);

        return node;
    }

    //private void GetTrainCollection()
    //{
    //    lsTrain.Clear();
    //    //获取训练集
    //    string sql = string.Format("select positionX,positionY,AVG(rssi) as AvgRssi from FigRSSI,ApMana " +
    //                                "where ApMana.id=FigRSSI.apid and apname='0060B3139D94' " +
    //                                "group by positionX,positionY");
    //    SqlConnection connectioin = new SqlConnection();
    //    connectioin.ConnectionString = @"Data Source=202.200.119.166,1433\SQLEXPRESS;Initial Catalog=PositionSystem;User ID=sa;Password=sasasa";
    //    SqlCommand cmd = new SqlCommand();
    //    cmd.Connection = connectioin;
    //    cmd.CommandText = sql;
    //    connectioin.Open();
    //    SqlDataReader reader = cmd.ExecuteReader();
    //    while (reader.Read())
    //    {
    //        lsTrain.Add(new Train()
    //        {
    //            positionX = Int32.Parse(reader[0].ToString()),
    //            positionY = Int32.Parse(reader[1].ToString()),
    //            positionZ = 0,        //默认值
    //            AvgRssi = Int32.Parse(reader[2].ToString())
    //        });
    //    }

    //    reader.Close();
    //    connectioin.Close();
    //}




    private int GetAxis(List<Train> train)
    {
        //计算方差,选择方向轴

        int axis = -1;  //坐标轴,0表示X轴，1表示Y轴，2表示Z轴

        float xDemonAvg = 0, yDemonAvg = 0;//, zDemonAvg = 0;   //定义了三个个维度的平均值
        foreach (var tmp in train)
        {
            xDemonAvg += tmp.positionX * 1.0f;
            yDemonAvg += tmp.positionY * 1.0f;
            //zDemonAvg += tmp.positionZ * 1.0f;
        }
        //计算均值
        xDemonAvg = xDemonAvg / train.Count;
        yDemonAvg = yDemonAvg / train.Count;
        //zDemonAvg = zDemonAvg / train.Count;

        //计算方差
        double xS2 = 0, yS2 = 0;//, zS2 = 0;    //初始化三个轴的方差
        foreach (var tmp in train)
        {
            xS2 += Math.Pow(tmp.positionX - xDemonAvg, 2);
            yS2 += Math.Pow(tmp.positionY - yDemonAvg, 2);
            //zS2 += Math.Pow(tmp.positionZ - zDemonAvg, 2);
        }
        xS2 = xS2 / train.Count;
        yS2 = yS2 / train.Count;
        //zS2 = zS2 / train.Count;

        if (xS2 >= yS2)
        {
            axis = 0;
        }
        else
        {
            axis = 1;
        }


        return axis;
    }

    private Train GetSplitPoint(List<Train> train, int axis)
    {
        //根据方向轴排序,选取该方向轴上中间的点

        if (axis == 0) //X方向轴排序
        {
            return this.QuickSort(train, 0);
        }
        else if (1 == axis) //Y方向轴排序
        {
            return this.QuickSort(train, 1);
        }
        else  //Z方向轴排序
        {
            return this.QuickSort(train, 2);
        }
    }

    private Train QuickSort(List<Train> train, int axis)
    {
        if (0 == axis)
        {
            train.Sort(this.CompareTrainX);
        }
        else// if (1 == axis)
        {
            train.Sort(this.CompareTrainY);
        }
        //else
        //    train.Sort(this.CompareTrainZ);
        return train[train.Count / 2];
    }


    #region 定义比较器
    private int CompareTrainX(Train trainFirst, Train trainSecond)
    {
        if (trainFirst.positionX == trainSecond.positionX)
        {
            return 0;
        }
        else if (trainFirst.positionX < trainSecond.positionX)
        {
            return -1;
        }
        else
            return 1;
    }
    private int CompareTrainY(Train trainFirst, Train trainSecond)
    {
        if (trainFirst.positionY == trainSecond.positionY)
        {
            return 0;
        }
        else if (trainFirst.positionY < trainSecond.positionY)
        {
            return -1;
        }
        else
            return 1;
    }
    //private int CompareTrainZ(Train trainFirst, Train trainSecond)
    //{
    //    if (trainFirst.positionZ == trainSecond.positionZ)
    //    {
    //        return 0;
    //    }
    //    else if (trainFirst.positionZ < trainSecond.positionZ)
    //    {
    //        return -1;
    //    }
    //    else
    //        return 1;
    //}
    #endregion

    #region 获取左右子树数据空间

    private List<Train> LeftTreeRange(List<Train> train, Train splitNode, int axis)
    {
        List<Train> tempTrain = new List<Train>();

        if (0 == axis)
        {
            foreach (var tmp in train)
            {
                if (tmp.positionX <= splitNode.positionX)
                {
                    tempTrain.Add(tmp);
                }
            }
        }
        else// if (1 == axis)
        {
            foreach (var tmp in train)
            {
                if (tmp.positionY <= splitNode.positionY)
                {
                    tempTrain.Add(tmp);
                }
            }
        }
        //else
        //{
        //    foreach (var tmp in train)
        //    {
        //        if (tmp.positionZ <= splitNode.positionZ)
        //        {
        //            tempTrain.Add(tmp);
        //        }
        //    }
        //}
        return tempTrain;
    }

    private List<Train> RightTreeRange(List<Train> train, Train splitNode, int axis)
    {
        List<Train> tempTrain = new List<Train>();

        if (0 == axis)
        {
            foreach (var tmp in train)
            {
                if (tmp.positionX > splitNode.positionX)
                {
                    tempTrain.Add(tmp);
                }
            }
        }
        else// if (1 == axis)
        {
            foreach (var tmp in train)
            {
                if (tmp.positionY > splitNode.positionY)
                {
                    tempTrain.Add(tmp);
                }
            }
        }
        //else
        //{
        //    foreach (var tmp in train)
        //    {
        //        if (tmp.positionZ > splitNode.positionZ)
        //        {
        //            tempTrain.Add(tmp);
        //        }
        //    }
        //}
        return tempTrain;
    }

    #endregion

    public Node KDTreeFindNearest(Node tree, Train target)
    {
        priorityList.Clear();       //清空队列
        double dist = double.MaxValue; //最近邻和目标点之间的距离
        Node nearest = null;    //最近邻节点
        Node searchNode = tree; //根节点
        List<Node> searchPath = new List<Node>();   //搜索路径
        #region 检索搜索路径
        while (searchNode != null)
        {
            searchPath.Add(searchNode); //添加当前搜索节点

            if (searchNode.split == 0)
            {
                //X方向轴分割
                if (target.positionX <= searchNode.point.positionX)
                {
                    searchNode = searchNode.leftNode;
                }
                else
                    searchNode = searchNode.righNode;
            }
            else// if (searchNode.split == 1)
            {
                //Y方向轴分割
                if (target.positionY <= searchNode.point.positionY)
                {
                    searchNode = searchNode.leftNode;
                }
                else
                    searchNode = searchNode.righNode;
            }
            //else
            //{
            //    if (target.positionZ <= searchNode.point.positionZ)
            //    {
            //        searchNode = searchNode.leftNode;
            //    }
            //    else
            //        searchNode = searchNode.righNode;
            //}
        }
        #endregion

        nearest = searchPath[searchPath.Count - 1]; //获取搜索路径最后一个节点
        searchPath.Remove(nearest); //从搜索路径中删除假定的最近邻节点

        //计算两个节点之间的距离
        dist = this.Distance(nearest.point, target);

        #region 回溯路径搜索

        Node backNode;
        int split = -1;
        while (searchPath.Count > 0)
        {
            backNode = searchPath[searchPath.Count - 1];    //取搜索路径最后一个
            if(backNode==null)
            {
                searchPath.RemoveAt(searchPath.Count - 1); continue;
            }
            if (backNode.leftNode == null && backNode.righNode == null)
            {
                //该节点为叶子节点
                if (Distance(nearest.point, target) > Distance(backNode.point, target))
                {
                    nearest = backNode;
                    dist = Distance(backNode.point, target);
                }
            }
            else
            {
                split = backNode.split;     //取出当前的分割方向轴
                if (split == 0)
                {
                    if (Distance(backNode.point, target) < dist)
                    {
                        nearest = backNode;
                        dist = Distance(backNode.point, target);
                    }

                    //确定是否进入子空间搜索
                    float disTmp = Math.Abs(target.positionX - backNode.point.positionX);
                    if (disTmp < dist)
                    {
                        //判断目标点是在左子空间还是右子空间
                        if (target.positionX > backNode.point.positionX)
                        {
                            //目标点在右子空间，则需要进入左子空间进行搜索
                            searchNode = backNode.leftNode;
                        }
                        else
                            searchNode = backNode.righNode;

                        searchPath.Add(searchNode);     //添加搜索节点
                    }

                }
                else// if (split == 1)
                {
                    if (Distance(backNode.point, target) < dist)
                    {
                        nearest = backNode;
                        dist = Distance(backNode.point, target);
                    }

                    //确定是否进入子空间搜索
                    float disTmp = Math.Abs(target.positionY - backNode.point.positionY);
                    if (disTmp < dist)
                    {
                        //判断目标点是在左子空间还是右子空间
                        if (target.positionY > backNode.point.positionY)
                        {
                            //目标点在右子空间，则需要进入左子空间进行搜索
                            searchNode = backNode.leftNode;
                        }
                        else
                            searchNode = backNode.righNode;

                        searchPath.Add(searchNode);     //添加搜索节点
                    }
                }
                //else
                //{
                //    if (Distance(backNode.point, target) < dist)
                //    {
                //        nearest = backNode;
                //        dist = Distance(backNode.point, target);
                //    }

                //    //确定是否进入子空间搜索
                //    float disTmp = Math.Abs(target.positionZ - backNode.point.positionZ);
                //    if (disTmp < dist)
                //    {
                //        //判断目标点是在左子空间还是右子空间
                //        if (target.positionZ > backNode.point.positionZ)
                //        {
                //            //目标点在右子空间，则需要进入左子空间进行搜索
                //            searchNode = backNode.leftNode;
                //        }
                //        else
                //            searchNode = backNode.righNode;

                //        searchPath.Add(searchNode);     //添加搜索节点
                //    }
                //}
            }

            searchPath.Remove(backNode);
        }

        #endregion

        return nearest;     //返回最近邻节点
    }

    public Node BBFFindNearest(Node tree, Train target)
    {
        Node nearest = new Node();        //最近邻节点
        nearest = tree;

        float priority = -1; //优先级
        //计算优先级
        priority = this.CalPriority(nearest, target, tree.split);
        this.InsertPriorityList(nearest, priority);     //将根节点加入优先队列中

        Node topNode = null;    //优先级最高的节点
        Node currentNode = null;
        double dist = 0;
        while (priorityList.Count > 0)
        {
            topNode = priorityList[0].node; //优先队列中的第一个总是优先级最高的
            this.RmovePriority(topNode);

            while (topNode != null)
            {
                //当前节点不是叶子节点
                if (topNode.leftNode != null || topNode.righNode != null)
                {
                    int split = topNode.split;
                    if (split == 0)
                    {
                        if (target.positionX <= topNode.point.positionX)
                        {
                            //current = topNode.point;
                            //若右节点不为空，将右子树节点添加到优先队列中
                            if (topNode.righNode != null)
                            {
                                priority = this.CalPriority(topNode.righNode, target, split);
                                this.InsertPriorityList(topNode.righNode, priority);
                            }

                            topNode = topNode.leftNode;
                        }
                        else
                        {
                            //current = topNode.point;
                            //将左子树节点添加到优先级队列中
                            if (topNode.leftNode != null)
                            {
                                priority = this.CalPriority(topNode.leftNode, target, split);
                                this.InsertPriorityList(topNode.leftNode, priority);
                            }

                            topNode = topNode.righNode;
                        }

                        currentNode = topNode;
                    }
                    else// if (split == 1)
                    {
                        if (target.positionY <= topNode.point.positionY)
                        {
                            //current = topNode.point;
                            //将右子树节点添加到优先队列中
                            if (topNode.righNode != null)
                            {
                                priority = this.CalPriority(topNode.righNode, target, split);
                                this.InsertPriorityList(topNode.righNode, priority);
                            }

                            topNode = topNode.leftNode;
                        }
                        else
                        {
                            //current = topNode.point;
                            //将左子树节点添加到优先级队列中
                            if (topNode.leftNode != null)
                            {
                                priority = this.CalPriority(topNode.leftNode, target, split);
                                this.InsertPriorityList(topNode.leftNode, priority);
                            }

                            topNode = topNode.righNode;
                        }

                        currentNode = topNode;
                    }
                    //else
                    //{
                    //    if (target.positionZ <= topNode.point.positionZ)
                    //    {
                    //        //current = topNode.point;
                    //        //将右子树节点添加到优先队列中
                    //        if (topNode.righNode != null)
                    //        {
                    //            priority = this.CalPriority(topNode.righNode, target, split);
                    //            this.InsertPriorityList(topNode.righNode, priority);
                    //        }

                    //        topNode = topNode.leftNode;
                    //    }
                    //    else
                    //    {
                    //        //current = topNode.point;
                    //        //将左子树节点添加到优先级队列中
                    //        if (topNode.leftNode != null)
                    //        {
                    //            priority = this.CalPriority(topNode.leftNode, target, split);
                    //            this.InsertPriorityList(topNode.leftNode, priority);
                    //        }

                    //        topNode = topNode.righNode;
                    //    }
                    //    currentNode = topNode;
                    //}
                }
                else
                {
                    currentNode = topNode;
                    topNode = null;     //叶子节点
                }

                if (currentNode != null && Distance(nearest.point, target) > Distance(currentNode.point, target))
                {
                    nearest = currentNode;
                    dist = Distance(currentNode.point, target);
                }
            }
        }
        return nearest;
    }



    private double Distance(Train trainFirst, Train trainSecond)
    {
        double tmp = double.MaxValue;     //初始化节点间距离为无穷大
        tmp = Math.Sqrt(Math.Pow(trainFirst.positionX - trainSecond.positionX, 2) +
                        Math.Pow(trainFirst.positionY - trainSecond.positionY, 2)); //+
                        //Math.Pow(trainFirst.positionZ - trainSecond.positionZ, 2));

        return tmp;
    }

    #region 优先队列的添加和删除
    private void InsertPriorityList(Node node, float priority)
    {
        PriorityList pl = new PriorityList() { node = node, priority = priority };
        if (priorityList.Count == 0)
        {
            priorityList.Insert(0, pl);
            return;
        }

        for (int i = 0; i < priorityList.Count; i++)
        {
            if (priorityList[i].priority >= priority)
            {
                priorityList.Insert(i, pl);
                break;
            }
        }
    }

    private void RmovePriority(Node node)
    {
        foreach (var tmp in priorityList)
        {
            if (tmp.node == node)
            {
                priorityList.Remove(tmp);
                break;
            }
        }
    }
    #endregion

    private float CalPriority(Node node, Train target, int split)
    {
        //计算目标点和分割点之间的距离，即优先级
        if (split == 0)
        {
            return Math.Abs(target.positionX - node.point.positionX);
        }
        else// if (split == 1)
        {
            return Math.Abs(target.positionY - node.point.positionY);
        }
        //else
        //    return Math.Abs(target.positionZ - node.point.positionZ);
    }
}

