using System;
using System.Timers;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using System.Data;

namespace Butson
{
    class Program
    {
        string email = "Butson@gmail.com";
        string pwd = "d_-bS57k";

        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Program pr = new Program();

            string token = await pr.Login(pr.email, pr.pwd);
            dynamic datas = await pr.GetData(token);
            pr.FormatDataTable(datas.data);
            pr.FormatDataTable(datas.data);
            System.Console.WriteLine("");
        }

        void FormatDataTable(dynamic datas)
        {
            DataTable Table = new DataTable();

            Table.Columns.Add("Name", typeof(string));
            Table.Columns.Add("Dust", typeof(float));
            Table.Columns.Add("CO", typeof(float));
            Table.Columns.Add("O2", typeof(float));
            Table.Columns.Add("Pressure", typeof(float));
            Table.Columns.Add("Temp", typeof(float));
            Table.Columns.Add("NOx", typeof(float));
            Table.Columns.Add("SO2", typeof(float));
            Table.Columns.Add("FLOW", typeof(float));

            for (int i = 0; i < 8; i++)
            {
                dynamic dataJson = datas[i].lastLog.measuringLogs;

                string Name = datas[i].name.ToObject<string>();
                float Dust = dataJson.Dust.value.ToObject<float>();
                float Pressure = dataJson.Pressure.value.ToObject<float>();
                float Temp = dataJson.Temp.value.ToObject<float>();
                float FLOW = dataJson.FLOW.value.ToObject<float>();

                if (i < 2)
                {
                    float CO = dataJson.CO.value.ToObject<float>();
                    float O2 = dataJson.O2.value.ToObject<float>();
                    float NOx = dataJson.NOx.value.ToObject<float>();
                    float SO2 = dataJson.SO2.value.ToObject<float>();

                    Table.Rows.Add(Name, Dust, CO, O2, Pressure, Temp, NOx, SO2, FLOW);
                }
                else
                    Table.Rows.Add(Name, Dust, 0, 0, Pressure, Temp, 0, 0, FLOW);

            }

            // foreach (DataRow row in Table.Rows)
            //     Console.WriteLine("Name: {0, 40}  Dust: {1, 5}\t  CO: {2, 5}\t  O2: {3, 5}\t  Pressure: {4, 5}\t  Temp: {5, 5}\t  NOx: {6, 5}\t  SO2: {7, 5}\t  FLOW: {8, 5}\t", row[0], row[1], row[2], row[3], row[4], row[5], row[6], row[7], row[8]);
        }

        async Task<dynamic> GetData(string token)
        {
            string url = "https://global-api.ilotusland.com/station-auto/last-log";

            using HttpClient client = new HttpClient();

            using HttpRequestMessage httpReqMess = new HttpRequestMessage();
            httpReqMess.Method = HttpMethod.Get;
            httpReqMess.RequestUri = new Uri(url);
            httpReqMess.Headers.Add("authorization", token);

            HttpResponseMessage httpResMess = await client.SendAsync(httpReqMess);
            using Stream responseStream = await httpResMess.Content.ReadAsStreamAsync();
            using StreamReader fileRes = new StreamReader(responseStream, Encoding.UTF8);
            string fileResJson = fileRes.ReadToEnd();

            dynamic datas = JsonConvert.DeserializeObject(fileResJson);

            return datas;
        }

        async Task<string> Login(string email, string pwd)
        {
            string url = "https://global-api.ilotusland.com/auth/login";
            List<KeyValuePair<string, string>> parameter = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("email", email),
                    new KeyValuePair<string, string>("password", pwd)
                };

            using HttpClient client = new HttpClient();

            using HttpRequestMessage httpReqMess = new HttpRequestMessage();
            httpReqMess.Method = HttpMethod.Post;
            httpReqMess.RequestUri = new Uri(url);
            httpReqMess.Content = new FormUrlEncodedContent(parameter);

            HttpResponseMessage httpResMess = await client.SendAsync(httpReqMess);
            Stream responseStream = await httpResMess.Content.ReadAsStreamAsync();
            using StreamReader fileRes = new StreamReader(responseStream, Encoding.UTF8);
            string fileResJson = fileRes.ReadToEnd();

            dynamic jsonData = JsonConvert.DeserializeObject(fileResJson);

            return jsonData.token.ToString();
        }
    }
}
