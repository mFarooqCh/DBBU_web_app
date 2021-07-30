using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace serverbackup
{
    public partial class Form1 : Form
    {
        string timevalforcopy = String.Empty;
        string timevalforBackup = string.Empty;
        private string connectionstring = ConfigurationManager.ConnectionStrings["TakeBackup"].ConnectionString;
        string databackup = ConfigurationManager.AppSettings["CreateBackup"];
        string destination = ConfigurationManager.AppSettings["CopyRecords"];
        // string date = DateTime.Now.ToString("dd-MM-yyyy-hh-mm-ss-tt");
        string setfilename = ConfigurationManager.AppSettings["Setfilename"];
        DataTable dt = new DataTable();
        string Name = string.Empty;
        public Form1()
        {
            InitializeComponent();

        }

        private void Form1_Load(object sender, EventArgs e)
        {

            timevalforcopy = ConfigurationManager.AppSettings["timevalforcopy"].ToString();
            timevalforBackup = ConfigurationManager.AppSettings["timevalforBackup"].ToString();
            if (timevalforcopy.ToString() == timevalforBackup.ToString())
            {
                Status.Text = "Time should be different for backup and copy data";

            }
            else
            {
                Status.Text = "Backup in process" + Environment.NewLine;
                #region GetDatabaseNames

                DatabasesNames();
              
                #endregion


                #region For backup

                InitTimerone();

                #endregion

                #region For Copy

                InitTimer();

                #endregion
            }
        }

        #region Backup Files Function
        public void Backup()
        {
            try
            {
                Status.Text += "Backup started at " + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss") + Environment.NewLine;
                using (var connection = new SqlConnection(connectionstring))
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        connection.Close();
                    }
                    connection.Open();
                    Status.Text += "Connected with database" + Environment.NewLine;

                    foreach (DataRow item in dt.Rows)
                    {
                        Name = item.ItemArray[0].ToString();
                        string CreatedFilename = Name + "-" + DateTime.Now.ToString("dd-MM-yyyy-hh-mm-ss") + "-" + "Backup.bak";
                        string backupdatarecords = databackup;
                        string backupfile = backupdatarecords + CreatedFilename;
                        var query = "BACKUP DATABASE  " + "[" + Name + "]" + "  TO DISK='" + backupfile + "'";

                        using (var command = new SqlCommand(query, connection))
                        {
                            command.CommandTimeout = 60 * 60;
                            command.ExecuteNonQuery();

                            Status.Text += "Backup generated successfully " + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss") + " " + backupfile + Environment.NewLine;
                            DebugLog(DateTime.Now.ToString("dd-MM-yyyy hh:mm:yy") + " " + CreatedFilename + "\n");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLog(ex.Message.ToString() + "\n");
                Status.Text += ex.Message.ToString() + Environment.NewLine;
            }
        }

        #endregion

        #region Copy Files Function

        public void copyData()
        {

            try
            {
                Status.Text += "Backup copy started at " + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss") + "\n";
                System.IO.DirectoryInfo dirinfo = new System.IO.DirectoryInfo(databackup);
                string SaveFilesInPath = (destination + "\\");

                foreach (System.IO.FileInfo finfo in dirinfo.GetFiles())
                {

                    if (File.Exists(SaveFilesInPath + finfo))
                    {
                        continue;
                    }
                    else
                    {
                        System.IO.File.Copy(finfo.FullName, (SaveFilesInPath + ("\\" + finfo.Name)));
                        DebugLog(DateTime.Now.ToString("dd-MM-yyyy hh:mm:yy") + " " + finfo.Name + "\n");
                    }
                }
                Status.Text += "Backup copied successfully " + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss") + Environment.NewLine;

            }
            catch (Exception ex)
            {
                Status.Text += ex.Message.ToString() + Environment.NewLine;
                ErrorLog(ex.Message.ToString());

            }
        }

        #endregion

        #region Timer Counter For Copy Files

        public void InitTimer()
        {

            timer1 = new Timer();
            timer1.Tick += new System.EventHandler(this.timer1_Tick);
            timer1.Interval = 60000;
            timer1.Start();
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            string systemTime = System.DateTime.Now.ToString("hh:mm tt");
            string setTime = timevalforcopy.ToString();
            if ((systemTime == setTime))
            {
                copyData();
            }

        }

        #endregion

        #region Timer Counter For Bakcup Files

        public void InitTimerone()
        {

            timer2 = new Timer();
            timer2.Tick += new System.EventHandler(this.timer2_Tick);
            timer2.Interval = 60000;
            timer2.Start();
        }
        private void timer2_Tick(object sender, EventArgs e)
        {
            string systemTime = System.DateTime.Now.ToString("hh:mm tt");
            string setTime = timevalforBackup.ToString();
            if ((systemTime == setTime))
            {
                Backup();
            }

        }

        #endregion

        #region Error & Debug Log
        public void DebugLog(string Message)
        {
            string pathfile = ConfigurationManager.AppSettings["Logs"].ToString();
            StreamWriter SW = null;
            try
            {
                if (!Directory.Exists(pathfile))
                {
                    Directory.CreateDirectory(pathfile);
                }

                if (!File.Exists((pathfile
                                + (DateTime.Now.ToString("dd-MM-yyyy") + "-Activity_Log_File.txt"))))
                {
                    SW = new StreamWriter((pathfile
                                    + (DateTime.Now.ToString("dd-MM-yyyy") + "-Activity_Log_File.txt")));
                }
                else
                {
                    SW = File.AppendText((pathfile
                                    + (DateTime.Now.ToString("dd-MM-yyyy") + "-Activity_Log_File.txt")));
                }

                string Data2Write = string.Format("{0}", Message.ToString());
                SW.Write(Data2Write);
                SW.Flush();
                SW.Close();
            }
            catch (Exception ex)
            {
            }

        }
        public void ErrorLog(string message)
        {
            string pathfile = ConfigurationManager.AppSettings["ErrorLogs"].ToString();
            StreamWriter SW = null;
            try
            {
                if (!Directory.Exists(pathfile))
                {
                    Directory.CreateDirectory(pathfile);
                }

                if (!File.Exists((pathfile
                                + (DateTime.Now.ToString("dd-MM-yyyy") + "-Error_Log_File.txt"))))
                {
                    SW = new StreamWriter((pathfile
                                    + (DateTime.Now.ToString("dd-MM-yyyy") + "-Error_Log_File.txt")));
                }
                else
                {
                    SW = File.AppendText((pathfile
                                    + (DateTime.Now.ToString("dd-MM-yyyy") + "-Error_Log_File.txt")));
                }

                string Data2Write = string.Format("{0}", message.ToString());
                SW.Write(Data2Write);
                SW.Flush();
                SW.Close();
            }
            catch (Exception ex)
            {

            }

        }

        #endregion

        #region Get Names 
        public void DatabasesNames()
        {

            SqlConnection connec = new SqlConnection(connectionstring);
            try
            {
                if (connec.State == ConnectionState.Open)
                {
                    connec.Close();
                }

                connec.Open();
                string query = "SELECT name FROM master.dbo.sysdatabases WHERE name NOT IN ('master', 'tempdb', 'model', 'msdb')";
                SqlDataAdapter da = new SqlDataAdapter(query, connec);

                da.Fill(dt);


            }
            catch (Exception ex)
            {


            }


        }

        #endregion
    }
}




