using HtmlAgilityPack;
using System;
using System.Data;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;


/// <summary>
/// Created By Philip M 2017 under MIT License for everyone to use, share and collaborate to make it better.
///
/// </summary>
namespace Auto_Web_Login
{
    public partial class Form1 : Form
    {

        DataTable table , table2 , table3 , table4 , table5;

        private int progressbarComplete = 0;
        private string  inputString = "";
        private bool keepSending;

        private string currentUrl = "";
        private string urlChanged = "";

        Thread sendPasswordToTxtBThread;

        public Form1()
        {
            InitializeComponent();

        }

        private void btnNavigate_Click(object sender, EventArgs e)
        {
            if(txtBURL.Text.StartsWith("http://") | txtBURL.Text.StartsWith("https://"))  //this is to make sure the user doesnt do stupid things ;)
            {
                NavigateToURL();

                currentUrl = txtBURL.Text;

            }
            else
            {
                webBrowser1.Navigate("https://www.google.com"); //redirect here incase of stupidity
            }
          
         
        }
        //method to navigate to the url and parse out html contenet
        private void NavigateToURL()
        {
            progressbarComplete = 0;
           // toolStripProgressBar1.Value = 0;
            toolStripStatusLabel1.Text = "Started";

            btnNavigate.Enabled = false;
            txtBURL.Enabled = false;


            webBrowser1.ScriptErrorsSuppressed = true;  //surpress any script errors
            webBrowser1.Navigate(txtBURL.Text);   //navigate to url
            FillTable();
             // await Task.Factory.StartNew(() => FillTable());   //load async incase of impatient people
        }


