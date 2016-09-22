using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinForms2SQLWithAAD
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string ConnectionString =
                @"Data Source=aadprotected.database.windows.net;Authentication=Active Directory Integrated;Initial Catalog=AdventureWorks";
            var conn = new SqlConnection(ConnectionString);
            await DisplayData(System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString(), conn);
            conn.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _display.Text = "";
        }

        AuthenticationContext _authCtx;
        private async void button3_Click(object sender, EventArgs e)
        {
            if (_authCtx == null)
            {
                _authCtx = new AuthenticationContext("https://login.windows.net/meraridom.com");
                _authCtx.TokenCache.Clear();
            }
            var resp = await _authCtx.AcquireTokenAsync(
                "https://database.windows.net/",
                "14b41d8a-ed45-4b35-bccf-3d0810912a89",
                new Uri("https://winforms2sql.com"),
                new PlatformParameters(PromptBehavior.Always));
            if (resp != null)
            {
                var builder = new SqlConnectionStringBuilder();
                builder["Data Source"] = "aadprotected.database.windows.net"; // replace with your server name
                builder["Initial Catalog"] = "AdventureWorks"; // replace with your database name
                builder["Connect Timeout"] = 30;
                var conn = new SqlConnection(builder.ConnectionString);
                conn.AccessToken = resp.AccessToken;
                await DisplayData(resp.UserInfo.GivenName, conn);
            }
        }
        private async Task DisplayData(string user, SqlConnection conn)
        {
            var sb = new StringBuilder(user);

            if (_useEF.Checked)
            {
  //              MetadataWorkspace workspace = new MetadataWorkspace(
  //new string[] { "res://*/" },
  //new Assembly[] { Assembly.GetExecutingAssembly() });

  //              using (var sqlConnection = new SqlConnection(connectionString))
  //              using (var entityConnection = new EntityConnection(workspace, sqlConnection))
  //              using (var context = new Ef6Context(entityConnection))
  //              {
  //              }

                sb.AppendLine("Using EF");
                var model = new AdventureWorksModel(conn, true);
                try
                {
                    foreach (var cust in model.Customers.Take(5))
                    {
                        sb.AppendLine(String.Format("{0} : {1}", cust.CompanyName, cust.LastName));
                    }
                } catch (Exception ex)
                {
                    sb.AppendLine(ex.Message);
                    sb.AppendLine(conn.ConnectionString);
                    sb.AppendLine(conn.AccessToken);
                }
            }
            else
            {
                sb.AppendLine("Using SQL");
                try
                {
                    await conn.OpenAsync();
                    var cmd = new SqlCommand("SELECT TOP 5 [ProductID],[Name] FROM[SalesLT].[Product]", conn);
                    var rdr = await cmd.ExecuteReaderAsync();

                    while (await rdr.ReadAsync())
                    {
                        sb.AppendLine(String.Format("{0} - {1}", rdr.GetInt32(0), rdr.GetString(1)));
                    }
                } catch(Exception ex)
                {
                    sb.AppendLine(ex.Message);
                }
                conn.Close();
            }
            _display.Text = sb.ToString();
        }

        private async void button4_Click(object sender, EventArgs e)
        {
            var builder = new SqlConnectionStringBuilder();
            builder["Data Source"] = "aadprotected.database.windows.net"; // replace with your server name
            builder["Initial Catalog"] = "AdventureWorks"; // replace with your database name
            builder["Connect Timeout"] = 30;
            builder["Authentication"] = "Active Directory Password";
            builder["UID"] = _upn.Text;
            builder["PWD"] = _pwd.Text;
            //var connStr = String.Format(@"Data Source = n9lxnyuzhv.database.windows.net; Authentication = Active Directory Password; UID = {0}; PWD = {1}", _upn.Text, _pwd.Text);
            var conn = new SqlConnection(builder.ConnectionString); //  builder.ConnectionString);
            await DisplayData(_upn.Text, conn);
        }
    }
}
