using ESRI.ArcGIS.Carto;

namespace DEWA.GIS.Desktop.Helpers
{
    public static class MapExtensions
    {
        /// <summary>
        /// Finds layer from the map by the given layer name.
        /// </summary>
        /// <param name="map"></param>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public static ILayer LayerByName(this IMap map, string layerName)
        {
            if (map == null || string.IsNullOrWhiteSpace(layerName))
                return null;

            ILayer layer = null;
            ILayer foundLayer = null;

            IEnumLayer enumLayer = map.Layers;
            enumLayer.Reset();

            while ((layer = enumLayer.Next()) != null && foundLayer == null)
            {
                if (layer.Name.ToLower() == layerName.ToLower())
                    foundLayer = layer;
            }

            return foundLayer;

        }

        /// <summary>
        /// Finds layer from the map by the given featureclass name.
        /// </summary>
        /// <param name="map"></param>
        /// <param name="featureClassName"></param>
        /// <returns></returns>
        public static ILayer LayerByFeatureClass(this IMap map, string featureClassName)
        {
            if (map == null || string.IsNullOrWhiteSpace(featureClassName))
                return null;

            ILayer layer = null;
            ILayer foundLayer = null;

            IEnumLayer enumLayer = map.Layers;
            enumLayer.Reset();

            while ((layer = enumLayer.Next()) != null && foundLayer == null)
            {
                if (layer is IFeatureLayer featureLayer && featureLayer.FeatureClass.GetName().ToLower() == featureClassName.ToLower())
                    foundLayer = layer;
            }

            return foundLayer;
        }
    }
}
