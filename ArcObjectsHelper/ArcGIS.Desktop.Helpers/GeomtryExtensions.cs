using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DEWA.GIS.Desktop.Helpers
{
    public static class GeomtryExtensions
    {
        /// <summary>
        /// Indicates if this geometry contains the other geometry.
        /// </summary>
        /// <param name="geo1"></param>
        /// <param name="geo2"></param>
        /// <returns></returns>
        public static bool Contains(this IGeometry geo1, IGeometry other)
        {
            IRelationalOperator relationalOperator = geo1 as IRelationalOperator;

            return relationalOperator.Contains(other);
        }

        /// <summary>
        /// Converts the given geometry to 2D geometry if it's not 2D otherwsie it will return the same input.
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public static IGeometry CreateFlatGeometry(IGeometry geometry)
        {
            //if (geometry.IsEmpty || geometry.Dimension == esriGeometryDimension.esriGeometry2Dimension)
            //    return geometry;

            IGeometry flatGeo = null;
            if (geometry.GeometryType == esriGeometryType.esriGeometryPoint)
            {
                IPoint point = geometry as IPoint;
                flatGeo = new Point() { X = point.X, Y = point.Y, SpatialReference = point.SpatialReference };
            }
            else if (geometry.GeometryType == esriGeometryType.esriGeometryPolyline)
            {
                Polyline newPolyline = new Polyline();
                ISegmentCollection newPolylineSegments = newPolyline as ISegmentCollection;
                ISegmentCollection segments = geometry as ISegmentCollection;
                newPolylineSegments.AddSegmentCollection(segments);

                flatGeo = newPolyline as IGeometry;
            }
            else if (geometry.GeometryType == esriGeometryType.esriGeometryPolygon)
            {
                Polygon newPolygon = new Polygon();
                ISegmentCollection newPolygonSegments = newPolygon as ISegmentCollection;
                ISegmentCollection segments = geometry as ISegmentCollection;
                newPolygonSegments.AddSegmentCollection(segments);

                flatGeo = newPolygon as IGeometry;
            }

            else if (geometry.GeometryType == esriGeometryType.esriGeometryEnvelope)
            {
                IEnvelope envelope = geometry as IEnvelope;

                Envelope newEnvelope = new Envelope
                {
                    XMin = envelope.XMin,
                    XMax = envelope.XMax,
                    YMin = envelope.YMin,
                    YMax = envelope.YMax
                };

                flatGeo = newEnvelope as IGeometry;
            }

            return flatGeo;
        }

        /// <summary>
        /// This does a spatial query within a search radius, then loops
        /// over features in that area, checking distance between the source geometry and each feature.
        /// </summary>
        /// <returns>KeyValuePair in which the key represents the OID of the feature and the value represents the distance to it.</returns>
        public static KeyValuePair<int, double> FindNearest(this IGeometry g, IFeatureClass fclass, double searchradius, string where = "")
        {
            IFeatureCursor fcursor = fclass.Search(g);

            KeyValuePair<int, double> returnval = FindNearest(g, fcursor);

            Marshal.FinalReleaseComObject(fcursor);

            return returnval;
        }

        /// <summary>
        /// Loops over rows in a FeatureCursor, measuring the distance from 
        /// the target Geometry to each feature in the FeatureCursor, to
        /// find the closest one.
        /// </summary>
        public static KeyValuePair<int, double> FindNearest(this IGeometry g, IFeatureCursor fcursor)
        {
            int closestoid = -1;
            double closestdistance = 0.0;
            IFeature feature = null;
            while ((feature = fcursor.NextFeature()) != null)
            {
                double distance = ((IProximityOperator)g).ReturnDistance(feature.Shape);

                if (closestoid == -1 || distance < closestdistance)
                {
                    closestoid = feature.OID;
                    closestdistance = distance;
                }
            }
            return new KeyValuePair<int, double>(closestoid, closestdistance);
        }

        /// <summary>
        /// Converts the given polygon into Envelope feature if it's 4 sides only, otherwise it will return the polygon's extent.
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public static IEnvelope ConvertPolygonToEnvelope(this IPolygon polygon)
        {
            if (polygon == null)
                return null;

            IEnvelope envelope = polygon.Envelope;

            IPointCollection pointCollection = polygon as IPointCollection;
            if (pointCollection.PointCount - 1 == 4)
            {
                envelope = new Envelope
                {
                    UpperLeft = pointCollection.Point[0],
                    LowerRight = pointCollection.Point[2]
                } as IEnvelope;
            }


            return envelope;
        }

    }
}
