using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.ApplicationServices;
using System.Reflection;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace MEM_AlbañileriaDiseño
{
    [Transaction(TransactionMode.Manual)]
    public class Class1 : IExternalCommand
    {
        //static public EventHandlerWithArgs eve = new EventHandlerWithArgs();
        //static public EventHandlerWithArgsDuplicateSheets sheets = new EventHandlerWithArgsDuplicateSheets();
        static public EventHandlerWithArgsAddNotaClave addNotaClave = new EventHandlerWithArgsAddNotaClave();
        static public EventHandlerWithArgsCreateFillRegion CreatefillR = new EventHandlerWithArgsCreateFillRegion();
        static public EventHandlerWithArgsModelFilledRegion createFillM = new EventHandlerWithArgsModelFilledRegion();
        static public EventHandlerWithArgsCreateDimensions createDim = new EventHandlerWithArgsCreateDimensions(); 

        //static public EventHandlerWithArgsCreateTextNote textNoteCreatorNC = new EventHandlerWithArgsCreateTextNote();
        static public EventHandlerWithArgsCreateTextNote textNoteCreator = new EventHandlerWithArgsCreateTextNote();

        public Result Execute(ExternalCommandData datosDelComandoExterno, ref string mensaje, ElementSet conjuntoDeElementos)
        {
            //Fecha Final de uso del plugin
            DateTime endTime = new DateTime(2023, 06, 30);
            if (DateTime.Compare(endTime, DateTime.Today) >= 0)
            {
                UIApplication aplicaciónDeLaIU = datosDelComandoExterno.Application;
                UIDocument documentoDeLaIU = aplicaciónDeLaIU.ActiveUIDocument;
                Autodesk.Revit.ApplicationServices.Application aplicación = aplicaciónDeLaIU.Application;
                Document documento = documentoDeLaIU.Document;



                VentanaInicial form = new VentanaInicial(aplicaciónDeLaIU);
                form.Show();


            }
            else
            {
                MessageBox.Show("Este plugin ha agotado su funcionamiento, para poder seguir usando la versión actual contacte con WWW.BIMSCAN.ES a través de info@bimscan.es");
            }
            return Result.Succeeded;
        }


    }
}
