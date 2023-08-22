using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;

using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Runtime.Remoting.Contexts;
using System.Drawing.Text;
using System.Security.Cryptography.X509Certificates;
using MEM_AlbañileriaDiseño2020.Properties;
using System.IO;
using static System.Net.WebRequestMethods;
using System.Xml.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Collections;

namespace MEM_AlbañileriaDiseño2020
{
   

    public partial class VentanaInicial : System.Windows.Forms.Form
    {
        UIApplication uiapp;
        Document doc;
        ICollection<TipoMuro> tipos = new List<TipoMuro>();
        ICollection<TipoMuro> sortedTipos = new List<TipoMuro>();
        string TipoDeSort = "Lista";//parametro para definir la forma en que se ordenan los muros

        public VentanaInicial( UIApplication ui)
        {
            uiapp = ui;
            doc = uiapp.ActiveUIDocument.Document;
            InitializeComponent();
        }
       
        private void VentanaInicial_Load(object sender, EventArgs e)
        {
            MainComments.Text = "";
            //relleno la info en vistas
            button7.Select();
            checkBoxKeyNote.Enabled = true;
            labelparamOrMatt.Text = "Parametros de muros";
            comboBoxParametros.Enabled = false;

            //Añado los datos del ComboBox De Materiales
            List<string> list = new List<string>();
            list.Add("Comentario"); list.Add("Descripción"); list.Add("Nota Clave"); list.Add("Marca");
            foreach (string p in list)
            {
                comboBoxMaterialParam.Items.Add(p);
                comboBoxMaterialDescripción.Items.Add(p);
            }
            comboBoxMaterialParam.SelectedItem = comboBoxMaterialParam.Items[0];
            comboBoxMaterialDescripción.SelectedItem = comboBoxMaterialDescripción.Items[1];



            #region texnotes
            ICollection<Element> notes = new FilteredElementCollector(doc)
                .OfClass(typeof(TextNoteType)).ToElements();

            foreach (Element v in notes)
            {
                comboBoxtextNotes.Items.Add(v.Name.ToString());
                comboBoxTextoNotaClave.Items.Add(v.Name.ToString());    
            }
            comboBoxtextNotes.SelectedItem = comboBoxtextNotes.Items[0];
            comboBoxTextoNotaClave.SelectedItem = comboBoxTextoNotaClave.Items[0];
            ICollection<Element> Ewalls = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Walls).WhereElementIsNotElementType().ToElements();
            #endregion
            #region plantillas
            updateComboTemplates();

            #endregion

            //Busco los tipos unicos
            ICollection<Element> walls = new List<Element>();
            List<string> ides = new List<string>();
            foreach (Element Ew in Ewalls)
            {
                Element tipoAc= doc.GetElement(Ew.GetTypeId());
                if (ides.Contains(tipoAc.Id.ToString())==false)
                {
                    //WallType wallT = Ew as WallType;
                    //if (wallT.get_Parameter(BuiltInParameter.ALL_MODEL_MANUFACTURER).AsString() != "0")
                    //{
                    ides.Add(tipoAc.Id.ToString());
                    walls.Add(tipoAc);
                    //}
                    
                }
                
            }
            //añado los parámetros a el listado de parámetros de muros

            Element wall1=Ewalls.First<Element>();
            //IList<Parameter> Instanceparms =wall1.GetOrderedParameters();
            ElementId wt = wall1.GetTypeId();
            IList<Parameter> wt2 = doc.GetElement(wt).GetOrderedParameters();
            
            IList<string> nombresParam = new List<string>();
            foreach (Parameter p1 in wt2) 
            { 
                comboBoxParametros.Items.Add(p1.Definition.Name);
                comboBoxParametroTitulo.Items.Add(p1.Definition.Name);
                nombresParam.Add(p1.Definition.Name);
            }
            System.Windows.Forms.ComboBox.ObjectCollection items = comboBoxParametroTitulo.Items;
            for(int i =0;i<items.Count;i++) { if (items[i].ToString() =="Nota clave" || items[i].ToString() == "Keynote") { comboBoxParametroTitulo.SelectedIndex = i; }; }

            ////Añado el listado de materiales a su combo
            //Material mat1 = new FilteredElementCollector(doc)
            //    .OfClass(typeof(Material)).ElementAt(0) as Material;

            //IList<Parameter> paramsMat = doc.GetElement(mat1.Id).GetOrderedParameters();
            //foreach (Parameter p in paramsMat)
            //{
            //    comboBoxParamMaterials.Items.Add(p.Definition.Name);
            //    if (p.Definition.Name =="Description" ||  p.Definition.Name == "Descripción")
            //    {
            //        int index = comboBoxParamMaterials.Items.IndexOf(p.Definition.Name);
            //        comboBoxParamMaterials.SelectedIndex = index;
            //    }
            //}


            //CREO TODOS LOS TIPOS DE MUROS Y SUS MATERIALES
            foreach (Element ele in walls)
            {
                TipoMuro tipoM = new TipoMuro();
                //MUROS BÁSICOS

                string tipodemuro = ele.get_Parameter(BuiltInParameter.ALL_MODEL_FAMILY_NAME).AsString();
                tipoM.familyName = tipodemuro;

                #region basicWall
                if (tipodemuro == "Basic Wall" || tipodemuro == "Muro básico")
                {
                    WallType wall = ele as WallType;
                    //textBox1.Text = textBox1.Text + ele.Name.ToString();

                    tipoM.nombreTipo = wall.Name;
                    tipoM.funcion = wall.get_Parameter(BuiltInParameter.FUNCTION_PARAM).AsValueString();
                    tipoM.anchuraTipo = wall.get_Parameter(BuiltInParameter.WALL_ATTR_WIDTH_PARAM).AsDouble();
                    tipoM.idTipo = wall.Id;

                    
                    

                    //ANTER DE ARRANCAR LA APLICACIÓN ALMACENO EL VALOR DE KEYNOTE YA QUE AÚN NO NO HA PODIDO SELECCIONAR NINGUNO EL USUARIO
                    tipoM.notaClave =wall.get_Parameter(BuiltInParameter.KEYNOTE_PARAM).AsString();

                    if (tipoM.notaClave == null)
                    {
                        tipoM.notaClave = "No existe nota clave";
                    }

                    try
                    {
                        tipoM.fueraDeLeyenda = wall.get_Parameter(BuiltInParameter.ALL_MODEL_MANUFACTURER).AsString();
                    }
                    catch { }


                    try
                    {
                        Parameter comentariosdeTipo = wall.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS);

                        tipoM.comentariosTipo = comentariosdeTipo.AsString();
                    }
                    catch { }


                    #region Materiales
                    ICollection<material> mats = new List<material>() { };
                    try
                    {
                        IList<CompoundStructureLayer> layers = wall.GetCompoundStructure().GetLayers();

                        BuiltInParameter builtCode = BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS;
                        BuiltInParameter builtDesc = BuiltInParameter.ALL_MODEL_DESCRIPTION;

                        string selcodes = comboBoxMaterialParam.SelectedItem.ToString();

                        string seldescription = comboBoxMaterialDescripción.SelectedItem.ToString();

                        if (selcodes == "Nota Clave") { builtCode = BuiltInParameter.KEYNOTE_PARAM; }
                        if (selcodes == "Marca") { builtCode = BuiltInParameter.ALL_MODEL_MARK; }
                        if (selcodes == "Descripción") { builtCode = BuiltInParameter.ALL_MODEL_DESCRIPTION; }

                        if (seldescription == "Nota Clave") { builtDesc = BuiltInParameter.KEYNOTE_PARAM; }
                        if (seldescription == "Marca") { builtDesc = BuiltInParameter.ALL_MODEL_MARK; }
                        if (seldescription == "Descripción") { builtDesc = BuiltInParameter.ALL_MODEL_DESCRIPTION; }

                        foreach (CompoundStructureLayer Clayer in layers)
                        {
                            try
                            {
                                Material material = doc.GetElement(Clayer.MaterialId) as Material;
                                //textBox1.Text = textBox1.Text + " - Mat: " + material.Name.ToString();
                                material mat = new material();
                                mat.anchuraCapa = Clayer.Width;
                                mat.codigo = material.get_Parameter(builtCode).AsString();
                                mat.descripcion = material.get_Parameter(builtDesc).AsString();
                                mat.nombre = material.Name;

                                Autodesk.Revit.DB.Color color = material.Color;
                                mat.R = color.Red;
                                mat.G = color.Green;
                                mat.B = color.Blue;

                                //SI ALGUN CODIGO ESTÁ EN BLANCO NO SE PUEDE RENOMBRAR
                                if (mat.codigo == "")
                                {
                                    tipoM.editable = false;
                                    tipoM.errormessage = "Comentario de material no existe: " + mat.nombre;
                                }

                                //ahora añado el material a la lista
                                mats.Add(mat);
                            }
                            catch
                            {
                                tipoM.editable = false;
                                tipoM.errormessage = "Material no asignado correctamente";

                            }

                        }
                        tipoM.materialesMuro = mats;
                        tipoM.nombreModif = tipoM.addNombreBasicW(tipoM.materialesMuro);
                        if (tipoM.fueraDeLeyenda != "0")
                        {
                            tipos.Add(tipoM);
                        }
                        
                    }
                    catch
                    {

                    }
                    #endregion
                }
                #endregion


