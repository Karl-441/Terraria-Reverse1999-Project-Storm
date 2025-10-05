using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria;
using System.IO;
using Terraria.DataStructures;
using Terra1999.Content.Systems;

namespace Terra1999
{
    public class Terra1999 : Mod
    {
        public override void Load()
        {
            // 注册天空特效（只在客户端）
            if (!Main.dedServ)
            {
                try
                {
                    // 确保 RainstormSky 类存在
                    SkyManager.Instance["Terra1999:Rainstorm"] = new Skies.RainstormSky();
                }
                catch (Exception ex)
                {
                    Logger.Warn($"Failed to register sky effect: {ex.Message}");
                }
            }
        }

        public override void Unload()
        {
            // 防御性卸载代码
            if (!Main.dedServ)
            {
                try
                {
                    // 先检查 SkyManager 实例是否存在
                    if (SkyManager.Instance != null)
                    {
                        // 再检查特定的天空效果是否存在
                        var skyKey = "Terra1999:Rainstorm";
                        if (SkyManager.Instance[skyKey] != null)
                        {
                            SkyManager.Instance[skyKey] = null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 静默处理异常，避免影响其他模组卸载
                    Logger.Warn($"Safe unload failed: {ex.Message}");
                }
            }
        }
    }
}