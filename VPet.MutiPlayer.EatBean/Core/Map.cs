using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPet.MutiPlayer.EatBean.Core
{
    public class Map
    {
        public readonly int Width;
        public readonly int Height;

        public List<EBObject>[,] Objects { get; set; }
        public Map(int width, int height)
        {
            Width = width;
            Height = height;
            Objects = new List<EBObject>[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Objects[i, j] = new List<EBObject>();
                }
            }
        }

        /// <summary>
        /// 添加物品
        /// </summary>
        public void AddObject(EBObject obj)
        {
            Objects[obj.X, obj.Y].Add(obj);
        }
        /// <summary>
        /// 添加随机位置物品
        /// </summary>
        public bool AddRandomObject(EBObject obj)
        {
            Random random = new Random();
            int x, y;
            int i = Width * Height;
            do
            {
                if (i-- <= 0)
                {
                    return false;
                }
                x = random.Next(Width);
                y = random.Next(Height);
            } while (Objects[x, y].Count != 0);
            obj.X = x;
            obj.Y = y;
            Objects[x, y].Add(obj);
            return true;
        }

        /// <summary>
        /// 移动物体
        /// </summary>
        public bool MoveObject(EBObject obj, int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                return false;
            }
            if (Objects[x, y].Count != 0)
            {
                if (Objects[x, y].Any(o => !o.IsPassable))
                {
                    return false;
                }
            }
            Objects[obj.X, obj.Y].Remove(obj);
            obj.X = x;
            obj.Y = y;
            Objects[x, y].Add(obj);
            if (obj is Player player)
            {
                foreach (var o in Objects[x, y])
                {
                    o.OnTouch(player);
                }
            }
            return true;
        }

        /// <summary>
        /// 计算距离
        /// </summary>
        public double Distance(EBObject A, EBObject B)
        {
            return Math.Sqrt(Math.Pow(A.X - B.X, 2) + Math.Pow(A.Y - B.Y, 2));
        }
    }
}
