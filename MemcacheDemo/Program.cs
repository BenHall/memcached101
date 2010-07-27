using System;
using System.Collections;
using System.Text;
using System.Threading;
using Memcached.ClientLibrary;

namespace MemcacheDemo
{
	class Program
	{
		private static int _loopFor = 5;

		static void Main(string[] args)
		{
			MemcachedClient mc = new MemcachedClient(); //Needs log4net
			SetupMemcache(mc);

			for (int i = 0; i < 5; i++) {

				Console.WriteLine("Adding item: " + i);
				mc.Set("1", DateTime.Now, DateTime.Now.AddSeconds(_loopFor*2));

				Console.WriteLine("Starting");

				//Thread write = new Thread(delegate()
				//                          {
				//                              WriteData(mc);
				//                          }
				//                     );

				//write.Start();

				Thread readA = new Thread(delegate(){ ReadData(mc); });
				readA.Start();

				Thread readB = new Thread(delegate(){ ReadData(mc); });
				readB.Start();

				Thread readC = new Thread(delegate(){ ReadData(mc); });
				readC.Start();


				Console.WriteLine(Status(mc));


				readA.Join();
				readB.Join();
				readC.Join();

				Console.WriteLine();
				Console.WriteLine("Bored now...");
				Console.WriteLine(Status(mc));
			}
			Console.ReadLine();
		}

		private static void WriteData(MemcachedClient mc)
		{
			for (int i = 0; i < _loopFor; i++)
			{
				Thread.Sleep(1000);
				if (mc.Get("1") == null)
					mc.Set("1", DateTime.Now, DateTime.Now.AddSeconds(10));
			}
		}

		private static void ReadData(MemcachedClient mc)
		{
			for (int i = 0; i < _loopFor; i++)
			{
				Thread.Sleep(1000);
				Console.WriteLine("Thread: " + Thread.CurrentThread.ManagedThreadId + " Contents of cache: " +
									mc.Get("1") + " @ " + DateTime.Now);
			}
		}

		private static void SetupMemcache(MemcachedClient mc)
		{
			mc.EnableCompression = true;
			SockIOPool pool = SockIOPool.GetInstance();
			pool.SetServers(new[] { "127.0.0.1:11211" }); //Needs to be an IP
			pool.InitConnections = 1;
			pool.MinConnections = 1;
			pool.MaxConnections = 3;

			pool.SocketConnectTimeout = 1000;
			pool.SocketTimeout = 3000;
			pool.MaintenanceSleep = 30;
			pool.Failover = true;

			pool.Nagle = false;
			pool.Initialize();
		}

		public static string Status(MemcachedClient mc)
		{
			var sb = new StringBuilder("Cachestatus:" + Environment.NewLine);
			IDictionary stats = mc.Stats();
			foreach (string key1 in stats.Keys)
			{
				sb.AppendLine(key1);
				var values = (Hashtable)stats[key1];
				foreach (string key2 in values.Keys)
				{
					sb.AppendLine(key2 + ":" + values[key2]);
				}
				sb.AppendLine();
			}
			return sb.ToString();
		}
	}
}
