using System;
using System.Reflection;
using Autodesk.Revit.UI;

namespace Cad2Revit.Core
{
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                string assemblyPath = Assembly.GetExecutingAssembly().Location;
                RibbonPanel panel = application.CreateRibbonPanel("CAD to Revit");

                PushButtonData buttonData = new PushButtonData(
                    "Cad2RevitCommand",
                    "CAD to Revit 3D",
                    assemblyPath,
                    "Cad2Revit.Core.Command");

                panel.AddItem(buttonData);
                return Result.Succeeded;
            }
            catch (Exception)
            {
                return Result.Failed;
            }
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}
