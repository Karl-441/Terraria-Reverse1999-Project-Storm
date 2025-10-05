using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Terra1999.Skies
{
    public class RainstormSky : CustomSky
    {
        private bool isActive;
        private float intensity;
        private int rainTimer;
        private Texture2D pixelTexture;

        public override void Update(GameTime gameTime)
        {
            if (isActive && intensity < 1f)
            {
                intensity += 0.01f; // 逐渐变灰
                intensity = Math.Min(intensity, 1f);
            }
            else if (!isActive && intensity > 0f)
            {
                intensity -= 0.005f; // 逐渐恢复
                if (intensity <= 0f)
                {
                    intensity = 0f;
                }
            }

            rainTimer++;
            if (rainTimer > 3600)
                rainTimer = 0;
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (maxDepth >= 0 && minDepth < 0)
            {
                if (pixelTexture == null)
                {
                    CreatePixelTexture();
                }
                
                // 灰色覆盖层 - 随着强度增加逐渐变灰
                Color color = Color.Gray * intensity * 0.6f;
                spriteBatch.Draw(pixelTexture, 
                               new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), 
                               color);
            }
        }

        private void CreatePixelTexture()
        {
            pixelTexture = new Texture2D(Main.graphics.GraphicsDevice, 1, 1);
            Color[] data = new Color[1] { Color.White };
            pixelTexture.SetData(data);
        }

        public override float GetCloudAlpha()
        {
            return 1f - intensity * 0.7f; // 云层也变暗
        }

        public override void Activate(Vector2 position, params object[] args)
        {
            isActive = true;
            intensity = 0.01f;
        }

        public override void Deactivate(params object[] args)
        {
            isActive = false;
        }

        public override void Reset()
        {
            isActive = false;
            intensity = 0f;
            rainTimer = 0;
            
            if (pixelTexture != null && !pixelTexture.IsDisposed)
            {
                pixelTexture.Dispose();
                pixelTexture = null;
            }
        }

        public override bool IsActive()
        {
            return isActive || intensity > 0f;
        }

        public override Color OnTileColor(Color inColor)
        {
            // 修改图格颜色，使其偏灰色
            Vector4 colorVector = inColor.ToVector4();
            colorVector = Vector4.Lerp(colorVector, new Vector4(0.3f, 0.3f, 0.3f, 1f), intensity * 0.8f);
            return new Color(colorVector);
        }
    }
}