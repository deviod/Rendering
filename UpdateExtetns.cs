using System;
using System.IO;
using GIS_FW;
using System.Collections;
using System.Collections.Generic;
using GIS_FW.DATA;
using GIS_FW.TERRAIN;
using GIS_FW.DATA.RASTER;

using UnityEngine;
using UnityEngine.Rendering;

namespace GIS_FW
{
    namespace TERRAIN
    {
        public class Level
        {
            public int  id;
            public int  mapPosts;
            public uint blockPosts;
            public int PatchPosts;
            public int PatchSegments;
            public int  mapSegments;

            public Level Terrain;
            public Level Imagery;
            public Level Finer;
            public Level Coarser;

            public Vector2 ScaleFactor;
            public Vector2 level_factor;
            public float useBlendRegions;
            public bool OffsetStripOnEast;
            public bool OffsetStripOnNorth;

            public Extent NxtTerExt = new Extent();
            public Extent NxtImgExt = new Extent();
            public Vector2d TerOrg = new Vector2d(0.0, 0.0);
            public Vector2d ImgOrg = new Vector2d(0.0, 0.0);
            public Extent CurTerExt = new Extent(0, 0, 0, 0);
            public Extent CurImgExt = new Extent(0, 0, 0, 0);

            public Vector2 fineOrigin = new Vector2(0.0f, 0.0f);
            public Vector2 TerOffsetInImg = new Vector2(0.0f, 0.0f);
            public Vector2 viewPosInClipped = new Vector2(0.0f, 0.0f);
            public Vector2 OffsetWorldOrigin = new Vector2(0.0f, 0.0f);
            public Vector2 fineOriginInCoarse = new Vector2(0.0f, 0.0f);

            public Level(int i, int mapPosts, int Segments)
            {
                id = i;
                counter = 0;
                Terrain = DataBase.Terrain.Levels[id];
                mapPosts = mapPosts;
                mapSegments = Segments;
                PatchPosts = (mapPosts - 1) / 4 + 1;
                PatchSegments = PatchPosts - 1;

                float _ScaleFactor = (float)Math.Pow(2.0, -id);
                ScaleFactor = new Vector2(_ScaleFactor, _ScaleFactor);

                double longitudeResRequired = Terrain.PostDeltaLongitude;
                double latitudeResRequired = Terrain.PostDeltaLatitude;
                Level imagery = null;
                for (int j = 0; j < DataBase.Imagery.Levels.Length; j++)
                {
                    imagery = DataBase.Imagery.Levels[j];
                    if (imagery.PostDeltaLongitude <= longitudeResRequired &&
                        imagery.PostDeltaLatitude <= latitudeResRequired)
                    {
                        break;
                    }
                }
                Imagery = imagery;
                ImgWidth = (int)Math.Ceiling(mapPosts * Terrain.PostDeltaLongitude / imagery.PostDeltaLongitude);
                ImgHeight = (int)Math.Ceiling(mapPosts * Terrain.PostDeltaLatitude / imagery.PostDeltaLatitude);

                    light_dass = new Vector4(0.05f, .05f, .900f, 1.0f);
                for (int ii = 0; ii < 1000; ii++) ground_values[ii] = 0.0f;

                light_dsas = Shader.PropertyToID("light_dsas");
            }

