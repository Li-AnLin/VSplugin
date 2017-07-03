//------------------------------------------------------------------------------
// <copyright file="Command.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Globalization;

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using EnvDTE;

namespace plugin_for_vs
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class Command
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("171a677f-e4b7-43f4-a381-034dcc08f87f");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="Command"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private Command(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static Command Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new Command(package);
        }

        /// <summary>
        /// Show error message.
        /// </summary>
        /// <param name="message"></param>
        private void ShowMessage(string message)
        {
            string title = "Error Message";

            // Show a message box
            VsShellUtilities.ShowMessageBox(
                this.ServiceProvider,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        private void LoopProjects(Array projects)
        {
            string message = "";
            //loop project
            foreach (Project p in projects)
            {
                message = p.Name + " : \n";
                //loop folder in project
                foreach (ProjectItem item in p.ProjectItems)
                {
                    message += (item.Name + " have " + item.FileCount + "\n");

                    if (item.ProjectItems.Count > 0)
                    {
                        //loop file in folder
                        foreach (ProjectItem i in item.ProjectItems)
                        {
                            message += (i.Name + " : " + i.Document.Path + "\n");
                        }
                    }
                }
                ShowMessage(message);
            }
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            string message = "";
            // dte maybe means "Development Tools Environment"
            DTE dte = this.ServiceProvider.GetService(typeof(DTE)) as DTE;   //get dte

            var projects = (Array)dte.ActiveSolutionProjects;    //get the current project in the solution

            ShowMessage(projects.Length.ToString());

            if(projects.Length == 0)
            {
                message = "ERROR: You need to select the project.";
                ShowMessage(message);
                return;
            }

            LoopProjects(projects);

            dte.Solution.SolutionBuild.Clean(true);
            dte.Solution.SolutionBuild.Build(true);
            
            
            //server端?
            System.Diagnostics.Process process = new System.Diagnostics.Process();

            process.StartInfo.FileName = dte.Name + ".exe";
            process.StartInfo.RedirectStandardInput = true;

        }
    }
}
