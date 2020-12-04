using ETPathfinder.PF;
using ETPathfinder.UnityEngine;
using System.Collections.Generic;

namespace ETPathfinder
{
    public class Pathfinder : INodeIndexGenerator
    {
        readonly PathProcessor pathProcessor;
        readonly PathReturnQueue pathReturnQueue;
        readonly PathfinderConfig config;

        public Pathfinder(RecastGraph graph)
        {
            config = new PathfinderConfig(graph);

            foreach (var tile in graph.GetTiles())
            {
                foreach (var node in tile.nodes)
                {
                    var nodeIndex = GetNewNodeIndex();
                    node.SetPathNodeIndex(nodeIndex);
                }
            }

            graph.GetNewNodeIndex = GetNewNodeIndex;

            pathReturnQueue = new PathReturnQueue(this);
            pathProcessor = new PathProcessor(pathReturnQueue, 1, false);
            pathProcessor.InitializeNode(graph);
            graph.OnDestroyNode += OnDestroyNode;
            graph.OnRecalculatedTiles += OnRecalculatedTiles;
        }

        void OnDestroyNode(GraphNode node)
        {
            if (node.NodeIndex == -1)
            {
                return;
            }

            PushNodeIndex(node.NodeIndex);
            pathProcessor.DestroyNode(node);
            node.Destroy();
        }

        void OnRecalculatedTiles(List<NavmeshTile> tiles)
        {
            foreach (var each in tiles)
            {
                pathProcessor.InitializeNode(each);
            }
        }

        bool Search(ABPath path)
        {
            this.pathProcessor.queue.Push(path);

            while (this.pathProcessor.CalculatePaths().MoveNext())
            {
                if (path.CompleteState != PathCompleteState.NotCalculated)
                {
                    // NOTE 200515 @hayandev : Claim이 무조건 불려야 PathReturnQueue.ReturnPaths가 동작합니다.
                    // Claim의 인자는 PathReturnQueue의 생성자에 넣은 인스턴스와 같아야 합니다.
                    path.Claim(this);
                    break;
                }
            }

            if (path.CompleteState != PathCompleteState.Complete)
            {
                return false;
            }

            PathModifyHelper.StartEndModify((ABPath)path);
            PathModifyHelper.FunnelModify(path);

            return true;
        }

        public List<Vector3> FindPath(Vector3 from, Vector3 to)
        {
            ABPath path;
            path = ABPath.Construct(config, from, to);

            var result = new List<Vector3>();

            if (Search(path))
            {
                result = path.vectorPath;
            }

            // NOTE 200515 @hayandev : ReturnPaths가 Path 목록을 반환하는게 아니라 자원을 해제합니다.
            // 안하면 메모리 릭이 일어납니다.
            pathReturnQueue.ReturnPaths(false);
            return result;
        }

        int nextNodeIndex = 0;
        readonly Stack<int> nodeIndexPool = new Stack<int>();
        public int GetNewNodeIndex()
        {
            return nodeIndexPool.Count > 0 ? nodeIndexPool.Pop() : nextNodeIndex++;
        }

        public void PushNodeIndex(int index)
        {
            nodeIndexPool.Push(index);
        }
    }
}
