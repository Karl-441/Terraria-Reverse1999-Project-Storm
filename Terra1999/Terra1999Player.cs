using Terraria;
using Terraria.ModLoader;
using System.IO;
using Terraria.DataStructures;
using Terra1999.Content.Systems;
using Terraria.ModLoader.IO;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace Terra1999
{
    public class Terra1999Player : ModPlayer
    {
        // 模组解锁状态
        public bool modUnlocked = false;
        // 暴雨事件遭遇记录
        public bool hasEncounteredRainstorm = false;
        // 平衡伞保护状态
        public bool hasUmbrellaProtection = false;
        // 使用过暴雨启动器
        public bool hasUsedRainstormStarter = false;
        // 完成的暴雨事件次数
        public int rainstormEventsCompleted = 0;
        
        public override void OnEnterWorld()
        {
            // 每次进入世界时的初始化逻辑
        }
        
        // 每帧重置效果状态
        public override void ResetEffects()
        {
            // 重置平衡伞保护状态，由 HoldItem 方法在每帧重新设置
            hasUmbrellaProtection = false;
        }

        // 每帧更新逻辑
        public override void PostUpdate()
        {
            // 可以在这里添加每帧都需要执行的逻辑
        }

        // === 数据保存与加载 ===

        // 保存数据到世界文件
        public override void SaveData(TagCompound tag)
        {
            tag["modUnlocked"] = modUnlocked;
            tag["hasEncounteredRainstorm"] = hasEncounteredRainstorm;
            tag["hasUsedRainstormStarter"] = hasUsedRainstormStarter;
            tag["rainstormEventsCompleted"] = rainstormEventsCompleted;
        }

        // 从世界文件加载数据
        public override void LoadData(TagCompound tag)
        {
            modUnlocked = tag.GetBool("modUnlocked");
            hasEncounteredRainstorm = tag.GetBool("hasEncounteredRainstorm");
            hasUsedRainstormStarter = tag.GetBool("hasUsedRainstormStarter");
            rainstormEventsCompleted = tag.GetInt("rainstormEventsCompleted");
        }

        // === 多人游戏同步 ===

        // 复制客户端状态到克隆对象
        public override void CopyClientState(ModPlayer clientClone)
        {
            Terra1999Player clone = clientClone as Terra1999Player;
            clone.modUnlocked = modUnlocked;
            clone.hasEncounteredRainstorm = hasEncounteredRainstorm;
            clone.hasUsedRainstormStarter = hasUsedRainstormStarter;
            clone.rainstormEventsCompleted = rainstormEventsCompleted;
        }

        // 发送客户端变化到服务器
        public override void SendClientChanges(ModPlayer clientPlayer)
        {
            Terra1999Player clone = clientPlayer as Terra1999Player;
            if (modUnlocked != clone.modUnlocked ||
                hasEncounteredRainstorm != clone.hasEncounteredRainstorm ||
                hasUsedRainstormStarter != clone.hasUsedRainstormStarter ||
                rainstormEventsCompleted != clone.rainstormEventsCompleted)
            {
                // 同步数据到服务器
                ModPacket packet = Mod.GetPacket();
                packet.Write((byte)1); // 包类型1：玩家数据同步
                packet.Write((byte)Player.whoAmI);
                packet.Write(modUnlocked);
                packet.Write(hasEncounteredRainstorm);
                packet.Write(hasUsedRainstormStarter);
                packet.Write(rainstormEventsCompleted);
                packet.Send();
            }
        }

        // 玩家复活时的处理
        public override void OnRespawn()
        {
            // 复活时重置临时状态
            hasUmbrellaProtection = false;
        }

        // 玩家死亡时的处理
        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            // 玩家死亡时，重置某些临时状态
            hasUmbrellaProtection = false;
        }

        // === 自定义方法 ===

        /// <summary>
        /// 激活模组功能
        /// </summary>
        public void UnlockModContent()
        {
            modUnlocked = true;
            // 同步给其他玩家
            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.PlayerInfo, -1, -1, null, Player.whoAmI);
            }
        }

        /// <summary>
        /// 记录暴雨事件完成
        /// </summary>
        public void RecordRainstormCompletion()
        {
            hasUsedRainstormStarter = true;
            rainstormEventsCompleted++;
            
            // 可以根据完成次数给予奖励等
            if (rainstormEventsCompleted == 1)
            {
                Main.NewText("首次完成暴雨事件！", Color.Gold);
            }
        }

        /// <summary>
        /// 检查玩家是否受到暴雨保护
        /// </summary>
        public bool IsProtectedFromRainstorm()
        {
            return RainstormProtectionSystem.IsPlayerProtected(Player) || 
                   RainstormProtectionSystem.IsPlayerInProtectedArea(Player) || 
                   hasUmbrellaProtection;
        }

        /// <summary>
        /// 处理接收到的网络数据包
        /// </summary>
        public void HandlePacket(BinaryReader reader)
        {
            byte packetType = reader.ReadByte();
            switch (packetType)
            {
                case 1: // 玩家数据同步
                    byte whoAmI = reader.ReadByte();
                    if (whoAmI == Player.whoAmI)
                    {
                        modUnlocked = reader.ReadBoolean();
                        hasEncounteredRainstorm = reader.ReadBoolean();
                        hasUsedRainstormStarter = reader.ReadBoolean();
                        rainstormEventsCompleted = reader.ReadInt32();
                    }
                    break;
            }
        }
    }
}