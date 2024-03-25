using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
//using BB_GlobalVariables;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using View = Autodesk.Revit.DB.View;

namespace MEM_AlbañileriaDiseño2020
{
    abstract public class ExternalEventMy<T> : IExternalEventHandler
    {

        

        private object @lock;
        private T savedArgs;
        public static ExternalEvent revitEvent;

        public ExternalEventMy()
        {
            revitEvent = ExternalEvent.Create(this);
            @lock = new object();
        }

        public void Execute(UIApplication uiApp)
        {
            T args;

            lock (@lock)
            {
                args = savedArgs;
                savedArgs = default(T);
            }

            Execute(uiApp, args);
        }
        public string GetName()
        {
            return "my event";
        }
        public void Raise(T args)
        {
            lock (@lock)
            {
                savedArgs = args;
            }

            revitEvent.Raise();
        }
        abstract public void Execute(UIApplication app, T args);
    }



    public class EventHandlerWithArgsAddNotaClave : ExternalEventMy<ICollection<TipoMuro>>
    {
        public string time { get; set; }
        public string report { get; set; }

        public List<string> elementIDS { get; set; }


        public FamilySymbol symbolF { get; set; }




        public override void Execute(UIApplication uiApp, ICollection<TipoMuro> tiposModif)
        {

            Autodesk.Revit.DB.Document doc = uiApp.ActiveUIDocument.Document;


            

            //results.Text = fam.Name;

            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Sobreescribir Nota Clave");
                List<string> listaids = new List<string>() { };

                try
                {

                    //PRIMERO RECORRO TODOS Y PONGO LA NOTA CLAVE A " "

                    ICollection<Element> muros = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Walls).WhereElementIsElementType().ToElements();

                    foreach (Element element in muros)
                    {
                        element.get_Parameter(BuiltInParameter.KEYNOTE_PARAM).Set(" ");
                    }


                    foreach (TipoMuro tipo in tiposModif)
                    {
                        WallType tipoActual = doc.GetElement(tipo.idTipo) as WallType;

                       
                        foreach (Parameter p in tipoActual.Parameters)
                        {
                            if (p.Definition.Name=="Nota clave" || p.Definition.Name == "Keynote" || p.Definition.Name=="Key note") 
                            {
                                string nc = tipo.notaClave.ToString();
                                p.Set(nc);
                            }
                        }


                        


                        try { string valor = tipoActual.LookupParameter("Keynote").AsValueString(); }
                        catch { }
                        try { string valor = tipoActual.LookupParameter("Notaclave").AsValueString(); }
                        catch { }

                        //string valor2 = tipoActual.get_Parameter(BuiltInParameter.KEYNOTE_TEXT).AsValueString();
                        //ElementType actualWall = doc.GetElement(tipo.idTipo) as ElementType;

                        //actualWall.Name = tipo.nombreModif;

                    }


                }
                catch (Exception args)
                {
                    MessageBox.Show(args.ToString());
                }


