namespace assignment02
{
    class entranceOfPro
    {
        public enum shapes { rectangle, circle, triangle }
        static void Main(string[] args)
        {
            Console.Write("开始生成随机图形");
            Console.WriteLine();
            Random r = new Random();
            for (int i = 0; i < 10; i++)
            {
                int categ = r.Next(1, 4);
                double val1 = r.NextDouble() * 100;
                double val2 = r.NextDouble() * 100;
                double val3 = r.NextDouble() * 100;
                if (categ == 1)
                {
                    rectangle s = new rectangle(val1, val2);
                    double area = s.computeArea();
                    Console.Write($"生成的图形是矩形，该矩形的长为{val1:F3}，宽为{val2:F3}，面积大小为{area:F3}.");
                }
                else if (categ == 2)
                {
                    circle c = new circle(val1);
                    double area = c.computeArea();
                    Console.Write($"生成的图形是圆，该圆的直径为{val1:F3}，面积大小为{area:F3}.");
                }
                else if (categ == 3)
                {
                    triangle t = new triangle(val1, val2, val3);
                    bool valid = t.valid();
                    if (!valid) Console.Write($"生成的图形是三角形，三变成分别为{val1:F3}，{val2:F3}，{val3:F3},无法组成三角形，跳过该事例。");
                    else
                    {
                        double area = t.computeArea();
                        Console.Write($"生成的图形是三角形，三变成分别为{val1:F3},{val2:F3}，{val3:F3},该三角形的面积是{area:F3}.");
                    }
                }
                Console.WriteLine();
            }
            Console.Write("生成已结束");
        }
    }

    public interface computed
    {
        double computeArea();
    }
    public interface isValid
    {
        bool valid();
    }

    class circle : computed
    {
        double radius;
        public circle(double radius)
        {
            this.radius = radius;
        }
        public double computeArea()
        {
            return radius * radius * Math.PI;
        }
    }
    class rectangle : computed
    {
        double length;
        double width;
        public rectangle(double length, double width)
        {
            this.length = length;
            this.width = width;
        }
        public double computeArea()
        {
            return length * width;
        }
    }
    class triangle : computed, isValid
    {
        double a;
        double b;
        double c;
        public triangle(double a, double b, double c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }
        public double computeArea()
        {
            double p = (a + b + c) / 2;
            return Math.Sqrt(p * (p - a) * (p - b) * (p - c));
        }
        public bool valid()
        {
            if (a + c <= b) return false;
            if (a + b <= c) return false;
            if (b + c <= a) return false;
            return true;
        }
    }
}