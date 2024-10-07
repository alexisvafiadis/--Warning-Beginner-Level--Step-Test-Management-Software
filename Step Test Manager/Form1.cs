using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Windows;
using Size = System.Windows.Size;
using Application = System.Windows.Forms.Application;
using MessageBox = System.Windows.Forms.MessageBox;
using System.Globalization;
using System.Windows.Forms.DataVisualization.Charting;
using System.Net.Mail;
using System.Net;

namespace Step_Test_Manager
{
    public partial class Form1 : Form
    {
        bool inSession = false;
        MP chosenmp;
        int chosenid = 0;
        int chosenrow = 0;
        System.Timers.Timer timer;
        int m, s;
        DBEntities db = new DBEntities();
        ST currenttest;
        List<decimal> hrlist = new List<decimal>();
        List<decimal> AerList;
        bool testdone;
        int gridx, gridy;
        SmtpClient smtpClient;
        public Form1()
        {
            InitializeComponent();
            this.inSession = false;

            //BirthDateDate.Value = Convert.ToDateTime(01/01/1999,08,25);

            tabControl1.Appearance = TabAppearance.FlatButtons;
            tabControl1.ItemSize = new System.Drawing.Size(0, 1);
            tabControl1.SizeMode = TabSizeMode.Fixed;

            foreach (TabPage tab in tabControl1.TabPages)
            {
                tab.Text = "";
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'database1DataSet.ST' table. You can move, or remove it, as needed.
            // TODO: This line of code loads data into the 'database1DataSet.MP' table. You can move, or remove it, as needed.
            timer = new System.Timers.Timer();
            timer.Interval = 1000;
            timer.Elapsed += OnTimeEvent;
            GenderCmb.SelectedIndex = 0;
            InputGender.SelectedIndex = 0;
            StepHeightCmb.SelectedItem = Properties.Settings.Default.stepheight;
            DefaultStepHeightCmb.SelectedItem = Properties.Settings.Default.stepheight;
            DefaultSendMailCmb.SelectedItem = Properties.Settings.Default.sendmail;
            TinitInfoTxt.Text = Properties.Settings.Default.tinit;
            StepHeightInfoCmb.Text = Properties.Settings.Default.stepheight;
            TinitTxt.Text = Properties.Settings.Default.tinit;
            LastUsedDate.Value = DateTime.Today;
            populateGrid(db.MP.ToList());

            TestTakingChart.Series.Add("TrendLine");
            TestTakingChart.Series["TrendLine"].ChartType = SeriesChartType.Line;
            TestTakingChart.Series["TrendLine"].BorderWidth = 3;
            TestTakingChart.Series["TrendLine"].Color = Color.Red;
            currenttest = new ST();

            smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("com540test@gmail.com", "justatest"),
                EnableSsl = true,
            };
        }
        private void OnTimeEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            Invoke(new Action(() =>
            {
                s += 1;
                if (s == 60)
                {
                    s = 0;
                    m += 1;
                }
                if (m == 60)
                {
                    timer.Stop();
                }
                CountdownTxt.Text = String.Format("{0}:{1}", m.ToString().PadLeft(2, '0'), s.ToString().PadLeft(2, '0'));
            }));
        }
        private void ImportFileBtn_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string path = openFileDialog1.FileName;
                List<string> mpList = GetDataFromFile(path);
                addToDb(mpList);
                populateGrid(db.MP.ToList());
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
            {
                return;
            }
            int i = (int)dataGridView1.Rows[e.RowIndex].Cells[0].Value;
            if (tabControl1.SelectedTab == MenuPage)
            {
            MP mp = db.MP.AsEnumerable().Where(m => m.Id == i).First();
            IDTxt.Text = mp.Id.ToString();
            LastNameTxt.Text = mp.FamilyName;
            FirstNameTxt.Text = mp.FirstName;
            if (mp.BirthDate.HasValue)
            {
                AgeTxt.Text = mp.BirthDate.ToString().Replace(" 00:00:00", "");
            }
            else
            {
                AgeTxt.Text = "dd/MM/YYYY";
            }
            GenderCmb.SelectedItem = mp.Gender;
            emailTxt.Text = mp.email;
            if (button1.Text == "Search")
            {
                SearchToEdit();
            }
            }
            else if (tabControl1.SelectedTab == ChoosePersonPage)
            {
                if (chosenrow != -1)
                {
                    for (int j = 0; j < dataGridView1.Rows[chosenrow].Cells.Count; j++)
                    {
                        dataGridView1.Rows[chosenrow].Cells[j].Style.BackColor = dataGridView1.DefaultCellStyle.BackColor;
                    }
                }
                chosenid = i;
                chosenrow = e.RowIndex;
                for (int j = 0; j < dataGridView1.Rows[e.RowIndex].Cells.Count; j++)
                {
                    dataGridView1.Rows[e.RowIndex].Cells[j].Style.BackColor = Color.SkyBlue;
                }
            }
        }
        public void populateGrid(List<MP> mPs)
        {
            dataGridView1.DataSource = mPs;
        }
        public void addToDb(List<string> mpList)
        {
            foreach (string item in mpList)
            {
                string[] wordArray = item.Split(',');
                MP mp = new MP();
                mp.FamilyName = wordArray[0].ToString();
                mp.FirstName = wordArray[1].ToString();
                if (wordArray[2] != "none")
                {
                    try
                    {
                        mp.BirthDate = Convert.ToDateTime(wordArray[2]);
                    }
                    catch (Exception ex) { MessageBox.Show("Error with the birth date\n" + ex.ToString()); }
                }
                if ("MaleFemale".Contains(wordArray[3])) mp.Gender = wordArray[3].ToString();
                if (wordArray[4] != "none") mp.email = wordArray[4].ToString();
                try
                {
                    if (wordArray[5] != "none") mp.lastused = Convert.ToDateTime(wordArray[5]);
                }
                catch ( Exception ex) {MessageBox.Show("Error with the last used date\n" + ex.ToString()); }
                db.MP.Add(mp);
                db.SaveChanges();
            }
        }
        public List<string> GetDataFromFile(string path)
        {
            StreamReader sr = new StreamReader(path);
            List<string> personlist = new List<string>();
            while (!sr.EndOfStream)
            {
                personlist.Add(sr.ReadLine());
            }
            sr.Close();
            return personlist;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "Edit")
            {
                String bd = AgeTxt.Text;
                if (!IsDateTimeValid(bd) && bd != "dd/MM/YYYY")
                {
                    MessageBox.Show("Enter a valid birth date, or just write 'dd/MM/YYYY' if you don't know it");
                    AgeTxt.Text = "dd/MM/YYYY";
                }
                int i = int.Parse(IDTxt.Text);
                MP mp = db.MP.Where(m => m.Id == i).First();
                mp.FamilyName = LastNameTxt.Text;
                mp.FirstName = FirstNameTxt.Text;
                if (!(AgeTxt.Text == "dd/MM/YYYY"))
                {
                    mp.BirthDate = Convert.ToDateTime(AgeTxt.Text);
                }
                mp.Gender = GenderCmb.SelectedItem.ToString();
                mp.email = emailTxt.Text;
                mp.lastused = DateTime.Now;
                db.SaveChanges();
                populateGrid(db.MP.ToList());
                EditToSearch();
            }
            else if (button1.Text == "Search")
            {
                int age;
                List<MP> mPs;
                IQueryable<MP> iq = db.MP.Where(m =>
m.FamilyName.Contains(LastNameTxt.Text) &&
m.FirstName.Contains(FirstNameTxt.Text) &&
m.email.Contains(emailTxt.Text) &&
m.Gender.Equals(GenderCmb.SelectedItem.ToString()));
                if (!LastUsedBox.Checked)
                {
                    iq = iq.Where(m => m.lastused.Day == LastUsedDate.Value.Day);
                }
                //Code that doesn't work for unknown reason, so abandoned for now
//                if (!AercapBox.Checked)
//                {
//                    iq = iq.Where(m => m.ST.Any());
//                   iq = iq.Where(m => m.ST.AsQueryable().Last().aercap >= MinaercapNum.Value && m.ST.AsQueryable().Last().aercap <= MaxaercapNum.Value);
//                }
                if (!int.TryParse(AgeTxt.Text, out age)) {
                    if (AgeTxt.Text == "") {
                        mPs = iq.ToList<MP>();
                    }
                    else {
                        MessageBox.Show("Please enter a valid input for the age");
                        return;
                    }
                }
                else
                {
                    mPs = iq.AsEnumerable().Where(m => m.BirthDate.HasValue).Where(m =>
calculateage(Convert.ToDateTime(m.BirthDate)) == age
).ToList<MP>();
                }
                if (mPs.Count > 0)
                {
                    populateGrid(mPs);
                }
                else
                {
                    MessageBox.Show("Search was not found!");
                }
            }
        }
        private void SearchToEdit()
        {
            LastUsedLbl.Visible = LastUsedLayout.Visible = AerCapLbl.Visible = AercapSearchLayout.Visible = false;
            button1.Text = "Edit";
            AgeLbl.Text = "Date of Birth";
        }
        private void EditToSearch()
        {
            LastUsedLbl.Visible = LastUsedLayout.Visible = AerCapLbl.Visible = AercapSearchLayout.Visible = true;

            button1.Text = "Search";
            AgeLbl.Text = "Age";
            IDTxt.Text = "";
            LastNameTxt.Text = "";
            FirstNameTxt.Text = "";
            AgeTxt.Text = "";
            GenderCmb.SelectedIndex = 0;
            emailTxt.Text = "";
            LastUsedDate.Value = LastUsedDate.MinDate;
        }
