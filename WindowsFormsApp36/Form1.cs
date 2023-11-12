using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.Management;

namespace WindowsFormsApp36
{
    public partial class Form1 : Form
    {
        private TreeView processTreeView;
        private TextBox processInfoTextBox;
        private Process selectedProcess;
        public Form1()
        {
            InitializeComponents();
            PopulateProcessTree();
        }
        private void InitializeComponents()
        {
            // Настройка основного окна и элементов управления
            this.Text = "Process Tree Viewer";
            this.Size = new System.Drawing.Size(600, 400);

            processTreeView = new TreeView();
            processTreeView.Size = new System.Drawing.Size(300, 400);
            processTreeView.AfterSelect += ProcessTreeView_AfterSelect;

            processInfoTextBox = new TextBox();
            processInfoTextBox.Multiline = true;
            processInfoTextBox.ReadOnly = true;
            processInfoTextBox.ScrollBars = ScrollBars.Vertical;
            processInfoTextBox.Size = new System.Drawing.Size(280, 200);
            processInfoTextBox.Location = new System.Drawing.Point(320, 10);

            this.Controls.Add(processTreeView);
            this.Controls.Add(processInfoTextBox);
        }

        private void PopulateProcessTree()
        {
            Thread processThread = new Thread(() =>
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Process");
                foreach (ManagementObject managementObject in searcher.Get())
                {
                    int parentId = Convert.ToInt32(managementObject["ParentProcessId"]);
                    if (parentId == 0)
                    {
                        int pid = Convert.ToInt32(managementObject["ProcessId"]);
                        Process process = Process.GetProcessById(pid);

                        TreeNode node = new TreeNode(process.ProcessName);
                        node.Tag = process;

                        // Используем Invoke для добавления узла в дерево
                        AddNodeToTreeView(node);
                        AddChildProcesses(pid, node);
                    }
                }
            });
            processThread.Start();
        }

        private void AddChildProcesses(int parentProcessId, TreeNode parentNode)
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Process WHERE ParentProcessId=" + parentProcessId);
            foreach (ManagementObject managementObject in searcher.Get())
            {
                int pid = Convert.ToInt32(managementObject["ProcessId"]);
                Process process = Process.GetProcessById(pid);

                TreeNode node = new TreeNode(process.ProcessName);
                node.Tag = process;

                // Используем Invoke для добавления дочернего узла в дерево
                AddNodeToTreeView(node, parentNode);
                AddChildProcesses(pid, node);
            }
        }

        private void AddNodeToTreeView(TreeNode node, TreeNode parentNode = null)
        {
            if (parentNode == null)
            {
                if (processTreeView.InvokeRequired)
                {
                    processTreeView.Invoke((MethodInvoker)delegate {
                        processTreeView.Nodes.Add(node);
                    });
                }
                else
                {
                    processTreeView.Nodes.Add(node);
                }
            }
            else
            {
                if (parentNode.TreeView.InvokeRequired)
                {
                    parentNode.TreeView.Invoke((MethodInvoker)delegate {
                        parentNode.Nodes.Add(node);
                    });
                }
                else
                {
                    parentNode.Nodes.Add(node);
                }
            }
        }

        private void ProcessTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            selectedProcess = (Process)e.Node.Tag;
            if (selectedProcess != null)
            {
                string processInfo = $"Process Name: {selectedProcess.ProcessName}\r\n" +
                                     $"PID: {selectedProcess.Id}\r\n" +
                                     $"Memory Usage: {selectedProcess.WorkingSet64} bytes\r\n" +
                                     $"Start Time: {selectedProcess.StartTime}\r\n" +
                                     $"Total Processor Time: {selectedProcess.TotalProcessorTime}\r\n";

                processInfoTextBox.Text = processInfo;
            }
        }
    }
}
