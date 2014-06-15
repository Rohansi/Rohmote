using System;
using System.Threading.Tasks;
using Rohmote;

namespace RohmoteDemo
{
    class TestClass
    {
        public string Member;
    }

    class Program
    {
        static async Task MainAsync()
        {
            var server = new RpcServer("127.0.0.1", 3000, conn =>
            {
                conn.On("Hello1", (string name) => string.Format("hello {0}", name));

                conn.On("Hello2", async () =>
                {
                    var name = await conn.Call<string>("GetName");
                    return string.Format("hello {0}", name);
                });

                conn.On("Add", (int num1, int num2) => num1 + num2);

                conn.On("Test", (TestClass o) => o.Member.ToLower());
            });

            var client = new RpcClient("127.0.0.1", 3000);
            client.On("GetName", () => "Rohan");

            try
            {
                Console.WriteLine("Hello1: {0}", await client.Call<string, string>("Hello1", "Brian"));

                Console.WriteLine("Hello2: {0}", await client.Call<string>("Hello2"));

                Console.WriteLine("Add: {0}", await client.Call<int, int, int>("Add", 10, 301));

                Console.WriteLine("Test: {0}", await client.Call<TestClass, string>("Test", new TestClass { Member = "HELP" }));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        static void Main()
        {
            MainAsync().Wait();
        }
    }
}
