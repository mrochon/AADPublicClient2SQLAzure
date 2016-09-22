using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinForms2SQLWithAAD
{
    public partial class AdventureWorksModel : DbContext
    {
        public AdventureWorksModel(SqlConnection conn, bool contextOwnsConnection = true)  :
            base(conn, contextOwnsConnection)
        {
            Database.SetInitializer< AdventureWorksModel>(null);
        }
    }
}
