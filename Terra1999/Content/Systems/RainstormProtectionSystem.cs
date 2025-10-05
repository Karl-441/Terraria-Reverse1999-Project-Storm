using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terra1999.Events;
using Terraria.ID;

namespace Terra1999.Content.Systems
{
    public class RainstormProtectionSystem : ModSystem
    {
        /// <summary>
        /// 检查玩家是否处于受保护的房屋中
        /// </summary>
        public static bool IsPlayerProtected(Player player)
{
    // 方法一：检查玩家当前位置是否是一个有效的房屋
    // 通过玩家中心的坐标来查询此位置是否被系统认定为有效的房屋空间
    int playerHomeX = (int)(player.Center.X / 16f); // 将玩家中心的像素坐标转换为图格坐标
    int playerHomeY = (int)(player.Center.Y / 16f);
    
    // 关键修改：使用 WorldGen.IsHouseTile 或其他房屋逻辑来判断
    // 这里我们检查玩家所在位置附近的一个区域是否符合房屋特征
    return DoesAreaQualifyAsHouse(playerHomeX, playerHomeY);
}

/// <summary>
/// 检查指定区域是否符合房屋条件
/// </summary>
private static bool DoesAreaQualifyAsHouse(int centerX, int centerY)
{
    // 定义检查范围，例如以玩家为中心检查一个 10x10 图格的区域
    int checkRange = 10;
    int validHouseTiles = 0;

    for (int x = centerX - checkRange; x <= centerX + checkRange; x++)
    {
        for (int y = centerY - checkRange; y <= centerY + checkRange; y++)
        {
            if (WorldGen.InWorld(x, y))
            {
                // 示例逻辑：如果区域内存在足够的“安全”图格（如椅子、桌子、工作台等家具），则认为是房屋
                // 注意：这是一个简化的逻辑，你需要根据你的模组防护机制来定义何为“有效”图格
                // 例如，你可以检查是否存在非对称核素R砖块
                Tile tile = Main.tile[x, y];
                if (tile.HasTile && IsProtectedTile(tile.TileType))
                {
                    validHouseTiles++;
                }
            }
        }
    }
    
    // 如果受保护的图格数量达到某个阈值，则认为该区域是合格的房屋
    int threshold = 15; // 这个阈值需要你根据实际情况调整
    return validHouseTiles >= threshold;
}
        private static bool IsHouseProtected(int homeX, int homeY)
        {
            // 定义检查范围（房屋的大致尺寸）
            int checkRadius = 20;
            int protectedBlockCount = 0;
            int totalBlockCount = 0;

            // 检查房屋区域内的方块
            for (int x = homeX - checkRadius; x <= homeX + checkRadius; x++)
            {
                for (int y = homeY - checkRadius; y <= homeY + checkRadius; y++)
                {
                    if (WorldGen.InWorld(x, y))
                    {
                        Tile tile = Main.tile[x, y];
                        if (tile.HasTile && tile.TileType > 0)
                        {
                            totalBlockCount++;
                            // 检查是否为非对称核素R方块
                            if (IsProtectedTile(tile.TileType))
                            {
                                protectedBlockCount++;
                            }
                        }
                    }
                }
            }

            // 如果超过80%的方块是保护性方块，则认为房屋受保护
            return totalBlockCount > 0 && (float)protectedBlockCount / totalBlockCount >= 0.8f;
        }

        /// <summary>
        /// 检查图格类型是否为受保护的非对称核素R材料
        /// </summary>
        private static bool IsProtectedTile(ushort tileType)
        {
            // 所有非对称核素R材料的图格类型
            var protectedTiles = new List<ushort>
            {
                (ushort)ModContent.TileType<Tiles.AsymmetricRadionuclideRBlock>(),
                (ushort)ModContent.TileType<Tiles.AsymmetricRadionuclideRPlatform>(),
                (ushort)ModContent.TileType<Tiles.AsymmetricRadionuclideRWorkbench>()
                // 可以在这里添加更多非对称核素R家具
            };

            return protectedTiles.Contains(tileType);
        }