        private async void FillTable()
        {

            //.AppendText("started at : " + DateTime.Now + "\n\r");
            try
            {
              
                //this rests the data grid view boxes cleans out the datasource
                dataGridViewInputIds.DataSource = null;
                dataGridViewButtonIds.DataSource = null;
                dataGridViewOtherHtml.DataSource = null;
                dataGridViewWebLinks.DataSource = null;
                dataGridView1.DataSource = null;

                WebRequest request = WebRequest.Create(txtBURL.Text);

            request.Credentials = CredentialCache.DefaultCredentials;
             request.Timeout = 10000;
            
            WebResponse response = request.GetResponse();

            Stream data = response.GetResponseStream();

                string html = string.Empty;

                using (StreamReader sr = new StreamReader(data))  
                {
                    // html = sr.ReadToEnd();
                    html = await (sr.ReadToEndAsync()); //you can choose normal synchronous read but asynchronous is better
                    sr.Close();
                }

                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);

                table = new DataTable("HTMLTableIDs");
                table.Columns.Add("Input ID Found", typeof(string)).SetOrdinal(0);  // to put the column back to position 0 
                dataGridViewInputIds.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridViewInputIds.ScrollBars = ScrollBars.Both;

                table2 = new DataTable("htmlButtons");
                table2.Columns.Add("Button IDs Found", typeof(string)).SetOrdinal(0);
                dataGridViewButtonIds.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridViewButtonIds.ScrollBars = ScrollBars.Both;

                table3 = new DataTable("htmlOther");
                table3.Columns.Add("Other HTML input Found", typeof(string)).SetOrdinal(0);
                dataGridViewOtherHtml.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridViewOtherHtml.ScrollBars = ScrollBars.Both;

                table4 = new DataTable("htmlLinks");
                table4.Columns.Add("Links Found", typeof(string)).SetOrdinal(0);
                dataGridViewWebLinks.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridViewWebLinks.ScrollBars = ScrollBars.Both;

                table5 = new DataTable("htmlImages");
                table5.Columns.Add("Images Found", typeof(string)).SetOrdinal(0);
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridView1.ScrollBars = ScrollBars.Both;

                //-------------------------------------------------
                //this gets the inputs and id values from the html form
                var inputid = doc.DocumentNode.SelectNodes("//input/@id");
                    if (inputid != null)
                        foreach (HtmlNode btnIDS in inputid)
                        {
                            table.Rows.Add(btnIDS.GetAttributeValue("id", "inputDefault1"));


                            dataGridViewInputIds.DataSource = table;
                        }
                    else
                    {
                    listBoxErrors.Items.Add("No Input IDs Available for this URL");
                       // MessageBox.Show("No Input IDs Available for this URL", "What have you done!! ha", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                //-------------------------------------------------
                //this gets the button ids from the html form
                var btnID = doc.DocumentNode.SelectNodes("//button/@class");
                    if (btnID != null)
                        foreach (HtmlNode btnid in btnID)
                        {
                            table2.Rows.Add(btnid.GetAttributeValue("id", "inputDefault2"));


                            dataGridViewButtonIds.DataSource = table2;
                        }
                    else
                    {
                    listBoxErrors.Items.Add("No Button IDs Available for this URL , Trying to find button as an input value instead !!");
                    // MessageBox.Show("No Button IDs Available for this URL , Trying to find button as an input value instead !!", "What have you done!! ha", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                //-------------------------------------------------
                //this gets the button classes from the html form
                var btnClass = doc.DocumentNode.SelectNodes("//button/@class");
                    if (btnClass != null)
                        foreach (HtmlNode btn in btnClass)
                        {
                            table2.Rows.Add(btn.GetAttributeValue("class", "buttonDefault"));


                            dataGridViewButtonIds.DataSource = table2;
                        }
                    else
                    {
                    listBoxErrors.Items.Add("No Button class IDs Available for this URL");
                    //  MessageBox.Show("No Button class IDs Available for this URL", "What have you done!! ha", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                //-------------------------------------------------
                //this gets the div ids from the html form
                var divs = doc.DocumentNode.SelectNodes("//div/@id");
                    if(divs != null)
                        foreach(HtmlNode divss in divs)
                        {
                            table3.Rows.Add(divss.GetAttributeValue("id", "submitDefault1"));


                            dataGridViewOtherHtml.DataSource = table3;
                        }
                    else
                    {
                    listBoxErrors.Items.Add("No Div with IDs Available for this URL");
                    //  MessageBox.Show("No Div with IDs Available for this URL", "What have you done!! ha", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
              
                //-------------------------------------------------
                //this gets the input names from the html form
                var nameIDs = doc.DocumentNode.SelectNodes("//input/@name");
                        if (nameIDs != null)
                            foreach (HtmlNode btnname in nameIDs)
                            {
                                table3.Rows.Add(btnname.GetAttributeValue("name", "submitDefault2"));


                            dataGridViewOtherHtml.DataSource = table3;
                            }
                        else
                        {
                    listBoxErrors.Items.Add("No Input Name IDs Available for this URL ");
                    // MessageBox.Show("No Input Name IDs Available for this URL , Trying to find button as an input value instead !!", "What have you done!! ha", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                //-------------------------------------------------
                //this gets the input names from the html form
                var tableClass = doc.DocumentNode.SelectNodes("//table/@class");
                if (tableClass != null)
                    foreach (HtmlNode btnname in tableClass)
                    {
                        table3.Rows.Add(btnname.GetAttributeValue("class", "submitDefault3"));


                        dataGridViewOtherHtml.DataSource = table3;
                    }
                else
                {
                    listBoxErrors.Items.Add("No table Class IDs Available for this URL ");
                    // MessageBox.Show("No Input Name IDs Available for this URL , Trying to find button as an input value instead !!", "What have you done!! ha", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                //-------------------------------------------------
                //this gets the input names from the html form
                var nameIDclass = doc.DocumentNode.SelectNodes("//input/@name");
                if (nameIDclass != null)
                    foreach (HtmlNode btnname in nameIDclass)
                    {
                        table3.Rows.Add(btnname.GetAttributeValue("class", "submitDefault3"));


                        dataGridViewOtherHtml.DataSource = table3;
                    }
                else
                {
                    listBoxErrors.Items.Add("No input name Class IDs Available for this URL ");
                    // MessageBox.Show("No Input Name IDs Available for this URL , Trying to find button as an input value instead !!", "What have you done!! ha", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                //-------------------------------------------------
                //this gets the input names from the html form
                var className = doc.DocumentNode.SelectNodes("//input/@class");
                if (className != null)
                    foreach (HtmlNode btnname in className)
                    {
                        table3.Rows.Add(btnname.GetAttributeValue("class", "submitDefault3"));


                        dataGridViewOtherHtml.DataSource = table3;
                    }
                else
                {
                    listBoxErrors.Items.Add("No input Class IDs Available for this URL ");
                    // MessageBox.Show("No Input Name IDs Available for this URL , Trying to find button as an input value instead !!", "What have you done!! ha", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                //-------------------------------------------------
                //this gets the input names from the html form
                var Weblinks = doc.DocumentNode.SelectNodes("//a[@href]");
                if (Weblinks != null)
                    foreach (HtmlNode webLink in Weblinks)
                    {
                        table4.Rows.Add(webLink.GetAttributeValue("href", "WebLinkDefault"));


                        dataGridViewWebLinks.DataSource = table4;
                    }
                else
                {
                    listBoxErrors.Items.Add("No Web Links Available for this URL ");
                    // MessageBox.Show("No Links Available for this URL , Trying to find button as an input value instead !!", "What have you done!! ha", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                //-------------------------------------------------
                //this gets the input names from the html form
                var WebImages = doc.DocumentNode.SelectNodes("//img/@src");
                if (WebImages != null)
                    foreach (HtmlNode webLink in WebImages)
                    {
                        table5.Rows.Add(webLink.GetAttributeValue("src", "ImageDefault"));


                        dataGridView1.DataSource = table5;
                    }
                else
                {
                    listBoxErrors.Items.Add("No Image sources Available for this URL ");
                    //  MessageBox.Show("No Links Available for this URL , Trying to find button as an input value instead !!", "What have you done!! ha", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }




            }
            catch (Exception ex)
            {
                listBoxErrors.Items.Add("Cannot scrape this site Bad Request, Original ERROR: " + ex.Message);
                listBoxErrors.Items.Add("TRY RELOADING");
                MessageBox.Show("Cannot scrape this site Bad Request , Original ERROR : " + ex.Message , "Something has gone wrong !! so fix it yourself lol");
            }
        }
    


        private void txtBURL_KeyPress(object sender, KeyPressEventArgs e)
        {
            //if enter key is used simulate the navigate button click event
            if(e.KeyChar == (char) ConsoleKey.Enter)
            {

                btnNavigate_Click(null, null);
            }
        }

        //opens file dialog to get file either richtext or text files filter
        private void btnSendFromFile_Click(object sender, EventArgs e)
        {
          

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "text files (*.txt)|*.txt|Richtext files (*.rtf)|*.rtf";


            if (ofd.ShowDialog() == DialogResult.OK)
            {

                lblFilePath.Text = ofd.FileName;

                btnStartSendFromFile.Enabled = true;
           
            }
        }

        private void btnStartSendFromFile_Click(object sender, EventArgs e)
        {
            timerCurrentUrl.Interval = (int)frequencySeconds.Value * 1000;
            timerCurrentUrl.Start();
            keepSending = true;

            btnStartSendFromFile.Enabled = false;
            btnStopSendFromFile.Enabled = true;
            //SendFromFile();

            sendPasswordToTxtBThread = new Thread(new ThreadStart(SendThread));
            sendPasswordToTxtBThread.Start();
        }
        private void timerCurrentUrl_Tick(object sender, EventArgs e)
        {
            
            if(currentUrl != urlChanged)
            {

                keepSending = false;

                listBoxFoundPass.Items.Add("Username : " + txtBUsername.Text);
                listBoxFoundPass.Items.Add("Password : " + txtBPassword.Text);

                timerCurrentUrl.Stop();

                btnSendFilePath.Enabled = true;
                btnStartSendFromFile.Enabled = true;

                try
                {
                    webBrowser1.Dispose();
                    timerCurrentUrl.Dispose();
                    sendPasswordToTxtBThread.Abort();
                }
                catch (Exception)
                {

                    // throw;
                }
            }
          
        }

        private void btnStopSendFromFile_Click(object sender, EventArgs e)
        {
           
            try
            {
                keepSending = false;

                btnSendFilePath.Enabled = true;
                btnStopSendFromFile.Enabled = false;
                btnStartSendFromFile.Enabled = true;

                sendPasswordToTxtBThread.Abort();
            }
            catch
            {
                
            }
        }
       

       // this is how you will send info to the webpage firstly to setup the automatic send web info
        private void btnTest_Click(object sender, EventArgs e)
        {
             System.Windows.Forms.HtmlDocument doc = webBrowser1.Document;
          
            try
            {
                HtmlElement username = doc.GetElementById(txtBUserElementID.Text);
                HtmlElement password = doc.GetElementById(txtBPassElementID.Text);
                HtmlElement submit = doc.GetElementById(txtBLoginElementID.Text);

              

                username.SetAttribute("value", txtBUsername.Text);
                password.SetAttribute("value", txtBPassword.Text);

                submit.InvokeMember("click");
              
            }
            catch (Exception)
            {

                //throw;
            }

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                sendPasswordToTxtBThread.Abort();

            }
            catch (Exception)
            {

                // throw;
            }
        }

        private void btnGoBack_Click(object sender, EventArgs e)
        {
            if (webBrowser1.CanGoBack)
            {
                webBrowser1.GoBack();
            }
        }
        //this way you can choose the internet explorer version
        private void btnUpgradeBrowser_Click(object sender, EventArgs e)
        {
          

        switch (comBoxUpgradeBrowser.Text)
            {
                case "ver 9":
                    {
                        BrowserHelper.SetBrowserEmulation(AppDomain.CurrentDomain.FriendlyName, IE.IE9);
                        break;
                    }
                case "ver 10":
                    {
                        BrowserHelper.SetBrowserEmulation(AppDomain.CurrentDomain.FriendlyName, IE.IE10);
                        break;
                    }
                case "ver 11":
                    {
                        BrowserHelper.SetBrowserEmulation(AppDomain.CurrentDomain.FriendlyName, IE.IE11);
                        break;
                    }
            }
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comBoxUpgradeBrowser.SelectedIndex = 2;
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            webBrowser1.Refresh();
        }

        private void btnGoForward_Click(object sender, EventArgs e)
        {
            if(webBrowser1.CanGoForward)
            {
                webBrowser1.GoForward();
            }
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            btnNavigate.Enabled = true;
            txtBURL.Enabled = true;
            toolStripStatusLabel1.Text = "Complete";
            txtBURL.Text = webBrowser1.Url.AbsoluteUri; //update the txt url with the new url path if redirected
            urlChanged = webBrowser1.Url.AbsoluteUri;  //update the url changed so that it can compare to the reference url before it was running

        }
        //this is to update the browser search and task complete method its not perfect but it works ok
        private void webBrowser1_ProgressChanged(object sender, WebBrowserProgressChangedEventArgs e)
        {
            if (e.CurrentProgress > 0 && e.MaximumProgress > 0)
            {
                progressbarComplete = (int)(e.CurrentProgress * 100 / e.MaximumProgress);

                if (progressbarComplete < 99)
                {
                    toolStripProgressBar1.ProgressBar.Value = progressbarComplete;

                }
                else
                {
                    toolStripProgressBar1.ProgressBar.Value = 100;
                  
                }
            }
        }


        //use the url as a reference to see when it changes either due to password being right (redirect) and then save the username and password to listbox
        private void btnUrlReference_Click(object sender, EventArgs e)
        {
            currentUrl = txtBURL.Text;  

           
        }

        private void SendThread() //this is a new thread to update password values and handle click events etc.. 
        {
            int sleepTime = (int)frequencySeconds.Value * 1000;

            Invoke((MethodInvoker)delegate
            {
                btnSendFilePath.Enabled = false;
                btnStartSendFromFile.Enabled = false;
            });

                try
                {
                    OpenFileDialog ofd = new OpenFileDialog();
                    ofd.FileName = lblFilePath.Text;

                    if (File.Exists(ofd.FileName))
                    {

                        StreamReader sw = new StreamReader(ofd.OpenFile(), true);
                    {


                       //while keepsending is true and there is still text to read
                        while (keepSending == true && (inputString = (sw.ReadLine())) != null)
                        {

                            Invoke((MethodInvoker)delegate
                           {
                              
                               txtBPassword.Text = inputString; //change the text in the textbox password to new value from file
                               Thread.Sleep(200);  //i put this here as sometimes the button would click before the textboxes were filled?? anyway it works now
                               btnTest_Click(null, null); //click the test button


                           });
                            Thread.Sleep(sleepTime);
                        }
                        sw.Close();
                        sendPasswordToTxtBThread.Abort();

                        btnSendFilePath.Enabled = true;
                        btnStartSendFromFile.Enabled = true;
                      

                    }
                    }
                else
                {
                    MessageBox.Show("Cannot find the file you selected");
                }

                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message);
                }
                Thread.Sleep(100);
            }
        
    }
}
