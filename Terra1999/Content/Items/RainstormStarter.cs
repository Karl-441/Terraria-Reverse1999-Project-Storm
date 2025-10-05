using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Terra1999.Items
{
    public class RainstormStarter : ModItem
    {
        public override void SetStaticDefaults()
        {
            // 在 .hjson 文件中设置显示名称和提示
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = 1;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = false;
            Item.rare = ItemRarityID.Red;
            Item.value = Item.sellPrice(0, 5, 0, 0);
        }

        public override bool CanUseItem(Player player)
        {
            // 检查模组是否已解锁且暴雨事件未激活
            Terra1999Player modPlayer = player.GetModPlayer<Terra1999Player>();
            return modPlayer.modUnlocked && !Events.RainstormEvent.RainstormActive;
        }

        public override bool? UseItem(Player player)
        {
            Events.RainstormEvent.StartRainstorm();
            return true;
        }

        public override void AddRecipes()
        {
        }
    }
}