                tx.Commit();
            }


        }

    }



    public class EventHandlerWithArgsCreateFillRegion : ExternalEventMy<(ICollection<material>, ElementId)>
    {
        public string time { get; set; }
        public string report { get; set; }

        public List<string> elementIDS { get; set; }


        public FamilySymbol symbolF { get; set; }


        public override void Execute(UIApplication uiApp, (ICollection<material>, ElementId) valor)
        {

            Autodesk.Revit.DB.Document doc = uiApp.ActiveUIDocument.Document;

            FilledRegionType typeToDuplicate = doc.GetElement(valor.Item2) as FilledRegionType;

            //miro los tipos de regiuones y me quedo con la que tengo relleno solido
            ICollection<Element> regions = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_DetailComponents).WhereElementIsElementType().ToElements(); //RegionTypes Existentes

            ElementId idSolid = null;
            foreach (Element p in regions)
            {
                try
                {
                    string familyName = p.get_Parameter(BuiltInParameter.ALL_MODEL_FAMILY_NAME).AsString();

                    if (familyName == "Filled region" || familyName == "Región rellenada")
                    {
                        FilledRegionType f = p as FilledRegionType;
                        string fillName = doc.GetElement(f.ForegroundPatternId).Name;

                        //MessageBox.Show(fillName);
                        if (fillName == "<Solid fill>" || fillName == "<Relleno uniforme>") { idSolid = f.ForegroundPatternId; }
                    }
                }
               catch (Exception ex) { }

            }

            //results.Text = fam.Name;

            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Create FillRegions");
                List<string> listaids = new List<string>() { };

                FilteredElementCollector elements = new FilteredElementCollector(doc);
                FillPatternElement solidFillPattern = elements.OfClass(typeof(FillPatternElement)).Cast<FillPatternElement>().First(a => a.GetFillPattern().IsSolidFill);


                try
                {
                    foreach (material m in valor.Item1)
                    {
                        string fillName = "FILLMAT_" + m.nombre;
                        FilledRegionType actualFill = typeToDuplicate.Duplicate(fillName) as FilledRegionType;
                        actualFill.ForegroundPatternId = solidFillPattern.Id;
                        
                        actualFill.ForegroundPatternColor = new Autodesk.Revit.DB.Color(Convert.ToByte(m.R), Convert.ToByte(m.G), Convert.ToByte(m.B));



                    }


                }
                catch (Exception args)
                {
                    //MessageBox.Show(args.ToString());
                }


                tx.Commit();
            }


        }

    }

    public class EventHandlerWithArgsModelFilledRegion : ExternalEventMy<ICollection<(ElementId, List<CurveLoop>,string)>>
    {
        public string time { get; set; }
        public string report { get; set; }

        public List<string> elementIDS { get; set; }


        public FamilySymbol symbolF { get; set; }




        public override void Execute(UIApplication uiApp, ICollection<(ElementId, List<CurveLoop>,string)> patrones)
        {
            
            Autodesk.Revit.DB.Document doc = uiApp.ActiveUIDocument.Document;

            using (Transaction tx = new Transaction(doc))
            {

                tx.Start("Model FillRegions");
                string test = "";int count = 0;
                foreach ((ElementId, List<CurveLoop>,string) valor in patrones)
                {
                    FilledRegion actualFill = FilledRegion.Create(doc, valor.Item1, doc.ActiveView.Id, valor.Item2);
                    Parameter com = actualFill.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);
                    com.Set(valor.Item3);

                   


                }

                tx.Commit();
            }
        }

    }

    public class EventHandlerWithArgsCreateTextNote : ExternalEventMy<ICollection<(XYZ, XYZ, string,bool,Element)>>
    {
        public string time { get; set; }
        public string report { get; set; }

        public List<string> elementIDS { get; set; }


        public FamilySymbol symbolF { get; set; }




        public override void Execute(UIApplication uiApp, ICollection<(XYZ, XYZ, string,bool,Element)> Textos)
        {

            Autodesk.Revit.DB.Document doc = uiApp.ActiveUIDocument.Document;

            ICollection<(XYZ, XYZ, string,bool,Element)> listas = Textos;
            


            //results.Text = fam.Name;

            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Create texts");
                List<string> listaids = new List<string>() { };

                try
                {
                    foreach ((XYZ, XYZ, string,bool,Element) item in listas)
                    {

                        double HText = item.Item5.get_Parameter(BuiltInParameter.TEXT_SIZE).AsDouble();


                        string text = item.Item3;
                        XYZ pa = item.Item1;
                        XYZ PB = item.Item2;
                        XYZ pa2 = new XYZ(pa.X,pa.Y-(HText),pa.Z);
                        //TextNoteOptions opt = new TextNoteOptions();
                        bool check = item.Item4; 

                        //if (check==false) {note = Textos.Item3;}
                        //else { note = Textos.Item2; }

                        if (text == null)
                        {
                            TextNote actual = TextNote.Create(doc, doc.ActiveView.Id, PB, "Sin información", item.Item5.Id);
                            
                            if (text != "" && check == true)
                            {

                                Leader lead = actual.AddLeader(TextNoteLeaderTypes.TNLT_STRAIGHT_L);
                                XYZ pEnd = lead.End;

                                XYZ Npa = new XYZ(pa.X, pEnd.Y, pEnd.Z);
                                lead.End = Npa;
                            }
                        }
                        if (text != null)
                        {
                            TextNote actual = TextNote.Create(doc, doc.ActiveView.Id, PB, text, item.Item5.Id);
                            if (text != "" && check == true)
                            {

                                Leader lead = actual.AddLeader(TextNoteLeaderTypes.TNLT_STRAIGHT_L);
                                XYZ pEnd = lead.End;

                                XYZ Npa = new XYZ(pa.X, pEnd.Y, pEnd.Z);
                                lead.End = Npa;
                            }
                        }
                        


                    }


                }
                catch (Exception args)
                {
                    MessageBox.Show(args.ToString());
                }


                tx.Commit();
            }


        }

    }

    public class EventHandlerWithArgsCreateDimensions : ExternalEventMy<ICollection<ICollection<FilledRegion>>>
    {

        public override void Execute(UIApplication uiApp, ICollection<ICollection<FilledRegion>> referencias)
        {

            Autodesk.Revit.DB.Document doc = uiApp.ActiveUIDocument.Document;
            //ICollection<Element> dimType = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Dimensions).WhereElementIsElementType().ToElements();
            Autodesk.Revit.DB.View v = doc.ActiveView;

            
            using (Transaction tx = new Transaction(doc))
            {

                tx.Start("Create Dimensions");


                foreach (ICollection<FilledRegion> par in referencias)//trabajo sobre la lista de cara muro
                {
                    try
                    {
                        FilledRegion f01 = par.First<FilledRegion>();
                        FilledRegion f02 = par.Last<FilledRegion>();
                        Options opt = new Options();
                        var edges01 = FindRegionEdges(f01);
                        var edges02 = FindRegionEdges(f02);
                        ICollection<Edge> edgesSum = new List<Edge>();
                        foreach (var edge in edges01) { edgesSum.Add(edge); }
                        foreach (var edge in edges02) { edgesSum.Add(edge); }
                        IEnumerable<Edge> edgeEnum = edgesSum.AsEnumerable<Edge>();




                        // Crea las preferencias para analizar la geometría
                        Options opcion = new Options();
                        // Activa el cálculo de referencias a objetos
                        opcion.ComputeReferences = true;

                        CurveLoop CL01 = f01.GetBoundaries().ElementAt<CurveLoop>(0);
                        Curve l01 = CL01.ElementAt<Curve>(0);

                        XYZ p01 = l01.GetEndPoint(0);
                        XYZ p01B = new XYZ(p01.X, p01.Y - 1, p01.Z);

                        CurveLoop CL02 = f02.GetBoundaries().ElementAt<CurveLoop>(0);
                        Curve l02 = CL01.ElementAt<Curve>(2);

                        XYZ p02 = l02.GetEndPoint(0);
                        XYZ p02B = new XYZ(p02.X, p02.Y - 1, p02.Z);

                        Line l = Line.CreateBound(p01B, p02B);

                        XYZ dimensionDirection = new XYZ(1, 0, 0);
                        var edgesDirection = dimensionDirection.CrossProduct(v.ViewDirection);
                        //var shift = UnitUtils.ConvertToInternalUnits(5 * v.Scale, DisplayUnitType.DUT_MILLIMETERS)* edgesDirection; for version21
                        var shift = UnitUtils.ConvertToInternalUnits(10 * v.Scale, DisplayUnitType.DUT_MILLIMETERS)* edgesDirection;
                        var dimensionLine = Line.CreateUnbound(f01.get_BoundingBox(v).Min + shift, dimensionDirection);
                        var edges = edgeEnum.Where(x => IsEdgeDirectionSatisfied(x, edgesDirection)) .ToList();
                        ReferenceArray references = new ReferenceArray();
                        //foreach (var edge in edges)
                        //{
                        //    references.Append(edge.Reference);
                        //}

                        references.Append(edges.First<Edge>().Reference);
                        references.Append(edges.Last<Edge>().Reference);
                        //foreach (var edge2 in edges02)
                        //{
                        //    references.Append(edge2.Reference);
                        //}


                        try
                        {
                            Dimension dim = doc.Create.NewDimension(v, dimensionLine, references);
                            ElementId dr_id = DimensionTypeId(doc);
                            //string valor = dim.Name;
                            //if (dr_id != null)
                            //{
                            //    dim.ChangeTypeId(dr_id);
                            //}
                        }
                        catch (Exception m) 
                        {
                            //MessageBox.Show(m.Message,"Error al crear la cota");                            
                        }
                    }
                    catch(Exception er) { MessageBox.Show(er.Message); }    
                    
                }



                tx.Commit();
            }


        }
        private static IEnumerable<Edge>  FindRegionEdges(FilledRegion filledRegion)
        {
            var view = (View)filledRegion.Document.GetElement(
              filledRegion.OwnerViewId);

            var options = new Options
            {
                View = view,
                ComputeReferences = true
            };

            return filledRegion
              .get_Geometry(options)
              .OfType<Solid>()
              .SelectMany(x => x.Edges.Cast<Edge>());
        }
        private static ElementId DimensionTypeId(Autodesk.Revit.DB.Document doc)
        {
            FilteredElementCollector mt_coll
              = new FilteredElementCollector(doc)
                .OfClass(typeof(DimensionType))
                .WhereElementIsElementType();

            DimensionType dimType = null;

            foreach (Element type in mt_coll)
            {
                string famName = type.get_Parameter(BuiltInParameter.ALL_MODEL_FAMILY_NAME).AsString();
                string dimTypeName = type.GetType().Name.ToString();
                if (type is DimensionType)
                {
                    if (famName == "Linear Dimension Style")
                    {
                        dimType = type as DimensionType;
                        break;
                    }
                }
            }
            //DimensionType dimType = mt_coll.FirstElement() as DimensionType;
            
            return dimType.Id;
        }

        private static bool IsEdgeDirectionSatisfied(Edge edge,XYZ edgeDirection)
        {
            var edgeCurve = edge.AsCurve() as Line;

            if (edgeCurve == null)
                return false;

            return edgeCurve.Direction.CrossProduct(
              edgeDirection).IsAlmostEqualTo(XYZ.Zero);
        }
    }

}