public bool IsDateTimeValid(string dateTime)
{
    string[] formats = { "dd/MM/yyyy" };
    DateTime parsedDateTime;
    return DateTime.TryParseExact(dateTime, formats, new CultureInfo("en-GB"), DateTimeStyles.None, out parsedDateTime);
}
private void dataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            List<MP> mpList;
            switch (e.ColumnIndex)
            {
                case 0:
                    mpList = db.MP.OrderBy(c => c.Id).ToList();
                    break;
                case 1:
                    mpList = db.MP.OrderBy(c => c.FamilyName).ToList();
                    break;
                case 2:
                    mpList = db.MP.OrderBy(c => c.FirstName).ToList();
                    break;
                case 3:
                    mpList = db.MP.OrderBy(c => c.BirthDate).ToList();
                    break;
                case 4:
                    mpList = db.MP.OrderBy(c => c.Gender).ToList();
                    break;
                case 5:
                    mpList = db.MP.OrderBy(c => c.lastused).ToList();
                    break;
                case 6:
                    mpList = db.MP.OrderBy(c => c.email).ToList();
                    break;

                default:
                    mpList = db.MP.ToList();
                    break;
            }
            populateGrid(mpList);
        }

        private void NewPersonBtn_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = NewPersonPage;
            BirthInputCheckbox.Checked = false;
        }

        private void RegisterBtn_Click(object sender, EventArgs e)
        {
            if (InputGender.SelectedIndex == -1 || InputLast.Text == "" || InputFirst.Text == "")
            {
                MessageBox.Show("You haven't filled all of the required fields!");
                return;
            }
            MP mp = new MP();
            mp.FamilyName = InputLast.Text;
            mp.FirstName = InputFirst.Text;
            if (!BirthInputCheckbox.Checked)
            {
                mp.BirthDate = InputBirth.Value;
            }
            mp.Gender = InputGender.SelectedItem.ToString();
            if (InputEmail.Text != "") { mp.email = InputEmail.Text; }
            mp.lastused = DateTime.Now;
            db.MP.Add(mp);
            db.SaveChanges();
            populateGrid(db.MP.ToList());
            if (!inSession)
            {
                RegisterMenu.Show(RegisterBtn, new System.Drawing.Point(0, RegisterBtn.Height));
            }
            else
            {
                chosenmp = mp;
                chosenid = mp.Id;
                filltest();
            }
        }

        private void goToMenuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = MenuPage;
            resetall(false);
        }

        private void registerNewPersonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearPersoninput();
        }
        private void ClearPersoninput()
        {
            InputLast.Text = "";
            InputFirst.Text = "";
            InputGender.SelectedItem = "Male";
            InputEmail.Text = "";
        }

        private void GmailBtn_Click(object sender, EventArgs e)
        {
            InputEmail.Text = InputEmail.Text + "@gmail.com";
        }

        private void OutlookBtn_Click(object sender, EventArgs e)
        {
            InputEmail.Text = InputEmail.Text + "@outlook.com";
        }

        private void YahooBtn_Click(object sender, EventArgs e)
        {
            InputEmail.Text = InputEmail.Text + "@yahoo.com";
        }

        private void StartSessionBtn_Click(object sender, EventArgs e)
        {
            currenttest.date = TestDate.Value;
            if (TinitTxt.Text == "") {
                System.Windows.Forms.MessageBox.Show("No tester's initials were entered! Please enter one");
                return;
            }
            else if (StepHeightCmb.SelectedIndex == -1)
            {
                System.Windows.Forms.MessageBox.Show("No step height was chosen! Please choose one!");
                return;
            }
            currenttest.stepheight = Convert.ToInt32(StepHeightCmb.SelectedItem);
            currenttest.tinit = TinitTxt.Text;
            inSession = true;
            chooseperson();
        }

        private void chooseperson()
        {
            tabControl1.SelectedTab = ChoosePersonPage;
            chosenmp = new MP();
            AddPersonLayout.Controls.Add(RegisterNewPersonContentLayout, 0, 1);
            ChooseExistingLayout.Controls.Add(dataGridView1, 0, 1);
            BirthInputCheckbox.Checked = false;
        }

        // useless now but saved just in case //
        private void addstepheight(int sh)
        {
            if (Properties.Settings.Default.stepheight == "")
            {
                Properties.Settings.Default.stepheight += sh.ToString();
            }
            else
            {
                Properties.Settings.Default.stepheight += "," + sh.ToString();
            }
        }
        private void delstepheight(int sh)
        {
            int index = Properties.Settings.Default.stepheight.IndexOf(sh.ToString());
            if (index == 0)
            {
                Properties.Settings.Default.stepheight.Replace(sh.ToString(), "");
            }
            else
            {
                Properties.Settings.Default.stepheight.Replace("," + sh.ToString(), "");
            }
        }
        // //
        private void NewSessionBtn_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = NewSessionPage;
            TestDate.Value = DateTime.Today;
        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = MenuPage;
        }

        private int calculateage(DateTime bd)
        {
            var today = DateTime.Today;
            var age = today.Year - bd.Year;
            if (bd.Date > today.AddYears(-age))
            {
                age--;
            }
            return age;
        }
        private void calculatetestdata()
        {
            FillHRBtn.Visible = false;
            FillHRNum1.Enabled = FillHRNum2.Enabled = FillHRNum3.Enabled = FillHRNum4.Enabled = FillHRNum5.Enabled = false;
            if (currenttest.stepheight == 15)
            {
                AerList = new List<decimal>()
                {
                    11,
                    14,
                    18,
                    21,
                    25
                };
            }
            else if (currenttest.stepheight == 20)
            {
                AerList = new List<decimal>()
                {
                    12,
                    17,
                    21,
                    25,
                    29
                };
            }
            else if (currenttest.stepheight == 25)
            {
                AerList = new List<decimal>()
                {
                    14,
                    19,
                    24,
                    28,
                    33
                };
            }
            else if (currenttest.stepheight == 30)
            {
                AerList = new List<decimal>()
                {
                    16,
                    21,
                    27,
                    32,
                    37
                };
            }
            else
            {
                MessageBox.Show("Step Height Error");
                resetall(false);
                tabControl1.SelectedTab = MenuPage;
                return;
            }
            if (hrlist.Count < AerList.Count) AerList.RemoveRange(hrlist.Count, AerList.Count - hrlist.Count);

            //This for loop is used 3 times because it seems to miss values probably because the list is adapted
            //every time something is removed, so every other value is skipped but i can't find a good fix
            // [x for x in hrlist if x > 0.5 * currenttest.maxhr] is a possibility i found but that wouldnt
            //modify the aerlist...
            for (int i = 0; i < AerList.Count; i++)
            {
                if (hrlist[i] < (decimal)0.5 * currenttest.maxhr)
                {
                    hrlist.Remove(hrlist[i]);
                    AerList.Remove(AerList[i]);
                }
            }
            for (int i = 0; i < AerList.Count; i++)
            {
                if (hrlist[i] < (decimal)0.5 * currenttest.maxhr)
                {
                    hrlist.Remove(hrlist[i]);
                    AerList.Remove(AerList[i]);
                }
            }
            for (int i = 0; i < AerList.Count; i++)
            {
                if (hrlist[i] < (decimal)0.5 * currenttest.maxhr)
                {
                    hrlist.Remove(hrlist[i]);
                    AerList.Remove(AerList[i]);
                }
            }
            if (hrlist.Count == 0)
            {
                MessageBox.Show("All of your heart rate values are either too low or too high to calculate anything, please redo the test.");
                FillHRNum1.Enabled = FillHRNum2.Enabled = FillHRNum3.Enabled = FillHRNum4.Enabled = FillHRNum5.Enabled = true;
                FillHRNum1.Value = FillHRNum2.Value = FillHRNum3.Value = FillHRNum4.Value = FillHRNum5.Value = 0;
                FillHRBtn.Visible = true;
                FillHRNum2.Visible = FillHRNum3.Visible = FillHRNum4.Visible = FillHRNum5.Visible = FillHRLbl2.Visible = FillHRLbl3.Visible = FillHRLbl4.Visible = FillHRLbl5.Visible = false;
                HRLayoutPanel.Controls.Add(FillHRBtn, 2, 1);
                return;
            }
            (decimal a, decimal b) = leastRegLine(AerList, hrlist);
            if ((a, b) == (0, 0)) return;
            decimal aercap = currenttest.aercap = Decimal.Round((currenttest.maxhr - a) / b, 4);
            displaychart();
            // foreach (var point in TestTakingChart.Series["TrendLine"].Points.ToList())
            //{
            //if (point.YValues.Contains(currenttest.maxhr)) { 
            //    decimal aercap = currenttest.aercap = (decimal) point.XValue;
            int r;
            if (chosenmp.Gender == "Male")
            {
                if (currenttest.age < 20)
                {
                    if (aercap < 30) r = 1;
                    else if (aercap < 39) r = 2;
                    else if (aercap < 48) r = 3;
                    else if (aercap < 60) r = 4;
                    else r = 5;
                }
                else if (currenttest.age < 30)
                {
                    if (aercap < 28) r = 1;
                    else if (aercap < 35) r = 2;
                    else if (aercap < 44) r = 3;
                    else if (aercap < 55) r = 4;
                    else r = 5;
                }
                else if (currenttest.age < 40)
                {
                    if (aercap < 26) r = 1;
                    else if (aercap < 34) r = 2;
                    else if (aercap < 40) r = 3;
                    else if (aercap < 50) r = 4;
                    else r = 5;
                }
                else if (currenttest.age < 50)
                {
                    if (aercap < 25) r = 1;
                    else if (aercap < 32) r = 2;
                    else if (aercap < 37) r = 3;
                    else if (aercap < 46) r = 4;
                    else r = 5;
                }
                else if (currenttest.age < 60)
                {
                    if (aercap < 23) r = 1;
                    else if (aercap < 29) r = 2;
                    else if (aercap < 35) r = 3;
                    else if (aercap < 44) r = 4;
                    else r = 5;
                }
                else
                {
                    if (aercap < 20) r = 1;
                    else if (aercap < 25) r = 2;
                    else if (aercap < 33) r = 3;
                    else if (aercap < 40) r = 4;
                    else r = 5;
                }
            }
            else
            {
                if (currenttest.age < 20)
                {
                    if (aercap < 29) r = 1;
                    else if (aercap < 36) r = 2;
                    else if (aercap < 44) r = 3;
                    else if (aercap < 55) r = 4;
                    else r = 5;
                }
                else if (currenttest.age < 30)
                {
                    if (aercap < 27) r = 1;
                    else if (aercap < 32) r = 2;
                    else if (aercap < 40) r = 3;
                    else if (aercap < 50) r = 4;
                    else r = 5;
                }
                else if (currenttest.age < 40)
                {
                    if (aercap < 25) r = 1;
                    else if (aercap < 30) r = 2;
                    else if (aercap < 36) r = 3;
                    else if (aercap < 45) r = 4;
                    else r = 5;
                }
                else if (currenttest.age < 50)
                {
                    if (aercap < 22) r = 1;
                    else if (aercap < 28) r = 2;
                    else if (aercap < 34) r = 3;
                    else if (aercap < 43) r = 4;
                    else r = 5;
                }
                else if (currenttest.age < 60)
                {
                    if (aercap < 21) r = 1;
                    else if (aercap < 26) r = 2;
                    else if (aercap < 33) r = 3;
                    else if (aercap < 41) r = 4;
                    else r = 5;
                }
                else
                {
                    if (aercap < 19) r = 1;
                    else if (aercap < 24) r = 2;
                    else if (aercap < 31) r = 3;
                    else if (aercap < 39) r = 4;
                    else r = 5;
                }
            }
            if (r == 1) currenttest.rating = "Poor";
            if (r == 2) currenttest.rating = "Below Average";
            if (r == 3) currenttest.rating = "Average";
            if (r == 4) currenttest.rating = "Good";
            if (r == 5) currenttest.rating = "Excellent";
            AerCapResultTxt.Text = currenttest.aercap.ToString() + " mlsO2/kg/min";
            RatingResultTxt.Text = currenttest.rating.ToString();
            TestTakingChart.Visible = TestResultLayout.Visible = true;
            testdone = true;
            SaveTestBtn.Text = "Save Test";
            SaveTestBtn.Visible = true;
        }
        private void setXYlists()
        {
            if (currenttest.stepheight == 15)
            {
                AerList = new List<decimal>()
                {
                    11,
                    14,
                    18,
                    21,
                    25
                };
            }
            else if (currenttest.stepheight == 20)
            {
                AerList = new List<decimal>()
                {
                    12,
                    17,
                    21,
                    25,
                    29
                };
            }
            else if (currenttest.stepheight == 25)
            {
                AerList = new List<decimal>()
                {
                    14,
                    19,
                    24,
                    28,
                    33
                };
            }
            else if (currenttest.stepheight == 30)
            {
                AerList = new List<decimal>()
                {
                    16,
                    21,
                    27,
                    32,
                    37
                };
            }
            else
            {
                MessageBox.Show("Step Height Error");
                resetall(false);
                tabControl1.SelectedTab = MenuPage;
                return;
            }
            if (hrlist.Count < AerList.Count) AerList.RemoveRange(hrlist.Count, AerList.Count - hrlist.Count);
            for (int i = 0; i < AerList.Count; i++)
            {
                if (hrlist[i] < (decimal)0.5 * currenttest.maxhr)
                {
                    hrlist.Remove(hrlist[i]);
                    AerList.Remove(AerList[i]);
                }
            }
        }
        private void displaychart()
        {
            TestTakingChart.Series["Graph"].Color = Color.DarkCyan;
            TestTakingChart.Series["Graph"].BorderWidth = 3;
            TestTakingChart.ChartAreas["ChartArea1"].AxisX.Minimum = 10;
            TestTakingChart.ChartAreas["ChartArea1"].AxisX.Maximum = (double) currenttest.aercap + 10;
            TestTakingChart.ChartAreas["ChartArea1"].AxisY.Minimum = 60;
            TestTakingChart.ChartAreas["ChartArea1"].AxisY.Maximum = currenttest.maxhr;

            for (int i = 0; i < hrlist.Count(); i++)
            {
                TestTakingChart.Series["Graph"].Points.AddXY(AerList[i], hrlist[i]);
            }

            string typeRegression = "Linear";
            string forecasting = "5";
            string error = "false";
            string forecastingError = "false";
            string parameters = typeRegression + ',' + forecasting + ',' + error + ',' + forecastingError;
            TestTakingChart.Series["Graph"].Sort(PointSortOrder.Ascending, "X");
            TestTakingChart.DataManipulator.FinancialFormula(FinancialFormula.Forecasting, parameters, TestTakingChart.Series["Graph"], TestTakingChart.Series["TrendLine"]);
        }
            
            
        private static decimal calculateB(List<decimal> x,
                                 List<decimal> y)
        {
            int n = x.Count;
            decimal sx = x.Sum();
            decimal sy = y.Sum();
            decimal sxsy = 0;
            decimal sx2 = 0;

            for (int i = 0; i < n; i++)
            {
                sxsy += x[i] * y[i];
                sx2 += x[i] * x[i];
            }
            if (n * sx2 - sx * sx == 0) return 0;
            decimal b = (decimal)(n * sxsy - sx * sy) /
                               (n * sx2 - sx * sx);

            return b;
        }
        public static (decimal, decimal) leastRegLine(List<decimal> X, List<decimal> Y)
        {

            decimal b = calculateB(X, Y);
            if (b == 0) return (0, 0);
            int n = X.Count;
            decimal meanX = X.Sum() / n;
            decimal meanY = Y.Sum() / n;
            return  (meanY - b * meanX, b);
        }

        private void filltest()
        {
            tabControl1.SelectedTab = FillTestPage;
            FillHRLbl1.Visible = FillHRLbl2.Visible = FillHRLbl3.Visible = FillHRLbl4.Visible = FillHRLbl5.Visible = FillHRNum1.Visible = FillHRNum2.Visible = FillHRNum3.Visible = FillHRNum4.Visible = FillHRNum5.Visible = FillHRBtn.Visible = false;
            TestTakingChart.Visible = TestResultLayout.Visible = false;
            if (hrlist.Any()) { hrlist.Clear(); }
            testdone = false;
            fillperson();
            DateInfoDate.Value = DateTime.Today;
            if (inSession)
            {
                finishpretest1();
                fillpretest1();
            }
            if (chosenmp.BirthDate.HasValue)
            {
                AgeInfoNum.Value = currenttest.age = calculateage(Convert.ToDateTime(chosenmp.BirthDate));
                if (inSession) finishpretest2();
            }
        }
        private void finishpretest2()
        {
            PreTestInfoBtn.Visible = false;
            AgeInfoNum.ReadOnly = true;
            AgeInfoNum.Increment = 0;
            currenttest.maxhr = (220 - currenttest.age);
            MaxHRInfoTxt.Text = currenttest.maxhr.ToString();
            FillHRLbl1.Visible = FillHRNum1.Visible = FillHRBtn.Visible = true;
            FillHRNum1.Enabled = FillHRNum2.Enabled = FillHRNum3.Enabled = FillHRNum4.Enabled = FillHRNum5.Enabled = true;
        }
        private void finishpretest1()
        {
            StepHeightInfoCmb.Enabled = false;
            DateInfoDate.Enabled = false;
            TinitInfoTxt.ReadOnly = true;
            AgeInfoNum.ReadOnly = false;
            PreTestInfoBtn.Visible = true;
            AgeInfoNum.Increment = 1;
        }
        private void fillperson()
        {
            LastNameInfoTxt.Text = chosenmp.FamilyName;
            FirstNameInfoTxt.Text = chosenmp.FirstName;
            GenderInfoTxt.Text = chosenmp.Gender;
            BirthDateInfoTxt.Text = chosenmp.BirthDate.ToString().Replace(" 00:00:00", "");
            emailInfoTxt.Text = chosenmp.email;
        }

        private void fillpretest1()
        {
            StepHeightInfoCmb.SelectedItem = currenttest.stepheight.ToString();
            DateInfoDate.Value = Convert.ToDateTime(currenttest.date);
            TinitInfoTxt.Text = currenttest.tinit;
        }

        private void SettingsBtn_Click(object sender, EventArgs e)
        {
            backto(SettingsPage);
        }

        private void BackBtn_Click(object sender, EventArgs e)
        {
            populateGrid(db.MP.ToList());
            if (button1.Text == "Edit") EditToSearch();
        }

        private void PlayBtn_Click(object sender, EventArgs e)
        {
            timer.Enabled = !timer.Enabled;
        }

        private void ResetBtn_Click(object sender, EventArgs e)
        {
            timerreset();
        }

        private void EnterTestPage_Leave(object sender, EventArgs e)
        {
            timerreset();
            Application.DoEvents();
        }

        private void timerreset()
        {
            timer.Stop();
            m = 0;
            s = 0;
            CountdownTxt.Text = String.Format("{0}:{1}", m.ToString().PadLeft(2, '0'), s.ToString().PadLeft(2, '0'));
        }
        private void RegisterChooseBtn_Click(object sender, EventArgs e)
        {
            filltest();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            backto(MenuPage);
        }
        private void backto(TabPage tp)
        {
            if (tp == tabControl1.SelectedTab) return;
            if (tp == HelpPage)
            {
                Help helpform = new Help();
                helpform.Show();
                return;
            }
            if (tabControl1.SelectedTab == MenuPage || tabControl1.SelectedTab == AnalyticsPage)
            {
                tabControl1.SelectedTab = tp;
                resetall(false);
            }
            else
            {
                DialogResult dialogResult = MessageBox.Show("Do you really want to go back without saving?", "Confirmation", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    tabControl1.SelectedTab = tp;
                    resetall(false);
                }
                else if (dialogResult == DialogResult.No)
                {
                    //nothing?
                }
            }
        }

        private void resetall(Boolean session)
        {
            ClearPersoninput();
            if (!session)
            {
                this.currenttest = new ST();
            }
            else
            {
                int stepheight = this.currenttest.stepheight;
                DateTime date = Convert.ToDateTime(this.currenttest.date);
                date.AddHours(-date.Hour);
                date.AddMinutes(-date.Minute);
                date.AddSeconds(-date.Second);
                String tinit = this.currenttest.tinit;
                this.currenttest = new ST();
                this.currenttest.stepheight = stepheight;
                this.currenttest.date = date;
                this.currenttest.tinit = tinit;
            }
            inSession = session;
            testdone = false;
            chosenid = 0;
            chosenrow = 0;
            hrlist = new List<Decimal>();
            DataMenuLayout.Controls.Add(dataGridView1, 1, 0);
            RegisterNewPersonLayout.Controls.Add(RegisterNewPersonContentLayout, 0, 1);
            GenderCmb.SelectedIndex = 0;
            InputGender.SelectedIndex = 0;
            StepHeightCmb.SelectedItem = Properties.Settings.Default.stepheight;
            DefaultStepHeightCmb.SelectedItem = Properties.Settings.Default.stepheight;
            DefaultSendMailCmb.SelectedItem = Properties.Settings.Default.sendmail;
            TinitTxt.Text = Properties.Settings.Default.tinit;
            LastUsedDate.Value = DateTime.Today;
            StepHeightInfoCmb.Enabled = true;
            DateInfoDate.Enabled = true;
            TinitInfoTxt.ReadOnly = false;
            AgeInfoNum.ReadOnly = false;
            PreTestInfoBtn.Visible = true;
            AgeInfoNum.Value = 0;
            TinitInfoTxt.Text = Properties.Settings.Default.tinit;
            StepHeightInfoCmb.Text = Properties.Settings.Default.stepheight;
            MaxHRInfoTxt.Text = "";
            FillHRLbl1.Visible = FillHRLbl2.Visible = FillHRLbl3.Visible = FillHRLbl4.Visible = FillHRLbl5.Visible = FillHRNum1.Visible = FillHRNum2.Visible = FillHRNum3.Visible = FillHRNum4.Visible = FillHRNum5.Visible = FillHRBtn.Visible = false;
            FillHRNum1.Value = FillHRNum2.Value = FillHRNum3.Value = FillHRNum4.Value = FillHRNum5.Value = 0;
            HRLayoutPanel.Controls.Add(FillHRBtn, 2, 1);
            TestTakingChart.Visible = TestResultLayout.Visible = SaveTestBtn.Visible = false;
            TestTakingChart.Series["Graph"].Points.Clear();
            RemarksResultTxt.Text = "";
        }

        private void showtest(MP mp, ST st)
        {

        }

        private void ChoosePersonHomeBtn_Click(object sender, EventArgs e)
        {
            backto(MenuPage);
        }

        private void RegisterAndSelectBtn_Click(object sender, EventArgs e)
        {
            RegisterBtn_Click(sender, e);
        }

        private void SearchBtn_Click(object sender, EventArgs e)
        {
            String t = SearchTxt.Text;
            List<MP> mPs = db.MP.Where(m =>
            m.FirstName.Contains(t) || m.FamilyName.Contains(t)
            ).ToList();
            populateGrid(mPs);
        }

        private void SearchTxt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SearchBtn.PerformClick();
            }
        }

        private void SelectPersonBtn_Click(object sender, EventArgs e)
        {
            if (chosenid != 0) {
                chosenmp = db.MP.AsEnumerable().Where(m => m.Id == chosenid).First();
                filltest();
            }
        }

        private void PreTestInfoBtn_Click(object sender, EventArgs e)
        {
            if (AgeInfoNum.Value == 0 || AgeInfoNum.Value != (int)AgeInfoNum.Value)
            {
                MessageBox.Show("Please enter a valid age");
                return;
            }
            if (!inSession)
            {
                if (TinitInfoTxt.Text == "")
                {
                    System.Windows.Forms.MessageBox.Show("No tester's initials were entered! Please enter one");
                    return;
                }
                else if (StepHeightInfoCmb.SelectedIndex == -1)
                {
                    System.Windows.Forms.MessageBox.Show("No step height was chosen! Please choose one!");
                    return;
                }
                currenttest.date = DateInfoDate.Value;
                currenttest.stepheight = Convert.ToInt32(StepHeightInfoCmb.SelectedItem);
                currenttest.tinit = TinitInfoTxt.Text;
                finishpretest1();
            }
            currenttest.age = Convert.ToInt32(AgeInfoNum.Value);
            finishpretest2();
        }

        private void FillHRBtn_Click(object sender, EventArgs e)
        {
            int i = HRLayoutPanel.GetPositionFromControl(FillHRBtn).Row;
            Boolean b;
            decimal hr;
            if (i == 1) hr = FillHRNum1.Value;
            else if (i == 2) hr = FillHRNum2.Value;
            else if (i == 3) hr = FillHRNum3.Value;
            else if (i == 4) hr = FillHRNum4.Value;
            else if (i == 5) hr = FillHRNum5.Value;
            else return;
            if (hr > (decimal)0.85 * currenttest.maxhr)
            {
                calculatetestdata();
            }
            else if (hrlist.Count() == 4)
            {
                hrlist.Add(hr);
                calculatetestdata();
            }
            else
            {
                hrlist.Add(hr);
                HRLayoutPanel.Controls.Add(FillHRBtn, 2, i + 1);
                HRLayoutPanel.GetControlFromPosition(1, i + 1).Visible = true;
                HRLayoutPanel.GetControlFromPosition(0, i + 1).Visible = true;
            }
        }

        private void SettingsSaveBtn_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.stepheight = DefaultStepHeightCmb.SelectedItem.ToString();
            Properties.Settings.Default.tinit = DefaultTinitTxt.Text;
            Properties.Settings.Default.sendmail = DefaultSendMailCmb.SelectedItem.ToString();
            Properties.Settings.Default.Save();
            tabControl1.SelectedTab = MenuPage;
            resetall(false);
        }

        private void AddPersonSettingsBtn_Click(object sender, EventArgs e)
        {
            backto(SettingsPage);
        }

        private void HelpBtn_Click(object sender, EventArgs e)
        {
            backto(HelpPage);
        }

        private void AddPersonHelpBtn_Click(object sender, EventArgs e)
        {
            backto(HelpPage);
        }

        private void FillHelpBtn_Click(object sender, EventArgs e)
        {
            backto(HelpPage);
        }

        private void ChoosePersonHelpBtn_Click(object sender, EventArgs e)
        {
            backto(HelpPage);
        }

        private void NewSessionHelpBtn_Click(object sender, EventArgs e)
        {
            backto(HelpPage);
        }

        private void SettingsHelpBtn_Click(object sender, EventArgs e)
        {
            backto(HelpPage);
        }

        private void FillSettingsBtn_Click(object sender, EventArgs e)
        {
            backto(SettingsPage);
        }

        private void ChoosePersonSettingsBtn_Click(object sender, EventArgs e)
        {
            backto(SettingsPage);
        }

        private void NewSessionSettingsBtn_Click(object sender, EventArgs e)
        {
            backto(SettingsPage);
        }

        private void SettingsSettingsBtn_Click(object sender, EventArgs e)
        {
            backto(SettingsPage);
        }

        private void FillHomeBtn_Click(object sender, EventArgs e)
        {
            backto(MenuPage);
        }

        private void SettingsHomeBtn_Click(object sender, EventArgs e)
        {
            backto(MenuPage);
        }

        private void NewSessionHomeBtn_Click(object sender, EventArgs e)
        {
            backto(MenuPage);
        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            Task.Delay(10);
            if (e.RowIndex == -1) return;
            String colname = dataGridView1.Columns[e.ColumnIndex].Name;
            int i = (int)dataGridView1.Rows[e.RowIndex].Cells[0].Value;
            if (colname == "Add")
            {
                if (tabControl1.SelectedTab == MenuPage)
                {
                    chosenid = i;
                    chosenmp = db.MP.AsEnumerable().Where(m => m.Id == i).First();
                    filltest();
                }
                else
                {
                    MessageBox.Show("You cannot use that here!");
                }
            }
            else if (colname == "View")
            {
                if (tabControl1.SelectedTab == MenuPage)
                {
                    Task.Delay(10);
                    chosenmp = db.MP.AsEnumerable().Where(m => m.Id == i).First();
                    if (chosenmp.ST.Any())
                    {
                        STMenu.Items.Clear();
                        List<ST> orderedlist = chosenmp.ST.OrderBy(s => s.date).ToList();
                        for (int j = 0; j < orderedlist.Count; j++)
                        {
                            STMenu.Items.Add(orderedlist[j].date.ToString().Replace(" 00:00:00", ""));
                            STMenu.Items[j].Click += STMenuItemClick;
                        }
                        STMenu.Show(dataGridView1, new System.Drawing.Point(gridx, gridy));
                        return;
                    }
                    else
                    {
                        MessageBox.Show("No tests were found!");
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("You cannot use that here!");
                }
            }
        }
        private void STMenuItemClick(object sender, EventArgs e)
        {
            var clickedItem = sender as ToolStripMenuItem;

            if (clickedItem == null)
            {
                MessageBox.Show("The event was not registered correctly.");
                return;
            }
            else
            {
                try
                {
                    currenttest = chosenmp.ST.AsEnumerable().Where(s => s.date == Convert.ToDateTime(clickedItem.Text)).First();
                }
                catch (Exception ex) { MessageBox.Show(ex.ToString()); return; }
                tabControl1.SelectedTab = FillTestPage;
                currenttest = chosenmp.ST.First();
                fillperson();
                fillpretest1();
                FillHRLbl1.Visible = FillHRLbl2.Visible = FillHRLbl3.Visible = FillHRLbl4.Visible = FillHRLbl5.Visible = FillHRNum1.Visible = FillHRNum2.Visible = FillHRNum3.Visible = FillHRNum4.Visible = FillHRNum5.Visible = true;
                FillHRBtn.Visible = PreTestInfoBtn.Visible = false;
                String[] arr = currenttest.hrlist.Split(',');
                for (int j = 0; j < arr.Length; j++)
                {
                    hrlist.Add(Convert.ToInt32(arr[j]));
                }
                setXYlists();
                displaychart();
                MaxHRInfoTxt.Text = currenttest.maxhr.ToString();
                TinitInfoTxt.Text = currenttest.tinit;
                AgeInfoNum.Value = currenttest.age;
                for (int j = 0; j < hrlist.Count; j++)
                {
                    if (j == 0) FillHRNum1.Value = hrlist[j];
                    else if (j == 1) FillHRNum2.Value = hrlist[j];
                    else if (j == 2) FillHRNum3.Value = hrlist[j];
                    else if (j == 3) FillHRNum4.Value = hrlist[j];
                    else if (j == 4) FillHRNum5.Value = hrlist[j];
                    else
                    {
                        MessageBox.Show("There was a problem with the indexes of the heart rates");
                        resetall(false);
                        tabControl1.SelectedTab = MenuPage;
                    }
                }
                TestTakingChart.Visible = true;
                AerCapResultTxt.Text = currenttest.aercap.ToString();
                RatingResultTxt.Text = currenttest.rating;
                RemarksResultTxt.Text = currenttest.remarks;
                SaveTestBtn.Text = "Delete Test";
                SaveTestBtn.Visible = true;
            }
        }

        private void HelpHomeBtn_Click(object sender, EventArgs e)
        {
            backto(MenuPage);
            backto(HelpPage);
        }

        private void AnalyticsBtn_Click(object sender, EventArgs e)
        {
            List<ST> list = db.ST.ToList();
            decimal avgaercap = 0;
            decimal mavgaercap = 0;
            int mtot = 0;
            decimal favgaercap = 0;
            int ftot = 0;
            int poor, bavg, avg, good, excellent;
            poor = bavg = avg = good = excellent = 0;
            for (int i = 0; i < list.Count; i++)
            {
                avgaercap += list[i].aercap;
                if (db.MP.AsEnumerable().Where(m => m.Id == list[i].MemberId).First().Gender == "Male")
                {
                    mavgaercap += list[i].aercap;
                    mtot++;
                }
                if (db.MP.AsEnumerable().Where(m => m.Id == list[i].MemberId).First().Gender == "Female")
                {
                    favgaercap += list[i].aercap;
                    ftot++;
                }
                if (list[i].rating == "Poor") poor++;
                if (list[i].rating == "Below Average") bavg++;
                if (list[i].rating == "Average") avg++;
                if (list[i].rating == "Good") good++;
                if (list[i].rating == "excellent") excellent++;
            }
            avgaercap = Decimal.Round(avgaercap / list.Count, 3);
            AvgaercapValueLbl.Text = avgaercap.ToString();
            mavgaercap = Decimal.Round(mavgaercap / mtot, 3);
            MaleavgaercapValueLbl.Text = mavgaercap.ToString();
            favgaercap = Decimal.Round(favgaercap / ftot, 3);
            FemaleavgaercapValueLbl.Text = favgaercap.ToString();

            PoorValueLbl.Text = poor.ToString();
            BelowavgValueLbl.Text = bavg.ToString();
            AvgValueLbl.Text = avg.ToString();
            GoodValueLbl.Text = good.ToString();
            ExcellentValueLbl.Text = excellent.ToString();

            tabControl1.SelectedTab = AnalyticsPage;

        }

        private void AnalyticsHomeBtn_Click(object sender, EventArgs e)
        {
            backto(MenuPage);
        }

        private void AnalyticsHelpBtn_Click(object sender, EventArgs e)
        {
            backto(HelpPage);
        }

        private void AnalyticsSettingsBtn_Click(object sender, EventArgs e)
        {
            backto(SettingsPage);
        }

        private void dataGridView1_MouseClick(object sender, MouseEventArgs e)
        {
            gridx = e.X;
            gridy = e.Y;
        }

        private void SaveTestBtn_Click(object sender, EventArgs e)
        {
            if (SaveTestBtn.Text == "Delete Test")
            {
                DialogResult dialogResult = MessageBox.Show("Do you really want to delete the test?", "Confirmation", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    db.ST.Remove(db.ST.Where(s => s.Id == currenttest.Id).First());
                    db.SaveChanges();
                    resetall(false);
                    tabControl1.SelectedTab = MenuPage;
                }
                else if (dialogResult == DialogResult.No)
                {
                    resetall(false);
                    tabControl1.SelectedTab = MenuPage;
                }
                return;
            }
            if (!testdone)
            {
                MessageBox.Show("Finih the test first!");
                return;
            }
            currenttest.hrlist = "";
            List<String> strings = hrlist.ConvertAll<String>(x => x.ToString());
            currenttest.hrlist = String.Join(",", strings);
            currenttest.remarks = RemarksResultTxt.Text;
            db.MP.AsEnumerable().Where(m => m.Id == chosenid).First().ST.Add(currenttest);
            db.MP.AsEnumerable().Where(m => m.Id == chosenid).First().lastused = DateTime.Now;
            db.SaveChanges();
            if (Properties.Settings.Default.sendmail == "yes") smtpClient.Send("com540test@gmail.com", chosenmp.email, "Step Test Results", "Hello " + chosenmp.FirstName + " " + chosenmp.FamilyName + "!\n Here are your step test results: \n\nAerobic capacity: " + currenttest.aercap.ToString() + "\nRating: " + currenttest.rating + "\nRemarks: " + currenttest.remarks);
            if (inSession)
            {
                DialogResult dialogResult = MessageBox.Show("Your test has been saved. Would you like to go take another test?", "Confirmation", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    resetall(true);
                    chooseperson();
                }
                else if (dialogResult == DialogResult.No)
                {
                    resetall(false);
                    tabControl1.SelectedTab = MenuPage;
                }
            }
            else
            {
                resetall(false);
                tabControl1.SelectedTab = MenuPage;
            }
        }
    }
}
