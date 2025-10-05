using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace Terra1999.Walls
{
    public class AsymmetricRadionuclideRWall : ModWall
    {
        public override void SetStaticDefaults()
        {
            Main.wallHouse[Type] = true;
            DustType = DustID.Electric;
            
            AddMapEntry(new Color(80, 160, 220), CreateMapEntryName());
        }
    }
}

namespace Terra1999.Items
{
    public class AsymmetricRadionuclideRWallItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            // 名称和提示文本在 .hjson 文件中设置
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = 999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 7;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createWall = ModContent.WallType<Walls.AsymmetricRadionuclideRWall>();
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(0, 0, 0, 50);
        }

        public override void AddRecipes()
        {
            // 通过维尔汀NPC购买获得
        }
    }
}