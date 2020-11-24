using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Geodatabase;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace DEWA.GIS.Desktop.Helpers
{
    public static class ArcGISHelper
    {
        public static IWorkspace CreateScratchMdbWorkspace()
        {
            IScratchWorkspaceFactory scratchWorkspaceFactory = new ScratchWorkspaceFactory();
            IWorkspace scratchWorkspace = scratchWorkspaceFactory.CreateNewScratchWorkspace();
            return scratchWorkspace;
        }

        public static IWorkspace CreateScratchFgdbWorkspace()
        {
            IScratchWorkspaceFactory scratchWorkspaceFactory = new FileGDBScratchWorkspaceFactory();
            IWorkspace scratchWorkspace = scratchWorkspaceFactory.CreateNewScratchWorkspace();
            return scratchWorkspace;
        }

        /// <summary>
        /// Creates an fgdb in the given path.
        /// </summary>
        /// <param name="fgdbPath"></param>
        public static void CreateFgdbWorkspace(string fgdbPath)
        {
            if (string.IsNullOrWhiteSpace(fgdbPath))
                return;

            var parentDir = Directory.GetParent(fgdbPath).FullName;
            string fgdbName = Path.GetFileName(fgdbPath);

            IWorkspaceFactory scratchWorkspaceFactory = new FileGDBWorkspaceFactory();
            IWorkspaceName workspaceName = scratchWorkspaceFactory.Create(parentDir, fgdbName, null, 0);

            Marshal.FinalReleaseComObject(workspaceName);
        }

        /// <summary>
        /// Opens the workspace of the given FGDB path. 
        /// <para>If the given workspace path doesn't exist or it's an empty directory, it will be used to create the workspace and open it.</para>
        /// </summary>
        /// <param name="fgdbPath">FGDB path.</param>
        /// <returns>IWorkspace opened.</returns>
        public static IWorkspace OpenworkSpace(string fgdbPath)
        {
            if (string.IsNullOrWhiteSpace(fgdbPath))
                return null;

            if (!Directory.Exists(fgdbPath) || Directory.GetFiles(fgdbPath).Length == 0)
                CreateFgdbWorkspace(fgdbPath);

            IWorkspaceFactory workspaceFactory = new FileGDBWorkspaceFactory();
            if (workspaceFactory.IsWorkspace(fgdbPath))
                return workspaceFactory.OpenFromFile(fgdbPath, 0);

            return null;
        }

        public static IEditor FindEditor()
        {
            var app = FindAppObject();
            UID editorUID = new UID { Value = "esriEditor.Editor" };
            return app.FindExtensionByCLSID(editorUID) as IEditor;
        }

        public static IApplication FindAppObject()
        {
            Type appReference = Type.GetTypeFromProgID("esriFramework.AppRef");

            return Activator.CreateInstance(appReference) as IApplication;
        }


        /// <summary>
        /// Open a Raster file on disk by the given name and file path.
        /// </summary>
        /// <param name="directoryPath">A System.String that is the directory location of the raster file. Example: "C:\raster_data"</param>
        /// <param name="name">A System.String that is the name of the raster file in the directory. Example: "landuse" or "watershed"</param>
        /// <returns></returns>
        /// <remarks>
        /// IRasterWorkspace is used to access a raster stored in a file system in any supported raster format. 
        /// RasterWorkspaceFactory must be used to create a raster workspace.
        /// To access raster from geodatabase, use IRasterWorkspaceEx interface.
        /// </remarks>
        public static IRasterDataset OpenRasterFileDataset(string directoryPath, string name)
        {
            IWorkspaceFactory workspaceFactory = new RasterWorkspaceFactoryClass();
            IRasterWorkspace rasterWorkspace = (IRasterWorkspace)workspaceFactory.OpenFromFile(directoryPath, 0);
            return rasterWorkspace.OpenRasterDataset(name);
        }

        /// <summary>
        /// Retrieves Feature class from File Geodatabase   
        /// </summary>
        /// <param name="pfeature">Current feature </param>
        /// <param name="FeatureClassname">Feature class name </param>
        /// <returns>Ifeatureclass</returns>
        public static int GetDefaultSubtypeFormFeatureClass(IFeatureClass pfeatureClass)
        {
            ISubtypes pSubtypes = (ISubtypes)pfeatureClass;
            return pSubtypes.DefaultSubtypeCode;
        }

        /// <summary>
        /// Find the domain description for the given field value.
        /// </summary>
        /// <param name="fieldValue">Field value.</param>
        /// <param name="domain">Domain object of the field.</param>
        /// <returns></returns>
        public static string FindCodedDomainDescription(this IDomain domain, string fieldValue)
        {
            if (string.IsNullOrWhiteSpace(fieldValue) || domain == null)
                return null;

            string desc = null;
            if (domain is ICodedValueDomain cvDomain)
            {
                int i = 0;
                while (i < cvDomain.CodeCount && desc == null)
                {
                    string strDomainVal = cvDomain.Value[i]?.ToString();

                    if (strDomainVal.Trim().ToUpper() == fieldValue.Trim().ToUpper())
                    {
                        desc = cvDomain.Name[i].Trim();
                    }

                    i++;
                }
            }

            return desc;
        }

        /// <summary>
        /// Opens the given TIFF path as a RasterDataset.
        /// </summary>
        /// <param name="tiffPath"></param>
        /// <returns></returns>
        public static IRasterDataset OpenTiffDataset(string tiffPath)
        {
            string gdubaiDirPath = Directory.GetParent(tiffPath).FullName;
            string gDubaiName = Path.GetFileName(tiffPath);

            return OpenRasterFileDataset(gdubaiDirPath, gDubaiName);
        }

    }
}
