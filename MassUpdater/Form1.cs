using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.ServiceProcess;
using System.DirectoryServices;
using Microsoft.Web.Administration;
using System.IO.Compression;


namespace MassUpdater
{

    public partial class Form1 : Form
    {
        ServiceController service = new ServiceController();
        ServerManager server = new ServerManager();

        // FORM CONSTRUCTOR____________________________________________________________________________    
        public Form1()
        {
            InitializeComponent();
            PopulateTreeView();
            PopulateServiceList();
            PopulateAppPoolList();
            PopulateSiteList();

            this.treeView1.NodeMouseClick +=
            new TreeNodeMouseClickEventHandler(this.treeView1_NodeMouseClick);
        }
        // POPULATE TREEVIEW1 W/ NODES_________________________________________________________________
        private void PopulateTreeView()
        {
            TreeNode rootNode;

            DirectoryInfo info = new DirectoryInfo(@"C:\Users\njerz\Documents\Catalyst");
            if (info.Exists)
            {
                rootNode = new TreeNode(info.Name);
                rootNode.Tag = info;
                GetDirectories(info.GetDirectories(), rootNode);
                treeView1.Nodes.Add(rootNode);
            }
        }
        // BUILD DIRECTORY HIERARCHY___________________________________________________________________
        private void GetDirectories(DirectoryInfo[] subDirs,
   TreeNode nodeToAddTo)
        {
            TreeNode aNode;
            DirectoryInfo[] subSubDirs;
            foreach (DirectoryInfo subDir in subDirs)
            {
                aNode = new TreeNode(subDir.Name, 0, 0);
                aNode.Tag = subDir;
                aNode.ImageKey = "folder";
                subSubDirs = subDir.GetDirectories();
                if (subSubDirs.Length != 0)
                {
                    GetDirectories(subSubDirs, aNode);
                }
                nodeToAddTo.Nodes.Add(aNode);
            }
        }
        // TREEVIEW1 CLICK TO FILL LISTVIEW1____________________________________________________________        
        void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {

            #region add these code
            uplevel = "";
            TreeNode newSelected1 = e.Node;
            sub = this.CheckParent(newSelected1);
            textBox1.Text = sub;
            #endregion    

            TreeNode newSelected = e.Node;
            listView1.Items.Clear();
            DirectoryInfo nodeDirInfo = (DirectoryInfo)newSelected.Tag;
            ListViewItem.ListViewSubItem[] subItems;
            ListViewItem item = null;

            foreach (DirectoryInfo dir in nodeDirInfo.GetDirectories())
            {
                item = new ListViewItem(dir.Name, 0);
                subItems = new ListViewItem.ListViewSubItem[]
                          {new ListViewItem.ListViewSubItem(item, "Directory"),
                   new ListViewItem.ListViewSubItem(item,
                dir.LastAccessTime.ToShortDateString())};
                item.SubItems.AddRange(subItems);
                listView1.Items.Add(item);
            }
            foreach (FileInfo file in nodeDirInfo.GetFiles())
            {
                item = new ListViewItem(file.Name, 1);
                subItems = new ListViewItem.ListViewSubItem[]
                          { new ListViewItem.ListViewSubItem(item, "File"),
                   new ListViewItem.ListViewSubItem(item,
                file.LastAccessTime.ToShortDateString())};

                item.SubItems.AddRange(subItems);
                listView1.Items.Add(item);
            }

            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }
        // EXECUTE BUTTON___________________________________________________________________________
        private void button1_Click(object sender, EventArgs e)
        {
            // StopServices();
            // StopAppPools();
            // StopSites();
            ExtractFiles();
        }

        // STOP SERVICES CHECKED ITEM LIST______________________________________________________________
        public void StopServices()
        {
            // Get Name of of Checked items & stop
            foreach (object itemChecked in checkedListBox1.CheckedItems)
            {
                service.ServiceName = itemChecked.ToString();

                if (service.Status == ServiceControllerStatus.Running)
                {
                    try
                    {
                        service.Stop();
                        service.WaitForStatus(ServiceControllerStatus.Stopped);
                    }
                    catch
                    {
                        MessageBox.Show("Could not stop " + service.ServiceName);
                    }
                }
                else
                {
                    MessageBox.Show(service.ServiceName + " is not running");
                }
            }
        }

        // STOP APPPOOLS CHECKED ITEM LIST________________________________________________________
        public void StopAppPools()
        {
            foreach (object itemChecked in checkedListBox2.CheckedItems)
            {
                string appName = itemChecked.ToString();
                var app = server.ApplicationPools.FirstOrDefault(s => s.Name == appName);
                app.Stop();
                if(app.State == ObjectState.Stopped)
                {
                    MessageBox.Show(appName+ " is stopped");
                }
                else
                {
                    throw new InvalidOperationException("Could not stop " + appName);
                }
            }
        }
        // STOP SITES CHECKED ITEM LIST______________________________________________________________
        public void StopSites()
        {
            foreach (object itemChecked in checkedListBox3.CheckedItems)
            {
                string siteName = itemChecked.ToString();
                var site = server.Sites.FirstOrDefault(s => s.Name == siteName);

                if (site != null)
                {
                    site.Stop();

                    if (site.State == ObjectState.Stopped)
                    {
                        MessageBox.Show(siteName + " is stopped");
                    }
                    else
                    {
                        throw new InvalidOperationException("Could not stop " + siteName);
                    }
                }
            }
        }

        // EXTRACT FILES_________________________________________________________________________________
        public void ExtractFiles()
        {
            string zipPath = @"C:\Users\njerz\Documents\Visual Studio 2015\Projects\MassUpdater\MassUpdater";
            string extractPath = @"C:\Users\njerz\Desktop\extraction";

            ZipFile.ExtractToDirectory(zipPath, extractPath);
        }


        // FILL TEXTBOX1 W/ DIR___________________________________________________________________________
        private string path = "C:\\"; //your file path root
        private string uplevel; // parent path
        private static string sub; // parent full path

        private string CheckParent(TreeNode child)
        {
            if (child.Parent != null)
            {
                TreeNode temp = child.Parent;
                uplevel = this.CheckParent(temp) + @"\" + child.Text;
            }
            else
            {
                uplevel = child.Text;
            }
            return uplevel;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string fileName = listView1.FocusedItem.Text;
            FileInfo f = new FileInfo(fileName);
            String fullName = f.FullName;
            textBox1.Text = fullName;
            
        }

        // POPULATE SERVICES CHECKLIST___________________________________________________________________________

        private void PopulateServiceList()
        {
            System.ServiceProcess.ServiceController[] services;
            services = System.ServiceProcess.ServiceController.GetServices();
            checkedListBox1.Items.Clear();
            for (int i = 0; i < services.Length; i++)
            {
                checkedListBox1.Items.Add(services[i].ServiceName);
            }
        }

        // POPULATE APP POOLS CHECKLIST__________________________________________________________________________

        private void PopulateAppPoolList()
        {
            ApplicationPoolCollection applicationPools = server.ApplicationPools;
            foreach (ApplicationPool pool in applicationPools)
            {
                string appPoolName = pool.Name;
                checkedListBox2.Items.Add(appPoolName);
            }
        }

        // POPULATE SITES CHECKLIST__________________________________________________________________________

        private void PopulateSiteList()
        {
            SiteCollection sites = server.Sites;
            foreach (Site site in sites)
            {
                string siteName = site.Name;
                checkedListBox3.Items.Add(siteName);
            }
        }

    } //END FORM

} // END NAMESPACE
