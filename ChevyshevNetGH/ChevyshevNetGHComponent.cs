using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ChevyshevNetGH
{
    public class ChevyshevNetGHComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public ChevyshevNetGHComponent()
          : base("CompassMethod", "CMethod",
                 "CompassMethod to obtain a same length quad grid on any given surface (has some exceptions)",
                 "Alan", "Gridshells")
        {
            /// <summary>
            /// This is the constructor of the component!
            /// Custom class variables should be initialized here to avoid early initialization
            /// when Grasshopper starts.
            /// </summary>

        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Surface", "Srf", "Surface on which to obtain the grid", GH_ParamAccess.item);
            pManager.AddPointParameter("Starting Point", "P", "Starting UV Coordinates for grid", GH_ParamAccess.item);
            pManager.AddNumberParameter("Grid Size", "L", "Specify grid size for Chebyshev net", GH_ParamAccess.item, 1.0);
            pManager.AddAngleParameter("Rotation Angle", "Angle", "Rotation angle in radians", GH_ParamAccess.item, 0.0);
            pManager.AddBooleanParameter("Extend Surface", "Extend", "Set to true to extend the surface", GH_ParamAccess.item, false);
            pManager.AddNumberParameter("Extension Length", "E. Length", "Optional: set a custom extension length", GH_ParamAccess.item, 2.0);
            pManager.AddIntegerParameter("Number of axis", "Axis no.", "Number of axis for the grid generation (3 to 6)",GH_ParamAccess.item, 4);
            pManager.AddAngleParameter("Skew Angle", "Skw.Angle", "OPTIONAL: List of Angles to use for uneven distribution", GH_ParamAccess.list, new List<double>());
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Chebyshev Point Grid", "PtGrid", "To be written....", GH_ParamAccess.tree);
            pManager.AddLineParameter("Warp Net direction", "Warp", "Resulting warp direction net of the Compass Method algorithm", GH_ParamAccess.tree);
            pManager.AddLineParameter("Weft Net direction", "Weft", "Resulting weft direction net of the Compass Method algorithm", GH_ParamAccess.tree);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // DECLARE INSTANCE VARIABLES

            Surface surf = null;
            Point3d stPt = Point3d.Unset;
            double gridLength = 1.0;
            double rotationAngle = 0.0;
            bool surfExtend = true;
            double surfExtendLength = 1.0;
            int numAxis = 0;
            List<double> axisAnglesList = new List<double>();

            if (!DA.GetData(0, ref surf)) { return; }
            if (!DA.GetData(1, ref stPt)) { return; }
            if (!DA.GetData(2, ref gridLength)) { return; }
            if (!DA.GetData(3, ref rotationAngle)) { return; }
            if (!DA.GetData(4, ref surfExtend)) { return; }
            if (!DA.GetData(5, ref surfExtendLength)) { return; }
            if (!DA.GetData(6, ref numAxis)) { return; }
            if (!DA.GetDataList(7,axisAnglesList)) { return; }


            // DATA VALIDATION

            if ((surf.IsClosed(0) && surf.IsClosed(1)))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Surfaces closed in both U and V direction are not supported");
                return;
            }

            if(numAxis <3 || numAxis >6)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Number of Axis must be between 3 and 6"); 
                return; 
            }


            // DO CHEBYSHEV HERE!!

            ChebyshevNet net = new ChebyshevNet(surf, stPt, gridLength, rotationAngle, surfExtend,surfExtendLength, numAxis, axisAnglesList);
            net.GenerateChebyshevNet();


            // OUTPUT DATA (MUST BE GH_TREE)

            if (net.Grid != null) DA.SetDataTree(0, net.Grid);
            else AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "OOPS! Something happened! No point grid was found"); 

            if (net.WarpNet != null) DA.SetDataTree(1, net.WarpNet);
            else AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "OOPS! Something happened! No WARP net was found"); 

            if (net.WeftNet != null) DA.SetDataTree(2, net.WeftNet);
            else AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "OOPS! Something happened! No WEFT net was found"); 
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
               
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("a6b18f52-cff2-49e2-850f-e3b3f91bf0d6"); }
        }
    }
}
