using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Microsoft.Xna.Framework;

namespace Terra1999.Tiles
{
    public class AsymmetricRadionuclideRPlatform : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileLighted[Type] = true;
            Main.tileFrameImportant[Type] = true;
            Main.tileSolidTop[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileTable[Type] = true;
            Main.tileLavaDeath[Type] = true;
            
            TileID.Sets.Platforms[Type] = true;
            TileObjectData.newTile.CoordinateHeights = new[] { 16 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.StyleMultiplier = 27;
            TileObjectData.newTile.StyleWrapLimit = 27;
            TileObjectData.newTile.UsesCustomCanPlace = false;
            TileObjectData.addTile(Type);
            
            AddToArray(ref TileID.Sets.RoomNeeds.CountsAsDoor);
            AddMapEntry(new Color(120, 180, 240), CreateMapEntryName());
            
            RegisterItemDrop(ModContent.ItemType<Items.AsymmetricRadionuclideRBlockItem>());
            DustType = DustID.Electric;
            AdjTiles = new int[] { TileID.Platforms };
        }
        
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.08f;
            g = 0.25f;
            b = 0.4f;
        }
    }
}

namespace Terra1999.Items
{
    public class AsymmetricRadionuclideRPlatformItem : ModItem
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
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.AsymmetricRadionuclideRPlatform>();
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(0, 0, 0, 75);
        }

        public override void AddRecipes()
        {
            // 通过维尔汀NPC购买获得
        }
    }
}