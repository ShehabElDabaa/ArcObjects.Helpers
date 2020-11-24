using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;

namespace DEWA.GIS.Desktop.Helpers
{
    public static class FeatureExtensions
    {
        /// <summary>
        /// Finds the nearest feature from the given featureclass.
        /// </summary>
        /// <param name="feature"></param>
        /// <param name="featureClass"></param>
        /// <returns></returns>
        public static IFeature NearestLineFeature(this IFeature feature, IFeatureClass featureClass)
        {
            IFeatureCursor cursor = null;
            IFeature nearestFeature = null;
            try
            {
                var proximityOperator = (IProximityOperator)feature.Shape;

                cursor = featureClass.Search(null, false);

                double shortestDist = 0.0;

                IFeature destFeature;
                while ((destFeature = cursor.NextFeature()) != null)
                {
                    double distance = proximityOperator.ReturnDistance(destFeature.Shape);

                    if (distance < shortestDist)
                    {
                        nearestFeature = destFeature;
                        shortestDist = distance;
                    }
                }
            }
            finally
            {
                if (cursor != null)
                    Marshal.ReleaseComObject(cursor);
            }

            return nearestFeature;
        }

        /// <summary>
        /// The featureclass of the current feature.
        /// </summary>
        /// <param name="feature"></param>
        /// <returns></returns>
        public static IFeatureClass FeatureClass(this IFeature feature)
        {
            return feature?.Class as IFeatureClass;
        }

        /// <summary>
        /// Indicates if the shape has changed.
        /// </summary>
        /// <param name="feature"></param>
        /// <returns></returns>
        public static bool ShapeChanged(this IFeature feature)
        {
            IFeatureChanges rowChanges = feature as IFeatureChanges;
            return rowChanges.ShapeChanged;
        }

        /// <summary>
        /// Indicate if the given field is changed.
        /// </summary>
        /// <param name="feature"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static bool FieldChanged(this IFeature feature, string fieldName)
        {
            IRowChanges rowChanges = feature as IRowChanges;
            string Oldval = rowChanges.OriginalValue[feature.Fields.FindField(fieldName)].ToString();
            string newval = feature.Value[feature.Fields.FindField(fieldName)].ToString();
            if (Oldval.Trim() != newval.Trim())
                return true;

            return false;
        }

    }
}
