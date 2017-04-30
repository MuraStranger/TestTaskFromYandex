using FileHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;
using System.Data.Common;

namespace AppForWorkingWithData
{
    class Program
    {
        static void Main(string[] args)
        {
            string filename = "data.tsv";// по умолчанию
            if (args.Length != 0)
            {
                filename = args[0];
            }

            Database db = new Database(filename);
            db.CreateDB();

            db.FirstQuary();// запрос 1
            db.SecondQuaryA();// запрос 2а
            db.SecondQuaryB();// запрос 2b
            db.ThirdQuary();// запрос 3
            
            Console.Read();
        }
    }
}