        /// <summary>
        /// 检查玩家当前位置是否在保护区域内
        /// </summary>
        public static bool IsPlayerInProtectedArea(Player player)
        {
            // 将玩家位置转换为图格坐标
            int playerTileX = (int)(player.Center.X / 16);
            int playerTileY = (int)(player.Center.Y / 16);

            // 检查玩家周围的保护方块密度
            int checkRadius = 15; // 检查半径（图格数）
            int protectedBlockCount = 0;
            int totalBlockCount = 0;

            for (int x = playerTileX - checkRadius; x <= playerTileX + checkRadius; x++)
            {
                for (int y = playerTileY - checkRadius; y <= playerTileY + checkRadius; y++)
                {
                    if (WorldGen.InWorld(x, y))
                    {
                        Tile tile = Main.tile[x, y];
                        if (tile.HasTile && tile.TileType > 0)
                        {
                            totalBlockCount++;
                            if (IsProtectedTile(tile.TileType))
                            {
                                protectedBlockCount++;
                            }
                        }
                    }
                }
            }

            // 如果周围有足够多的保护方块，则认为玩家处于保护区域
            return totalBlockCount > 0 && (float)protectedBlockCount / totalBlockCount >= 0.7f;
        }

        public override void Load()
        {
            // 在暴雨事件应用伤害前检查保护状态
            On_Main.Update += CheckRainstormProtection;
        }

        public override void Unload()
        {
            // 清理钩子
            On_Main.Update -= CheckRainstormProtection;
        }

        private void CheckRainstormProtection(On_Main.orig_Update orig, Terraria.Main main, GameTime gameTime)
        {
            orig(main, gameTime);

            if (RainstormEvent.RainstormActive)
            {
                // 检查所有玩家是否受到保护
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player player = Main.player[i];
                    if (player.active && !player.dead)
                    {
                        bool isProtected = IsPlayerProtected(player) || 
                                          IsPlayerInProtectedArea(player) ||
                                          player.GetModPlayer<Terra1999Player>().hasUmbrellaProtection;

                        if (isProtected)
                        {
                            // 为受保护的玩家添加视觉效果
                            if (Main.rand.NextBool(10))
                            {
                                Vector2 dustPosition = player.Center + 
                                    new Vector2(Main.rand.Next(-player.width / 2, player.width / 2), 
                                               Main.rand.Next(-player.height / 2, player.height / 2));
                                
                                Dust dust = Dust.NewDustPerfect(
                                    dustPosition,
                                    DustID.Electric,
                                    Vector2.Zero,
                                    100, 
                                    Color.Cyan, 
                                    0.8f);
                                dust.noGravity = true;
                                dust.velocity *= 0.1f;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取保护状态信息（用于调试或UI显示）
        /// </summary>
        public static string GetProtectionStatus(Player player)
        {
            if (!RainstormEvent.RainstormActive)
                return "暴雨未激活";

            bool houseProtected = IsPlayerProtected(player);
            bool areaProtected = IsPlayerInProtectedArea(player);
            bool umbrellaProtected = player.GetModPlayer<Terra1999Player>().hasUmbrellaProtection;

            if (umbrellaProtected)
                return "平衡伞保护中";
            else if (houseProtected)
                return "房屋保护中";
            else if (areaProtected)
                return "区域保护中";
            else
                return "未受保护";
        }

        /// <summary>
        /// 计算房屋的保护百分比（用于UI显示）
        /// </summary>
        public static float GetHouseProtectionPercentage(int homeX, int homeY)
        {
            int checkRadius = 20;
            int protectedBlockCount = 0;
            int totalBlockCount = 0;

            for (int x = homeX - checkRadius; x <= homeX + checkRadius; x++)
            {
                for (int y = homeY - checkRadius; y <= homeY + checkRadius; y++)
                {
                    if (WorldGen.InWorld(x, y))
                    {
                        Tile tile = Main.tile[x, y];
                        if (tile.HasTile && tile.TileType > 0)
                        {
                            totalBlockCount++;
                            if (IsProtectedTile(tile.TileType))
                            {
                                protectedBlockCount++;
                            }
                        }
                    }
                }
            }

            return totalBlockCount > 0 ? (float)protectedBlockCount / totalBlockCount : 0f;
        }
    }
}