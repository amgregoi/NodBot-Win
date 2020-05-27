using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodBot.Code.Model
{
    public class UIPoint
    {
        public static UIPoint Default()
        {
            return new UIPoint(new Point(), new Rectangle());
        }

        private Point _point;
        private Rectangle _rect;


        public UIPoint(Point point, Rectangle rect)
        {
            _point = point;
            _rect = rect;
        }

        public UIPoint(Point point)
        {
            _point = point;
            _rect = new Rectangle();
        }

        public UIPoint(int x, int y) : this(new Point(x,y))
        {

        }

        public int X { get => _point.X; set => _point.X = value; }
        public int Y { get => _point.Y; set => _point.Y = value; }

        public Rectangle Rect { get => _rect; set => _rect = value; }
    }
}