            public bool UpdParameters(Levels[] levels, Level coarser, Geodetic3D center, Level TLast)
            {
                float factor = 1.0f;
                int west = CurTerExt.West;
                int south = CurTerExt.South;
                int east = CurTerExt.East;
                int north = CurTerExt.North;

                OffsetWorldOrigin.x = (float)((double)CurTerExt.West - Terrain.LongitudeToIndex(0.0));
                OffsetWorldOrigin.y = (float)((double)CurTerExt.South - Terrain.LatitudeToIndex(0.0));

                viewPosInClipped.x = (float)(Terrain.LongitudeToIndex(Trig.ToDegrees(center.Longitude)) - CurTerExt.West);
                viewPosInClipped.y = (float)(Terrain.LatitudeToIndex(Trig.ToDegrees(center.Latitude)) - CurTerExt.South);
                if (id < Globals.TLevels + 1)
                {
                    fineOriginInCoarse.x = (float)coarser.TerOrg.x + west / 2 - coarser.CurTerExt.West + 0.5f;
                    fineOriginInCoarse.y = (float)coarser.TerOrg.y + south / 2 - coarser.CurTerExt.South + 0.5f;
                }
                else if (id == Globals.TLevels + 1)
                {
                    factor = 0.25f;
                    fineOriginInCoarse.x = (float)TLast.TerOrg.x + west * factor - TLast.CurTerExt.West + 0.5f;
                    fineOriginInCoarse.y = (float)TLast.TerOrg.y + south * factor - TLast.CurTerExt.South + 0.5f;
                }
                else if (id == Globals.TLevels + 2)
                {
                    factor = 0.125f;
                    fineOriginInCoarse.x = (float)TLast.TerOrg.x + west * factor - TLast.CurTerExt.West + 0.5f;
                    fineOriginInCoarse.y = (float)TLast.TerOrg.y + south * factor - TLast.CurTerExt.South + 0.5f;
                }

                fineOrigin.x = (float)TerOrg.x + 0.5f;
                fineOrigin.y = (float)TerOrg.y + 0.5f;

                double terWest = Terrain.IndexToLongitude(CurTerExt.West);
                double terSouth = Terrain.IndexToLatitude(CurTerExt.South);
                double imgWestIndex = Imagery.LongitudeToIndex(terWest);
                double imgSouthIndex = Imagery.LatitudeToIndex(terSouth);

                TerOffsetInImg.x = (float)ImgOrg.x;
                TerOffsetInImg.y = (float)ImgOrg.y;
                TerOffsetInImg.x += (float)(imgWestIndex - CurImgExt.West);
                TerOffsetInImg.y += (float)(imgSouthIndex - CurImgExt.South);

                levels[id].u_fto_ubrs.x = fineOrigin.x;
                levels[id].u_fto_ubrs.y = fineOrigin.y;
                levels[id].u_fto_ubrs.z = useBlendRegions;
                levels[id].u_fto_ubrs.w = factor;

                levels[id].u_lofwo_toii.x = OffsetWorldOrigin.x;
                levels[id].u_lofwo_toii.y = OffsetWorldOrigin.y;
                levels[id].u_lofwo_toii.z = TerOffsetInImg.x;
                levels[id].u_lofwo_toii.w = TerOffsetInImg.y;

                levels[id].u_vpicl_floic.x = viewPosInClipped.x;
                levels[id].u_vpicl_floic.y = viewPosInClipped.y;
                levels[id].u_vpicl_floic.z = fineOriginInCoarse.x;
                levels[id].u_vpicl_floic.w = fineOriginInCoarse.y;

                return true;
            }

            public void UpdExtents(double CntLng, double CntLat)
            {
                double TerLngInd = Terrain.LongitudeToIndex(CntLng);
                double TerLatInd = Terrain.LatitudeToIndex(CntLat);
                double ImgLngInd = Imagery.LongitudeToIndex(CntLng);
                double ImgLatInd = Imagery.LatitudeToIndex(CntLat);

                int TerWest = (int)(TerLngInd - mapPosts / 2);
                int TerSouth = (int)(TerLatInd - mapPosts / 2);
                if ((TerWest % 2) != 0) ++TerWest;
                if ((TerSouth % 2) != 0) ++TerSouth;
                NxtTerExt.West = TerWest;
                NxtTerExt.South = TerSouth;
                NxtTerExt.East = TerWest + mapSegments;
                NxtTerExt.North = TerSouth + mapSegments;
                int ImgWest = (int)(ImgLngInd - ImgWidth / 2);
                int ImgSouth = (int)(ImgLatInd - ImgHeight / 2);
                NxtImgExt.West = ImgWest;
                NxtImgExt.South = ImgSouth;
                NxtImgExt.East = ImgWest + ImgWidth;
                NxtImgExt.North = ImgSouth + ImgHeight;
                UpdateTerOrg();
                UpdateImgOrg();
            }

            public void UpdExtents(Level finer, int _fillPatchSegments)
            {
                NxtTerExt.West = finer.NxtTerExt.West / 2 - _fillPatchSegments;
                NxtTerExt.East = NxtTerExt.West + mapSegments;

                NxtTerExt.South = finer.NxtTerExt.South / 2 - _fillPatchSegments;
                NxtTerExt.North = NxtTerExt.South + mapSegments;

                int ImgWest = (int)Imagery.LongitudeToIndex(Terrain.IndexToLongitude(NxtTerExt.West));
                int ImgSouth = (int)Imagery.LatitudeToIndex(Terrain.IndexToLatitude(NxtTerExt.South));
                NxtImgExt.West = ImgWest;
                NxtImgExt.South = ImgSouth;
                NxtImgExt.East = ImgWest + ImgWidth - 1;
                NxtImgExt.North = ImgSouth + ImgHeight - 1;

                UpdateTerOrg();
                UpdateImgOrg();
            }

            private void UpdateTerOrg()
            {
                int deltaX = NxtTerExt.West - CurTerExt.West;
                int deltaY = NxtTerExt.South - CurTerExt.South;
                if (deltaX == 0 && deltaY == 0) return;

                if (CurTerExt.West > CurTerExt.East || Math.Abs(deltaX) >= mapPosts || Math.Abs(deltaY) >= mapPosts)
                {  // complete update
                    TerOrg.x = 0.0;
                    TerOrg.y = 0.0;
                }
                else
                {
                    int newOriginX = ((int)TerOrg.x + deltaX) % mapPosts;
                    if (newOriginX < 0) newOriginX += mapPosts;
                    int newOriginY = ((int)TerOrg.y + deltaY) % mapPosts;
                    if (newOriginY < 0) newOriginY += mapPosts;
                    TerOrg.x = newOriginX;
                    TerOrg.y = newOriginY;
                }
            }

