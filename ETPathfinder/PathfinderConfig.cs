using ETPathfinder.UnityEngine;
using ETPathfinder.PF;

namespace ETPathfinder
{
    public class PathfinderConfig
    {
        public static readonly float PositionComparePrecision = 1E-3f;

        readonly RecastGraph graph;

        readonly TileHandler tileHandler;

        static readonly bool fullGetNearestSearch = false;
        static readonly bool prioritizeGraphs = false;
        static readonly float prioritizeGraphsLimit = 1;

        public EuclideanEmbedding euclideanEmbedding;

        public PathfinderConfig(RecastGraph graph)
        {
            this.graph = graph;
            euclideanEmbedding = new EuclideanEmbedding(new NavGraph[] { graph });
            tileHandler = new TileHandler(graph);
            tileHandler.CreateTileTypesFromGraph();
        }

        public NNInfo GetNearest(Vector3 position, NNConstraint constraint)
        {
            var nearestNode = graph.GetNearest(position, constraint);

            if ((nearestNode.clampedPosition - position).sqrMagnitude > PositionComparePrecision)
            {
                return new NNInfo();
            }

            // Convert to NNInfo which doesn't have all the internal fields
            return new NNInfo(nearestNode);
        }
    }
}
