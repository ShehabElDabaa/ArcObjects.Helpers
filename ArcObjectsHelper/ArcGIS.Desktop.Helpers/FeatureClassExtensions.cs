using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geoprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace DEWA.GIS.Desktop.Helpers
{
    public static class FeatureClassExtensions
    {
        /// <summary>
        /// Creates fieldmapping object with the OBJECTID field to be extracted to be used with GP tools.
        /// </summary>
        /// <param name="classObj">FeatureClass or table.</param>
        /// <param name="newOIDFldName">Name of the object id field in the mapping.</param>
        /// <returns></returns>
        public static IGPFieldMapping CreateFieldMap(this IClass classObj, string newOIDFldName, bool newOidFldNullable = false, int newOidDefaultVal = -1)
        {
            IGPFieldMapping fieldmapping = null;

            if (classObj is ITable || classObj is IFeatureClass)
            {
                IGPUtilities gputilities = new GPUtilities();

                IDETable inputTableA = (IDETable)gputilities.MakeDataElementFromNameObject((classObj as IDataset).FullName);

                IArray inputTables = new ESRI.ArcGIS.esriSystem.Array();
                inputTables.Add(inputTableA);

                fieldmapping = new GPFieldMapping() as IGPFieldMapping;
                fieldmapping.Initialize(inputTables, null);

                IFieldEdit2 oidField = new Field() as IFieldEdit2;
                oidField.Name_2 = newOIDFldName;
                oidField.Type_2 = esriFieldType.esriFieldTypeInteger;
                oidField.IsNullable_2 = newOidFldNullable;
                oidField.DefaultValue_2 = newOidDefaultVal;

                IGPFieldMap orgOIDFldMap = new GPFieldMap { OutputField = oidField };
                IField OIDFld = classObj.Fields.Field[classObj.FindField(classObj.OIDFieldName)];
                orgOIDFldMap.AddInputField(inputTableA, OIDFld, 0, OIDFld.Length);

                fieldmapping.AddFieldMap(orgOIDFldMap);
            }

            return fieldmapping;
        }

        /// <summary>
        /// Gets the featureclass name.
        /// </summary>
        /// <param name="classObj"></param>
        /// <returns></returns>
        public static string GetName(this IClass @class)
        {
            return (@class as IDataset)?.Name;
        }

        /// <summary>
        /// Gets the disk path of the given object. However only if it's on the root gdb not inside a dataset.
        /// </summary>
        /// <param name="class"></param>
        /// <returns></returns>
        public static string GetPath(this IClass @class)
        {
            return (@class as IDataset)?.Workspace.PathName + "\\" + @class.GetName();
        }

        /// <summary>
        /// Gets the disk path of the given object. However only if it's on the root gdb not inside a dataset.
        /// </summary>
        /// <param name="class"></param>
        /// <returns></returns>
        public static string GetPath(this IClass @class, out string className)
        {
            className = @class.GetName();
            return (@class as IDataset)?.Workspace.PathName + "\\" + className;
        }

        /// <summary>
        /// Gets the disk path of the given object. However only if it's inside the given dataset.
        /// </summary>
        /// <param name="class"></param>
        /// <returns></returns>
        public static string GetPath(this IClass @class, string datasetName)
        {
            return (@class as IDataset)?.Workspace.PathName + $@"\{datasetName}\" + @class.GetName();
        }

        /// <summary>
        /// Gets the disk path of the given object. However only if it's inside the given dataset.
        /// </summary>
        /// <param name="class"></param>
        /// <returns></returns>
        public static string GetPath(this IClass @class, string datasetName, out string className)
        {
            className = @class.GetName();
            return (@class as IDataset)?.Workspace.PathName + $@"\{datasetName}\" + className;
        }

        /// <summary>
        /// Utility function to create a spatial filter.
        /// </summary>
        public static ISpatialFilter CreateSpatialFilter(this IFeatureClass fclass, IGeometry searchGeometry, esriSpatialRelEnum spatialRelation, double bufferdistance, string where)
        {
            ISpatialFilter spatialFilter = new SpatialFilter
            {
                GeometryField = fclass.ShapeFieldName,
                SpatialRel = spatialRelation,
                Geometry = searchGeometry
            };

            if (bufferdistance > 0.0)
            {
                ITopologicalOperator topoOperator = (ITopologicalOperator)searchGeometry;
                IGeometry buffer = topoOperator.Buffer(bufferdistance);
                spatialFilter.Geometry = buffer;
            }

            spatialFilter.SubFields = string.Format("{0},{1}", fclass.OIDFieldName, fclass.ShapeFieldName);

            if (!string.IsNullOrEmpty(where))
                spatialFilter.WhereClause = where;

            return spatialFilter;
        }

        /// <summary>
        /// Searches the featureclass for the data with the given filter and sub fields.
        /// </summary>
        /// <param name="featureClass"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public static IFeatureCursor Search(this IFeatureClass featureClass, string where = "")
        {
            if (string.IsNullOrWhiteSpace(where))
                return featureClass.Search(null, false);

            IQueryFilter queryFilter = new QueryFilterClass { WhereClause = where };

            return featureClass.Search(queryFilter, false);
        }

        /// <summary>
        /// Opens update cursor on the featureclass with the given filter.
        /// </summary>
        /// <param name="featureClass"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public static IFeatureCursor Update(this IFeatureClass featureClass, string where = "")
        {
            if (string.IsNullOrWhiteSpace(where))
                return featureClass.Search(null, false);

            IQueryFilter queryFilter = new QueryFilterClass { WhereClause = where };

            return featureClass.Update(queryFilter, false);
        }

        /// <summary>
        /// Searches the featureclass for the data with the given filter and sub fields.
        /// </summary>
        /// <param name="featureClass"></param>
        /// <param name="where"></param>
        /// <param name="subFields"></param>
        /// <returns></returns>
        public static IFeatureCursor Search(this IFeatureClass featureClass, string where, IEnumerable<string> subFields)
        {
            IQueryFilter queryFilter = new QueryFilterClass
            {
                SubFields = subFields.Aggregate((a, b) => a + "," + b),
                WhereClause = where
            };

            return featureClass.Search(queryFilter, false);
        }

        /// <summary>
        /// Searches for the intersected features with the given geometry.
        /// </summary>
        /// <param name="featureClass"></param>
        /// <param name="where"></param>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public static IFeatureCursor Search(this IFeatureClass featureClass, IGeometry geometry)
        {
            IQueryFilter queryFilter = new SpatialFilterClass
            {
                Geometry = geometry,
                SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects
            };

            return featureClass.Search(queryFilter, false);
        }

        /// <summary>
        /// Gets features count.
        /// </summary>
        /// <param name="featureClass"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public static int FeatureCount(this IFeatureClass featureClass, string where = null)
        {
            if (string.IsNullOrWhiteSpace(where))
                return featureClass.FeatureCount(null);

            IQueryFilter queryFilter = new QueryFilterClass { WhereClause = where };

            return featureClass.FeatureCount(queryFilter);
        }

        /// <summary>
        /// Deletes features by cursor. If you want to delete all features, then for better performance use the toolbox tool.
        /// </summary>
        /// <param name="featureClass"></param>
        /// <param name="where"></param>
        public static void DeleteFeatures(this IFeatureClass featureClass, string where)
        {
            IQueryFilter queryFilter = new QueryFilterClass { WhereClause = where };
            if (featureClass.FeatureCount(where) > 0)
            {
                IFeatureCursor cursor = featureClass.Update(queryFilter, false);

                IFeature feature = null;
                while ((feature = cursor.NextFeature()) != null)
                    feature.Delete();

                if (cursor != null)
                    Marshal.FinalReleaseComObject(cursor);
            }
        }

        /// <summary>
        /// Copies features from the source feature class to the target featureclass using cursors.
        /// </summary>
        /// <param name="source">Source featureclass</param>
        /// <param name="target">Target featureclass</param>
        /// <param name="where">(Optional) Filter used to filter the source data.</param>
        public static void CopyFeaturesAttributes(this IFeatureClass source, IFeatureClass target, string where = null)
        {
            if (source.FeatureCount(where) > 0)
            {
                IFeatureCursor targetCursor = target.Insert(true);

                IFeatureCursor sourceCursor = source.Search(where);

                IFeature sourceFeature = null;
                IFeatureBuffer targetFeature = target.CreateFeatureBuffer();

                int fldsCount = source.Fields.FieldCount;

                while ((sourceFeature = sourceCursor.NextFeature()) != null)
                {
                    if (!sourceFeature.Shape.IsEmpty && sourceFeature.HasOID)
                    {
                        targetFeature.Shape = sourceFeature.ShapeCopy;

                        for (int i = 0; i < fldsCount; i++)
                        {
                            IField sourceField = sourceFeature.Fields.Field[i];

                            if (sourceField != null && sourceField.Type != esriFieldType.esriFieldTypeOID && sourceField.Type != esriFieldType.esriFieldTypeGeometry)
                            {
                                int targetFldIdx = targetFeature.Fields.FindField(sourceField.Name);

                                if (targetFldIdx > -1 && targetFeature.Fields.Field[targetFldIdx].Editable == true)
                                    targetFeature.Value[targetFldIdx] = sourceFeature.Value[i];
                            }
                        }

                        targetCursor.InsertFeature(targetFeature);
                    }
                }

                targetCursor.Flush();

                if (targetCursor != null)
                    Marshal.FinalReleaseComObject(targetCursor);
            }

        }

        /// <summary>
        /// Returns a list of feature integer values of the given ID field name or OID value.
        /// </summary>
        /// <param name="featureClass"></param>
        /// <param name="filter"></param>
        /// <param name="idFldName"></param>
        /// <returns></returns>
        public static IList<int> GetIDs(this IFeatureClass featureClass, string filter = null, string idFldName = null)
        {
            IList<int> ids = new List<int>();

            int idFldIdx = -1;
            IFeatureCursor newfeatures = featureClass.Search(filter);
            if (!string.IsNullOrWhiteSpace(idFldName) && (idFldIdx = newfeatures.FindField(idFldName)) > -1)
            {
                IFeature feature = null;
                while ((feature = newfeatures.NextFeature()) != null)
                    ids.Add(Convert.ToInt32(feature.Value[idFldIdx]));
            }
            else
            {
                IFeature feature = null;
                while ((feature = newfeatures.NextFeature()) != null)
                    ids.Add(feature.OID);
            }

            return ids;
        }

        /// <summary>
        /// Add new field.
        /// </summary>
        /// <param name="featureClass"></param>
        /// <param name="fieldName"></param>
        /// <param name="fieldType"></param>
        /// <param name="fldLength"></param>
        /// <param name="isNullable"></param>
        /// <param name="defaultValue"></param>
        public static void AddNewField(this IFeatureClass featureClass, string fieldName, esriFieldType fieldType, int fldLength = 1, bool isNullable = true, object defaultValue = null)
        {
            ISchemaLock schemaLock = (ISchemaLock)featureClass;
            try
            {
                schemaLock.ChangeSchemaLock(esriSchemaLock.esriExclusiveSchemaLock);

                IFieldEdit2 field = new Field() as IFieldEdit2;
                field.Name_2 = fieldName;
                field.Type_2 = fieldType;
                field.IsNullable_2 = isNullable;
                field.DefaultValue_2 = defaultValue;
                if (fieldType == esriFieldType.esriFieldTypeString)
                    field.Length_2 = fldLength;

                featureClass.AddField(field);
            }
            finally
            {
                schemaLock.ChangeSchemaLock(esriSchemaLock.esriSharedSchemaLock);
            }
        }


    }
}
