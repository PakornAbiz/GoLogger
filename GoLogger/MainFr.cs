using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Net.Sockets;
using System.IO;
using System.Drawing;
using System.Xml;
using System.Net.NetworkInformation;
using System.Linq;

namespace GoLogger
{
    public partial class MainFr : Form
    {
        TcpClient gocatorClient = null;
        List<int> indexDivision;
        CreateTable createTable = null;
        ConnectDB connectDB = null;
        string commandSQL = string.Empty;
        List<int> indexColumnSQL;
        dataFormatFr formatFr = null;
        public string OKStatus { get; set; }
        public string NGStatus { get; set; }
        private bool loopData { get; set; }
        public MainFr()
        {
            InitializeComponent();
            loopData = false;
            createTable = new CreateTable();
            indexDivision = new List<int>();
        }

        public static Thread Start(Action action)
        {
            var thread = new Thread(() => { action(); });
            thread.Start();
            return thread;
        }

        private string GetRowNumber() => Convert.ToString(dataGridView1.Rows.Count + 1);

        private void sentMsg(TcpClient client, string msg)
        {
            NetworkStream stream = null;
            stream = client.GetStream();
            byte[] byteMsg = new byte[1024];
            byteMsg = Encoding.ASCII.GetBytes(msg + "\r\n");
            stream.Write(byteMsg, 0, byteMsg.Length);
            Array.Clear(byteMsg, 0, byteMsg.Length);
        }

