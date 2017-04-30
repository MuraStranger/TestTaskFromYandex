using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppForWorkingWithData
{
    class Database
    {
        string databaseName = "mydb.db";
        string tableOrder = "orders";
        string tableProduct = "product";
        string connectionString = "Data Source=mydb.db; Foreign Keys=True;";
        string filename;

        public Database(string filename)
        {
            this.filename = filename;
        }

        public void CreateDB()
        {
            Console.WriteLine("--------------------------------------------------");
            SQLiteConnection.CreateFile(databaseName);
            Console.WriteLine(File.Exists(databaseName) ? "База данных " + databaseName + " создана." : "Возникла ошибка при создании базы данных");

            CreateTable();
        }
        public void CreateTable()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                try
                {
                    SQLiteCommand command = new SQLiteCommand("CREATE TABLE IF NOT EXISTS " + tableOrder + " (id INTEGER PRIMARY KEY, dt INTEGER, amount REAL NOT NULL, product_id INTEGER NOT NULL, FOREIGN KEY (product_id) REFERENCES " + tableProduct + "(id));", conn);
                    command.ExecuteNonQuery();
                    command = new SQLiteCommand("CREATE TABLE IF NOT EXISTS " + tableProduct + " (id INTEGER PRIMARY KEY, name TEXT);", conn);
                    command.ExecuteNonQuery();
                }
                catch (SQLiteException e)
                {
                    Console.WriteLine(e.Message);
                }
                conn.Close();
            }
            FillProductTable();
            FillOrderTable();
        }

        public void FillProductTable()
        {
            Dictionary<int, string> products = new Dictionary<int, string>//фиксированный справочник таблицы product
            {
                {1,"A"},{2,"B"},{3,"C"},{4,"D"},{5,"E"},{6,"F"},{7,"G"}
            };
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                try
                {
                    SQLiteCommand cmd = new SQLiteCommand("DELETE FROM 'product';", conn);
                    cmd.ExecuteNonQuery();
                    foreach (var pr in products)
                    {
                        cmd = new SQLiteCommand("INSERT INTO 'product' ('id', 'name') VALUES (" + pr.Key + ", '" + pr.Value + "');", conn);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (SQLiteException e)
                {
                    Console.WriteLine(e.Message);
                }
                conn.Close();
            }
        }

        public void FillOrderTable()
        {
        
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                try
                {
                    FileHelper fh = new FileHelper();
                    var orders = fh.GetOrders(filename);
                    SQLiteCommand cmd = new SQLiteCommand("DELETE FROM 'orders';", conn);
                    cmd.ExecuteNonQuery();
                    foreach (var order in orders)
                    {
                        cmd = new SQLiteCommand("INSERT INTO 'orders' ('id', 'dt', 'amount', 'product_id') VALUES (" + order.Id + ", strftime('%s','" + order.Dt.ToString("yyyy-MM-ddThh:mm:ss") + "'), " + order.Amount.ToString().Replace(",", ".") + ", " + order.ProductId + ");", conn);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (SQLiteException e)
                {
                    Console.WriteLine(e.Message);
                }
                conn.Close();
            }
        }

        //Запрос 1
        public void FirstQuary()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                SQLiteCommand command = new SQLiteCommand(
                   "SELECT strftime('%m %Y', datetime(dt, 'unixepoch')) AS period, product.name AS product_name, product_id, count(product_id), round(sum(amount)) FROM orders INNER JOIN product ON orders.product_id = product.id " +
                   "WHERE strftime('%m', datetime(dt, 'unixepoch')) = strftime('%m', 'now') AND strftime('%Y', datetime(dt, 'unixepoch')) = strftime('%Y', 'now')" +
                   "GROUP BY product_id;"
                   , conn);

                try
                {
                    Console.WriteLine("--------------------------------------------------");
                    Console.WriteLine("Запрос 1:\nВывести количество и сумму заказов по каждому продукту за текущей месяц.\n");
                    Console.WriteLine("Период\t\tПродукт\tКоличество\tСумма");
                    SQLiteDataReader r = command.ExecuteReader();
                    string line = String.Empty;
                    while (r.Read())
                    {
                        line = GetMonthName(r["period"].ToString()) + "\t"
                             + r["product_name"] + "\t"
                             + r["count(product_id)"] + "\t\t"
                             + r["round(sum(amount))"];
                        Console.WriteLine(line);
                    }
                    r.Close();
                }
                catch (SQLiteException e)
                {
                    Console.WriteLine(e.Message);
                }
                conn.Close();
            }
        }

        //запрос 2(а,б)
        public void SecondQuaryA()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
               
                SQLiteCommand command = new SQLiteCommand(
                   "SELECT DISTINCT strftime('%m %Y', datetime(dt, 'unixepoch')) AS period, product.name AS product_name, round(amount) AS r_amount FROM orders INNER JOIN product ON orders.product_id = product.id " +
                   "WHERE strftime('%m', datetime(dt, 'unixepoch')) = strftime('%m', 'now') AND strftime('%Y', datetime(dt, 'unixepoch')) = strftime('%Y', 'now')" +
                   "AND product_id NOT IN (SELECT product_id FROM orders WHERE strftime('%m', datetime(dt, 'unixepoch')) = strftime('%m','now','-1 month') AND strftime('%Y', datetime(dt, 'unixepoch')) = strftime('%Y', 'now'));"
                   , conn);
                try
                {
                    Console.WriteLine("--------------------------------------------------");
                    Console.WriteLine("Запрос 2.a:\nВывести продукты, которые были заказаны в текущем месяце, но которых не было в прошлом.\n");
                    Console.WriteLine("Период\t\tПродукт\tСумма");
                    SQLiteDataReader r = command.ExecuteReader();
                    string line = String.Empty;
                    while (r.Read())
                    {
                        line = GetMonthName(r["period"].ToString()) + "\t"
                             + r["product_name"] + "\t"
                             + r["r_amount"];
                        Console.WriteLine(line);
                    }
                    r.Close();
                }
                catch (SQLiteException e)
                {
                    Console.WriteLine(e.Message);
                }
                conn.Close();
            }
        }
        public void SecondQuaryB()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                SQLiteCommand command = new SQLiteCommand(
                 "SELECT DISTINCT strftime('%m %Y', datetime(dt, 'unixepoch')) AS period, product.name AS product_name, round(amount) AS r_amount FROM orders INNER JOIN product ON orders.product_id = product.id " +
                 "WHERE strftime('%m', datetime(dt, 'unixepoch')) = strftime('%m', 'now','-1 month') AND strftime('%Y', datetime(dt, 'unixepoch')) = strftime('%Y', 'now')" +
                 "AND product_id NOT IN (SELECT product_id FROM orders WHERE strftime('%m', datetime(dt, 'unixepoch')) = strftime('%m','now') AND strftime('%Y', datetime(dt, 'unixepoch')) = strftime('%Y', 'now'));"
                 , conn);

                try
                {
                    Console.WriteLine("--------------------------------------------------");
                    Console.WriteLine("Запрос 2.b:\nВывести продукты, которые были только в прошлом	месяце, но не в текущем.\n");
                    Console.WriteLine("Период\t\tПродукт\tСумма");
                    SQLiteDataReader r = command.ExecuteReader();
                    string line = String.Empty;
                    while (r.Read())
                    {
                        line = GetMonthName(r["period"].ToString()) + "\t"
                             + r["product_name"] + "\t"
                             + r["r_amount"];
                        Console.WriteLine(line);
                    }
                    r.Close();
                }
                catch (SQLiteException e)
                {
                    Console.WriteLine(e.Message);
                }
                conn.Close();
            }
        }
        public string GetMonthName(string r)
        {
            Dictionary<string, string> month = new Dictionary<string, string>//фиксированный справочник таблицы product
            {
                {"01","янв"},{"02","фев"},{"03","мар"},{"04","апр"},{"05","май"},{"06","июн"},{"07","июл"},{"08","авг"},{"09","сен"},{"10","окт"},{"11","ноя"},{"12","дек"}
            };
            string m = r.Substring(0, 2);
            string y = r.Substring(2);
            foreach (var a in month)
            {
                if (m == a.Key)
                    return a.Value + y;
            }
            return "";
        }
        //запрос 3
        public void ThirdQuary()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                SQLiteCommand command = new SQLiteCommand(
                   "SELECT period, name, round(max(summa)) AS max_amount, round((100*max(summa))/(sum(summa))) AS quota FROM " +
                   "(SELECT strftime('%m %Y', datetime(dt, 'unixepoch')) AS period, product.name, product_id, sum(amount) AS summa  FROM orders INNER JOIN product ON orders.product_id = product.id " +
                   "GROUP BY product.name, period) "+
                   "GROUP BY period;"
                   , conn);

                try
                {
                    Console.WriteLine("--------------------------------------------------");
                    Console.WriteLine("Запрос 3:\nПомесячно вывести продукт, по которому была максимальная сумма заказов за этот период, сумму по этому продукту и его долю от общего объема за этот период.\n");
                    Console.WriteLine("Период\t\tПродукт\tСумма\tДоля");
                    SQLiteDataReader r = command.ExecuteReader();
                    string line = String.Empty;
                    while (r.Read())
                    {
                        line = GetMonthName(r["period"].ToString()) + "\t"
                             + r["name"] + "\t"
                             + r["max_amount"] + "\t"
                             + r["quota"];
                        Console.WriteLine(line);
                    }
                    r.Close();
                    //SQLiteCommand cmd = new SQLiteCommand("SELECT strftime('%m','now','-1 month');", conn);
                    //Console.WriteLine(cmd.ExecuteScalar());
                }
                catch (SQLiteException e)
                {
                    Console.WriteLine(e.Message);
                }
                conn.Close();
            }
        }
    }
}
