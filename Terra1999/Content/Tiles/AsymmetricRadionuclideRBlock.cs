using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace Terra1999.Tiles
{
    public class AsymmetricRadionuclideRBlock : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = true;
            
            // 设置挖掘属性
            MineResist = 2f;
            MinPick = 70; // 需要70%镐力
            
            // 设置地图颜色和名称
            AddMapEntry(new Color(100, 200, 255), CreateMapEntryName());
            
            // 设置掉落物品
            RegisterItemDrop(ModContent.ItemType<Items.AsymmetricRadionuclideRBlockItem>());
            
            // 音效和尘埃
            HitSound = SoundID.Tink;
            DustType = DustID.Electric;
        }
        
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            // 发出微弱的蓝光，体现其放射性
            r = 0.1f;
            g = 0.3f;
            b = 0.5f;
        }
        
        public override bool CanExplode(int i, int j)
        {
            return false; // 防止爆炸破坏
        }
    }
}

namespace Terra1999.Items
{
    public class AsymmetricRadionuclideRBlockItem : ModItem
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
            Item.createTile = ModContent.TileType<Tiles.AsymmetricRadionuclideRBlock>();
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(0, 0, 1, 0);
        }

        public override void AddRecipes()
        {
            // 通过维尔汀NPC购买获得，不需要合成配方
        }
    }
}