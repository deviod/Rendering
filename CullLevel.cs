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
        public enum TPR
        {
            Inside = 0,
            Intersect,
            Outside
        }

        public class CullLevel
        {
            Geodetic3D levelPos = new Geodetic3D(0.0f, 0.0f, 0.0f);

            public CullLevel(int id, int Posts, int Segments) : base(id, Posts, Segments)
            {
                
            }
            
            public override bool FrustumCull()
            {
                index = 0;
                min_height = -10000.0;
                max_height = -10000.0;
                if (id > Globals.elevel || id < Globals.slevel) return true;

                maxi_height = 0.0f;
                CullBlock(CurTerExt.West, CurTerExt.South, -1, 1);

                return true;
            }

            public void CullBlock(int basewest, int basesouth, int slid, int depth)
            {
                int blockwest, blocksouth;
                int segments = ClipmapRenderer.Segments[depth];
                bool fillRing = id == Globals.elevel ? false : true;
                int[] West = { basewest, basewest + segments, basewest, basewest + segments };
                int[] South = { basesouth, basesouth, basesouth + segments, basesouth + segments };

                for (int i = 0; i < 4; i++)
                {
                    if (depth == 2 && fillRing && i == slid) continue;
                    blockwest = West[i]; blocksouth = South[i];
                    TPR res = IsCulled(blockwest, blocksouth, ClipmapRenderer.Extents[depth]);
                    if (res == TPR.Outside) continue;

                    if (res == TPR.Inside && depth > 1) DrawBlocks(blockwest, blocksouth, depth);
                    else
                    {
                        if (ClipmapRenderer.quad_depth == depth) DrawBlocks(blockwest, blocksouth, depth);
                        else CullBlock(blockwest, blocksouth, 3-i, depth + 1);
                    }
                }
            }

            private TPR IsCulled(int blockWest, int blockSouth, RectangleD block)
            {
                Vector2 minPos, maxPos;
                if (Clipmap.planes == null) return TPR.Outside;
                if (blockWest >= 0 && blockSouth >= 0)
                {
                    int delx = blockWest - CurTerExt.West;
                    int dely = blockSouth - CurTerExt.South;
                    minPos.x = block.LowerLeft_f.x + delx;
                    minPos.y = block.LowerLeft_f.y + dely;
                    maxPos.x = block.UpperRight_f.x + delx;
                    maxPos.y = block.UpperRight_f.y + dely;
                    Vector3 minv3 = SurfacePos(minPos);
                    if (levelPos.Height == -10001.0f) return TPR.Outside;
                    Vector3 maxv3 = SurfacePos(maxPos);
                    if (levelPos.Height == -10001.0f) return TPR.Outside;
                    return FrustumCull(Clipmap.planes, ref minv3, ref maxv3);
                }
                else return TPR.Outside;
            }

            private void DrawBlocks(int blockwest, int blocksouth, int depth)
            {
                int segments = ClipmapRenderer.Segments[depth];
                int blockeast = blockwest + segments;
                int blocknorth = blocksouth + segments;
                int inc = ClipmapRenderer.Segments[ClipmapRenderer.quad_depth];
                Vector2 block = ClipmapRenderer.Extents[ClipmapRenderer.quad_depth].LowerLeft_f + ClipmapRenderer.Extents[ClipmapRenderer.quad_depth].UpperRight_f;
                block.x /= 2; block.y /= 2;

                uint idy = (uint)((blocksouth - CurTerExt.South) / inc);
                for (int y = blocksouth; y < (blocknorth - inc / 2); y += inc)
                {
                    uint idx = (uint)((blockwest - CurTerExt.West) / inc);
                    for (int x = blockwest; x < (blockeast - inc / 2); x += inc)
                    {
                        blocks[index] = 0;
                        blocks[index] |= (idx << 25);
                        blocks[index] |= (idy << 18);
                        blocks[index] |= (((uint)id) << 13);
                        uint del_ge = SSE(block.x + x - CurTerExt.West + (Globals.Earth.blockPosts - 1) / 2, block.y + y - CurTerExt.South + (Globals.Earth.blockPosts - 1) / 2);
                        blocks[index] |= (del_ge & mask_ge);
                        index++;
                        idx++;
                    }
                    idy++;
                }
            }

            public Vector3 SurfacePos(Vector2 pos)
            {
                levelPos.Longitude = ((pos.x + OffsetWorldOrigin.x) * level_factor.x) * RadianPerDeg;
                levelPos.Latitude = ((pos.y + OffsetWorldOrigin.y) * level_factor.y) * RadianPerDeg;
                levelPos.Height = GetGroundHeight(levelPos.Longitude / RadianPerDeg, levelPos.Latitude / RadianPerDeg);
                if (levelPos.Height <= -9999.0f) levelPos.Height = Clipmap.cur_grd_height;
                return Clipmap.Map_local_2_world.MultiplyVector(Ellipsoid.ToVector3(Globals.Earth.ellipsoid.ToVector3d(levelPos)));
            }

            public uint SSE(double pos_x, double pos_y)
            {
                levelPos.Longitude = ((pos_x + OffsetWorldOrigin.x) * level_factor.x) * RadianPerDeg;
                levelPos.Latitude = ((pos_y + OffsetWorldOrigin.y) * level_factor.y) * RadianPerDeg;
                double grd_h = GetGroundHeight(levelPos.Longitude / RadianPerDeg, levelPos.Latitude / RadianPerDeg);
                if (grd_h == -10001.0f) grd_h = (GetAvgGroundHeight(levelPos.Longitude / RadianPerDeg, levelPos.Latitude / RadianPerDeg)).x;
                
                levelPos.Height = grd_h + 5.0f;
                Vector4 upper_point = Local2Screen(Ellipsoid.ToVector4(Globals.Earth.ellipsoid.ToVector3d(levelPos)));
                levelPos.Height = grd_h - 5.0f;
                Vector4 lower_point = Local2Screen(Ellipsoid.ToVector4(Globals.Earth.ellipsoid.ToVector3d(levelPos)));
                Vector4 delta = upper_point - lower_point;
                float val = delta.x * delta.x + delta.y * delta.y;
                if (val != 0.0f) return (uint)(10.0f * Globals.Earth.scr_err / Math.Sqrt(val));
                return 0;
            }

            public Vector4 Local2Screen(Vector4 lp)
            {
                Vector4 temp = Clipmap.Map_local_2_proj * lp;
                if (temp.w == 0f) return Vector4.zero;
                temp.x = (temp.x / temp.w + 1f) * .5f * Clipmap.pixelWidth;
                temp.y = (temp.y / temp.w + 1f) * .5f * Clipmap.pixelHeight;
                return temp;
            }

            public TPR FrustumCull(Plane[] planes, ref Vector3 boundsMin, ref Vector3 boundsMax, bool ti = true)
            {
                Vector3 vmin, vmax;

                if (Globals.wireframe_display)
                {
                    return TPR.Inside;
                }

                for (int planeIndex = 0; planeIndex < planes.Length - 1; planeIndex++)
                {
                    var normal = planes[planeIndex].normal;
                    var planeDistance = planes[planeIndex].distance;
                    if (normal.x < 0) { vmin.x = boundsMin.x; vmax.x = boundsMax.x; }
                    else { vmin.x = boundsMax.x; vmax.x = boundsMin.x; }

                    if (normal.y < 0) { vmin.y = boundsMin.y; vmax.y = boundsMax.y; }
                    else { vmin.y = boundsMax.y; vmax.y = boundsMin.y; }

                    if (normal.z < 0) { vmin.z = boundsMin.z; vmax.z = boundsMax.z; }
                    else { vmin.z = boundsMax.z; vmax.z = boundsMin.z; }

                    var dot1 = normal.x * vmin.x + normal.y * vmin.y + normal.z * vmin.z;
                    if (dot1 + planeDistance <= 0) return TPR.Outside;
                    if (ti)
                    {
                        var dot2 = normal.x * vmax.x + normal.y * vmax.y + normal.z * vmax.z;
                        if (dot2 + planeDistance <= 0) return TPR.Intersect;
                    }
                }
                return TPR.Inside;
            }

            public float GetGroundHeight(double longitude, double latitude)
            {
                Level Terrain1 = null;
                TileTexture tiletexture = null;
                if (Terrain.Id < 12) Terrain1 = Terrain;
                else Terrain1 = DataBase.Terrain.Levels[11];

                double TerLngInd = Terrain1.LongitudeToIndex(longitude);
                double TerLatInd = Terrain1.LatitudeToIndex(latitude);

                int X = (int)(TerLngInd / Terrain1.LongitudePostsPerTile);
                int Y = (int)(TerLatInd / Terrain1.LatitudePostsPerTile);

                tile.Update(Terrain1.Id, X, Y);
                Terrain1.Loaded.TryGetValue(tile, out tiletexture);
                if (tiletexture == null) return -10001.0f;

                int col = X % Terrain1.LongitudePostsPerTile;
                int row = Y % Terrain1.LatitudePostsPerTile;
                return Mathf.HalfToFloat(tiletexture.tile.Data.data[row * Terrain1.LatitudePostsPerTile + col]);
            }

            public TPR isInside(Vector2 minpos, Vector2 maxpos, float delx, float dely)
            {
                minPos.x = minpos.x + delx;
                minPos.y = minpos.y + dely;
                maxPos.x = maxpos.x + delx;
                maxPos.y = maxpos.y + dely;
                Vector3 minv3 = GetBounds(minPos);
                if (levelPos.Height == -10001.0f) return TPR.Outside;
                Vector3 maxv3 = GetBounds(maxPos);
                if (levelPos.Height == -10001.0f) return TPR.Outside;
                return FrustumCull(Clipmap.planes, ref minv3, null);
            }
            
            private float GetHeight(int Index_X, int Index_Y)
            {
                TileTexture tiletexture = null;
                if (Terrain.Id < 12)
                {
                    int X = Index_X / Terrain.LongitudePostsPerTile;
                    int Y = Index_Y / Terrain.LatitudePostsPerTile;
                    tile.Update(Terrain.Id, X, Y);
                    Terrain.Loaded.TryGetValue(tile, out tiletexture);
                }
                else
                {
                    int X = Index_X / DataBase.Terrain.Levels[11].LongitudePostsPerTile;
                    int Y = Index_Y / DataBase.Terrain.Levels[11].LatitudePostsPerTile;
                    tile.Update(11, X, Y);
                    Terrain.Loaded.TryGetValue(tile, out tiletexture);
                }
                if (tiletexture == null) return maxi_height;

                int row = Index_X % Terrain.LongitudePostsPerTile;
                int col = Index_Y % Terrain.LatitudePostsPerTile;
                float height = Mathf.HalfToFloat(tiletexture.tile.Data.data[row * Terrain.LongitudePostsPerTile + col]);
                if (max_height < height) maxi_height = height;
                if (height <= -9998.5) height = maxi_height;
                return height;
            }

            uint mask_idx = 0xFE000000;
            uint mask_idy = 0x01FC0000;
            uint mask_lid = 0x0003E000;
            uint mask_ge  = 0x00001FFF;
        }
    }
}
