using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Microsoft.Xna.Framework;

namespace Terra1999.Tiles
{
    public class AsymmetricRadionuclideRWorkbench : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileTable[Type] = true;
            Main.tileSolidTop[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileFrameImportant[Type] = true;
            TileID.Sets.DisableSmartCursor[Type] = true;
            TileID.Sets.IgnoredByNpcStepUp[Type] = true;
            
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
            TileObjectData.newTile.CoordinateHeights = new[] { 18 };
            TileObjectData.addTile(Type);
            
            AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);
            AddMapEntry(new Color(100, 170, 230), CreateMapEntryName());
            
            RegisterItemDrop(ModContent.ItemType<Items.AsymmetricRadionuclideRBlockItem>());
            DustType = DustID.Electric;
            AdjTiles = new int[] { TileID.WorkBenches };
        }
        
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.05f;
            g = 0.15f;
            b = 0.3f;
        }
    }
}

namespace Terra1999.Items
{
    public class AsymmetricRadionuclideRWorkbenchItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            // 名称和提示文本在 .hjson 文件中设置
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 18;
            Item.maxStack = 99;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.AsymmetricRadionuclideRWorkbench>();
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(0, 0, 2, 0);
        }

        public override void AddRecipes()
        {
            // 通过维尔汀NPC购买获得
        }
    }
}