using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terra1999.Items;

namespace Terra1999.Content.Items
{
    // 坐骑召唤物品
    public class Su01veKey : ModItem
    {
        public override void SetStaticDefaults()
        {
            // 将物品与坐骑关联
            ItemID.Sets.ItemNoGravity[Item.type] = true; // 物品无重力
            Item.mountType = ModContent.MountType<Mounts.Su01veMount>();
        }
       public override bool CanUseItem(Player player)
        {
            // 检查模组是否已解锁
            // 注意：这里假设Terra1999Player类存在，如果不存在需要创建
            Terra1999Player modPlayer = player.GetModPlayer<Terra1999Player>();
            if (!modPlayer.modUnlocked)
            {
                Main.NewText("某位空中女巫小姐还没答应你可以使用。", Color.Orange);
                 return false;
             }
            return true;
        }
        
        public override void SetDefaults()
        {
            // 物品属性设置
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.value = Item.buyPrice(gold: 5); // 价值5金币 
            Item.UseSound = SoundID.Item79; // 使用时的声音
            Item.noMelee = true;
            Item.consumable = false; // 不可消耗，可重复使用
        }
        
        // 可以在这里添加物品的合成配方
        public override void AddRecipes()
        {
            // 需要通讯器解锁后才能制作
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.HallowedBar, 15);
            recipe.AddIngredient(ItemID.SoulofFlight, 10);
            recipe.AddIngredient(ItemID.Wire, 20);
            recipe.AddIngredient(ModContent.ItemType<FoundationCommunicator>()); // 需要通讯器
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
    }
}