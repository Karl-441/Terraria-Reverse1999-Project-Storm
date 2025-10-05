using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Terra1999.Items
{
    public class FoundationCommunicator : ModItem
    {
        public override void SetStaticDefaults()
        {
            // 在 .hjson 文件中设置显示名称和提示
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 28;
            Item.maxStack = 1;
            Item.useTime = 45;
            Item.useAnimation = 45;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = false;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 1, 0, 0);
        }

        public override bool CanUseItem(Player player)
        {
            // 检查是否已经激活过
            Terra1999Player modPlayer = player.GetModPlayer<Terra1999Player>();
            return !modPlayer.modUnlocked;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                Terra1999Player modPlayer = player.GetModPlayer<Terra1999Player>();
                modPlayer.modUnlocked = true;
                
                // 视觉效果
                for (int i = 0; i < 15; i++)
                {
                    Dust dust = Dust.NewDustDirect(player.position, player.width, player.height, 
                                                  DustID.Electric, 0f, 0f, 100, default, 1.5f);
                    dust.velocity *= 1.4f;
                    dust.noGravity = true;
                }
                
                // 音效和消息
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item4, player.position);
                Main.NewText("通讯器简单地作响两声，随后又归于沉寂...", 50, 200, 255);
            }
            return true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.MeteoriteBar, 10);
            recipe.AddIngredient(ItemID.Wire, 30);
            recipe.AddTile(TileID.WorkBenches);
            recipe.Register();
        }
    }
}