using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CalculateWorkingPeriod
{
    struct Weekends
    {
        DateTime wStartDate;
        DateTime wEndDate;

        public DateTime WStartDate
        {
            get { return wStartDate; }
            set { wStartDate = value; }
        }

        public DateTime WEndDate
        {
            get { return wEndDate; }
            set { wEndDate = value; }
        }
    }

    class Program
    {
        static DateTime CaclulateDate(DateTime date, int duration, List<Weekends> weekends)
        {
            if (date > DateTime.MaxValue || date < DateTime.MinValue)
                throw new ArgumentOutOfRangeException();

            TimeSpan maxAddingDuration = DateTime.MaxValue - date;
            TimeSpan minAddingDuration = DateTime.MinValue - date;
            if (maxAddingDuration.Days < duration || minAddingDuration.Days > duration )
                throw new ArgumentOutOfRangeException("Impossible duration");          

            DateTime result = date.AddDays(duration - 1);  // We start counting including first day, so we have to subtract 1
            foreach (Weekends ww in weekends)
	        {
                if (result < ww.WStartDate)
                    return result;
                else
                    {
                        // When we subtract two dates, we lose one day, so we have to add 1
                        int allWeekends = (ww.WEndDate - ww.WStartDate).Days + 1; 
                        result = result.AddDays(allWeekends);                        
                    }
	        }
            return result;                     
        }

        static void Main(string[] args)
        {
            //    TEST 1   Start Date (2017, 04, 21)    duration 5 days  Weekends (2017, 04, 23) - (2017, 04, 25)

            List<Weekends> listW = new List<Weekends>();
            Weekends w1 = new Weekends() 
            { WStartDate = new DateTime(2017, 04, 23), WEndDate = new DateTime(2017, 04, 25) };
      
            listW.Add(w1);          

            try
            {
                DateTime startTime = new DateTime(2017, 04, 21);
                int durationDays = 5;
                DateTime workingDate = CaclulateDate(startTime, durationDays, listW);
                Console.WriteLine( "TEST 1 --  {0}", workingDate.ToShortDateString() );
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Console.WriteLine(ex.Message);
            }

            //    TEST 2  Start Date (2016, 12, 20) duration 10 days  Weekends (2016, 12, 28), (2016, 12, 30)-(2016, 12, 31)

            Weekends w2 = new Weekends() { WStartDate = new DateTime(2016, 12, 28), WEndDate = new DateTime(2016, 12, 28) };
            Weekends w3 = new Weekends() { WStartDate = new DateTime(2016, 12, 30), WEndDate = new DateTime(2016, 12, 31) };
            List<Weekends> listW2 = new List<Weekends>();
            listW2.Add(w2);
            listW2.Add(w3);

            try
            {
                DateTime startTime2 = new DateTime(2016, 12, 20);
                int durationDays2 = 10;
            
                DateTime workingDate = CaclulateDate(startTime2, durationDays2, listW2);
                Console.WriteLine("TEST 2 --  {0}", workingDate.ToShortDateString());
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Console.WriteLine(ex.Message);
            }
                       
            Console.ReadKey();
            
        }
    }
}