                #region Curtain wall
                //if (tipodemuro == "Curtain Wall" || tipodemuro == "Muro cortina")
                //{
                //    WallType wall = ele as WallType;
                //    //textBox1.Text = textBox1.Text + ele.Name.ToString();

                //    tipoM.nombreTipo = wall.Name;
                //    tipoM.funcion = wall.get_Parameter(BuiltInParameter.FUNCTION_PARAM).AsValueString();
                //    tipoM.JustificationHori = wall.get_Parameter(BuiltInParameter.SPACING_LAYOUT_HORIZ).AsValueString();
                //    tipoM.JustificationVert = wall.get_Parameter(BuiltInParameter.SPACING_LAYOUT_VERT).AsValueString();
                //    tipoM.spacingHori = wall.get_Parameter(BuiltInParameter.SPACING_LENGTH_HORIZ).AsDouble() / 0.0328084;
                //    tipoM.spacingVert = wall.get_Parameter(BuiltInParameter.SPACING_LENGTH_VERT).AsDouble() / 0.0328084;

                //    tipoM.panelTypeId = wall.get_Parameter(BuiltInParameter.AUTO_PANEL_WALL).AsElementId();



                //    tipoM.idTipo = wall.Id;



                //    try
                //    {
                //        tipoM.panelTypeComment = doc.GetElement(tipoM.panelTypeId).get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS).AsString();
                //        tipoM.editable = true;
                //    }

                //    catch { tipoM.editable = false; tipoM.errormessage = tipoM.errormessage + "No hay comentario de panel"; }

                //    try
                //    {
                //        tipoM.editable = true;
                //        tipoM.nombreModif = tipoM.addNombreCurtainW();

                //    }
                //    catch { tipoM.editable = false; tipoM.errormessage = tipoM.errormessage + "No hay comentario de panel"; }
                //    tipos.Add(tipoM);
                //}
                #endregion

            }

