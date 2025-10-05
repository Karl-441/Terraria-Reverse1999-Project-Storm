using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.Personalities;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;
using Terra1999.Events;
using Terra1999.Content.Items;

namespace Terra1999.NPCs
{
    [AutoloadHead]
    public class VertinNPC : ModNPC
    {
        public override void SetStaticDefaults()
        {
            // 动画辅助参数与向导同步（依赖AnimationType自动关联帧数量）
            NPCID.Sets.ExtraFramesCount[Type] = NPCID.Sets.ExtraFramesCount[NPCID.Guide];
            NPCID.Sets.AttackFrameCount[Type] = NPCID.Sets.AttackFrameCount[NPCID.Guide];
            NPCID.Sets.DangerDetectRange[Type] = 700;
            NPCID.Sets.AttackType[Type] = 0;
            NPCID.Sets.AttackTime[Type] = 90;
            NPCID.Sets.AttackAverageChance[Type] = 30;
            NPCID.Sets.HatOffsetY[Type] = 4;
            
            // 标记为城镇NPC
            NPCID.Sets.ActsLikeTownNPC[Type] = true;
            NPCID.Sets.NoTownNPCHappiness[Type] = false;
            
            // 修复警告：使用无参构造函数初始化图鉴绘制参数（替代过时的int参数构造）
            NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                Velocity = 1f // 保持原有的Velocity设置
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
        }

        public override void SetDefaults()
        {
            // 复制向导的所有基础属性（包括纹理、默认帧高度等）
            NPC.CloneDefaults(NPCID.Guide);
            
            // 仅修改必要的自定义属性（不覆盖动画相关参数）
            NPC.townNPC = true;
            NPC.friendly = true;
            NPC.damage = 10;
            NPC.defense = 15;
            NPC.lifeMax = 250;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.5f;
            
            // 关键：指定动画类型为向导，自动复用动画逻辑+帧数量
            AnimationType = NPCID.Guide;
            
            // 修复错误：用Main.npcFrameHeight获取向导帧高度（替代不存在的NPCID.Sets.FrameHeight）
            //NPC.frame.Height = Main.npcFrameHeight[NPCID.Guide];
            
            // 确保网络同步
            NPC.netAlways = true;
        }
        // 在VertinNPC类中添加网络同步
        public override void OnKill()
        {
            // 在服务器上重新生成NPC
            if (Main.netMode == NetmodeID.Server)
            {
                int newNPC = NPC.NewNPC(null, (int)NPC.position.X, (int)NPC.position.Y, Type);
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, newNPC);
            }
        }

        // 确保NPC在客户端也能正确生成
        public override bool CheckActive()
        {
            return true; // 确保NPC始终保持活跃状态
        }

        public override bool CanTownNPCSpawn(int numTownNPCs)
        {
            // 生成条件逻辑不变
            if (Main.netMode == NetmodeID.Server)
            {
                foreach (Player player in Main.player)
                {
                    if (player.active && !player.dead)
                    {
                        var modPlayer = player.GetModPlayer<Terra1999Player>();
                        if (modPlayer.modUnlocked)
                            return true;
                    }
                }
                return false;
            }
            else
            {
                Player player = Main.LocalPlayer;
                if (player.active && !player.dead)
                {
                    var modPlayer = player.GetModPlayer<Terra1999Player>();
                    return modPlayer.modUnlocked;
                }
                return false;
            }
        }

        public override List<string> SetNPCNameList()
        {
            return new List<string>() { "司辰 维尔汀" };
        }

        public override string GetChat()
        {
            // 对话逻辑不变
            Player player = Main.LocalPlayer;
            Terra1999Player modPlayer = player.GetModPlayer<Terra1999Player>();
            if (!modPlayer.modUnlocked)
            {
                return "此处似乎不需要我。";
            }
            int partyGirl = NPC.FindFirstNPC(NPCID.PartyGirl);
            if (partyGirl >= 0 && Main.rand.NextBool(4))
            {
                return "拉普拉斯研究中心已经证实这些非对称核素R材料可以避免暴雨的侵蚀。";
            }
            switch (Main.rand.Next(4))
            {
                case 0:
                    return "我之前从没来过如此奇特的地方...下次可以叫上其他成员。";
                case 1:
                    return "看起来这里要解决的麻烦也不少。";
                case 2:
                    return "这把平衡伞原本被设计用来避免暴雨侵蚀...但它在这里反倒多了些别的用途？";
                default:
                    return "因为暴雨的出现，我已经失去不少同伴了...希望你不会成为它的下一个牺牲品。";
            }
        }

        public override void SetChatButtons(ref string button, ref string button2)
        {
            button = Language.GetTextValue("LegacyInterface.28"); // "商店"
            button2 = "";
        }

        public override void OnChatButtonClicked(bool firstButton, ref string shop)
        {
            if (firstButton)
            {
                shop = "VertinShop";
            }
        }

        public override void AddShops()
        {
            // 商店逻辑不变
            var npcShop = new NPCShop(Type, "VertinShop")
                .Add<Items.AsymmetricRadionuclideRBlockItem>()
                .Add<Items.AsymmetricRadionuclideRWallItem>()
                .Add<Items.AsymmetricRadionuclideRPlatformItem>()
                .Add<Items.AsymmetricRadionuclideRWorkbenchItem>()
                .Add<Su01veKey>()
                .Add<BalanceUmbrella>()
                .Add<Items.RainstormStarter>();
            npcShop.Register();
        }

        public override void TownNPCAttackStrength(ref int damage, ref float knockback)
        {
            damage = 20;
            knockback = 4f;
        }

        public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
        {
            cooldown = 30;
            randExtraCooldown = 30;
        }

        public override void TownNPCAttackProj(ref int projType, ref int attackDelay)
        {
            projType = ProjectileID.Electrosphere;
            attackDelay = 1;
        }

        public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset)
        {
            multiplier = 12f;
            randomOffset = 2f;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.DayTime,
                new FlavorTextBestiaryInfoElement("来自基金会的司辰，专门研究暴雨现象并提供防护材料。")
            });
        }

        public override bool CheckDead()
        {
            // 死亡逻辑不变
            if (RainstormEvent.RainstormActive)
            {
                NPC.life = NPC.lifeMax;
                return false;
            }
            return base.CheckDead();
        }

        // 禁用自定义PreDraw，完全复用向导的纹理绘制
        /*
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[NPCID.Guide].Value;
            SpriteEffects effects = NPC.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            
            Vector2 drawPos = NPC.Center - screenPos;
            drawPos.Y += NPC.gfxOffY;
            
            spriteBatch.Draw(texture, drawPos, NPC.frame, drawColor, NPC.rotation, NPC.frame.Size() / 2, NPC.scale, effects, 0f);
            
            return false;
        }
        */
    }
}