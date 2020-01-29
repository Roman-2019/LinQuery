using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqApp
{
    class Program
    {
        static void Main(string[] args)
        {
            int currency = 2;
            var banks = GenerateBanks();
            var users = GenerateUsers(banks);

            //1) Сделать выборку всех Пользователей, имя + фамилия которых длиннее чем 12 символов.
            var usersAll12 = users.Where(user => (user.FirstName.Length + user.LastName.Length) > 12);
            /******************/
            var usersAll = from user in users
                           where (user.FirstName.Length + user.LastName.Length) > 12
                           select user;
            var cheker = usersAll12.SequenceEqual(usersAll);
            //2) Сделать выборку всех транзакций (в результате должен получится список из 1000 транзакций)
            var transactionAll = from allTrans in banks
                                 select allTrans.Transactions.ToList();
            /********************/
            var transactionAll1 = users.Where(u => (u.Transactions?.Any()).GetValueOrDefault()).
                SelectMany(u => u.Transactions).ToList();
            
            //3) Вывести Банк: и всех его пользователей (Имя + фамилия + количество транзакий в гривне) отсортированных по Фамилии по убиванию. в таком виде :
            //   Имя банка 
            //   ***************
            //   Игорь Сердюк 
            //   Николай Басков
            var userOrderedByBank = users.GroupBy(p => p.Bank.Name)
                        .Select(g => new
                        {
                            Name = g.Key,
                            Users = g.OrderByDescending(t => t.LastName).Select(u => new {
                                User = u,
                                Transactions = u.Transactions.Count(t => t.Currency == Currency.UAH)
                            })
                        });
            /**************************/
            var ordered = from user in users
                          where currency.Equals(Currency.UAH) //&& users.OrderByDescending(user.FirstName)
                          orderby user.Bank.Name ascending, user.LastName descending
                          select new
                          {
                              name = user.Bank.Name,
                              name1 = user.FirstName,
                              name2 = user.LastName,
                              name3 = user.Transactions.Count()
                          };
            //foreach (var ord in ordered)
            //    Console.WriteLine(ord.name);// $"{ord.name}\n***********\n{ord.name1} {ord.name2} {ord.name3}");
            //Console.ReadKey();
            //4) Сделать выборку всех Пользователей типа Admin, у которых счет в банке, в котором больше всего транзакций
            var bankWithMaxCount = banks.FirstOrDefault(t => t.Transactions.Count == banks.Max(tu => tu.Transactions.Count));
            var usersResult = users.Where(u => u.Type == UserType.Admin && u.Bank == bankWithMaxCount);
            /*******************************/
            var maxUser = from user in users
                          where user.Type == UserType.Admin //&&
                          orderby user.Bank.Transactions.Count descending
                          select user.Bank.Transactions.FirstOrDefault();
            /////
            //var cheker = usersResult.SequenceEqual(maxUser);
            //5) Найти Пользователей(НЕ АДМИНОВ), которые произвели больше всего транзакций в определенной из валют (UAH,USD,EUR) 
            //то есть найти трёх пользователей: 1й который произвел больше всего транзакций в гривне, второй пользователь, который произвел больше всего транзакций в USD 
            //и третьего в EUR
            var usersTransactionsGroupByCurrency = users.Where(u => u.Type != UserType.Admin).Select(t => new
            {
                User = t,
                Transactions = t.Transactions.GroupBy(tr => tr.Currency).Select(res => new
                {
                    TransactionCurrency = res.Key,
                    TransactionCount = res.Count()
                })
            }).Select(r => new
            {
                r.User,
                TransactionGrouping = r.Transactions.GroupBy(s => s.TransactionCurrency).Select(res => new
                {
                    Currency = res.Key,
                    Transactions = res.Max(ra => ra.TransactionCount)
                }).FirstOrDefault()
            });

            var USDMaxResult =
                usersTransactionsGroupByCurrency
                .Where(t => t.TransactionGrouping.Currency == Currency.USD)
                .Max(r => r.TransactionGrouping.Transactions);

            var UAHMaxResult =
                usersTransactionsGroupByCurrency
                .Where(t => t.TransactionGrouping.Currency == Currency.UAH)
                .Max(r => r.TransactionGrouping.Transactions);

            var EURMaxResult =
                usersTransactionsGroupByCurrency
                .Where(t => t.TransactionGrouping.Currency == Currency.EUR)
                .Max(r => r.TransactionGrouping.Transactions);

            var MaxUSDTRansactionUser =
                usersTransactionsGroupByCurrency
                    .Where(u => u.TransactionGrouping.Transactions == USDMaxResult && u.TransactionGrouping.Currency == Currency.USD);

            var MaxUAHTransactionUser =
                usersTransactionsGroupByCurrency
                    .Where(u => u.TransactionGrouping.Transactions == UAHMaxResult && u.TransactionGrouping.Currency == Currency.UAH);

            var MaxEURTRansactionUser =
                usersTransactionsGroupByCurrency
                    .Where(u => u.TransactionGrouping.Transactions == EURMaxResult && u.TransactionGrouping.Currency == Currency.EUR);
        
        /**********************/
        currency = 1;
            var notAdmin = from user in users
                           where user.Type != UserType.Admin
                           select user;


            var notAdmin1 = from user1 in notAdmin
                            where currency.Equals(Currency.USD)
                            orderby user1.Transactions.Count() descending
                            select user1;
            Console.WriteLine(notAdmin1);
            Console.ReadKey();

            //6) Сделать выборку транзакций банка, у которого больше всего Pemium пользователей
            var bankTransaction = users.GroupBy(b => b.Bank.Name);
            //banks.FirstOrDefault(t => t.Transactions.Count == banks.Max(tu => tu.Transactions.Count));
            var usersPremium = users.Where(u => u.Type == UserType.Pemium && u.Bank == bankTransaction).ToList(); ;
            /***********************/
            var transacBanks = from us in users
                               where us.Type == UserType.Pemium
                               select us.Bank.Transactions.Count;


            /////////////////////////////////////////////////////////////////////
            Console.ReadKey();
        }

        public class User
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public List<Transaction> Transactions { get; set; }
            public UserType Type { get; set; }
            public Bank Bank { get; set; }
        }

        public class Transaction
        {
            public decimal Value { get; set; }

            public Currency Currency { get; set; }
        }

        public static List<Transaction> GetTenTransactions()
        {
            var result = new List<Transaction>();
            var sign = random.Next(0, 1);
            var signValue = sign == 0 ? -1 : 1;
            for (var i = 0; i < 10; i++)
            {
                result.Add(new Transaction
                {
                    Value = (decimal)random.NextDouble() * signValue * 100m,
                    Currency = GetRandomCurrency(),
                });
            }

            return result;
        }

        public class Bank
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public List<Transaction> Transactions { get; set; }
        }

        public enum UserType
        {
            Default = 1,
            Pemium = 2,
            Admin = 3
        }

        public static UserType GetRandomUserType()
        {
            int userTypeInt = random.Next(1, 4);

            return (UserType)userTypeInt;
        }

        public enum Currency
        {
            USD = 1,
            UAH = 2,
            EUR = 3
        }

        public static Currency GetRandomCurrency()
        {
            int userTypeInt = random.Next(1, 4);

            return (Currency)userTypeInt;
        }

        public static List<Bank> GenerateBanks()
        {
            var banksCount = random.Next(BANKS_MIN, BANKS_MAX);
            var result = new List<Bank>();

            for (int i = 0; i < banksCount; i++)
            {
                result.Add(new Bank
                {
                    Id = i + 1,
                    Name = RandomString(random.Next(NAME_MIN_LENGTH, NAME_MAX_LENGTH)),
                    Transactions = new List<Transaction>()
                });
            }

            return result;
        }

        public static List<User> GenerateUsers(List<Bank> banks)
        {
            var result = new List<User>();
            int bankId = 0;
            Bank bank = null;
            List<Transaction> transactions = null;
            for (int i = 0; i < 100; i++)
            {
                bankId = random.Next(0, banks.Count);
                bank = banks[bankId];
                transactions = GetTenTransactions();
                result.Add(new User
                {
                    Bank = bank,
                    FirstName = RandomString(random.Next(NAME_MIN_LENGTH, NAME_MAX_LENGTH)),
                    Id = i + 1,
                    LastName = RandomString(random.Next(NAME_MIN_LENGTH, NAME_MAX_LENGTH)),
                    Type = GetRandomUserType(),
                    Transactions = transactions
                });
                bank.Transactions.AddRange(transactions);
            }

            return result;
        }

        private const int BANKS_MIN = 2;
        private const int BANKS_MAX = 5;

        private const int NAME_MAX_LENGTH = 10;
        private const int NAME_MIN_LENGTH = 4;

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
