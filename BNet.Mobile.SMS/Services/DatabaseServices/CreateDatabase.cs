using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNet.Mobile.SMS.Services.DatabaseServices
{
    public class CreateDatabase
    {
        public async Task Create()
        {
            string table = "sms_table";
            List<string> columns = new List<string>();
            columns.Add("id");
            columns.Add("sms_id");
            columns.Add("message");
            columns.Add("created_date");
            columns.Add("created_time");
            columns.Add("address");
            columns.Add("type");

            MySQLService mySQLService = new MySQLService();
            string database_schema = mySQLService.DatabaseName();
            try
            {
                if (database_schema != "")
                {
                    DBSelectValue count_table = new DBSelectValue();
                    count_table.Command("SELECT  COUNT(DISTINCT(table_name)) from information_schema.columns  where table_schema = '" + database_schema + "' AND  table_name = '" + table + "' ");
                    DBQuery_IDU dBQuery_IDU = new DBQuery_IDU();
                    dBQuery_IDU.Command("ALTER TABLE " + database_schema + "." + table + " DISABLE KEYS", "");
                    if (count_table.getValue == "0")
                    {
                        dBQuery_IDU.Command("ALTER TABLE " + database_schema + "." + table + " ROW_FORMAT = Fixed", "");
                        dBQuery_IDU.Command("CREATE TABLE " + database_schema + "." + table + " ( `id` INT(250) NOT NULL AUTO_INCREMENT , PRIMARY KEY (`id`)) ENGINE = InnoDB;", "");
                        //await dBQuery_IDU.Command("ALTER TABLE " + database_schema + "." + table + " ENGINE ='MyISAM'", "");


                        for (int i = 1; i < columns.Count; i++)
                        {
                            DBSelectValue count_columns = new DBSelectValue();
                            count_columns.Command("SELECT COUNT(COLUMN_NAME) FROM  information_schema.columns WHERE table_schema  = '" + database_schema + "' AND table_name ='" + table + "' AND  COLUMN_NAME ='" + columns[i] + "'");
                            if (count_columns.getValue == "0")
                            {
                                dBQuery_IDU.Command("ALTER TABLE " + database_schema + "." + table + "  ADD " + columns[i] + " TEXT NOT NULL", "");// AFTER " + columns[i - 1]
                                dBQuery_IDU.Command("ALTER TABLE " + database_schema + "." + table + " ADD  INDEX (" + columns[i] + ")", "");
                            }
                        }
                        dBQuery_IDU.Command("ALTER TABLE " + database_schema + "." + table + " ENABLE KEYS", "");
                        dBQuery_IDU.Command("ALTER TABLE " + database_schema + "." + table + " CHANGE `id` `id` INT(250) NOT NULL AUTO_INCREMENT", "");
                    }
                }
            }
            catch
            {
            }
            await Task.CompletedTask;
        }
    }
}