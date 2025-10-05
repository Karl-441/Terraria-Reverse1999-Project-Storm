using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terra1999.Events;

namespace Terra1999.NPCs
{
    public class Musician : ModNPC
    {
        public override void SetStaticDefaults()
        {
            // 设置怪物名称，实际显示名通过本地化文件配置

            // 这里可以配置与动画帧数等相关的主城属性
            Main.npcFrameCount[NPC.type] = Main.npcFrameCount[NPCID.Zombie]; // 暂时借用原版僵尸的动画帧数
        }

        public override void SetDefaults()
        {
            // 基础属性
            NPC.width = 18; // 怪物碰撞箱宽度
            NPC.height = 40; // 怪物碰撞箱高度
            NPC.damage = 20; // 接触伤害
            NPC.defense = 5; // 防御力
            NPC.lifeMax = 100; // 最大生命值
            NPC.HitSound = SoundID.NPCHit1; // 受击音效
            NPC.DeathSound = SoundID.NPCDeath2; // 死亡音效

            // AI与行为
            NPC.aiStyle = 3; // 参考原版僵尸的AI风格
            AIType = NPCID.Zombie; // 明确指定AI类型为僵尸
            AnimationType = NPCID.Zombie; // 动画类型也指定为僵尸

            // 敌对设置
            NPC.noTileCollide = false; // 遵循图格碰撞
            NPC.noGravity = false; // 受重力影响
            NPC.knockBackResist = 0.5f; // 击退抗性

            // 阵营与掉落
            NPC.value = 100f; // 击杀后掉落的钱币价值基础
            Banner = Item.NPCtoBanner(NPCID.Zombie); // 旗帜相关（可选）
            BannerItem = Item.BannerToItem(Banner); // 旗帜物品（可选）
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            // 确保只在暴雨事件中生成
            if (RainstormEvent.RainstormActive)
            {
                // 可以在此处添加其他生成条件，例如玩家不在特定保护区域内
                return 0.5f; // 生成权重，可调整
            }
            return 0f; // 非暴雨事件不生成
        }

        // 可选：自定义AI，让怪物行为更有特色
        public override void AI()
        {
            // 调用基础AI (基于NPC.aiStyle)
            base.AI();

            // 示例：在暴雨中偶尔产生水花粒子（需要先配置好粒子）
            // 这里可以添加自定义逻辑
        }

        // 可选：修改掉落物
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // 添加基础掉落（例如，有1/2几率掉落2-3个礼乐卷轴）
            // npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<LeafScroll>(), 2, 2, 3));
            // 添加其他特定掉落物
        }

        // 可选：受击时效果
        public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
        }
    }
}