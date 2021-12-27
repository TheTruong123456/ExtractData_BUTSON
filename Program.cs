using System;
using System.Timers;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Timers;

namespace Butson
{
    class Program
    {
        string email = "Butson@gmail.com";
        string pwd = "d_-bS57k";
        SqlConnection Connect = new SqlConnection("Data Source=S03,1433;Initial Catalog=ArchiveDB;User ID=xhquser;Password=xhquser");

        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Program pr = new Program();

            DataTable Table = pr.CreateDataTable();
            string token = await pr.Login(pr.email, pr.pwd);
            dynamic datas = await pr.GetData(token);

            pr.FormatDataTable(datas.data, Table);

            pr.SaveToSQL(Table, pr.Connect);

            // foreach (DataRow row in Table.Rows)
            //     Console.WriteLine("ID: {0, 1}  Station: {1, 7} Datetime: {2, 10}  Dust: {3, 4}\t  CO: {4, 3}\t  NOx: {5, 3}\t  O2: {6, 3}\t  SO2: {7, 3}\t  Pressure: {8, 4}\t  Temp: {9, 4}\t  Flow: {10, 4}\t", row[0], row[1], row[2], row[3], row[4], row[5], row[6], row[7], row[8], row[9], row[10]);


        }

        void SaveToSQL(DataTable Table, SqlConnection Connect)
        {            
            Connect.Open();
            SqlCommand command_DeleteData = new SqlCommand(" delete FROM [ArchiveDB].[dbo].[ElectricalMarket_Raw]", Connect);
            command_DeleteData.ExecuteNonQuery();

            foreach (DataRow row in Table.Rows)
            {
                SqlCommand command_InsertData = new SqlCommand("insert into ElectricalMarket_Raw values('" + row["Date"] + "', '" + row["Category"] + "', '" + row["Values"] + "' )", Connect);
                command_InsertData.ExecuteNonQuery();
            }    

            Connect.Close();

        }
        void FormatDataTable(dynamic datas, DataTable Table)
        {
            Table.Clear();

            for (int i = 0; i < 8; i++)
            {
                dynamic dataJson = datas[i].lastLog.measuringLogs;

                int ID = i;
                string Station = "Tram " + ID++.ToString();
                DateTime Datetime = DateTime.Now;
                float Dust = dataJson.Dust.value.ToObject<float>();
                float Pressure = dataJson.Pressure.value.ToObject<float>();
                float Temp = dataJson.Temp.value.ToObject<float>();
                float Flow = dataJson.FLOW.value.ToObject<float>();

                if (i < 2)
                {
                    float CO = dataJson.CO.value.ToObject<float>();
                    float O2 = dataJson.O2.value.ToObject<float>();
                    float NOx = dataJson.NOx.value.ToObject<float>();
                    float SO2 = dataJson.SO2.value.ToObject<float>();

                    Table.Rows.Add(ID, Station, Datetime, Dust, CO, NOx, O2, SO2, Pressure, Temp, Flow);
                }
                else
                    Table.Rows.Add(ID, Station, Datetime, Dust, 0, 0, 0, 0, Pressure, Temp, Flow);

            }
        }
        DataTable CreateDataTable()
        {
            DataTable Table = new DataTable();

            Table.Columns.Add("ID", typeof(int));
            Table.Columns.Add("Station", typeof(string));
            Table.Columns.Add("DateTime", typeof(DateTime));

            Table.Columns.Add("Dust", typeof(float));
            Table.Columns.Add("CO", typeof(float));
            Table.Columns.Add("NOx", typeof(float));
            Table.Columns.Add("O2", typeof(float));
            Table.Columns.Add("SO2", typeof(float));
            Table.Columns.Add("Pressure", typeof(float));
            Table.Columns.Add("Temp", typeof(float));
            Table.Columns.Add("Flow", typeof(float));

            return Table;
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
