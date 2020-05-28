using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

public class FindClosePath
{
    public static List<System.Drawing.Point> FindPath(int[,] field, System.Drawing.Point start, System.Drawing.Point goal)
    {
        return PathNode.FindPath(field, start, goal);
    }

    public class PathNode
    {
        // Координаты точки на карте.
        public System.Drawing.Point Position { get; set; }
        // Длина пути от старта (G).
        public int PathLengthFromStart { get; set; }
        // Точка, из которой пришли в эту точку.
        public PathNode CameFrom { get; set; }
        // Примерное расстояние до цели (H).
        public int HeuristicEstimatePathLength { get; set; }
        // Ожидаемое полное расстояние до цели (F).
        public int EstimateFullPathLength
        {
            get
            {
                return this.PathLengthFromStart + this.HeuristicEstimatePathLength;
            }
        }

        public static List<System.Drawing.Point> FindPath(int[,] field, System.Drawing.Point start, System.Drawing.Point goal)
        {
            // Шаг 1.
            var closedSet = new Collection<PathNode>();
            var openSet = new Collection<PathNode>();
            // Шаг 2.
            PathNode startNode = new PathNode()
            {
                Position = start,
                CameFrom = null,
                PathLengthFromStart = 0,
                HeuristicEstimatePathLength = GetHeuristicPathLength(start, goal)
            };
            openSet.Add(startNode);
            while (openSet.Count > 0)
            {
                // Шаг 3.
                var currentNode = openSet.OrderBy(node =>
                  node.EstimateFullPathLength).First(); //МОЖНО ПОПРОБОВАТЬ ЗАМЕНИТЬ НА ПОИСК МИНИМУМА
                // Шаг 4.
                if (currentNode.Position == goal)
                    return GetPathForNode(currentNode);
                // Шаг 5.
                openSet.Remove(currentNode);
                closedSet.Add(currentNode);
                // Шаг 6.
                foreach (var neighbourNode in GetNeighbours(currentNode, goal, field))
                {
                    // Шаг 7.
                    if (closedSet.Count(node => node.Position == neighbourNode.Position) > 0)
                        continue;
                    var openNode = openSet.FirstOrDefault(node =>
                      node.Position == neighbourNode.Position);
                    // Шаг 8.
                    if (openNode == null)
                        openSet.Add(neighbourNode);
                    else
                      if (openNode.PathLengthFromStart > neighbourNode.PathLengthFromStart)
                    {
                        // Шаг 9.
                        openNode.CameFrom = currentNode;
                        openNode.PathLengthFromStart = neighbourNode.PathLengthFromStart;
                    }
                }
            }
            // Шаг 10.
            return null;
        }

        private static int GetDistanceBetweenNeighbours()
        {
            return 1;
        }

        private static int GetHeuristicPathLength(System.Drawing.Point from, System.Drawing.Point to)
        {
            return Math.Abs(from.X - to.X) + Math.Abs(from.Y - to.Y);
        }

        private static Collection<PathNode> GetNeighbours(PathNode pathNode,
  System.Drawing.Point goal, int[,] field)
        {
            var result = new Collection<PathNode>();

            // Соседними точками являются соседние по стороне клетки.
            System.Drawing.Point[] neighbourPoints = new System.Drawing.Point[4];
            neighbourPoints[0] = new System.Drawing.Point(pathNode.Position.X + 1, pathNode.Position.Y);
            neighbourPoints[1] = new System.Drawing.Point(pathNode.Position.X - 1, pathNode.Position.Y);
            neighbourPoints[2] = new System.Drawing.Point(pathNode.Position.X, pathNode.Position.Y + 1);
            neighbourPoints[3] = new System.Drawing.Point(pathNode.Position.X, pathNode.Position.Y - 1);

            foreach (var point in neighbourPoints)
            {
                // Проверяем, что не вышли за границы карты.
                if (point.X < 0 || point.X >= field.GetLength(0))
                    continue;
                if (point.Y < 0 || point.Y >= field.GetLength(1))
                    continue;
                // Проверяем, что по клетке можно ходить.
                if ((field[point.X, point.Y] != 0))
                    continue;
                // Заполняем данные для точки маршрута.
                var neighbourNode = new PathNode()
                {
                    Position = point,
                    CameFrom = pathNode,
                    PathLengthFromStart = pathNode.PathLengthFromStart +
                    GetDistanceBetweenNeighbours(),
                    HeuristicEstimatePathLength = GetHeuristicPathLength(point, goal)
                };
                result.Add(neighbourNode);
            }
            return result;
        }

        private static List<System.Drawing.Point> GetPathForNode(PathNode pathNode)
        {
            var result = new List<System.Drawing.Point>();
            var currentNode = pathNode;
            while (currentNode != null)
            {
                result.Add(currentNode.Position);
                currentNode = currentNode.CameFrom;
            }
            result.Reverse();
            return result;
        }
    }
}
