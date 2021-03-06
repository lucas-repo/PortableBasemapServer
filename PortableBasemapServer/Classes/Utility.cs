﻿using System;
using PBS.DataSource;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client;
using System.IO;
using System.Configuration;
using System.Windows;

namespace PBS.APP.Classes
{
    /// <summary>
    /// App实用类
    /// </summary>
    public static class AppUtility
    {
        /// <summary>
        /// 读取配置文件
        /// </summary>
        /// <param name="configKey">key</param>
        /// <param name="defaultValue">默认值（如果未找到）</param>
        /// <returns></returns>
        public static string ReadConfig(string configKey, string defaultValue)
        {
            try
            {
                return ConfigurationManager.AppSettings[configKey] == null ? defaultValue : ConfigurationManager.AppSettings[configKey];
            }
            catch (Exception ex)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 写入配置项
        /// </summary>
        /// <param name="configKey"></param>
        /// <param name="value"></param>
        public static void WriteConfig(string configKey, string value)
        {
            System.Configuration.Configuration config = null;
            //reading config file
            try
            {
                //do not use ConfigurationUserLevel.None, otherwise saving settings will be failed.
                config = ConfigurationManager.OpenExeConfiguration("PortableBasemapServer.exe");
                if (!config.HasFile)
                    throw new FileNotFoundException(Application.Current.FindResource("msgConfigFileNotExist").ToString());
            }
            catch (Exception e)
            {
                MessageBox.Show(Application.Current.FindResource("msgConfigFileBroken").ToString() + "\r\n" + e.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                if (config.AppSettings.Settings[configKey].Value != null)
                {
                    config.AppSettings.Settings[configKey].Value = value;
                    config.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection("appSettings");
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="levels"></param>
        /// <param name="extent">sr=3857</param>
        /// <returns></returns>
        public static long CalculateTileCount(int[] levels, ESRI.ArcGIS.Client.Geometry.Envelope extent)
        {
            long total = 0;
            int constTileSize = 256;
            LODInfo[] LODs = new LODInfo[20];
            const double cornerCoordinate = 20037508.3427892;
            double resolution = cornerCoordinate * 2 / 256;
            double scale = 591657527.591555;
            for (int i = 0; i < LODs.Length; i++)
            {
                LODs[i] = new LODInfo()
                {
                    Resolution = resolution,
                    LevelID = i,
                    Scale = scale
                };
                resolution /= 2;
                scale /= 2;
            }
            PBS.Util.Point TileOrigin = new PBS.Util.Point(-cornerCoordinate, cornerCoordinate);
            foreach (int level in levels)
            {
                LODInfo lod = LODs[level];
                double oneTileDistance = lod.Resolution * constTileSize;
                //calculate start tile and end tile
                int startTileRow = (int)(Math.Abs(TileOrigin.Y - extent.YMax) / oneTileDistance);
                int startTileCol = (int)(Math.Abs(TileOrigin.X - extent.XMin) / oneTileDistance);
                int endTileRow = (int)(Math.Abs(TileOrigin.Y - extent.YMin) / oneTileDistance);
                int endTileCol = (int)(Math.Abs(TileOrigin.X - extent.XMax) / oneTileDistance);
                //"startR,startC,endR,endC"            
                total += Math.Abs((endTileCol - startTileCol + 1) * (endTileRow - startTileRow + 1));
            }
            return total;
        }

        public static PBS.Util.Polygon ConvertEsriPolygonToPBSPolygon(ESRI.ArcGIS.Client.Geometry.Polygon ePolygon)
        {
            if (ePolygon == null)
                return null;
            PBS.Util.Polygon pPolygon = new Util.Polygon();
            foreach (ESRI.ArcGIS.Client.Geometry.PointCollection ePC in ePolygon.Rings)
            {
                PBS.Util.PointCollection pPC = new Util.PointCollection();
                foreach (ESRI.ArcGIS.Client.Geometry.MapPoint ePoint in ePC)
                {
                    pPC.Add(new PBS.Util.Point(ePoint.X, ePoint.Y));
                }
                pPolygon.Rings.Add(pPC);
            }
            return pPolygon;
        }

        public static ESRI.ArcGIS.Client.Geometry.Polygon ConvertPBSPolygonToEsriPolygon(PBS.Util.Polygon pPolygon)
        {
            ESRI.ArcGIS.Client.Geometry.Polygon ePolygon = new ESRI.ArcGIS.Client.Geometry.Polygon();
            foreach (PBS.Util.PointCollection pPC in pPolygon.Rings)
            {
                ESRI.ArcGIS.Client.Geometry.PointCollection ePC = new ESRI.ArcGIS.Client.Geometry.PointCollection();
                foreach (PBS.Util.Point pPoint in pPC)
                {
                    ePC.Add(new MapPoint(pPoint.X, pPoint.Y));
                }
                ePolygon.Rings.Add(ePC);
            }
            return ePolygon;
        }
        /// <summary>
        /// union the polygon shapes in every graphic into one big multi part polygon.
        /// return esri polygon
        /// </summary>
        /// <param name="graphics"></param>
        /// <returns>esri polygon</returns>
        public static ESRI.ArcGIS.Client.Geometry.Polygon UnionEsriPolygon(GraphicCollection graphics)
        {
            ESRI.ArcGIS.Client.Geometry.Polygon p = new ESRI.ArcGIS.Client.Geometry.Polygon();
            foreach (Graphic g in graphics)
            {
                foreach (ESRI.ArcGIS.Client.Geometry.PointCollection pc in (g.Geometry as ESRI.ArcGIS.Client.Geometry.Polygon).Rings)
                {
                    p.Rings.Add(pc);
                }
            }
            return p;
        }

        
    }
}
