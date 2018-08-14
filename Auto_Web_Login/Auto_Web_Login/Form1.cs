using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


/// <summary>
/// Created By Philip M 2017 - 2018 under MIT License for everyone to use, share and collaborate to make it better.
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

        private string html = string.Empty;
        private HtmlAgilityPack.HtmlDocument doc;

        private bool gowithProxy = false;
        private bool goWithoutProxy = false;

        Thread sendPasswordToTxtBThread;

       


        //place holders while running async gathering info then are used to update the GUI by Invoke
        private List<string> listBoxErrorItemsList = new List<string>();
        private List<string> tableItems1 = new List<string>();
        private List<string> tableItems2 = new List<string>();
        private List<string> tableItems3 = new List<string>();
        private List<string> tableItems4 = new List<string>();
        private List<string> tableItems5 = new List<string>();

        //holds the current key value of the dictionary
        int currProxyValue = 0;

        //holds the proxy list
        Dictionary<int, string> proxyDictionary = new Dictionary<int, string>();


        private bool threadStarted = false;

        public Form1()
        {
            InitializeComponent();
            
        }
        //this just navigates to the URL 
        private  void btnNavigate_Click(object sender, EventArgs e)
        {
           
            if(txtBURL.Text.StartsWith("http://") | txtBURL.Text.StartsWith("https://"))  //this is to make sure the user doesnt do stupid things ;)
            {
                goWithoutProxy = true;
                gowithProxy = false;

                NavigateToURL();

                currentUrl = "";

            }
            else
            {
                webBrowser1.Navigate("http://www.bing.com"); //redirect here incase of stupidity
            }
           
        }
        //method to navigate to the url and parse out html content
        private void NavigateToURL()
        {
            //dont use a proxy
            if(goWithoutProxy)
            {
                //reset the IE setting to default and remove proxy settings
                WinInetInterop.RestoreSystemProxy();
            }
            //use the proxy
            if (gowithProxy)
            {
               
                WinInetInterop.SetConnectionProxy(txtBProxy.Text);
            }
           

            listBoxErrorItemsList.Clear();
            tableItems1.Clear();
            tableItems2.Clear();
            tableItems3.Clear();
            tableItems4.Clear();
            tableItems5.Clear();

            listBoxErrors.Items.Clear();

            progressbarComplete = 0;
           // toolStripProgressBar1.Value = 0;
            toolStripStatusLabel1.Text = "Started";

            btnNavigate.Enabled = false;
            txtBURL.Enabled = false;

            
            //does anyone know how to not have the form lock up due to webrowser load on a shitty internet connection ??????????
            webBrowser1.ScriptErrorsSuppressed = true;  //surpress any script errors

            webBrowser1.Navigate(txtBURL.Text);
            /* webBrowser1.Navigate(txtBURL.Text, null,null,authHdr);*/   //navigate to url with credentials
            //  await Task.Run(() => webBrowser1.Navigate(txtBURL.Text)); //navigate to url async


        }
       
            

           //this is the first part of the HTML async and then updates the GUI
        private  void FillTable()
        {

           
            try
            {

                //this rests the data grid view boxes cleans out the datasource
                dataGridViewInputIds.DataSource = null;
                dataGridViewButtonIds.DataSource = null;
                dataGridViewOtherHtml.DataSource = null;
                dataGridViewWebLinks.DataSource = null;
                dataGridView1.DataSource = null;

                



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


               // CarryOnFillTable();

                ////I dont need to make 2 web requests i will just use the document completed event from the webbrowser and get the document text

                Invoke((MethodInvoker)async delegate
                        {
                           
                            await Task.Factory.StartNew(() => CarryOnFillTable(), TaskCreationOptions.LongRunning);  //this one works best async as it handles better in the thread pool
                        });


             
            }
            catch (Exception ex)
            {
               // MessageBox.Show(ex.Message, "Fill Tables Failed");

                listBoxErrorItemsList.Add(DateTime.Now.ToLongTimeString() + "     " + ex.Message + "  == Fill Tables Failed");
            }
        }

        //this is the second part of the HTML async and then updates the GUI
        private void CarryOnFillTable()
        { 
       
            try { 
            
                doc = new HtmlAgilityPack.HtmlDocument();
               // doc.LoadHtml(html); //documentWebBrow
                doc.LoadHtml(documentWebBrow);


                //-------------------------------------------------
                //this gets the inputs and id values from the html form
                var inputid = doc.DocumentNode.SelectNodes("//input/@id");
                    if (inputid != null)
                        foreach (HtmlNode btnIDS in inputid)
                        {

                        tableItems1.Add(btnIDS.GetAttributeValue("id", "inputDefault1"));
                        
                        }
                    else
                    {

                    listBoxErrorItemsList.Add(DateTime.Now.ToLongTimeString() + "     " + "No Input IDs Available for this URL");
                
                       // MessageBox.Show("No Input IDs Available for this URL", "What have you done!! ha", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                //-------------------------------------------------
                //this gets the button ids from the html form
                var btnID = doc.DocumentNode.SelectNodes("//button/@class");
                    if (btnID != null)
                        foreach (HtmlNode btnid in btnID)
                        {

                        tableItems2.Add(btnid.GetAttributeValue("id", "inputDefault2"));
                      
                        }
                    else
                    {

                    listBoxErrorItemsList.Add(DateTime.Now.ToLongTimeString() + "     " + "No Button IDs Available for this URL , Trying to find button as an input value instead !!");
                  
                    // MessageBox.Show("No Button IDs Available for this URL , Trying to find button as an input value instead !!", "What have you done!! ha", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                //-------------------------------------------------
                //this gets the button classes from the html form
                var btnClass = doc.DocumentNode.SelectNodes("//button/@class");
                    if (btnClass != null)
                        foreach (HtmlNode btn in btnClass)
                        {

                        tableItems2.Add(btn.GetAttributeValue("class", "buttonDefault"));
                    
                        }
                    else
                    {
                    listBoxErrorItemsList.Add(DateTime.Now.ToLongTimeString() + "     " + "No Button class IDs Available for this URL");
           
                    //  MessageBox.Show("No Button class IDs Available for this URL", "What have you done!! ha", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                //-------------------------------------------------
                //this gets the div ids from the html form
                var divs = doc.DocumentNode.SelectNodes("//div/@id");
                    if(divs != null)
                        foreach(HtmlNode divss in divs)
                        {

                        tableItems3.Add(divss.GetAttributeValue("id", "submitDefault1"));
                      
                        }
                    else
                    {
                    listBoxErrorItemsList.Add(DateTime.Now.ToLongTimeString() + "     " + "No Div with IDs Available for this URL");
                   
                    //  MessageBox.Show("No Div with IDs Available for this URL", "What have you done!! ha", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
              
                //-------------------------------------------------
                //this gets the input names from the html form
                var nameIDs = doc.DocumentNode.SelectNodes("//input/@name");
                        if (nameIDs != null)
                            foreach (HtmlNode btnname in nameIDs)
                            {

                        tableItems3.Add(btnname.GetAttributeValue("name", "submitDefault2"));
                     
                            }
                        else
                        {
                    listBoxErrorItemsList.Add(DateTime.Now.ToLongTimeString() + "     " + "No Input Name IDs Available for this URL ");
                 
                    // MessageBox.Show("No Input Name IDs Available for this URL , Trying to find button as an input value instead !!", "What have you done!! ha", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                //-------------------------------------------------
                //this gets the input names from the html form
                var tableClass = doc.DocumentNode.SelectNodes("//table/@class");
                if (tableClass != null)
                    foreach (HtmlNode btnname in tableClass)
                    {
                        tableItems3.Add(btnname.GetAttributeValue("class", "submitDefault3"));
                     
                    }
                else
                {
                    listBoxErrorItemsList.Add(DateTime.Now.ToLongTimeString() + "     " + "No table Class IDs Available for this URL ");
                  
                    // MessageBox.Show("No Input Name IDs Available for this URL , Trying to find button as an input value instead !!", "What have you done!! ha", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                //-------------------------------------------------
                //this gets the input names from the html form
                var nameIDclass = doc.DocumentNode.SelectNodes("//input/@name");
                if (nameIDclass != null)
                    foreach (HtmlNode btnname in nameIDclass)
                    {
                        tableItems3.Add(btnname.GetAttributeValue("class", "submitDefault3"));
                  
                    }
                else
                {
                    listBoxErrorItemsList.Add(DateTime.Now.ToLongTimeString() + "     " + "No input name Class IDs Available for this URL ");
                 
                    // MessageBox.Show("No Input Name IDs Available for this URL , Trying to find button as an input value instead !!", "What have you done!! ha", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                //-------------------------------------------------
                //this gets the input names from the html form
                var className = doc.DocumentNode.SelectNodes("//input/@class");
                if (className != null)
                    foreach (HtmlNode btnname in className)
                    {
                        tableItems3.Add(btnname.GetAttributeValue("class", "submitDefault3"));
                     
                    }
                else
                {
                    listBoxErrorItemsList.Add(DateTime.Now.ToLongTimeString() + "     " + "No input Class IDs Available for this URL ");
                 
                    // MessageBox.Show("No Input Name IDs Available for this URL , Trying to find button as an input value instead !!", "What have you done!! ha", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                //-------------------------------------------------
                //this gets the input names from the html form
                var Weblinks = doc.DocumentNode.SelectNodes("//a[@href]");
                if (Weblinks != null)
                    foreach (HtmlNode webLink in Weblinks)
                    {

                        tableItems4.Add(webLink.GetAttributeValue("href", "WebLinkDefault"));
                     
                    }
                else
                {
                    listBoxErrorItemsList.Add(DateTime.Now.ToLongTimeString() + "     " + "No Web Links Available for this URL ");
                 
                    // MessageBox.Show("No Links Available for this URL , Trying to find button as an input value instead !!", "What have you done!! ha", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                //-------------------------------------------------
                //this gets the input names from the html form
                var WebImages = doc.DocumentNode.SelectNodes("//img/@src");
                if (WebImages != null)
                    foreach (HtmlNode webLink in WebImages)
                    {

                        tableItems5.Add(webLink.GetAttributeValue("src", "ImageDefault"));
                    
                    }
                else
                {
                    listBoxErrorItemsList.Add(DateTime.Now.ToLongTimeString() + "     " + "No Image sources Available for this URL ");
              
                    //  MessageBox.Show("No Links Available for this URL , Trying to find button as an input value instead !!", "What have you done!! ha", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }




            }
            catch (Exception ex)
            {
                listBoxErrorItemsList.Add(DateTime.Now.ToLongTimeString() + "     " + "Cannot scrape this site Bad Request, Original ERROR: " + ex.Message);
                listBoxErrorItemsList.Add(DateTime.Now.ToLongTimeString() + "     " + "TRY RELOADING");
             
                MessageBox.Show("Cannot scrape this site Bad Request , TRY RELOADING it !!! , Original ERROR : " + ex.Message , "Something has gone wrong !! so fix it yourself lol", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            // invoke method to update the datagrids rows with HTML scraping and listbox with any errors
            Invoke((MethodInvoker) delegate
            {
                foreach(string i in tableItems1)
                {
                    table.Rows.Add(i);

                    dataGridViewInputIds.DataSource = table;
                }
                foreach (string i in tableItems2)
                {
                    table2.Rows.Add(i);

                    dataGridViewButtonIds.DataSource = table2;
                }
                foreach (string i in tableItems3)
                {
                    table3.Rows.Add(i);

                    dataGridViewOtherHtml.DataSource = table3;
                }
                foreach (string i in tableItems4)
                {
                    table4.Rows.Add(i);

                    dataGridViewWebLinks.DataSource = table4;
                }
                foreach (string i in tableItems5)
                {
                    table5.Rows.Add(i);

                    dataGridView1.DataSource = table5;
                }

                toolStripStatusLabelErrorCount.Text = "List of Errors Counter : " + listBoxErrorItemsList.Count.ToString();
                listBoxErrors.Items.AddRange(listBoxErrorItemsList.ToArray());
            });
        }
    


        private void txtBURL_KeyPress(object sender, KeyPressEventArgs e)
        {
            //if enter key is used simulate the navigate button click event
            if(e.KeyChar == (char) ConsoleKey.Enter)
            {

               //btnNavigate_Click(null, null);  //this has the beep sound unless you like that sort of thing lol
                btnNavigate_Click(e.Handled = true,null); // stop the button beep sound
               
              
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
            if(currentUrl == "")
            {
                MessageBox.Show("You must use the ( Set URL Reference ) button before auto sending the logins !!!", "Hey Noob , What ya doing ?", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            //start the timer that will send the password at the interval set
            timerCurrentUrl.Interval = (int)frequencySeconds.Value * 1000;
            timerCurrentUrl.Start();
            keepSending = true;

            btnStartSendFromFile.Enabled = false;
            btnStopSendFromFile.Enabled = true;
            //SendFromFile();

            sendPasswordToTxtBThread = new Thread(new ThreadStart(SendThread));
            sendPasswordToTxtBThread.Start();

            threadStarted = true;
        }
        private void timerCurrentUrl_Tick(object sender, EventArgs e)
        {
            //if the url has changed it is more than lickely you have logged in or got redirected so stop sending
            if(currentUrl != urlChanged)
            {
                //stop sending the passwords
                keepSending = false;

                listBoxFoundPass.Items.Add("Username : " + txtBUsername.Text);
                listBoxFoundPass.Items.Add("Password : " + txtBPassword.Text);

                timerCurrentUrl.Stop();

                btnSendFilePath.Enabled = true;
                btnStartSendFromFile.Enabled = true;

                //stop the change of the proxy
                proxyDictionary.Clear();
                currProxyValue = 0;
                checkBoxProxyAuto.Checked = false;
                timerChangeProxy.Stop();
                timerChangeProxy.Enabled = false;

                try
                {

                    sendPasswordToTxtBThread.Abort();
                }
                catch (Exception ex)
                {
                   // MessageBox.Show(ex.Message , "Auto Sending Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    listBoxErrorItemsList.Add(DateTime.Now.ToLongTimeString() + "     " + ex.Message + "  == Auto Sending Failed");
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

                threadStarted = false;

                sendPasswordToTxtBThread.Abort();
            }
            catch (Exception ex)
            {
               // MessageBox.Show(ex.Message, "Stopping Thread Failed" ,MessageBoxButtons.OK,MessageBoxIcon.Error);

                listBoxErrorItemsList.Add(DateTime.Now.ToLongTimeString() + "     " + ex.Message + "  == Stopping Thread Failed");
            }
        }
       

       // this is how you will send info to the webpage firstly to setup the automatic send web info
        private  void btnTest_Click(object sender, EventArgs e)
        {
             System.Windows.Forms.HtmlDocument doc = webBrowser1.Document;

            try
            {
                HtmlElement username = doc.GetElementById(txtBUserElementID.Text);
                HtmlElement password = doc.GetElementById(txtBPassElementID.Text);
                HtmlElement submit = doc.GetElementById(txtBLoginElementID.Text);

                if (!checkBoxjustButtonClick.Checked)
                {

                    username.SetAttribute("value", txtBUsername.Text);
                    password.SetAttribute("value", txtBPassword.Text);

                }
             
                submit.InvokeMember("click");

              
              
            }
            catch (Exception )
            {
                listBoxErrors.Items.Add(DateTime.Now.ToLongTimeString() + "     " + "ERROR Could Not Send command to web page !!");
               // MessageBox.Show(ex.Message, "m3");
                //throw;
            }

        }
      
        //dispose of resources while form is closing
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
               
                if (!webBrowser1.IsDisposed)
                {
                    webBrowser1.Dispose();
                }
              
              if(timerCurrentUrl.Enabled)
                {
                    timerCurrentUrl.Dispose();
                }


            
              if (threadStarted == true)
                {
                    sendPasswordToTxtBThread.Abort();
                }
                  
  
            }
            catch (Exception )
            {
              //do nothing i dont care just close so
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
            txtBURL.Focus();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
         
            webBrowser1.Refresh();

           // FillTable();

          //  currentUrl = txtBURL.Text;
        }

        private void btnGoForward_Click(object sender, EventArgs e)
        {
           
            if (webBrowser1.CanGoForward)
            {
                webBrowser1.GoForward();
            }
        }

        private void btnGetFreeProxList_Click(object sender, EventArgs e)
        {
            Process.Start("https://free-proxy-list.net/");
        }

        private void btnGoWithProxy_Click(object sender, EventArgs e)
        {
            if (txtBURL.Text.StartsWith("http://") | txtBURL.Text.StartsWith("https://"))  //this is to make sure the user doesnt do stupid things ;)
            {
                goWithoutProxy = false;
                gowithProxy = true;

                NavigateToURL();

                currentUrl = "";

            }
            else
            {
                webBrowser1.Navigate("http://www.bing.com"); //redirect here incase of stupidity
            }
        }

        private void checkBoxProxyAuto_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBoxProxyAuto.Checked)
            {
                if (dataGridViewProxy.Rows.Count > 1)
                {

                  //add the proxylist and ports to a dictionary and the timer will work from that
                        for (int i = 0; i < dataGridViewProxy.RowCount -1; i++)
                        {
                            string ip = dataGridViewProxy.Rows[i].Cells["ProxyIP"].Value.ToString();
                            string port = ":" + dataGridViewProxy.Rows[i].Cells["ProxyPort"].Value.ToString();

                        if (string.IsNullOrEmpty( ip) || string.IsNullOrEmpty(port))
                        {
                            return;
                        
                        }

                        if (!proxyDictionary.ContainsKey(i))
                            {
                                proxyDictionary.Add(i, ip + port);
                            }

                           
                        }
                  
                    timerChangeProxy.Enabled = true;
                    int mins = 1000 * 60 * Convert.ToInt32( ChangeTimeProxyMins.Value.ToString());  //; TimeSpan.FromMinutes(Convert.ToDouble(ChangeTimeProxyMins.Value.ToString())).Seconds;
                    timerChangeProxy.Interval = mins;
                    timerChangeProxy.Start();

                
                }
                else
                {
                    proxyDictionary.Clear();
                    currProxyValue = 0;
                  
                    MessageBox.Show("No Proxy List Data is Entered in Section PROXY LIST", "Hey NOOB!!  What are you doing ?", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    checkBoxProxyAuto.Checked = false;
                    timerChangeProxy.Stop();
                   timerChangeProxy.Enabled = false;
                }
            }
            else
            {
                proxyDictionary.Clear();
                currProxyValue = 0;
                timerChangeProxy.Stop();
                timerChangeProxy.Enabled = false;
            }
        }

       
        //change the current proxy value to the next value else just start again
        private void timerChangeProxy_Tick(object sender, EventArgs e)
        {
          

            Invoke((MethodInvoker)delegate
            {

                int dicCount = proxyDictionary.Keys.Count;

                if (dicCount > 0)
                {
                    for (int i = 0; i < dicCount; i++)
                    {
                        if (currProxyValue == i)
                        {
                            txtBProxy.Text = proxyDictionary[i].ToString();
                           

                            btnGoWithProxy.PerformClick();
                        }

                    }

                    currProxyValue++;

                    if (currProxyValue == dicCount)
                    {
                        currProxyValue = 0;
                    }


                }

            });
         
        }
       private string documentWebBrow ;

        private void backgroundWorkerHtml_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Invoke((MethodInvoker)delegate
            {
                dataGridViewInputIds.DataSource = null;
                dataGridViewButtonIds.DataSource = null;
                dataGridViewOtherHtml.DataSource = null;
                dataGridViewWebLinks.DataSource = null;
                dataGridView1.DataSource = null;

                listBoxErrorItemsList.Clear();
                tableItems1.Clear();
                tableItems2.Clear();
                tableItems3.Clear();
                tableItems4.Clear();
                tableItems5.Clear();

                listBoxErrors.Items.Clear();
            });

            Invoke((MethodInvoker)delegate
            {


                FillTable();
            });

        }

        private void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            progressbarComplete = 0;
            // toolStripProgressBar1.Value = 0;
            toolStripStatusLabel1.Text = "Started";
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            Invoke((MethodInvoker)delegate
            {
                btnNavigate.Enabled = true;
                txtBURL.Enabled = true;
                toolStripStatusLabel1.Text = "Complete";
                txtBURL.Text = webBrowser1.Url.AbsoluteUri; //update the txt url with the new url path if redirected
                urlChanged = webBrowser1.Url.AbsoluteUri;  //update the url changed so that it can compare to the reference url before it was running
                toolStripProgressBar1.ProgressBar.Value = 100;
            });
          

            documentWebBrow = "";
            System.Windows.Forms.HtmlDocument document = null;
            document = webBrowser1.Document;
            documentWebBrow = document.Body.OuterHtml ;

            if(backgroundWorkerHtml.IsBusy == false)
            {
                backgroundWorkerHtml.RunWorkerAsync();
            }

           

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
                listBoxErrorItemsList.Add(DateTime.Now.ToLongTimeString() + "     " + ex.Message + "  == Send Thread ERROR");
               // MessageBox.Show(ex.Message);
                }
                Thread.Sleep(100);
            }
        
    }
}
