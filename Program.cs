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
            parseSettings("settings.xml", ref m_utils);
            Util selectedUtility = findUtility(ref m_utils, args[0]);
            if (selectedUtility == null)
            {
                System.Console.WriteLine("ERROR: No utility registered for this file type.");
                return -1;
            }

            string arguments;
            if (args.Length == 2)
                arguments = getDiffCommand(selectedUtility, args);
            else
                arguments = getMergeCommand(selectedUtility, args);

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = selectedUtility.Exe;
            startInfo.Arguments = arguments;

            try
            {
                System.Console.WriteLine("Using utility " + selectedUtility.Name);
                process.StartInfo = startInfo;
                process.Start();
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
        private static Util findUtility(ref List<Util> p_utils, string filename)
        {
            string ext = Path.GetExtension(filename);
            foreach (Util utility in p_utils)
            {
                if (utility.Extensions.Contains(ext))
                    return utility;
            }
            return null;
        }

        /// <summary>
        /// 
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
            return string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_filename"></param>
        /// <param name="p_utils"></param>
        private static void parseSettings(string p_filename, ref List<Util> p_utils)
        {
            XmlDocument settings = new XmlDocument();
            settings.Load(p_filename);

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
                        }
                        p_utils.Add(tmp);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    class Util
    {
        public Util()
        {
            m_extensions = new List<string>();
        }

        private string m_name;
        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        private string m_exe;
        public string Exe
        {
            get { return m_exe; }
            set { m_exe = value; }
        }

        private List<string> m_extensions;
        public List<string> Extensions
        {
            get { return m_extensions; }
            set { m_extensions = value; }
        }

    }
}
