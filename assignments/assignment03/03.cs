namespace assignment03
{
    class entranceOfPro
    {
        static void Main(string[] args)
        {
            clock myclock = new clock(18);
            myclock.onclockHandler += updatetime;
            myclock.onalarmHandler += alarm;
            myclock.start();
        }
        static void updatetime()
        {
            Console.WriteLine("一秒过去，现在：" + DateTime.Now.ToString("HH:mm:ss"));
        }

        static void alarm()
        {
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine("oiiii，时间到了");
            }
        }
    }
    public delegate void clockHandler();
    public class clock
    {
        private int _targetTime;

        public clock(int _targetTime)
        {
            this._targetTime = _targetTime;
        }
        public event clockHandler? onclockHandler;
        public event clockHandler? onalarmHandler;
        public void start()
        {
            Console.WriteLine("时间开始流动");
            while (true)
            {
                Thread.Sleep(1000);
                onclockHandler?.Invoke();
                if (DateTime.Now.Hour == _targetTime)
                {
                    onalarmHandler?.Invoke();
                }
            }
        }
    }
}