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
        /// 
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
        /// 
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
            return " " + p_args[0] + " " + p_args[1];
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
            string command = string.Empty;

            foreach (string str in p_args)
            {
                if (str.CompareTo(p_selectedUtil.BaseSwitch) == 0)
                    command += " " + str + " " + p_args[index + 1];
                else if (str.CompareTo(p_selectedUtil.OutputSwitch) == 0)
                    command += " " + str + " " + p_args[index + 1];
                else if (str.CompareTo(p_selectedUtil.MergeSwitch) == 0)
                    command += " " + str + " " + p_args[index + 1];
                else if (str.Contains(p_selectedUtil.ExtraOptions))
                    command += " " + str;
                
                index++;
            }

            return command;
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
                            if (node4.Name.CompareTo("merge") == 0)
                                tmp.MergeSwitch = node4.InnerText;
                            if (node4.Name.CompareTo("base") == 0)
                                tmp.BaseSwitch = node4.InnerText;
                            if (node4.Name.CompareTo("output") == 0)
                                tmp.OutputSwitch = node4.InnerText;
                            if (node4.Name.CompareTo("extraoptions") == 0)
                                tmp.ExtraOptions = node4.InnerText;
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
        private string m_extra;
        private string m_baseSwitch;
        private string m_outputSwitch;
        private List<string> m_extensions;

        public Util()
        {
            m_extensions = new List<string>();
        }

        private string m_mergeSwitch;

        /// <summary>
        /// Swtich for merge input file.
        /// </summary>
        public string MergeSwitch
        {
            get { return m_mergeSwitch; }
            set { m_mergeSwitch = value; }
        }        

        /// <summary>
        /// Swtich for base file.
        /// </summary>
        public string BaseSwitch
        {
            get { return m_baseSwitch; }
            set { m_baseSwitch = value; }
        }       

        /// <summary>
        /// Switch for output file.
        /// </summary>
        public string OutputSwitch
        {
            get { return m_outputSwitch; }
            set { m_outputSwitch = value; }
        }        

        /// <summary>
        /// Extra command line switches for utility.
        /// </summary>
        public string ExtraOptions
        {
            get { return m_extra; }
            set { m_extra = value; }
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
