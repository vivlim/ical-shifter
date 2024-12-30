using Ical.Net;
using Ical.Net.Serialization;
using System.Text;

namespace IcalShifter
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = System.Net.DecompressionMethods.All,
            };

            HttpClient client = new HttpClient(handler);
            using HttpResponseMessage response = await client.GetAsync("https://p24-calendars.icloud.com/holiday/US_en.ics");
            response.EnsureSuccessStatusCode();
            var contentBytes = await response.Content.ReadAsByteArrayAsync();
            var content = Encoding.UTF8.GetString(contentBytes);

            var iCal = SimpleDeserializer.Default.Deserialize(new StringReader(content)).Cast<Calendar>().Single();

            foreach (var evt in iCal.Events)
            {
                evt.DtStart = evt.DtStart.AddMonths(-1);
                if (evt.DtEnd != null)
                {
                    evt.DtEnd = evt.DtEnd.AddMonths(-1);
                }

                evt.Summary = $"1 month to {evt.Summary}";

                evt.Uid = $"SHIFTED-1MonthBefore-{evt.Uid}";
            }

            var serializer = new CalendarSerializer();
            using var outfile = File.OpenWrite("us-holidays-shifted.ics");
            serializer.Serialize(iCal, outfile, Encoding.UTF8);
        }
    }
}