        private string readMsg(TcpClient client)
        {
            NetworkStream stream = null;
            byte[] byteMsg = null;
            string recevieData = string.Empty;
            try
            {
                stream = client.GetStream();
                byteMsg = new byte[1024];
                do
                {
                    stream.Read(byteMsg, 0, byteMsg.Length);
                    recevieData = Encoding.ASCII.GetString(byteMsg, 0, byteMsg.Length);
                } while (stream.DataAvailable);
                Array.Clear(byteMsg, 0, byteMsg.Length);
            }
            catch
            {

            }
            return recevieData;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Columns.Count > 0)
            {
                if (gocatorClient.Connected && gocatorClient.Client != null)
                {
                    if (btnStart.Text == "Start")
                    {
                        btnStart.Text = "Stop";
                        btnDis.Enabled = false;
                        loopData = true;
                        try
                        {
                            Start(new Action(recordeData));
                            Start(new Action(ConnectionCheck));
                        }
                        catch
                        {

                        }
                        lbStatus.Text = "Started";
                    }
                    else if (btnStart.Text == "Stop")
                    {
                        btnStart.Text = "Start";
                        btnDis.Enabled = true;
                        loopData = false;
                        lbStatus.Text = string.Empty;
                    }
                }
                else
                {
                    MessageBox.Show("Please check connection", "Error Connection", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Please create the table", "Error Table", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public static TcpState GetState(TcpClient tcpClient)
        {
            try
            {
                var foo = IPGlobalProperties.GetIPGlobalProperties()
             .GetActiveTcpConnections()
             .SingleOrDefault(x => x.LocalEndPoint.Equals(tcpClient.Client.LocalEndPoint));
                return foo != null ? foo.State : TcpState.Unknown;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private void ConnectionCheck()
        {
            while (loopData)
            {
                if (GetState(gocatorClient) != 0 && GetState(gocatorClient).ToString() == "CloseWait")
                {
                    this.Invoke(new Action(() =>
                    {
                        btnStart.Text = "Start";
                        btnDis.Enabled = false;
                        btnCon.Enabled = true;
                        loopData = false;
                        lbStatus.Text = string.Empty;
                    }));
                    MessageBox.Show("Connect lost", "Error Connection", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void recordeData()
        {
            while (loopData)
            {
                string rawMsg = string.Empty;
                string[] dataMsg = null;
                rawMsg = readMsg(gocatorClient);
                dataMsg = rawMsg.Split(',');
                List<string> stdList = new List<string>(dataMsg.Length + 1);
                stdList.Add(GetRowNumber());
                stdList.Add(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                for (int i = 0; i < dataMsg.Length - 1; i++)
                {
                    stdList.Add(dataMsg[i + 1]);
                }
                int indexOfList = 0;
                for (int i = 0; i < stdList.Count; i++)
                {
                    try
                    {
                        if (i == indexDivision[indexOfList])
                        {
                            indexOfList += 1;
                            double bufferDouble = Convert.ToDouble(stdList[i]) / 1000;
                            stdList[i] = bufferDouble.ToString();
                        }
                    }
                    catch
                    {

                    }
                }
                if (loopData)
                {
                    if (dataMsg[0] == "OK")
                    {
                        dataGridView1.Invoke(new Action(() =>
                        {
                            dataGridView1.Rows.Add(stdList.ToArray());
                        }
                        ));
                    }
                    stdList.Clear();
                }

                if (btnConnectDB.Text == "Disconnect")
                {
                    commandSQL = formatFr.cmdSQL;
                    for (int i = 0; i < indexColumnSQL.Count; i++)
                    {
                        if (i != indexColumnSQL.Count-1)
                        {
                            commandSQL += (dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[indexColumnSQL[i]].Value.ToString() + "','");
                        }
                        else
                        {
                            commandSQL += (dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[indexColumnSQL[i]].Value.ToString() + "')");
                        }
                    }
                    if (connectDB.SQLIsConnect)
                    {
                        try
                        {
                            connectDB.insert(commandSQL);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Error insert Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Database disconect", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnCon_Click(object sender, EventArgs e)
        {
            try
            {
                gocatorClient = new TcpClient();
                gocatorClient.Connect(tbIP.Text, Convert.ToInt32(tbPort.Text));
                if (gocatorClient.Connected)
                {
                    lbStatus.Text = "Connected";
                    btnDis.Enabled = true;
                    btnCon.Enabled = false;
                    sentMsg(gocatorClient, "LoadJob");
                    lbJob.Text = readMsg(gocatorClient).Substring(3);
                }
                else
                {
                    lbStatus.Text = "Not connected";
                }
            }
            catch
            {
                lbStatus.Text = "TCP/IP Error";
            }
        }

        private void btnDis_Click(object sender, EventArgs e)
        {
            gocatorClient.Dispose();
            btnCon.Enabled = true;
            btnCreateTable.Enabled = false;
            btnStart.Enabled = false;
            btnDis.Enabled = false;
            lbStatus.Text = "Disconnected";
            lbJob.Text = string.Empty;
            this.Refresh();
        }

        private void btnCreateTable_Click(object sender, EventArgs e)
        {
            if (!loopData)
            {
                createTable.ShowDialog();
                if (DialogResult.OK == createTable.DialogResult)
                {
                    dataGridView1.Columns.Clear();
                    foreach (string item in createTable.GetColumn())
                    {
                        dataGridView1.Columns.Add(item, item);
                    }
                    dataGridView1.Update();
                    foreach (var item in createTable.GetDivisionIndex())
                    {
                        indexDivision.Add(item);
                    }
                    btnStart.Enabled = true;
                }
            }
            else
            {
                MessageBox.Show("Please stop record for create the table", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void MainFr_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                loopData = false;
                if (gocatorClient != null)
                {
                    gocatorClient.Dispose();
                }
            }
            catch
            {

            }
            Environment.Exit(Environment.ExitCode);
        }
        public void ExtoCSV(DataGridView gv, string str)
        {
            String Date = DateTime.Now.ToString("ddMMyyyy");
            if (dataGridView1.Rows.Count > 0)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "CSV (*.csv)|*.csv";
                sfd.FileName = str + Date + ".csv";
                bool fileError = false;
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    if (File.Exists(sfd.FileName))
                    {
                        try
                        {
                            File.Delete(sfd.FileName);
                        }
                        catch (IOException ex)
                        {
                            fileError = true;
                            MessageBox.Show("It wasn't possible to write the data to the disk." + ex.Message);
                        }
                    }
                    if (!fileError)
                    {
                        int columnCount = gv.Columns.Count;
                        string columnNames = "";
                        string[] outputCsv = new string[gv.Rows.Count + 2];
                        for (int i = 0; i < columnCount; i++)
                        {
                            columnNames += gv.Columns[i].HeaderText.ToString() + ",";
                        }
                        outputCsv[0] += "Date :" + DateTime.Now;
                        outputCsv[1] += columnNames;

                        for (int i = 2; (i - 2) < gv.Rows.Count; i++)
                        {
                            for (int j = 0; j < columnCount; j++)
                            {
                                try
                                {
                                    outputCsv[i] += gv.Rows[i - 2].Cells[j].Value.ToString() + ",";
                                }
                                catch
                                {

                                }

                            }
                        }

                        File.WriteAllLines(sfd.FileName, outputCsv, Encoding.UTF8);
                        MessageBox.Show("Data Exported Successfully !!!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                DialogResult dialogResult = MessageBox.Show("Do you want to clear the table?", "Clear Table", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (dialogResult == DialogResult.Yes)
                {
                    gv.Rows.Clear();
                }
            }
            else
            {
                MessageBox.Show("No Record To Export !!!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void btnExCsv_Click(object sender, EventArgs e)
        {
            ExtoCSV(dataGridView1, "GoLogger");
        }

        private void btnRemoveTable_Click(object sender, EventArgs e)
        {
            if (!loopData)
            {
                DialogResult dialogResult = MessageBox.Show("Do you want to remove the table?", "Clear Table", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (dialogResult == DialogResult.Yes)
                {
                    dataGridView1.Rows.Clear();
                    dataGridView1.Columns.Clear();
                    btnCreateTable.Enabled = true;
                    indexDivision.Clear();
                    this.Refresh();
                }
            }
            else
            {
                MessageBox.Show("Please stop record for remove the table", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnSaveConfig_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Columns.Count > 0)
            {
                using (var saveDialogFile = new SaveFileDialog())
                {
                    saveDialogFile.InitialDirectory = @"C:\";
                    saveDialogFile.Title = "Save Comfig Table";
                    saveDialogFile.DefaultExt = "xml";
                    saveDialogFile.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*";
                    saveDialogFile.RestoreDirectory = true;
                    if (saveDialogFile.ShowDialog() == DialogResult.OK)
                    {
                        using (XmlWriter writer = XmlWriter.Create(saveDialogFile.FileName))
                        {
                            writer.WriteStartElement("TableConfig");
                            writer.WriteStartElement("Table");
                            for (int i = 0; i < dataGridView1.Columns.Count; i++)
                            {
                                writer.WriteElementString("Column", dataGridView1.Columns[i].Name);
                            }
                            writer.WriteEndElement();
                            writer.WriteStartElement("DivisionIndex");
                            foreach (var item in createTable.GetDivisionIndex())
                            {
                                writer.WriteElementString("Index", item.ToString());
                            }
                            writer.WriteEndElement();
                            writer.WriteStartElement("SQLServer");
                            writer.WriteElementString("DataSource", tbDataSource.Text);
                            writer.WriteElementString("InitialCatalog", tbInitialCatalog.Text);
                            writer.WriteElementString("Username", tbUserID.Text);
                            writer.WriteElementString("Passsword", tbPassDB.Text);
                            writer.WriteEndElement();
                            writer.Flush();
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please Create Table", "Can't save config", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnUploadConfig_Click(object sender, EventArgs e)
        {
            if (!loopData)
            {
                if (dataGridView1.Columns.Count > 0)
                {
                    var dialogResult = MessageBox.Show("Do you want to change config ?", "Change config", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (dialogResult == DialogResult.Yes)
                    {
                        dataGridView1.Rows.Clear();
                        dataGridView1.Columns.Clear();
                        this.Refresh();
                        ReadConfig();
                        btnStart.Enabled = true;
                    }
                }
                else
                {
                    ReadConfig();
                    btnStart.Enabled = true;
                }
            }
            else
            {
                MessageBox.Show("Please stop record for upload the table", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ReadConfig()
        {
            List<string> columnTable = new List<string>();
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = @"c:\";
                openFileDialog.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var xmlFile = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read);
                    var doc = new XmlDocument();

                    doc.Load(xmlFile);
                    var nodeColumn = doc.GetElementsByTagName("Column");
                    var nodeIndex = doc.GetElementsByTagName("Index");
                    var nodeDS = doc.GetElementsByTagName("DataSource");
                    var nodeIC = doc.GetElementsByTagName("InitialCatalog");
                    var nodeUName = doc.GetElementsByTagName("Username");
                    var nodePass = doc.GetElementsByTagName("Passsword");

                    tbDataSource.Text = nodeDS[0].ChildNodes.Item(0).InnerText;
                    tbInitialCatalog.Text = nodeIC[0].ChildNodes.Item(0).InnerText;
                    tbUserID.Text = nodeUName[0].ChildNodes.Item(0).InnerText;
                    tbPassDB.Text = nodePass[0].ChildNodes.Item(0).InnerText;

                    for (int i = 0; i < nodeColumn.Count; i++)
                    {
                        columnTable.Add(nodeColumn[i].ChildNodes.Item(0).InnerText);
                    }

                    for (int i = 0; i < nodeIndex.Count; i++)
                    {
                        indexDivision.Add(Convert.ToInt32(nodeIndex[i].ChildNodes.Item(0).InnerText));
                    }

                    foreach (var item in columnTable)
                    {
                        dataGridView1.Columns.Add(item, item);
                    }
                }
            }
        }

        private void btnYield_Click(object sender, EventArgs e)
        {
            YieldReport yieldFr = new YieldReport(ref dataGridView1);
            yieldFr.ShowDialog();
        }

        private void btnConnectDB_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Columns.Count == 0)
            {
                MessageBox.Show("Please create Table", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!string.IsNullOrEmpty(tbDataSource.Text) && !string.IsNullOrEmpty(tbInitialCatalog.Text) && !string.IsNullOrEmpty(tbUserID.Text) && !string.IsNullOrEmpty(tbPassDB.Text))
            {
                try
                {
                    if (btnConnectDB.Text == "Connect")
                    {
                        connectDB = new ConnectDB();
                        string connectionString = $"Data Source = {tbDataSource.Text}; Initial Catalog = {tbInitialCatalog.Text}; User ID = {tbUserID.Text}; Password ={tbPassDB.Text}";
                        connectDB.connect(connectionString);
                        if (connectDB.SQLIsConnect)
                        {
                            btnConnectDB.Text = "Disconnect";
                            btnSetDataFormat.Enabled = true;
                        }
                        else
                        {
                            btnConnectDB.Text = "Connect";
                            MessageBox.Show("Can not connect SQL Server", "Error Conection", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        connectDB.disconnect();
                        connectDB = null;
                        btnConnectDB.Text = "Connect";
                        btnSetDataFormat.Enabled = false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Connection string is worng : {ex.Message}", "Error Conection string", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Parameter invalid", "Error Conection", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void btnSetDataFormat_Click(object sender, EventArgs e)
        {
            formatFr = new dataFormatFr(ref dataGridView1);
            formatFr.ShowDialog();
            if (formatFr.DialogResult == DialogResult.OK)
            {
                commandSQL = formatFr.cmdSQL;
                indexColumnSQL = new List<int>(formatFr.indexList.Count);
                for (int i = 0; i < formatFr.indexList.Count; i++)
                {
                    indexColumnSQL.Add(formatFr.indexList[i]);
                }
            }
        }
    }
}
