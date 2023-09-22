using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
using CrystalDecisions.Shared;

namespace HealthInsuranceSystem
{
    public partial class MainForm : Form
    {
        private const string ConnectionString = "Data Source=orcl; User Id=scott; Password=tiger;";

        private OracleConnection con;
        CrystalReport1 report;
        private long UserSSN = 0;
        private bool m_ageflag = false;
        
        public MainForm()
        {
            InitializeComponent();
        }

        private void ConnectedForm_Load(object sender, EventArgs e)
        {
            LoadTabs(true);

            con = new OracleConnection(ConnectionString);
            con.Open();

            report = new CrystalReport1();

            OracleCommand cmd = new OracleCommand();
            cmd.CommandText = "select Name from Insurance_Plans";
            cmd.CommandType = CommandType.Text;
            cmd.Connection = con;

            OracleDataReader dReader = cmd.ExecuteReader();
            while (dReader.Read())
            {
                comboBox1.Items.Add(dReader[0]);
            }
            dReader.Close();
        }

        private void LoadTabs(bool login)
        {
            tabPage1.Enabled = login;
            tabPage2.Enabled = login;
            tabPage3.Enabled = !login;

            tabControl1.SelectTab(login ? 0 : 2);
                
        }

        private void ConnectedForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            con.Close();
        }

