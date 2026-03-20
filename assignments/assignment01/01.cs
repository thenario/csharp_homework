namespace theFirstHomework
{
    class entraceOfProg
    {
        static void Main(string[] args)
        {
            helloWorld hw = new helloWorld();
            hw.print("朱玉坤");
            Console.WriteLine();
            primeNumber np = new primeNumber();
            List<int> res = np.getPrimeNum();
            if(res is []) Console.Write("无质数");
            else
            {
                for(int i=0;i<res.Count;i++)
                {
                    if ((i + 1) % 10 == 0)
                    {
                        Console.Write($"第{i+1}个质数:{res[i]}\r");
                    }
                    else
                    {
                        Console.Write($"第{i+1}个质数:{res[i]}  ");
                    }
                }
            }
        }
    }

    class primeNumber
    {
        public List<int> getPrimeNum()
        {   
            Console.Write("请输入上下限（先上后下）");
            List<int> res = [];
            string uL = "";
            string lL = "";
            uL = Console.ReadLine() ?? "";
            lL = Console.ReadLine() ?? "";
            while (string.IsNullOrWhiteSpace(uL) || string.IsNullOrWhiteSpace(lL) || int.Parse(uL)<int.Parse(lL))
            {
                Console.Write("请输入正确数字");
                uL = Console.ReadLine() ?? "";
                lL = Console.ReadLine() ?? "";
            }
            int up = int.Parse(uL);
            int lw = int.Parse(lL);
            for(int i = lw; i < up; i++)
            {
                if(isPrime(i)) res.Add(i);
            }
            return res;
        }
        public static bool isPrime(int number)
        {
            if (number <= 1) return false;
            if (number == 2) return true;
            if (number % 2 == 0) return false;

            int boundary = (int)Math.Sqrt(number);
            for (int i = 3; i <= boundary; i += 2)
            {
                if (number % i == 0) return false;
            }
            return true;
        }
    }
    class helloWorld
    {
        public void print(string s)
        {
            Console.Write($"你好！我是{s}");
        }
    }
}