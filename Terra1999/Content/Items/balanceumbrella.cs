using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terra1999.Events; // 引用你的事件系统

namespace Terra1999.Content.Items
{
    public class BalanceUmbrella : ModItem
    {
        private int timer = 0;

        public override void SetStaticDefaults()
{
    // DisplayName 和 Tooltip 现在通过 .hjson 本地化文件自动关联
}

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 5;
            Item.useAnimation = 5;
            Item.useStyle = ItemUseStyleID.HoldUp; // 使用举起的用法
            Item.mana = 0; // 我们手动处理魔力消耗
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(0, 5, 0, 0);
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = false;
        }
        public override void HoldItem(Player player)
        {
            if (RainstormEvent.RainstormActive)
            {
                // 这里给玩家添加一个免疫标签，暴雨伤害系统会检查这个标签
                player.GetModPlayer<Terra1999Player>().hasUmbrellaProtection = true;
                
                // 可选：添加一些视觉效果表明保护生效
                if (Main.rand.NextBool(10))
                {
                    Dust dust = Dust.NewDustDirect(player.position, player.width, player.height, 
                                                  DustID.Electric, 0f, 0f, 100, Color.Blue);
                    dust.noGravity = true;
                    dust.scale = 0.8f;
                }
            }
        }

        public override bool CanUseItem(Player player)
        {
            // 检查玩家是否有足够魔力
            return player.statMana >= 10;
        }

        public override bool? UseItem(Player player)
        {
            timer++;

            // 每秒消耗10点魔力（60帧=1秒）
            if (timer >= 60)
            {
                player.statMana -= 10;
                timer = 0;

                // 如果魔力不足，停止使用
                if (player.statMana < 10)
                {
                    return false;
                }
            }

            // 消除投射体
            ClearProjectiles(player.Center, 15f);

            // 产生一些视觉效果
            for (int i = 0; i < 3; i++)
            {
                Dust dust = Dust.NewDustDirect(
                    player.position,
                    player.width,
                    player.height,
                    DustID.MagicMirror,
                    Alpha: 150
                );
                dust.noGravity = true;
                dust.velocity *= 0.5f;
            }

            return true;
        }

        private void ClearProjectiles(Vector2 center, float radiusInTiles)
        {
            float radiusInPixels = radiusInTiles * 16f; // 将格子转换为像素
            float radiusSquared = radiusInPixels * radiusInPixels;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];

                // 检查投射体是否存在、活跃且是敌对投射物
                if (proj.active && proj.hostile)
                {
                    // 计算距离
                    float distanceSquared = Vector2.DistanceSquared(center, proj.Center);

                    // 如果在范围内，消除投射体
                    if (distanceSquared <= radiusSquared)
                    {
                        proj.active = false;
                        // 产生消除效果
                        for (int j = 0; j < 5; j++)
                        {
                            Dust dust = Dust.NewDustDirect(
                                proj.position,
                                proj.width,
                                proj.height,
                                DustID.Clentaminator_Blue,
                                Alpha: 150
                            );
                            dust.velocity = proj.velocity * 0.2f;
                            dust.noGravity = true;
                        }
                    }
                }
            }
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.DirtBlock, 10);
            recipe.AddTile(TileID.WorkBenches);
            recipe.Register();
        }
    }
}
