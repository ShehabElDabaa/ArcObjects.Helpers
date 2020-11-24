using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DEWA.GIS.Desktop.Helpers
{
    public static class WorkspaceExtensions
    {
        public static IFeatureDataset OpenFeatureDataset(this IWorkspace workspace, string datasetName)
        {
            var featWorkspace = workspace as IFeatureWorkspace;

            return featWorkspace?.OpenFeatureDataset(datasetName);
        }


        /// <summary>
        /// Gets the extent of the workspace featureclasses unioned if featureclass has data.
        /// </summary>
        /// <param name="workspace">Workspace.</param>
        /// <param name="featureDatasetName">Featuredataset name </param>
        /// <returns></returns>
        public static IEnvelope FindExtent(this IWorkspace workspace, string featureDatasetName = null)
        {
            if (workspace == null)
                return null;

            IEnvelope extent = null;

            IEnumDataset featureClasses = null;

            if (string.IsNullOrWhiteSpace(featureDatasetName))
                featureClasses = workspace.Datasets[esriDatasetType.esriDTFeatureClass];
            else
                featureClasses = workspace.OpenFeatureDataset(featureDatasetName)?.Subsets;

            featureClasses.Reset();

            IDataset fcDataset = null;
            while ((fcDataset = featureClasses.Next()) != null)
            {
                IGeoDataset geodataSet = fcDataset as IGeoDataset;
                IFeatureClass featureClass = fcDataset as IFeatureClass;

                if (featureClass.FeatureCount(null) > 0 && geodataSet != null)
                {
                    if (extent == null)
                        extent = geodataSet.Extent;
                    else
                        extent.Union(geodataSet.Extent);
                }
            }

            extent?.Expand(1.25, 1.25, true);

            Marshal.ReleaseComObject(featureClasses);

            return extent;
        }

        /// <summary>
        /// Finds the ObjectClassID of the given featureclass name.
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="featureclassName"></param>
        /// <returns></returns>
        public static int GetObjectClassID(this IWorkspace workspace, string featureclassName)
        {
            IFeatureClass fclass = (workspace as IFeatureWorkspace)?.OpenFeatureClass(featureclassName);
            int id = fclass.ObjectClassID;
            Marshal.ReleaseComObject(fclass);
            return id;
        }

        /// <summary>
        /// Finds the ObjectClassID of the given featureclass name.
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="featureclassName"></param>
        /// <returns></returns>
        public static int GetObjectClassID(this IFeatureWorkspace workspace, string featureclassName)
        {
            IFeatureClass fclass = workspace.OpenFeatureClass(featureclassName);
            int id = fclass.ObjectClassID;
            Marshal.ReleaseComObject(fclass);
            return id;
        }

        /// <summary>
        /// Finds feature classes names from the given feature dataset and the given workspace.
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="featureDatasetName"></param>
        /// <returns></returns>
        public static IEnumerable<string> FindFeatureClassesNames(this IWorkspace workspace, string featureDatasetName = null)
        {
            List<string> names = new List<string>();

            IEnumerable<IFeatureClass> classes = null;
            if (string.IsNullOrWhiteSpace(featureDatasetName))
                classes = OpenFeatureClasses(workspace, esriGeometryType.esriGeometryAny);
            else
                classes = OpenFeatureClasses(workspace, featureDatasetName, esriGeometryType.esriGeometryAny);

            foreach (var c in classes)
            {
                names.Add(c.GetName());
                Marshal.FinalReleaseComObject(c);
            }

            return names;
        }

        /// <summary>
        /// Finds all featureclasses from the dataset of the given name with the given geometry type.
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="featureDatasetName"></param>
        /// <param name="geoType"></param>
        /// <returns></returns>
        public static IEnumerable<IFeatureClass> OpenFeatureClasses(this IWorkspace workspace, string featureDatasetName, esriGeometryType geoType)
        {
            IFeatureDataset featureDataset = workspace.OpenFeatureDataset(featureDatasetName);

            IEnumerable<IFeatureClass> classes = featureDataset.OpenFeatureClasses(geoType);

            Marshal.FinalReleaseComObject(featureDataset);

            return classes;
        }

        /// <summary>
        /// Finds all featureclasses from the dataset of the given name with the given geometry type.
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="featureDatasetName"></param>
        /// <param name="geoType"></param>
        /// <returns></returns>
        public static IEnumerable<IFeatureClass> OpenFeatureClasses(this IWorkspace workspace, string featureDatasetName, params esriGeometryType[] geoType)
        {
            IFeatureDataset featureDataset = workspace.OpenFeatureDataset(featureDatasetName);

            IEnumerable<IFeatureClass> classes = featureDataset.OpenFeatureClasses(geoType);

            Marshal.FinalReleaseComObject(featureDataset);

            return classes;
        }

        /// <summary>
        /// Finds all featureclasses with the given geometry type.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <param name="geoType">The geometry type used to filter the featureclasses. you may pass Any to return all types of featureclasses.</param>
        /// <returns></returns>
        public static IEnumerable<IFeatureClass> OpenFeatureClasses(this IWorkspace workspace, esriGeometryType geoType)
        {
            IList<IFeatureClass> featureClasses = new List<IFeatureClass>();

            IEnumDataset enumDataset = workspace.Datasets[esriDatasetType.esriDTFeatureClass];
            while (enumDataset.Next() is IFeatureClass featureClass)
            {
                if (featureClass.ShapeType == geoType || geoType == esriGeometryType.esriGeometryAny)
                    featureClasses.Add(featureClass);
            }

            return featureClasses;
        }

    }
}
