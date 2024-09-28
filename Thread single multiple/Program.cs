using Bogus;
using System.Text.Json;
using System.Threading;

namespace Thread_single_multiple
{
    internal class Program
    {
        public static void GenerateUsers(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Faker<User> faker = new();

                var users = faker.RuleFor(u => u.Name, f => f.Person.FirstName)
                    .RuleFor(u => u.Surname, f => f.Person.LastName)
                    .RuleFor(u => u.Email, f => f.Internet.Email())
                    .RuleFor(u => u.DateOfBirth, f => f.Person.DateOfBirth)
                    .Generate(50);

                var json = JsonSerializer.Serialize(users);
                File.WriteAllText($"users{i + 1}.json", json);
            }
        }

        public static List<User> DeserializeUsers(string fileName)
        {
            var readFile = File.ReadAllText(fileName);
            var users = JsonSerializer.Deserialize<List<User>>(readFile);
            return users;
        }

        public static void MultiThreads(int count, List<User> listOfUser)
        {
            Console.WriteLine("Basladi");
         
            int remainingThreads = count;
            object lockObj = new object(); 
            for (int i = 0; i < count; i++)
            {
                int fileIndex = i + 1; 

                ThreadPool.QueueUserWorkItem(_ =>
                {
                    string fileName = $"users{fileIndex}.json";
                  
                    var users = DeserializeUsers(fileName);

                    lock (listOfUser)
                    {
                        listOfUser.AddRange(users);
                    }
                    lock (lockObj)
                    {
                        remainingThreads--;
                        if (remainingThreads == 0)
                        {
                            Monitor.Pulse(lockObj);
                        }
                    }
                });
            }

            lock (lockObj)
            {
                while (remainingThreads > 0)
                {
                    Monitor.Wait(lockObj); 
                }
            }

            Console.WriteLine("Bitti");
        }

        public static void SingleThread(int count, List<User> listOfUser)
        {
            Console.WriteLine("Basladi");

            for (int i = 0; i < count; i++)
            {
                string fileName = $"users{i + 1}.json";
                var users = DeserializeUsers(fileName);
                listOfUser.AddRange(users);
            }

            Console.WriteLine("Bitti");
        }

        static void Main(string[] args)
        {
            int count = 5;  
            GenerateUsers(count);  

            var listOfUser = new List<User>();  
            bool inMenu = true;

            while (inMenu)
            {
                Console.WriteLine("1) Single Thread");
                Console.WriteLine("2) Multiple Thread");
                Console.WriteLine("3) Exit");
                Console.Write("Secim edin: ");
                var secimThread = Console.ReadLine();

                switch (secimThread)
                {
                    case "1":
                        SingleThread(count, listOfUser);
                        break;

                    case "2":
                        MultiThreads(count, listOfUser);
                        break;

                    case "3":
                        inMenu = false;
                        break;

                    default:
                        Console.WriteLine("Wrong input!!!");
                        break;
                }
            }
        }
    } 
}
