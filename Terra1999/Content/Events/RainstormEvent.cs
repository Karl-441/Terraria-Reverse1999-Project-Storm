using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Terra1999.NPCs;
using Terra1999.Content.Systems;
using Terraria.Localization;
using System.IO;
using Terraria.DataStructures;
using Terra1999.Items;
using Terra1999;
using Terraria.ModLoader.IO;

namespace Terra1999.Events
{
    public class RainstormEvent : ModSystem
    {
        public static bool RainstormActive = false;
        public static bool HasEncounteredRainstorm = false;
        public static int RainstormTimer = 0;
        public static int RainstormDuration = 54000; // 15分钟
        public static List<int> EventNPCs = new List<int>();
        public static bool BossSpawned = false;
        public static bool BossDefeated = false;

        public override void Load()
        {
            On_Main.Update += UpdateRainstormEvent;
            On_Main.DrawBackground += DrawRainEffect;
        }

        public override void Unload()
        {
            On_Main.Update -= UpdateRainstormEvent;
            On_Main.DrawBackground -= DrawRainEffect;
        }

        private void UpdateRainstormEvent(On_Main.orig_Update orig, Main main, GameTime gameTime)
        {
            orig(main, gameTime);

            // 只在服务器端检查触发条件
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                CheckNoonTrigger();
            }