        private void reg_btn_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab(1);
        }

        private void login_btn2_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab(0);
        }

        private void login_btn_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(login_user.Text) && !string.IsNullOrEmpty(login_pw.Text))
            {
                try
                {
                    OracleCommand cmd = new OracleCommand("OnLogin", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("LogUser", login_user.Text);
                    cmd.Parameters.Add("LogPassword", login_pw.Text);
                    cmd.Parameters.Add("Result", OracleDbType.Int64, ParameterDirection.Output);
                    cmd.ExecuteNonQuery();

                    UserSSN = Convert.ToInt64(cmd.Parameters["Result"].Value.ToString());
                    if (UserSSN > 0)
                    {
                        MessageBox.Show("Successfully logged in!", "Logged In", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadCurrentPlan();

                        LoadTabs(false);
                        return;
                    }
                }
                catch { }
                MessageBox.Show("Failed to login, please enter correct username and password.", "Error");
            }
            else
                MessageBox.Show("Please fill username and password.", "Missing Fields");
        }

        

        private void reg_btn2_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(reg_user.Text)
                && !string.IsNullOrEmpty(reg_pw.Text)
                && !string.IsNullOrEmpty(reg_fname.Text)
                && !string.IsNullOrEmpty(reg_lname.Text)
                && !string.IsNullOrEmpty(reg_phone.Text)
                )
            {
                try
                {
                    OracleCommand cmd = new OracleCommand("OnRegistration", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("Username", reg_user.Text);
                    cmd.Parameters.Add("Password", reg_pw.Text);
                    cmd.Parameters.Add("FirstName", reg_fname.Text);
                    cmd.Parameters.Add("LastName", reg_lname.Text);
                    cmd.Parameters.Add("JobTitle", reg_job.Text);
                    cmd.Parameters.Add("PhoneNum", reg_phone.Text);
                    cmd.Parameters.Add("Gender", reg_gender_m.Checked ? "M" : "F");
                    cmd.Parameters.Add("DOB", reg_dob.Value);
                    cmd.Parameters.Add("Address", reg_address.Text);

                    cmd.Parameters.Add("Result", OracleDbType.Int64, ParameterDirection.Output);
                    cmd.ExecuteNonQuery();

                    long ssn = Convert.ToInt64(cmd.Parameters["Result"].Value.ToString());
                    if (ssn > 0)
                    {
                        MessageBox.Show("Your account has been registered.", "Registered", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        tabControl1.SelectTab(0);
                        return;
                    }
                }
                catch { }
                MessageBox.Show("Failed to register, username is already used.", "Error");
            }
            else
                MessageBox.Show("Please fill missing fields.", "Missing Fields");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ApplyForInsurance(textBox1.Text);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ChangePlan(textBox1.Text);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            CancelPlan();
        }

        private void LoadCurrentPlan()
        {
            OracleCommand cmd = new OracleCommand("SELECT ip.Name, ps.Start_Date, ps.End_Date FROM Plan_Subscriptions ps, Insurance_Plans ip WHERE ps.P_ID = ip.P_ID AND ps.SSN = :ssn", con);
            cmd.Parameters.Add("ssn", UserSSN);

            OracleDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                current_plan.Text = reader["Name"].ToString();
                start_date.Text = Convert.ToDateTime(reader["Start_Date"]).ToString("dd-MM-yy");
                end_date.Text = Convert.ToDateTime(reader["End_Date"]).ToString("dd-MM-yy");
            }
            reader.Close();
        }

        void AgeChecker()
        {
            DateTime BirthDate = new DateTime();

            OracleCommand cmd = new OracleCommand();
            cmd.CommandText = @"select DOB from Users
                                where SSN = :uSSN";
            cmd.Connection = con;
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("uSSN", UserSSN);

            OracleDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                BirthDate = reader.GetDateTime(0);
            }
            reader.Close();
            int age = DateTime.Today.Year - BirthDate.Year;

            //month and day validation
            if (BirthDate > DateTime.Today.AddYears(-age))
            {
                age--;
            }

            //required age to apply for insurance is 18
            if (age < 18)
            {
                MessageBox.Show("You do not fulfill the requirements to Apply for this plan.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                m_ageflag = true;
                return;
            }
        }

        void ApplyForInsurance(string id)
        {
            try
            {
                OracleCommand cmd = new OracleCommand();
                cmd.Connection = con;

                m_ageflag = false;
                AgeChecker();
                if (m_ageflag)
                {
                    return;
                }

                //Applying for plan
                cmd.CommandText = ("insert into Plan_Subscriptions values( :uSSN, :pID, :startDate, :endDate)");
                cmd.CommandType = CommandType.Text;

                //generating todays Date and 10 years from now
                DateTime TodayDate = DateTime.Today;
                string sDate = TodayDate.ToString("dd-MMM-yyyy");
                string eDate = TodayDate.AddYears(10).ToString("dd-MMM-yyyy");

                cmd.Parameters.Add("uSSN", UserSSN);
                cmd.Parameters.Add("pID", id.ToString());
                cmd.Parameters.Add("startDate", sDate);
                cmd.Parameters.Add("endDate", eDate);


                int r = cmd.ExecuteNonQuery();
                if (r == -1)
                {
                    MessageBox.Show("Applying failed!");
                }
                else
                {
                    current_plan.Text = comboBox1.SelectedItem.ToString();
                    start_date.Text = Convert.ToDateTime(sDate).ToString("dd-MM-yy");
                    end_date.Text = Convert.ToDateTime(eDate).ToString("dd-MM-yy");
                    MessageBox.Show("You are now subscribed to this plan!");
                }
            }
            catch
            {
                MessageBox.Show("You are already subscribed to a plan.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        bool CancelPlan()
        {
            OracleCommand cmd = new OracleCommand();
            cmd.Connection = con;

            int planCount = -1;

            //check if table is empty
            cmd.CommandText = @"select count(*) from plan_subscriptions
                                where SSN =:uSSN";
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("uSSN", UserSSN);

            OracleDataReader rdr = cmd.ExecuteReader();

            if (rdr.Read())
            {
                planCount = Convert.ToInt32(rdr.GetValue(0));
            }
            rdr.Close();

            if (planCount == 0)
            {
                MessageBox.Show("You have no plan to cancel/change.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            //canceling part
            OracleCommand c = new OracleCommand();
            c.Connection = con;
            c.CommandText = @"delete from Plan_Subscriptions
                              where SSN =:u";
            c.CommandType = CommandType.Text;
            c.Parameters.Add("u", UserSSN);

            int r2 = c.ExecuteNonQuery();

            if (r2 > 0) // rows effected
            {
                current_plan.Text = "none";
                start_date.Text = "none";
                end_date.Text = "none";
                MessageBox.Show("Current plan canceled successfully");
                
                return true;
            }
            MessageBox.Show("Operation failed!");
            return false;
        }

        void ChangePlan(string id)
        {
            if (CancelPlan())
                ApplyForInsurance(id);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            OracleCommand cmd = new OracleCommand();
            cmd.Connection = con;

            // Filling ID box
            cmd.CommandText = @"select P_ID from Insurance_Plans
                                where Name =: planName";
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("planName", comboBox1.SelectedItem.ToString());

            string planID = "";
            OracleDataReader dr1 = cmd.ExecuteReader();
            if (dr1.Read())
            {
                planID = dr1[0].ToString();
                textBox1.Text = dr1[0].ToString();
            }
            dr1.Close();

            //Filling DataGridView with providers info
            string cmdstr = @"select Name, Type, Address from providers
                              where pr_id in (select pr_id from plan_providers where p_id =: planID)";
            OracleDataAdapter adapter = new OracleDataAdapter(cmdstr, ConnectionString);
            adapter.SelectCommand.Parameters.Add("planID", planID);
            DataSet ds = new DataSet();
            adapter.Fill(ds);
            dataGridView1.DataSource = ds.Tables[0];
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox2.Text)
                && !string.IsNullOrEmpty(textBox3.Text)
                && !string.IsNullOrEmpty(textBox4.Text)
                )
            {
                try
                {
                    OracleCommand cmd = new OracleCommand("insert into Dependents values (:SSN, :FirstName, :LastName, :Gender, :DOB, :Relationship)", con);

                    cmd.Parameters.Add("SSN", UserSSN);
                    cmd.Parameters.Add("FirstName", textBox4.Text);
                    cmd.Parameters.Add("LastName", textBox3.Text);
                    cmd.Parameters.Add("Gender", radioButton2.Checked ? "M" : "F");
                    cmd.Parameters.Add("DOB", dateTimePicker1.Value);
                    cmd.Parameters.Add("Relationship", textBox2.Text);
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Dependent added successfully.", "Registered", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch
                {
                    MessageBox.Show("Failed to add dependent.", "Error");
                }
            }
            else
                MessageBox.Show("Please fill missing fields.", "Missing Fields");
        }

        private void searchButton_Click(object sender, EventArgs e)
        {
            if (hospbox.Checked)
            {
                OracleDataAdapter adapter = new OracleDataAdapter("select Name,Address,Type from providers where type ='Hospital'", ConnectionString);
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                dataGridView3.DataSource = ds.Tables[0];
                errorlabel.Hide();
            }

            if (pharmbox.Checked)
            {
                OracleDataAdapter adapter = new OracleDataAdapter("select Name,Address,Type from providers where type ='Pharmacy'", ConnectionString);
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                dataGridView2.DataSource = ds.Tables[0];
                errorlabel.Hide();
            }

            if (!pharmbox.Checked && !hospbox.Checked)         
                errorlabel.Show();
        }
        
        private void crystalReportViewer1_Load(object sender, EventArgs e)
        {

        }

        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            if (e.TabPageIndex == 5)
            {
                report.SetParameterValue(0, UserSSN);
                crystalReportViewer1.ReportSource = report;
            }
        }
    }
}
