using ESRI.ArcGIS.AnalysisTools;
using ESRI.ArcGIS.Analyst3DTools;
using ESRI.ArcGIS.ConversionTools;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.SpatialAnalystTools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DEWA.GIS.Desktop.Helpers
{
    public class ToolboxHelper
    {
        public enum JoinType
        {
            KEEP_ALL,
            KEEP_COMMON
        }

        public static void ExportCAD(string cadFilePath, string outputGdb, string outputDatasetName, double refScale = 1000, ISpatialReference sr = null)
        {
            var geoprocessor = new Geoprocessor()
            {
                OverwriteOutput = true,
                AddOutputsToMap = false
            };
            try
            {
                CADToGeodatabase cadToGdb = new CADToGeodatabase(cadFilePath, outputGdb, outputDatasetName, refScale);
                if (sr != null)
                    cadToGdb.spatial_reference = sr;

                geoprocessor.Execute(cadToGdb, null);
            }
            catch (Exception)
            {
                if (geoprocessor.MessageCount > 0)
                {
                    string excMessage = null;
                    for (int Count = 0; Count <= geoprocessor.MessageCount - 1; Count++)
                        excMessage += geoprocessor.GetMessage(Count) + Environment.NewLine;

                    throw new Exception(excMessage);
                }
            }
        }

        public static void IntersectTool(IEnumerable<string> inFeatures, string outputFCName)
        {
            var geoprocessor = new Geoprocessor()
            {
                OverwriteOutput = true,
                AddOutputsToMap = false
            };

            string inFeats = inFeatures.Aggregate((a, b) => a + ";" + b);

            Delete(outputFCName);

            Intersect intersect = new Intersect(inFeats, outputFCName)
            {
                join_attributes = "ALL",
                output_type = "INPUT"
            };

            try
            {
                var result = geoprocessor.Execute(intersect, null);
            }
            catch (Exception)
            {
                if (geoprocessor.MessageCount > 0)
                {
                    string excMessage = null;
                    for (int Count = 0; Count <= geoprocessor.MessageCount - 1; Count++)
                        excMessage += geoprocessor.GetMessage(Count) + Environment.NewLine;

                    throw new Exception(excMessage);
                }
            }
        }

        public static void SpatialJoin(object targetFC, object joinFC, string outputFcPath, JoinType joinType, string matchOption, IGPFieldMapping fieldMapping = null)
        {
            var geoprocessor = new Geoprocessor()
            {
                OverwriteOutput = true,
                AddOutputsToMap = false
            };

            try
            {
                Delete(outputFcPath);

                SpatialJoin spatialJoin = new SpatialJoin
                {
                    join_features = joinFC,
                    target_features = targetFC,
                    join_type = joinType.ToString(),
                    match_option = matchOption,
                    out_feature_class = outputFcPath,
                    field_mapping = fieldMapping
                };

                var result = geoprocessor.Execute(spatialJoin, null);
            }
            catch (Exception)
            {
                if (geoprocessor.MessageCount > 0)
                {
                    string excMessage = null;
                    for (int i = 0; i < geoprocessor.MessageCount; i++)
                        excMessage += geoprocessor.GetMessage(i) + Environment.NewLine;

                    throw new Exception(excMessage);
                }
            }
        }

        public static void JoinField(object in_data, object in_field, object join_table, object join_field, params string[] joinFields)
        {
            var geoprocessor = new Geoprocessor()
            {
                OverwriteOutput = true,
                AddOutputsToMap = false
            };

            try
            {
                JoinField joinFld = new JoinField(in_data, in_field, join_table, join_field)
                {
                    fields = string.Join(",", joinFields)
                };

                geoprocessor.Execute(joinFld, null);
            }
            catch (Exception)
            {
                if (geoprocessor.MessageCount > 0)
                {
                    string excMessage = null;
                    for (int i = 0; i < geoprocessor.MessageCount; i++)
                        excMessage += geoprocessor.GetMessage(i) + Environment.NewLine;

                    throw new Exception(excMessage);
                }
            }
        }

        public static void FeatureClassToFeatureClass(object srcFc, string outputLocation, string outputFcName, string where = null, object fieldMapping = null)
        {
            var geoprocessor = new Geoprocessor()
            {
                OverwriteOutput = true,
                AddOutputsToMap = false
            };

            try
            {
                FeatureClassToFeatureClass fcToFc = new FeatureClassToFeatureClass(srcFc, outputLocation, outputFcName)
                {
                    field_mapping = fieldMapping,
                    where_clause = where
                };

                geoprocessor.Execute(fcToFc, null);
            }
            catch (Exception)
            {
                if (geoprocessor.MessageCount > 0)
                {
                    string excMessage = null;
                    for (int Count = 0; Count <= geoprocessor.MessageCount - 1; Count++)
                        excMessage += geoprocessor.GetMessage(Count) + Environment.NewLine;

                    throw new Exception(excMessage);
                }
            }
        }

        public static void TableToTable(ITable srcTable, string outputLocation, string outputFcName, string where = null, object fieldMapping = null)
        {
            var geoprocessor = new Geoprocessor()
            {
                OverwriteOutput = true,
                AddOutputsToMap = false
            };

            try
            {
                TableToTable fcToFc = new TableToTable(srcTable, outputLocation, outputFcName)
                {
                    field_mapping = fieldMapping,
                    where_clause = where
                };

                geoprocessor.Execute(fcToFc, null);
            }
            catch (Exception)
            {
                if (geoprocessor.MessageCount > 0)
                {
                    string excMessage = null;
                    for (int Count = 0; Count <= geoprocessor.MessageCount - 1; Count++)
                        excMessage += geoprocessor.GetMessage(Count) + Environment.NewLine;

                    throw new Exception(excMessage);
                }
            }
        }

        /// <summary>
        ///Permanently deletes data from disk. All types of geographic data supported by ArcGIS, as well as toolboxes and workspaces (folders, geodatabases), can be deleted. 
        ///<para>If the specified item is a workspace, all contained items are also deleted.</para>
        /// </summary>
        /// <param name="item"></param>
        public static void Delete(object item, string dataType = null)
        {
            var geoprocessor = new Geoprocessor()
            {
                OverwriteOutput = true,
                AddOutputsToMap = false
            };

            if (item == null)
                return;

            try
            {
                Delete delete = new Delete(item);
                if (!string.IsNullOrWhiteSpace(dataType))
                    delete.data_type = dataType;

                geoprocessor.Execute(delete, null);
            }
            catch (Exception)
            {
                if (geoprocessor.MessageCount > 0)
                {
                    string excMessage = null;
                    for (int Count = 0; Count <= geoprocessor.MessageCount - 1; Count++)
                        excMessage += geoprocessor.GetMessage(Count) + Environment.NewLine;

                    throw new Exception(excMessage);
                }
            }
        }

        public static void RecalculateExtent(string fcPath)
        {
            var geoprocessor = new Geoprocessor()
            {
                OverwriteOutput = true,
                AddOutputsToMap = false
            };
            try
            {
                RecalculateFeatureClassExtent recalcExtent = new RecalculateFeatureClassExtent(fcPath);

                geoprocessor.Execute(recalcExtent, null);
            }
            catch (Exception)
            {
                if (geoprocessor.MessageCount > 0)
                {
                    string excMessage = null;
                    for (int Count = 0; Count <= geoprocessor.MessageCount - 1; Count++)
                        excMessage += geoprocessor.GetMessage(Count) + Environment.NewLine;

                    throw new Exception(excMessage);
                }
            }
        }

        public static void CalculateField(object table, string fldName, object expression)
        {
            var geoprocessor = new Geoprocessor()
            {
                OverwriteOutput = true,
                AddOutputsToMap = false
            };
            try
            {
                CalculateField fieldCalc = new CalculateField(table, fldName, expression)
                {
                    expression_type = "PYTHON_9.3"
                };

                geoprocessor.Execute(fieldCalc, null);
            }
            catch (Exception)
            {
                if (geoprocessor.MessageCount > 0)
                {
                    string excMessage = null;
                    for (int Count = 0; Count <= geoprocessor.MessageCount - 1; Count++)
                        excMessage += geoprocessor.GetMessage(Count) + Environment.NewLine;

                    throw new Exception(excMessage);
                }
            }
        }

        public static void Append(object inputs, object target)
        {
            var geoprocessor = new Geoprocessor()
            {
                OverwriteOutput = true,
                AddOutputsToMap = false
            };

            try
            {
                Append append = new Append(inputs, target);

                geoprocessor.Execute(append, null);
            }
            catch (Exception)
            {
                if (geoprocessor.MessageCount > 0)
                {
                    string excMessage = null;
                    for (int Count = 0; Count <= geoprocessor.MessageCount - 1; Count++)
                        excMessage += geoprocessor.GetMessage(Count) + Environment.NewLine;

                    throw new Exception(excMessage);
                }
            }
        }

        public static void DeleteFeatures(object infeatures)
        {
            var geoprocessor = new Geoprocessor()
            {
                OverwriteOutput = true,
                AddOutputsToMap = false
            };

            try
            {
                DeleteFeatures del = new DeleteFeatures(infeatures);

                geoprocessor.Execute(del, null);
            }
            catch (Exception)
            {
                if (geoprocessor.MessageCount > 0)
                {
                    string excMessage = null;
                    for (int Count = 0; Count <= geoprocessor.MessageCount - 1; Count++)
                        excMessage += geoprocessor.GetMessage(Count) + Environment.NewLine;

                    throw new Exception(excMessage);
                }
            }
        }

        public static void ExtractValuesToPoints(object points, object raster, object outpoints)
        {
            var geoprocessor = new Geoprocessor()
            {
                OverwriteOutput = true,
                AddOutputsToMap = false
            };

            try
            {
                ExtractValuesToPoints extract = new ExtractValuesToPoints(points, raster, outpoints);

                geoprocessor.Execute(extract, null);
            }
            catch (Exception)
            {

                if (geoprocessor.MessageCount > 0)
                {
                    string excMessage = null;
                    for (int Count = 0; Count <= geoprocessor.MessageCount - 1; Count++)
                        excMessage += geoprocessor.GetMessage(Count) + Environment.NewLine;

                    throw new Exception(excMessage);
                }
            }
        }

        public static void AlterField(object inFeatureClass, object field, string newName, string newAlias = null)
        {
            var geoprocessor = new Geoprocessor()
            {
                OverwriteOutput = true,
                AddOutputsToMap = false
            };
            try
            {
                AlterField alter = new AlterField(inFeatureClass, field)
                {
                    new_field_name = newName,
                    new_field_alias = newAlias ?? newName
                };

                geoprocessor.Execute(alter, null);
            }
            catch (Exception)
            {

                if (geoprocessor.MessageCount > 0)
                {
                    string excMessage = null;
                    for (int Count = 0; Count <= geoprocessor.MessageCount - 1; Count++)
                        excMessage += geoprocessor.GetMessage(Count) + Environment.NewLine;

                    throw new Exception(excMessage);
                }
            }
        }

        public static void DeleteField(object table, object field)
        {
            var geoprocessor = new Geoprocessor()
            {
                OverwriteOutput = true,
                AddOutputsToMap = false
            };
            try
            {
                DeleteField del = new DeleteField(table, field);

                geoprocessor.Execute(del, null);
            }
            catch (Exception)
            {

                if (geoprocessor.MessageCount > 0)
                {
                    string excMessage = null;
                    for (int Count = 0; Count <= geoprocessor.MessageCount - 1; Count++)
                        excMessage += geoprocessor.GetMessage(Count) + Environment.NewLine;

                    throw new Exception(excMessage);
                }
            }
        }

        public static void AddField(object table, string fieldName, string fieldtype)
        {
            var geoprocessor = new Geoprocessor()
            {
                OverwriteOutput = true,
                AddOutputsToMap = false
            };

            try
            {
                AddField addField = new AddField(table, fieldName, fieldtype);
                geoprocessor.Execute(addField, null);
            }
            catch (Exception)
            {

                if (geoprocessor.MessageCount > 0)
                {
                    string excMessage = null;
                    for (int Count = 0; Count <= geoprocessor.MessageCount - 1; Count++)
                        excMessage += geoprocessor.GetMessage(Count) + Environment.NewLine;

                    throw new Exception(excMessage);
                }
            }
        }

        /// <summary>
        /// Converts the given inFeatureClass to a 3d featureclass using the given height field. If out featureclass exist it will be overwritten.
        /// </summary>
        /// <param name="inFeatureClass"></param>
        /// <param name="outFeatureClass"></param>
        /// <param name="heightField"></param>
        public static void Convert2dTo3dByAttr(object inFeatureClass, object outFeatureClass, object heightField, object toHeightField = null)
        {
            var geoprocessor = new Geoprocessor()
            {
                OverwriteOutput = true,
                AddOutputsToMap = false
            };

            try
            {
                Delete(outFeatureClass);

                FeatureTo3DByAttribute featureTo3D = new FeatureTo3DByAttribute(inFeatureClass, outFeatureClass, heightField)
                {
                    to_height_field = toHeightField
                };

                geoprocessor.Execute(featureTo3D, null);
            }
            catch (Exception)
            {

                if (geoprocessor.MessageCount > 0)
                {
                    string excMessage = null;
                    for (int Count = 0; Count <= geoprocessor.MessageCount - 1; Count++)
                        excMessage += geoprocessor.GetMessage(Count) + Environment.NewLine;

                    throw new Exception(excMessage);
                }
            }
        }

        public static void Near(object in_features, object near_features, object searchRadius = null)
        {
            Geoprocessor geoprocessor = new Geoprocessor()
            {
                OverwriteOutput = true,
                AddOutputsToMap = false
            };

            try
            {
                Near near = new Near(in_features, near_features)
                {
                    search_radius = searchRadius
                };

                geoprocessor.Execute(near, null);
            }
            catch (Exception)
            {
                if (geoprocessor.MessageCount > 0)
                {
                    string excMessage = null;
                    for (int Count = 0; Count <= geoprocessor.MessageCount - 1; Count++)
                        excMessage += geoprocessor.GetMessage(Count) + Environment.NewLine;
                    throw new Exception(excMessage);
                }
            }
        }

        public static void InterpolateShape(object in_surface, object in_feature_class, object out_feature_class, bool verticesOnly = false)
        {
            Geoprocessor geoprocessor = new Geoprocessor()
            {
                OverwriteOutput = true,
                AddOutputsToMap = false
            };

            try
            {
                InterpolateShape inter = new InterpolateShape(in_surface, in_feature_class, out_feature_class);
                if (verticesOnly)
                    inter.vertices_only = "VERTICES_ONLY";

                geoprocessor.Execute(inter, null);
            }
            catch (Exception)
            {
                if (geoprocessor.MessageCount > 0)
                {
                    string excMessage = null;
                    for (int Count = 0; Count <= geoprocessor.MessageCount - 1; Count++)
                        excMessage += geoprocessor.GetMessage(Count) + Environment.NewLine;
                    throw new Exception(excMessage);
                }
            }
        }

        public static void Copy(object in_data, object out_data)
        {
            Geoprocessor geoprocessor = new Geoprocessor()
            {
                OverwriteOutput = true,
                AddOutputsToMap = false
            };

            try
            {
                Copy copy = new Copy(in_data, out_data);
                geoprocessor.Execute(copy, null);
            }
            catch (Exception)
            {
                if (geoprocessor.MessageCount > 0)
                {
                    string excMessage = null;
                    for (int Count = 0; Count <= geoprocessor.MessageCount - 1; Count++)
                        excMessage += geoprocessor.GetMessage(Count) + Environment.NewLine;
                    throw new Exception(excMessage);
                }
            }
        }

        public static void Rename(object item, object newItem)
        {
            Geoprocessor geoprocessor = new Geoprocessor()
            {
                OverwriteOutput = true,
                AddOutputsToMap = false
            };

            try
            {
                Rename copy = new Rename(item, newItem);
                geoprocessor.Execute(copy, null);
            }
            catch (Exception)
            {
                if (geoprocessor.MessageCount > 0)
                {
                    string excMessage = null;
                    for (int Count = 0; Count <= geoprocessor.MessageCount - 1; Count++)
                        excMessage += geoprocessor.GetMessage(Count) + Environment.NewLine;
                    throw new Exception(excMessage);
                }
            }
        }


    }
}
