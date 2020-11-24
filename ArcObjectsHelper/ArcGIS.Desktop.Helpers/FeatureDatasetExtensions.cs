using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace DEWA.GIS.Desktop.Helpers
{
    public static class FeatureDatasetExtensions
    {
        /// <summary>
        /// Finds all featureclasses inside the given featuredataset with the given geometry type.
        /// </summary>
        /// <param name="featureDataset">The feature dataset.</param>
        /// <param name="geoType">The geometry type used to filter the featureclasses. you may pass Any to return all types of featureclasses.</param>
        /// <returns></returns>
        public static IEnumerable<IFeatureClass> OpenFeatureClasses(this IFeatureDataset featureDataset, esriGeometryType geoType)
        {
            IList<IFeatureClass> featureClasses = new List<IFeatureClass>();

            IEnumDataset subSets = featureDataset.Subsets;
            while (subSets.Next() is IFeatureClass featureClass)
            {
                if (featureClass.ShapeType == geoType || geoType == esriGeometryType.esriGeometryAny)
                    featureClasses.Add(featureClass);
            }

            return featureClasses;
        }

        /// <summary>
        /// Finds all featureclasses inside the given featuredataset with the given geometry type.
        /// </summary>
        /// <param name="featureDataset">The feature dataset.</param>
        /// <param name="geoType">The geometry type used to filter the featureclasses. you may pass Any to return all types of featureclasses.</param>
        /// <returns></returns>
        public static IEnumerable<IFeatureClass> OpenFeatureClasses(this IFeatureDataset featureDataset, params esriGeometryType[] geoTypes)
        {
            IList<IFeatureClass> featureClasses = new List<IFeatureClass>();

            IEnumDataset subSets = featureDataset.Subsets;
            while (subSets.Next() is IFeatureClass featureClass)
            {
                if (geoTypes.Contains(featureClass.ShapeType))
                    featureClasses.Add(featureClass);
            }

            return featureClasses;
        }

    }
}
