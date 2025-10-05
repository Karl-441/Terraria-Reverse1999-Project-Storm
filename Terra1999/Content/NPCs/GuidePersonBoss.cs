using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.IO; // 用于网络同步
using Terra1999.Events;

namespace Terra1999.NPCs
{
    [AutoloadBossHead]
    // 这个属性有助于tModLoader将其识别为Boss
    public class GuidePersonBoss : ModNPC
    {
        // 阶段控制
        private enum AttackState
        {
            Phase1Chase,
            Phase1Projectiles,
            Phase2Summon,
            Phase2Rage
        }
        private AttackState CurrentAttack
        {
            get => (AttackState)NPC.ai[0];
            set => NPC.ai[0] = (float)value;
        }
        private float AttackTimer
        {
            get => NPC.ai[1];
            set => NPC.ai[1] = value;
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 6; // 根据你的动画帧数设置
        }

        public override void SetDefaults()
        {
            NPC.width = 80; // Boss碰撞箱宽度
            NPC.height = 120; // Boss碰撞箱高度
            NPC.damage = 50; // 接触伤害
            NPC.defense = 15; // 防御力
            NPC.lifeMax = 20000; // 最大生命值
            NPC.HitSound = SoundID.NPCHit1; // 受击音效
            NPC.DeathSound = SoundID.NPCDeath10; // 死亡音效
            NPC.knockBackResist = 0f; // Boss通常免疫击退
            NPC.noGravity = true; // 是否不受重力影响
            NPC.noTileCollide = true; // 是否穿透物块
            NPC.boss = true; // 将其标记为Boss，这会影响一些游戏机制，比如Boss血条
            NPC.lavaImmune = true; // 免疫岩浆
            NPC.netAlways = true; // 重要的网络同步设置
            NPC.value = Item.buyPrice(1, 0, 0, 0); // 掉落钱币价值
        }

        public override void AI()
        {
            // 确保AI在多人模式下正确同步
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                NPC.TargetClosest();
            }
            Player player = Main.player[NPC.target];
            if (player.dead)
            {
                // 如果目标玩家死亡，Boss可能会消失或进入无敌状态
                NPC.velocity.Y -= 0.04f;
                if (NPC.timeLeft > 10)
                {
                    NPC.timeLeft = 10;
                }
                return;
            }

            // 根据当前状态执行不同的攻击模式
            switch (CurrentAttack)
            {
                case AttackState.Phase1Chase:
                    Phase1_ChasePlayer(player);
                    break;
                case AttackState.Phase1Projectiles:
                    Phase1_ShootProjectiles(player);
                    break;
                    // 可以添加更多阶段和行为
            }
            AttackTimer++;
        }

        private void Phase1_ChasePlayer(Player player)
        {
            float speed = 6f;
            float acceleration = 0.1f;
            Vector2 direction = player.Center - NPC.Center;
            direction.Normalize();
            direction *= speed;
            NPC.velocity = (NPC.velocity * (1 - acceleration)) + (direction * acceleration);

            // 追击一段时间后切换到发射弹幕状态
            if (AttackTimer > 300)
            { // 300帧后切换
                CurrentAttack = AttackState.Phase1Projectiles;
                AttackTimer = 0;
                NPC.netUpdate = true; // 状态改变时通知网络同步
            }
        }

        private void Phase1_ShootProjectiles(Player player)
        {
            // 停止移动或缓慢移动
            NPC.velocity *= 0.95f;

            // 每隔一段时间发射弹幕
            if (AttackTimer % 60 == 0)
            { // 每秒发射一次
                Vector2 toPlayer = player.Center - NPC.Center;
                toPlayer.Normalize();
                toPlayer *= 8f; // 弹幕速度
                // 创建弹幕，你需要先创建对应的Projectile类
                // int proj = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, toPlayer, ModContent.ProjectileType<GuideBossProjectile>(), 20, 0f);
            }

            // 发射弹幕一段时间后切换回追击状态
            if (AttackTimer > 180)
            {
                CurrentAttack = AttackState.Phase1Chase;
                AttackTimer = 0;
                NPC.netUpdate = true;
            }
        }

        // 阶段转换检测：当生命值低于一定比例时进入第二阶段
        public override void FindFrame(int frameHeight)
        {
            // 这里可以控制Boss的动画帧
            // 例如，根据速度或攻击状态切换帧
            NPC.frameCounter++;
            if (NPC.frameCounter >= 10)
            { // 每10帧切换一次动画
                NPC.frameCounter = 0;
                NPC.frame.Y = (NPC.frame.Y + frameHeight) % (Main.npcFrameCount[NPC.type] * frameHeight);
            }
        }

        // 修改Boss的掉落物
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // 添加Boss专属掉落物
            // npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<GuideTrophy>(), 10)); // 纪念章
            // npcLoot.Add(ItemDropRule.OneFromOptions(1, ModContent.ItemType<StormCaller>(), ModContent.ItemType<RainScythe>())); // 从多个武器中随机掉落一个
        }

        // 在Boss存活时，可以阻止暴雨事件意外结束，并设置Boss血条
        public override void OnKill()
        {
            base.OnKill(); 
        }
    }
}