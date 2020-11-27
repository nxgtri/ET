using Pathfinding.ClipperLib;
using System.Collections.Generic;
using ETPathfinder.UnityEngine;

namespace ETPathfinder.PF
{
    public class NavmeshCut
    {
        public NavmeshCut(Vector3 center, Vector2 size)
        {
            this.type = MeshType.Rectangle;
            this.center = center;
            this.rectangleSize = size;
        }

        public NavmeshCut(Vector3 center, float radius)
        {
            this.type = MeshType.Circle;
            this.center = center;
            this.circleRadius = radius;
        }

        public NavmeshCut(List<Vector3> points)
        {
            this.type = MeshType.Points;
            this.Points = points;
        }

        public enum MeshType
        {
            Rectangle,
            Circle,
            Points,
        }

       
        public MeshType type;

        /** Size of the rectangle */
        public Vector2 rectangleSize = new Vector2(1, 1);

        /** Radius of the circle */
        public float circleRadius = 1;

        /** Number of vertices on the circle */
        public int circleResolution = 6;
        public float height = 1;

        public Vector3 center;

        public Vector3 position;

        public List<Vector3> Points;

        /** Only makes a split in the navmesh, but does not remove the geometry to make a hole.
		 * This is slower than a normal cut
		 */
        public bool isDual;

        /** Cuts geometry added by a NavmeshAdd component.
		 * You rarely need to change this
		 */
        public bool cutsAddedGeom = true;

        /** Cached variable, to avoid allocations */
        static readonly Dictionary<Int2, int> edges = new Dictionary<Int2, int>();
        /** Cached variable, to avoid allocations */
        static readonly Dictionary<int, int> pointers = new Dictionary<int, int>();

        
        /** World space bounds of this cut */
        public Bounds GetBounds()
        {
            var bounds = new Bounds();

            switch (type)
            {
                case MeshType.Rectangle:
                    bounds = new Bounds(center + position, new Vector3(rectangleSize.x, height, rectangleSize.y));
                    break;
                case MeshType.Circle:
                    bounds = new Bounds(center + position, new Vector3(circleRadius * 2, height, circleRadius * 2));
                    break;
                case MeshType.Points:
                    foreach(var point in Points)
                    {
                        bounds.Encapsulate(new Bounds(position + point, new Vector3(0, height, 0)));
                    }
                    break;

                default:
                    throw new System.Exception("Invalid mesh type");
            }
            return bounds;
        }

        public void SetPositionXZ(Vector3 position)
        {
            position.y = 0;
            this.position = position;
        }

        /**
		 * World space contour of the navmesh cut.
		 * Fills the specified buffer with all contours.
		 * The cut may contain several contours which is why the buffer is a list of lists.
		 */
        public void GetContour(List<List<IntPoint>> buffer)
        {
            if (circleResolution < 3)
                circleResolution = 3;

            switch (type)
            {
                case MeshType.Rectangle:
                    List<IntPoint> buffer0 = ListPool<IntPoint>.Claim();

                    buffer0.Add(V3ToIntPoint(position + center + new Vector3(-rectangleSize.x, 0, -rectangleSize.y) * 0.5f));
                    buffer0.Add(V3ToIntPoint(position + center + new Vector3(rectangleSize.x, 0, -rectangleSize.y) * 0.5f));
                    buffer0.Add(V3ToIntPoint(position + center + new Vector3(rectangleSize.x, 0, rectangleSize.y) * 0.5f));
                    buffer0.Add(V3ToIntPoint(position + center + new Vector3(-rectangleSize.x, 0, rectangleSize.y) * 0.5f));
                    
                    buffer.Add(buffer0);
                    break;
                case MeshType.Circle:
                    buffer0 = ListPool<IntPoint>.Claim(circleResolution);
                    for (int i = 0; i < circleResolution; i++)
                    {
                        buffer0.Add(V3ToIntPoint(position + center + new Vector3(Mathf.Cos((i * 2 * MathHelper.Pi) / circleResolution), 0, Mathf.Sin((i * 2 * MathHelper.Pi) / circleResolution)) * circleRadius));}
                    buffer.Add(buffer0);
                    break;
                case MeshType.Points:
                    buffer0 = ListPool<IntPoint>.Claim(Points.Count);
                    foreach(var point in Points)
                    {
                        buffer0.Add(V3ToIntPoint(position + point));
                    }
                    buffer.Add(buffer0);
                    break;
            }
        }

        /** Converts a Vector3 to an IntPoint.
		 * This is a lossy conversion.
		 */
        public static IntPoint V3ToIntPoint(Vector3 p)
        {
            var ip = (Int3)p;

            return new IntPoint(ip.x, ip.z);
        }

        /** Converts an IntPoint to a Vector3.
		 * This is a lossy conversion.
		 */
        public static Vector3 IntPointToV3(IntPoint p)
        {
            var ip = new Int3((int)p.X, 0, (int)p.Y);

            return (Vector3)ip;
        }
    }
}
