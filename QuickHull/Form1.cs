using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuickHull
{
    public partial class Form1 : Form
    {
        private List<Point> _points;
        private List<Point> hull;
        private Graphics g;

        public Form1()
        {
            InitializeComponent();
            pictureBox1.Image = new Bitmap(700,450);
            g = Graphics.FromImage(pictureBox1.Image);
            _points = new List<Point>();
            hull = new List<Point>();
        }

        //косое произведение двух векторов положительно, если поворот от первого вектора ко второму идет против часовой стрелки,
        //равно нулю, если векторы коллинеарны
        //отрицательно, если поворот идет по часовой стрелки.
        public bool IsUp(Point A, Point B, Point C)
        {
            //косое произведение векторов.
            return (C.X - A.X) * (B.Y - A.Y) - (C.Y - A.Y) * (B.X - A.X) >= 0;//Больше 0, значит лежит в верхней полуплоскости
        }

        //получаем длину стороны треугольника
        public double GetSide(Point A, Point B)//расстояние между точками
        {
            return Math.Sqrt(Math.Pow(B.X - A.X, 2) + Math.Pow(B.Y - A.Y, 2));
        }

        //площадь треугольника
        public double AreaTriangle(Point A, Point B, Point C)
        {
            double a = GetSide(A, B), b = GetSide(B, C), c = GetSide(A, C);
            double p = (a + b + c) / 2;
            return Math.Sqrt(p * (p - a) * (p - b) * (p - c));
        }

        public void QuickHull(List<Point> points, ref List<Point> convexHull)
        {
            points = points.OrderBy(p => p.X).ToList();//сортируем по иксу
            Point left = points.First(), right = points.Last();//ищем самую левую и самую правую вершину
            
            List<Point> pointsUp = new List<Point>();//точки выше прямой
            List<Point> pointsDown = new List<Point>();//точки ниже прямой

            convexHull.Add(left);//добавляем самую левую в выпуклую оболочку

            //делим множество точек на верхнее и нижнее множество относительно прямой
            foreach (var p in points)
            {
                if (p != left && p != right)
                        if (IsUp(left, right, p))
                            pointsUp.Add(p);//идет в верхнюю полуплоскость
                         else
                            pointsDown.Add(p);//в нижнюю
            }

            if (pointsUp.Count > 0)
                convexHull.AddRange(NextPoint(pointsUp, left, right));

            convexHull = convexHull.OrderBy(p => p.X).ToList();//сортируем по возрастанию

            convexHull.Add(right);//добавляем самую правую в выпуклую оболочку

            List<Point> temp;
            if (pointsDown.Count > 0)
            {
                temp = NextPoint(pointsDown, right, left);
                temp = temp.OrderByDescending(p => p.X).ToList();//сортируем по убыванию
                convexHull.AddRange(temp);
            }
        }

        public List<Point> NextPoint(List<Point> points, Point left, Point right)
        {
            List<Point> res = new List<Point>();

            points = points.OrderByDescending(p => AreaTriangle(left, right, p)).ToList();//ищем точку, которая дает треугольник с самой большой площадью
            Point top = points.First();
            res.Add(top);//добавляем ее в оболочку

            //теперь должны рассмотреть точки над левой и правой стороной полученного тругольника
            List<Point> pointsLeft = new List<Point>();
            List<Point> pointsRight = new List<Point>();

            foreach(var p in points)
                if (p != left && p != right && p != top)
                    if (IsUp(left, top, p))
                         pointsLeft.Add(p);
                    else
                    if (IsUp(top, right, p))
                         pointsRight.Add(p);

            if (pointsLeft.Count > 0)
                res.AddRange(NextPoint(pointsLeft, left, top));
            if (pointsRight.Count > 0)
                res.AddRange(NextPoint(pointsRight, top, right));
            return res;
        }

        private void clear_button_Click(object sender, EventArgs e)
        {
            _points = new List<Point>();
            hull = new List<Point>();
            g.Clear(pictureBox1.BackColor);
            pictureBox1.Invalidate();
        }

        private void Build_Click(object sender, EventArgs e)
        {
            hull.Clear();
            if (_points.Count() >= 3)
            {
                g.Clear(pictureBox1.BackColor);
                foreach (var p in _points)
                    g.DrawEllipse(Pens.Black, p.X - 3, p.Y - 3, 6, 6);
                QuickHull(_points, ref hull);
                g.DrawPolygon(Pens.Red, hull.ToArray());
                foreach (var p in hull)
                    g.DrawEllipse(Pens.Blue, p.X - 3, p.Y - 3, 6, 6);
            }
            pictureBox1.Invalidate();
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            _points.Add(new Point(e.X, e.Y));
            g.DrawEllipse(Pens.Black, _points.Last().X - 3, _points.Last().Y - 3, 6, 6);
            pictureBox1.Invalidate();
        }
    }
}