            if (RainstormActive)
            {
                RainstormTimer++;

                // 检查Boss是否被击败 - 只在服务器端处理
                if (Main.netMode != NetmodeID.MultiplayerClient && BossSpawned && !NPC.AnyNPCs(ModContent.NPCType<GuidePersonBoss>()))
                {
                    BossDefeated = true;
                    EndRainstorm();
                    return;
                }

                // 30秒后生成Boss - 只在服务器端处理
                if (Main.netMode != NetmodeID.MultiplayerClient && RainstormTimer == 1800 && !BossSpawned)
                {
                    SpawnBoss();
                    BossSpawned = true;
                }

                // 持续生成怪物（每秒3个）- 只在服务器端处理
                if (Main.netMode != NetmodeID.MultiplayerClient && Main.GameUpdateCount % 60 == 0)
                {
                    for (int i = 0; i < 3; i++) // 改为每秒3个
                    {
                        SpawnMonster();
                    }
                }

                // 每秒应用全屏伤害 - 只在服务器端处理
                if (Main.netMode != NetmodeID.MultiplayerClient && Main.GameUpdateCount % 60 == 0)
                {
                    ApplyScreenDamage();
                }

                // 清理已死亡的NPC
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    EventNPCs.RemoveAll(id => !Main.npc[id].active);
                }
            }
        }

        private void CheckNoonTrigger()
        {
            // 检查是否为正午（12:00），添加时间范围检查避免多次触发
            if (Main.dayTime && Main.time >= 26900 && Main.time <= 27100)
            {
                // 检查是否满足触发条件：困难模式、肉山已击败、模组已解锁
                if (Main.hardMode && NPC.downedBoss3) // downedBoss3 表示肉山已被击败
                {
                    // 检查是否有任意玩家解锁了模组
                    bool anyPlayerUnlocked = false;
                    bool anyPlayerCompletedEvent = false;
                    
                    for (int i = 0; i < Main.maxPlayers; i++)
                    {
                        Player player = Main.player[i];
                        if (player.active && !player.dead)
                        {
                            var modPlayer = player.GetModPlayer<Terra1999Player>();
                            if (modPlayer.modUnlocked)
                            {
                                anyPlayerUnlocked = true;
                                if (modPlayer.rainstormEventsCompleted > 0)
                                {
                                    anyPlayerCompletedEvent = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (anyPlayerUnlocked)
                    {
                        // 计算触发概率：如果完成过事件则为1%，否则为33%
                        float chance = anyPlayerCompletedEvent ? 0.01f : 0.33f;
                        
                        if (Main.rand.NextFloat() < chance && !RainstormActive)
                        {
                            StartRainstorm();
                        }
                    }
                }
            }
        }

        public static void StartRainstorm()
        {
            if (!RainstormActive)
            {
                RainstormActive = true;
                HasEncounteredRainstorm = true;
                RainstormTimer = 0;
                BossSpawned = false;
                BossDefeated = false;
                EventNPCs.Clear();

                // 向所有玩家发送消息
                Main.NewText("暴雨将至...天空逐渐变得灰暗。", Color.DarkBlue);

                // 在服务器端生成初始怪物
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int i = 0; i < 6; i++) // 初始生成6个怪物（2秒的生成量）
                    {
                        SpawnMonster();
                    }
                }

                // 激活天空效果
                if (!Main.dedServ && !SkyManager.Instance["Terra1999:Rainstorm"].IsActive())
                {
                    SkyManager.Instance.Activate("Terra1999:Rainstorm");
                }

                // 同步事件状态
                if (Main.netMode == NetmodeID.Server)
                {
                    NetMessage.SendData(MessageID.WorldData);
                }
            }
        }

        public static void EndRainstorm()
        {
            RainstormActive = false;
            RainstormTimer = 0;
            BossSpawned = false;

            // 向所有玩家发送消息
            Main.NewText("雨过天晴，天空逐渐恢复平静。", Color.LightBlue);

            // 停用天空效果
            if (!Main.dedServ && SkyManager.Instance["Terra1999:Rainstorm"].IsActive())
            {
                SkyManager.Instance.Deactivate("Terra1999:Rainstorm");
            }

            // Boss被击败时掉落事件生成器 - 只在服务器端处理
            if (Main.netMode != NetmodeID.MultiplayerClient && BossDefeated)
            {
                // 给所有活跃玩家记录事件完成
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player player = Main.player[i];
                    if (player.active && !player.dead)
                    {
                        var modPlayer = player.GetModPlayer<Terra1999Player>();
                        modPlayer.RecordRainstormCompletion();
                        
                        // 掉落事件生成器
                        int item = Item.NewItem(player.GetSource_GiftOrReward(), 
                            (int)player.Center.X, (int)player.Center.Y, 0, 0, 
                            ModContent.ItemType<RainstormStarter>());
                            
                        if (Main.netMode == NetmodeID.Server)
                        {
                            NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item);
                        }
                    }
                }
            }

            // 同步事件状态
            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.WorldData);
            }
        }

        private static void SpawnMonster()
        {
            int[] monsterTypes = new int[]
            {
                ModContent.NPCType<Musician>(),
                ModContent.NPCType<Knocker>(),
                ModContent.NPCType<Welcomer>(),
                ModContent.NPCType<Thug>(),
                ModContent.NPCType<ErosionFollower>(),
                ModContent.NPCType<Creation>()
            };

            int monsterType = monsterTypes[Main.rand.Next(monsterTypes.Length)];
            SpawnNPCAtScreenTop(monsterType);
        }

        public static void SpawnBoss()
        {
            if (NPC.AnyNPCs(ModContent.NPCType<GuidePersonBoss>()))
                return;

            // 在第一个活跃玩家位置生成
            Player targetPlayer = null;
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                if (Main.player[i].active && !Main.player[i].dead)
                {
                    targetPlayer = Main.player[i];
                    break;
                }
            }

            if (targetPlayer == null) return;

            int spawnX = (int)targetPlayer.Center.X;
            int spawnY = (int)targetPlayer.Center.Y - 600;

            int bossIndex = NPC.NewNPC(null, spawnX, spawnY, ModContent.NPCType<GuidePersonBoss>());
            EventNPCs.Add(bossIndex);

            // 同步Boss生成
            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, bossIndex);
            }
            
            Main.NewText("引导之人已在暴雨的漩涡中现身！", Color.DarkOrange);
        }

        private static void SpawnNPCAtScreenTop(int npcType)
        {
            // 在第一个活跃玩家位置生成
            Player targetPlayer = null;
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                if (Main.player[i].active && !Main.player[i].dead)
                {
                    targetPlayer = Main.player[i];
                    break;
                }
            }

            if (targetPlayer == null) return;
            
            int x = (int)targetPlayer.Center.X + Main.rand.Next(-Main.screenWidth / 2, Main.screenWidth / 2);
            int y = (int)targetPlayer.Center.Y - Main.screenHeight - Main.rand.Next(100, 300);

            int npcIndex = NPC.NewNPC(null, x, y, npcType);
            if (npcIndex >= 0)
            {
                EventNPCs.Add(npcIndex);
                
                // 设置维尔汀为主要目标
                NPC npc = Main.npc[npcIndex];
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC otherNPC = Main.npc[i];
                    if (otherNPC.active && otherNPC.type == ModContent.NPCType<VertinNPC>())
                    {
                        npc.target = i;
                        break;
                    }
                }

                // 同步NPC生成
                if (Main.netMode == NetmodeID.Server)
                {
                    NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npcIndex);
                }
            }
        }

        private void ApplyScreenDamage()
        {
            // 对NPC造成伤害
            for (int h = 0; h < Main.maxNPCs; h++)
            {
                NPC npc = Main.npc[h];
                if (npc.active && !npc.friendly && npc.lifeMax > 5 &&
                    !EventNPCs.Contains(h) && npc.type != ModContent.NPCType<VertinNPC>())
                {
                    NPC.HitInfo hit = new NPC.HitInfo
                    {
                        Damage = 40,
                        Knockback = 0f,
                        HitDirection = 0
                    };
                    npc.StrikeNPC(hit);

                    // 同步伤害
                    if (Main.netMode == NetmodeID.Server)
                    {
                        NetMessage.SendData(MessageID.DamageNPC, -1, -1, null, h, hit.Damage, hit.Knockback, hit.HitDirection);
                    }
                }
            }

            // 对玩家造成伤害
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (player.active && !player.dead)
                {
                    bool isProtected = player.GetModPlayer<Terra1999Player>().IsProtectedFromRainstorm();

                    if (!isProtected)
                    {
                        var deathReason = PlayerDeathReason.ByCustomReason($"{player.name} 被暴雨无情地吞噬了。");
                        player.Hurt(deathReason, 40, 0);

                        // 伤害特效
                        for (int j = 0; j < 8; j++)
                        {
                            Dust dust = Dust.NewDustDirect(player.position, player.width, player.height, 
                                DustID.Electric, 0, 0, 100, Color.DarkBlue);
                            dust.velocity = Vector2.UnitY * -4f;
                            dust.noGravity = true;
                        }
                    }
                }
            }
        }

        private void DrawRainEffect(On_Main.orig_DrawBackground orig, Main self)
        {
            orig(self);

            // 客户端绘制雨滴效果
            if (RainstormActive && Main.netMode != NetmodeID.Server)
            {
                DrawRainParticles();
            }
        }

        private void DrawRainParticles()
        {
            // 创建从屏幕底部向顶部移动的雨滴粒子，减少数量并增加随机性
            for (int i = 0; i < 20; i++) // 从50减少到20个粒子
            {
                Vector2 position = new Vector2(
                    Main.rand.Next(-100, Main.screenWidth + 100), // 水平位置随机化，略微超出屏幕边界
                    Main.rand.Next(Main.screenHeight - 100, Main.screenHeight + 50) // 从屏幕底部及稍下方开始
                );

                Vector2 velocity = new Vector2(
                    Main.rand.NextFloat(-0.5f, 0.5f), // 增加水平随机性
                    Main.rand.NextFloat(-8f, -5f) // 降低垂直速度
                );

                Dust dust = Dust.NewDustPerfect(
                    position + Main.screenPosition,
                    DustID.Rain,
                    velocity,
                    100,
                    Color.LightGray, // 改为淡灰色
                    Main.rand.NextFloat(0.8f, 1.5f) // 稍微减小粒子大小
                );
                dust.noGravity = true;
                dust.fadeIn = 0.5f; // 添加淡入效果
            }
        }

        // 保存和加载事件状态
        public override void SaveWorldData(TagCompound tag)
        {
            tag["RainstormActive"] = RainstormActive;
            tag["HasEncounteredRainstorm"] = HasEncounteredRainstorm;
            tag["RainstormTimer"] = RainstormTimer;
            tag["BossSpawned"] = BossSpawned;
            tag["BossDefeated"] = BossDefeated;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            RainstormActive = tag.GetBool("RainstormActive");
            HasEncounteredRainstorm = tag.GetBool("HasEncounteredRainstorm");
            RainstormTimer = tag.GetInt("RainstormTimer");
            BossSpawned = tag.GetBool("BossSpawned");
            BossDefeated = tag.GetBool("BossDefeated");
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(RainstormActive);
            writer.Write(HasEncounteredRainstorm);
            writer.Write(RainstormTimer);
            writer.Write(BossSpawned);
            writer.Write(BossDefeated);
        }

        public override void NetReceive(BinaryReader reader)
        {
            RainstormActive = reader.ReadBoolean();
            HasEncounteredRainstorm = reader.ReadBoolean();
            RainstormTimer = reader.ReadInt32();
            BossSpawned = reader.ReadBoolean();
            BossDefeated = reader.ReadBoolean();
        }
    }
}