            private void UpdateImgOrg()
            {
                int deltaX = NxtImgExt.West - CurImgExt.West;
                int deltaY = NxtImgExt.South - CurImgExt.South;
                if (deltaX == 0 && deltaY == 0) return;

                if (CurImgExt.West > CurImgExt.East || Math.Abs(deltaX) >= ImgWidth || Math.Abs(deltaY) >= ImgHeight)      // complete update
                {
                    ImgOrg.x = 0.0;
                    ImgOrg.y = 0.0;
                }
                else {
                    int newOriginX = ((int)ImgOrg.x + deltaX) % ImgWidth;
                    if (newOriginX < 0) newOriginX += ImgWidth;
                    int newOriginY = ((int)ImgOrg.y + deltaY) % ImgHeight;
                    if (newOriginY < 0) newOriginY += ImgHeight;
                    ImgOrg.x = newOriginX;
                    ImgOrg.y = newOriginY;
                }
            }

            public Update[] GetUpdates(DATA_TYPE type)
            {
                Extent CurExt, NxtExt;
                int refwidth, refheight;
                ClipmapUpdate[] Updates = new ClipmapUpdate[2];
                Updates[0] = null; Updates[1] = null;

                if (type == DATA_TYPE.TERRAIN)
                {
                    CurExt = CurTerExt; NxtExt = NxtTerExt;
                    refwidth = mapPosts; refheight = mapPosts;
                }
                else
                {
                    CurExt = CurImgExt; NxtExt = NxtImgExt;
                    refwidth = ImgWidth; refheight = ImgHeight;
                }

                int deltaX = NxtExt.West - CurExt.West;
                int deltaY = NxtExt.South - CurExt.South;
                if (!first_time && deltaY == 0 && deltaX == 0) return Updates;

                int minLat = deltaY > 0 ? CurExt.North + 1 : NxtExt.South;
                int maxLat = deltaY > 0 ? NxtExt.North : CurExt.South - 1;
                int height = maxLat - minLat + 1;

                int minLng = deltaX > 0 ? CurExt.East + 1 : NxtExt.West;
                int maxLng = deltaX > 0 ? NxtExt.East : CurExt.West - 1;
                int width = maxLng - minLng + 1;

                if (first_time || CurExt.West > CurExt.East || width >= refwidth || height >= refheight)
                {
                    height = refheight;
                    deltaY = refheight;
                    minLat = NxtExt.South;
                    maxLat = NxtExt.North;
                    width = refwidth;
                    deltaX = refwidth;
                    minLng = NxtExt.West;
                    maxLng = NxtExt.East;
                    first_time = false;
                }
                if (height > 0) Updates[0] = new Update(this, NxtExt.West, minLat, NxtExt.East, maxLat);
                if (width > 0) Updates[1] = new Update(this, minLng, NxtExt.South, maxLng, NxtExt.North);

                CurExt.West = NxtExt.West;
                CurExt.East = NxtExt.East;
                CurExt.South = NxtExt.South;
                CurExt.North = NxtExt.North;
                return Updates;
            }
            
            public virtual bool FrustumCull() { return true; }
            
            public Vector2 GetAvgGroundHeight(double longitude, double latitude)
            {
                Level Terrain1 = null;
                TileTexture tiletexture = null;
                if (Terrain.Id < 12) Terrain1 = Terrain;
                else Terrain1 = DataBase.Terrain.Levels[11];

                double TerLngInd = Terrain1.LongitudeToIndex(longitude);
                double TerLatInd = Terrain1.LatitudeToIndex(latitude);

                int X = (int)(TerLngInd / Terrain1.LongitudePostsPerTile);
                int Y = (int)(TerLatInd / Terrain1.LatitudePostsPerTile);

                tile1.Update(Terrain1.Id, X, Y);
                Terrain1.Loaded.TryGetValue(tile1, out tiletexture);
                if (tiletexture == null)
                {
                    if (countera > 0)
                    {
                        ground_height.x = ground_avg_height / countera;
                        ground_height.y = ground_height.x;
                        return ground_height;
                    }
                    ground_height.x = -10001.0f;
                    ground_height.y = -10001.0f;
                    return ground_height;
                }

                int col = X % Terrain1.LongitudePostsPerTile;
                int row = Y % Terrain1.LatitudePostsPerTile;
                float height = Mathf.HalfToFloat(tiletexture.tile.Data.data[row * Terrain1.LatitudePostsPerTile + col]);

                ground_avg_height -= ground_values[counterh];
                ground_values[counterh] = height;
                ground_avg_height += ground_values[counterh];

                counterh++;
                if (countera < 1000) countera++;
                if (counterh == 1000) counterh = 0;
                if (height > ground_avg_height)
                {
                    ground_height.x = height;
                    ground_height.y = height;
                    return ground_height;
                }
                ground_height.x = height;
                ground_height.y = ground_avg_height / countera;
                return ground_height;
            }
        }
    }
}
