using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace Terra1999.Mounts
{
    // 坐骑定义类
    public class Su01veMount : ModMount
    {
        public override void SetStaticDefaults()
        {
            // 基础属性设置
            MountData.buff = ModContent.BuffType<Su01veMountBuff>(); // 关联坐骑Buff
            MountData.heightBoost = 20; // 骑乘时的高度提升
            MountData.fallDamage = 0f; // 跌落伤害（0表示无伤害）
            MountData.runSpeed = 9f; // 地面移动速度
            MountData.dashSpeed = 10f; // 冲刺速度
            MountData.flightTimeMax = 0; // 0表示无限飞行时间

            // 飞行属性（关键：实现永久滞空）
            MountData.usesHover = true; // 允许悬停

            // 视觉相关
            MountData.spawnDust = 15; // 生成时的粒子效果
            MountData.totalFrames = 1; // 总帧数
            
            // 玩家位置偏移
            int[] array = new int[MountData.totalFrames];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = 15; // 统一的Y轴偏移
            }
            MountData.playerYOffsets = array;
            
            MountData.xOffset = 13; // X轴偏移
            MountData.yOffset = -1; // Y轴偏移
            MountData.bodyFrame = 31; // 身体帧
            MountData.acceleration = 0.15f; // 加速度
        }

        // 重写Update方法处理加速逻辑
          public override void UpdateEffects(Player player)
        {
            // 基础速度（41mph）
            float baseSpeed = 9f;
            // 加速速度（61mph）
            float boostSpeed = 13.5f; // 13.5 * 4.5 ≈ 61mph
            
            // 移除重力实现永久飞行
            player.gravity = 0f;
            player.fallStart = (int)(player.position.Y / 16f);

            // 检查玩家是否按下Shift键进行加速
            bool isAccelerating = player.controlDown; // 使用下键作为加速键

            if (isAccelerating)
            {
                // 检查魔力是否充足（每秒消耗10魔力）
                if (player.statMana >= 10)
                {
                    // 消耗魔力（每60帧=1秒消耗10魔力）
                    if (Main.GameUpdateCount % 60 == 0)
                    {
                        player.statMana -= 10;
                        player.manaRegenDelay = 60; // 暂时停止魔力恢复
                    }
                    // 应用加速
                    MountData.runSpeed = boostSpeed;
                }
                else
                {
                    // 魔力不足，恢复基础速度
                    MountData.runSpeed = baseSpeed;
                }
            }
            else
            {
                // 未加速，使用基础速度
                MountData.runSpeed = baseSpeed;
            }
        }
    }

    // 坐骑Buff类（保持骑乘状态）
    public class Su01veMountBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // 保持坐骑状态
            player.mount.SetMount(ModContent.MountType<Su01veMount>(), player);
            player.buffTime[buffIndex] = 10; // 持续刷新Buff时间
        }
    }
}