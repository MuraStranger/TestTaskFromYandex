using FileHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppForWorkingWithData
{
    [DelimitedRecord("\t")]
    [IgnoreFirst(1)]
    class Order
    {
        public int Id;
        [FieldConverter(ConverterKind.Date, "yyyy-MM-ddTHH:mm:ss")]
        public DateTime Dt;
        public double Amount; //сумма заказа
        public int ProductId;
    }
}
