
using FileHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppForWorkingWithData
{
    class FileHelper
    {
        public Order[] GetOrders(string filename)
        {
            var engine = new DelimitedFileEngine<Order>();

            // Switch error mode on
            engine.ErrorManager.ErrorMode = ErrorMode.SaveAndContinue;

            //  This fails with not in enumeration error
            var orders = engine.ReadFile(filename);
            
            if (engine.ErrorManager.HasErrors)
            {
                engine.ErrorManager.SaveErrors("errors.out");
                LoadErrors(filename);
            }
 
           // Console.WriteLine("=====================================================");

            //foreach (var order in orders)
               // Console.WriteLine("Order id {0} dt {1} amount {2} productId {3}", order.Id, order.Dt.ToString("yyyy-MM-ddThh:mm:ss"), order.Amount, order.ProductId);

            return orders;
        }
        public void LoadErrors(string filename)
        {
            // sometime later you can read it back using:
            ErrorInfo[] errors = ErrorManager.LoadErrors("errors.out");

            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine("В файле " + filename + " обнаружены следующие ошибки:");
            // This will display error from line 2 of the file.
            foreach (var err in errors)
            {
                Console.WriteLine();
                Console.WriteLine("Error on Line number: {0}", err.LineNumber);
                Console.WriteLine("Record causing the problem: {0}", err.RecordString);
                Console.WriteLine("Complete exception information: {0}", err.ExceptionInfo.ToString());
            }
        }
    }
}