            #region creo los patrones que me hacen falta
            ICollection<Element> regions = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_DetailComponents).WhereElementIsElementType().ToElements(); //RegionTypes Existentes
            string nombres = "";

            #region Genero la lista base para el orden de los muros
            ICollection<string> baseCodigos = new List<string>();
            baseCodigos.Add("APPIE20"); baseCodigos.Add("LCV92"); baseCodigos.Add("PHP10"); baseCodigos.Add("MM20");
            baseCodigos.Add("MCH15"); baseCodigos.Add("LHOP120"); baseCodigos.Add("HA150"); baseCodigos.Add("PM48(AT50)");
            baseCodigos.Add("PM70(AT50)"); baseCodigos.Add("ALIC20"); baseCodigos.Add("CM3"); baseCodigos.Add("EY15"); baseCodigos.Add("YL15");
            baseCodigos.Add("YL18"); baseCodigos.Add("YLWA15"); baseCodigos.Add("YL13"); baseCodigos.Add("LHOP240"); baseCodigos.Add("HA300");
            baseCodigos.Add("LHS40"); baseCodigos.Add("TCH15"); baseCodigos.Add("TCH30"); baseCodigos.Add("AT80"); baseCodigos.Add("VP5");
            baseCodigos.Add("CA");
            textBox1.Text = "";
            foreach (string c in baseCodigos)
            {
                textBox1.Text = textBox1.Text + c + Environment.NewLine;
            }

            #endregion fin de lista de materiales

            //Hago lista de materiales únicos en muros
            ICollection<material> matsMuros = new List<material>();

            sortedTipos = reorganizarMurosPorCodigos(tipos, baseCodigos);//Antes de entrar los ordeno con la funcion lo uso para tenerlo ordenados


            foreach (TipoMuro t in sortedTipos)
            {
                try
                {
                    foreach (material m in t.materialesMuro)
                    {
                        string nombre = m.nombre;
                        //recorro la lista para saber si ya lo he incluido
                        bool exist = false;
                        foreach (material mat in matsMuros)
                        {
                            if (mat.nombre == nombre) { exist = true; }
                        }
                        if (exist == false) { matsMuros.Add(m); }
                    }
                }
                catch
                {

                }
                
                
            }
            
            
            ElementId typeFillRegionToduplicate = null;
            ICollection<material> filledQueNecesitoCrear = new List<material>();//lista de filledRegions que necesito crear
            foreach (material m in matsMuros)
            {

                string fillMatName = "FILLMAT_" + m.nombre;

                //recorro los materiales para ver si coincide el nombre con el nombre del FilledRegion
                bool existFilled = false;
                foreach (Element ele in regions)
                {
                    string familyName = ele.get_Parameter(BuiltInParameter.ALL_MODEL_FAMILY_NAME).AsString();

                    if (familyName == "Filled region" || familyName == "Región rellenada")
                    {
                        string fillTypename = ele.get_Parameter(BuiltInParameter.SYMBOL_NAME_PARAM).AsString();


                        if (fillMatName == fillTypename) { existFilled = true; }
                        typeFillRegionToduplicate = ele.Id;
                    }
                    
                }
                if (existFilled == false)
                {
                    filledQueNecesitoCrear.Add(m);
                    nombres += fillMatName + Environment.NewLine;
                }

            }

            if (filledQueNecesitoCrear.Count() > 0)
            {

                MessageBoxButtons botones = MessageBoxButtons.OKCancel;
                //DialogResult result = MessageBox.Show(nombres, "Faltan los siguientes regiones rellenadas", botones);
                DialogResult result = MessageBox.Show("¿Quiere crear las regiones rellenada que faltan?","Regiones Rellenadas", botones);
                if (result == DialogResult.OK)
                {
                    Class1.CreatefillR.Raise((filledQueNecesitoCrear, typeFillRegionToduplicate));
                }
                if (result == DialogResult.Cancel) { }
            }

            #endregion


            //ActualizarImagen();

        }

        private void buttonCrearRegiones_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBoxParametros.SelectedItem.ToString() != "" || comboBoxParametros.SelectedItem.ToString() != null) { TipoDeSort = "Parametro"; }
            }
            catch { }

            #region sobreescribo los keynotes si hace falta
           
                try
                {
                    string param = comboBoxParametroTitulo.SelectedItem.ToString();
                    foreach (TipoMuro t in tipos)
                    {
                        WallType wtype = doc.GetElement(t.idTipo) as WallType;
                        string paramSelected = comboBoxParametroTitulo.SelectedItem.ToString();
                        string valor = getParameterValueByName(wtype, paramSelected);
                        
                        t.notaClave = valor;
                    }
                }
                catch { }
            
            #endregion


            //#endregion  

            #region leer codigos de muros

            ICollection<string> leidos = new List<string>();
            ICollection<TipoMuro> ResortedTipos = new List<TipoMuro>();
            ICollection<ICollection<TipoMuro>> FilasDeMuros = new List<ICollection<TipoMuro>>();

            //listado de códigos
            string text = textBox1.Text;
            string[] textos = text.Split('\n');
            foreach (string t in textos)
            {
                string nT = t.Replace("\r", "");
                leidos.Add(nT);
            }


            if (TipoDeSort == "Lista")
            {
                ResortedTipos = reorganizarMurosPorCodigos(sortedTipos, leidos);
                FilasDeMuros.Add(ResortedTipos);
            }
            if (TipoDeSort == "Parametro")
            {
                FilasDeMuros = reorganizarMurosPorParametro(sortedTipos, comboBoxParametros.SelectedItem.ToString());
            }


            #endregion
            //Machacar nota clave
            if (checkBoxKeyNote.Checked)
            {
                Class1.addNotaClave.Raise(ResortedTipos);
            }
            

            #region listas iniciales
            ICollection<Element> notesF = new FilteredElementCollector(doc).OfClass(typeof(TextNoteType)).ToElements();
            TextNoteType noteTypeMaterials = null;
            TextNoteType noteTypeNotaClave = null;
            #endregion

            #region lectura de tipos de textos seleccionados
            if (comboBoxtextNotes.SelectedIndex >= 0 && comboBoxTextoNotaClave.SelectedIndex>=0)
            {
                foreach (TextNoteType n in notesF)
                {
                    if (n.Name == comboBoxtextNotes.SelectedItem.ToString()) { noteTypeMaterials = n; }
                    if (n.Name == comboBoxTextoNotaClave.SelectedItem.ToString()) { noteTypeNotaClave = n; }
                }
            }
            #endregion

            #region Compruebo que la vista activa es valida para dibujar los muros y los dibujo

            string tipoVistaActiva = doc.ActiveView.ViewType.ToString();
            Autodesk.Revit.DB.View v = doc.ActiveView;
            if (tipoVistaActiva != "DraftingView") { MessageBox.Show("La vista activa debe ser una vista de diseño, la vista actual es: " + tipoVistaActiva); }
            else
            {
                ICollection<Element> filledR = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_FilledRegion).WhereElementIsElementType().ToElements();
                ICollection<Element> regiones = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_DetailComponents).WhereElementIsElementType().ToElements(); //RegionTypes Existentesfgdhdhgh
                try
                {
                    ICollection<(ElementId, List<CurveLoop>,string)> patronesParaCrearFinal = new List<(ElementId, List<CurveLoop>,string)>();
                    ICollection<(XYZ,XYZ,string,bool,Element)> pointsAndTextos = new List<(XYZ,XYZ,string,bool,Element)>();
                    double alturaFila = 0;

                    
                    
                    foreach (ICollection<TipoMuro> col in FilasDeMuros)
                    {


                        string parameterTitle = comboBoxParametroTitulo.SelectedItem.ToString();
                        ICollection<string> parameterValues = getParameterValueByName(col, parameterTitle);
                        string HMuro1 = textHWalls.Text;
                        double HMuro = convertUnits(double.Parse(HMuro1,System.Globalization.CultureInfo.InvariantCulture),v);//con el invariant culture no hace cambios entre , y .
                        string HhNotaclave1 = textBoxHNotaClave.Text;
                        double hNotaclave = convertUnits(double.Parse(HhNotaclave1, System.Globalization.CultureInfo.InvariantCulture),v);
                        string DesfNotaClave1 = textBoxDesNotaClave.Text;
                        double DesfNotaClave = convertUnits(double.Parse(DesfNotaClave1, System.Globalization.CultureInfo.InvariantCulture),v);
                        string distText1 = textBoxDistText.Text;
                        double distText = convertUnits(double.Parse(distText1, System.Globalization.CultureInfo.InvariantCulture),v);
                        string distTextIni1 = textBoxDistInicioTextos.Text;
                        double distTextIni = convertUnits(double.Parse(distTextIni1, System.Globalization.CultureInfo.InvariantCulture),v);
                        double distanciaMuros = 0;
                        int countCapas = 0;
                        int countMuros = 0;
                        foreach (TipoMuro tm in col)
                        {
                      
                            //Solo trabajamos con muros básicos
                            if (tm.familyName == "Basic Wall" || tm.familyName == "Muro básico")
                            {

                                ElementId filledRewgionTypeID = null;
                                double alturamateriales = HMuro;
                                double anchuramaterial = 0;
                                
                                double distTextosFromTop = distText;
                                foreach (material m in tm.materialesMuro)
                                {
                                    string filledRegionName = "FILLMAT_" + m.nombre;

                                    foreach (Element ele in regiones)//busco los filledregions, el que se llame igual
                                    {
                                        string familyName = ele.get_Parameter(BuiltInParameter.ALL_MODEL_FAMILY_NAME).AsString();

                                        if (familyName == "Filled region" || familyName == "Región rellenada")
                                        {
                                            string fillTypename = ele.get_Parameter(BuiltInParameter.SYMBOL_NAME_PARAM).AsString();
                                            if (fillTypename == filledRegionName) { filledRewgionTypeID = ele.Id; }
                                        }
                                    }

                                    //Geometria de cada material

                                    if ((m.anchuraCapa / 0.0034) > 1) //si la capa mide mas de un mm
                                    {
                                        XYZ p0 = new XYZ(anchuramaterial + distanciaMuros, -alturaFila, 0);
                                        XYZ p1 = new XYZ(anchuramaterial + distanciaMuros, alturamateriales - alturaFila, 0);


                                        anchuramaterial += m.anchuraCapa;//lo meto en pies

                                        XYZ p2 = new XYZ(anchuramaterial + distanciaMuros, alturamateriales - alturaFila, 0);
                                        XYZ p3 = new XYZ(anchuramaterial + distanciaMuros, -alturaFila, 0);

                                        Curve c1 = Line.CreateBound(p0, p1);
                                        Curve c2 = Line.CreateBound(p1, p2);
                                        Curve c3 = Line.CreateBound(p2, p3);
                                        Curve c4 = Line.CreateBound(p3, p0);

                                        CurveLoop loop = new CurveLoop();
                                        loop.Append(c1); loop.Append(c2); loop.Append(c3); loop.Append(c4);
                                        List<CurveLoop> lista = new List<CurveLoop>();
                                        lista.Add(loop);
                                        patronesParaCrearFinal.Add((filledRewgionTypeID, lista, tm.nombreTipo));


                                        //Calculo los puntos de los textos


                                        XYZ pt0 = new XYZ(p1.X + (m.anchuraCapa / 2), p1.Y - distTextosFromTop, 0);
                                        XYZ pt1 = new XYZ(distanciaMuros + tm.anchuraTipo + distTextIni, p1.Y - distTextosFromTop, 0);
                                        distTextosFromTop = distTextosFromTop + distText;
                                        pointsAndTextos.Add((pt0, pt1, m.descripcion.ToString(), true, noteTypeMaterials));

                                    }


                                    if ((m.anchuraCapa / 0.0034) <= 1)//si la capa mide menos o igual a 1mm
                                    {
                                        XYZ p0 = new XYZ(anchuramaterial + distanciaMuros, 0, 0);
                                        XYZ p1 = new XYZ(anchuramaterial + distanciaMuros, alturamateriales, 0);

                                        XYZ pt0 = new XYZ(p1.X + (m.anchuraCapa / 2), p1.Y - distTextosFromTop, 0);
                                        XYZ pt1 = new XYZ(distanciaMuros + tm.anchuraTipo + distTextIni, p1.Y - distTextosFromTop, 0);
                                        distTextosFromTop = distTextosFromTop + distText;
                                        pointsAndTextos.Add((pt0, pt1, m.descripcion.ToString(), true, noteTypeMaterials));
                                    }

                                }
                                #region puntos y textos para las notas clave
                                ////creo los puntos y los textos para las notas de nota clave
                                if (countCapas >= 0 && checkBoxNotaClave.Checked)
                                {

                                    XYZ colocacionNC = new XYZ((tm.anchuraTipo / 2) + distanciaMuros + DesfNotaClave, HMuro + hNotaclave-alturaFila, 0);
                                    XYZ fakePoint = new XYZ(0, 0, 0);
                                    if (comboBoxParametroTitulo.SelectedItem == null)
                                    {
                                        pointsAndTextos.Add((fakePoint, colocacionNC, tm.notaClave, false, noteTypeNotaClave));
                                    }
                                    if (comboBoxParametroTitulo.SelectedItem != null)
                                    {
                                        pointsAndTextos.Add((fakePoint, colocacionNC, tm.notaClave, false, noteTypeNotaClave));
                                    }
                                }


                                string dist = textBoxDistWalls.Text;
                                double distW = convertUnits(Convert.ToDouble(dist),v);
                                distanciaMuros += distW;//* 0.0034
                                
                                #endregion

                            }

                            Class1.createFillM.Raise(patronesParaCrearFinal); //Se ejecuta para cada muro
                            
                        }
                        alturaFila = alturaFila + HMuro * 1.8;
                    }
                    
                    

                    Class1.textNoteCreator.Raise(pointsAndTextos);
                }
                catch (Exception er) { MessageBox.Show(er.Message); }
            }
            #endregion
        }

        public string getParameterValueByName(WallType w, string name)
        {
            string result = "";
            foreach (Parameter P in w.GetOrderedParameters())
            {
                if (P.Definition.Name == name) 
                {
                    if (P.AsValueString() != null) { result= P.AsValueString(); }
                    if (P.AsString() != null) { result = P.AsString(); }

                }
            }
            return result;
        }

        public ICollection<TipoMuro> reorganizarMurosPorCodigos(ICollection<TipoMuro> listOri,ICollection<string> baseCodigos)
        {
            List<TipoMuro> inte = new List<TipoMuro>();
            List<TipoMuro> exte = new List<TipoMuro>();
            List<TipoMuro> restF = new List<TipoMuro>();
            ICollection<TipoMuro> ret = new List<TipoMuro>();

            

            //ordeno primero por anchura creando un diccionario ordenandolo y pasandolo de nuevo a lista
            List<TipoMuro> tiposSort = new List<TipoMuro>();
            var dictionary = new Dictionary<TipoMuro, double>();
            foreach (TipoMuro tOri in listOri)
            {
                dictionary.Add(tOri, tOri.anchuraTipo);
            }

            var myList = dictionary.ToList();
            myList.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
           
            ICollection<TipoMuro> ordenada = new List<TipoMuro>();
            foreach (var i in myList)
            {
                ordenada.Add(i.Key);
            }


            //Primer Orden
            foreach (TipoMuro tm in ordenada)
            {
                if (tm.funcion == "Exterior") { exte.Add(tm); }
                if (tm.funcion != "Exterior") { inte.Add(tm); }
                
            }
            #region exteriores
            //Ordeno y añado los exteriores
            ICollection<TipoMuro> finalLista = new List<TipoMuro>();
            int countNota = 1;
            foreach (string actualE in baseCodigos)
            {
                try
                {
                    
                    foreach (TipoMuro tmE in exte)
                    {
                        //Recorro los materiales
                        int contadoir = 0;
                        foreach (material mE in tmE.materialesMuro)
                        {
                            if (mE.codigo == actualE) { contadoir = 1; }
                        }
                        if (contadoir == 1)
                        {
                            if (checkBoxKeyNote.Checked)
                            {
                                if (countNota < 10) { tmE.notaClave = "ME 0" + countNota.ToString(); }
                                if (countNota > 9) { tmE.notaClave = "ME " + countNota.ToString(); }
                            }
                            

                            finalLista.Add(tmE);
                            exte.Remove(tmE);
                            countNota = countNota + 1;
                        }                    
                    }             
                }
                catch { }               
            }
            //hago una segunda vuelta para aquellos que se han quedado sin añadir
            foreach (TipoMuro tmE2 in exte) 
            {
                if (checkBoxKeyNote.Checked)
                {
                    if (countNota < 10) { tmE2.notaClave = "ME 0" + countNota.ToString(); }
                    if (countNota > 9) { tmE2.notaClave = "ME " + countNota.ToString(); }
                }
                finalLista.Add(tmE2);
                countNota = countNota + 1;
            }
            #endregion

            //Ordeno y añado los interiores
            countNota = 1;
            foreach (string actualI in baseCodigos)
            {
                try
                {
                    foreach (TipoMuro tmI in inte)
                    {
                        //Recorro los materiales
                        int contadoir = 0;
                        foreach (material mI in tmI.materialesMuro)
                        {
                            if (mI.codigo == actualI) { contadoir = 1; }
                        }
                        if (contadoir == 1)
                        {
                            if (checkBoxKeyNote.Checked)
                            {
                                if (countNota < 10) { tmI.notaClave = "MI 0" + countNota.ToString(); }
                                if (countNota > 9) { tmI.notaClave = "MI " + countNota.ToString(); }
                            }

                            finalLista.Add(tmI);
                            inte.Remove(tmI);
                            countNota = countNota + 1;
                        }                          
                    } 
                }
                catch { }
            }
            // hago una segunda vuelta para aquellos que se han quedado sin añadir
            foreach (TipoMuro tmI2 in inte) 
            {
                if (checkBoxKeyNote.Checked)
                {
                    if (countNota < 10) { tmI2.notaClave = "MI 0" + countNota.ToString(); }
                    
                    if (countNota > 9) { tmI2.notaClave = "MI " + countNota.ToString(); }
                }

                finalLista.Add(tmI2);
                countNota = countNota + 1;
            }

            return finalLista;
        }
        public ICollection<ICollection<TipoMuro>> reorganizarMurosPorParametro(ICollection<TipoMuro> listOri, string paramName)
        {

            ICollection<string> valoresParam = new List<string>();
            ICollection<TipoMuro> ret = new List<TipoMuro>();
            ICollection<ICollection<TipoMuro>> solved = new List<ICollection<TipoMuro>>();
            

            //ordeno primero por anchura creando un diccionario ordenandolo y pasandolo de nuevo a lista
            List<TipoMuro> tiposSort = new List<TipoMuro>();
            var dictionary = new Dictionary<TipoMuro, double>();
            foreach (TipoMuro tOri in listOri)
            {
                dictionary.Add(tOri, tOri.anchuraTipo);
            }

            var myList = dictionary.ToList();
            myList.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
            ICollection<TipoMuro> ordenada = new List<TipoMuro>();
            foreach (var i in myList)
            {
                ordenada.Add(i.Key);
            }
            //OIbtengo los valores e parámetros y corrigo si está en null
            foreach (TipoMuro tmI in ordenada)
            {
                valoresParam.Add(doc.GetElement(tmI.idTipo).LookupParameter(paramName).AsValueString());
            }
            if (valoresParam.First<string>() == null)
            {
                valoresParam = new List<string>();
                foreach (TipoMuro tmI in ordenada)
                {
                    valoresParam.Add(doc.GetElement(tmI.idTipo).LookupParameter(paramName).AsString());
                }
            }
            //voy a agrupar los muros por el valor del parámetro
            ICollection<string> YaAgrupados = new List<string>();
            
            foreach (string valorParam in valoresParam)
            {
                ICollection<TipoMuro> actualGroup = new List<TipoMuro>();

                if (!YaAgrupados.Contains(valorParam))
                {
                    int count = 0;
                    YaAgrupados.Add(valorParam);
                    foreach (string actualST in valoresParam)
                    {
                        if (actualST == valorParam) { actualGroup.Add(ordenada.ElementAt<TipoMuro>(count)); }
                        count++;
                    }

                    //Voy a ordenar las sublistas dependiendo del código del material
                    ICollection<string> leidos = new List<string>();
                    //listado de códigos
                    string text = textBox1.Text;
                    string[] textos = text.Split('\n');
                    foreach (string t in textos)
                    {
                        string nT = t.Replace("\r", "");
                        leidos.Add(nT);
                    }

                    var dictionary2 = new Dictionary<TipoMuro, double>();
                    foreach (TipoMuro tOri2 in actualGroup)
                    {
                        dictionary2.Add(tOri2, materialTopIndex(tOri2, leidos));
                    }
                    var mySubList = dictionary2.ToList();
                    mySubList.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
                    ICollection<TipoMuro> ordenada2 = new List<TipoMuro>();
                    foreach (var i in mySubList)
                    {
                        ordenada2.Add(i.Key);
                    }


                    solved.Add(ordenada2);
                }
                


            }
            





            return solved;
        }
        public int materialTopIndex(TipoMuro tipo, ICollection<string> baseCodigos)
        {

            int valor = 100;
            foreach (material m in tipo.materialesMuro)
            {
                int indice = 0;
                
                foreach(string code in baseCodigos)
                {
                    if (m.codigo == code) { valor = indice; }
                    indice++;
                }
            }


            return valor;
        }

        public ICollection<string> getParameterValueByName(ICollection<TipoMuro> listOri,string paramName)
        {
            ICollection<string> strings = new List<string>();
            foreach (TipoMuro tm2 in listOri)
            {
                Parameter current = doc.GetElement(tm2.idTipo).LookupParameter(paramName);
                if (current.AsString() != "")
                {
                    strings.Add(current.AsString());
                }
                if(current.AsValueString()!="")
                {
                    strings.Add(current.AsValueString());
                }
                
            }

            return strings;
        }

        public double convertUnits(double d, Autodesk.Revit.DB.View v)
        {

            double shift = UnitUtils.ConvertToInternalUnits(d, DisplayUnitType.DUT_METERS); //v.Scale para modificar el valor según la escala (no lo entiendo aún)
            //Autodesk.Revit.DB.DisplayUnitType.DUT_MILLIMETERS para versiones 21 hacia atrás
            return shift;
        }
      

       

        

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            //try { ActualizarImagen(); } catch { }
        }

        private void groupBox4_Enter(object sender, EventArgs e)
        {

        }

        private void Acotar_Button(object sender, EventArgs e)
        {
            

            Autodesk.Revit.DB.View v = doc.ActiveView;
            IEnumerable<FilledRegion> fL= new FilteredElementCollector(doc).OwnedByView(v.Id).OfType<FilledRegion>();
            ICollection<ICollection<FilledRegion>> filled = new List<ICollection<FilledRegion>>();
            if (fL.Count<FilledRegion>() == 0) { MessageBox.Show("No existen regiones rellenadas en la vista"); }
            if (fL.Count<FilledRegion>() > 0)
            {
                #region creo el listado filled ara poder cosultarlo después con las regiones creadas
                string test = "";
                
                ICollection<FilledRegion> filledRef = new List<FilledRegion>();
                int count = 0;
                foreach (FilledRegion f in fL)
                {
                    string Tipo = f.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString();
                    if (test == Tipo)
                    {
                        filledRef.Add(f);
                    }
                    if (test != Tipo)
                    {
                        test = Tipo;
                        if (count == 0) { filledRef.Add(f); }
                        if (count == 1) { filled.Add(filledRef); filledRef = new List<FilledRegion>(); filledRef.Add(f); }
                        count = 1;

                    }
                }
                filled.Add(filledRef);
                #endregion


                Class1.createDim.Raise(filled);

            }

            
            
        }

        private void Parametros_Enter(object sender, EventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void label14_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void comboWallSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            //try { ActualizarImagen(); } catch { }
        }

        private void textBoxDistText_TextChanged(object sender, EventArgs e)
        {
            //try { ActualizarImagen(); } catch { }
        }

        private void textHWalls_TextChanged(object sender, EventArgs e)
        {
            //try { ActualizarImagen(); } catch { }
        }

        private void textBoxDistText_Enter(object sender, EventArgs e)
        {
            picturePreviaMuro.Image = Resources.BasicWallDistanceBtext;
        }

        private void textBoxDistText_Leave(object sender, EventArgs e)
        {
            picturePreviaMuro.Image = Resources.BasicWall;
        }

        private void textBoxDistInicioTextos_Enter(object sender, EventArgs e)
        {
            picturePreviaMuro.Image = Resources.BasicWallInitial_text_Dist;
        }

        private void textBoxDistInicioTextos_Leave(object sender, EventArgs e)
        {
            picturePreviaMuro.Image = Resources.BasicWall;
        }

        private void textBoxHNotaClave_Enter(object sender, EventArgs e)
        {
            picturePreviaMuro.Image = Resources.BasicWallNC_Heigthg;
        }

        private void textBoxHNotaClave_Leave(object sender, EventArgs e)
        {
            picturePreviaMuro.Image = Resources.BasicWall;
        }

        private void textBoxDesNotaClave_Enter(object sender, EventArgs e)
        {
            picturePreviaMuro.Image = Resources.BasicWallNC_Offset;
        }

        private void textBoxDesNotaClave_Leave(object sender, EventArgs e)
        {
            picturePreviaMuro.Image = Resources.BasicWall;
        }
        private void textBoxDistWalls_Enter(object sender, EventArgs e)
        {
            picturePreviaMuro.Image = Resources.BasicWallNC_Distance;
        }
        private void textBoxDistWalls_Leave(object sender, EventArgs e)
        {
            picturePreviaMuro.Image = Resources.BasicWall;
        }
        private void checkBoxNotaClave_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxNotaClave.Checked) { pictureBoxWhite.Visible = false; }
            if (!checkBoxNotaClave.Checked) 
            { 
                pictureBoxWhite.Visible = true;
                pictureBoxWhite.BringToFront();
            }
        }

        private void saveTemplateButton(object sender, EventArgs e)
        {
            string newName = "";
            string distWall = textBoxDistWalls.Text;
            string hwall = textHWalls.Text;
            string distTextos = textBoxDistText.Text;
            string distInitext = textBoxDistInicioTextos.Text;
            string hNotaClave = textBoxHNotaClave.Text;
            string offsetNotaClaveSTR = textBoxDesNotaClave.Text;
            bool textsup = checkBoxNotaClave.Checked;
            string listaGrupos = textBox1.Text;

            TemplateName tn = new TemplateName();
            tn.ShowDialog();
            
            newName=tn.nombre;
            List<string> temp = new List<string>();
            if (newName != "")
            {
                temp.Add(newName);
                temp.Add(comboBoxtextNotes.SelectedItem.ToString());
                temp.Add(comboBoxTextoNotaClave.SelectedItem.ToString());
                temp.Add(comboBoxParametroTitulo.SelectedItem.ToString());
                temp.Add(comboBoxMaterialDescripción.SelectedItem.ToString());
                temp.Add(comboBoxMaterialParam.SelectedItem.ToString());
                temp.Add(distWall);
                temp.Add(hwall);
                temp.Add(distTextos);
                temp.Add(distInitext);
                temp.Add(hNotaClave);
                temp.Add(offsetNotaClaveSTR);
                temp.Add(textsup.ToString());
                foreach (string valor in listaGrupos.Split('\n'))
                {
                    int length = valor.Length;
                    temp.Add(valor.Split('\r')[0]);
                }

                addTemplate(temp);
            }

            updateComboTemplates();

        }
        private ICollection<List<string>> readTemplates()
        {
            ICollection<List<string>> valores = new List<List<string>>(); 
            string url = @"C:\\ProgramData\\Autodesk\\MEM_Template.mt";
            try
            {
                bool exist = System.IO.File.Exists(url);
                if (exist==false) 
                {
                    System.IO.FileStream f= System.IO.File.Create(url);
                    f.Close();
                }

            }
            catch (Exception e) { MessageBox.Show(e.Message, "Creación de archivo de plantillas"); }
            StreamReader sr = new StreamReader(url);
            string[] lineas = sr.ReadToEnd().Split('\r');
            foreach (string l in lineas)
            {
                if (l.Split(';').Count<string>() > 0)
                {
                    List<string> values = l.Split(';').ToList<string>();
                    valores.Add(values);
                }
                
            }
            sr.Close();
            return valores;
        }
        private void addTemplate(List<string> template)
        {
            string url = @"C:\\ProgramData\\Autodesk\\MEM_Template.mt";
            StreamWriter sw = new StreamWriter(url,true); //el true es para añadir a la lista es para hacer un append
            
            string[] lin = template.ToArray();
            string actual = "";
            for (int i = 0; i < template.Count<string>(); i++)
            {
                if (i != template.Count<string>() - 1) { actual = actual + template[i] + ";"; }
                else { actual = actual + template[i]; }
            }

            sw.WriteLine(actual);
            sw.Close();
            StreamReader sr = new StreamReader(url);

            #region limpio el documento
            ICollection<string> values = new List<string>();
            List<string> lineas = sr.ReadToEnd().Split('\r').ToList<string>();
            foreach (string line in lineas)
            {
                if (line != "") 
                {
                    values.Add(line);
                }
            }
            sr.Close ();
            System.IO.File.Create(url).Close();
            StreamWriter sw2 = new StreamWriter(url, true);

            foreach(string l in values)
            {
                sw2.WriteLine(l);
            }

            #endregion
            sw2.Close();


        }
        private void renameTemplate(string newName, string oldName)
        {
            ICollection<List<string>> valores = new List<List<string>>();
            string url = @"C:\\ProgramData\\Autodesk\\MEM_Template.mt";
            StreamReader sr = new StreamReader(url);
            List<string> lineas = sr.ReadToEnd().Split('\r').ToList<string>();
            foreach (string l in lineas)
            {
                List<string> values = l.Split(';').ToList<string>();
                if (values[0] == oldName) { values[0] = newName; valores.Add(values); }
                else { valores.Add(values); }
                valores.Add(values);
            }
            overwrite(valores);
        }
        private void overwrite(ICollection<List<string>> valores)
        {
            string url = @"C:\\ProgramData\\Autodesk\\MEM_Template.mt";
            StreamWriter sw = new StreamWriter(url);
            sw.Write(String.Empty);
            sw.Close();
            StreamWriter sw2 = new StreamWriter(url,true);
            foreach (List<string> linea in valores)
            {
                string actual = "";
                for (int i=0;i< linea.Count<string>();i++)
                {
                    if(i != linea.Count<string>() - 1) { actual=actual+linea[i]+";"; }
                    else { actual=actual+linea[i]; }
                }
                sw2.WriteLine(actual);
            }
            
            sw2.Close();
            updateComboTemplates();
        }
        
        private void updateComboTemplates()
        {
            ICollection<List<string>> list = readTemplates();
            comboBoxTemplates.Items.Clear();
            foreach (List<string> l in list)
            {
                if (l.Count>1)
                {
                    comboBoxTemplates.Items.Add(l[0]);
                }
                
            }
        }

        private void removeLine(string valor)
        {
            ICollection<List<string>> valores = readTemplates();
            ICollection <List<string>>  nuevoValores = new List<List<string>>();
            foreach (List<string> l in valores)
            {
                if (l[0] != valor)
                {
                    nuevoValores.Add(l);
                }
            }
            overwrite(nuevoValores);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            removeLine(comboBoxTemplates.SelectedItem.ToString());
        }        

        private void label13_Click_1(object sender, EventArgs e)
        {

        }

        private void label13_Click_2(object sender, EventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {
           
            comboBoxParametros.Enabled = false;
            checkBoxKeyNote.Enabled = true;
            button7.ForeColor = System.Drawing.Color.AliceBlue;
            button8.ForeColor = System.Drawing.Color.Transparent;


        }

        private void button8_Click(object sender, EventArgs e)
        {
            
            comboBoxParametros.Enabled = true;
            checkBoxNotaClave.Checked = false;
            checkBoxKeyNote.Enabled = false;
            button8.ForeColor = System.Drawing.Color.AliceBlue;
            button7.ForeColor = System.Drawing.Color.Transparent;


        }

        private void button9_Apply(object sender, EventArgs e)
        {
            try
            {
                string plantilla = comboBoxTemplates.SelectedItem.ToString();
                string url = @"C:\\ProgramData\\Autodesk\\MEM_Template.mt";
                ICollection<List<string>> lista = readTemplates();
                foreach (List<string> l in lista)
                {
                    if (l[0] == plantilla)
                    {
                        replaceData(l);
                    }
                }
            }
            catch { }
        }

        private void replaceData(List<string> l)
        {
            try
            {
                int indexTex = 0;
                foreach (string s in comboBoxtextNotes.Items)
                {

                    if (s == l[1]) { comboBoxtextNotes.SelectedIndex = indexTex; }
                    indexTex++;
                }
                int indexTexTittle = 0;
                foreach (string s in comboBoxTextoNotaClave.Items)
                {

                    if (s == l[2]) { comboBoxTextoNotaClave.SelectedIndex = indexTexTittle; }
                    indexTexTittle++;
                }
                int indexParamTittle = 0;
                foreach (string s in comboBoxParametroTitulo.Items)
                {

                    if (s == l[3]) { comboBoxParametroTitulo.SelectedIndex = indexParamTittle; }
                    indexParamTittle++;
                }
                int indexParamDescrition = 0;
                foreach (string s in comboBoxMaterialDescripción.Items)
                {

                    if (s == l[4]) { comboBoxMaterialDescripción.SelectedIndex = indexParamDescrition; }
                    indexParamDescrition++;
                }
                int indexParamCode = 0;
                foreach (string s in comboBoxMaterialParam.Items)
                {

                    if (s == l[5]) { comboBoxMaterialParam.SelectedIndex = indexParamCode; }
                    indexParamCode++;
                }
                textBoxDistWalls.Text= l[6];
                textHWalls.Text= l[7];
                textBoxDistText.Text= l[8];
                textBoxDistInicioTextos.Text= l[9];
                textBoxHNotaClave.Text= l[10];
                textBoxDesNotaClave.Text= l[11];
                textBox1.Text= "";
                for (int i = 13;i<l.Count;i++) 
                {
                    textBox1.Text+= l[i]+Environment.NewLine;
                }
            }
            catch { }

        }

        private void label13_MouseHover(object sender, EventArgs e) { MainComments.Text = "Parámetro del material que servirá de descripción para las etiquetas de muros"; }

        private void label13_MouseLeave(object sender, EventArgs e){MainComments.Text = "";}

        private void label14_MouseHover(object sender, EventArgs e){MainComments.Text = "Parámetro del material que servirá de código para la agrupación de muros";}

        private void label14_MouseLeave(object sender, EventArgs e) { MainComments.Text = ""; }

        private void groupBox2_MouseHover(object sender, EventArgs e) { MainComments.Text = "Tipo de etiqueta existente en el proyecto que se aplicará para los títulos de los muros"; }

        private void groupBox2_Leave(object sender, EventArgs e) { MainComments.Text = ""; }
        private void groupBox1_MouseHover(object sender, EventArgs e) { MainComments.Text = "Tipo de etiqueta existente en el proyecto que se aplicará para los materiales"; }

        private void groupBox1_Leave(object sender, EventArgs e) { MainComments.Text = ""; }

        private void checkBoxNotaClave_MouseHover(object sender, EventArgs e) { MainComments.Text = "Check para definir si queremos incluir o no un texto encima de cada muro"; }

        private void checkBoxNotaClave_MouseLeave(object sender, EventArgs e) { MainComments.Text = ""; }

        private void comboBoxParametroTitulo_MouseHover(object sender, EventArgs e) { MainComments.Text = "Parámetro que se va a definir como título de muro"; }
        private void comboBoxParametroTitulo_MouseLeave(object sender, EventArgs e) { MainComments.Text = ""; }

        private void label2_MouseHover(object sender, EventArgs e) 
        { 
            MainComments.Text = "Distancia en metros para definir la separación horizontal entre muros";
            picturePreviaMuro.Image = Resources.BasicWallNC_Distance;
        }

        private void label2_MouseLeave(object sender, EventArgs e) 
        { 
            MainComments.Text = "";
            picturePreviaMuro.Image = Resources.BasicWall;
        }

        private void label3_MouseHover(object sender, EventArgs e)
        {
            MainComments.Text = "Distancia en metros para definir la altura de los muros";
            picturePreviaMuro.Image = Resources.BasicWallNC_Heigthg;
        }

        private void label3_MouseLeave(object sender, EventArgs e)
        {
            MainComments.Text = "";
            picturePreviaMuro.Image = Resources.BasicWall;
        }

        private void label5_MouseHover(object sender, EventArgs e)
        {
            MainComments.Text = "Distancia en metros para definir la separación vertical entre los textos de los parámetros";
            picturePreviaMuro.Image = Resources.BasicWallDistanceBtext;
        }

        private void label5_MouseLeave(object sender, EventArgs e)
        {
            MainComments.Text = "";
            picturePreviaMuro.Image = Resources.BasicWall;

        }

        private void label7_MouseHover(object sender, EventArgs e)
        {
            MainComments.Text = "Distancia en metros para definir la separación horizontal entre el muro y los textos de los parámetros";
            picturePreviaMuro.Image = Resources.BasicWallInitial_text_Dist;
        }

        private void label7_MouseLeave(object sender, EventArgs e)
        {
            MainComments.Text = "";
            picturePreviaMuro.Image = Resources.BasicWall;
        }

        private void label9_MouseHover(object sender, EventArgs e)
        {
            MainComments.Text = "Distancia en metros para definir la separación vertical entre el muro y el texto del título";
            picturePreviaMuro.Image = Resources.BasicWallNC_Heigthg;
        }

        private void label9_MouseLeave(object sender, EventArgs e)
        {
            MainComments.Text = "";
            picturePreviaMuro.Image = Resources.BasicWall;
        }

        private void label12_MouseHover(object sender, EventArgs e)
        {
            MainComments.Text = "Distancia en metros para definir el desfase horizontal entre el texto del título y el eje del muro";
            picturePreviaMuro.Image = Resources.BasicWallNC_Offset;
        }

        private void label12_MouseLeave(object sender, EventArgs e)
        {
            MainComments.Text = "";
            picturePreviaMuro.Image = Resources.BasicWall;
        }

        private void button7_MouseHover(object sender, EventArgs e)
        {
            MainComments.Text = "Opción A: Agrupación de muros en una sola línea basado en la función del muro. " +
                "Se añaden primero los muros exteriores y posteriormente los interiores, finalmente el resto." +
                "Dentro de cada agrupación los muros se ordenan dependiendo de los materiales que contienen " +
                "y de su anchura.";
            picturePreviaMuro.Image = Resources.One_Line;
        }

        private void button7_MouseLeave(object sender, EventArgs e)
        {
            MainComments.Text = "";
            picturePreviaMuro.Image = Resources.BasicWall;

        }

        private void checkBoxKeyNote_MouseHover(object sender, EventArgs e)
        {
            MainComments.Text = "Check para sobreescribir o no el parámetro nota clave en los muros según el orden definido según la opción seleccionada";
        }

        private void checkBoxKeyNote_MouseLeave(object sender, EventArgs e)
        {
            MainComments.Text = "";
        }

        private void button8_MouseHover(object sender, EventArgs e)
        {
            MainComments.Text = "Opción B: Agrupación de muros en diferentes lineas dependiendo del valor en el parámetro seleccionado. " +
                            "Dentro de cada agrupación los muros se ordenan dependiendo de los materiales que contienen " +
                            "y de su anchura.";
            picturePreviaMuro.Image = Resources.Multi_Line;
            picturePreviaMuro.BringToFront();
        }

        private void button8_MouseLeave(object sender, EventArgs e)
        {
            MainComments.Text = "";
            picturePreviaMuro.Image = Resources.BasicWall;
            pictureBoxWhite.BringToFront();

        }

        private void comboBoxParametros_MouseHover(object sender, EventArgs e)
        {
            MainComments.Text = "Parámetro de tipo que se usará para la agrupación de los muros en la opción 2.";

        }

        private void comboBoxParametros_MouseLeave(object sender, EventArgs e)
        {
            MainComments.Text = "";
        }

        private void groupBox5_MouseHover(object sender, EventArgs e)
        {
            MainComments.Text = "Sistema de gestión de plantillas que sirve para almecenar todos los parámetros recogidos en la ventana.";

        }

        private void groupBox5_Leave(object sender, EventArgs e)
        {
            MainComments.Text = "";

        }

        private void comboBoxTemplates_MouseHover(object sender, EventArgs e)
        {
            MainComments.Text = "Sistema de gestión de plantillas que sirve para almecenar todos los parámetros recogidos en la ventana.";

        }

        private void comboBoxTemplates_MouseLeave(object sender, EventArgs e)
        {
            MainComments.Text = "";

        }

        private void button3_MouseHover(object sender, EventArgs e)
        {
            MainComments.Text = "Sistema de gestión de plantillas que sirve para almecenar todos los parámetros recogidos en la ventana.";

        }

        private void button3_MouseLeave(object sender, EventArgs e)
        {
            MainComments.Text = "";

        }

        private void button4_MouseHover(object sender, EventArgs e)
        {
            MainComments.Text = "Sistema de gestión de plantillas que sirve para almecenar todos los parámetros recogidos en la ventana.";

        }

        private void button4_MouseLeave(object sender, EventArgs e)
        {
            MainComments.Text = "";

        }

        private void button9_MouseHover(object sender, EventArgs e)
        {
            MainComments.Text = "Sistema de gestión de plantillas que sirve para almecenar todos los parámetros recogidos en la ventana.";

        }

        private void button9_MouseLeave(object sender, EventArgs e)
        {
            MainComments.Text = "";

        }
    }

    public class TipoMuro
    {
        public ICollection<material> materialesMuro { get; set; } //lista de materiales
        public double anchuraTipo { get; set; }//Anchura total del muro
        public string funcion { get; set; }//función del muro

        public string nombreTipo { get; set; }//Nombre del tipo actual sin modificar

        public bool editable = true;//el muro tienen un nombre generable o no?
        public string errormessage { get; set; }//mensaje de error generado desde ejecución

        public string nombreModif { get; set; }//Nuevo nombre creado

        public string comentariosTipo { get; set; }

        public ElementId idTipo { get; set; }
        public string familyName { get; set; }
        public string notaClave { get; set; }
        public string fueraDeLeyenda { get; set; }
        public string títuloSeleccionado { get; set; }

        //Muros cortina valores extra

        public double spacingVert { get; set; }//cm
        public double spacingHori { get; set; }//cm
        public string JustificationVert { get; set; }
        public string JustificationHori { get; set; }
        public string panelName { get; set; }
        public ElementId panelTypeId { get; set; }
        public string panelTypeComment { get; set; }
        

      


        public string addNombreBasicW(ICollection<material> mats)
        {
            string finalName = "NombreCalculado";
            finalName = this.funcion.ToString().Substring(0, 3).ToUpper();



            foreach (material mat in mats)
            {
                finalName = finalName + "_" + mat.codigo;
            }


            string comentarios = "";
            try { comentarios = "_" + this.comentariosTipo.ToString(); } catch { }
            finalName = finalName + "_" + Math.Round(this.anchuraTipo, 2).ToString() + "cm" + comentarios;

            return finalName;
        }
        public string addNombreCurtainW()
        {
            string finalName = "NombreCalculado";
            finalName = this.funcion.ToString().Substring(0, 3).ToUpper();

            string justV = ""; string justH = "";
            //Distancia fija, Número fijo, Espaciado máximo, Espaciado mínimo
            //Fixed Distance, Fixed Number, Maximum Spacing, Minimum Spacing
            if (JustificationVert == "Ninguno" || JustificationVert == "None") justV = "XXX";
            if (JustificationVert == "Distancia fija" || JustificationVert == "Fixed Distance") justV = "DFI";
            if (JustificationVert == "Número fijo" || JustificationVert == "Fixed Number") justV = "NFI";
            if (JustificationVert == "Espaciado máximo" || JustificationVert == "Maximum Spacing") justV = "EMA";
            if (JustificationVert == "Espaciado mínimo" || JustificationVert == "Minimum Spacing") justV = "EMI";

            if (JustificationVert == "Ninguno" || JustificationVert == "None") justH = "XXX";
            if (JustificationHori == "Distancia fija" || JustificationHori == "Fixed Distance") justH = "DFI";
            if (JustificationHori == "Número fijo" || JustificationHori == "Fixed Number") justH = "NFI";
            if (JustificationHori == "Espaciado máximo" || JustificationHori == "Maximum Spacing") justH = "EMA";
            if (JustificationHori == "Espaciado mínimo" || JustificationHori == "Minimum Spacing") justH = "EMI";



            finalName = finalName + "_" + justV + "_" + Math.Round(this.spacingVert, 2).ToString()
                + "_" + justH + "_" + Math.Round(this.spacingHori, 2).ToString();

            try
            {
                string comentT = this.panelTypeComment;
                if (comentT != null) { finalName = finalName + "_Panel_" + comentT; }
            }
            catch { }


            try
            {
                string commentT = this.comentariosTipo.ToString();
                if (commentT != null) { finalName = finalName + "_" + commentT; }
            }
            catch { }

            return finalName;
        }
    }
    public class material
    {
        public string nombre { get; set; }
        public string codigo { get; set; }
        public double anchuraCapa { get; set; }
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }
        public string descripcion { get; set; }

    }
  
}
