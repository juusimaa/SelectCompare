using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

namespace SelectCompare
{
    class Program
    {
        /// <summary>
        /// Usage: SelectCompare.exe "$BASE" "$LOCAL" "$REMOTE" "$MERGED"
        /// </summary>
        /// <param name="args"></param>
        static int Main(string[] args)
        {
            List<Util> m_utils = new List<Util>();
            parseSettings(AppDomain.CurrentDomain.BaseDirectory + "\\settings.xml", ref m_utils);
            if (m_utils == null)
            {
                System.Console.WriteLine("ERROR: Parsing 'settings.xml' failed.");
                return -1;
            }

            Util selectedUtility = findUtility(ref m_utils, args);
            if (selectedUtility == null)
            {
                System.Console.WriteLine("ERROR: No utility registered for this file type.");
                return -1;
            }

            string arguments;

            // if only two arguments, assume diff
            if (args.Length == 2)
                arguments = getDiffCommand(selectedUtility, args);
            else
                arguments = getMergeCommand(selectedUtility, args);

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.FileName = selectedUtility.Exe;
            startInfo.Arguments = arguments;

            try
            {
                System.Console.WriteLine("Using utility " + selectedUtility.Name);
                System.Console.WriteLine("Command: " + startInfo.FileName + startInfo.Arguments);
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();
            }
            catch (ArgumentNullException)
            {
                System.Console.WriteLine("ERROR: Invalid argument(" + arguments + ")");
                return -1;
            }
            catch (InvalidOperationException)
            {
                System.Console.WriteLine("ERROR: Invalid operation(" + selectedUtility.Exe + " " + arguments + ")");
                return -1;
            }

            return 0;
        }

        /// <summary>
        /// Try to find correct utility based on file extension. If utility is not
        /// found returns default utility.
        /// </summary>
        /// <param name="p_utils"></param>
        /// <param name="p_extension"></param>
        /// <returns></returns>
        private static Util findUtility(ref List<Util> p_utils, string[] p_args)
        {
            foreach (string argument in p_args)
            {
                string currentExt = Path.GetExtension(argument);
                foreach (Util utility in p_utils)
                {
                    foreach (string ext in utility.Extensions)
                    {
                        if (currentExt.CompareTo(ext) == 0)
                            return utility;
                    }
                }
            }

            foreach (Util u in p_utils)
            {
                if (u.Default == true)
                    return u;
            }
            return null;
        }

        /// <summary>
        /// Operation to get diff command line options for selected utility.
        /// </summary>
        /// <param name="p_selectedUtil"></param>
        /// <param name="p_args"></param>
        /// <returns></returns>
        private static string getDiffCommand(Util p_selectedUtil, string[] p_args)
        {
            int index = 0;
            p_selectedUtil.LOCAL = p_args[index++];
            p_selectedUtil.REMOTE = p_args[index++];

            return p_selectedUtil.GetDiffCommand();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_selectedUtil"></param>
        /// <param name="p_args"></param>
        /// <returns></returns>
        private static string getMergeCommand(Util p_selectedUtil, string[] p_args)
        {
            int index = 0;
            p_selectedUtil.BASE = p_args[index++];
            p_selectedUtil.LOCAL = p_args[index++];
            p_selectedUtil.REMOTE = p_args[index++];
            p_selectedUtil.MERGED = p_args[index++];

            return p_selectedUtil.GetMergeCommand();
        }

        /// <summary>
        /// Operation to parse 'settings.xml' file.
        /// </summary>
        /// <param name="p_filename"></param>
        /// <param name="p_utils"></param>
        private static void parseSettings(string p_filename, ref List<Util> p_utils)
        {
            XmlDocument settings = new XmlDocument();

            try
            {
                settings.Load(p_filename);
            }
            catch (XmlException)
            {
                p_utils = null;
            }

            foreach (XmlNode node1 in settings)
            {
                foreach (XmlNode node2 in node1.ChildNodes)
                {
                    foreach (XmlNode node3 in node2.ChildNodes)
                    {
                        Util tmp = new Util();
                        tmp.Name = node3.Attributes.Item(0).Value;
                        foreach (XmlNode node4 in node3.ChildNodes)
                        {
                            if (node4.Name.CompareTo("executable") == 0)
                                tmp.Exe = node4.InnerText;
                            if (node4.Name.CompareTo("ext") == 0)
                                tmp.Extensions.Add(node4.InnerText);
                            if (node4.Name.CompareTo("mergeCommand") == 0)
                                tmp.MergeTemplate = node4.InnerText;
                            if (node4.Name.CompareTo("diffCommand") == 0)
                                tmp.DiffTemplate = node4.InnerText;
                            if (node4.Name.CompareTo("default") == 0)
                                tmp.Default = Convert.ToBoolean(node4.InnerText);
                        }
                        p_utils.Add(tmp);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Class for holding compare/merge utility parameters.
    /// </summary>
    class Util
    {
        private string m_name;
        private string m_exe;
        private string m_mergedFile;
        private string m_remoteFile;
        private string m_localFile;
        private string m_baseFile;
        private string m_mergeTemplate;
        private string m_diffTemplate;
        private bool m_default;
        private List<string> m_extensions;

        public Util()
        {
            m_extensions = new List<string>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetMergeCommand()
        {
            string command = m_mergeTemplate;
            command = command.Replace("$BASE", m_baseFile);
            command = command.Replace("$LOCAL", m_localFile);
            command = command.Replace("$REMOTE", m_remoteFile);
            command = command.Replace("$MERGED", m_mergedFile);

            return command;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetDiffCommand()
        {
            string command = m_diffTemplate;
            command = command.Replace("$LOCAL", m_localFile);
            command = command.Replace("$REMOTE", m_remoteFile);

            return command;
        }        

        /// <summary>
        /// 
        /// </summary>
        public bool Default
        {
            get { return m_default; }
            set { m_default = value; }
        }
                
        /// <summary>
        /// 
        /// </summary>
        public string MergeTemplate
        {
            get { return m_mergeTemplate; }
            set { m_mergeTemplate = value; }
        }
                
        /// <summary>
        /// 
        /// </summary>
        public string DiffTemplate
        {
            get { return m_diffTemplate; }
            set { m_diffTemplate = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string MERGED
        {
            get { return m_mergedFile; }
            set { m_mergedFile = value; }
        }       

        /// <summary>
        /// 
        /// </summary>
        public string REMOTE
        {
            get { return m_remoteFile; }
            set { m_remoteFile = value; }
        }        

        /// <summary>
        /// 
        /// </summary>
        public string LOCAL
        {
            get { return m_localFile; }
            set { m_localFile = value; }
        }        

        /// <summary>
        /// 
        /// </summary>
        public string BASE
        {
            get { return m_baseFile; }
            set { m_baseFile = value; }
        }
              	
        /// <summary>
        /// Utility name.
        /// </summary>
        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        /// <summary>
        /// Full path to executable.
        /// </summary>
        public string Exe
        {
            get { return m_exe; }
            set { m_exe = value; }
        }

        /// <summary>
        /// Extension supported.
        /// </summary>
        public List<string> Extensions
        {
            get { return m_extensions; }
            set { m_extensions = value; }
        }
    }
}
