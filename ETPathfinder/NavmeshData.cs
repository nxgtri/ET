using ETPathfinder.UnityEngine;
using ETPathfinder.PF;

namespace ETPathfinder
{
    public class NavmeshData
    {
        public static readonly float PositionComparePrecision = 1E-3f;

        NavGraph[] graphs;

        TileHandler walkableNavmeshHandler;

        static readonly bool fullGetNearestSearch = false;
        static readonly bool prioritizeGraphs = false;
        static readonly float prioritizeGraphsLimit = 1;

        public EuclideanEmbedding euclideanEmbedding;

        public NavmeshData(NavGraph[] navGraphs)
        {
            this.graphs = navGraphs;
            euclideanEmbedding = new EuclideanEmbedding(this);
            walkableNavmeshHandler = new TileHandler(graphs[0] as RecastGraph);
            walkableNavmeshHandler.CreateTileTypesFromGraph();
        }

        public NavmeshBase WalkableNavmesh
        {
            get
            {
                return graphs[0] as NavmeshBase;
            }
                
        }

        public NavmeshBase BlindNavmesh
        {
            get
            {
                return graphs[1] as NavmeshBase;
            }
        }

        public NavGraph[] Graphs
        {
            get
            {
                return graphs;
            }
        }

        public NNInfo GetNearestOnWalkableNavmesh(Vector3 position, NNConstraint constraint)
        {
            NNInfoInternal nearestNode = new NNInfoInternal();
            nearestNode = WalkableNavmesh.GetNearest(position, constraint);

            var diff = nearestNode.clampedPosition - position;
            var lengthSquaredXZ = LengthSquaredXZ(diff);
            if (lengthSquaredXZ > PositionComparePrecision)
            {
                return new NNInfo();
            }

            // Convert to NNInfo which doesn't have all the internal fields
            return new NNInfo(nearestNode);
        }

        float LengthSquaredXZ(Vector3 vector)
        {
            return (float)((double)vector.x * (double)vector.x + (double)vector.z * (double)vector.z);
        }
    }
}
