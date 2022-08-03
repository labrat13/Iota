using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace MyCodeLibrary
{
    public class CImageProcessor
    {
        /// <summary>
        /// RT-Конвертировать картинку в блок данных
        /// </summary>
        /// <param name="img">Конвертируемое изображение</param>
        /// <returns></returns>
        public static byte[] ImageToBytes(Image img)
        {
            //если картинка пустая, вернуть null
            if (img == null) return null;
            //иначе вернуть массив с данными
            MemoryStream ms = new MemoryStream();
            img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            byte[] res = ms.ToArray();//тут возвращается весь буфер (данные плюс остаток страницы памяти), это на 4 кб больше чем размер данных.
            ms.Close();
            return res;
        }

        /// <summary>
        /// RT-Конвертировать блок данных в картинку. Возвращает изображение или null
        /// </summary>
        /// <param name="bar">Конвертируемый блок данных</param>
        /// <returns></returns>
        public static Image BytesToImage(byte[] bytes)
        {
            //если массив пустой, вернуть null
            if (bytes.Length == 0) return null;
            //иначе вернуть восстановленную из байт картинку
            MemoryStream ms = new MemoryStream(bytes, false);
            Image img = Image.FromStream(ms, true, true);
            ms.Close();
            return img;
        }

        /// <summary>
        /// RT-Создать серо-белую (неактивную) иконку из цветной иконки категории
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public static Image CreateGrayscaleImage(Image original)
        {
            //создать серо-белую (неактивную) иконку из цветной иконки категории
            //проверить время исполнения
            //create a blank bitmap the same size as original
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);

            //get a graphics object from the new image
            Graphics g = Graphics.FromImage(newBitmap);

            //create the grayscale ColorMatrix
            ImageAttributes GrayScaleImageAttributes = new ImageAttributes(); ;

            //получить светло-серую иконку
            ColorMatrix colorMatrix = new ColorMatrix(new float[][] 
              {
                 new float[] {.3f, .3f, .3f, 0, 0},    //R*0.3
                 new float[] {.59f, .59f, .59f, 0, 0}, //G*0.59
                 new float[] {.11f, .11f, .11f, 0, 0}, //B*0.11
                 new float[] {0, 0, 0, 1, 0},
                 new float[] {0.3f, 0.3f, 0.3f, 0, 1} //R,G,B + 0.3 для получения светло-серой картинки
              });

            //set the color matrix attribute
            GrayScaleImageAttributes.SetColorMatrix(colorMatrix);

            //draw the original image on the new image
            //using the grayscale color matrix
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
               0, 0, original.Width, original.Height, GraphicsUnit.Pixel, GrayScaleImageAttributes);

            //dispose the Graphics object
            g.Dispose();
            return newBitmap;
        }

        /// <summary>
        /// RT-Получить хеш иконки
        /// </summary>
        /// <param name="icon">Картинка иконки</param>
        /// <returns>строка хеша иконки</returns>
        public static string GetMD5Hash(Image icon)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.
            MD5 md5Hasher = MD5.Create();

            // Convert the input image to a byte array and compute the hash.
            MemoryStream ms = new MemoryStream();
            icon.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            byte[] data = md5Hasher.ComputeHash(ms);

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2", CultureInfo.InvariantCulture));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        /// <summary>
        /// RT-resize image with preserve aspect ratio
        /// </summary>
        /// <param name="image"></param>
        /// <param name="pictureSize"></param>
        /// <returns></returns>
        public static Image ResizeImage(Image image, Size pictureSize)
        {
            if (image == null) return null;
            //resize image with preserve aspect ratio
            int newWidth;
            int newHeight;
            int originalWidth = image.Width;
            int originalHeight = image.Height;
            float percentWidth = (float)pictureSize.Width / (float)originalWidth;
            float percentHeight = (float)pictureSize.Height / (float)originalHeight;
            float percent = percentHeight < percentWidth ? percentHeight : percentWidth;
            newWidth = Convert.ToInt32(originalWidth * percent);
            newHeight = Convert.ToInt32(originalHeight * percent);

            Image newImage = new Bitmap(newWidth, newHeight);
            using (Graphics graphicsHandle = Graphics.FromImage(newImage))
            {
                graphicsHandle.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphicsHandle.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            return newImage;
        }

        /// <summary>
        /// RT-Сделать скриншот экрана
        /// </summary>
        /// <param name="screen">Экран (дисплей)(если их в системе несколько)</param>
        /// <returns></returns>
        /// <example>
        /// Bitmap b = GetScreenShot(Screen.PrimaryScreen);
        /// b.Save(@"c:\my_screenshot.jpg", ImageFormat.Jpeg);
        /// </example>
        public static Bitmap GetScreenShot(Screen screen)
        {
            Bitmap bmp = new Bitmap(screen.Bounds.Width, screen.Bounds.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.CopyFromScreen(0, 0, screen.Bounds.X, screen.Bounds.Y, screen.Bounds.Size, CopyPixelOperation.SourceCopy);
            g.Dispose();

            return bmp;
        }

    }
}
