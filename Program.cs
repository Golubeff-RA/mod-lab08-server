using System.Text;

namespace TPProj
{
    class Program
    {
        public static double Fact(double n)
        {
            if (n == 0)
                return 1;
            else
                return n * Fact(n - 1);
        }


        static void Main()
        {
            StreamWriter writer = new StreamWriter("../../../stats.txt", false, Encoding.UTF8);
            writer.WriteLine("lambda  mu  P0  Pn  Q  A  k");
            double[] lambdas = new double[] { 0.5, 1.5, 2.5, 2.7, 2.8, 3, 3.5, 4, 4.5, 5, 5.5, 6};
            foreach(var lambda in lambdas)
            {
                WriteStats(lambda, 0.5, writer);
                Console.WriteLine("Обработано lambda = {0}", lambda);
            }

            writer.Close();
            
        }

        static void WriteStats(double lambda, double mu, StreamWriter w)
        {
            int cnt_req = 100;
            int time_to_work = (int)(250 /  mu);
            int time_request = (int)(250 / lambda);
            Server server = new Server(time_to_work);
            Client client = new Client(server);
            for (int id = 1; id <= cnt_req; id++)
            {
                client.send(id);
                Thread.Sleep(time_request);
            }

            Console.WriteLine("Всего заявок: {0}", server.requestCount);
            Console.WriteLine("Обработано заявок: {0}", server.processedCount);
            Console.WriteLine("Отклонено заявок: {0}", server.rejectedCount);
            w.Write(lambda.ToString() + " " + mu.ToString());
            double ro = lambda / mu;

            double P0 = 0;
            for (int i = 0; i < 5; i++)
            {
                P0 += Math.Pow(ro, i) / Fact(i);
            }
            P0 = 1 / P0;
            double P01 = server.rejectedCount > 0 ? 0 : 1 - (double)(cnt_req * time_to_work) / (double)(5 * cnt_req * time_request);
            w.Write(" " + P01.ToString() + " " + P0.ToString());

            double Pn = (Math.Pow(ro, 5) / Fact(5)) * P0;
            double Pn1 = server.rejectedCount / (double)cnt_req;
            w.Write(" " + Pn1.ToString() + " " + Pn.ToString());

            double Q = 1 - Pn;
            double Q1 = 1 - Pn1;
            w.Write(" " + Q1.ToString() + " " + Q.ToString());

            double A = lambda * Q;
            double A1 = lambda * Q1;
            w.Write(" " + A1.ToString() + " " + A.ToString());

            double k = A / mu;
            double k1 = A1 / mu;
            w.Write(" " + k1.ToString() + " " + k.ToString() + "\n");


        }
    }
    struct PoolRecord
    {
        public Thread thread;
        public bool in_use;
    }
    class Server
    {
        private PoolRecord[] pool;
        private object threadLock = new object();
        private int time_to_sleep_;
        public int requestCount = 0;
        public int processedCount = 0;
        public int rejectedCount = 0;
        public Server(int time_to_sleep)
        {
            pool = new PoolRecord[5];
            time_to_sleep_ = time_to_sleep;
        }
        public void proc(object sender, procEventArgs e)
        {
            lock (threadLock)
            {
                //Console.WriteLine("Заявка с номером: {0}", e.id);
                requestCount++;
                for (int i = 0; i < 5; i++)
                {
                    if (!pool[i].in_use)
                    {
                        pool[i].in_use = true;
                        pool[i].thread = new Thread(new ParameterizedThreadStart(Answer));
                        pool[i].thread.Start(e.id);
                        processedCount++;
                        return;
                    }
                }
                rejectedCount++;
            }
        }
        public void Answer(object arg)
        {
            int id = (int)arg;
            //Console.WriteLine("Обработка заявки: {0}", id);
            Thread.Sleep(time_to_sleep_);
            for (int i = 0; i < 5; i++)
                if (pool[i].thread == Thread.CurrentThread)
                    pool[i].in_use = false;
        }
    }
    class Client
    {
        private Server server;
        public Client(Server server)
        {
            this.server = server;
            this.request += server.proc;
        }
        public void send(int id)
        {
            procEventArgs args = new procEventArgs();
            args.id = id;
            OnProc(args);
        }
        protected virtual void OnProc(procEventArgs e)
        {
            EventHandler<procEventArgs> handler = request;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        public event EventHandler<procEventArgs> request;
    }
    public class procEventArgs : EventArgs
    {
        public int id { get; set; }
    }
}