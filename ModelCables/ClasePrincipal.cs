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
using static System.Net.WebRequestMethods;
//using PresentationCore;

namespace MEM_AlbañileriaDiseño2020
{
    class ClassPrincipal : IExternalApplication
    {
        //CREACION DEL PANEL

        const string tabName = "BIMSCAN";




        const string panelName = "Albañilerías";


        public Result OnStartup(UIControlledApplication application)
        {

            //creo la cinta
            try
            {
                application.CreateRibbonTab(tabName);
            }
            catch (Exception) { }//miramos a ver si existe ya 

            //Voy a buscar si existe el panel 

            RibbonPanel panel = null;
            List<RibbonPanel> panels = application.GetRibbonPanels(tabName);
            foreach (RibbonPanel pnl in panels)
            {
                if (pnl.Name == panelName)
                {
                    panel = pnl;
                    break;
                }
            }
            //Creamos el panel si no existe ya
            if (panel == null)
            {
                panel = application.CreateRibbonPanel(tabName, panelName);

                string direcciónDeEnsamblado = Assembly.GetExecutingAssembly().Location;
            }

            //busco la imagen para el boton



            System.Drawing.Image img = MEM_AlbañileriaDiseño2020.Properties.Resources.Ladrillo100x100;
            BitmapSource imgSrc = GetImageSource(img);

            string label = "Memoría de " + Environment.NewLine+ "Albañilería";
            string description = "Creación de memoria de Albañilería en vista de diseño usando " +
                "los materiales de los muros. Para ello generará lo patrones de relleno que sean " +
                "necesarios para la representación de los muros en vista de sección. El color de los patrones de " +
                "relleno dependerá de los colores asignados a los materiales.";

            //Creo los datos del boton 
            PushButtonData botonDatosModeladoGeneral = new PushButtonData("internalButtonMEM_AlbañileriaDiseño2020", label, Assembly.GetExecutingAssembly().Location, "MEM_AlbañileriaDiseño2020.Class1");


            botonDatosModeladoGeneral.LargeImage = imgSrc;


            PushButton botonPulsadoModeladoGeneral = panel.AddItem(botonDatosModeladoGeneral) as PushButton;
            botonPulsadoModeladoGeneral.Enabled = true;

            botonPulsadoModeladoGeneral.LongDescription = description;

            return Result.Succeeded;
        }


        //metodo para convertir la imagen de resources a bitmapImage
        private BitmapSource GetImageSource(System.Drawing.Image imp)
        {
            BitmapImage bmp = new BitmapImage();
            using (MemoryStream ms = new MemoryStream())
            {
                imp.Save(ms, ImageFormat.Png);
                ms.Position = 0;

                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.UriSource = null;
                bmp.StreamSource = ms;
                bmp.EndInit();

            }
            return bmp;
        }


        public Result OnShutdown(UIControlledApplication aplicación)
        {
            //apagado no hace nada

            return Result.Succeeded;

        }
       

    }

    
}
