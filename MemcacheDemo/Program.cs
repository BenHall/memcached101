﻿using System;
using System.Collections;
using System.Threading;
using Memcached.ClientLibrary;

namespace MemcacheDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            MemcachedClient mc = new MemcachedClient(); //Needs log4net
            SetupMemcache(mc);

            Console.WriteLine("Adding item");
            mc.Set("1", DateTime.Now, DateTime.Now.AddSeconds(10));

            Console.WriteLine("Starting");

            Thread write = new Thread(delegate()
                                      {
                                          WriteData(mc);
                                      }
                                 );

            write.Start();

            Thread readA = new Thread(delegate()
            {
                ReadData(mc);
            });
            readA.Start();

            Thread readB = new Thread(delegate()
            {
                ReadData(mc);
            });
            readB.Start();

            Thread readC = new Thread(delegate()
            {
                ReadData(mc);
            });
            readC.Start();

            Console.WriteLine("Bored now...");
            Console.ReadLine();
        }

        private static void WriteData(MemcachedClient mc)
        {
            for (int i = 0; i < 60; i++)
            {
                Thread.Sleep(1000);
                if (mc.Get("1") == null)
                    mc.Set("1", DateTime.Now, DateTime.Now.AddSeconds(10));
            }
        }

        private static void ReadData(MemcachedClient mc)
        {
            for (int i = 0; i < 60; i++)
            {
                Thread.Sleep(1000);
                Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " Contents of cache: " + mc.Get("1") + " @ " +
                                  DateTime.Now);
            }
        }

        private static void SetupMemcache(MemcachedClient mc)
        {
            mc.EnableCompression = true;
            SockIOPool pool = SockIOPool.GetInstance();
            pool.SetServers(new[] {"127.0.0.1:11211"}); //Needs to be an IP
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
    }
}